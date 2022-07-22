using MinimalLog;
using HtmlAgilityPack;
using BADownloader.Sites;
using System.Text;

namespace BADownloader.App
{
    class Program
    {
        public static void Main(string[] args)
        {
            bool writable = false;
            int loglevel = -1;

            if ( args.Contains("--verbose") )
                loglevel = 4;
            if ( args.Contains("--savelog") )
                writable = true;

            MinimalLogger.SetUpLogger(loglevel, writable);
            string time = "############ " + DateTime.Now.TimeOfDay.ToString() + " ############";
            MinimalLogger.Log(Logging.Debug, id, time);

            MainAsync().GetAwaiter().GetResult();
        }

        static readonly string id = "BADownloader";
        delegate Task<Extractor> GetExtractorAsync(HtmlDocument document, string url);

        public static async Task MainAsync()
        {
            Console.Title = "BADownloader";

            MinimalLogger.Log(Logging.Debug, id, "BADownloader verbose");
            MinimalLogger.Log(Logging.Information, "AnimesData", "Diretório de animes: " + AnimesData.Directory);

            try
            {
                BADHttp.Client.Timeout = TimeSpan.FromMinutes(5);
                IExtractor extractor = await InitializeExtractor();

                var episodes = extractor.ACollection.EpisodesCollection;
                var episodesString = new StringBuilder(episodes.Count == extractor.Total ? "Episódios: " : "Episódios restantes: ");

                foreach (var num in episodes)
                {
                    if ( num != episodes.ElementAt(0) )
                        episodesString.AppendFormat(", [darkblue]{0};", num);
                    else
                        episodesString.AppendFormat("[darkblue]{0};", num);
                }

                BADConsole.WriteLine($"\nNome: [darkblue]{extractor.Name};");
                BADConsole.WriteLine($"Total de episódios do anime: [darkblue]{extractor.Total};");
                BADConsole.WriteLine(episodesString.ToString() + "\n");

                int start = StartInput(extractor.ACollection.EpisodesCollection);
                extractor.TrySetStart(start);

                Quality quality = QualityInput();
                extractor.SetOptionQuality(quality);

                int downloadnum = DownloadInput();

                Downloader downloader = new( extractor, downloadnum );
                await downloader.StartAsync();
                // downloader.Dispose();
            }
            catch (Exception ex)
            {
                MinimalLogger.Log(Logging.Error, id, ex.ToString());
            }
            finally
            {
                // Não sei se isso faz alguma diferença
                BADHttp.Dispose();

                Console.WriteLine("Pressione qualquer botão pra sair");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Seleciona o extrator necessário pro site inserido
        /// </summary>
        /// <returns>Extrator</returns>
        /// <exception cref="Exception"></exception>
        static async Task<IExtractor> InitializeExtractor()
        {
            string example = "Exemplo de url:\n[yellow]https://betteranime.net/anime/legendado/shingeki-no-kyojin\nhttps://animeyabu.com/anime/kimetsu-no-yaiba-yuukaku-hen-part-3;";
            
            BADConsole.WriteLine(example);

            string url;
            Console.WriteLine("Insira a URL do Anime: ");
            url = Console.ReadLine() ?? string.Empty;

            bool IsSiteSupported = AvailableSites.Contains(url);

            while ( !IsSiteSupported )
            {
                BADConsole.WriteLine("URL [red]inválido;");
                Console.WriteLine("Insira a URL do Anime: ");
                url = Console.ReadLine() ?? string.Empty;

                IsSiteSupported = AvailableSites.Contains(url);
            }

            var response = await BADHttp.SendAsync(url);
            HtmlDocument doc = new();
            doc.Load(await response.Content.ReadAsStreamAsync());

            GetExtractorAsync betterAnime = new(BetterAnime.InitializeExtractorAsync);
            GetExtractorAsync animeYabu = new(AnimeYabu.InitializeExtractorAsync);

            var website = AvailableSites.GetSite( url );

            return website switch
            {
                Website.BetterAnime => await betterAnime(doc, url),
                Website.AnimeYabu => await animeYabu(doc, url),
                _ => throw new Exception("Algo errado ocorreu")
            };
        }

        static int StartInput(IEnumerable<int> episodes)
        {
            string str;
            Console.Write("Insira de qual episódio deseja começar à baixar: ");

            str = Console.ReadLine() ?? string.Empty;
            if ( str.Equals(string.Empty) ) return StartInput(episodes);

            foreach (var ch in str)
            {
                if ( !Char.IsDigit(ch) )
                    return StartInput(episodes);
            }

            int number = int.Parse(str);
            if ( !episodes.Any(num => num.Equals(number)) ) return StartInput(episodes);

            return number;
        }

        static int DownloadInput()
        {
            string str;

            BADConsole.WriteLine("Quantos downloads deseja ter? 1, 2, 3, 4 ou 5");
            BADConsole.WriteLine("[yellow]Recomendado; um PC/Notebook [green]bom; suficiente para baixar 5/4/3/2 arquivos simultâneamente");
            str = Console.ReadLine() ?? string.Empty;

            if (!int.TryParse(str, out int input))
            {
                Console.WriteLine("Isso não é um número!");
                return DownloadInput();
            }
            else
            {
                if (input < 1) input = 1;
                else if (input > 5) input = 5;
                return input;
            }
        }

        static Quality QualityInput()
        {
            string message = "Selecione a qualidade de vídeo preferida." +
            "\nOBS: Nem todas as qualidades estarão disponíveis dependendo do [yellow]anime; e do [yellow]site;." +
            "\nCada episódio pode variar de [green]~100mb; à [red]~1gb; dependendo da qualidade" +
            "\nVerifique se seu disco contém espaço suficiente! \n[yellow]1;. SD\n[yellow]2;. HD\n[yellow]3;. FULL HD\n[red]4;. AUTOMÁTICO (PRIORIZA QUALIDADE)";

            BADConsole.WriteLine(message);
            char digitChar = Console.ReadKey(true).KeyChar;
            int digit = int.Parse(digitChar.ToString());

            while (!Char.IsDigit(digitChar) && !(digit > 0 && digit < 5))
            {
                digitChar = Console.ReadKey(true).KeyChar;
                digit = int.Parse(digitChar.ToString());
            }

            BADConsole.WriteLine(
                (Quality)digit == Quality.SD ? "Qualidade [yellow]SD; selecionado.\n" : 
                (Quality)digit == Quality.HD ? "Qualidade [darkblue]HD; selecionado.\n" : 
                (Quality)digit == Quality.FHD ? "Qualidade [green]FULL HD; selecionado.\n" :
                "Qualidade [green]mais alta; será [yellow]priorizada;."
            );

            return digit switch
            {
                1 => Quality.SD,
                2 => Quality.HD,
                3 => Quality.FHD,
                4 => Quality.AUTO,
                _ => throw new Exception("Opção inválida")
            };
        }
    }
}