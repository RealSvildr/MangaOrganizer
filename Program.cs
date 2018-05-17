using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MangaOrganizer {
    // TODO: v2.3 >> Descompact Winrar, Delete Winrar;
    // TODO: v2.31 >> 
    class Program {
        private const string Ver = "v2.2";

        static void Main(string[] args) {
            Console.Title = "Manga Organizer " + Ver;
            long timeNow = DateTime.Now.Ticks;
            DirectoryInfo curDir = new DirectoryInfo(Directory.GetCurrentDirectory());

            if (true) {
                List<DirectoryInfo> listDir = curDir.Parent.GetDirectories().ToList();
                int index = listDir.FindIndex(p => p.Name == curDir.Name);
                string prevChapter = index > 0 ? listDir[index - 1].Name : "";
                string nextChapter = index < listDir.Count - 1 ? listDir[index + 1].Name : "";

                UpdateFolder(curDir, prevChapter, nextChapter);
            }

            for (int i = 0; i < curDir.GetDirectories().Length; i++) {
                string prevChapter = i > 0 ? NewName(curDir.GetDirectories()[i - 1].Name) : "";
                string nextChapter = i < curDir.GetDirectories().Length - 1 ? NewName(curDir.GetDirectories()[i + 1].Name) : "";

                UpdateFolder(curDir.GetDirectories()[i], prevChapter, nextChapter);
            }

            Console.WriteLine("");
            Console.WriteLine("Completed in " + TimeSpan.FromTicks(DateTime.Now.Ticks - timeNow).TotalSeconds + " sec.");
            Console.ReadKey();
        }

        private static void UpdateFolder(DirectoryInfo folder, string prevChapter, string nextChapter) {
            List<Img> listImage = new List<Img>();

            string newName = NewName(folder.Name);

            listImage = new List<Img>();
            foreach (var file in folder.GetFiles()) {
                if (file.Extension.ToLower().In(".png", ".bmp", ".jpg", ".jpeg", ".webp")) {
                    int Pos = 0;
                    int.TryParse(file.Name.Split('_').Last().Replace(file.Extension, ""), out Pos);
                    string Name = Pos > 0 ? Pos.ToString("00") + file.Extension : file.Name;

                    listImage.Add(new Img() {
                        Pos = Pos,
                        Name = Name
                    });

                    if (Name != file.Name) {
                        file.MoveTo(file.DirectoryName + "\\" + Name);
                    }
                }
            }

            listImage = listImage.OrderBy(p => p.Pos).ToList();
            if (listImage.Count > 0) {
                using (StreamWriter fS = new StreamWriter(folder.FullName + "\\Manga Reader.html")) {
                    fS.WriteLine(GenerateHTML(folder.Parent.Name + " " + newName, listImage, prevChapter, nextChapter));
                }
            }

            if (newName != folder.Name) {
                folder.MoveTo(folder.Parent.FullName + "\\" + newName);
            }

            Console.WriteLine(folder.Name + " done.");
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
                min-width: 950px;
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
                document.onkeydown = function (e) {{
                    switch (e.keyCode) {{
                        case 39:
                        case 68:
                            document.getElementsByClassName('next')[0].click();
                            break;
                        case 37:
                        case 65:
                            document.getElementsByClassName('back')[0].click();
                            break;
                    }};
                }};
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

        private static string NewName(string name) {
            int Chapter = 0;

            int.TryParse(name.Split('_', ' ', '.', ',').Last(), out Chapter);

            if (Chapter > 0) {
                return Chapter.ToString("000");
            }

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