namespace BADownloader
{
    public class ACollection
    {
        private List<AnimeInfo> _aCollection;
        public IReadOnlyCollection<AnimeInfo> InfoCollection { get => this._aCollection; }
        public IReadOnlyCollection<int> EpisodesCollection { get => this.GetAllEpisodesNumbersAsCollection(); }
        public int Count { get => this._aCollection.Count; }

        public ACollection()
        {
            this._aCollection = new();
        }

        public ACollection(int capacity)
        {
            this._aCollection = new(capacity);
        }

        public void SetCollection(List<AnimeInfo> animeDictionary) => this._aCollection = animeDictionary;
        
        public bool TryAdd(AnimeInfo info)
        {
            if ( this._aCollection.Contains(info) ) return false;

            this._aCollection.Add(info);
            return true;
        }

        public bool Remove(int index) => this._aCollection.Remove(this._aCollection[index]);
        public bool Remove(AnimeInfo info) => this._aCollection.Remove(info);

        public AnimeInfo GetAnimeInfo(int index) => this._aCollection[index];
        public AnimeInfo First() => this._aCollection.FirstOrDefault();
        public AnimeInfo Last() => this._aCollection.LastOrDefault();
        public bool Contains(AnimeInfo info) => this._aCollection.Contains(info);

        public bool TryGetAnimeInfoWithEpisodeNumber(int number, out AnimeInfo animeInfo)
        {
            foreach (var info in this._aCollection)
            {
                if (info.Number.Equals(number))
                {
                    animeInfo = info;
                    return true;
                }
            }

            animeInfo = default;
            return false;
        }

        public bool TryGetIndexFromEpisodeNumber(int number, out int index)
        {
            index = 0;
            foreach (var num in this.EpisodesCollection)
            {
                if (num.Equals(number))
                {
                    return true;
                }
                index++;
            }

            index = 0;
            return false;
        }

        public bool RemoveFromEpisodeValue(int number) 
        {
            foreach (var info in this._aCollection)
            {
                if (info.Number.Equals(number))
                {
                    bool remove = this._aCollection.Remove(info);
                    return remove;
                }
            }

            return false;
        }

        private IReadOnlyCollection<int> GetAllEpisodesNumbersAsCollection()
        {
            int[] numbers = new int[this.Count];

            int current_index = 0;
            foreach (var info in this.InfoCollection)
            {
                numbers[current_index++] = info.Number;
            }

            return numbers;
        }
    }

    public readonly struct AnimeInfo
    {
        public string Name { get; init; }
        public int Number { get; init; }
        public string URLDownload { get; init; }

        public override string ToString()
        {
            return $"Name: {Name} | Number: {Number} | URLDownload: {URLDownload}";
        }
    }
}
