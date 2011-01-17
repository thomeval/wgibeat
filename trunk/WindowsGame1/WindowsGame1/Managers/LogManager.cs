using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;

namespace WGiBeat.Managers
{

    public class LogManager : Manager
    {
        private readonly List<string> _logMessages;

        public bool SaveLog { get; set; }

        //TODO: Complete this.

        public LogLevel LogLevel { get; set; }
        public LogManager()
        {
            _logMessages = new List<string>();
            _logMessages.Add("INFO: Log initialized.");
        }

        public bool Enabled
        {
            get; set;
        }

        public void ClearMessages()
        {
            Monitor.Enter(_logMessages);
            _logMessages.Clear();
            Monitor.Exit(_logMessages);
        }

        public void AddMessage(string message)
        {
            if (!Enabled)
            {
                return;
            }
            Monitor.Enter(_logMessages);
            _logMessages.Add(message);
            Monitor.Exit(_logMessages);
        }

        public string[] GetMessages()
        {
            Monitor.Enter(_logMessages);
            var result = _logMessages.ToArray();
            Monitor.Exit(_logMessages);
            return result;
        }

        public int MessageCount()
        {
            Monitor.Enter(_logMessages);
            int result = _logMessages.Count;
            Monitor.Exit(_logMessages);
            return result;
        }

        public void SaveToFile()
        {
            if (SaveLog)
            {
                var filename  = Path.GetDirectoryName(
                    Assembly.GetAssembly(typeof(GameCore)).CodeBase) + "\\log.txt";
                filename = filename.Replace("file:\\", "");
                File.WriteAllLines(filename, _logMessages.ToArray());
            }
        }
    }
    public enum LogLevel
    {
        ERROR = 4,
        WARN = 3,
        NOTE = 2,
        INFO = 1,
        DEBUG = 0
    }
}
