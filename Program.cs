using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;

using System.Runtime.InteropServices;

using HtmlAgilityPack;
using OpenQA.Selenium.Support.UI;

class Program
{
    public static void Main()
    {
        var pro = new Program();
        pro.MainAsync().GetAwaiter().GetResult();
    }

    HtmlWeb? web;
    ChromeOptions? chrome;
    FirefoxOptions? firefox;

    public async Task MainAsync()
    {
        System.Console.Title = "Better Animes Downloader";
        System.Console.WriteLine("Navegadores suportados: Chrome e Opera.");
        System.Console.WriteLine("Exemplo de url: https://betteranime.net/anime/legendado/shingeki-no-kyojin");
        System.Console.WriteLine("Insira url do anime: ");
        string url = Console.ReadLine() ?? throw new Exception("Url não pode ficar vazio");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            string location;
            string programfilesx86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            string programfiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string localappdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            System.Console.WriteLine("Escolha o navegador: 1. [Chrome] | 2. [Firefox]");
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
                    
                    chrome = new ChromeOptions();

                    chrome.BinaryLocation = location;
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

                    firefox = new FirefoxOptions();

                    firefox.BrowserExecutableLocation = location;
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
            System.Console.WriteLine("Apenas o Chrome é suportado no linux.");
            string location = "/usr/bin/google-chrome-stable";

            chrome = new();

            chrome.BinaryLocation = location;
            chrome.AddArguments("--headless", "--disable-gpu", "--log-level=0", "--incognito");

            chrome.SetLoggingPreference(LogType.Browser, LogLevel.Off);
            chrome.SetLoggingPreference(LogType.Client, LogLevel.Off);
            chrome.SetLoggingPreference(LogType.Driver, LogLevel.Off);
            chrome.SetLoggingPreference(LogType.Profiler, LogLevel.Off);
            chrome.SetLoggingPreference(LogType.Server, LogLevel.Off);

        }

        try
        {
            // --------------------------------------------

            this.web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(url);
            var animename = doc.DocumentNode.SelectSingleNode("//*[@id='page-content']/main/div[1]/div/h2").InnerText;
            var episodes_length = int.Parse(doc.DocumentNode.SelectSingleNode("//*[@id='page-content']/main/div[1]/div/p[4]/span").InnerText);

            System.Console.WriteLine("Anime: {0}\nNúmero de episódios: {1}", animename, episodes_length);

            System.Console.WriteLine("Digite de qual episódio você quer começar baixar:");
            string startpoint = Console.ReadLine() ?? throw new Exception("Número de episódios não pode ficar vazio");
            int startindex = int.Parse(startpoint);

            System.Console.WriteLine("Selecione a qualidade de vídeo preferida: 1.[SD] | 2.[HD] | 3.[FULL HD]\nOBS: Nem todas as qualidades estarão disponíveis dependendo do anime.");
            string qualityinput = Console.ReadLine() ?? throw new Exception("Escolha entre 1, 2 ou 3.");

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
                    System.Console.WriteLine("??? selecionei o sd pra vc");
                    quality = "//*[@id='page-content']/div[2]/section/div[2]/div[1]/div/a[1]";
                break;
            } 

            // --------------------------------------------

            if (!Directory.Exists($"Animes/{animename}"))
                Directory.CreateDirectory($"Animes/{animename}");
            
            IWebDriver browser;
            if ( chrome != null )
            {
                System.Console.WriteLine("Abrindo browser, isso pode demorar um pouco!");
                await Task.Delay(TimeSpan.FromSeconds(5));
                browser = new ChromeDriver(@"drivers\", chrome, TimeSpan.FromSeconds(180));
                await Download(browser, animename, url, quality, startindex, episodes_length);
            }
            else if ( firefox != null )
            {
                System.Console.WriteLine("Abrindo browser, isso pode demorar um pouco!");
                await Task.Delay(TimeSpan.FromSeconds(5));
                browser = new FirefoxDriver(@"drivers\", firefox, TimeSpan.FromSeconds(180));
                await Download(browser, animename, url, quality, startindex, episodes_length);
            }

            System.Console.WriteLine("Downloads concluídos!");
        }
        catch (System.Exception)
        {
            throw;
        }
    }

    public static string GetEpisodeUrl(string url, int index)
    {
        if (index > 0 && index <= 9)
        {
            return url + "/episodio-0" + Convert.ToString(index) + "/download";
        }
        else
        {
            return url + "/episodio-" + Convert.ToString(index) + "/download";
        }
    }

    public async Task Download(IWebDriver browser, string animename, string url, string quality, int startindex, int episodes_length)
    {
        for (int i = startindex; i < episodes_length + 1 ; i++)
        {
            System.Console.WriteLine(GetEpisodeUrl(url, i));
            if ( this.web is null) throw new Exception("HtmlWeb web null!");
            var page = await this.web.LoadFromWebAsync(GetEpisodeUrl(url, i));
            System.Console.WriteLine("Procurando link de download...");
            var media = page.DocumentNode.SelectSingleNode(quality).GetAttributeValue("href", "");

            browser.Navigate().GoToUrl(media);

            // --------------------------------------------------

            System.Console.WriteLine("Esperando carregar...");

            IWait<IWebDriver> wait = new WebDriverWait(browser, TimeSpan.FromSeconds(30.00));

            wait.Until(browser => ((IJavaScriptExecutor)browser).ExecuteScript("return document.readyState").Equals("complete"));
            
            // --------------------------------------------------

            // //*[@id="page-content"]/div[2]/section/div[2]/div[1]/div/a[1]
            browser.FindElement(By.XPath("//*[@id='__next']/header/div/div/button")).Click();
            var animeurl = browser.FindElement(By.XPath("//*[@id='__next']/header/div/div/a")).GetAttribute("href");

            using (var http = new HttpClient())
            {
                using (var response = await http.GetStreamAsync(animeurl))
                {
                    using (var file = File.OpenWrite($"Animes/{animename}/{i}"))
                    {
                        System.Console.WriteLine($"Baixando episódio {i}...");
                        await response.CopyToAsync(file);
                    }
                }
            }
        }

        browser.Dispose();
    }
}