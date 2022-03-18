using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Spectre.Console;

namespace BADownloader.Sites
{
    public class BetterAnime : Anime
    {
        private string Quality { get; }
        public BetterAnime( HtmlDocument Document , string Url ) : base()
        {
            string name = Document.DocumentNode.SelectSingleNode("//*[@id='page-content']/main/div[1]/div/h2").InnerText ?? 
                throw new Exception("Não foi encontrado o nome do anime, página não carregou ou o site caiu!");
            
            string chars = Regex.Escape(@"<>:" + "\"" + "/|?*");
            string pattern = "[" + chars + "]";
            Name = Regex.Replace( name, pattern, "" );
            
            URL = Url;

            var links = GetEpisodesURL( Document );

            AnsiConsole.Write(new Markup(string.Format("Anime: [green bold]{0}[/]\nNúmero de episódios: [green bold]{1}[/]\n", Name, links.Last().Key)));

            int[] episodes = new int[links.Count];
            for ( int i = 0; i < links.Count; i++ )
            {
                episodes[i] = links.ElementAt(i).Key;
            }

            if ( AnimesData.CheckUserFolder( Name ) )
            {
                episodes = AnimesData.ExistingEpisodes( Name );
                episodes = AnimesData.OtherEpisodes( episodes, links.ElementAt(0).Key, links.Count );

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
                    temporary.Add( episodes[i], links.Single( ctx => ctx.Key == episodes[i] ).Value );
                }
                links = temporary;
            }
            else
            {
                int index = 0;
                foreach ( var key in links.Keys )
                {
                    episodes[index] = key;
                    index++;
                }
            }

            Episodes = episodes;
            LinkDownloads = links;
            StartCount = AnimesData.EpisodeInput( AnimeLength, Episodes );
            Quality = BetterAnime.QualityInput();
        }

        public override async Task<string> GetSourceLink( string episodeURL )
        {
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync( episodeURL );

            episodeURL = doc.DocumentNode.SelectSingleNode( this.Quality ).GetAttributeValue( "href", "" );

            doc = await web.LoadFromWebAsync( episodeURL );

            var nextdatastr = doc.DocumentNode.SelectSingleNode( "//*[@id='__NEXT_DATA__']" ).InnerText;

            var NextData = JsonConvert.DeserializeObject<NextData>( nextdatastr );

            var countlink = "https://download.betteranime.net/".Length;
            var datalink = episodeURL[countlink..];
            var path = "/_next/data/" + NextData.BuildId + "/" + datalink + ".json";

            var url = "https://download.betteranime.net" + path;

            Dictionary<string, string> headers = new() 
            {
                { "path", path },
                { "referer", episodeURL },
                { "user-agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.4844.51 Safari/537.36" }
            };

            var response = await BADHttp.GetResponseMessageAsync( url, headers );

            var dl = JsonConvert.DeserializeObject<Props>( await response.Content.ReadAsStringAsync() ).PageProps.Anime.URLDownload;

            return dl;
        }

        public override void WriteDebug()
        {
            base.WriteDebug();
            Console.WriteLine( "Quality: " + this.Quality );
        }

        // private static string GetGenres(HtmlDocument doc)
        // {
        //     var genresnodes = doc.DocumentNode.SelectSingleNode("//*[@id='page-content']/main/div[1]/div/div[@class='anime-genres']").Descendants();

        //     List<string> genres = new();
        //     // O primeiro node é bugadasso
        //     int i = 0;
        //     foreach (var node in genresnodes)
        //     {
        //         if (i > 0 && !genres.Contains(node.InnerText.Trim()) && node.InnerText != " " && node.InnerText != "\n")
        //         {
        //             string gen = Regex.Replace(node.InnerText, @"[^0-9a-zA-Z\p{L}]+", "");
        //             if (string.IsNullOrEmpty(gen)) continue;
        //             if (!genres.Contains(gen)) genres.Add(gen);
        //         }
        //         i++;
        //     }

        //     string strgenres = string.Empty;
        //     foreach (var gen in genres)
        //     {
        //         if (strgenres == string.Empty)
        //             strgenres = $"[green bold]{gen}[/]";
        //         else
        //             strgenres += $", [green bold]{gen}[/]";
        //     }

        //     return strgenres;
        // }

        private static Dictionary<int, string> GetEpisodesURL(HtmlDocument doc)
        {
            var episodeshtml = doc.DocumentNode.SelectSingleNode("//*[@id='episodesList']");

            List<string> urls = new();
            foreach (var node in episodeshtml.Descendants())
            {
                if (node.GetAttributeValue("class", "") != "list-group-item" 
                && node.GetAttributeValue("href", "") != default 
                && node.GetAttributeValue("href", "").Contains("/download"))
                {
                    urls.Add(node.GetAttributeValue("href", ""));
                }
            }

            Dictionary<int, string> eps = new();
            foreach (var url in urls)
            {
                int num = AnimesData.GetEpisodeParsed(url);

                eps.Add(num, url);
            }

            return eps;
        }

        private static string QualityInput()
        {
            var str = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title("\nSelecione a qualidade de vídeo preferida.\nOBS: Nem todas as qualidades estarão disponíveis dependendo do anime.\nCada episódio pode variar de [green]~100mb[/] à [yellow]~1gb[/] dependendo da qualidade\n[yellow underline]Verifique se seu disco contém espaço suficiente![/]")
                .PageSize(5)
                .AddChoices(new []
                {
                    "SD", "HD", "FULL HD"
                }));

            switch (str)
            {
                case "SD":
                return "//*[@id='page-content']/div[2]/section/div[2]/div[1]/div/a[1]";

                case "HD":
                return "//*[@id='page-content']/div[2]/section/div[2]/div[1]/div/a[2]";

                case "FULL HD":
                return "//*[@id='page-content']/div[2]/section/div[2]/div[1]/div/a[3]";

                default:
                    Console.WriteLine("??? selecionei o sd pra vc");
                return "//*[@id='page-content']/div[2]/section/div[2]/div[1]/div/a[1]";
            }
        }
    }
}