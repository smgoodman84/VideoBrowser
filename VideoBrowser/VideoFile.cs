using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;

namespace VideoBrowser
{
    public class VideoFile
    {
        public VideoFile(string baseDirectory, string subDirectory, string file)
        {
            BaseDirectory = baseDirectory;
            SubDirectory = subDirectory;

            Filename = file.Substring(SubDirectory.Length + 1);
            Imagename = GetImageFilename();
            ProcessMetadata();
            Title ??= Filename;
            Timestamp ??= int.MinValue;
        }

        private void ProcessMetadata()
        {
            var metadataFilename = Path.Join(SubDirectory, $"{Filename}.json");
            if (!File.Exists(metadataFilename))
            {
                return;
            }

            try
            {
                var jsonContent = File.ReadAllText(metadataFilename);
                var jsonObject = JsonObject.Parse(jsonContent);
                if (jsonObject == null)
                {
                    return;
                }
                
                Title = jsonObject["title"]?.GetValue<string>();
                Timestamp = jsonObject["timestamp"]?.GetValue<int>();
            }
            catch (Exception e)
            {
            }
        }

        private static string[] _imageExtensions = new[] { "jpg", "webp" };
        private string GetImageFilename()
        {
            if (!Filename.Contains('.'))
            {
                return null;
            }
            
            var filenameNoExtension = Filename.Substring(0, Filename.LastIndexOf('.'));

            foreach (var name in new string[] { filenameNoExtension, Filename })
            {
                foreach (var extension in _imageExtensions)
                {
                    var imageName = $"{name}.{extension}";
                    if (File.Exists(Path.Join(SubDirectory, imageName)))
                    {
                        return imageName;
                    }
                }
            }

            return null;
        }
        

        public string BaseDirectory { get; set; }
        public string SubDirectory { get; set; }
        public string Filename { get; set; }
        public string Imagename { get; set; }
        public string Title { get; set; }
        public int? Timestamp { get; set; }

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
                ".mp4",
                ".webm"
            };

            return extensions.Any(filename.EndsWith) && !filename.StartsWith("._");
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
