using PCLStorage;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using System;
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
        public Stream OpenFile(string fileName, FileAccess fileAccess)
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
            Stream file = this.OpenFile(fileName, FileAccess.ReadAndWrite);
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
            return new StreamWriter(this.OpenFile(fileName, FileAccess.ReadAndWrite));
        }
        public StreamReader OpenFileForReading(string fileName)
        {
            return new StreamReader(this.OpenFile(fileName, FileAccess.Read));
        }

        public void EraseFileAndWriteContent(string fileName, string content)
        {
            StreamWriter writer = this.EraseFileAndOpenForWriting(fileName);
            writer.Write(content);
            writer.Dispose();
        }

        public string ReadAllText(string fileName)
        {
            // If the file exists, then we want to read all of its data
            StreamReader reader = this.OpenFileForReading(fileName);
            string content = reader.ReadToEnd();
            reader.Dispose();
            return content;
        }

        #endregion

    }

    // a PublicFileIo does reading and writing of files in a shared location
    // This is used for things like data import and export
    public class PublicFileIo
    {
        // saves text to a file where the user can do something with it
        public bool ExportFile(string fileName, string content)
        {
            IFilePicker filePicker = CrossFilePicker.Current;
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(content);

            FileData fileData = new FileData();
            fileData.FileName = fileName;
            fileData.DataArray = bytes;
            Task<bool> task = Task.Run(async () => await filePicker.SaveFile(fileData));
            bool success = task.Result;

            return success;
        }

        // asks the user to choose a file, asynchronously
        public async Task<FileData> PromptUserForFile()
        {
            IFilePicker filePicker = CrossFilePicker.Current;
            return await filePicker.PickFile();
        }
    }
}
