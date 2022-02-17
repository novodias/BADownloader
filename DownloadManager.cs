using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using HtmlAgilityPack;

namespace BADownloader
{
    public class DownloadManager
    {
        private static readonly HttpClient Client = new();
        private readonly int TaskDownload;
        private readonly List<string> URLsDownload;
        private readonly Anime Anime;
        public DownloadManager(int amount, int anime_length, Anime anime)
        {
            this.TaskDownload = amount;
            this.URLsDownload = new(anime_length);
            this.Anime = anime;
        }

        private string GetEpisodeUrl(int index)
        {
            if (index > 0 && index <= 9)
            {
                return this.Anime.URL + "/episodio-0" + Convert.ToString(index) + "/download";
            }
            else
            {
                return this.Anime.URL + "/episodio-" + Convert.ToString(index) + "/download";
            }
        }

        public async Task GetLinkDownloadAsync(IWebDriver browser, HtmlWeb web)
        {
            Console.WriteLine($"Anime: {this.Anime.Name}\n");
            for (int i = this.Anime.StartCount; i < this.Anime.Episodes + 1 ; i++)
            {
                Console.WriteLine($"Procurando link de download: [{i}/{this.Anime.Episodes}]");
                
                if ( web is null ) throw new Exception("HtmlWeb web null!");

                var page = await web.LoadFromWebAsync(GetEpisodeUrl(i));

                var media = page.DocumentNode.SelectSingleNode(this.Anime.Quality).GetAttributeValue("href", "");

                browser.Navigate().GoToUrl(media);

                // --------------------------------------------------

                IWait<IWebDriver> wait = new WebDriverWait(browser, TimeSpan.FromSeconds(30.00));

                wait.Until(x => x.FindElement(By.XPath("//*[@id='__next']/header/div/div/button")));

                browser.FindElement(By.XPath("//*[@id='__next']/header/div/div/button")).Click();
                
                // --------------------------------------------------

                var episodeURL = browser.FindElement(By.XPath("//*[@id='__next']/header/div/div/a")).GetAttribute("href");

                this.URLsDownload.Add(episodeURL);
            }
        }

        private async Task DownloadAsync(int i)
        {

            var downloadURL = this.URLsDownload.ElementAt(i);
            var animeindex = i + this.Anime.StartCount;

            try
            {
                var resp = await Client.GetAsync(downloadURL, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                
                resp.EnsureSuccessStatusCode();

                var mb = 1024L * 1024L;
                var tamanho = resp.Content.Headers.ContentLength / mb;

                resp.Dispose();

                var content = await Client.GetStreamAsync(downloadURL);

                Console.WriteLine($"Baixando episódio: [{animeindex}/{this.Anime.Episodes}] [{tamanho} mb]");

                var file = File.OpenWrite($"Animes/{this.Anime.Name}/{this.Anime.Name}-{animeindex}.mp4");
                await content.CopyToAsync(file);

                await content.DisposeAsync();
                await file.DisposeAsync();

                Console.WriteLine($"Download do episódio [{animeindex}/{this.Anime.Episodes}] concluído.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Download do episódio [{animeindex}] falhou! {ex.Message}");
            }
        }

        public async Task StartDownloadAsync()
        {
            Console.WriteLine("\nComeçando download.");

            int length = this.URLsDownload.Count;
            
            for (int i = 0; i < length;)
            {
                List<Task> tasks = new();

                if ( this.URLsDownload.ElementAtOrDefault(i) is not null )
                {
                    tasks.Add(DownloadAsync(i));
                    i++;
                }
                if ( this.URLsDownload.ElementAtOrDefault(i) is not null && TaskDownload >= 2 )
                {
                    tasks.Add(DownloadAsync(i));
                    i++;
                }
                if ( this.URLsDownload.ElementAtOrDefault(i) is not null && TaskDownload >= 3 )
                {
                    tasks.Add(DownloadAsync(i));
                    i++;
                }
                if ( this.URLsDownload.ElementAtOrDefault(i) is not null && TaskDownload >= 4 )
                {
                    tasks.Add(DownloadAsync(i));
                    i++;
                }
                if ( this.URLsDownload.ElementAtOrDefault(i) is not null && TaskDownload == 5 )
                {
                    tasks.Add(DownloadAsync(i));
                    i++;
                }

                await Task.WhenAll(tasks);
                tasks.Clear();
            }

            Console.WriteLine("Downloads concluídos!\n");
        }

        public void Dispose()
        {
            this.URLsDownload.Clear();
            Client.Dispose();
        }
    }

}