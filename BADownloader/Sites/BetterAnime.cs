using HtmlAgilityPack;
using Newtonsoft.Json;

namespace BADownloader.Sites
{
    public class BetterAnime : Extractor
    {
        readonly Dictionary<Quality, string> OptionsQuality = new(3)
        { 
            { Quality.SD, "//*[@id='page-content']/div[2]/section/div[2]/div[1]/div/a[1]" }, // SD
            { Quality.HD, "//*[@id='page-content']/div[2]/section/div[2]/div[1]/div/a[2]" }, // HD
            { Quality.FHD, "//*[@id='page-content']/div[2]/section/div[2]/div[1]/div/a[3]" }  // FHD
        };

        public BetterAnime(HtmlDocument document, string url) : base()
        {
            this.Name = document.DocumentNode.SelectSingleNode("//*[@id='page-content']/main/div[1]/div/h2").InnerText;
            this.BaseURL = url;

            var animeCollection = GetEpisodesURL( document, Name );
            this.Total = animeCollection.Count;

            this.ACollection = Extractor.SearchInAnimeFolder( Name, animeCollection );
        }

        private string GetOptionQuality(HtmlNode node)
        {
            string GetLink(string xpath) {
                return node.SelectSingleNode(xpath).GetAttributeValue("href", "");
            }

            if (this.Quality is Quality.NULL)
            {
                for (int i = OptionsQuality.Count - 1; i >= 0 ; i--)
                {
                    string value = GetLink(this.OptionsQuality.ElementAt(i).Value);

                    if ( value is not default(string) )
                        return value;
                }
            }

            return GetLink(this.OptionsQuality[this.Quality]);
        }

        public static Task<Extractor> InitializeExtractorAsync(HtmlDocument document, string url) 
        {
            var tcs = new TaskCompletionSource<Extractor>();
            _ = Task.Run(() =>
            {
                try
                {
                    var result = new BetterAnime(document, url);
                    tcs.SetResult(result);
                }
                catch (Exception e) {
                    tcs.SetException(e); 
                }
            });

            return tcs.Task;
        }
        public static BetterAnime InitializeExtractor(HtmlDocument document, string url) => new(document, url);

        public override async Task<string> GetSourceLink( string episodeURL )
        {
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync( episodeURL );

            // episodeURL = doc.DocumentNode.SelectSingleNode( node ).GetAttributeValue( "href", "" );
            episodeURL = this.GetOptionQuality(doc.DocumentNode);

            doc = await web.LoadFromWebAsync( episodeURL );

            var nextdatastr = doc.DocumentNode.SelectSingleNode( "//*[@id='__NEXT_DATA__']" ).InnerText;

            var NextData = JsonConvert.DeserializeObject<NextData>(nextdatastr);

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

            var response = await BADHttp.SendAsync( url, headers );

            var dl = JsonConvert.DeserializeObject<Props>( await response.Content.ReadAsStringAsync() ).PageProps.Anime.URLDownload;

            return dl;
        }

        public override void WriteDebug()
        {
            base.WriteDebug();
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

        private static ACollection GetEpisodesURL(HtmlDocument doc, string name)
        {
            var episodeshtml = doc.DocumentNode.SelectSingleNode("//*[@id='episodesList']");

            List<string> collectionURL = new();

            // Pega o url de cada episódio
            foreach (var node in episodeshtml.Descendants())
            {
                if (node.GetAttributeValue("class", "") != "list-group-item" 
                && node.GetAttributeValue("href", "") != default 
                && node.GetAttributeValue("href", "").Contains("/download"))
                {
                    collectionURL.Add(node.GetAttributeValue("href", ""));
                }
            }

            ACollection ACollection = new();
            
            foreach (var url in collectionURL)
            {
                int number = AnimesData.GetEpisodeParsed(url);
                var info = new AnimeInfo()
                {
                    Name = $"{name}-{number}",
                    Number = number,
                    URLDownload = url
                };

                ACollection.TryAdd(info);
            }

            return ACollection;
        }

        // private static string QualityInput()
        // {
        //     string str;

        //     Console.WriteLine("\nSelecione a qualidade de vídeo preferida.\nOBS: Nem todas as qualidades estarão disponíveis dependendo do anime.\nCada episódio pode variar de ~100mb à ~1gb dependendo da qualidade\nVerifique se seu disco contém espaço suficiente! \n[1] SD\n[2] HD\n[3] FULL HD");
        //     str = Console.ReadLine() ?? string.Empty;

        //     switch (str)
        //     {
        //         case "SD":
        //         case "1":
        //             return "//*[@id='page-content']/div[2]/section/div[2]/div[1]/div/a[1]";

        //         case "HD":
        //         case "2":
        //             return "//*[@id='page-content']/div[2]/section/div[2]/div[1]/div/a[2]";

        //         case "FULL HD":
        //         case "3":
        //             return "//*[@id='page-content']/div[2]/section/div[2]/div[1]/div/a[3]";

        //         default:
        //             Console.WriteLine("Opção inválida");
        //             return QualityInput();
        //     }
        // }
    }
}