using System;
using System.Text;
using System.Net;

namespace WGiBeat.Managers
{
    public class UpdateManager : Manager
    {

        public string LatestVersion { get; private set; }
        public string UpdateDetails { get; private set; }
        public string NewsFeed { get; private set; }
        public string ErrorMessage { get; private set; }

        public event EventHandler UpdateInfoAvailable;
        public event EventHandler UpdateInfoFailed;
        public Exception LastException { get; private set; }

        public void GetLatestVersion()
        {
            if (!String.IsNullOrEmpty(LatestVersion))
            {
                Log.AddMessage("Skipping version information web request.", LogLevel.DEBUG);
                if (UpdateInfoAvailable != null)
                {
                    UpdateInfoAvailable(this, null);
                }
                return;
            }

            try
            {
                Log.AddMessage("Fetching latest version information...",LogLevel.INFO);
                var result = WebRequest.Create("http://wgibeat.googlecode.com/svn/trunk/Notes/LatestVersion.txt");
                var response = (HttpWebResponse)result.GetResponse();
                var stream = response.GetResponseStream();

                var buffer = new byte[8192];
                var stringBuilder = new StringBuilder();
                string tempString;
                int count;

                do
                {
                    // fill the buffer with data
                    count = stream.Read(buffer, 0, buffer.Length);

                    // make sure we read some data
                    if (count != 0)
                    {
                        // translate from bytes to ASCII text
                        tempString = Encoding.ASCII.GetString(buffer);
                        tempString = tempString.Replace("\0", "");
                        // continue building the string
                        stringBuilder.Append(tempString);
                    }
                }
                while (count > 0);
                ApplyVersionData(stringBuilder.ToString());
                Log.AddMessage("Update check successful. Latest version is " + this.LatestVersion,LogLevel.INFO);
                if (UpdateInfoAvailable != null)
                {
                    UpdateInfoAvailable(this, null);
                }
            }
            catch (Exception ex)
            {
                LastException = ex;
                ErrorMessage = ex.Message;
                Log.AddMessage("Update check failed: " + this.ErrorMessage, LogLevel.NOTE);
                if (UpdateInfoFailed != null)
                {
                    UpdateInfoFailed(this, null);
                }              
            }
        }

        private void ApplyVersionData(string str)
        {
            str = str.Replace('\r', ' ');
            var lines = str.Split('\n');
            LatestVersion = lines[0].Trim();
            UpdateDetails = lines[1].Trim();

            if (lines.Length > 2)
            {
                NewsFeed = lines[2].Trim();
            }
        }

        public void Reset()
        {
            LatestVersion = "";
            ErrorMessage = "";
            UpdateDetails = "";
        }

    }
}
