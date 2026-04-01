using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace VideoBrowser
{
    public class VideoBrowserGenerator
    {
        public static void GenerateFile(
            string basedir, 
            string filename = "VideoBrowser.html")
        {
            var outputFile = Path.Join(basedir, filename);
            var contentRoot = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Content");
            var fileContent = GenerateFileInternal(basedir, contentRoot);

            File.WriteAllText(outputFile, fileContent);
            Console.WriteLine($"VideoBrowser generated {outputFile}");
        }

        private static string GenerateFileInternal(string basedir, string contentRoot)
        {
            var htmlTemplate = File.ReadAllText(Path.Join(contentRoot, "html", "template.html"));

            var jsData = AsJavascriptElement(GetJavascriptData(basedir));
            var jsCode = AsJavascriptElement(File.ReadAllText(Path.Join(contentRoot, "js", "videobrowser.js")).Substring(1));
            var js = jsData + jsCode;

            var fontAwesome = GetCssElement(Path.Join(contentRoot, "css", "font-awesome.min.css"));
            var styles = GetCssElement(Path.Join(contentRoot, "css", "styles.css"));
            var css = fontAwesome + styles;

            return string.Format(htmlTemplate, css, js);
        }

        private static string GetJavascriptData(string basedir)
        {
            var files = VideoFile.GetVideoFilesInDirectory(basedir)
                .ToList();
            
            var directories = GetDirectories(basedir)
                .OrderBy(x => x)
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine("var filedata = [];");
            sb.AppendLine("var directories = [];");

            var orderedFiles = files
                .OrderByDescending(f => f.Timestamp ?? int.MinValue)
                .ThenBy(f => f.Title ?? f.Title);
            
            var i = 0;
            foreach (var file in orderedFiles)
            {
                var uploadDate = "";
                if (file.Timestamp.HasValue)
                {
                    var dateTime = DateTimeOffset.FromUnixTimeSeconds(file.Timestamp.Value);
                    uploadDate = dateTime.ToString("dd MMM yy");
                }
                
                sb.AppendLine(string.Format(@"filedata[{0}] = new Object();", i));
                sb.AppendLine(string.Format(@"filedata[{0}].subdir = ""{1}"";", i, JsStringEscape(file.RelativeDirectory)));
                sb.AppendLine(string.Format(@"filedata[{0}].filename = ""{1}"";", i, UrlEncode(JsStringEscape(file.RelativePath))));
                sb.AppendLine(string.Format(@"filedata[{0}].imagename = ""{1}"";", i, UrlEncode(JsStringEscape(file.RelativeImagePath))));
                sb.AppendLine(string.Format(@"filedata[{0}].title = ""{1}"";", i, JsStringEscape(file.Title)));
                sb.AppendLine(string.Format(@"filedata[{0}].duration = ""{1}"";", i, JsStringEscape(file.Duration)));
                sb.AppendLine(string.Format(@"filedata[{0}].uploadDate = ""{1}"";", i, JsStringEscape(uploadDate)));
                i++;
            }

            i = 0;
            foreach (var dir in directories)
            {
                var relativeParent = Path.GetRelativePath(basedir, ParentDir(dir));
                var relativeName = Path.GetRelativePath(basedir, dir);

                if (relativeParent == ".")
                {
                    relativeParent = "";
                }
                
                sb.AppendLine(string.Format(@"directories[{0}] = new Object();", i));
                sb.AppendLine(string.Format(@"directories[{0}].parent = ""{1}"";", i, JsStringEscape(relativeParent)));
                sb.AppendLine(string.Format(@"directories[{0}].name = ""{1}"";", i, relativeName));
                i++;
            }

            return sb.ToString();
        }

        private static string JsStringEscape(string input)
        {
            return input?
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"");
        }

        private static string UrlEncode(string input)
        {
            return input?
                .Replace(" ", "%20")
                .Replace("#", "%23");
        }

        private static IEnumerable<string> GetDirectories(string dir, bool includeSelf = false)
        {
            var recursiveVideoFile = false;

            foreach (var subdir in Directory.GetDirectories(dir).SelectMany(d => GetDirectories(d, true)))
            {
                recursiveVideoFile = true;
                yield return subdir;
            }

            if (includeSelf && (ContainsVideoFile(dir) || recursiveVideoFile))
            {
                yield return dir;
            }
        }

        private static string ParentDir(string dir)
        {
            return Directory.GetParent(dir)?.FullName;
        }

        private static bool ContainsVideoFile(string dir)
        {
            var files = Directory.GetFiles(dir);
            return files.Any(VideoFile.IsVideoFilePath);
        }

        public static string GetCssElement(string cssFilename)
        {
            var css = File.ReadAllText(cssFilename);

            return string.Format("<style media=\"screen\" type=\"text/css\">{0}{1}{0}</style>",
                Environment.NewLine,
                css);
        }

        public static string AsJavascriptElement(string javascript)
        {
            return string.Format("<script>{0}{1}{0}</script>",
                Environment.NewLine,
                javascript);
        }
    }
}
