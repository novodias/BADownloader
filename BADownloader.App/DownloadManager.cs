using MinimalLog;

namespace BADownloader.App
{
    public class DownloadManager
    {
        private readonly IExtractor _anime;
        private readonly int _taskDownload;
        private readonly List<string> _urlsCollectionFailed;
        public DownloadManager(IExtractor anime, int amount)
        {
            this._anime = anime;
            this._taskDownload = amount;
            this._urlsCollectionFailed = new();
        }

        private async Task DownloadAsync(string url, int i, (int x, int y) position)
        {
            var info = this._anime.AnimeCollection.GetAnimeInfo(i);
            int animeNumber = info.Number;

            /*
            string epURL = string.Empty;
            if ( animeindex < 10 )
                epURL = "0" + animeindex;

            // string failedURL = info.URLDownload;
            */

            var file = File.OpenWrite(
                Path.Combine(AnimesData.Directory, $"{this._anime.Name}/{info.Name}")
            );

            try
            {
                var lastNumber = this._anime.AnimeCollection.Last().Number;
                string message = $"Baixando: [{animeNumber}/{lastNumber}]";

                // var file = File.OpenWrite($"Animes/{this.Anime.Name}/{info.Name}.mp4");

                using var response = await BADHttp.DownloadFileAsync(url);
                using var source = await response.Content.ReadAsStreamAsync();
                var length = response.Content.Headers.ContentLength;

                if ( length.HasValue )
                {
                    var size = Math.Round((double)(length / (1024 * 1024)));
                    message += $" [{size} Mb]";
                    await source.CopyToAsyncWithProgress(file, (long)length, message, position);
                }
                else
                {
                    BADConsole.WriteLine("Progresso do download não está disponível.");
                    await source.CopyToAsync(file);
                }

                // if ( this.URLsFailed.Contains( failedURL ) )
                //     this.URLsFailed.Remove( failedURL );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Download do episódio [{animeNumber}] falhou! {ex.Message}");
                MinimalLogger.Log<DownloadManager>(Logging.Error, ex.ToString());
                File.Delete(file.Name);

                // if ( !this.URLsFailed.Contains( failedURL ) )
                //     this.URLsFailed.Add( failedURL );
            }
        }

        /// <summary>
        /// Inicia o ManageDownloader.
        /// </summary>
        /// <returns>Task</returns>
        public async Task StartDownloadAsync()
        {
            await this.ManageDownloader();

            Console.WriteLine($"\nDownloads concluídos!");

            await this.RetryDownload();
        }

        /// <summary>
        /// Verifica se há URLsFailed, se sim, pergunta ao usuário se deseja baixar os episódios restantes e então os baixa.
        /// </summary>
        /// <returns>Task</returns>
        private async Task RetryDownload()
        {
            if ( !( this._urlsCollectionFailed.Count > 0 ) )
                return;

            BADConsole.Write("Deseja baixar os episódios que falharam? ([green]Y;/[red]n;) ");
            ConsoleKey key = Console.ReadKey().Key;
            bool bkey = key == ConsoleKey.Y | key == ConsoleKey.Enter;

            await Task.Delay(TimeSpan.FromSeconds(5));

            if ( bkey )
            {
                Console.WriteLine("\nBaixando episódios que falharam..." + "\nEm caso de error 'Gone', feche e abra novamente o BADownloader");
                // await this.RetryManageDownloader( this._urlsCollectionFailed );
            }
        }

        /// <summary>
        /// Baixa os episódios do anime
        /// </summary>
        /// <returns>Task</returns>
        private async Task ManageDownloader()
        {
            Console.CursorVisible = false;

            List<Task> tasks = new();
            var animeCollection = this._anime.AnimeCollection;

            Console.Clear();
            int current_cursor_position_top = 0;
            Console.SetCursorPosition(0, 0);

            for ( int i = this._anime.Index; i < animeCollection.Count; )
            {
                for ( int j = 0; j < this._taskDownload; j++ )
                {
                    if ( i > animeCollection.Count )
                        break;

                    await Task.Delay(TimeSpan.FromSeconds(3));
                    
                    // Pega o url de download do episódio do anime,
                    // e então pega o link direto implementado na classe
                    // do site.
                    var episodeurl = animeCollection.GetAnimeInfo(i).URLDownload;
                    var downloadlink = await this._anime.GetSourceLink(episodeurl);

                    // Progresso do download.
                    var position = (0, current_cursor_position_top++);

                    tasks.Add(DownloadAsync(downloadlink, i, position));

                    i++;
                }

                // Esperar os downloads terminarem.
                await Task.WhenAll( tasks );

                tasks.ForEach(task => task.Dispose());
                tasks.Clear();

                // Se o cursor chegar no fim vai colocar NEWLINE e resetar
                // o current_cursor_position_top para iniciar novamente no topo.
                if ( Console.CursorTop >= Console.WindowHeight )
                {
                    for (int newline = 0; i < Console.WindowHeight; newline++) {
                        Console.Write("\n");
                    }

                    current_cursor_position_top = 0;
                    Console.SetCursorPosition(0, 0);
                }
            }

            Console.CursorVisible = true;
        }

        /// <summary>
        /// Baixa os episódios do anime pela lista de URLs que falharam.
        /// </summary>
        /// <param name="Links">Lista com os links</param>
        /// <returns>Task</returns>
        // private async Task RetryManageDownloader( List<string> Links )
        // {
        //     List<Task> tasks = new();

        //     for ( int i = 0; i < Links.Count; )
        //     {
        //         for ( int j = 0; j < this._taskDownload; j++ )
        //         {
        //             if ( i > this._anime.AnimeCollection.Count )
        //                 break;

        //             await Task.Delay(TimeSpan.FromSeconds(3));
                    
        //             tasks.Add( DownloadAsync( await this._anime.GetSourceLink( Links.ElementAt(i) ), i ) );
        //             i++;
        //         }

        //         await Task.WhenAll( tasks );
        //         tasks.Clear();
        //     }
        // }

        public void Dispose()
        {
            this._urlsCollectionFailed.Clear();
        }
    }
}