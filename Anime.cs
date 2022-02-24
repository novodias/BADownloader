namespace BADownloader
{
    public class Anime
    {
        public string Name { get; }
        public int[] Episodes { get; }
        public int Episodes_Length { get; }
        public string URL { get; }
        public int StartCount { get; }
        public int Index { get; }
        public string Quality { get; }

        public Anime(string name, int[] episodes, int episodes_length, string url, int startcount, string quality)
        {
            this.Name = name;
            this.Episodes = episodes;
            this.Episodes_Length = episodes_length;
            this.URL = url;
            this.StartCount = startcount;
            this.Index = this.StartCount - 1;
            this.Quality = quality;
        }
    }
}