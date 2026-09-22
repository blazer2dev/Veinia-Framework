using System;
using System.IO;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace VeiniaFramework
{
    public class FileManager
    {
        public static bool UseEncryption = true;

        public static object Save(object objectToSave, string path, string fileName, bool appdataFolder = false)
        {
            object dataToSave = JsonConvert.SerializeObject(objectToSave);

            if (UseEncryption) dataToSave = Encryption.Encrypt((string)dataToSave);

            var useDirectory = !string.IsNullOrWhiteSpace(path);

            // game directory

            var saveFilePathDir = appdataFolder ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), path) : path;
            if (useDirectory && !Directory.Exists(saveFilePathDir))
                Directory.CreateDirectory(saveFilePathDir);

            var saveFilePath = useDirectory ? Path.Combine(saveFilePathDir, fileName) : fileName;

            if (UseEncryption) File.WriteAllBytes(saveFilePath, (byte[])dataToSave);
            else File.WriteAllText(saveFilePath, (string)dataToSave);
            //

#if !RELEASE
            // project directory
            var devProjectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;

            var devProjectDirPath = Path.Combine(devProjectDirectory, path);
            if (useDirectory && !Directory.Exists(devProjectDirPath))
                Directory.CreateDirectory(devProjectDirPath);

            var devProjectWritePath = Path.Combine(devProjectDirPath, fileName);

            if (UseEncryption) File.WriteAllBytes(devProjectWritePath, (byte[])dataToSave);
            else File.WriteAllText(devProjectWritePath, (string)dataToSave);
            //
#endif

            return UseEncryption ? (byte[])dataToSave : (string)dataToSave;
        }

        public static T1 Load<T1>(string path, string fileName, bool appdataFolder = false)
        {
            var loadPath = string.IsNullOrWhiteSpace(path) ? fileName : Path.Combine(path, fileName);
            if (appdataFolder) loadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), loadPath);

            object dataToLoad;

            if (OperatingSystem.IsBrowser())
            {
                using (var stream = TitleContainer.OpenStream(loadPath))
                {
                    if (stream == null) throw new Exception("No File Found! " + loadPath);

                    using (var reader = new StreamReader(stream))
                        dataToLoad = reader.ReadToEnd();
                }
            }
            else
            {
                dataToLoad = UseEncryption ? Encryption.Decrypt(File.ReadAllBytes(loadPath)) : File.ReadAllText(loadPath);

                if (!File.Exists(loadPath)) throw new Exception("No File Found! " + loadPath);
            }

            return JsonConvert.DeserializeObject<T1>((string)dataToLoad);
        }
    }
}