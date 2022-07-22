using System.Runtime.InteropServices;
using MinimalLog;

namespace BADownloader.App
{
    public class Downloader
    {
        private readonly bool _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        private readonly IExtractor _anime;
        private readonly int _taskDownload;
        private readonly ACollection _collectionFailed;
        public Downloader(IExtractor anime, int amount)
        {
            this._anime = anime;
            this._taskDownload = amount;
            this._collectionFailed = new();
        }

        private async Task DownloadAsync(string url, int i, (int x, int y) position)
        {
            /*
            string epURL = string.Empty;
            if ( animeindex < 10 )
                epURL = "0" + animeindex;

            // string failedURL = info.URLDownload;
            */

            var info = this._anime.ACollection.GetAnimeInfo(i);
            int animeNumber = info.Number;

            string animefile = _isWindows ? $"{this._anime.Name}/{info.Name}.mp4" : $"{this._anime.Name}/{info.Name}";
            var path = Path.Combine(AnimesData.Directory, animefile);

            var fileinfo = new FileInfo(path);

            using var file = fileinfo.Create();

            try
            {
                var lastNumber = this._anime.ACollection.Last().Number;
                string message = $"Baixando: [{animeNumber}/{lastNumber}]";

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
                    BADConsole.Write("Progresso do download não está disponível.");
                    await source.CopyToAsync(file);
                }

                if ( this._collectionFailed.Contains(info) )
                    this._collectionFailed.Remove(info);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Download do episódio [{animeNumber}] falhou! {ex.Message}");
                MinimalLogger.Log<Downloader>(Logging.Error, ex.ToString());
                fileinfo.Delete();

                if ( !this._collectionFailed.Contains(info) )
                    this._collectionFailed.TryAdd(info);
            }
        }

        /// <summary>
        /// Inicia o ManageDownloader.
        /// </summary>
        /// <returns>Task</returns>
        public async Task StartAsync()
        {
            await this.DownloadAnime();

            Console.WriteLine($"\nDownloads concluídos!");

            await this.Retry();
        }

        /// <summary>
        /// Verifica se há URLsFailed, se sim, pergunta ao usuário se deseja baixar os episódios restantes e então os baixa.
        /// </summary>
        /// <returns>Task</returns>
        private async Task Retry()
        {
            if ( !( this._collectionFailed.Count > 0 ) )
                return;

            BADConsole.Write("Deseja baixar os episódios que falharam? ([green]Y;/[red]n;) ");
            ConsoleKey key = Console.ReadKey().Key;
            bool bkey = key == ConsoleKey.Y | key == ConsoleKey.Enter;

            await Task.Delay(TimeSpan.FromSeconds(5));

            if ( bkey )
            {
                Console.WriteLine("\nBaixando episódios que falharam..." + "\nEm caso de error 'Gone', feche e abra novamente o BADownloader");
                // await this.RetryManageDownloader( this._urlsCollectionFailed );
                Console.WriteLine("Opção ainda não implementada, inicie novamente o programa.");
            }
        }

        /// <summary>
        /// Baixa os episódios do anime
        /// </summary>
        /// <returns>Task</returns>
        private async Task DownloadAnime()
        {
            Console.CursorVisible = false;

            List<Task> tasks = new();
            var ACollection = this._anime.ACollection;

            Console.Clear();
            int current_cursor_position_top = 0;
            Console.SetCursorPosition(0, 0);

            for ( int i = this._anime.Index; i < ACollection.Count; )
            {
                for ( int j = 0; j < this._taskDownload; j++ )
                {
                    if ( i > ACollection.Count )
                        break;

                    await Task.Delay(TimeSpan.FromSeconds(3));
                    
                    // Pega o url de download do episódio do anime,
                    // e então pega o link direto implementado na classe
                    // do site.
                    var episodeurl = ACollection.GetAnimeInfo(i).URLDownload;
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
        //             if ( i > this._anime.ACollection.Count )
        //                 break;

        //             await Task.Delay(TimeSpan.FromSeconds(3));
                    
        //             tasks.Add( DownloadAsync( await this._anime.GetSourceLink( Links.ElementAt(i) ), i ) );
        //             i++;
        //         }

        //         await Task.WhenAll( tasks );
        //         tasks.Clear();
        //     }
        // }

        // public void Dispose()
        // {

        // }
    }
}