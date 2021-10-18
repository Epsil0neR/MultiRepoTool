using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using CommandLine;

namespace PublishedZipper
{
	class Program
	{
		static void Main(string[] args)
		{
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(WithParsed)
                .WithNotParsed(WithNotParsed);
        }

        private static void WithNotParsed(IEnumerable<Error> enumerable)
        {
            Console.WriteLine("Error parsing arguments.");
            Console.Write("Press any key to exit...");
            Console.ReadKey(false);
        }

        private static void WithParsed(Options options)
        {
            var directorySource = new DirectoryInfo(options.Path);
            var directoryOutput = new DirectoryInfo(options.OutputPath);

            if (!directorySource.Exists)
                return;
            if (!directoryOutput.Exists)
                return;
            if (string.IsNullOrWhiteSpace(options.ExeName))
                return;

            IReadOnlyList<FileInfo> sources = directorySource
                .GetFiles("*.*", SearchOption.AllDirectories);

            if (options.ExtensionsToExclude?.Any() == true)
            {
                var toExclude = options.ExtensionsToExclude
                    .Select(x=>x.Trim().ToLowerInvariant())
                    .ToList();
                sources = sources
                    .Where(x => !toExclude.Contains(Path.GetExtension(x.Name)))
                    .ToList();
            }

            var exe = sources.FirstOrDefault(x => string.Equals(x.Name, options.ExeName, StringComparison.InvariantCultureIgnoreCase));
            var version = exe?.GetVersion();

            if (version is null)
                return;

            var fileName = $"{Path.GetFileNameWithoutExtension(exe.Name)} v{version}.zip";
            string path = Path.Combine(directoryOutput.FullName, fileName);
            if (File.Exists(path))
                File.Delete(path);
            using var archiveStream = File.Create(path);
            using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, false);
            foreach (var file in sources)
            {
                WriteFileToArchive(archive, file);
            }
        }

        private static void WriteFileToArchive(ZipArchive archive, FileInfo file)
        {
            var entry = archive.CreateEntry(file.Name);
            using var stream = entry.Open();
            using var reader = file.OpenRead();
            reader.CopyTo(stream);
        }
    }

    public static class Utils
    {
        public static Version GetVersion(this FileInfo fileInfo)
        {
            FileVersionInfo info = FileVersionInfo.GetVersionInfo(fileInfo.FullName);
            if (string.IsNullOrWhiteSpace(info.ProductVersion))
                return null;

            return Version.Parse(info.ProductVersion);
        }
    }
}
