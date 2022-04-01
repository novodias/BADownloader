using BADownloader.Extractor;

namespace BADownloader
{
    public class DownloadManager
    {
        private readonly IExtractor Anime;
        private readonly int TaskDownload;
        private readonly List<string> URLsDownload;
        private readonly List<string> URLsFailed;
        public DownloadManager(IExtractor anime, int amount)
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
                    i++;
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
}