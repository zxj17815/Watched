using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Watched
{
    public class Logger
    {
        public static void Write(Exception ex) 
        {
            string str = "log.txt";
            FileStream fs = new FileStream(str, FileMode.Append, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.Default);
            string log = string.Format("Date:{0}\n", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff"));
            log += string.Format("Erroe:{0}\n", ex.Message);
            sw.WriteLine(log);
            sw.Close();
            fs.Close();
        }
    }
}
