using Spectre.Console;

namespace BADownloader
{
    public class AnimesData
    {
        private static readonly DirectoryInfo UserDir = new("Animes/");

        /// <summary>
        /// Método input, o usuário insere o episódio de onde quer começar a baixar.
        /// </summary>
        /// <param name="episodes_length">Tamanho de episódios do anime</param>
        /// <param name="episodes">Array dos episódios</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static int EpisodeInput(int episodes_length, int[] episodes)
        {
            string str;
            if ( !Program.IsWindows7 )
                str = AnsiConsole.Ask<string>("Alguns animes começam no episódio 00\nDigite de qual episódio você quer começar baixar: ");
            else
            {
                Console.WriteLine("Alguns animes começam no episódio 00\nDigite de qual episódio você quer começar baixar: ");
                str = Console.ReadLine() ?? string.Empty;
            }

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
                if ( Program.IsDebugMode )
                    System.Console.WriteLine( "[" + nameof(CheckUserFolder) + "] UserDir.Exists: " + UserDir.Exists + "\nDiretórios criado." );
                    
                UserDir.Create();
                UserDir.CreateSubdirectory(animename);
                return false;
            }

            foreach ( var item in UserDir.GetDirectories() )
            {
                if ( item.Name == animename )
                {
                    if ( Program.IsDebugMode )
                        System.Console.WriteLine( "[" + nameof(CheckUserFolder) + "] Anime encontrado" );

                    var files = item.GetFiles();
                    if ( files is not null )
                    {
                        if ( Program.IsDebugMode )
                            System.Console.WriteLine( "[" + nameof(CheckUserFolder) + "] Episódios baixados encontrados" );
                        return true;
                    }
                }
            }

            if ( Program.IsDebugMode )
                System.Console.WriteLine( "[" + nameof(CheckUserFolder) + "] UserDir.Exists: " + UserDir.Exists + "\nDiretório do anime criado." );

            UserDir.CreateSubdirectory(animename);
            return false;
        }

        /// <summary>
        /// Pega o nome do arquivo, e pega o número do episódio
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>Número do episódio</returns>
        public static int GetEpisodeParsed(string filename)
        {
            int one = filename.LastIndexOf('-') + 1;

            string numberconcat = string.Empty;

            for (int i = 0; i < 4; i++)
            {
                if (!int.TryParse(filename.AsSpan(one + i, 1), out int num))
                {
                    break;
                }
                
                numberconcat += num.ToString();
            }

            return int.Parse(numberconcat);
        }

        /// <summary>
        /// Cria um array com os episódios baixados do anime.
        /// </summary>
        /// <param name="animename"></param>
        /// <returns>Array dos episódios baixados</returns>
        public static int[] ExistingEpisodes(string animename)
        {
            // DirectoryInfo AnimeDir = new($"Animes/{animename}");
            DirectoryInfo AnimeDir = new( Path.Combine(UserDir.FullName + animename) );
            int Length = AnimeDir.GetFiles().Length;

            int[] epi = new int[Length];

            for (int i = 0; i < Length; i++)
            {
                string name = AnimeDir.GetFiles().ElementAt(i).Name;

                epi[i] = GetEpisodeParsed(name);
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
        public static int[] OtherEpisodes(int[] episodes, int startepisode, int animelength)
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