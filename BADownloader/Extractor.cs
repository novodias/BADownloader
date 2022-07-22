using System.Text;
using System.Text.RegularExpressions;

namespace BADownloader
{
    public class Extractor : IExtractor
    {
        private string? _name;
        private ACollection? _aCollection;
        private string? _url;

        public string Name 
        { 
            get 
            {
                if ( this._name is not null )
                    return _name;

                return "DESCONHECIDO";
            }
            set 
            {
                string chars = Regex.Escape(@"<>:" + "\"" + "/|?*");
                string pattern = "[" + chars + "]";
                string tempName = Regex.Replace( value, pattern, "" );
                _name = tempName; 
            }
        }

        public ACollection ACollection 
        { 
            get 
            {
                if ( this._aCollection is not null )
                    return _aCollection;
                throw new Exception("AnimeDict null");
            }
            set { _aCollection = value; }
        }

        public string BaseURL 
        { 
            get 
            {
                if ( this._url is not null )
                    return this._url;
                return "SEM URL";
            }
            set { _url = value; }
        }

        public int Start { get; private set; } = -1;

        /// <summary>
        /// Retorna o index da variável Start. Se não encontrar, retornará 0.
        /// </summary>
        public int Index
        {
            get 
            {
                if ( this.ACollection.TryGetIndexFromEpisodeNumber(Start, out int index) )
                    return index;

                // Retorna 0.
                return index;
            }
        }

        public int Total { get; protected set; }
        public Quality Quality { get; protected set; } = Quality.NULL;

        /// <summary>
        /// Classe base dos sites sem paramêtros
        /// </summary>
        internal Extractor() {}

        /// <summary>
        /// Classe base dos sites
        /// </summary>
        /// <param name="name">Nome do anime</param>
        /// <param name="links">(Key) Número do episódio e (Value) Link do episódio</param>
        /// <param name="url">URL do anime</param>
        /// <param name="startcount">Episódio que usuário escolheu</param>
        internal Extractor(string name, ACollection ACollection, string url)
        {
            this.Name = name;
            this.ACollection = ACollection;
            this.BaseURL = url;
        }

        public bool TrySetStart(int number)
        {
            if ( this.ACollection.TryGetAnimeInfoWithEpisodeNumber(number, out var info) )
            {
                this.Start = info.Number;
                return true;
            }
            
            return false;
        }

        public void SetOptionQuality(Quality quality) => this.Quality = quality;

        // private static void Log(Logging logenum, ReadOnlySpan<char> log) => MinimalLogger.Log<Extractor>(logenum, log);

        public virtual void WriteDebug()
        {
            // Log(Logging.Information, $"Anime: {this.Name}");

            // if ( this.ACollection is null )
            // {
            //     Log(Logging.Error, "Episodes é null");
            //     throw new ArgumentNullException("AnimeDict");
            // }

            // var episodesCollection = new StringBuilder("Episodes: ")
            //     .AppendJoin(", ", ACollection.EpisodesCollection)
            //     .ToString();

            // Log(Logging.Information, episodesCollection);
            // Log(Logging.Information, $"BaseURL: {this.BaseURL}");
            // Log(Logging.Information, $"Start: {this.Start}");
            // Log(Logging.Information, $"Index: {this.Index}");
        }

        /// <summary>
        /// Pega a source link do episódio do anime, geralmente por requests ou web scraping.
        /// NÃO USE O "base.GetSourceLink"!
        /// </summary>
        /// <param name="episodeURL">Link do site/episódio</param>
        /// <returns>String do source link do episódio</returns>
        /// <exception cref="Exception"></exception>
        public virtual Task<string> GetSourceLink(string episodeURL) => throw new Exception("GetSourceLink vazio!");

        /// <summary>
        /// Verifica se a pasta Animes existe, e então a pasta do anime e em seguida se há episódios baixados.
        /// </summary>
        /// <param name="name">Nome do anime</param>
        /// <param name="episodes">Array com os episódios</param>
        /// <param name="animedict">(Key) Número do episódio e (Value) Link do episódio</param>
        /// <returns>AnimeDictionary com dicionário dos animes baixados</returns>
        protected static ACollection SearchInAnimeFolder(string name, ACollection ACollection)
        {
            // Checa a pasta do anime e o conteúdo dentro
            if (AnimesData.CheckUserFolder(name))
            {
                // Pega os episódios da pasta e então remove do array os episódios que já estão baixados
                var remainingEpisodes = GetRemainingEpisodes(name, ACollection);

                ACollection.SetCollection(ACollection.InfoCollection
                    .Where(ctx => remainingEpisodes.Any(num => num == ctx.Number))
                    .ToList());
            }

            return ACollection;
        }

        private static IEnumerable<int> GetRemainingEpisodes(string name, ACollection animedict)
        {
            int firstEpisodeNumber = animedict.First().Number;
            int count = animedict.Count;

            var remainingEpisodes = GetCollectionOfDownloadedEpisodes(name);
            remainingEpisodes = GetEpisodesToBeDownloaded(remainingEpisodes, firstEpisodeNumber, count);

            return remainingEpisodes;
        }

        /// <summary>
        /// Cria um array com os episódios baixados do anime.
        /// </summary>
        /// <param name="animename"></param>
        /// <returns>Array dos episódios baixados</returns>
        private static IEnumerable<int> GetCollectionOfDownloadedEpisodes(string animename)
        {
            DirectoryInfo AnimeDir = new( Path.Combine(AnimesData.Directory, animename) );
            int Length = AnimeDir.GetFiles().Length;

            int[] epi = new int[Length];

            for (int i = 0; i < Length; i++)
            {
                string name = AnimeDir.GetFiles().ElementAt(i).Name;

                epi[i] = AnimesData.GetEpisodeParsed(name);
            }

            return epi;
        }

        /// <summary>
        /// Cria um novo array com os episódios que falta baixar.
        /// </summary>
        /// <param name="episodes">Array dos episódios</param>
        /// <param name="startepisode">Episódio que o usuário escolheu</param>
        /// <param name="animelength">Tamanho do anime</param>
        /// <returns>Array com episódios restantes</returns>
        private static IEnumerable<int> GetEpisodesToBeDownloaded(IEnumerable<int> episodes, int startepisode, int animelength)
        {
            int[] episodes_all = new int[animelength];
            int x = startepisode;

            for (int i = 0; i < animelength; i++)
            {
                episodes_all[i] = x;
                x++;
            }

            return episodes_all.Except(episodes).ToArray();
        }
    }

    public enum Quality
    {
        NULL = 0,
        SD = 1,
        HD = 2,
        FHD = 3,
        AUTO = 4,
    }
}