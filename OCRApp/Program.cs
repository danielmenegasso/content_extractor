using System;
using System.Collections.Generic;
using ZetaLongPaths;
using OCRApp.Database;
using OCRApp.Model;
using System.Threading;

namespace OCRApp
{
    class Program
    {
        private const string TesseractPath = @"C:\Program Files (x86)\Tesseract-OCR";
        private static string[] _language = new[] { "por" };
        private static DataBase _database = new DataBase();
        private static List<Arquivos> listArquivos;
        private static int _counter = 1;
        private static bool _run = true;

        static void Main(string[] args)
        {

            ConsoleKeyInfo cki;

            Console.Clear();

            // Establish an event handler to process key press events.
            Console.CancelKeyPress += new ConsoleCancelEventHandler((obj, evtArgs) =>
            {
                evtArgs.Cancel = true;
                _run = false;
            });

            while (_run)
            {
                listArquivos = _database.GetArquivosSincronizador();

                Console.WriteLine(DateTime.Now + " - " + $"OCR de arquivos GED_FILA, total:{listArquivos?.Count}");

                //Console.ReadKey();

                var tmp = _database.GetArquivosEntidade(listArquivos, 1);

                if(tmp != null) {

                    foreach (var item in tmp)
                    {
                        // Caso ctrl + C, então retorna
                        if (_run == false) break;


                        Console.WriteLine(DateTime.Now + " - " + $"OCR de arquivo: {_counter} de :{listArquivos.Count}");
                        var ext = ZlpFileInfo.FromString(item.CaminhoArquivo).Extension;
                        if (ext == ".pdf")
                        {
                            item.DataProcInicio = DateTime.Now;
                            Console.WriteLine(item.CaminhoArquivo);
                            TesseractOCR.ParseText(TesseractPath, item, _language);
                            item.DataProFim = DateTime.Now;
                            // Salva o OCR
                            var result = _database.SalvarConteudo(item);
                            var resultGedFila = _database.UpdateGedFila(item);
                            if (result == false || resultGedFila == false)
                            {
                                Console.WriteLine(DateTime.Now + " - " + $"Falha ao salvar OCR do arquivo {item.CaminhoArquivo}");
                            }
                            else
                            {
                                Console.WriteLine(DateTime.Now + " - " + $"Captura de OCR salva, arquivo {item.CaminhoArquivo}");
                            }
                        }
                        else if (ext == ".docx")
                        {
                            item.DataProcInicio = DateTime.Now;
                            Console.WriteLine(item.CaminhoArquivo);
                            TesseractOCR.ParseTextDocx(item);
                            item.DataProFim = DateTime.Now;
                            // Salva o OCR
                            var result = _database.SalvarConteudo(item);
                            var resultGedFila = _database.UpdateGedFila(item);
                            if (result == false || resultGedFila == false)
                            {
                                Console.WriteLine(DateTime.Now + " - " + $"Falha ao salvar OCR do arquivo {item.CaminhoArquivo}");
                            }
                            else
                            {
                                Console.WriteLine(DateTime.Now + " - " + $"Captura de OCR salva, arquivo {item.CaminhoArquivo}");
                            }

                        }
                        else if (ext == ".xls" || ext == ".xlsx")
                        {
                            item.DataProcInicio = DateTime.Now;
                            Console.WriteLine(item.CaminhoArquivo);

                            if (ext == ".xls")
                            {
                                TesseractOCR.ParseTextXls(item);
                            }
                            else
                            {
                                TesseractOCR.ParseTextXlsx(item);
                            }

                            item.DataProFim = DateTime.Now;
                            // Salva o OCR
                            var result = _database.SalvarConteudo(item);
                            var resultGedFila = _database.UpdateGedFila(item);
                            if (result == false || resultGedFila == false)
                            {
                                Console.WriteLine(DateTime.Now + " - " + $"Falha ao salvar OCR do arquivo {item.CaminhoArquivo}");
                            }
                            else
                            {
                                Console.WriteLine(DateTime.Now + " - " + $"Captura de OCR salva, arquivo {item.CaminhoArquivo}");
                            }

                        }
                        else if (ext == ".txt")
                        {
                            item.DataProcInicio = DateTime.Now;
                            Console.WriteLine(item.CaminhoArquivo);
                            TesseractOCR.ParseTextTxt(item);
                            item.DataProFim = DateTime.Now;
                            // Salva o OCR
                            var result = _database.SalvarConteudo(item);
                            var resultGedFila = _database.UpdateGedFila(item);
                            if (result == false || resultGedFila == false)
                            {
                                Console.WriteLine(DateTime.Now + " - " + $"Falha ao salvar OCR do arquivo {item.CaminhoArquivo}");
                            }
                            else
                            {
                                Console.WriteLine(DateTime.Now + " - " + $"Captura de OCR salva, arquivo {item.CaminhoArquivo}");
                            }
                        }
                        else if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".gif" || ext == ".tiff")
                        {
                            item.DataProcInicio = DateTime.Now;
                            Console.WriteLine(item.CaminhoArquivo);
                            TesseractOCR.ParseTextImage(TesseractPath, item, _language);
                            item.DataProFim = DateTime.Now;
                            // Salva o OCR
                            var result = _database.SalvarConteudo(item);
                            var resultGedFila = _database.UpdateGedFila(item);
                            if (result == false || resultGedFila == false)
                            {
                                Console.WriteLine(DateTime.Now + " - " + $"Falha ao salvar OCR do arquivo {item.CaminhoArquivo}");
                            }
                            else
                            {
                                Console.WriteLine(DateTime.Now + " - " + $"Captura de OCR salva, arquivo {item.CaminhoArquivo}");
                            }
                        }
                        else
                        {
                            Console.WriteLine(DateTime.Now + " - " + $"Captura de OCR da extensão: {ext} ainda não suportada, pulando arquivo...");
                        }
                        _counter++;
                    }
                }

                Console.WriteLine(DateTime.Now + " - OCR de arquivos finalizado, proxima execução será em 1 minuto...");
                Thread.Sleep(TimeSpan.FromMinutes(1));
            }

            Console.WriteLine(DateTime.Now + " - OCR dos arquivos no GED_FILA finalizado! Pressione qualquer tecla para sair...");

            Console.ReadKey();
        }

    }
}
