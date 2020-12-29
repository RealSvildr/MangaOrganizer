using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;

namespace MangaOrganizer {
    // TODO: v2.3 >> Descompact Winrar, Delete Winrar;
    // TODO: v2.31 >> 
    class Program {
        #region Variables
        private const string Ver = "v2.2";

        private static readonly string[] imageExtensions = { ".png", ".bmp", ".jpg", ".jpeg", ".webp" };
        private static readonly string[] ignoreExtensions = { ".exe", ".pdb" };
        private static readonly string[] removeFiles = { "gohome.png", "logo-chap.png" };
        private static readonly string[] removeFolders = { "feedback_data" };
        private static readonly string[] removeFromFolderName = { " - Manganelo_files", "'", " - ManhuaPLus_files" };
        #endregion

        static void Main(string[] args) {
            Console.Title = "Manga Organizer " + Ver;
            long timeNow = DateTime.Now.Ticks;
            //var curDir = new DirectoryInfo(@"D:\Data\Desktop\Manga");
            DirectoryInfo curDir = new DirectoryInfo(Directory.GetCurrentDirectory());

            DoFolder(curDir, true);

            Console.WriteLine("");
            Console.WriteLine("Completed in " + TimeSpan.FromTicks(DateTime.Now.Ticks - timeNow).TotalSeconds + " sec.");
            Console.ReadKey();
        }

        private static void DoFolder(DirectoryInfo curDir, bool doThisFolder = false) {
            var listDir = new List<DirectoryInfo>();

            if (doThisFolder) {
                listDir = curDir.Parent.GetDirectories().ToList();

                int index = listDir.FindIndex(p => p.Name == curDir.Name);

                GetPrevNext(listDir, index, out string prev, out string next);
                UpdateFolder(curDir, prev, next);
            }

            RemoveBadFolders(curDir, out listDir);
            for (int i = 0; i < listDir.Count; i++) {
                GetPrevNext(listDir, i, out string prev, out string next, true);
                UpdateFolder(listDir[i], prev, next);

                RemoveBadFolders(listDir[i], out List<DirectoryInfo> childDir);
                if (childDir.Count > 0)
                    DoFolder(listDir[i]);
            }
        }

        private static void GetPrevNext(List<DirectoryInfo> listDir, int curIndex, out string prev, out string next, bool format = false) {
            next = "";
            prev = "";

            if (curIndex > 0)
                prev = listDir[curIndex - 1].Name;

            if (curIndex < listDir.Count - 1)
                next = listDir[curIndex + 1].Name;


            if (format) {
                prev = NewName(prev);
                next = NewName(next);
            }
        }

        private static void UpdateFolder(DirectoryInfo folder, string prevChapter, string nextChapter) {
            List<Img> listImage = new List<Img>();

            string folderName = NewName(folder.Name);

            listImage = new List<Img>();
            foreach (var file in folder.GetFiles()) {
                if (file.Extension.ToLower().In(imageExtensions) && !file.Name.ToLower().In(removeFiles)) {
                    var fileName = NewName(file.Name, file.Extension, out int pos);

                    listImage.Add(new Img() {
                        Pos = pos,
                        Name = fileName
                    });

                    if (fileName != file.Name) {
                        file.MoveTo(file.DirectoryName + "\\" + fileName);
                    }

                } else if (file.Extension.ToLower().In(ignoreExtensions)) {
                    continue;
                } else {
                    file.Delete();
                }
            }

            listImage = listImage.OrderBy(p => p.Pos).ToList();
            if (listImage.Count > 0) {
                using (StreamWriter fS = new StreamWriter(folder.FullName + "\\Manga Reader.html")) {
                    fS.WriteLine(
                        GenerateHTML(
                            GetTitle(folder.Parent.Name, folderName),
                            listImage,
                            prevChapter,
                            nextChapter
                        )
                    );
                }
            }

            if (folderName != folder.Name) {
                folder.MoveTo(folder.Parent.FullName + "\\" + folderName);
            }

            Console.WriteLine(folder.Name + " done.");
        }

        private static void RemoveBadFolders(DirectoryInfo curDir, out List<DirectoryInfo> listDir) {
            listDir = curDir.GetDirectories().ToList();

            foreach (var dir in listDir.Where(o => o.Name.ToLower().In(removeFolders))) {
                dir.Delete(true);
            }

            listDir = curDir.GetDirectories().ToList();
        }

