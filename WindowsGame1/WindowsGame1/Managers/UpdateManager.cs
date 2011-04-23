using System;
using System.Text;
using System.Net;

namespace WGiBeat.Managers
{
    public class UpdateManager : Manager
    {

        public string LatestVersion { get; set; }
        public string NewsFeed { get; set; }

        public event EventHandler UpdateInfoAvailable;
        public event EventHandler UpdateInfoFailed;

        public void GetLatestVersion()
        {
            var result = WebRequest.Create("http://wgibeat.googlecode.com/svn/trunk/Notes/LatestVersion.txt");
            var response = (HttpWebResponse) result.GetResponse();
            var stream = response.GetResponseStream();

            var buffer = new byte[8192];
            var stringBuilder = new StringBuilder();
            string tempString = null;
            int count = 0;

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
            if (UpdateInfoAvailable != null)
            {
                UpdateInfoAvailable(this, null);
            }
        }

        private void ApplyVersionData(string str)
        {
            str = str.Replace('\r', ' ');
            var lines = str.Split('\n');
            LatestVersion = lines[0];
            NewsFeed = lines[1];
        }


    }
}
