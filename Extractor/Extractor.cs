namespace BADownloader.Extractor
{
    public class Extractor : IExtractor
    {
        private string? _name;
        private Dictionary<int, string>? _linkdownloads;
        private int[]? _episodes;
        private string? _url;
        private int _startcount = 0;

        public string Name 
        { 
            get 
            {
                if ( this._name is not null )
                    return _name;
                return "DESCONHECIDO";
            }
            set { _name = value; }
        }
        public Dictionary<int, string> LinkDownloads 
        { 
            get 
            {
                if ( this._linkdownloads is not null )
                    return _linkdownloads;
                throw new Exception("LinkDownloads null");
            }
            set { _linkdownloads = value; }
        }
        public int[] Episodes 
        { 
            get 
            {
                if ( this._episodes is not null )
                    return this._episodes;
                throw new Exception("Episodes null");
            }
            set { _episodes = value; }
        }
        public string URL 
        { 
            get 
            {
                if ( this._url is not null )
                    return this._url;
                return "SEM URL";
            }
            set { _url = value; }
        }
        public int StartCount 
        {
            get { return this._startcount; }
            set { this._startcount = value; }
        }
        public int AnimeLength 
        { 
            get 
            { 
                if ( this.LinkDownloads is not null )
                    return this.LinkDownloads.Count;
                throw new Exception("LinkDownloads é null");
            }
        }
        public int LastEpisode 
        { 
            get 
            { 
                if ( this.LinkDownloads is not null )
                    return this.LinkDownloads.Last().Key; 
                throw new Exception("LinkDownloads é null");
            } 
        }
        public int Index 
        { 
            get 
            { 
                if ( this.Episodes is not null )
                    return Array.FindIndex(this.Episodes, num => num == this.StartCount); 
                throw new Exception("Episodes é null");
            }
        }

        public Extractor() 
        {

        }

        public Extractor(string name, Dictionary<int, string> links, int[] episodes, string url, int startcount)
        {
            this.Name = name;
            this.LinkDownloads = links;
            this.Episodes = episodes;
            this.URL = url;
            this.StartCount = startcount;
        }

        public virtual void WriteDebug()
        {
            Console.WriteLine($"Anime:{this.Name}");
            
            if ( this.Episodes is null ) 
                throw new Exception("Episodes é null");
            
            Console.Write($"Episodes: ");
            foreach ( var num in this.Episodes )
            {
                if ( num == this.Episodes[0] )
                    Console.Write(num);
                else
                    Console.Write(", " + num);
            }
            
            Console.WriteLine("\nLast Episode: " + this.LastEpisode);
            Console.WriteLine("URL:" + this.URL);
            Console.WriteLine("Startcount: " + this.StartCount );
            Console.WriteLine("Index: " + this.Index);
        }

        public virtual Task<string> GetSourceLink(string episodeURL) => throw new Exception("GetSourceLink vazio!");
    }
}