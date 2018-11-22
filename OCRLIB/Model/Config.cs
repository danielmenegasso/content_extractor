using System;
using System.IO;
using System.Xml.Linq;

namespace OCRLIB.Model
{
    public class Config
    {
        // Propriedade publica do caminho da lib Tesseract
        public static string TESSERACT_PATH { get; private set; }

        // Define caminho do arquivo de log
        private static readonly string RootPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) +
                                                  @"\WebPrev\Sincronizador\";

        // Define o caminho da configuração
        private static readonly string ConfigPath = RootPath + "content_extractor_config.xml";

        public static bool SalvarConfig(string tessPath)
        {
            var output = false;

            if (string.IsNullOrEmpty(tessPath))
            {
                return output;
            }

            // Salva as configurações em um arquivo
            var doc = new XDocument();

            var body = new XElement("body");
            var userConf = new XElement("userconf");
            var tesseractPath = new XElement("tess_path", tessPath);
            // Adiciona configurações dos usuário
            userConf.Add(tesseractPath);
            // Adiciona ao corpo do XML
            body.Add(userConf);
            // Adiciona o corpo ao documento
            doc.Add(body);

            try
            {
                // Verifica se a não pasta existe
                if (!Directory.Exists(RootPath))
                {
                    Directory.CreateDirectory(RootPath);
                }
                // Salva o arquivo
                doc.Save(ConfigPath);
                output = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }

            // Retorna
            return output;
        }

        public static bool ReadConfig()
        {
            var output = false;
            try
            {
                // Verifica se o arquivo existe, para carregar as configurações
                if (File.Exists(ConfigPath))
                {
                    // Carrega o doc de config
                    var doc = XDocument.Load(ConfigPath);

                    var body = doc.Element("body");
                    // Usa null propagation para verificar se existem os elementos no XML
                    var user = body?.Element("userconf");
                    // Caso não seja null, acessa os XElements do user e preeenche as propriedades
                    if (user != null)
                    {
                        var tesseractPathXE = user.Element("tess_path");
                        if (tesseractPathXE != null) TESSERACT_PATH = tesseractPathXE.Value;
                        output = true;
                    }
                }
            }
            catch (Exception msg)
            {
                Console.WriteLine(msg);
            }

            return output;
        }
    }
}