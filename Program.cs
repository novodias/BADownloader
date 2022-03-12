using HtmlAgilityPack;
using Spectre.Console;

namespace BADownloader
{
    class Program
    {
        public static void Main()
        {
            var pro = new Program();
            pro.MainAsync().GetAwaiter().GetResult();
        }

        public async Task MainAsync()
        {
            Console.Title = "BADownloader";

            AnsiConsole.Write(new Markup("Exemplo de url: https://betteranime.net/anime/legendado/shingeki-no-kyojin\n"));
            string url = AnsiConsole.Ask<string>("Insira a URL do anime:");

            if (!url.StartsWith("https://betteranime.net")) throw new Exception("URL inválida");

            try
            {
                HtmlWeb web = new();

                Anime anime = await AnimeInfo.GetAnimeInfo(url, web);

                // --------------------------------------------

                if (!Directory.Exists($"Animes/{anime.Name}"))
                    Directory.CreateDirectory($"Animes/{anime.Name}");
                
                // --------------------------------------------

                int downloadnum = AnimeInfo.DownloadInput();

                DownloadManager Manage = new(downloadnum, anime.AnimeLength, anime);

                await Manage.StartDownloadAsync();

                Manage.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Algum erro ocorreu:");
                AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
            }
            finally
            {
                Console.WriteLine("Pressione qualquer botão pra sair");
                Console.ReadKey();
            }
        }
    }
}