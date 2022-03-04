using System.Text.RegularExpressions;
using OpenQA.Selenium;
using HtmlAgilityPack;
using Spectre.Console;

namespace BADownloader
{
    class Program
    {
        public static void Main()
        {
            var pro = new Program();
            pro.MainAsync().GetAwaiter().GetResult();
        }

        IWebDriver? browser;

        public async Task MainAsync()
        {
            Console.Title = "BADownloader";

            AnsiConsole.Write(new Markup("Navegadores suportados: [bold yellow]Chrome[/] e [bold red]Firefox[/]\n"));
            AnsiConsole.Write(new Markup("Exemplo de url: https://betteranime.net/anime/legendado/shingeki-no-kyojin\n"));
            string url = AnsiConsole.Ask<string>("Insira a URL do anime:");

            if (!url.StartsWith("https://betteranime.net")) throw new Exception("URL inválida");

            try
            {
                HtmlWeb web = new();
                string animename;
                int episodes_length;

                List<object> info = await AnimeInfo.GetAnimeInfo(url, web);
                
                animename = (string)info.ElementAt(0);
                episodes_length = (int)info.ElementAt(1);
                string genres = (string)info.ElementAt(4);

                string chars = Regex.Escape(@"<>:" + "\"" + "/|?*");
                string pattern = "[" + chars + "]";
                animename = Regex.Replace(animename, pattern, "");

                AnsiConsole.Write(new Markup(string.Format("Anime: [green bold]{0}[/]\nNúmero de episódios: [green bold]{1}[/]\n", animename, episodes_length)));
                AnsiConsole.Write(new Markup(string.Format($"Gêneros: {genres}\n")));
                
                Dictionary<int, string> episodesdic = (Dictionary<int, string>)info.ElementAt(2);
                int[] episodes = (int[])info.ElementAt(3);

                // --------------------------------------------

                bool animeExist = AnimeInfo.CheckExistingFolder(animename);

                if (animeExist)
                {
                    episodes = AnimeInfo.ExistingEpisodes(animename);
                    episodes = AnimeInfo.OtherEpisodes(episodes, episodesdic.ElementAt(0).Key, episodes_length);

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
                        temporary.Add(episodes[i], episodesdic.Single(ctx => ctx.Key == episodes[i]).Value);
                    }
                    episodesdic = temporary;
                }
                else
                {
                    for (int i = 0; i < episodes_length; i++)
                    {
                        episodes[i] = i + 1;
                    }
                }

                // --------------------------------------------
                
                // Substituir episodes_length
                int startpoint = AnimeInfo.EpisodeInput(episodes_length, episodes);
                int downloadnum = AnimeInfo.DownloadInput();
                string quality = AnimeInfo.QualityInput();

                // --------------------------------------------

                if (!Directory.Exists($"Animes/{animename}"))
                    Directory.CreateDirectory($"Animes/{animename}");
                
                // --------------------------------------------

                this.browser = Browser.Setup();

                Anime anime = new(animename, episodesdic, episodes, url, startpoint, quality);

                DownloadManager Manage = new(downloadnum, episodes_length, anime);

                await Manage.GetLinkDownloadAsync(this.browser, web);
                await Manage.StartDownloadAsync();

                Manage.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Algum erro ocorreu:");
                AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
            }
            finally
            {
                if ( this.browser is not null )
                    this.browser.Dispose();

                Console.WriteLine("Pressione qualquer botão pra sair");
                Console.ReadKey();
            }
        }
    }
}