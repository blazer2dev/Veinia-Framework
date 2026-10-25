using System;
using System.IO;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace VeiniaFramework
{
    public class FileManager
    {
        public static bool UseEncryption = true;

        public static string ApplicationName { get; set; } = "Veinia";

        public static object Save(object objectToSave, string path, string fileName, bool appdataFolder = false, bool saveToDevWorkspace = false)
        {
            object dataToSave = JsonConvert.SerializeObject(objectToSave);

            if (UseEncryption) dataToSave = Encryption.Encrypt((string)dataToSave);

            var saveRoot = appdataFolder ? GetUserDataDirectory() : AppContext.BaseDirectory;
            var saveFilePath = ResolvePath(saveRoot, path, fileName);
            WriteData(saveFilePath, dataToSave);

#if DEBUG
            if (saveToDevWorkspace)
            {
                var devProjectWritePath = ResolvePath(GetDevelopmentWorkspaceDirectory(), path, fileName);
                WriteData(devProjectWritePath, dataToSave);
            }
#endif

            return UseEncryption ? (byte[])dataToSave : (string)dataToSave;
        }

        public static T1 Load<T1>(string path, string fileName, bool appdataFolder = false, bool allowNone = false)
        {
            var relativePath = BuildRelativePath(path, fileName);

            object dataToLoad;

            if (OperatingSystem.IsBrowser())
            {
                using (var stream = TitleContainer.OpenStream(relativePath.Replace('\\', '/')))
                {
                    if (stream == null) throw new Exception("No File Found! " + relativePath);

                    using (var reader = new StreamReader(stream))
                        dataToLoad = reader.ReadToEnd();
                }
            }
            else
            {
                var loadRoot = appdataFolder ? GetUserDataDirectory() : AppContext.BaseDirectory;
                var loadPath = ResolvePath(loadRoot, path, fileName);

                try
                {
                    dataToLoad = UseEncryption ? Encryption.Decrypt(File.ReadAllBytes(loadPath)) : File.ReadAllText(loadPath);
                }
                catch (FileNotFoundException)
                {
                    if (allowNone) return default;
                    throw;
                }
                catch (DirectoryNotFoundException)
                {
                    if (allowNone) return default;
                    throw;
                }
            }

            return JsonConvert.DeserializeObject<T1>((string)dataToLoad);
        }

        private static void WriteData(string filePath, object dataToSave)
        {
            var directory = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
            if (UseEncryption) File.WriteAllBytes(filePath, (byte[])dataToSave);
            else File.WriteAllText(filePath, (string)dataToSave);
        }

        private static string ResolvePath(string root, string path, string fileName)
        {
            var relativePath = BuildRelativePath(path, fileName);

            if (Path.IsPathRooted(relativePath)) return Path.GetFullPath(relativePath);

            return Path.GetFullPath(Path.Combine(root, relativePath));
        }

        private static string BuildRelativePath(string path, string fileName)
        {
            var combined = string.IsNullOrWhiteSpace(path) ? fileName : Path.Combine(path, fileName);

            return combined.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
        }

        private static string GetUserDataDirectory()
        {
            string baseDirectory;

            if (OperatingSystem.IsWindows())
            {
                baseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            }
            else if (OperatingSystem.IsMacOS())
            {
                baseDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Application Support");
            }
            else
            {
                var xdgDataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
                var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

                baseDirectory = !string.IsNullOrWhiteSpace(xdgDataHome) && Path.IsPathRooted(xdgDataHome) ? xdgDataHome : Path.Combine(homeDirectory, ".local", "share");
            }

            if (string.IsNullOrWhiteSpace(baseDirectory))
            {
                throw new InvalidOperationException("Could not determine a user-data directory.");
            }

            return Path.Combine(baseDirectory, ApplicationName);
        }

        private static string GetDevelopmentWorkspaceDirectory()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "Veinia.sln")) || Directory.GetFiles(directory.FullName, "*.csproj").Length > 0)
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            throw new DirectoryNotFoundException("Development workspace not found.");
        }
    }
}
