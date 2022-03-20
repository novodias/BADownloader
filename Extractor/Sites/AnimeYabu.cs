using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Spectre.Console;

namespace BADownloader.Extractor.Sites
{
    public class AnimeYabu : Extractor
    {
        private string Quality { get; }
        public AnimeYabu( HtmlDocument Document, string Url ) : base()
        {
            string name = Document.DocumentNode.SelectSingleNode("//*[@id='channel-content']/div[1]/div[2]/div[2]/h1").InnerText;
            string chars = Regex.Escape(@"<>:" + "\"" + "/|?*");
            string pattern = "[" + chars + "]";
            Name = Regex.Replace(name, pattern, "");

            URL = Url;

            Dictionary<int, string> Links = GetEpisodesURL(Document);

            int[] episodes = new int[Links.Count];
            for (int i = 0; i < Links.Count; i++)
            {
                episodes[i] = Links.ElementAt(i).Key;
            }

            AnsiConsole.Write(new Markup(string.Format("Anime: [green bold]{0}[/]\nNúmero de episódios: [green bold]{1}[/]\n", Name, Links.Last().Key)));

            if ( AnimesData.CheckUserFolder( Name ) )
            {
                episodes = AnimesData.ExistingEpisodes( Name );
                episodes = AnimesData.OtherEpisodes( episodes, Links.ElementAt(0).Key, Links.Count );

                string StrEpisodes = string.Empty;
                foreach ( var i in episodes )
                {
                    if ( StrEpisodes.Equals( string.Empty ) )
                        StrEpisodes = $"Episódio(s) faltando: {i}";
                    else
                        StrEpisodes += $", {i}";
                }
                Console.WriteLine( StrEpisodes );

                Dictionary<int, string> temporary = new();
                for ( int i = 0; i < episodes.Length; i++ )
                {
                    temporary.Add( episodes[i], Links.Single( ctx => ctx.Key == episodes[i]).Value );
                }
                Links = temporary;
            }
            else
            {
                int index = 0;
                foreach ( var key in Links.Keys )
                {
                    episodes[index] = key;
                    index++;
                }
            }

            Episodes = episodes;
            LinkDownloads = Links;
            StartCount = AnimesData.EpisodeInput( AnimeLength, Episodes );
            Quality = AnimeYabu.QualityInput();
        }

        public override async Task<string> GetSourceLink( string episodeURL )
        {
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
            
            var response = await BADHttp.GetResponseMessageAsync( episodeURL, headers );

            var src = await response.Content.ReadAsStringAsync();

            var HDindex = src.IndexOf( "type: \"video/mp4\",label: \"HD\",file: \"" ) + 37;
            var SDindex = src.IndexOf( "type: \"video/mp4\",label: \"SD\",file: \"" ) + 37;

            string mp4link;
            if ( Quality == "HD" )
            {
                string HD = src[HDindex..];
                var endlink = HD.IndexOf( "\"" );
                mp4link = HD[0..endlink];
            }
            else 
            {
                string SD = src[SDindex..];
                var endlink = SD.IndexOf( "\"" );
                mp4link = SD[0..endlink];
            }

            return mp4link;
        }

        private static Dictionary<int, string> GetEpisodesURL(HtmlDocument doc)
        {
            bool IsPaged = doc.DocumentNode.SelectSingleNode("//*[@id='channel-content']/div[3]/div[8]").HasChildNodes;

            if ( !IsPaged )
            {
                var ChildsNodes = doc.DocumentNode.SelectSingleNode("//*[@id='channel-content']/div[3]").Descendants();

                var VideosNodes = ChildsNodes.Where(node => node.HasClass("video"));

                Dictionary<int, string> EpisodesDictionary = new();
                foreach (var node in VideosNodes)
                {
                    var ClipLink = node.Descendants().Single(node => node.HasClass("video-thumb")).FirstChild;

                    string Link = ClipLink.GetAttributeValue("href", "");
                    string strNumber = ClipLink.GetAttributeValue("title", "");

                    int Number = int.Parse(
                            Regex.Replace(strNumber[strNumber.IndexOf("Epis")..strNumber.Length], "[^0-9.]", "")
                    );

                    Console.WriteLine(Link);

                    EpisodesDictionary.Add(Number, Link);
                }

                return EpisodesDictionary;
            }
            else
            {
                /*
                Pra ser implementado.

                Como funcionará?
                Usando a seguinte url: https://animeyabu.com/anime/one-piece/page/X
                One Piece possui atualmente 34 páginas, inserida 
                na página principal no 'div class=naco'.

                A ideia é pegar o número de páginas no 'naco'
                e usar um for loop e pegar os links dos episódios.
                */

                throw new Exception("Feature ainda não implementada com animes com mais de uma página");
            }
        }

        private static string QualityInput()
        {
            var str = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title("\nSelecione a qualidade de vídeo preferida.\nOBS: Nem todas as qualidades estarão disponíveis dependendo do anime.\nCada episódio pode variar de [green]~100mb[/] à [yellow]~1gb[/] dependendo da qualidade\n[yellow underline]Verifique se seu disco contém espaço suficiente![/]")
                .PageSize(5)
                .AddChoices(new []
                {
                    "SD", "HD"
                }));

            return str;
        }

        // private static string GetGenres(HtmlDocument doc)
        // {
        //     var GenresNode = doc.DocumentNode.SelectSingleNode("//*[@id='channel-content']/div[1]/div[2]/div[4]/b");

        //     return GenresNode.InnerText;
        // }
    }
}