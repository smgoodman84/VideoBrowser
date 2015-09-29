using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VideoBrowser
{
    public class VideoFile
    {
        public VideoFile(string baseDirectory, string subDirectory, string file)
        {
            BaseDirectory = baseDirectory;
            SubDirectory = subDirectory;

            Filename = file.Substring(SubDirectory.Length + 1);
        }

        public string BaseDirectory { get; set; }
        public string SubDirectory { get; set; }
        public string Filename { get; set; }

        public string Fullpath
        {
            get { return SubDirectory + "\\" + Filename; }
        }

        public static bool IsVideoFile(string filename)
        {
            var extensions = new List<string>()
            {
                ".mkv",
                ".avi",
                ".mp4"
            };

            return extensions.Any(filename.EndsWith);
        }
        private bool IsVideoFile()
        {
            return IsVideoFile(Filename);
        }

        public static List<VideoFile> GetVideoFilesInDirectory(string baseDirectory)
        {
            var files = GetFiles(baseDirectory)
                .Where(f => f.IsVideoFile())
                .ToList();

            return files;
        }

        private static List<VideoFile> GetFiles(string basedir, string dir = null)
        {
            if (dir == null)
            {
                dir = basedir;
            }

            var subFiles = Directory.GetDirectories(dir).SelectMany(sd => GetFiles(basedir, sd));
            var files = Directory.GetFiles(dir).Select(f => new VideoFile(basedir, dir, f));

            var allFiles = files.Union(subFiles).ToList();

            return allFiles;
        }
    }
}
