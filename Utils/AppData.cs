using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace LiveSplit.SourceSplit.Utils
{
    public static class AppData
    {
        private static string _appDataString => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        public static string GetDataPath(params string[] paths)
        {
            string dir = Path.Combine(_appDataString, "SourceSplit", string.Join("\\", paths.Take(paths.Count() - 1)));
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            return Path.Combine(dir, paths[paths.Count() - 1] + ".dat");
        }

        public static void DeleteData(params string[] paths)
        {
            string path = GetDataPath(paths);
            if (!File.Exists(path))
                return;
            File.Delete(path);
        }
    }

    // for serialization purposes
    class SerialBinder : System.Runtime.Serialization.SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            // Get the current assembly
            string currentAssembly = Assembly.GetExecutingAssembly().FullName;

            // Create the new type and return it
            return Type.GetType(string.Format("{0}, {1}", typeName, currentAssembly));
        }
    }
}
