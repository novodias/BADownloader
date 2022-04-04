using Spectre.Console;
using HtmlAgilityPack;
using BADownloader.Extractor;
using BADownloader.Extractor.Sites;

namespace BADownloader
{
    class Program
    {

        public static bool IsDebugMode = false;

        public static bool IsWindows7
        {
            get 
            {
                if ( !Environment.OSVersion.Platform.Equals(PlatformID.Win32NT) )
                    return false;

                if ( !Environment.OSVersion.Version.ToString().Equals("6.1") )
                    return false;

                return true;
            }
        }

        public static void Main(string[] args)
        {
            if ( args.Contains("--debug") )
                IsDebugMode = true;

            MainAsync().GetAwaiter().GetResult();
        }

        public static async Task MainAsync()
        {
            Console.Title = "BADownloader";
            
            if ( IsDebugMode )
                System.Console.WriteLine("DebugMode = true");

            try
            {
                BADHttp.Client.Timeout = TimeSpan.FromMinutes(10);
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
                if ( !IsWindows7 )
                    AnsiConsole.WriteException(ex, ExceptionFormats.Default);
                else
                    Console.WriteLine( ex.ToString() );
            }
            finally
            {
                // Não sei se isso faz alguma diferença
                BADHttp.Dispose();

                Console.WriteLine("Pressione qualquer botão pra sair");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Seleciona o extrator necessário pro site inserido
        /// </summary>
        /// <returns>Extrator</returns>
        /// <exception cref="Exception"></exception>
        static async Task<IExtractor> GetAnimeTypeAsync()
        {
            AnsiConsole.Write(new Markup("Exemplo de url: https://betteranime.net/anime/legendado/shingeki-no-kyojin\n"));
            AnsiConsole.Write(new Markup("Exemplo de url: https://animeyabu.com/anime/kimetsu-no-yaiba-yuukaku-hen-part-3\n"));

            string url;
            if ( !IsWindows7 )
                url = AnsiConsole.Ask<string>("Insira a URL do anime:");
            else 
            {
                Console.WriteLine("Insira a URL do Anime: ");
                url = Console.ReadLine() ?? string.Empty;
            }
            
            bool IsSiteSupported = AvailableSites.DictList.Keys.Any(ctx => url.Contains(ctx));

            while ( !IsSiteSupported )
            {
                if ( !IsWindows7 )
                {
                    AnsiConsole.Write(new Markup("[red underline]URL inválido[/]\n"));
                    url = AnsiConsole.Ask<string>("Insira a URL do anime:");
                }
                else 
                {
                    Console.WriteLine("URL inválido\n");
                    Console.WriteLine("Insira a URL do Anime: ");
                    url = Console.ReadLine() ?? string.Empty;
                }

                IsSiteSupported = AvailableSites.DictList.Keys.Any( ctx => url.Contains(ctx) );
            }

            var response = await BADHttp.SendAsync(url);
            HtmlDocument doc = new();
            doc.Load(await response.Content.ReadAsStreamAsync());

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
            string str;
            if ( !IsWindows7 )
                str = AnsiConsole.Prompt(new SelectionPrompt<string>()
                    .Title("Quantos downloads deseja ter? [green]1[/], [green]2[/], [green]3[/], [green]4[/] ou [green]5[/]\nRecomendado um PC bom suficiente para baixar 5/4/3/2 arquivos simultâneamente")
                    .PageSize(10)
                    .AddChoices(new [] 
                    {
                        "1", "2", "3", "4", "5"
                    })
                );
            else
            {
                Console.WriteLine("Quantos downloads deseja ter? 1, 2, 3, 4 ou 5\nRecomendado um PC bom suficiente para baixar 5/4/3/2 arquivos simultâneamente");
                str = Console.ReadLine() ?? string.Empty;
            }

            if (!int.TryParse(str, out int input))
            {
                Console.WriteLine("Isso não é um número!");
                return DownloadInput();
            }
            else
            {
                if (input < 1) input = 1;
                else if (input > 5) input = 5;
                return input;
            }
        }
    }
}