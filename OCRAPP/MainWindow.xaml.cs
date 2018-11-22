using OCRAPP.Views;
using OCRLIB.Database;
using OCRLIB.Model;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace OCRAPP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static DataBase Db;
        private static CancellationTokenSource Cts;
        private static CancellationToken Token;
        public Config Config { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            Db = new DataBase();

            Cts = new CancellationTokenSource();
            Token = Cts.Token;

            //var arquivosSinc = db.GetArquivosSincronizador();
            //var arquivosEntidade = db.GetArquivosEntidade(arquivosSinc, 1);

            c_id.Binding = new Binding("Id");
            c_id_ged.Binding = new Binding("IdGed");
            c_id_entidade.Binding = new Binding("IdEntidade");
            c_id_usuario.Binding = new Binding("IdUsuario");
            c_etapa.Binding = new Binding("Etapa");
            c_status.Binding = new Binding("Status");
            c_qtd_paginas.Binding = new Binding("QtdPaginas");
            //c_data_entrada.Binding = new Binding("DataEntrada");
            //c_data_proc_inicio.Binding = new Binding("DataProcInicio");
            //c_data_proc_fim.Binding = new Binding("DataProFim");
            c_caminho_arquivo.Binding = new Binding("CaminhoArquivo");
            c_conteudo.Binding = new Binding("Conteudo");
            // Espera 4 segundos para carregar a tela e Roda a verificação do arquivo de configuração
            Task.Delay(4000).ContinueWith(t => VerifyConfiguration(this));
        }

        public void VerifyConfiguration(MainWindow main)
        {
            var result = Config.ReadConfig();

            if (!result)
            {
                MessageBox.Show("Configuração não encontrada, por favor adicione o caminho do TESSERACT para que a aplicação possa ser executada",
                    "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                // Chama o dispatcher pois a background thread / task não pode acessar componentes da UI
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    // Abre a tela de configuração
                    try
                    {
                        var config = new Configuracao(main);
                        config.Show();
                    }
                    catch (Exception msg)
                    {
                        Console.WriteLine(msg);
                    }
                }));
            } else
            {
                StartTask();
            }
        }

        public void StartTask()
        {
            // Tarefa que fica rodando atualizando a tabela
            Task.Factory.StartNew(() => StartExecuteOCR(Token, this), TaskCreationOptions.LongRunning);
        }

        public void CancelTask()
        {
            // Cancela a tarefa
            Cts.Cancel();
            // Libera o objeto
            Cts.Dispose();
            // Cria um novo
            Cts = new CancellationTokenSource();
            Token = Cts.Token;
        }

        public static void StartExecuteOCR(CancellationToken token, MainWindow main)
        {
            while (true)
            {
                try
                {
                    Console.WriteLine("Executando tarefa");
                    main.updateLabelStatus("Executando tarefa");
                    if (token.IsCancellationRequested)
                    {
                        Console.WriteLine("Tarefa foi cancelada....");
                        main.updateLabelStatus("Tarefa foi cancelada....");
                        break;
                    }
                    var tmp = main.readItems(Db);

                    foreach(var item in tmp)
                    {
                        var caminho = item?.CaminhoArquivo;
                        if (caminho == null)
                        {
                            Console.WriteLine("nulo");
                            continue;
                        }
                        if (caminho.ToLower().Contains(".pdf"))
                        {
                            Console.WriteLine(item.CaminhoArquivo);
                        }
                    }

                    //main.updateDataGrid(tmp);

                    Thread.Sleep(TimeSpan.FromSeconds(5));
                }
                catch (Exception msg)
                {
                    Console.WriteLine(msg);
                }
            }
        }

        public List<Arquivos> readItems(DataBase db)
        {
            var arquivosSinc = db.GetArquivosSincronizador();
            return db.GetArquivosEntidade(arquivosSinc, 1);
        }

        public void updateDataGrid(List<Arquivos> items)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                if (dataGrid.HasItems)
                    dataGrid.Items.Clear();

                foreach (var item in items)
                    dataGrid.Items.Add(item);

            }
            else
            {
                Application.Current.Dispatcher.Invoke(new Action(() => updateDataGrid(items)));
            }
        }

        public void updateLabelStatus(string message)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                lbStatus.Content = message;

            }
            else
            {
                Application.Current.Dispatcher.Invoke(new Action(() => updateLabelStatus(message)));
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var config = new Configuracao(this);
            config.Show();

        }
    }
}
