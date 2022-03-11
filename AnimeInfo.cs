using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Spectre.Console;

namespace BADownloader
{
    public class AnimeInfo
    {
        public async static Task<Anime> GetAnimeInfo(string url, HtmlWeb web)
        {
            var doc = await web.LoadFromWebAsync(url);
            
            string animename = doc.DocumentNode.SelectSingleNode("//*[@id='page-content']/main/div[1]/div/h2").InnerText ?? 
                throw new Exception("Não foi encontrado o nome do anime, página não carregou ou o site caiu!");

            var episodesdictionary = GetEpisodesURL(doc);

            var animelength = episodesdictionary.Count;

            int[] episodes = new int[animelength];
            for (int i = 0; i < animelength; i++)
            {
                episodes[i] = episodesdictionary.ElementAt(i).Key;
            }

            string genres = GetGenres(doc);

            string chars = Regex.Escape(@"<>:" + "\"" + "/|?*");
            string pattern = "[" + chars + "]";
            animename = Regex.Replace(animename, pattern, "");

            AnsiConsole.Write(new Markup(string.Format("Anime: [green bold]{0}[/]\nNúmero de episódios: [green bold]{1}[/]\n", animename, episodesdictionary.Last().Key)));
            AnsiConsole.Write(new Markup(string.Format($"Gêneros: {genres}\n")));

            if (CheckExistingFolder(animename))
            {
                episodes = ExistingEpisodes(animename);
                episodes = OtherEpisodes(episodes, episodesdictionary.ElementAt(0).Key, animelength);

                string strepisodes = string.Empty;
                foreach (var i in episodes)
                {
                    if (strepisodes == string.Empty)
                        strepisodes = $"Episódio(s) faltando: {i}";
                    else
                        strepisodes += $", {i}";
                }
                Console.WriteLine(strepisodes);

                Dictionary<int, string> temporary = new();
                for (int i = 0; i < episodes.Length; i++)
                {
                    temporary.Add(episodes[i], episodesdictionary.Single(ctx => ctx.Key == episodes[i]).Value);
                }
                episodesdictionary = temporary;
            }
            else
            {
                for (int i = 0; i < animelength; i++)
                {
                    episodes[i] = i + 1;
                }
            }

            int startpoint = EpisodeInput(animelength, episodes);
            string quality = QualityInput();

            Anime animeinfo = new(animename, episodesdictionary, episodes, url, startpoint, quality, genres, animelength);

            if ( doc == null || string.IsNullOrEmpty(animename) )
                throw new Exception("Não foi encontrado a página ou não tem informações suficientes disponíveis");
            else
            {
                return animeinfo;
            }
        }

        private static string GetGenres(HtmlDocument doc)
        {
            var genresnodes = doc.DocumentNode.SelectSingleNode("//*[@id='page-content']/main/div[1]/div/div[@class='anime-genres']").Descendants();

            List<string> genres = new();
            // O primeiro node é bugadasso
            int i = 0;
            foreach (var node in genresnodes)
            {
                if (i > 0 && !genres.Contains(node.InnerText.Trim()) && node.InnerText != " " && node.InnerText != "\n")
                {
                    string gen = Regex.Replace(node.InnerText, @"[^0-9a-zA-Z\p{L}]+", "");
                    if (string.IsNullOrEmpty(gen)) continue;
                    if (!genres.Contains(gen)) genres.Add(gen);
                }
                i++;
            }

            string strgenres = string.Empty;
            foreach (var gen in genres)
            {
                if (strgenres == string.Empty)
                    strgenres = $"[green bold]{gen}[/]";
                else
                    strgenres += $", [green bold]{gen}[/]";
            }

            return strgenres;
        }

