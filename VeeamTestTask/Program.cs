using System.Runtime.InteropServices;
using System.Text;
using CommandLine;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace VeeamTestTask
{
    class Program
    {

        static void Main(string[] args)
        {
            if (!CommandParser.TryParseArguments(args))
            {
                return;
            }

            var test = new FolderModel(Config.Src, Config.Dest);
            var folderInit = DateTime.Now.ToString() + "\n" + test.ToString();
            log(folderInit, test.Errors, Config.logFile);

            while (true)
            {
                Thread.Sleep(TimeSpan.FromSeconds(Config.Interval));
                var result = test.Sync();
                result = result == string.Empty ? "No changes.\n" : result;
                result = DateTime.Now.ToString() + "\n" + result;
                log(result, test.Errors, Config.logFile);
            }

        }

        // logger class
        private static void log(string result, List<string> errors, FileInfo logFile)
        {
            var sb = new StringBuilder();
            sb.Append(result);
            foreach (var error in errors)
                sb.AppendLine(error);

            File.AppendAllText(logFile.FullName, sb.ToString());
            Console.Write(sb.ToString());

        }
    }

}
