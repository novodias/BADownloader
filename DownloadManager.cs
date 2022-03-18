using Newtonsoft.Json;

namespace BADownloader
{
    public class DownloadManager
    {
        private readonly IAnime Anime;
        private readonly int TaskDownload;
        private readonly List<string> URLsDownload;
        private readonly List<string> URLsFailed;
        public DownloadManager(IAnime anime, int amount)
        {
            this.Anime = anime;
            this.TaskDownload = amount;
            this.URLsDownload = new( this.Anime.AnimeLength );
            this.URLsFailed = new();
        }

        private async Task DownloadAsync(string url, int i)
        {
            int animeindex = this.Anime.LinkDownloads.Keys.Single( key => key == this.Anime.Episodes[i] );

            /*
            string epURL = string.Empty;
            if ( animeindex < 10 )
                epURL = "0" + animeindex;

            string failedURL = this.Anime.URL + "/episodio-" + epURL + "/download";
            */

            try
            {
                Console.WriteLine($"Baixando episódio: [{animeindex}/{this.Anime.LastEpisode}]");
                var file = File.OpenWrite($"Animes/{this.Anime.Name}/{this.Anime.Name}-{animeindex}.mp4");

                await BADHttp.DownloadFileAsync( url, file );

                Console.WriteLine($"Download do episódio [{animeindex}/{this.Anime.LastEpisode}] concluído.");
                
                // if ( this.URLsFailed.Contains( failedURL ) )
                    // this.URLsFailed.Remove( failedURL );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Download do episódio [{animeindex}] falhou! {ex.Message}");

                // if ( !this.URLsFailed.Contains( failedURL ) )
                //     this.URLsFailed.Add( failedURL );
            }
        }
        public async Task StartDownloadAsync()
        {
            Console.WriteLine("\nComeçando download.");

            await this.ManageDownloader();

            Console.WriteLine($"Downloads concluídos!");

            await this.RetryDownload();
        }

        private async Task RetryDownload()
        {
            if ( !( this.URLsFailed.Count > 0 ) )
                return;

            Console.Write("Deseja baixar os episódios que falharam? [Y/n]: ");
            ConsoleKey key = Console.ReadKey().Key;
            bool bkey = key == ConsoleKey.Y | key == ConsoleKey.Enter;

            if ( bkey )
            {
                Console.WriteLine("\nBaixando episódios que falharam..." + "\nEm caso de error 'Gone', feche e abra novamente o BADownloader");
                await this.RetryManageDownloader( this.URLsFailed );
            }
        }

        private async Task ManageDownloader()
        {
            List<Task> tasks = new();

            for ( int i = this.Anime.Index; i < this.Anime.Episodes.Length; )
            {
                for ( int j = 0; j < this.TaskDownload; j++ )
                {
                    if ( i > this.Anime.LinkDownloads.Count )
                        break;

                    var downloadlink = this.Anime.LinkDownloads.ElementAt(i).Value;
                    tasks.Add( DownloadAsync( await this.Anime.GetSourceLink( downloadlink ), i ) );
                    i++;
                }

                await Task.WhenAll( tasks );
                tasks.Clear();
            }
        }

        private async Task RetryManageDownloader( List<string> list )
        {
            List<Task> tasks = new();

            for ( int i = 0; i < list.Count; )
            {
                for ( int j = 0; j < this.TaskDownload; j++ )
                {
                    if ( i > this.Anime.LinkDownloads.Count )
                        break;
                    
                    tasks.Add( DownloadAsync( await this.Anime.GetSourceLink( list.ElementAt(i) ), i ) );
                }

                await Task.WhenAll( tasks );
                tasks.Clear();
            }
        }

        public void Dispose()
        {
            this.URLsDownload.Clear();
            this.URLsFailed.Clear();
        }
    }

    public struct NextData
    {
        public NextData(Props props, string page, Query query, string buildid, bool nextexport, bool isfallback, bool gsp)
        {
            this.Props = props;
            this.Page = page;
            this.Query = query;
            this.BuildId = buildid;
            this.NextExport = nextexport;
            this.IsFallback = isfallback;
            this.GSP = gsp;
        }

        [JsonProperty("props")]
        public Props Props { get; private set; }

        [JsonProperty("page")]
        public string Page { get; private set; }

        [JsonProperty("query")]
        public Query Query { get; private set; }

        [JsonProperty("buildId")]
        public string BuildId { get; private set; }

        [JsonProperty("runtimeConfig")]
        public RuntimeConfig? RuntimeConfig { get; private set; } = null;

        [JsonProperty("nextExport")]
        public bool NextExport { get; private set; }

        [JsonProperty("isFallback")]
        public bool IsFallback { get; private set; }

        [JsonProperty("gsp")]
        public bool GSP { get; private set; }
    }

    public struct Props
    {
        [JsonProperty("pageProps")]
        public PageProps PageProps { get; private set; }

        [JsonProperty("__N_SSG")]
        public bool N_SSG { get; private set; }
    }

    public struct PageProps
    {
        [JsonProperty("anime")]
        public AnimeJson Anime { get; private set; }
    }

    public struct AnimeJson
    {
        [JsonProperty("nome")]
        public string Nome { get; private set; }

        [JsonProperty("path")]
        public string URLDownload { get; private set; }
    }

    public struct Query
    {
        [JsonProperty("download")]
        public string Download { get; set; }
    }

    public struct RuntimeConfig {}

}