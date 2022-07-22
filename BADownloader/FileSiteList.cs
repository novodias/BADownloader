namespace BADownloader
{
    public static class AvailableSites
    {
        private readonly static Dictionary<string, Website> WebsiteDict = new()
        {
            { "https://betteranime.net/", Website.BetterAnime },
            { "animeyabu", Website.AnimeYabu },
        };

        public static Website GetSite(string websiteStr) => WebsiteDict.Single( ctx => websiteStr.StartsWith(ctx.Key) ).Value;

        public static bool Contains(string website) => WebsiteDict.Keys.Any( ctx => website.Contains(ctx) );

    }

    public enum Website
    {
        BetterAnime,
        AnimeYabu,
    }
}