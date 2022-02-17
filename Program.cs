using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using HtmlAgilityPack;

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

            Console.WriteLine("Navegadores suportados: Chrome e Firefox.");
            Console.WriteLine("Exemplo de url: https://betteranime.net/anime/legendado/shingeki-no-kyojin");
            Console.WriteLine("Insira url do anime: ");
            string url = Console.ReadLine() ?? throw new Exception("URL não pode ficar vazio");

            Console.WriteLine();

            if (!url.StartsWith("https://betteranime.net")) throw new Exception("URL inválida");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string location;
                string programfilesx86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                string programfiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                string localappdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                Console.WriteLine("Escolha o navegador: 1. [Chrome] | 2. [Firefox]");
                string navegador = Console.ReadLine() ?? throw new Exception("Navegador não pode ser vazio.");

                if (navegador == "1")
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
                else if (navegador == "2")
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
                Console.WriteLine("Escolha o navegador: 1. [Chrome] | 2. [Firefox]");
                string navegador = Console.ReadLine() ?? throw new Exception("Navegador não pode ser vazio.");

                Console.WriteLine("Insira o path do executável ");
                Console.WriteLine("Não sabe onde fica? Abra o terminal e use o comando \"whereis\"");
                Console.WriteLine("Deixe em branco caso tenha selecionado o Chrome:");

                if (navegador == "1")
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
                else if (navegador == "2")
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

            try
            {
                // --------------------------------------------

                var web = new HtmlWeb();
                var doc = await web.LoadFromWebAsync(url);
                var animename = doc.DocumentNode.SelectSingleNode("//*[@id='page-content']/main/div[1]/div/h2").InnerText;
                var episodes_length = int.Parse(doc.DocumentNode.SelectSingleNode("//*[@id='page-content']/main/div[1]/div/p[4]/span").InnerText);

                Console.WriteLine("\nAnime: {0}\nNúmero de episódios: {1}", animename, episodes_length);

                // --------------------------------------------

                Console.WriteLine("Digite de qual episódio você quer começar baixar:");
                string strstartpoint = Console.ReadLine() ?? throw new Exception("Número do episódio não pode ficar vazio");

                int startpoint = int.Parse(strstartpoint);
                if (startpoint < 1) startpoint = 1;
                else if (startpoint > episodes_length) startpoint = episodes_length;

                // --------------------------------------------

                Console.WriteLine("\nQuantos downloads deseja ter? [1], [2], [3], [4] ou [5]\nRecomendado um PC bom suficiente para baixar 5/4/3/2 arquivos simultâneamente");
                string strdownloadnum = Console.ReadLine() ?? "1";

                int downloadnum = int.Parse(strdownloadnum);

                if (downloadnum < 1) downloadnum = 1;
                else if (downloadnum > 5) downloadnum = 5;

                // --------------------------------------------

                Console.WriteLine("\nSelecione a qualidade de vídeo preferida: 1.[SD] | 2.[HD] | 3.[FULL HD]\nOBS: Nem todas as qualidades estarão disponíveis dependendo do anime.");
                Console.WriteLine("Cada episódio pode variar de ~100mb à ~1gb dependendo da qualidade");
                Console.WriteLine("Verifique se seu disco contém espaço suficiente!");
                string qualityinput = Console.ReadLine() ?? "1";

                string quality = string.Empty;

                switch (qualityinput)
                {
                    case "1":
                        quality = "//*[@id='page-content']/div[2]/section/div[2]/div[1]/div/a[1]";
                    break;

                    case "2":
                        quality = "//*[@id='page-content']/div[2]/section/div[2]/div[1]/div/a[2]";
                    break;

                    case "3":
                        quality = "//*[@id='page-content']/div[2]/section/div[2]/div[1]/div/a[3]";
                    break;

                    default:
                        Console.WriteLine("??? selecionei o sd pra vc");
                        quality = "//*[@id='page-content']/div[2]/section/div[2]/div[1]/div/a[1]";
                    break;
                } 

                // --------------------------------------------
                
                string chars = Regex.Escape(@"<>:" + "\"" + "/|?*");
                string pattern = "[" + chars + "]";
                animename = Regex.Replace(animename, pattern, "");

                // --------------------------------------------

                if (!Directory.Exists($"Animes/{animename}"))
                    Directory.CreateDirectory($"Animes/{animename}");

                // --------------------------------------------
                
                Console.WriteLine("\nAbrindo browser, isso pode demorar um pouco!");
                await Task.Delay(TimeSpan.FromSeconds(5));
                
                // --------------------------------------------

                if ( chrome != null )
                {
                    this.browser = new ChromeDriver(@"drivers", chrome, TimeSpan.FromSeconds(180));
                }
                else
                {
                    this.browser = new FirefoxDriver(@"drivers", firefox, TimeSpan.FromSeconds(180));
                }

                Anime anime = new(animename, episodes_length, url, startpoint, quality);

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