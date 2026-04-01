using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;

namespace VideoBrowser
{
    public class VideoFile
    {
        public string Title { get; private set; }
        public string Duration { get; private set; }
        public int? Timestamp { get; private set; }

        public string RelativeDirectory { get; private set; }
        public string RelativePath { get; private set; }
        public string RelativeImagePath { get; private set; }
        
        public bool IsVideoFile { get; private set; }
        
        public VideoFile(string basedir, string fullPath)
        {
            var fileInfo = new FileInfo(fullPath);
            RelativeDirectory = Path.GetRelativePath(basedir, fileInfo.Directory.FullName);
            RelativePath = Path.GetRelativePath(basedir, fullPath);
            
            var fullImagePath = GetImagePath(fullPath);
            if (fullImagePath != null)
            {
                RelativeImagePath = Path.GetRelativePath(basedir, fullImagePath);
            }
            
            ProcessMetadata(fullPath);
            Title ??= TitleFromFile(fileInfo);
            Timestamp ??= int.MinValue;
            Duration ??= "";
            IsVideoFile = IsVideoFilePath(fileInfo);
        }

        private string TitleFromFile(FileInfo fileInfo)
        {
            if (fileInfo == null)
            {
                return "";
            }
            
            var name = fileInfo.Name ?? "";
            if (name.Length > fileInfo.Extension.Length)
            {
                name = name.Substring(0, name.Length - fileInfo.Extension.Length);
            }
            
            return name;
        }

        private void ProcessMetadata(string fullPath)
        {
            var metadataFilename = $"{fullPath}.json";
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
                
                var duration = jsonObject["duration"]?.GetValue<int>();
                if (duration != null)
                {
                    var timespan = TimeSpan.FromSeconds(duration.Value);
                    Duration = timespan.ToString();
                    while (Duration.StartsWith("0") || Duration.StartsWith(":"))
                    {
                        Duration = Duration.Substring(1);
                    }
                }
            }
            catch (Exception e)
            {
            }
        }

        private static string[] _imageExtensions = new[] { "jpg", "webp" };
        private string GetImagePath(string fullPath)
        {
            if (!fullPath.Contains('.'))
            {
                return null;
            }
            
            var filenameNoExtension = fullPath.Substring(0, fullPath.LastIndexOf('.'));

            foreach (var name in new string[] { filenameNoExtension, fullPath })
            {
                foreach (var extension in _imageExtensions)
                {
                    var imageName = $"{name}.{extension}";
                    if (File.Exists(imageName))
                    {
                        return imageName;
                    }
                }
            }

            return null;
        }


        private static readonly List<string> _videoExtensions =
        [
            ".mkv",
            ".avi",
            ".mp4",
            ".webm"
        ];
        
        public static bool IsVideoFilePath(string fullPath)
        {
            return IsVideoFilePath(new FileInfo(fullPath));
        }
        
        private static bool IsVideoFilePath(FileInfo fileInfo)
        {
            return _videoExtensions.Any(fileInfo.Extension.ToLower().Equals)
                   && !fileInfo.Name.StartsWith("._");
        }

        public static List<VideoFile> GetVideoFilesInDirectory(string baseDirectory)
        {
            var files = GetFiles(baseDirectory)
                .Where(f => f.IsVideoFile)
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
            var files = Directory.GetFiles(dir).Select(f => new VideoFile(basedir, f));

            var allFiles = files.Union(subFiles).ToList();

            return allFiles;
        }
    }
}
