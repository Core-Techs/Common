using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace CoreTechs.Common
{
    public static class FileSystemExtensions
    {
        public static IEnumerable<byte> EnumerateBytes(this FileInfo file)
        {
            if (file == null) throw new ArgumentNullException("file");

            using (var stream = file.OpenRead())
            {
                foreach (var b in stream.EnumerateBytes())
                    yield return b;

                stream.Close();
            }
        }

        public static FileSystemInfo CreateFileSystemInfoFromPath(this string path)
        {
            return path.IsDirectoryPath() ? (FileSystemInfo)new DirectoryInfo(path) : new FileInfo(path);
        }

        public static bool IsDirectoryPath(this string path)
        {
            if (path == null) throw new ArgumentNullException("path");
            path = path.Trim();

            if (Directory.Exists(path))
                return true;

            if (System.IO.File.Exists(path))
                return false;

            // neither file nor directory exists. guess intention

            // if has trailing slash then it's a directory
            if (new[] { "\\", "/" }.Any(x => path.EndsWith(x)))
                return true; // ends with slash

            // has if extension then its a file; directory otherwise
            return string.IsNullOrWhiteSpace(Path.GetExtension(path));
        }

        public static bool IsFilePath(this string path)
        {
            return !path.IsDirectoryPath();
        }

        public static FileInfo GetFile(this DirectoryInfo directory, string relativeFilePath)
        {
            return new FileInfo(Path.Combine(directory.FullName, relativeFilePath));
        }

        /// <summary>
        /// Attempts to open a file. 
        /// Immediately closes the file if successful. 
        /// Swallows (but returns) any exception encountered while trying to open the file.
        /// </summary>
        /// <param name="file">The file to attempt to open.</param>
        /// <param name="fileAccess">The access required to open the file. Defaults to ReadWrite.</param>
        /// <param name="fileShare">The access granted to others trying to manipulate the file. Defaults to none.</param>
        /// <returns>True if the file could be opened. False otherwise.</returns>
        public static Attempt AttemptOpen(this FileInfo file, FileAccess fileAccess = FileAccess.ReadWrite,
            FileShare fileShare = FileShare.None)
        {
            return Attempt.Do(() =>
            {
                using (var r = file.Open(FileMode.Open, fileAccess, fileShare))
                    r.Close();
            });
        }

        public static byte[] ComputeHash(this Stream stream, HashAlgorithm algorithm = null)
        {
            algorithm = algorithm ?? SHA1.Create();
            return algorithm.ComputeHash(stream);
        }

        public static string ComputeFileHash(this FileInfo fileInfo, HashAlgorithm algorithm = null)
        {
            using (var stream = fileInfo.OpenRead())
                return stream.ComputeHash(algorithm).ConvertToString();
        }

        public static string ConvertToString(this byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "");
        }

        /// <summary>
        /// Checks and rechecks a directory for files. Only returns files that haven't changed between checks.
        /// </summary>
        /// <param name="di">The directory to check.</param>
        /// <param name="interval">The time between checking for files.</param>
        /// <param name="searchPattern">The search pattern.</param>
        /// <param name="searchOption">The search options.</param>
        /// <param name="cancellationToken">Token to cancel the wait between checks.</param>
        /// <returns>Files that haven't changed during the interval.</returns>
        public static FileInfo[] PollForFiles(this DirectoryInfo di, TimeSpan interval, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var e = di.EnumerateFiles(searchPattern, searchOption)
                .Select(f => new
                {
                    FileInfo = f,
                    HashAttempt = Attempt.Get(() => f.ComputeFileHash())
                })
                .Where(f => f.HashAttempt.Succeeded);

            // ReSharper disable PossibleMultipleEnumeration
            var files = e.ToArray();
            try
            {
                Task.Delay(interval, cancellationToken)
                    // ReSharper disable once MethodSupportsCancellation
                    .Wait();
            }
            catch (AggregateException ex)
            {
                ex.Handle(x => x is OperationCanceledException);
            }
            cancellationToken.ThrowIfCancellationRequested();
            return e.Join(files, x => x.FileInfo.FullName, x => x.FileInfo.FullName, Tuple.Create)
                .Where(x =>
                {
                    try
                    {
                        return x.Item1.FileInfo.Length == x.Item2.FileInfo.Length
                               && x.Item1.FileInfo.LastWriteTimeUtc == x.Item2.FileInfo.LastWriteTimeUtc
                               && x.Item1.HashAttempt.Value == x.Item2.HashAttempt.Value;
                    }
                    catch (IOException)
                    {
                        return false;
                    }
                })
                .Select(x => x.Item1.FileInfo).ToArray();
            // ReSharper restore PossibleMultipleEnumeration
        }

        /// <summary>
        /// Checks and rechecks a directory and all sub-directories for files. Only returns files that haven't changed between checks.
        /// </summary>
        /// <param name="di">The directory to check.</param>
        /// <param name="interval">The time between checking for files.</param>
        /// <param name="cancellationToken">Token to cancel the wait between checks.</param>
        /// <returns>Files that haven't changed during the interval.</returns>
        public static FileInfo[] PollForAllFiles(this DirectoryInfo di, TimeSpan interval, CancellationToken cancellationToken = default(CancellationToken))
        {
            return di.PollForFiles(interval, searchOption: SearchOption.AllDirectories, cancellationToken: cancellationToken);
        }

        public static IEnumerable<FileInfo> WhereCanOpen(this IEnumerable<FileInfo> files)
        {
            if (files == null) throw new ArgumentNullException("files");
            return files.Where(f => f.AttemptOpen().Succeeded);
        }


        public static byte[] ComputeHash(this byte[] bytes, HashAlgorithm algorithm = null)
        {
            using (var stream = new MemoryStream(bytes))
                return ComputeHash(stream, algorithm);
        }

        public static bool AreSame(this DirectoryInfo a, DirectoryInfo b)
        {
            if (!a.Name.Equals(b.Name, StringComparison.OrdinalIgnoreCase))
                return false;

            if (a.Parent == null && b.Parent == null)
                return true;

            if (a.Parent == null || b.Parent == null)
                return false;

            return a.Parent.FullName.Equals(b.Parent.FullName, StringComparison.OrdinalIgnoreCase);
        }

        public static string GetRelativePathFrom(this FileSystemInfo to, FileSystemInfo from)
        {
            return from.GetRelativePathTo(to);
        }

        public static string GetRelativePathTo(this FileSystemInfo from, FileSystemInfo to)
        {
            Func<FileSystemInfo, string> getPath = fsi =>
            {
                var d = fsi as DirectoryInfo;
                return d == null ? fsi.FullName : d.FullName.TrimEnd('\\') + "\\";
            };

            var fromPath = getPath(from);
            var toPath = getPath(to);

            var fromUri = new Uri(fromPath);
            var toUri = new Uri(toPath);

            var relativeUri = fromUri.MakeRelativeUri(toUri);
            var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            return relativePath.Replace('/', Path.DirectorySeparatorChar);
        }
    }
}