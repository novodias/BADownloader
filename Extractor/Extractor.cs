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

        /// <summary>
        /// Classe base dos sites sem paramêtros
        /// </summary>
        public Extractor() 
        {

        }

        /// <summary>
        /// Classe base dos sites
        /// </summary>
        /// <param name="name">Nome do anime</param>
        /// <param name="links">(Key) Número do episódio e (Value) Link do episódio</param>
        /// <param name="episodes">Array com os episódios</param>
        /// <param name="url">URL do anime</param>
        /// <param name="startcount">Episódio que usuário escolheu</param>
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
            string debug = $"[{nameof(WriteDebug)}] ";
            Console.WriteLine( debug + $"Anime:{this.Name}");
            
            if ( this.Episodes is null ) 
                throw new Exception("Episodes é null");
            
            Console.Write(debug + $"Episodes: ");
            string final = string.Empty;
            foreach ( var num in this.Episodes )
            {
                if ( num == this.Episodes[0] )
                    final = num.ToString();
                else
                    final += ", " + num.ToString();
            }
            Console.Write(final + "\n");

            Console.WriteLine( debug + "Last Episode: " + this.LastEpisode);
            Console.WriteLine( debug + "URL:" + this.URL);
            Console.WriteLine( debug + "Startcount: " + this.StartCount );
            Console.WriteLine( debug + "Index: " + this.Index);
        }

        /// <summary>
        /// Pega a source link do episódio do anime, geralmente por requests ou web scraping
        /// </summary>
        /// <param name="episodeURL">Link do site/episódio</param>
        /// <returns>String do source link do episódio</returns>
        /// <exception cref="Exception"></exception>
        public virtual Task<string> GetSourceLink(string episodeURL) => throw new Exception("GetSourceLink vazio!");

        /// <summary>
        /// Verifica se a pasta Animes existe, e então a pasta do anime e em seguida se há episódios baixados.
        /// </summary>
        /// <param name="Name">Nome do anime</param>
        /// <param name="episodes">Array com os episódios</param>
        /// <param name="links">(Key) Número do episódio e (Value) Link do episódio</param>
        protected static void CheckAnimeFolder(string Name, ref int[] episodes, ref Dictionary<int, string> links)
        {
            if ( AnimesData.CheckUserFolder( Name ) )
            {
                episodes = AnimesData.ExistingEpisodes( Name );
                episodes = AnimesData.OtherEpisodes( episodes, links.ElementAt(0).Key, links.Count );

                string episodesString = string.Empty;
                foreach ( var i in episodes )
                {
                    if ( episodesString.Equals( string.Empty ) )
                        episodesString = $"Episódio(s) faltando: {i}";
                    else
                        episodesString += $", {i}";
                }
                Console.WriteLine( episodesString );

                Dictionary<int, string> temporary = new();
                for ( int i = 0; i < episodes.Length; i++ )
                {
                    int episode_index_value = episodes[i];
                    temporary.Add( episodes[i], links.Single( ctx => ctx.Key == episode_index_value ).Value );
                }
                links = temporary;
            }
            else
            {
                int index = 0;
                foreach ( var key in links.Keys )
                {
                    episodes[index] = key;
                    index++;
                }
            }
        }
    }
}