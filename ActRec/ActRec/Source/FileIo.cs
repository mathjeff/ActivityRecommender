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

        public void EraseFileAndWriteContent(string fileName, string content)
        {
            StreamWriter writer = this.EraseFileAndOpenForWriting(fileName);
            writer.Write(content);
            writer.Dispose();
            if (this.ReadAllText(fileName) != content)
            {
                throw new Exception("Failed to write " + fileName);
            }
            int suffixLength = Math.Min(content.Length, 1000);
            string suffix = content.Substring(content.Length - suffixLength);
            System.Diagnostics.Debug.WriteLine("To file " + fileName + ", wrote " + content.Length + " characters ending with: " + suffix);
        }

        public string ReadAllText(string fileName)
        {
            // If the file exists, then we want to read all of its data
            StreamReader reader = this.OpenFileForReading(fileName);
            string content = reader.ReadToEnd();
            reader.Dispose();

            int suffixLength = Math.Min(content.Length, 1000);
            string suffix = content.Substring(content.Length - suffixLength);
            System.Diagnostics.Debug.WriteLine("From file " + fileName + ", read " + content.Length + " characters ending with: " + suffix);
            return content;
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
        public async Task<bool> ExportFile(string fileName, string content)
        {
            Permission[] permissions = new Permission[] { Permission.Storage };
            Dictionary<Permission, PermissionStatus> status = await Plugin.Permissions.CrossPermissions.Current.RequestPermissionsAsync(permissions);

            // print statuses for debugging
            System.Diagnostics.Debug.WriteLine("Got status for " + status.Count + " statuses ");
            foreach (Permission permission in status.Keys)
            {
                System.Diagnostics.Debug.WriteLine("Permissions[" + permission + "] = " + status[permission]);
            }

            string destDir = RootDir;
            if (!Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);
            string path = Path.Combine(destDir, fileName);
            File.WriteAllText(path, content);
            return true;
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
        public async Task<FileData> PromptUserForFile()
        {
            IFilePicker filePicker = CrossFilePicker.Current;
            return await filePicker.PickFile();
        }
    }
}
