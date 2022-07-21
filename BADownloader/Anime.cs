namespace BADownloader
{
    public class AnimeCollection
    {
        private List<AnimeInfo> _animeCollection;
        public IReadOnlyCollection<AnimeInfo> InfoCollection { get => this._animeCollection; }
        public IReadOnlyCollection<int> EpisodesCollection { get => this.GetAllEpisodesNumbersAsCollection(); }
        public int Count { get => this._animeCollection.Count; }
        // private int _index = 0;

        public AnimeCollection()
        {
            this._animeCollection = new();
        }

        public AnimeCollection(int capacity)
        {
            this._animeCollection = new(capacity);
        }

        public void SetCollection(List<AnimeInfo> animeDictionary) => this._animeCollection = animeDictionary;
        
        public bool TryAdd(AnimeInfo info)
        {
            if ( this._animeCollection.Contains(info) ) return false;

            this._animeCollection.Add(info);
            return true;
        }

        public bool Remove(int index) => this._animeCollection.Remove(this._animeCollection[index]);
        public bool Remove(AnimeInfo info) => this._animeCollection.Remove(info);

        public AnimeInfo GetAnimeInfo(int index) => this._animeCollection[index];
        public AnimeInfo First() => this._animeCollection.FirstOrDefault();
        public AnimeInfo Last() => this._animeCollection.LastOrDefault();

        public bool TryGetAnimeInfoWithEpisodeNumber(int number, out AnimeInfo animeInfo)
        {
            foreach (var info in this._animeCollection)
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
            foreach (var info in this._animeCollection)
            {
                if (info.Number.Equals(number))
                {
                    bool remove = this._animeCollection.Remove(info);
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

        // public bool MoveNext(int amount, out AnimeInfo animeInfo)
        // {
        //     if ( amount <= 0 ) amount = 1;

        //     AnimeInfo[] animeInfos = new AnimeInfo[amount];

        //     for (int i = 0; i < amount; i++)
        //     {
        //         if ( this.Count < _index )
        //             break;

        //         animeInfos[i] = this._animeCollection[i];
        //         _index++;
        //     }
            
        //     if ( _index < this.Count )
        //     {
        //         animeInfo = this._animeCollection[_index++];
        //         return true;
        //     }

        //     animeInfo = default;
        //     return false;
        // }

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
