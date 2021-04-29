using PCLStorage;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using Plugin.Permissions.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ActivityRecommendation
{
    // an InternalFileIo does reading and writing to files that are stored with the application
    class InternalFileIo
    {
        #region Public Member Functions
        public Boolean FileExists(string fileName)
        {
            IFile file = this.GetFile(fileName, false);
            return (file != null);
        }
        public Stream OpenFile(string fileName, PCLStorage.FileAccess fileAccess)
        {
            IFile file = this.GetFile(fileName, true);
            if (file == null)
            {
                return null;
            }
            Stream stream;
            try
            {
                Task<Stream> streamTask = Task.Run(async () => await file.OpenAsync(fileAccess));
                stream = streamTask.Result;
            }
            catch (Exception e)
            {
                string message = "exception " + e;
                System.Diagnostics.Debug.WriteLine(message);
                return null;
            }
            return stream;
        }
        private IFile GetFile(string fileName, bool createIfMissing)
        {
            IFileSystem fs = FileSystem.Current;
            IFile file = null;
            // TODO: instead of ConfigureAwait here, should we make everything function async?
            Task<ExistenceCheckResult> existenceTask = Task.Run(async () => await fs.LocalStorage.CheckExistsAsync(fileName));
            ExistenceCheckResult result = existenceTask.Result;
            if (result == ExistenceCheckResult.FileExists)
            {
                Task<IFile> contentTask = Task.Run(async () => await fs.LocalStorage.GetFileAsync(fileName));
                file = contentTask.Result;
            }
            else
            {
                if (createIfMissing)
                {
                    Task<IFile> creationTask = Task.Run(async () => await fs.LocalStorage.CreateFileAsync(fileName, CreationCollisionOption.FailIfExists));
                    file = creationTask.Result;
                }

            }

            return file;
        }
        public void AppendText(string text, string fileName)
        {
            Stream file = this.OpenFile(fileName, PCLStorage.FileAccess.ReadAndWrite);
            file.Seek(0, SeekOrigin.End);

            StreamWriter writer = new StreamWriter(file);
            writer.Write(text);
            writer.Dispose();
            file.Dispose();
        }
        public StreamWriter EraseFileAndOpenForWriting(string fileName)
        {
            IFile file = this.GetFile(fileName, false);
            if (file != null)
            {
                Task deletion = file.DeleteAsync();
                deletion.Wait();
            }
            return new StreamWriter(this.OpenFile(fileName, PCLStorage.FileAccess.ReadAndWrite));
        }
        public StreamReader OpenFileForReading(string fileName)
        {
            return new StreamReader(this.OpenFile(fileName, PCLStorage.FileAccess.Read));
        }

        public void EraseFileAndWriteContent(string fileName, TextReader content)
        {
            this.EraseFileAndWriteContent(fileName, content.ReadToEnd());
            content.Close();
            content.Dispose();
        }

        public void EraseFileAndWriteContent(string fileName, string content)
        {
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
        // saves text to a file where the user can do something with it
        public async Task<FileExportResult> ExportFile(string fileName, string content)
        {
            await this.requestPermission();
            string destDir = RootDir;
            if (!Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);
            string path = Path.Combine(destDir, fileName);
            File.WriteAllText(path, content);
            return new FileExportResult(path, content, true);
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
            IFilePicker filePicker = CrossFilePicker.Current;
            FileData fileData = await filePicker.PickFile();
            if (fileData == null)
                return null;
            return new OpenedFile(fileData.FilePath, fileData.GetStream());
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
            Permission[] permissions = new Permission[] { Permission.Storage };
            Dictionary<Permission, PermissionStatus> status = await Plugin.Permissions.CrossPermissions.Current.RequestPermissionsAsync(permissions);

            // print statuses for debugging
            System.Diagnostics.Debug.WriteLine("Got status for " + status.Count + " statuses ");
            foreach (Permission permission in status.Keys)
            {
                System.Diagnostics.Debug.WriteLine("Permissions[" + permission + "] = " + status[permission]);
            }
        }
    }

    public class FileExportResult
    {
        public FileExportResult(string path, string content, bool successful)
        {
            this.Path = path;
            this.Content = content;
            this.Successful = successful;
        }
        public string Path;
        public string Content;
        public bool Successful;
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
