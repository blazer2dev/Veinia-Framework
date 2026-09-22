using System;
using System.IO;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace VeiniaFramework
{
    public class FileManager
    {
        public static bool UseEncryption = true;

        public static object Save(object objectToSave, string path, string fileName)
        {
            object dataToSave = JsonConvert.SerializeObject(objectToSave);

            if (UseEncryption) dataToSave = Encryption.Encrypt((string)dataToSave);

            // game directory
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            var gameWritePath = Path.Combine(path, fileName);

            if (UseEncryption) File.WriteAllBytes(gameWritePath, (byte[])dataToSave);
            else File.WriteAllText(gameWritePath, (string)dataToSave);
            //

            // project directory
            var projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
            var projectLevelFolder = Path.Combine(projectDirectory, path);
            if (!Directory.Exists(projectLevelFolder)) Directory.CreateDirectory(projectLevelFolder);

            var projectWritePath = Path.Combine(projectLevelFolder, fileName);

            if (UseEncryption) File.WriteAllBytes(projectWritePath, (byte[])dataToSave);
            else File.WriteAllText(projectWritePath, (string)dataToSave);
            //

            return UseEncryption ? (byte[])dataToSave : (string)dataToSave;
        }

        public static T1 Load<T1>(string path, string fileName)
        {
            var loadDir = Path.Combine(path, fileName);
            object dataToLoad;

            if (OperatingSystem.IsBrowser())
            {
                using (var stream = TitleContainer.OpenStream(loadDir))
                {
                    if (stream == null) throw new Exception("No File Found! " + loadDir);

                    using (var reader = new StreamReader(stream))
                        dataToLoad = reader.ReadToEnd();
                }
            }
            else
            {
                dataToLoad = UseEncryption ? Encryption.Decrypt(File.ReadAllBytes(loadDir)) : File.ReadAllText(loadDir);

                if (!File.Exists(loadDir)) throw new Exception("No File Found! " + loadDir);
            }

            return JsonConvert.DeserializeObject<T1>((string)dataToLoad);
        }
    }
}