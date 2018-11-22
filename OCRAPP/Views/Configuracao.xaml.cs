using OCRLIB.Model;
using System.Windows;
using System.Windows.Forms;
using static System.String;
using MessageBox = System.Windows.MessageBox;

namespace OCRAPP.Views
{
    /// <summary>
    /// Interaction logic for Configuracao.xaml
    /// </summary>
    public partial class Configuracao : Window
    {
        private string _tesseract_path = "";
        private MainWindow _main;

        public Configuracao(MainWindow mainWindow)
        {
            InitializeComponent();
            _main = mainWindow;
        }

        private void Button_Tesseract_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.ShowDialog();
                lbPath.Text = dialog.SelectedPath;
                _tesseract_path = lbPath.Text;
            }
        }

        private void Button_Cancelar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Button_Salvar_Click(object sender, RoutedEventArgs e)
        {
            if (IsNullOrEmpty(_tesseract_path))
            {
                // Alerta de erro
                MessageBox.Show("Caminho do TESSERACT não preenchido ou inválido, por favor preencha corretamente o caminho",
                                          "Erro",
                                          MessageBoxButton.OK,
                                          MessageBoxImage.Error);
                return;
            }
            var output = Config.SalvarConfig(_tesseract_path);

            if (!output)
            {
                // Alerta de erro
                // Alerta de erro
                MessageBox.Show("Erro ao salvar arquivo de configuração, tente novamente mais tarde",
                                          "Erro",
                                          MessageBoxButton.OK,
                                          MessageBoxImage.Error);
                return;
            }
            else
            {
                MessageBox.Show("Configuração salva com sucesso",
                                       "Aviso",
                                       MessageBoxButton.OK,
                                       MessageBoxImage.Information);
                // Manda a thread principal rodar a task
                _main.StartTask();
                // Alerta de sucesso
                Close();
            }
        }
    }
}