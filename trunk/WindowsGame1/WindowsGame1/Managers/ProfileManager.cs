using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace WGiBeat.Managers
{
    public class ProfileManager : Manager
    {
        private List<Profile> _profiles;

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
            log.AddMessage("INFO: Loading profiles from " + Path.GetFullPath(path) + " ...");
            pm.Log = log;
            var bf = new BinaryFormatter();

            if (!Directory.Exists(path))
            {
                pm.Log.AddMessage("WARN: Folder '" + Path.GetFullPath(path) + "' doesn't exist.");
                return pm;
            }
            foreach (string file in Directory.GetFiles(path,"*.prf"))
            {
                var fs = File.OpenRead(file);

                var profile = (Profile) bf.Deserialize(fs);
                pm.Add(profile);
                fs.Close();
            }
            pm.Log.AddMessage("INFO: "+pm.Count +"Profiles loaded successfully.");
            return pm;
        }

        public void SaveToFolder(string path)
        {
            var bf = new BinaryFormatter();
            foreach (Profile profile in _profiles)
            {
                var fs = File.OpenWrite(path + "\\" + profile.Name + ".prf");
                bf.Serialize(fs,profile);
                fs.Close();
            }
            Log.AddMessage("INFO: " + Count + "Profiles save successfully.");
        }

    }
}
