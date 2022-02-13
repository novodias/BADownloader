using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

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

    public async Task MainAsync()
    {
        System.Console.Title = "Better Animes Downloader";
        System.Console.WriteLine("Exemplo de url: https://betteranime.net/anime/legendado/shingeki-no-kyojin");
        System.Console.WriteLine("Insira url do anime: ");
        string url = Console.ReadLine() ?? throw new Exception("Url não pode ficar vazio");

        ChromeOptions options = new ChromeOptions();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            string location;

            if (File.Exists(@"%ProgramFiles%\Google\Chrome\Application\chrome.exe"))
            {
                location = @"%ProgramFiles%\Google\Chrome\Application\chrome.exe";
            }
            else if (File.Exists(@"%ProgramFiles(x86)%\Google\Chrome\Application\chrome.exe"))
            {
                location = @"%ProgramFiles(x86)%\Google\Chrome\Application\chrome.exe";
            }
            else
            {
                location = @"%LocalAppData%\Google\Chrome\Application\chrome.exe";
            }

            options.BinaryLocation = location;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            string location = "/usr/bin/google-chrome-stable";

            options.BinaryLocation = location;
        }

        options.AddArguments("--headless", "--disable-gpu", "--log-level=0", "--incognito");


        try
        {
            // --------------------------------------------

            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(url);
            var animename = doc.DocumentNode.SelectSingleNode("//*[@id='page-content']/main/div[1]/div/h2").InnerText;
            var episodes_length = int.Parse(doc.DocumentNode.SelectSingleNode("//*[@id='page-content']/main/div[1]/div/p[4]/span").InnerText);

            System.Console.WriteLine("Anime: {0}\nNúmero de episódios: {1}", animename, episodes_length);

            System.Console.WriteLine("Digite de qual episódio você quer começar baixar:");
            string startpoint = Console.ReadLine() ?? throw new Exception("Número de episódios não pode ficar vazio");
            int startindex = int.Parse(startpoint);

            // DEBUG
            // System.Console.WriteLine(startindex);
            // DEBUG

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

            var browser = new ChromeDriver(options);

            if (!Directory.Exists($"Animes/{animename}"))
                Directory.CreateDirectory($"Animes/{animename}");

            for (int i = startindex; i < episodes_length + 1 ; i++)
            {
                System.Console.WriteLine(GetEpisodeUrl(url, i));
                var page = await web.LoadFromWebAsync(GetEpisodeUrl(url, i));
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
            
            System.Console.WriteLine("Downloads concluídos!");

            // System.Console.WriteLine(media);
        }
        catch (System.Exception)
        {
            throw;
        }

        // a7c12f5645a203e46018b6f2cab8c115
    }
}