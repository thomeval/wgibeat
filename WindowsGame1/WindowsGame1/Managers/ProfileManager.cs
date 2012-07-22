using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using WGiBeat.Players;

namespace WGiBeat.Managers
{
    public class ProfileManager : Manager
    {
        private readonly List<Profile> _profiles;

        public ProfileManager()
        {
            _profiles = new List<Profile>();
        }
        public Profile this[string name]
        {
            get { return (from e in _profiles where e.Name == name select e).SingleOrDefault(); }
        }

        public IEnumerable GetAll()
        {
            return _profiles.ToArray();
        }

        public void Add(Profile profile)
        {
            var check = (from e in _profiles where e.Name == profile.Name select e).SingleOrDefault();
            if (check != null)
            {
                throw new Exception("ProfileManager: Already have a profile called " + profile.Name);
            }
            _profiles.Add(profile);
        }

        public int Count
        {
            get { return _profiles.Count; }
        }

        public static ProfileManager LoadFromFolder(string path, LogManager log)
        {
            var pm = new ProfileManager();
            log.AddMessage("Loading profiles from " + Path.GetFullPath(path) + " ...", LogLevel.INFO);
            pm.Log = log;
            var bf = new BinaryFormatter();

            if (!Directory.Exists(path))
            {
                pm.Log.AddMessage("Folder '" + Path.GetFullPath(path) + "' doesn't exist.", LogLevel.WARN);
                return pm;
            }
            foreach (string file in Directory.GetFiles(path,"*.prf"))
            {

                try
                {

                    var fs = File.OpenRead(file);
                    var profile = (Profile)bf.Deserialize(fs);
                    if (Profile.ProfileOutOfDate)
                    {
                        pm.Log.AddMessage("Profile is outdated but loaded successfully: " + file, LogLevel.NOTE);
                    }
                    pm.Add(profile);
                    fs.Close();
                }
                catch (Exception ex)
                {
                    pm.Log.AddMessage("Failed to load profile due to error: " + ex.Message + " File: " + file,LogLevel.WARN);
                }
            }
            pm.Log.AddMessage(""+pm.Count +" Profiles loaded successfully.",LogLevel.INFO);
            return pm;
        }

        public void SaveToFolder(string path)
        {
            try
            {

            
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var bf = new BinaryFormatter();
            foreach (Profile profile in _profiles)
            {
                var fs = File.OpenWrite(path + "\\" + profile.Name + ".prf");
                bf.Serialize(fs,profile);
                fs.Close();
            }
            Log.AddMessage("" + Count + " Profiles save successfully.",LogLevel.INFO);
            }
            catch (Exception ex)
            {
                Log.AddException(ex);
                Log.AddMessage("Failed to save profiles: " + ex.Message, LogLevel.ERROR);
                throw;
            }
        }

    }
}
