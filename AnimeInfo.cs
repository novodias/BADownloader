using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Spectre.Console;

namespace BADownloader
{
    public class AnimeInfo
    {
        public async static Task<List<string>> GetAnimeInfo(string url, HtmlWeb web)
        {
            var doc = await web.LoadFromWebAsync(url);
            List<string> info = new();
            
            string animename = doc.DocumentNode.SelectSingleNode("//*[@id='page-content']/main/div[1]/div/h2").InnerText ?? 
                throw new Exception("Não foi encontrado o nome do anime, página não carregou ou o site caiu!");
            info.Add(animename); 

            string episodes_length = doc.DocumentNode.SelectSingleNode("//*[@id='page-content']/main/div[1]/div/p[4]/span").InnerText ??
                throw new Exception("Não foi encontrado o total de episódios, a página não carregou ou o site caiu!");
            info.Add(episodes_length);

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

            // info.Add(genres);

            if ( doc == null || string.IsNullOrEmpty(animename) || string.IsNullOrEmpty(episodes_length) )
                throw new Exception("Não foi encontrado a página ou não tem informações suficientes disponíveis");
            else
            {
                return info;
            }
        }

        public static int EpisodeInput(int episodes_length, int[] episodes)
        {
            var str = AnsiConsole.Ask<string>("Digite de qual episódio você quer começar baixar: ");

            if (!int.TryParse(str, out int input))
                throw new Exception("Isso não é um número!");
            else
            {
                if (!episodes.Any(x => x == input))
                {
                    if (input < 1) input = 1;
                    else if (input > episodes_length) input = episodes_length;
                    else input = episodes[0];
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

        // Não funciona com animes com mais de
        // 99 episódios!!!
        private static int GetEpisodeParsed(string filename)
        {
            int frnumber = filename.LastIndexOf('-') + 1;
            int scnumber = frnumber + 1;
            
            int one = int.Parse(filename.Substring(frnumber, 1));
            if ( int.TryParse(filename.AsSpan(scnumber, 1), out int two ))
            {
                string number = string.Concat(one, two);
                return int.Parse(number);
            }
            else
            {
                return one;
            }
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