using Spectre.Console;
using HtmlAgilityPack;
using BADownloader.Extractor;
using BADownloader.Extractor.Sites;

namespace BADownloader
{
    class Program
    {

        public static bool IsDebugMode = false;

        public static void Main(string[] args)
        {
            if ( args.Contains("--debug") )
                IsDebugMode = true;

            MainAsync().GetAwaiter().GetResult();
        }

        public static async Task MainAsync()
        {
            Console.Title = "BADownloader";

            try
            {
                IExtractor anime = await GetAnimeTypeAsync();
                
                if ( IsDebugMode )                
                    anime.WriteDebug();

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

        static async Task<IExtractor> GetAnimeTypeAsync()
        {
            AnsiConsole.Write(new Markup("Exemplo de url: https://betteranime.net/anime/legendado/shingeki-no-kyojin\n"));
            AnsiConsole.Write(new Markup("Exemplo de url: https://animeyabu.com/anime/kimetsu-no-yaiba-yuukaku-hen-part-3\n"));
            
            string url = AnsiConsole.Ask<string>("Insira a URL do anime:");
            bool IsSiteSupported = AvailableSites.DictList.Keys.Any( ctx => url.Contains(ctx) );

            while ( !IsSiteSupported )
            {
                AnsiConsole.Write(new Markup("[red underline]URL inválido[/]\n"));
                url = AnsiConsole.Ask<string>("Insira a URL do anime:");
                IsSiteSupported = AvailableSites.DictList.Keys.Any( ctx => ctx == url );
            }

            HtmlWeb web = new();
            HtmlDocument doc = await web.LoadFromWebAsync(url);

            var siteEnum = AvailableSites.GetSite( url );

            return siteEnum switch
            {
                SiteEnum.BetterAnime => new BetterAnime(doc, url),
                SiteEnum.AnimeYabu => await AnimeYabu.InitializeExtractorAsync(doc, url),
                _ => throw new Exception("Algo errado ocorreu")
            };
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