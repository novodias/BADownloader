namespace BADownloader
{
    public class Anime
    {
        public string Name { get; }
        public Dictionary<int, string> LinkDownloads { get; }
        public int[] Episodes { get; }
        public int Episodes_Length { get; }
        public string URL { get; }
        public int StartCount { get; }
        public int Index { get; }
        public string Quality { get; }

        public Anime(string name, Dictionary<int, string> links, int[] episodes, int episodes_length, string url, int startcount, string quality)
        {
            this.Name = name;
            this.LinkDownloads = links;
            this.Episodes = episodes;
            this.Episodes_Length = episodes_length;
            this.URL = url;
            this.StartCount = startcount;
            this.Index = Array.FindIndex(this.Episodes, x => x == startcount);
            this.Quality = quality;
        }

        public void WriteInfo()
        {
            Console.WriteLine($"Anime:{this.Name}");
            Console.Write($"Episodes: ");
            foreach (var num in this.Episodes)
            {
                if ( num == this.Episodes[0] )
                    Console.Write(num);
                else
                    Console.Write(", " + num);
            }
            Console.WriteLine("Episodes Length: " + this.Episodes_Length);
            Console.WriteLine("URL:" + this.URL);
            Console.WriteLine("Startcount: " + this.StartCount );
            Console.WriteLine("Index: " + this.Index);
            Console.WriteLine("Quality: " + this.Quality);
        }
    }
}