using System;
using System.Drawing;
using System.IO;
using System.Text;
using MuPDF.NET;

namespace DocToMd
{
    public enum ExtractionLevel
    {
        Minimal,
        Default,
        Full
    }

    public static class MuPdfConverter
    {
        public static void Convert(string filePath, string parentOutputDir, ExtractionLevel level, int minImageSize = 50)
        {
            using var doc = new Document(filePath);
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
            string safeName = MakeValidFileName(fileNameWithoutExt);

            // Create a dedicated folder for this document
            string docOutputDir = Path.Combine(parentOutputDir, safeName);
            Directory.CreateDirectory(docOutputDir);

            string outMdPath = Path.Combine(docOutputDir, safeName + ".md");

            switch (level)
            {
                case ExtractionLevel.Minimal:
                    ConvertMinimal(doc, safeName, outMdPath);
                    break;
                case ExtractionLevel.Default:
                    ConvertDefault(doc, safeName, outMdPath);
                    break;
                case ExtractionLevel.Full:
                    ConvertFull(doc, safeName, docOutputDir, outMdPath, minImageSize);
                    break;
            }
        }

        private static void ConvertMinimal(Document doc, string safeName, string outMdPath)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"# {safeName}");
            sb.AppendLine();

            int totalPages = doc.PageCount;
            for (int i = 0; i < totalPages; i++)
            {
                using var page = doc[i];
                var text = page.GetText();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    sb.AppendLine(text.Trim());
                    sb.AppendLine();
                }
            }

            File.WriteAllText(outMdPath, sb.ToString());
        }

        private static void ConvertDefault(Document doc, string safeName, string outMdPath)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"# {safeName}");
            sb.AppendLine();

            int totalPages = doc.PageCount;
            for (int i = 0; i < totalPages; i++)
            {
                using var page = doc[i];
                sb.AppendLine($"## Page {i + 1}");
                sb.AppendLine();

                var text = page.GetText();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    sb.AppendLine(text.Trim());
                    sb.AppendLine();
                }

                sb.AppendLine("---");
                sb.AppendLine();
            }

            File.WriteAllText(outMdPath, sb.ToString());
        }

        private static void ConvertFull(Document doc, string safeName, string docOutputDir, string outMdPath, int minImageSize)
        {
            int totalPages = doc.PageCount;
            var sb = new StringBuilder();

            sb.AppendLine($"# {safeName}");
            sb.AppendLine();

            int globalImageCount = 0;

            for (int i = 0; i < totalPages; i++)
            {
                using var page = doc[i];
                int pageNum = i + 1;

                sb.AppendLine($"## Page {pageNum}");
                sb.AppendLine();

                var text = page.GetText();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    sb.AppendLine(text.Trim());
                    sb.AppendLine();
                }

                var images = page.GetImages();
                if (images != null && images.Count > 0)
                {
                    bool hasValidImagesHeader = false;

                    foreach (var imgInfo in images)
                    {
                        try
                        {
                            int xref = imgInfo.Xref;
                            using var pixmap = new Pixmap(doc, xref);

                            if (pixmap != null && pixmap.Samples != null)
                            {
                                int width = pixmap.Width;
                                int height = pixmap.Height;

                                // Apply user-defined dimension filters
                                if (width < minImageSize || height < minImageSize) continue;

                                // Apply aspect ratio filter to skip extreme divider lines
                                double aspectRatio = (double)width / height;
                                if (aspectRatio > 10.0 || aspectRatio < 0.1) continue;

                                if (!hasValidImagesHeader)
                                {
                                    sb.AppendLine("### Diagrams & Figures");
                                    sb.AppendLine();
                                    hasValidImagesHeader = true;
                                }

                                globalImageCount++;
                                string imgFileName = $"img_{globalImageCount}.png";
                                string imgPath = Path.Combine(docOutputDir, imgFileName);

                                pixmap.Save(imgPath);

                                // Direct filename reference since the .md and images live side-by-side
                                sb.AppendLine($"![Diagram {globalImageCount}]({imgFileName})");
                                sb.AppendLine();
                            }
                        }
                        catch
                        {
                            // Skip non-extractable types gracefully
                        }
                    }
                }

                sb.AppendLine("---");
                sb.AppendLine();
            }

            File.WriteAllText(outMdPath, sb.ToString());
        }

        private static string MakeValidFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }
            return name;
        }
    }
}