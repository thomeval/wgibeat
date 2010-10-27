using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace WGiBeat.Managers
{

    public class LogManager : Manager
    {
        private readonly List<string> _logMessages;

        public LogManager()
        {
            _logMessages = new List<string>();
            _logMessages.Add("INFO: Log initialized.");
        }

        public void ClearMessages()
        {
            Monitor.Enter(_logMessages);
            _logMessages.Clear();
            Monitor.Exit(_logMessages);
        }

        public void AddMessage(string message)
        {
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
    }
}
