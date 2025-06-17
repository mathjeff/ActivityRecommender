using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace ActivityRecommendation
{
    // an InternalFileIo does reading and writing to files that are stored with the application
    class InternalFileIo
    {
        #region Public Member Functions
        private string pathForFilename(string fileName)
        {
            return Path.Combine(FileSystem.AppDataDirectory, fileName);
        }
        public void AppendText(string text, string fileName)
        {
            String path = pathForFilename(fileName);
            System.Diagnostics.Debug.WriteLine("AppendText to " + path);
            StreamWriter writer = new StreamWriter(path, true);
            writer.Write(text);
            writer.Dispose();
        }
        public StreamWriter EraseFileAndOpenForWriting(string fileName)
        {
            String path = pathForFilename(fileName);
            System.Diagnostics.Debug.WriteLine("EraseFileAndOpenForWriting to " + path);
            return new StreamWriter(path, false);
        }
        public StreamReader OpenFileForReading(string fileName)
        {
            String path = pathForFilename(fileName);
            System.Diagnostics.Debug.WriteLine("OpenFileForReading checking existence of " + path);
            if (!File.Exists(path))
            {
                System.Diagnostics.Debug.WriteLine("OpenFileForReading creating " + path);
                FileStream stream = File.Create(path);
                System.Diagnostics.Debug.WriteLine("OpenFileForReading returning new stream for " + path);
                return new StreamReader(stream);
            }
            System.Diagnostics.Debug.WriteLine("OpenFileForReading reading existing " + path);
            StreamReader result = new StreamReader(path);
            System.Diagnostics.Debug.WriteLine("OpenFileForReading read " + path);
            return result;
        }


        public void EraseFileAndWriteContent(string fileName, TextReader content)
        {
            String path = pathForFilename(fileName);
            System.Diagnostics.Debug.WriteLine("EraseFileAndWriteContent to " + path);
            this.EraseFileAndWriteContent(path, content.ReadToEnd());
            content.Close();
            content.Dispose();
        }

        public void EraseFileAndWriteContent(string fileName, string content)
        {
            System.Diagnostics.Debug.WriteLine("EraseFileAndWriteContent " + fileName);
            StreamWriter writer = this.EraseFileAndOpenForWriting(fileName);
            writer.Write(content);
            writer.Dispose();
            StreamReader reader = this.OpenFileForReading(fileName);
            if (reader.ReadToEnd() != content)
            {
                throw new Exception("Failed to write " + fileName);
            }
            reader.Close();
            reader.Dispose();
            int suffixLength = Math.Min(content.Length, 1000);
            string suffix = content.Substring(content.Length - suffixLength);
            System.Diagnostics.Debug.WriteLine("To file " + fileName + ", wrote " + content.Length + " characters ending with: " + suffix);
        }

        #endregion

    }

    // a PublicFileIo does reading and writing of files in a shared location
    // This is used for things like data import and export
    public class PublicFileIo
    {
        private static string basedir = null;
        public static void setBasedir(string dir)
        {
            basedir = dir;
        }
        // gives a file to the user to save
        public async Task<FileShareResult> Share(string fileName, string content)
        {
            var file = Path.Combine(Microsoft.Maui.Storage.FileSystem.CacheDirectory, fileName);
            File.WriteAllText(file, content);

            await Microsoft.Maui.ApplicationModel.DataTransfer.Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = fileName,
                File = new ShareFile(file)
            });

            return new FileShareResult(content);
        }

        private string RootDir
        {
            get
            {
                if (basedir == null)
                    basedir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                return basedir;
            }
        }

        // asks the user to choose a file, asynchronously
        public async Task<OpenedFile> PromptUserForFile()
        {
            await this.requestPermission();

            FileResult fileData = await FilePicker.PickAsync();
            if (fileData == null)
                return null;
            return new OpenedFile(fileData.FullPath, await fileData.OpenReadAsync());
        }
        public async Task<OpenedFile> Open(string path)
        {
            FileStream stream = File.OpenRead(path);
            return new OpenedFile(path, stream);
        }

        public async Task<List<string>> ListDir()
        {
            await this.requestPermission();
            string[] arrayResult = Directory.GetFiles(this.RootDir);
            return new List<string>(arrayResult);
        }

        private async Task requestPermission()
        {
            Task<PermissionStatus> statusTask = Permissions.RequestAsync<Permissions.StorageRead>();
            PermissionStatus status = await statusTask;
            System.Diagnostics.Debug.WriteLine("Status of Permissions.StorageRead = " + status);
        }
    }

    public class FileShareResult
    {
        public FileShareResult(string content)
        {
            this.Content = content;
        }
        public string Content;
    }

    public class OpenedFile
    {
        public OpenedFile(string path, Stream content)
        {
            this.Path = path;
            this.Content = content;
        }
        public string Path;
        public Stream Content;
    }
}
