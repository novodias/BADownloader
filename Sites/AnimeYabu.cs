using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Spectre.Console;

namespace BADownloader.Sites
{
    public class AnimeYabu : AnimeInfo, IAnimeInfo
    {
        public async Task<Anime> GetAnimeAsync(string url, HtmlWeb web)
        {
            var doc = await web.LoadFromWebAsync(url);

            string Name = doc.DocumentNode.SelectSingleNode("//*[@id='channel-content']/div[1]/div[2]/div[2]/h1").InnerText;

            var EpisodesDictionary = GetEpisodesURL(doc);
            var AnimeLength = EpisodesDictionary.Count;

            int[] Episodes = new int[AnimeLength];
            for (int i = 0; i < AnimeLength; i++)
            {
                Episodes[i] = EpisodesDictionary.ElementAt(i).Key;
            }

            string Genres = GetGenres(doc);

            string chars = Regex.Escape(@"<>:" + "\"" + "/|?*");
            string pattern = "[" + chars + "]";
            Name = Regex.Replace(Name, pattern, "");

            AnsiConsole.Write(new Markup(string.Format("Anime: [green bold]{0}[/]\nNúmero de episódios: [green bold]{1}[/]\n", Name, EpisodesDictionary.Last().Key)));
            AnsiConsole.Write(new Markup(string.Format($"{Genres}\n")));

            // Transformar isso em um método no AnimeInfo.
            if (CheckExistingFolder(Name))
            {
                Episodes = ExistingEpisodes(Name);
                Episodes = OtherEpisodes(Episodes, EpisodesDictionary.ElementAt(0).Key, AnimeLength);

                string strepisodes = string.Empty;
                foreach (var i in Episodes)
                {
                    if (strepisodes == string.Empty)
                        strepisodes = $"Episódio(s) faltando: {i}";
                    else
                        strepisodes += $", {i}";
                }
                Console.WriteLine(strepisodes);

                Dictionary<int, string> temporary = new();
                for (int i = 0; i < Episodes.Length; i++)
                {
                    temporary.Add(Episodes[i], EpisodesDictionary.Single(ctx => ctx.Key == Episodes[i]).Value);
                }
                EpisodesDictionary = temporary;
            }

            // TODO: 
            // Isso não faz sentido, usar o EpisodesDictionary 
            // ao invés de criar episódios do nada
            else
            {
                for (int i = 0; i < AnimeLength; i++)
                {
                    Episodes[i] = i + 1;
                }
            }

            int startpoint = EpisodeInput(AnimeLength, Episodes);
            
            // Ainda não implementado!
            // string quality = QualityInput();

            throw new Exception("AnimeYabu ainda não implementado!");

            // return new Anime( Name, EpisodesDictionary, Episodes, url, startpoint, "", Genres, AnimeLength );
        }

        private static IDictionary<int, string> GetEpisodesURL(HtmlDocument doc)
        {
            bool IsPaged = doc.DocumentNode.SelectSingleNode("//*[@id='channel-content']/div[3]/div[8]").HasChildNodes;

            if ( !IsPaged )
            {
                var ChildsNodes = doc.DocumentNode.SelectSingleNode("//*[@id='channel-content']/div[3]").Descendants();

                var VideosNodes = ChildsNodes.Where(node => node.HasClass("video"));

                Dictionary<int, string> EpisodesDictionary = new();
                foreach (var node in VideosNodes)
                {
                    var ClipLink = node.Descendants().Single(node => node.HasClass("video-thumb")).FirstChild;

                    string Link = ClipLink.GetAttributeValue("href", "");
                    string strNumber = ClipLink.GetAttributeValue("title", "");

                    int Number;
                    if ( !strNumber.Contains("(HD)"))
                        Number = int.Parse(strNumber[strNumber.LastIndexOf("Episódio ")..strNumber.Length].Trim());
                    else
                        Number = int.Parse(strNumber[strNumber.LastIndexOf("Episódio ")..strNumber.IndexOf(" (HD)")].Trim());

                    EpisodesDictionary.Add(Number, Link);
                }

                return EpisodesDictionary;
            }
            else
            {
                /*
                Pra ser implementado.

                Como funcionará?
                Usando a seguinte url: https://animeyabu.com/anime/one-piece/page/X
                One Piece possui atualmente 34 páginas, inserida 
                na página principal no 'div class=naco'.

                A ideia é pegar o número de páginas no 'naco'
                e usar um for loop e pegar os links dos episódios.
                */

                throw new Exception("Feature ainda não implementada com animes com mais de uma página");
            }
        }

        private static string GetGenres(HtmlDocument doc)
        {
            var GenresNode = doc.DocumentNode.SelectSingleNode("//*[@id='channel-content']/div[1]/div[2]/div[4]/b");

            return GenresNode.InnerText;
        }
    }
}