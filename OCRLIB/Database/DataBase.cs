using Npgsql;
using NpgsqlTypes;
using OCRLIB.Model;
using System;
using System.Collections.Generic;

namespace OCRLIB.Database
{
    public class DataBase
    {
        private static string _server = "localhost";
        private static string _port = "5432";
        private static string _user = "postgres";
        private static string _password = "siprev123";
        private static string _databaseGenerico = "webprev_generico";
        private static string _databaseXtudo = "xtudo";
        private const string FtpRootDir = "E:/ged/entidades/{0}";
        private const string DirPrivado = "/usuarios/{0}/";
        private const string DirPublico = "/publico/";
        private const string DirBiblioteca = "/biblioteca/";

        private static NpgsqlConnection OpenConnectionGenerico()
        {
            try
            {
                // String de conexão com o banco
                var connString =
                    $"Server={_server};Port={_port};User Id={_user};Password={_password};Database={_databaseGenerico};";
                // Fazendo a conexão com o Npgsql
                var conn = new NpgsqlConnection(connString);
                conn.Open();
                return conn;
            }
            catch (Exception msg)
            {
                Console.WriteLine(msg.ToString());
            }
            // Retorna nulo caso não for possível a conexão
            return null;
        }

        private static NpgsqlConnection OpenConnectionXtudo()
        {
            try
            {
                // String de conexão com o banco
                var connString =
                    $"Server={_server};Port={_port};User Id={_user};Password={_password};Database={_databaseXtudo};";
                // Fazendo a conexão com o Npgsql
                var conn = new NpgsqlConnection(connString);
                conn.Open();
                return conn;
            }
            catch (Exception msg)
            {
                Console.WriteLine(msg.ToString());
            }
            // Retorna nulo caso não for possível a conexão
            return null;
        }

        public static NpgsqlConnection RedirectConnection(int idEntidade)
        {
            var stm = "SELECT usuario, senha, database, porta_interna, hostname FROM conexoes WHERE id_entidade = @idEntidade AND id_produto = 15;";

            var server = "";
            var user = "";
            var database = "";
            var password = "";
            var port = "";

            using (var conn = OpenConnectionXtudo())
            {
                using (var command = new NpgsqlCommand(stm, conn))
                {
                    command.Parameters.AddWithValue("@idEntidade", NpgsqlDbType.Bigint, idEntidade);

                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.HasRows) return null;

                        while (reader.Read())
                        {
                            database = reader["database"].ToString();
                            user = reader["usuario"].ToString();
                            password = reader["senha"].ToString();
                            port = reader["porta_interna"].ToString() == string.Empty ? _port : reader["porta_interna"].ToString();
                            server = reader["hostname"].ToString();
                        }
                    }
                }
            }