        private static string GetTitle(string parent, string curFolder) {
            var title = curFolder;
            var strTest = curFolder.Replace("Chapter", "").Trim();

            if (int.TryParse(strTest, out int chapter))
                title = parent + " - Chapter " + chapter;

            return title;
        }

        private static string GenerateHTML(string title, List<Img> listImage, string prevChapter, string nextChapter) {
            string htmlBase = @"<html>
    <head>
        <title>MangaReader {0} - {1}</title>
        <style>
            body {{
                background: black;
                width: 100%;
                margin: 0;
            }}

            div {{
                margin: 15px 0 30px;
                width: 100%;
            }}

            h1 {{
                color: #fff;
                text-align: center;
                font-family: cursive;
            }}

            span {{
                display: inline-block;
                text-align: center;
                width: 100%;
            }}

                span:first-child {{
                    margin-bottom: 15px;
                }}
                
                span:last-child {{
                    margin-top: 15px;
                }}

            img {{
                width: 47vw;
            }}

            a {{
                text-decoration: none;
                color: #fff;
            }}

        </style>
    </head>
    <body>
        <div>
            <h1>{1}</h1>
{2}
{3}{2}
            <script>
                var goNext = false;
                document.onkeydown = function (e) {{
                    switch (e.keyCode) {{
                        case 39:
                        case 68:
                            fnGoto('next');
                            break;
                        case 37:
                        case 65:
                            fnGoto('back');
                            break;
                    }};
                }};

                function fnGoto(cName) {{
                    if(goNext)
                        document.getElementsByClassName(cName)[0].click();
                    else {{
                        goNext = true;
                        setTimeout(() =>  goNext = false, 450);
                    }}
                }}

                document.querySelectorAll('img').forEach(e => {{
                    e.addEventListener('click', function() {{
                        var T = this;

                      if(T.style.width == '')
                        T.style.width = '72vw';
                      else if(T.style.width == '72vw')
                        T.style.width = '90vw';
                      else
                        T.style.width = '';
                    }})
                }});
            </script>
        </div>
    </body>
</html>";
            string htmlBack = @"               <a class='back' href='../{0}/Manga Reader.html'>&lt;&lt; Back</a>&emsp;&emsp;&emsp;&emsp;";
            string htmlNext = @"               <a class='next' href='../{0}/Manga Reader.html'>Next &gt;&gt;</a>";
            string htmlImage = @"           <span><img src='{0}' /></span>";

            // Back and Next at the start
            string buttons = string.Empty;
            string images = string.Empty;


            buttons += "           <span>" + Environment.NewLine;
            if (!string.IsNullOrEmpty(prevChapter)) {
                buttons += string.Format(htmlBack, prevChapter) + Environment.NewLine;
            }
            if (!string.IsNullOrEmpty(nextChapter)) {
                buttons += string.Format(htmlNext, nextChapter) + Environment.NewLine;
            }

            buttons += "           </span>";

            foreach (var img in listImage) {
                images += string.Format(htmlImage, img.Name) + Environment.NewLine;
            }

            return string.Format(htmlBase, Ver, title, buttons, images);
        }

        private static string NewName(string name)
            => NewName(name, "", out int pos);

        private static string NewName(string name, string extension, out int position) {
            if (!string.IsNullOrEmpty(extension)) {
                extension = extension.ToLower();
                name = name.ToLower().Replace(extension, "");
            }

            if (!int.TryParse(name.Split('_', ' ', '.', ',').First(), out position))
                int.TryParse(name.Split('-').Last(), out position);

            if (position == 0 && !string.IsNullOrEmpty(extension) && name.IndexOf("img_") > -1) {
                name = name.Replace("img_", "");

                int.TryParse(name.Split('-').First(), out position);
            }

            if (position > 0)
                name = position.ToString("000");

            if (!string.IsNullOrEmpty(extension))
                name += extension;

            if (position == 0 && string.IsNullOrEmpty(extension))
                foreach (var rFolderName in removeFromFolderName)
                    if (name.Contains(rFolderName))
                        name = name.Replace(rFolderName, "");

            return name;
        }

        public class Img {
            public int Pos { get; set; }
            public string Name { get; set; }
        }
    }
}

public static class String {
    public static bool In(this string value, params string[] values) {
        foreach (string v in values) {
            if (value == v) {
                return true;
            }
        }
        return false;
    }
}