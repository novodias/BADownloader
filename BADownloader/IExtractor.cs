namespace BADownloader
{
    public interface IExtractor
    {
        string Name { get; }
        AnimeCollection AnimeCollection { get; }
        string BaseURL { get; }
        int Total { get; }
        int Start { get; }
        int Index { get; }
        void WriteDebug();
        Task<string> GetSourceLink(string episodeURL);
        bool TrySetStart(int number);
        void SetOptionQuality(Quality quality);
    }
}