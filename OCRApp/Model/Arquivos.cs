using System;

namespace OCRApp.Model
{
    class Arquivos
    {
        public int Id { get; set; }
        public int IdGed { get; set; }
        public int IdEntidade { get; set; }
        public int IdUsuario { get; set; }
        public string Etapa { get; set; }
        public int Status { get; set; }
        public int QtdPaginas { get; set; }
        public DateTime DataEntrada { get; set; }
        public DateTime DataProcInicio { get; set; }
        public DateTime DataProFim { get; set; }
        public String CaminhoArquivo { get; set; }
        public String Conteudo { get; set; }
    }
}
