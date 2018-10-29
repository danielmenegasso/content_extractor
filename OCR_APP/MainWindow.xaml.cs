using OCRAPP.Views;
using OCRLIB.Database;
using OCRLIB.Model;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace OCRAPP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var db = new DataBase();
            var cts = new CancellationTokenSource();
            var token = cts.Token;

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
            // Tarefa que fica rodando atualizando a tabela
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        Console.WriteLine("Executando tarefa");
                        if (token.IsCancellationRequested)
                        {
                            Console.WriteLine("Tarefa foi cancelada....");
                            break;
                        }
                        var tmp = readItems(db);

                        updateDataGrid(tmp);

                        Thread.Sleep(TimeSpan.FromSeconds(5));
                    }
                    catch (Exception msg)
                    {
                        Console.WriteLine(msg);
                    }
                }
            }, token);

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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var config = new Configuracao();
            config.Show();

        }
    }
}
