using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using HtmlAgilityPack;

namespace src
{
    public class DownloadManager
    {
        private readonly int TaskDownload;
        private List<string> URLsDownload;
        private readonly int StartPoint;
        public DownloadManager(int amount, int anime_length, int startpoint)
        {
            this.TaskDownload = amount;
            this.URLsDownload = new(anime_length);
            this.StartPoint = startpoint;
        }

        private static string GetEpisodeUrl(string url, int index)
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

        public async Task GetLinkDownload(IWebDriver browser, HtmlWeb web, string animename, string url, string quality, int startindex, int episodes_length)
        {
            Console.WriteLine($"Anime: {animename}\n");
            for (int i = startindex; i < episodes_length + 1 ; i++)
            {
                // Console.WriteLine(GetEpisodeUrl(url, i));
                Console.WriteLine($"Procurando link de download: [{i}/{episodes_length}]");
                
                if ( web is null ) throw new Exception("HtmlWeb web null!");
                var page = await web.LoadFromWebAsync(GetEpisodeUrl(url, i));
                var media = page.DocumentNode.SelectSingleNode(quality).GetAttributeValue("href", "");

                browser.Navigate().GoToUrl(media);

                // --------------------------------------------------

                // Console.WriteLine("Esperando carregar...");

                IWait<IWebDriver> wait = new WebDriverWait(browser, TimeSpan.FromSeconds(30.00));

                // wait.Until(browser => ((IJavaScriptExecutor)browser).ExecuteScript("return document.readyState").Equals("complete"));
                wait.Until(x => x.FindElement(By.XPath("//*[@id='__next']/header/div/div/button")));
                browser.FindElement(By.XPath("//*[@id='__next']/header/div/div/button")).Click();
                
                // --------------------------------------------------

                var episodeURL = browser.FindElement(By.XPath("//*[@id='__next']/header/div/div/a")).GetAttribute("href");

                this.URLsDownload.Add(episodeURL);
                // Console.Clear();
            }
        }

        private async Task Download(int i, int animelength, string animename)
        {

            var downloadURL = this.URLsDownload.ElementAt(i);
            var animeindex = i + this.StartPoint;

            using (var http = new HttpClient())
            {
                using var resp = await http.GetStreamAsync(downloadURL);
                // resp.EnsureSuccessStatusCode();

                Console.WriteLine($"Baixando episódio: [{animeindex}/{animelength}]");

                using var file = File.OpenWrite($"Animes/{animename}/{animename}-{animeindex}.mp4");
                await resp.CopyToAsync(file);
            }

            Console.WriteLine($"Download do episódio [{animeindex}/{animelength}] concluído.");
        }

        public async Task StartDownload(string animename, int animelength)
        {
            Console.WriteLine("\nComeçando download.\n");

            int length = this.URLsDownload.Count;
            
            // int animeindex = 1;
            for (int i = 0; i < length;)
            {
                List<Task> tasks = new();
                // animeindex += i;

                if ( this.URLsDownload.ElementAtOrDefault(i) is not null )
                {
                    tasks.Add(Download(i, animelength, animename));
                    i++;
                }
                if ( this.URLsDownload.ElementAtOrDefault(i) is not null && TaskDownload >= 2 )
                {
                    tasks.Add(Download(i, animelength, animename));
                    i++;
                }
                if ( this.URLsDownload.ElementAtOrDefault(i) is not null && TaskDownload >= 3 )
                {
                    tasks.Add(Download(i, animelength, animename));
                    i++;
                }
                if ( this.URLsDownload.ElementAtOrDefault(i) is not null && TaskDownload >= 4 )
                {
                    tasks.Add(Download(i, animelength, animename));
                    i++;
                }
                if ( this.URLsDownload.ElementAtOrDefault(i) is not null && TaskDownload == 5 )
                {
                    tasks.Add(Download(i, animelength, animename));
                    i++;
                }

                Console.WriteLine("Esperando download.");
                await Task.WhenAll(tasks);
                tasks.Clear();
                // tasks.RemoveRange(0, tasks.Count);
            }

            Console.WriteLine("Downloads concluídos!");
        }

        public void Dispose()
        {
            // this.TaskDownload.Clear();
            this.URLsDownload.Clear();
        }
    }

}