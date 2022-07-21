namespace BADownloader
{
    public class AnimesData
    {
        private static readonly DirectoryInfo UserDir = GetUserDirectory();
        public static string Directory { get => UserDir.FullName; }

        private static DirectoryInfo GetUserDirectory()
        {
            var dirInfo = new DirectoryInfo(Environment.CurrentDirectory);

            var files = dirInfo.GetFiles();

            bool found = false;
            foreach (var file in files)
            {
                if ( file.Name.Equals("diretorio.txt") )
                    found = true;
            }

            if (found)
            {
                var dirStr = File.ReadAllText("diretorio.txt").Trim();
                var dir = new DirectoryInfo(dirStr);
                
                if (!dir.Exists)
                    dir.Create();

                return dir;
            }
            
            return new("Animes/");
        }

        /// <summary>
        /// Método input, o usuário insere o episódio de onde quer começar a baixar.
        /// </summary>
        /// <param name="episodes_length">Tamanho de episódios do anime</param>
        /// <param name="episodes">Array dos episódios</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static int EpisodeInput(int episodes_length, IEnumerable<int> episodes)
        {
            string str;

            Console.WriteLine("Alguns animes começam no episódio 00\nDigite de qual episódio você quer começar baixar: ");
            str = Console.ReadLine() ?? string.Empty;

            if (!int.TryParse(str, out int input))
            {
                Console.WriteLine("Isso não é um número!");
                return EpisodeInput(episodes_length, episodes);
            }
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

        public static bool CheckUserFolder(string animename)
        {
            if ( !UserDir.Exists ) 
            {
                UserDir.Create();
                UserDir.CreateSubdirectory(animename);
                return false;
            }

            foreach ( var item in UserDir.GetDirectories() )
            {
                if ( item.Name == animename )
                {
                    var files = item.GetFiles();
                    if ( files.Length > 0 )
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }                
                }
            }

            UserDir.CreateSubdirectory(animename);
            return false;
        }

        /// <summary>
        /// Pega o nome do arquivo, e pega o número do episódio
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>Número do episódio</returns>
        public static int GetEpisodeParsed(ReadOnlySpan<char> filename)
        {
            int one = filename.LastIndexOf('-') + 1;

            var number = GetNumber(filename[one..]);

            // string numberconcat = string.Empty;
            // for (int i = 0; i < 4; i++)
            // {


            //     if ( (one + i) > filename.Length || !int.TryParse(filename[one + i].ToString(), out int num))
            //         break;

            //     // if (!int.TryParse(filename.AsSpan(one + i, 1), out int num))
            //     //     break;

            //     MinimalLogger.Log<AnimesData>(Logging.Debug, $"GetEpisodeParsed - for: {num}");
            //     numberconcat += num.ToString();
            // }

            return number;
        }

        private static int GetNumber(ReadOnlySpan<char> name)
        {
            string numberString = string.Empty;
            
            foreach (var c in name)
            {
                if (Char.IsDigit(c))
                    numberString += c;
            }

            return int.Parse(numberString);
        }
    }
}
