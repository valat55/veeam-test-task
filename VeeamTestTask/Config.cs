using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VeeamTestTask
{
    public class Config
    {
        public static DirectoryInfo Src { get; set; }
        public static DirectoryInfo Dest { get; set; }
        public static FileInfo logFile { get; set; }
        public static int Interval { get; set; }
    }
}
