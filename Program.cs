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

            try
            {
                IAnime anime = await GetAnimeTypeAsync();                
                // anime.WriteDebug();

                int downloadnum = DownloadInput();

                DownloadManager Manage = new( anime, downloadnum );

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
                // Não sei se isso faz alguma diferença
                BADHttp.Dispose();

                Console.WriteLine("Pressione qualquer botão pra sair");
                Console.ReadKey();
            }
        }

        static async Task<IAnime> GetAnimeTypeAsync()
        {
            AnsiConsole.Write(new Markup("Exemplo de url: https://betteranime.net/anime/legendado/shingeki-no-kyojin\n"));
            AnsiConsole.Write(new Markup("Exemplo de url: https://animeyabu.com/anime/kimetsu-no-yaiba-yuukaku-hen-part-3\n"));
            
            string url = AnsiConsole.Ask<string>("Insira a URL do anime:");

            IAnime anime;
            HtmlWeb web = new();
            HtmlDocument doc = await web.LoadFromWebAsync(url);

            if (url.StartsWith("https://betteranime.net"))
            {
                anime = new BetterAnime(doc, url);
            }
            else if (url.StartsWith("https://animeyabu.com"))
            {
                anime = new AnimeYabu(doc, url);
            }
            else
            {
                throw new Exception("O site que você colocou não é suportado");
            }
            
            return anime;
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