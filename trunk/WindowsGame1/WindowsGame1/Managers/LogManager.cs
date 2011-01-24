using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace WGiBeat.Managers
{

    public class LogManager : Manager
    {
        private readonly List<LogEntry> _logMessages;
     
        public bool SaveLog { get; set; }

        //TODO: Complete this.

        public LogLevel LogLevel { get; set; }
        public LogManager()
        {
            _logMessages = new List<LogEntry>();
            AddMessage("Log Initialized", LogLevel.INFO);
            LogLevel = LogLevel.INFO;
        }

        public bool Enabled
        {
            get;
            set;
        }

        public void ClearMessages()
        {
            Monitor.Enter(_logMessages);
            _logMessages.Clear();
            Monitor.Exit(_logMessages);
        }

        public void AddMessage(string message, LogLevel level)
        {
            AddMessage(new LogEntry{Message = message, Level = level});
        }

        public void AddMessage(LogEntry logEntry)
        {
            if (!Enabled)
            {
                return;
            }
            if (logEntry.Level < LogLevel)
            {
                return;
            }
            Monitor.Enter(_logMessages);
            _logMessages.Add(logEntry);
            Monitor.Exit(_logMessages);
        }

        public LogEntry[] GetMessages()
        {
            Monitor.Enter(_logMessages);
            var result = _logMessages.ToArray();
            Monitor.Exit(_logMessages);
            return result;
        }
        
        public string[] ToStringArray()
        {
            Monitor.Enter(_logMessages);
            var result = (from e in _logMessages select e.ToString()).ToArray();
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
                var filename = Path.GetDirectoryName(
                    Assembly.GetAssembly(typeof(GameCore)).CodeBase) + "\\log.txt";
                filename = filename.Replace("file:\\", "");
                File.WriteAllLines(filename, ToStringArray());
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

    public class LogEntry
    {
        public LogEntry()
        {
            TimeStamp = DateTime.Now;
        }
        public LogLevel Level { get; set; }
        public string Message { get; set; }
        public DateTime TimeStamp { get; set; }

        public override string ToString()
        {
            return String.Format("{0} - [{1}]: {2}", TimeStamp, Level, Message);
        }
    }

}
