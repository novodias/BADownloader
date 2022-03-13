using HtmlAgilityPack;

namespace BADownloader.Sites
{
    public interface IAnimeInfo
    {
        public Task<Anime> GetAnimeAsync(string url, HtmlWeb web);
    }
}