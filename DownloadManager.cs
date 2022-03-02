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
        private readonly List<string> URLsFailed;
        public DownloadManager(int amount, int anime_length, Anime anime)
        {
            this.TaskDownload = amount;
            this.URLsDownload = new(anime_length);
            this.Anime = anime;
            this.URLsFailed = new();
        }

        public async Task GetLinkDownloadAsync(IWebDriver browser, HtmlWeb web)
        {
            for (int i = this.Anime.Index; i < this.Anime.Episodes.Length; i++)
            {
                Console.WriteLine($"Procurando link de download: [{this.Anime.Episodes[i]}/{this.Anime.Episodes_Length}]");
                
                if ( web is null ) throw new Exception("HtmlWeb web null!");

                var page = await web.LoadFromWebAsync(this.Anime.LinkDownloads.ElementAt(i).Value);
                Console.WriteLine(this.Anime.LinkDownloads.ElementAt(i).Value);

                var media = page.DocumentNode.SelectSingleNode(this.Anime.Quality).GetAttributeValue("href", "");


                // --------------------------------------------------

                browser.Navigate().GoToUrl(media);

                IWait<IWebDriver> wait = new WebDriverWait(browser, TimeSpan.FromSeconds(30.00));
                wait.Until(x => x.FindElement(By.XPath("//*[@id='__next']/header/div/div/button")));

                browser.FindElement(By.XPath("//*[@id='__next']/header/div/div/button")).Click();
                var episodeURL = browser.FindElement(By.XPath("//*[@id='__next']/header/div/div/a")).GetAttribute("href");
                
                // --------------------------------------------------


                this.URLsDownload.Add(episodeURL);
            }
        }

        private async Task DownloadAsync(int i)
        {
            string downloadURL = this.URLsDownload.ElementAt(i);
            int animeindex = this.Anime.LinkDownloads.ElementAt(this.Anime.Index + i).Key;

            long mb = 1000 * 1000;

            if (this.URLsFailed.Contains(downloadURL))
            {
                this.URLsFailed.Remove(downloadURL);
            }

            try
            {
                var resp = await Client.GetAsync(downloadURL, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                
                resp.EnsureSuccessStatusCode();

                long? tamanho = resp.Content.Headers.ContentLength / mb;

                resp.Dispose();

                var content = await Client.GetStreamAsync(downloadURL);

                Console.WriteLine($"Baixando episódio: [{animeindex}/{this.Anime.Episodes_Length}] [{tamanho} mb]");

                var file = File.OpenWrite($"Animes/{this.Anime.Name}/{this.Anime.Name}-{animeindex}.mp4");

                await content.CopyToAsync(file);

                await content.DisposeAsync();
                await file.DisposeAsync();

                Console.WriteLine($"Download do episódio [{animeindex}/{this.Anime.Episodes_Length}] concluído.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Download do episódio [{animeindex}] falhou! {ex.Message}");
                this.URLsFailed.Add(downloadURL);
            }
        }
        public async Task StartDownloadAsync()
        {
            Console.WriteLine("\nComeçando download.");

            await this.ManageDownloader(this.URLsDownload);

            if ( this.URLsFailed.Any() ) 
                await this.RetryDownload();

            Console.WriteLine("Downloads concluídos!");
        }
        private async Task RetryDownload()
        {
            Console.WriteLine(@"Baixando episódios que falharam...\nEm caso de error 'Gone', feche e abra novamente o BADownloader");

            await this.ManageDownloader(this.URLsFailed);
        }
        private async Task ManageDownloader(List<string> list)
        {
            int length = list.Count;
            List<Task> tasks = new();

            for (int i = 0; i < length;)
            {
                for (int j = 0; j < this.TaskDownload; j++)
                {
                    if ( list.ElementAtOrDefault(i) is not null )
                    {
                        tasks.Add(DownloadAsync(i));
                        i++;
                    }
                    else
                    {
                        continue;
                    }
                }

                await Task.WhenAll(tasks);
                tasks.Clear();
            }
        }

        public void Dispose()
        {
            this.URLsDownload.Clear();
            this.URLsFailed.Clear();
            Client.Dispose();
        }
    }

}