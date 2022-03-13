using BADownloader.Sites;
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
                Anime anime = await GetAnimeTypeAsync();

                // --------------------------------------------

                if (!Directory.Exists($"Animes/{anime.Name}"))
                    Directory.CreateDirectory($"Animes/{anime.Name}");
                
                // --------------------------------------------

                int downloadnum = DownloadInput();

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

        static async Task<Anime> GetAnimeTypeAsync()
        {
            AnsiConsole.Write(new Markup("Exemplo de url: https://betteranime.net/anime/legendado/shingeki-no-kyojin\n"));
            string url = AnsiConsole.Ask<string>("Insira a URL do anime:");

            HtmlWeb web = new();

            if (url.StartsWith("https://betteranime.net"))
            {
                BetterAnime info = new();
                return await GetAnimeInfoAsync(info, url, web);
            }
            else if (url.StartsWith("https://animeyabu.com"))
            {
                AnimeYabu info = new();
                return await GetAnimeInfoAsync(info, url, web);
            }
            else
            {
                throw new Exception("O site que você colocou não é suportado");
            }
        }

        static Task<Anime> GetAnimeInfoAsync(IAnimeInfo info, string url, HtmlWeb web)
        {
            return info.GetAnimeAsync(url, web);
        }

        static int DownloadInput()
        {
            var str = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title("Quantos downloads deseja ter? [green]1[/], [green]2[/], [green]3[/], [green]4[/] ou [green]5[/]\nRecomendado um PC bom suficiente para baixar 5/4/3/2 arquivos simultâneamente")
                .PageSize(10)
                .AddChoices(new [] 
                {
                    "1", "2", "3", "4", "5"
                }));

            if (!int.TryParse(str, out int input))
                throw new Exception("Isso não é um número!");
            else
            {
                if (input < 1) input = 1;
                else if (input > 5) input = 5;
                return input;
            }
        }
    }
}