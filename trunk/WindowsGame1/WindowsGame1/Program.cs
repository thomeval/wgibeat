using System;
using System.Windows.Forms;
using System.IO;

namespace WGiBeat
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
#if (DEBUG)
            if (!File.Exists("RanDebug.txt"))
            {
                MessageBox.Show(
                    "Thank you for trying out this alpha version of WGiBeat. Since this is not a stable release, there WILL be bugs, and some features will not work as expected. " +
                    "Please report any bugs you find to the issue tracker on the WGiBeat website, or send an email to wgibeat@gmail.com.",
                    "Attention", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

            }
            try
            {
                File.Create("RanDebug.txt");
            }
            catch (Exception)
            {
                
                
            }
            using (var game = GameCore.Instance)
            {
                game.Run();
                game.Log.SaveToFile();
            }
#else
            try
            {
                using (GameCore game = GameCore.Instance)
                {
                    game.Run();
                    game.Log.SaveToFile();
                }
            }
             catch (Exception ex)
            {
                MessageBox.Show(
                    "A critical error has occurred. Please check the error.txt and log.txt files created in the wgibeat folder for details. If this is a bug, please send these files to a developer.");
                SaveErrorLog(ex);
            }
#endif
        }

        private static void SaveErrorLog(Exception ex)
        {
            try
            {
                var file = new StreamWriter(File.Open("error.txt", FileMode.OpenOrCreate, FileAccess.Write));
                file.WriteLine(ex.Message + "\n");
                file.WriteLine(ex.StackTrace);
                if (ex.InnerException != null)
                {
                    file.WriteLine(ex.InnerException.Message);
                }
            file.Close();
            }
            catch (Exception ex2)
            {
                MessageBox.Show("Failed to write error.txt file. " + ex2.Message + "\n Original wgibeat error: " +
                                ex.Message + "\n" + ex.StackTrace);
            }

        }
    }

}

