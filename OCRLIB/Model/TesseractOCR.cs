using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.XWPF.UserModel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace OCRLIB.Model
{
    public class TesseractOCR
    {
        private const string _tessPath = "";

        public static void ParseText(string tesseractPath, Arquivos file, params string[] lang)
        {
            string output = string.Empty;
            // Create new output file for text
            var tempOutputFile = Path.GetTempPath() + Guid.NewGuid();
            // Create new tmp file for Image convertion
            //string[] tempImageFiles;
            // Path of tmp image file created by imagick
            var pageCount = "";
            IEnumerable<string> tmpImageFiles = null;

            try
            {
                using (var webClient = new WebClient())
                {
                    //webClient.Headers[HttpRequestHeader.ContentType] = "text/plain";
                    webClient.Encoding = Encoding.UTF8;
                    var values = new NameValueCollection();
                    values["arquivo"] = file.CaminhoArquivo;//;

                    var response = webClient.UploadValues("http://localhost/webprev/servicos/php_imagick", values);

                    var responseString = Encoding.UTF8.GetString(response);

                    pageCount = responseString;
                }

                if (string.IsNullOrEmpty(pageCount)) return;


                file.QtdPaginas = int.Parse(pageCount);

                tmpImageFiles = Directory.GetFiles(@"C:\Windows\Temp", "*.jpg")
                    .Where(s => s.Contains("converted"));

                // Cria arquivo tmp com os nomes das imagens
                File.WriteAllLines(@"C:\Windows\Temp\input.txt", tmpImageFiles);

                ProcessStartInfo info = new ProcessStartInfo();
                info.WorkingDirectory = tesseractPath;
                info.WindowStyle = ProcessWindowStyle.Hidden;
                info.UseShellExecute = false;
                info.FileName = "cmd.exe";
                info.Arguments =
                    "/c tesseract.exe " +
                    // Image file.
                    @"C:\Windows\Temp\input.txt" + " " +
                    // Output file (tesseract add '.txt' at the end)
                    tempOutputFile +
                    // Languages.
                    " -l " + string.Join("+", lang);

                // Start tesseract.
                Process process = Process.Start(info);
                process.WaitForExit();
                if (process.ExitCode == 0)
                {
                    // Exit code: success.
                    output = File.ReadAllText(tempOutputFile + ".txt");
                    // Salva o conteudo na string conteudo
                    file.Conteudo = output;
                }
                else
                {
                    //throw new Exception("Error. Tesseract stopped with an error code = " + process.ExitCode);
                }
            }
            catch (Exception msg)
            {
                Console.WriteLine(msg);
            }
            finally
            {
                //File.Delete(tempImageFile);
                if (File.Exists(tempOutputFile + ".txt"))
                {
                    File.Delete(tempOutputFile + ".txt");
                }

                if (tmpImageFiles != null)
                {
                    foreach (var img in tmpImageFiles)
                    {
                        File.Delete(img);
                    }
                }
            }
        }

        public static void ParseTextImage(string tesseractPath, Arquivos file, params string[] lang)
        {
            string output = string.Empty;

            var tempOutputFile = Path.GetTempPath() + Guid.NewGuid();

            try
            {
                ProcessStartInfo info = new ProcessStartInfo();
                info.WorkingDirectory = tesseractPath;
                info.WindowStyle = ProcessWindowStyle.Hidden;
                info.UseShellExecute = false;
                info.FileName = "cmd.exe";
                info.Arguments =
                    "/c tesseract.exe " +
                    // Image file.
                    $"\"{file.CaminhoArquivo}\"" + " " +
                    // Output file (tesseract add '.txt' at the end)
                    $"\"{tempOutputFile}\"" +
                    // Languages.
                    " -l " + string.Join("+", lang);

                // Start tesseract.
                Process process = Process.Start(info);
                process.WaitForExit();
                if (process.ExitCode == 0)
                {
                    // Exit code: success.
                    output = File.ReadAllText(tempOutputFile + ".txt");
                    // Salva o conteudo na string conteudo
                    file.Conteudo = output;
                    file.QtdPaginas = 1;
                }
                else
                {
                    //throw new Exception("Error. Tesseract stopped with an error code = " + process.ExitCode);
                }
            }
            catch (Exception msg)
            {
                Console.WriteLine(msg);
            }
            finally
            {
                if (File.Exists(tempOutputFile + ".txt"))
                {
                    File.Delete(tempOutputFile + ".txt");
                }
            }
        }

        public static void ParseTextDocx(Arquivos file)
        {
            XWPFDocument doc;

            using (FileStream fl = new FileStream(file.CaminhoArquivo, FileMode.Open, FileAccess.Read))
            {
                doc = new XWPFDocument(fl);
            }

            var extractor = new NPOI.XWPF.Extractor.XWPFWordExtractor(doc);

            file.Conteudo = extractor.Text;
            file.QtdPaginas = doc.GetProperties().ExtendedProperties.Pages;
        }

        public static void ParseTextXlsx(Arquivos file)
        {
            XSSFWorkbook hssfwb;
            using (FileStream fl = new FileStream(file.CaminhoArquivo, FileMode.Open, FileAccess.Read))
            {
                hssfwb = new XSSFWorkbook(fl);
            }

            var extractor = new NPOI.XSSF.Extractor.XSSFExcelExtractor(hssfwb);

            file.Conteudo = extractor.Text;
            file.QtdPaginas = hssfwb.Count;
        }

        public static void ParseTextXls(Arquivos file)
        {
            HSSFWorkbook hssfwb;
            using (FileStream fl = new FileStream(file.CaminhoArquivo, FileMode.Open, FileAccess.Read))
            {
                hssfwb = new HSSFWorkbook(fl);
            }

            //hssfwb.getShe
            //Assign the sheet

            var extractor = new NPOI.HSSF.Extractor.ExcelExtractor(hssfwb);

            file.Conteudo = extractor.Text;
            file.QtdPaginas = hssfwb.Count;
        }

        public static void ParseTextTxt(Arquivos file)
        {
            file.Conteudo = File.ReadAllText(file.CaminhoArquivo);
            file.QtdPaginas = 1;
        }
    }
}