            try
            {
                // String de conexão com o banco
                var connString =
                    $"Server={server};Port={port};User Id={user};Password={password};Database={database};";
                // Fazendo a conexão com o Npgsql
                var conn = new NpgsqlConnection(connString);
                conn.Open();
                return conn;
            }
            catch (Exception msg)
            {
                Console.WriteLine(msg.ToString());
            }
            // Retorna nulo caso não for possível a conexão
            return null;
        }

        public string GetUserEntityKey(int idEntidade)
        {
            var output = "";

            const string sql = "SELECT e.chave FROM entidade e WHERE e.id = @id";

            try
            {
                using (var conn = OpenConnectionXtudo())
                {
                    using (var command = new NpgsqlCommand(sql, conn))
                    {
                        command.Parameters.AddWithValue("@id", NpgsqlDbType.Integer, idEntidade);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    output = reader["chave"].ToString();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception msg)
            {
                Console.WriteLine(msg);
            }
            return output;
        }

        private string GetUserUniqueCode(int idUsuario)
        {
            var output = "";

            const string sql = "SELECT matricula FROM usuario WHERE id = @idUsuario;";

            try
            {
                using (var conn = OpenConnectionXtudo())
                {
                    using (var command = new NpgsqlCommand(sql, conn))
                    {
                        command.Parameters.AddWithValue("@idUsuario", NpgsqlDbType.Bigint, idUsuario);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                    output = reader["matricula"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception msg)
            {
                Console.WriteLine(msg.ToString());
            }

            return output;
        }

        public List<Arquivos> GetArquivosSincronizador()
        {
            List<Arquivos> output = null;

            const string sql = "SELECT id, id_ged, etapa, status, id_entidade, data_entrada FROM ged_fila WHERE data_proc_inicio is null";

            try
            {
                using (var conn = OpenConnectionGenerico())
                {
                    using (var command = new NpgsqlCommand(sql, conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                output = new List<Arquivos>();

                                while (reader.Read())
                                {
                                    output.Add(new Arquivos
                                    {
                                        Id = int.Parse(reader[0].ToString()),
                                        IdGed = int.Parse(reader[1].ToString()),
                                        Etapa = reader[2].ToString(),
                                        Status = int.Parse(reader[3].ToString()),
                                        IdEntidade = int.Parse(reader[4].ToString().Equals(string.Empty) ? "0" : reader[4].ToString()),
                                        DataEntrada = DateTime.Parse(reader[5].ToString())
                                    });
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return output;
        }

        public List<Arquivos> GetArquivosEntidade(List<Arquivos> arquivos, int idEntidade)
        {
            const string sql =
                "SELECT  (gp.pasta_servidor || g.id || '_' || g.nome_servidor) as \"arquivo\", g.id_usuario, gp.sinc_privado, gp.sinc_publico, gp.sinc_biblioteca FROM ged g JOIN ged_pastas gp ON(g.sinc_id_ged_pastas = gp.id) WHERE g.id = @idGed;";

            try
            {
                using (var conn = RedirectConnection(idEntidade))
                {
                    using (var command = new NpgsqlCommand(sql, conn))
                    {
                        if (arquivos == null) return arquivos;

                        foreach (var arquivo in arquivos)
                        {
                            var chaveEntidade = GetUserEntityKey(arquivo.IdEntidade);

                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("@idGed", NpgsqlDbType.Bigint, arquivo.IdGed);

                            using (var reader = command.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        arquivo.IdUsuario = int.Parse(reader[1].ToString());
                                        // SINC_PRIVADO = TRUE
                                        if (bool.Parse(reader[2].ToString()))
                                        {
                                            var matricula = GetUserUniqueCode(arquivo.IdUsuario);
                                            // Monta caminho privado
                                            arquivo.CaminhoArquivo = string.Format(FtpRootDir, chaveEntidade) + string.Format(DirPrivado, matricula) + reader[0].ToString();

                                        } // SINC_PUBLICO = TRUE
                                        else if (bool.Parse(reader[3].ToString()))
                                        {
                                            // Monta caminho publico
                                            // Monta caminho privado
                                            arquivo.CaminhoArquivo = string.Format(FtpRootDir, chaveEntidade) + DirPublico + reader[0].ToString();
                                        } // SINC_BIBLIOTECA = TRUE
                                        else
                                        {
                                            // Monta caminho biblioteca
                                            arquivo.CaminhoArquivo = string.Format(FtpRootDir, chaveEntidade) + DirBiblioteca + reader[0].ToString();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return arquivos;
        }


        public bool SalvarConteudo(Arquivos arquivo)
        {
            var output = false;

            const string sql = "INSERT INTO ged_conteudo (ativo, id_entidade, conteudo, id_ged, qtd_paginas) " +
                "VALUES (@ativo, @idEntidade, @conteudo, @idGed, @qtdPaginas);";

            try
            {
                using (var conn = RedirectConnection(arquivo.IdEntidade))
                {
                    using (var command = new NpgsqlCommand(sql, conn))
                    {
                        command.Parameters.AddWithValue("@ativo", NpgsqlDbType.Boolean, true);
                        command.Parameters.AddWithValue("@idEntidade", NpgsqlDbType.Bigint, arquivo.IdEntidade);
                        command.Parameters.AddWithValue("@conteudo", NpgsqlDbType.Text, arquivo.Conteudo);
                        command.Parameters.AddWithValue("@idGed", NpgsqlDbType.Bigint, arquivo.IdGed);
                        command.Parameters.AddWithValue("@qtdPaginas", NpgsqlDbType.Bigint, arquivo.QtdPaginas);

                        output = command.ExecuteNonQuery() > 0;
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return output;
        }

        public bool UpdateGedFila(Arquivos arquivo)
        {

            var output = false;

            const string sql = "UPDATE ged_fila SET data_proc_inicio = @dataInicio, data_proc_fim = @dataFim, proc_total = @total WHERE id = @id;";

            try
            {
                using (var conn = OpenConnectionGenerico())
                {
                    using (var command = new NpgsqlCommand(sql, conn))
                    {
                        command.Parameters.AddWithValue("@dataInicio", NpgsqlDbType.Timestamp, arquivo.DataProcInicio);
                        command.Parameters.AddWithValue("@dataFim", NpgsqlDbType.Timestamp, arquivo.DataProFim);
                        command.Parameters.AddWithValue("@total", NpgsqlDbType.Double, arquivo.DataProFim.Subtract(arquivo.DataProcInicio).TotalMinutes);
                        command.Parameters.AddWithValue("@id", NpgsqlDbType.Bigint, arquivo.Id);

                        output = command.ExecuteNonQuery() > 0;
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return output;

        }

        public bool RemoveGedFila(Arquivos arquivo)
        {

            var output = false;

            const string sql = "DELETE FROM ged_fila WHERE id = @id;";

            try
            {
                using (var conn = OpenConnectionGenerico())
                {
                    using (var command = new NpgsqlCommand(sql, conn))
                    {
                        command.Parameters.AddWithValue("@id", NpgsqlDbType.Bigint, arquivo.Id);

                        output = command.ExecuteNonQuery() > 0;
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return output;

        }

    }
}
