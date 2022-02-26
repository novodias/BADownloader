using System.Runtime.InteropServices;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using Spectre.Console;

namespace BADownloader
{
    public class Browser
    {
        public static IWebDriver Setup()
        {
            ChromeOptions? chromeoptions;
            FirefoxOptions? firefoxoptions;

            var strnavegador = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title("Selecione o seu navegador:")
                .PageSize(10)
                .AddChoices(new [] 
                {
                    "Chrome", "Firefox"
                }));

            BrowserEnum? navegador = null;
            if (strnavegador == "Chrome") navegador = BrowserEnum.Chrome;
            if (strnavegador == "Firefox") navegador = BrowserEnum.Firefox;

            switch (navegador)
            {
                case BrowserEnum.Chrome:
                    chromeoptions = ChromeSetup();
                    return new ChromeDriver(@"drivers", chromeoptions, TimeSpan.FromSeconds(180));

                case BrowserEnum.Firefox:
                    firefoxoptions = FirefoxSetup();
                    return new FirefoxDriver(@"drivers", firefoxoptions, TimeSpan.FromSeconds(180));

                default:
                    throw new Exception("Algo de errado ocorreu selecionando o navegador");
            }  
        }

        private static ChromeOptions? ChromeSetup()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string ProgramFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                string ProgramFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                string LocalAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                string[] chromeloc = 
                {
                    $@"{ProgramFiles}\Google\Chrome\Application\chrome.exe",
                    $@"{ProgramFilesX86}\Google\Chrome\Application\chrome.exe",
                    $@"{LocalAppData}\Google\Chrome\Application\chrome.exe"
                };

                if (File.Exists(chromeloc[0]) || File.Exists(chromeloc[1]) || File.Exists(chromeloc[2]))
                {
                    string location;
                    if (File.Exists(chromeloc[0]))
                        location = chromeloc[0];
                    else if (File.Exists(chromeloc[1]))
                        location = chromeloc[1];
                    else
                        location = chromeloc[2];

                    var chrome = new ChromeOptions
                    {
                        BinaryLocation = location
                    };

                    chrome.AddArguments("--headless", "--disable-gpu", "--log-level=3", "--incognito", "--no-sandbox");

                    chrome.SetLoggingPreference(LogType.Browser, LogLevel.Off);
                    chrome.SetLoggingPreference(LogType.Client, LogLevel.Off);
                    chrome.SetLoggingPreference(LogType.Driver, LogLevel.Off);
                    chrome.SetLoggingPreference(LogType.Profiler, LogLevel.Off);
                    chrome.SetLoggingPreference(LogType.Server, LogLevel.Off);

                    return chrome;
                }
                else
                {
                    return null;
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Console.WriteLine("Insira o path do executável ");
                Console.WriteLine("Não sabe onde fica? Abra o terminal e use o comando \"whereis\"");
                Console.WriteLine("Deixe em branco caso tenha selecionado o Chrome:");

                string location = Console.ReadLine() ?? "/usr/bin/google-chrome-stable";
                if (string.IsNullOrEmpty(location)) location = "/usr/bin/google-chrome-stable";

                if (File.Exists(location))
                {
                    var chrome = new ChromeOptions
                    {
                        BinaryLocation = location
                    };

                    chrome.AddArguments("--headless", "--disable-gpu", "--log-level=3", "--incognito", "--no-sandbox");

                    chrome.SetLoggingPreference(LogType.Browser, LogLevel.Off);
                    chrome.SetLoggingPreference(LogType.Client, LogLevel.Off);
                    chrome.SetLoggingPreference(LogType.Driver, LogLevel.Off);
                    chrome.SetLoggingPreference(LogType.Profiler, LogLevel.Off);
                    chrome.SetLoggingPreference(LogType.Server, LogLevel.Off);

                    return chrome;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
    
        private static FirefoxOptions? FirefoxSetup()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string ProgramFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                string ProgramFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                string LocalAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                string[] firefoxloc = 
                {
                    $@"{LocalAppData}\Mozilla Firefox\firefox.exe",
                    $@"{ProgramFiles}\Mozilla Firefox\firefox.exe",
                    $@"{ProgramFilesX86}\Mozilla Firefox\firefox.exe"
                };

                if (File.Exists(firefoxloc[0]) || File.Exists(firefoxloc[1]) || File.Exists(firefoxloc[2]))
                {
                    string location;
                    if (File.Exists(firefoxloc[0]))
                        location = firefoxloc[0];
                    else if (File.Exists(firefoxloc[1]))
                        location = firefoxloc[1];
                    else
                        location = firefoxloc[2];

                    var firefox = new FirefoxOptions
                    {
                        BrowserExecutableLocation = location
                    };

                    firefox.AddArguments("--headless", "--disable-gpu", "--log-level=3", "--incognito", "--no-sandbox");

                    firefox.SetLoggingPreference(LogType.Browser, LogLevel.Off);
                    firefox.SetLoggingPreference(LogType.Client, LogLevel.Off);
                    firefox.SetLoggingPreference(LogType.Driver, LogLevel.Off);
                    firefox.SetLoggingPreference(LogType.Profiler, LogLevel.Off);
                    firefox.SetLoggingPreference(LogType.Server, LogLevel.Off);

                    return firefox;
                }
                else
                {
                    return null;
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                string location = Console.ReadLine() ?? throw new Exception("Insira o path do Firefox!");

                if (File.Exists(location))
                {
                    var firefox = new FirefoxOptions
                    {
                        BrowserExecutableLocation = location
                    };

                    firefox.AddArguments("--headless", "--disable-gpu", "--log-level=3", "--incognito", "--no-sandbox");

                    firefox.SetLoggingPreference(LogType.Browser, LogLevel.Off);
                    firefox.SetLoggingPreference(LogType.Client, LogLevel.Off);
                    firefox.SetLoggingPreference(LogType.Driver, LogLevel.Off);
                    firefox.SetLoggingPreference(LogType.Profiler, LogLevel.Off);
                    firefox.SetLoggingPreference(LogType.Server, LogLevel.Off);

                    return firefox;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
    }

    public enum BrowserEnum
    {
        Chrome,
        Firefox
    }
}