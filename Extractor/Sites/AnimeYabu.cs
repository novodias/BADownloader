using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Spectre.Console;

namespace BADownloader.Extractor.Sites
{
    public class AnimeYabu : Extractor
    {
        private string Quality { get; }

        private AnimeYabu( string name, Dictionary<int, string> links, int[] episodes, string url, int startcount, string quality ) : base( name, links, episodes, url, startcount )
        {
            this.Quality = quality;
        }

        public static async Task<AnimeYabu> InitializeExtractorAsync( HtmlDocument Document, string Url )
        {
            string name = Document.DocumentNode.SelectSingleNode("//div[contains(@class, 'anime-title')]").InnerText;
            string chars = Regex.Escape(@"<>:" + "\"" + "/|?*");
            string pattern = "[" + chars + "]";
            string Name = Regex.Replace(name, pattern, "");

            string URL = Url;

            Dictionary<int, string> Links = await GetEpisodesURL( Document, URL );
            int AnimeLength = Links.Count;

            int[] episodes = new int[Links.Count];
            for (int i = 0; i < Links.Count; i++)
            {
                episodes[i] = Links.ElementAt(i).Key;
            }
            
            if ( !Program.IsWindows7 )
                AnsiConsole.Write(new Markup(string.Format("Anime: [green bold]{0}[/]\nNúmero de episódios: [green bold]{1}[/]\n", Name, Links.Last().Key)));
            else
                Console.WriteLine( string.Format("Anime: {0}\nNúmero de episódios: {1}\n", Name, Links.Last().Key) );

            Extractor.CheckAnimeFolder( Name, ref episodes, ref Links );

            var Episodes = episodes;
            var LinkDownloads = Links;
            var StartCount = AnimesData.EpisodeInput( AnimeLength, Episodes );
            var Quality = AnimeYabu.QualityInput();

            return new AnimeYabu( Name, LinkDownloads, Episodes, URL, StartCount, Quality );
        }

        public override async Task<string> GetSourceLink( string episodeURL )
        {
            // Alguns animes possuem um URL direto no source da classe video
            // Exemplo: https://pitou.goyabu.com/beelzebub/60.mp4
            
            Dictionary<string, string> headers = new() 
            {
                { "age", "25" },
                { "alt-svc", "h3=\":443\"; ma=86400, h3-29=\":443\"; ma=86400" },
                { "cache-control", "max-age=1800" },
                { "cf-apo-via", "tcache" },
                { "cf-cache-status", "HIT" },
                { "cf-ray", "6ed971d1ed626f86-JDO" },
                { "expect-ct", "max-age=604800, report-uri=\"https://report-uri.cloudfare.com/cdn-cgi/beacon/expect-ct" },
                { "nel", "{\"sucess_fraction\":0,\"report_to\":\"cf-nel\",\"max_age\":604800}" },
                { "report-to", "{\"endpoints\":[{\"url\":\"https://a.nel.cloudflare.com/report/v3?s=Zxvb1kVGbogPecjtb0Ktzt/ticEQaVIBeTNrxw2k+Zt+D2YiLd+LD5zJ+5jXyXVo7LcABld+FArmOYnl3VMT7hxs+3j4xeeu8qAxgrA/aAPMEV4yI5nyoMkmXXa2gUc=\"}],\"group\":\"cf-nel\",\"max_age\":604800}" },
                { "server", "cloudfare" },
                { "vary", "Accept-Encoding" },
                { "x-cache", "MISS" },
                { "x-page-speed", "1.13.35.2-0" },
            };
            
            var response = await BADHttp.SendAsync( episodeURL, headers );

            var src = await response.Content.ReadAsStringAsync();

            bool checkHD = src.Contains("type: \"video/mp4\",label: \"HD\"");
            bool checkSD = src.Contains("type: \"video/mp4\",label: \"SD\"");

            string mp4link;
            if ( Quality == "HD" && checkHD )
            {
                var HDindex = src.IndexOf( "type: \"video/mp4\",label: \"HD\",file: \"" ) + 37;
                string HD = src[HDindex..];
                var endlink = HD.IndexOf( "\"" );
                mp4link = HD[0..endlink];
            }
            else if ( Quality == "SD" && checkSD )
            {
                var SDindex = src.IndexOf( "type: \"video/mp4\",label: \"SD\",file: \"" ) + 37;
                string SD = src[SDindex..];
                var endlink = SD.IndexOf( "\"" );
                mp4link = SD[0..endlink];
            }
            else
            {
                mp4link = src[(src.IndexOf("file: \"https://pitou.goyabu.com") + 7)..(src.IndexOf(".mp4\"") + 4)];
            }

            return mp4link;
        }

