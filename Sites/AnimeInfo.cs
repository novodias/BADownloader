using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Spectre.Console;

namespace BADownloader.Sites
{
    public class AnimeInfo
    {
        protected static int EpisodeInput(int episodes_length, int[] episodes)
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

        protected static bool CheckExistingFolder(string animename)
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

        protected static int GetEpisodeParsed(string filename)
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

        protected static int[] ExistingEpisodes(string animename)
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

        protected static int[] OtherEpisodes(int[] episodes, int startepisode, int animelength)
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