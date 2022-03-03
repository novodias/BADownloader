using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Spectre.Console;

namespace BADownloader
{
    public class AnimeInfo
    {
        public async static Task<List<object>> GetAnimeInfo(string url, HtmlWeb web)
        {
            var doc = await web.LoadFromWebAsync(url);
            List<object> info = new();
            
            string animename = doc.DocumentNode.SelectSingleNode("//*[@id='page-content']/main/div[1]/div/h2").InnerText ?? 
                throw new Exception("Não foi encontrado o nome do anime, página não carregou ou o site caiu!");
            info.Add(animename); 

            var episodesdictionary = GetEpisodesURL(doc);

            var episodes_length = episodesdictionary.Count;
            info.Add(episodes_length);

            info.Add(episodesdictionary);

            int[] episodes = new int[episodes_length];
            for (int i = 0; i < episodes_length; i++)
            {
                episodes[i] = episodesdictionary.ElementAt(i).Key;
            }
            info.Add(episodes);

            // string episodes_length = doc.DocumentNode.SelectSingleNode("//*[@id='page-content']/main/div[1]/div/p[4]/span").InnerText ??
            //     throw new Exception("Não foi encontrado o total de episódios, a página não carregou ou o site caiu!");

            // var genresnodes = doc.DocumentNode.SelectSingleNode("//*[@id='page-content']/main/div[1]/div/div[@class='anime-genres']").Descendants();
            // string genres = string.Empty;

            // foreach (var item in genresnodes)
            // {
            //     if ( string.IsNullOrEmpty(genres) )
            //     {
            //         genres = $"[green bold]{item.InnerText.Replace(" ", "").Replace("\n", "")}[/]";
            //     }
            //     else
            //     {
            //         genres += $", [green bold]{item.InnerText.Replace(" ", "").Replace("\n", "")}[/]";
            //     }
            // }

            string genres = GetGenres(doc);
            info.Add(genres);

            if ( doc == null || string.IsNullOrEmpty(animename) )
                throw new Exception("Não foi encontrado a página ou não tem informações suficientes disponíveis");
            else
            {
                return info;
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

        public static int EpisodeInput(int episodes_length, int[] episodes)
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

        public static string QualityInput()
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

        public static bool CheckExistingFolder(string animename)
        {
            DirectoryInfo dir = new("Animes/");

            foreach (var item in dir.GetDirectories())
            {
                if ( item.Name == animename )
                {
                    return true;
                }
            }

            return false;
        }

        private static int GetEpisodeParsed(string filename)
        {
            int frnumber = filename.LastIndexOf('-') + 1;
            int scnumber = frnumber + 1;
            int thnumber = scnumber + 1;
            int qtnumber = thnumber + 1;
            string number;

            int one = int.Parse(filename.Substring(frnumber, 1));

            if (!int.TryParse(filename.AsSpan(scnumber, 1), out int two))
            {
                return one;
            }

            number = string.Concat(one, two);

            if (!int.TryParse(filename.AsSpan(thnumber, 1), out int thr))
            {
                return int.Parse(number);
            }

            number = string.Concat(one, two, thr);

            if (!int.TryParse(filename.AsSpan(qtnumber, 1), out int qtr))
            {
                return int.Parse(number);
            }

            number = string.Concat(one, two, thr, qtr);

            return int.Parse(number);
        }

        public static int[] ExistingEpisodes(string animename)
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

        public static int[] OtherEpisodes(int[] episodes, int animelength)
        {
            int[] episodes_all = new int[animelength];
            int x = 0;

            for (int i = 0; i < animelength; i++)
            {
                episodes_all[i] = x + 1;
                x++;
            }

            return episodes_all.Except(episodes).ToArray();
        }
    }
}