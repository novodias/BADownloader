using System.Net.Http.Headers;
using Newtonsoft.Json;
using HtmlAgilityPack;

namespace BADownloader
{
    public class DownloadManager
    {
        private static readonly HttpClient Client = new();
        private readonly HtmlWeb Web = new();
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

        private async Task<string> GetResponseURL(string downloadlink)
        {
            var doc = await this.Web.LoadFromWebAsync(downloadlink);

            downloadlink = doc.DocumentNode.SelectSingleNode(this.Anime.Quality).GetAttributeValue("href", "");

            doc = await this.Web.LoadFromWebAsync(downloadlink);
            var nextdatastr = doc.DocumentNode.SelectSingleNode("//*[@id='__NEXT_DATA__']").InnerText;
            var NextData = JsonConvert.DeserializeObject<NextData>(nextdatastr);

            var countlink = "https://download.betteranime.net/".Length;
            var datalink = downloadlink[countlink..];
            var path = "/_next/data/" + NextData.BuildId + "/" + datalink + ".json";

            var url = "https://download.betteranime.net" + path;

            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("path", path);
            request.Headers.Add("referer", downloadlink);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            request.Headers.Add("user-agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.4844.51 Safari/537.36");

            using var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var dl = JsonConvert.DeserializeObject<Props>(await response.Content.ReadAsStringAsync()).PageProps.Anime.URLDownload;

            return dl;
        }

        private async Task DownloadAsync(string url, int i)
        {
            string downloadURL = url;
            int animeindex = this.Anime.LinkDownloads.Keys.Single(x => x == this.Anime.Episodes[i]);

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

                Console.WriteLine($"Baixando episódio: [{animeindex}/{this.Anime.LastEpisode}] [{tamanho} mb]");

                var file = File.OpenWrite($"Animes/{this.Anime.Name}/{this.Anime.Name}-{animeindex}.mp4");

                await content.CopyToAsync(file);

                await content.DisposeAsync();
                await file.DisposeAsync();

                Console.WriteLine($"Download do episódio [{animeindex}/{this.Anime.LastEpisode}] concluído.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Download do episódio [{animeindex}] falhou! {ex.Message}");

                string epURL = string.Empty;
                if (animeindex < 10)
                    epURL = "0" + animeindex;

                string failedURL = this.Anime.URL + "/episodio-" + epURL + "/download";
                Console.WriteLine(failedURL);

                this.URLsFailed.Add(downloadURL);
            }
        }
        public async Task StartDownloadAsync()
        {
            Console.WriteLine("\nComeçando download.");

            await this.ManageDownloader();

            Console.WriteLine($"Downloads concluídos!");

            await this.RetryDownload();
        }

        private async Task RetryDownload()
        {
            if ( !(this.URLsFailed.Count > 0) )
                return;

            Console.Write("Deseja baixar os episódios que falharam? [Y/n]: ");
            char key = Console.ReadKey().KeyChar;
            bool bkey = key == 'Y' | key == 'y' | key == '\0';

            if ( bkey )
            {
                Console.WriteLine("Baixando episódios que falharam..." + "\nEm caso de error 'Gone', feche e abra novamente o BADownloader");
                await this.RetryManageDownloader(this.URLsFailed);
            }
        }

        private async Task ManageDownloader()
        {
            List<Task> tasks = new();

            for (int i = this.Anime.Index; i < this.Anime.Episodes.Length;)
            {
                for (int j = 0; j < this.TaskDownload; j++)
                {
                    if ( i > this.Anime.LinkDownloads.Count )
                        break;

                    var downloadlink = this.Anime.LinkDownloads.ElementAt(i).Value;
                    tasks.Add(DownloadAsync(await this.GetResponseURL(downloadlink), i));
                    i++;
                }

                await Task.WhenAll(tasks);
                tasks.Clear();
            }
        }

        private async Task RetryManageDownloader(List<string> list)
        {
            List<Task> tasks = new();

            for (int i = 0; i < list.Count;)
            {
                for (int j = 0; j < this.TaskDownload; j++)
                {
                    if ( i > this.Anime.LinkDownloads.Count )
                        break;
                    
                    tasks.Add(DownloadAsync(await this.GetResponseURL(list.ElementAt(i)), i));
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

    public struct NextData
    {
        public NextData(Props props, string page, Query query, string buildid, bool nextexport, bool isfallback, bool gsp)
        {
            this.Props = props;
            this.Page = page;
            this.Query = query;
            this.BuildId = buildid;
            this.NextExport = nextexport;
            this.IsFallback = isfallback;
            this.GSP = gsp;
        }

        [JsonProperty("props")]
        public Props Props { get; private set; }

        [JsonProperty("page")]
        public string Page { get; private set; }

        [JsonProperty("query")]
        public Query Query { get; private set; }

        [JsonProperty("buildId")]
        public string BuildId { get; private set; }

        [JsonProperty("runtimeConfig")]
        public RuntimeConfig? RuntimeConfig { get; private set; } = null;

        [JsonProperty("nextExport")]
        public bool NextExport { get; private set; }

        [JsonProperty("isFallback")]
        public bool IsFallback { get; private set; }

        [JsonProperty("gsp")]
        public bool GSP { get; private set; }
    }

    public struct Props
    {
        [JsonProperty("pageProps")]
        public PageProps PageProps { get; private set; }

        [JsonProperty("__N_SSG")]
        public bool N_SSG { get; private set; }
    }

    public struct PageProps
    {
        [JsonProperty("anime")]
        public AnimeJson Anime { get; private set; }
    }

    public struct AnimeJson
    {
        [JsonProperty("nome")]
        public string Nome { get; private set; }

        [JsonProperty("path")]
        public string URLDownload { get; private set; }
    }

    public struct Query
    {
        [JsonProperty("download")]
        public string Download { get; set; }
    }

    public struct RuntimeConfig {}

}