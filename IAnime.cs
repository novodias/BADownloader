using HtmlAgilityPack;

namespace BADownloader
{
    public interface IAnime
    {
        public string Name { get; }
        public Dictionary<int, string> LinkDownloads { get; }
        public int[] Episodes { get; }
        public int AnimeLength { get; }
        public int LastEpisode { get; }
        public string URL { get; }
        public int StartCount { get; }
        public int Index { get; }
        public void WriteDebug();
        public Task<string> GetSourceLink(string episodeURL);
    }
}