namespace BADownloader.Extractor
{
    public class AvailableSites
    {
        public readonly static Dictionary<string, SiteEnum> DictList = new()
        {
            { "betteranime", SiteEnum.BetterAnime },
            { "animeyabu", SiteEnum.AnimeYabu },
        };

        public static SiteEnum GetSite( string websiteStr )
        {
            return DictList.Single( ctx => websiteStr.Contains(ctx.Key) ).Value;
        }
    }

    public enum SiteEnum
    {
        BetterAnime,
        AnimeYabu,
    }
}