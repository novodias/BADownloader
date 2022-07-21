namespace BADownloader
{
    public class AvailableSites
    {
        public readonly static Dictionary<string, Website> DictList = new()
        {
            { "https://www.betteranime.net", Website.BetterAnime },
            { "https://betteranime.net", Website.BetterAnime },
            { "animeyabu", Website.AnimeYabu },
        };

        public static Website GetSite( string websiteStr )
        {
            return DictList.Single( ctx => websiteStr.StartsWith(ctx.Key) ).Value;
        }

    }

    public enum Website
    {
        BetterAnime,
        AnimeYabu,
    }
}