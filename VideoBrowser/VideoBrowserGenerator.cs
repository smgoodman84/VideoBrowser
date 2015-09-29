using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VideoBrowser
{
    public class VideoBrowserGenerator
    {
        public static void GenerateFile(string basedir, string filename = "VideoBrowser.html")
        {
            var outputFile = basedir + "\\" + filename;
            var fileContent = GenerateFile(basedir);

            File.WriteAllText(outputFile, fileContent);
        }

        private static string GenerateFile(string basedir)
        {
            var htmlTemplate = File.ReadAllText(@"Content\html\template.html");

            var jsData = AsJavascriptElement(GetJavascriptData(basedir));
            var jsCode = AsJavascriptElement(File.ReadAllText(@"Content\js\videobrowser.js").Substring(1));
            var js = jsData + jsCode;

            var fontAwesome = GetCssElement(@"Content\css\font-awesome.min.css");
            var styles = GetCssElement(@"Content\css\styles.css");
            var css = fontAwesome + styles;

            return string.Format(htmlTemplate, css, js);
        }

        private static string GetJavascriptData(string basedir)
        {
            var files = VideoFile.GetVideoFilesInDirectory(basedir).OrderBy(x => x.Fullpath);
            var directories = GetDirectories(basedir).OrderBy(x => x);

            var sb = new StringBuilder();
            sb.AppendLine("var filedata = [];");
            sb.AppendLine("var directories = [];");
            sb.AppendLine(string.Format(@"var basedir =""{0}"";", basedir.Replace("\\", "\\\\")));

            var i = 0;
            foreach (var file in files)
            {
                sb.AppendLine(string.Format(@"filedata[{0}] = new Object();", i));
                sb.AppendLine(string.Format(@"filedata[{0}].subdir = ""{1}"";", i, file.SubDirectory.Replace("\\", "\\\\")));
                sb.AppendLine(string.Format(@"filedata[{0}].filename = ""{1}"";", i, file.Filename));
                i++;
            }

            i = 0;
            foreach (var dir in directories)
            {
                sb.AppendLine(string.Format(@"directories[{0}] = new Object();", i));
                sb.AppendLine(string.Format(@"directories[{0}].parent = ""{1}"";", i, ParentDir(dir).Replace("\\", "\\\\")));
                sb.AppendLine(string.Format(@"directories[{0}].name = ""{1}"";", i, dir.Replace("\\", "\\\\")));
                i++;
            }

            return sb.ToString();
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
            return dir.Substring(0, dir.LastIndexOf('\\'));
        }

        private static bool ContainsVideoFile(string dir)
        {
            var files = Directory.GetFiles(dir);
            return files.Any(VideoFile.IsVideoFile);
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