        private static Dictionary<int, string> GetEpisodesURL(HtmlDocument doc)
        {
            var episodeshtml = doc.DocumentNode.SelectSingleNode("//*[@id='episodesList']");

            List<string> urls = new();
            foreach (var node in episodeshtml.Descendants())
            {
                if (node.GetAttributeValue("class", "") != "list-group-item" 
                && node.GetAttributeValue("href", "") != default 
                && node.GetAttributeValue("href", "").Contains("/download"))
                {
                    urls.Add(node.GetAttributeValue("href", ""));
                }
            }

            Dictionary<int, string> eps = new();
            foreach (var url in urls)
            {
                int num = GetEpisodeParsed(url);

                eps.Add(num, url);
            }

            return eps;
        }

        private static int EpisodeInput(int episodes_length, int[] episodes)
        {
            var str = AnsiConsole.Ask<string>("Alguns animes começam no episódio 00\nDigite de qual episódio você quer começar baixar: ");

            if (!int.TryParse(str, out int input))
                throw new Exception("Isso não é um número!");
            else
            {
                if (!episodes.Any(x => x == input))
                {
                    if (input < 0) input = 0;
                    else if (input > episodes_length) input = episodes_length;
                }
                return input;
            }
        }

        public static int DownloadInput()
        {
            var str = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title("Quantos downloads deseja ter? [green]1[/], [green]2[/], [green]3[/], [green]4[/] ou [green]5[/]\nRecomendado um PC bom suficiente para baixar 5/4/3/2 arquivos simultâneamente")
                .PageSize(10)
                .AddChoices(new [] 
                {
                    "1", "2", "3", "4", "5"
                }));

            if (!int.TryParse(str, out int input))
                throw new Exception("Isso não é um número!");
            else
            {
                if (input < 1) input = 1;
                else if (input > 5) input = 5;
                return input;
            }
        }

        private static string QualityInput()
        {
            var str = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title("\nSelecione a qualidade de vídeo preferida.\nOBS: Nem todas as qualidades estarão disponíveis dependendo do anime.\nCada episódio pode variar de [green]~100mb[/] à [yellow]~1gb[/] dependendo da qualidade\n[yellow underline]Verifique se seu disco contém espaço suficiente![/]")
                .PageSize(5)
                .AddChoices(new []
                {
                    "SD", "HD", "FULL HD"
                }));

            switch (str)
            {
                case "SD":
                return "//*[@id='page-content']/div[2]/section/div[2]/div[1]/div/a[1]";

                case "HD":
                return "//*[@id='page-content']/div[2]/section/div[2]/div[1]/div/a[2]";

                case "FULL HD":
                return "//*[@id='page-content']/div[2]/section/div[2]/div[1]/div/a[3]";

                default:
                    Console.WriteLine("??? selecionei o sd pra vc");
                return "//*[@id='page-content']/div[2]/section/div[2]/div[1]/div/a[1]";
            }

        }

        private static bool CheckExistingFolder(string animename)
        {
            DirectoryInfo dir = new("Animes/");

            foreach (var item in dir.GetDirectories())
            {
                if ( item.Name == animename )
                {
                    var files = item.GetFiles();
                    if (files is not null) return true;
                }
            }

            return false;
        }

        private static int GetEpisodeParsed(string filename)
        {
            int one = filename.LastIndexOf('-') + 1;

            int[] numbers = new int[4] 
            {
                one, one + 1, one + 2, one + 3
            };

            string numberconcat = string.Empty;

            for (int i = 0; i < 4; i++)
            {
                if (!int.TryParse(filename.AsSpan(numbers[i], 1), out int num))
                {
                    break;
                }
                
                numberconcat += num.ToString();
            }

            return int.Parse(numberconcat);
        }

        private static int[] ExistingEpisodes(string animename)
        {
            DirectoryInfo dir = new($"Animes/{animename}");
            int length = dir.GetFiles().Length;

            int[] epi = new int[length];

            for (int i = 0; i < length; i++)
            {
                string name = dir.GetFiles().ElementAt(i).Name;

                epi[i] = GetEpisodeParsed(name);
            }

            return epi;
        }

        private static int[] OtherEpisodes(int[] episodes, int startepisode, int animelength)
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
}