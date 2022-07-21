using MinimalLog;

namespace BADownloader.App
{
    public static class BADConsole
    {
        private static readonly Dictionary<string, ConsoleColor> _searchColor = new()
        {
            { "[black]", ConsoleColor.Black },
            { "[blue]", ConsoleColor.Blue },
            { "[cyan]", ConsoleColor.Cyan },
            { "[darkblue]", ConsoleColor.DarkBlue },
            { "[darkcyan]", ConsoleColor.DarkCyan },
            { "[darkgray]", ConsoleColor.DarkGray },
            { "[darkgreen]", ConsoleColor.DarkGreen },
            { "[darkmagenta]", ConsoleColor.DarkMagenta },
            { "[darkred]", ConsoleColor.DarkRed },
            { "[darkyellow]", ConsoleColor.DarkYellow },
            { "[gray]", ConsoleColor.Gray },
            { "[green]", ConsoleColor.Green },
            { "[magenta]", ConsoleColor.Magenta },
            { "[red]", ConsoleColor.Red },
            { "[white]", ConsoleColor.White },
            { "[yellow]", ConsoleColor.Yellow },
        };

        public static void Write(string message)
        {
            bool shouldWrite = true;
            string colorString = string.Empty;

            for (int i = 0; i < message.Length; i++)
            {
                var ch = message[i];
                if (ch.Equals('['))
                {
                    shouldWrite = false;
                    colorString += ch;
                }
                else if ( !shouldWrite && !ch.Equals(']') )
                    colorString += ch;
                else if (ch.Equals(';'))
                    Console.ResetColor();
                else if (ch.Equals(']'))
                {
                    shouldWrite = true;
                    Console.ForegroundColor = _searchColor[colorString + ']'];
                    colorString = string.Empty;
                }
                else if ( shouldWrite )
                    Console.Write(ch);
            }

            Console.ResetColor();
        }

        public static void WriteLine(string message)
        {
            Write(message);
            Console.Write("\n");
        }
    }
}