        private async static Task<Dictionary<int, string>> GetEpisodesURL( HtmlDocument doc, string URL )
        {
            bool IsPaged = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'naco')]").HasChildNodes;

            if ( !IsPaged )
            {
                return AnimeYabu.GetAnimeYabuEpisodes( doc );
            }
            else
            {
                var ChildsNodes = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'naco')]");
                var PagesNumNodes = ChildsNodes.Descendants().Where( childs => childs.HasClass("page-numbers") );

                int tempPages = 0;
                foreach ( var node in PagesNumNodes )
                {
                    if ( int.TryParse( node.InnerText, out int num ) && num > tempPages )
                        tempPages = num;
                }

                HtmlWeb web = new();
                Dictionary<int, string> linkdownloads = new();
                string tempLink;
                for ( int page = 0; page < tempPages; page++ )
                {
                    if ( page == 0 )
                        tempLink = URL;
                    else
                        tempLink = URL + "/page/" + (page + 1);

                    await Task.Delay(TimeSpan.FromSeconds(4));

                    HtmlDocument docTemp = await web.LoadFromWebAsync( tempLink );
                    var tempDictionary = AnimeYabu.GetAnimeYabuEpisodes( docTemp );
                    foreach ( var item in tempDictionary )
                    {
                        if ( !linkdownloads.TryAdd( item.Key, item.Value ) )
                            System.Console.WriteLine("Não foi possível adicionar o seguinte episódio: " + item.Key + " | " + item.Value + " ( OVA? )");
                    }
                }

                return linkdownloads;
            }
        }

        private static Dictionary<int, string> GetAnimeYabuEpisodes( HtmlDocument doc )
        {
            var ChildsNodes = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'loop-content phpvibe-video-list miau')]").Descendants();

            var VideosNodes = ChildsNodes.Where(node => node.HasClass("video"));

            Dictionary<int, string> EpisodesDictionary = new();
            foreach (var node in VideosNodes)
            {
                var ClipLink = node.Descendants().Single(node => node.HasClass("video-thumb")).FirstChild;

                string Link = ClipLink.GetAttributeValue( "href", "" );
                string strNumber = ClipLink.GetAttributeValue( "title", "" );

                int Number;
                if ( strNumber.Contains( "Epis", StringComparison.OrdinalIgnoreCase ) )
                    Number = int.Parse(
                            Regex.Replace( strNumber[strNumber.IndexOf("Epis")..strNumber.Length], "[^0-9.]", "" )
                    );
                else if ( strNumber.Contains( "OVA", StringComparison.OrdinalIgnoreCase ) )
                    Number = int.Parse(
                            Regex.Replace( strNumber[strNumber.IndexOf("OVA")..strNumber.Length], "[^0-9.]", "" )
                    );
                else
                    Number = 0;

                if ( Program.IsDebugMode )
                    Console.WriteLine( Link );

                EpisodesDictionary.Add( Number, Link );
            }

            return EpisodesDictionary;
        }

        private static string QualityInput()
        {
            string str;
            if ( !Program.IsWindows7 )
                return AnsiConsole.Prompt(new SelectionPrompt<string>()
                    .Title("\nSelecione a qualidade de vídeo preferida.\nOBS: Nem todas as qualidades estarão disponíveis dependendo do anime.\nCada episódio pode variar de [green]~100mb[/] à [yellow]~1gb[/] dependendo da qualidade\n[yellow underline]Verifique se seu disco contém espaço suficiente![/]")
                    .PageSize(5)
                    .AddChoices(new []
                    {
                        "SD", "HD"
                    })
                );
            else
            {
                Console.WriteLine("\nSelecione a qualidade de vídeo preferida.\nOBS: Nem todas as qualidades estarão disponíveis dependendo do anime.\nCada episódio pode variar de ~100mb à ~1gbdependendo da qualidade\nVerifique se seu disco contém espaço suficiente!\n[1] SD\n[2] HD");
                str = Console.ReadLine() ?? string.Empty;
            }

            switch (str)
            {
                case "1":
                    return "SD";

                case "2":
                    return "HD";

                default:
                    System.Console.WriteLine("Opção inválida");
                    return QualityInput();
            }
        }

        // private static string GetGenres(HtmlDocument doc)
        // {
        //     var GenresNode = doc.DocumentNode.SelectSingleNode("//*[@id='channel-content']/div[1]/div[2]/div[4]/b");

        //     return GenresNode.InnerText;
        // }
    }
}