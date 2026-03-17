using CommandLine;
using CommandLine.Text;

namespace VeeamTestTask
{
    public class CommandParser
    {
        public class Options
        {
            [Option('s', "src", Required = true, HelpText = "Path to source folder")]
            public string Source { get; set; }

            [Option('d', "dest", Required = true, HelpText = "Path to replica folder")]
            public string Replica { get; set; }

            [Option('l', "log", Required = true, HelpText = "Path to log file")]
            public string LogfileName { get; set; }

            [Option('i', "interval", Required = true, HelpText = "Interval for the syncing(seconds)")]
            public int Interval { get; set; }
        }

        public static bool TryParseArguments(string[] args)
        {

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(opts =>
                {
                    Config.Src = new DirectoryInfo(opts.Source);
                    Config.Dest = new DirectoryInfo(opts.Replica);
                    Config.Interval = opts.Interval;
                    Config.logFile = new FileInfo(opts.LogfileName);
                });
            if (!Config.Src.Exists)
            {
                Console.WriteLine("Source folder doesn't exist!");
                return false;
            }
            if (Config.Dest.Exists)
            {
                Console.WriteLine("Destination folder already exist! Do you want to rewrite it? (Press Enter if yes)");
                var keyInfo = Console.ReadKey();
                if (!(keyInfo.Key == ConsoleKey.Enter))
                {
                    return false;
                }
                Config.Dest.Delete(true);

            }
            Config.Dest.Create();
            if (Config.logFile.Exists)
            {
                Config.logFile.Delete();
            }
            try
            {
                Config.logFile.Create().Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Logfile cannot be created\n" + ex.Message);
            }
            return true;
        }
    }
}
