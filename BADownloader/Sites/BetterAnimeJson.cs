using Newtonsoft.Json;

namespace BADownloader.Sites
{
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