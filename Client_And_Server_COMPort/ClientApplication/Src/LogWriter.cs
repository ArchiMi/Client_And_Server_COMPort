using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection;

namespace ClientAppNameSpace.Src
{
    public class Log
    {
        private string _file_name;

        private static object sync = new object();

        public Log(string file_name)
        {
            this._file_name = file_name;
        }

        private void WriteLog(string type, string message)
        {
            try
            {
                string date_str = DateTime.Now.ToString("yyyy_MM_dd");
#if DEBUG
                string pathToLog = $"Logs\\{date_str}";
#else
            string pathToLog = $"C:\\Log\\{date_str}";
#endif

                //string filename = Path.Combine(pathToLog, string.Format("{0}_{1:yyy.MM.dd}.log", nameLogFile, DateTime.Now));
                string filename = Path.Combine(pathToLog, string.Format("{0}.log", this._file_name));
                string fullText = string.Format("[{0:dd.MM.yyy HH:mm:ss.fff}] {1} {2}\r\n", DateTime.Now, type, message);
                lock (sync)
                {
                    if (!Directory.Exists(pathToLog))
                        Directory.CreateDirectory(pathToLog);

                    File.AppendAllText(filename, fullText, Encoding.GetEncoding("Windows-1251"));
                }
            }
            catch
            {
                // Перехватываем все и ничего не делаем
            }
        }

        public void WriteError(string message)
        {
            WriteLog("Error: ", message);
        }

        public void WriteWarning(string message)
        {
            WriteLog("Warning: ", message);
        }

        public void WriteInfo(string message)
        {
            WriteLog("", message);
        }
    }

}
