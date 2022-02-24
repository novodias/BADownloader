using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
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

        // HtmlWeb? web;
        ChromeOptions? chrome;
        FirefoxOptions? firefox;
        IWebDriver? browser;

        public async Task MainAsync()
        {
            Console.Title = "BADownloader";

            #region BROWSER

            AnsiConsole.Write(new Markup("Navegadores suportados: [bold yellow]Chrome[/] e [bold red]Firefox[/]\n"));
            AnsiConsole.Write(new Markup("Exemplo de url: https://betteranime.net/anime/legendado/shingeki-no-kyojin\n"));
            // Console.WriteLine("Insira url do anime: ");
            string url = AnsiConsole.Ask<string>("Insira a URL do anime:");

            if (!url.StartsWith("https://betteranime.net")) throw new Exception("URL inválida");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string location;
                string programfilesx86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                string programfiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                string localappdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                // Console.WriteLine("Escolha o navegador: 1. [Chrome] | 2. [Firefox]");
                // string navegador = Console.ReadLine() ?? throw new Exception("Navegador não pode ser vazio.");

                var navegador = AnsiConsole.Prompt(new SelectionPrompt<string>()
                    .Title("Selecione o seu navegador:")
                    .PageSize(10)
                    .AddChoices(new [] 
                    {
                        "Chrome", "Firefox"
                    }));

                if (navegador == "Chrome")
                {
                    string[] chromeloc = 
                    {
                        $@"{programfiles}\Google\Chrome\Application\chrome.exe",
                        $@"{programfilesx86}\Google\Chrome\Application\chrome.exe",
                        $@"{localappdata}\Google\Chrome\Application\chrome.exe"
                    };
                    if (File.Exists(chromeloc[0]) || File.Exists(chromeloc[1]) || File.Exists(chromeloc[2]))
                    {
                        if (File.Exists(chromeloc[0]))
                            location = chromeloc[0];
                        else if (File.Exists(chromeloc[1]))
                            location = chromeloc[1];
                        else
                            location = chromeloc[2];

                        chrome = new ChromeOptions
                        {
                            BinaryLocation = location
                        };

                        chrome.AddArguments("--headless", "--disable-gpu", "--log-level=3", "--incognito", "--no-sandbox");

                        chrome.SetLoggingPreference(LogType.Browser, LogLevel.Off);
                        chrome.SetLoggingPreference(LogType.Client, LogLevel.Off);
                        chrome.SetLoggingPreference(LogType.Driver, LogLevel.Off);
                        chrome.SetLoggingPreference(LogType.Profiler, LogLevel.Off);
                        chrome.SetLoggingPreference(LogType.Server, LogLevel.Off);
                    }
                }
                else if (navegador == "Firefox")
                {
                    string[] firefoxloc = 
                    {
                        $@"{localappdata}\Mozilla Firefox\firefox.exe",
                        $@"{programfiles}\Mozilla Firefox\firefox.exe",
                        $@"{programfilesx86}\Mozilla Firefox\firefox.exe"
                    };

                    if (File.Exists(firefoxloc[0]) || File.Exists(firefoxloc[1]) || File.Exists(firefoxloc[2]))
                    {
                        if (File.Exists(firefoxloc[0]))
                            location = firefoxloc[0];
                        else if (File.Exists(firefoxloc[1]))
                            location = firefoxloc[1];
                        else
                            location = firefoxloc[2];

                        firefox = new FirefoxOptions
                        {
                            BrowserExecutableLocation = location
                        };

                        firefox.AddArguments("--headless", "--disable-gpu", "--log-level=3", "--incognito", "--no-sandbox");

                        firefox.SetLoggingPreference(LogType.Browser, LogLevel.Off);
                        firefox.SetLoggingPreference(LogType.Client, LogLevel.Off);
                        firefox.SetLoggingPreference(LogType.Driver, LogLevel.Off);
                        firefox.SetLoggingPreference(LogType.Profiler, LogLevel.Off);
                        firefox.SetLoggingPreference(LogType.Server, LogLevel.Off);
                    }
                }
                else
                {
                    Environment.Exit(0);
                }


            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Console.WriteLine("Escolha o navegador: 1. [Chrome] | 2. [Firefox]");
                // string navegador = Console.ReadLine() ?? throw new Exception("Navegador não pode ser vazio.");

                var navegador = AnsiConsole.Prompt(new SelectionPrompt<string>()
                    .Title("Selecione o seu navegador:")
                    .PageSize(10)
                    .AddChoices(new [] 
                    {
                        "Chrome", "Firefox"
                    }));

                Console.WriteLine("Insira o path do executável ");
                Console.WriteLine("Não sabe onde fica? Abra o terminal e use o comando \"whereis\"");
                Console.WriteLine("Deixe em branco caso tenha selecionado o Chrome:");

                if (navegador == "Chrome")
                {
                    string location = Console.ReadLine() ?? "/usr/bin/google-chrome-stable";
                    if (string.IsNullOrEmpty(location)) location = "/usr/bin/google-chrome-stable";

                    if (File.Exists(location))
                    {
                        chrome = new ChromeOptions
                        {
                            BinaryLocation = location
                        };

                        chrome.AddArguments("--headless", "--disable-gpu", "--log-level=3", "--incognito", "--no-sandbox");

                        chrome.SetLoggingPreference(LogType.Browser, LogLevel.Off);
                        chrome.SetLoggingPreference(LogType.Client, LogLevel.Off);
                        chrome.SetLoggingPreference(LogType.Driver, LogLevel.Off);
                        chrome.SetLoggingPreference(LogType.Profiler, LogLevel.Off);
                        chrome.SetLoggingPreference(LogType.Server, LogLevel.Off);
                    }
                }
                else if (navegador == "Firefox")
                {
                    string location = Console.ReadLine() ?? throw new Exception("Insira o path do Firefox!");
                    if (File.Exists(location))
                    {
                        firefox = new FirefoxOptions
                        {
                            BrowserExecutableLocation = location
                        };

                        firefox.AddArguments("--headless", "--disable-gpu", "--log-level=3", "--incognito", "--no-sandbox");

                        firefox.SetLoggingPreference(LogType.Browser, LogLevel.Off);
                        firefox.SetLoggingPreference(LogType.Client, LogLevel.Off);
                        firefox.SetLoggingPreference(LogType.Driver, LogLevel.Off);
                        firefox.SetLoggingPreference(LogType.Profiler, LogLevel.Off);
                        firefox.SetLoggingPreference(LogType.Server, LogLevel.Off);
                    }
                }
                else
                {
                    Environment.Exit(0);
                }
            }

            #endregion 

            try
            {
                HtmlWeb web = new();
                string animename;
                int episodes_length;

                List<string> info = await AnimeInfo.GetAnimeInfo(url, web);
                
                animename = info.ElementAt(0);
                episodes_length = int.Parse(info.ElementAt(1));
                // string genres = info.ElementAt(2);

                string chars = Regex.Escape(@"<>:" + "\"" + "/|?*");
                string pattern = "[" + chars + "]";
                animename = Regex.Replace(animename, pattern, "");

                AnsiConsole.Write(new Markup(string.Format("Anime: [green bold]{0}[/]\nNúmero de episódios: [green bold]{1}[/]\n", animename, episodes_length)));
                // AnsiConsole.Write(new Markup(string.Format($"Gêneros: {genres}\n")));
                
                int[] episodes = new int[episodes_length];

                // --------------------------------------------

                bool animeExist = AnimeInfo.CheckExistingFolder(animename);

                if (animeExist)
                {
                    episodes = AnimeInfo.ExistingEpisodes(animename);

                    episodes = AnimeInfo.OtherEpisodes(episodes, episodes_length);

                    foreach (var i in episodes)
                    {
                        Console.WriteLine($"Episódio faltando: {i}");
                    }
                }
                else
                {
                    for (int i = 0; i < episodes_length; i++)
                    {
                        episodes[i] = i + 1;
                    }
                }

                // --------------------------------------------

                // startpoint faz exatamente nada no momento.
                int startpoint = AnimeInfo.EpisodeInput(episodes_length, episodes) - 1;
                
                int downloadnum = AnimeInfo.DownloadInput();
                string quality = AnimeInfo.QualityInput();

                // --------------------------------------------

                if (!Directory.Exists($"Animes/{animename}"))
                    Directory.CreateDirectory($"Animes/{animename}");
                
                // --------------------------------------------

                Console.WriteLine("\nAbrindo navegador, isso pode demorar um pouco!");
                await Task.Delay(TimeSpan.FromSeconds(5));

                if ( chrome != null )
                {
                    this.browser = new ChromeDriver(@"drivers", chrome, TimeSpan.FromSeconds(180));
                }
                else
                {
                    this.browser = new FirefoxDriver(@"drivers", firefox, TimeSpan.FromSeconds(180));
                }

                Anime anime = new(animename, episodes, episodes_length, url, startpoint, quality);

                DownloadManager Manage = new(downloadnum, episodes_length, anime);

                await Manage.GetLinkDownloadAsync(this.browser, web);
                await Manage.StartDownloadAsync();

                Manage.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Algum erro ocorreu: " + ex.Message + ex.StackTrace);
            }
            finally
            {
                if ( this.browser is not null )
                    this.browser.Dispose();

                Console.WriteLine("Pressione qualquer botão pra sair");
                Console.ReadKey();
            }
        }
    }
}