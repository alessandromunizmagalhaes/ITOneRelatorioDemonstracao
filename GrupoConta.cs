namespace ITOneRelatorioDemonstracao
{
    public class GrupoConta
    {
        public int CatID { get; set; }
        public int RowInDataSource { get; set; }

        public double TotalOrcadoMes { get; set; }
        public double TotalRealizadoMes { get; set; }
        public double VarMesReal { get; set; }
        public double VarMesPerc { get; set; }
        public string SetaMes
        {
            get
            {
                var variacao_mes = VarMesReal;
                return variacao_mes == 0 ? "" : (variacao_mes > 0 ? Addon.seta_pra_cima : Addon.seta_pra_baixo);

            }
        }

        public double TotalOrcadoAno { get; set; }
        public double TotalRealizadoAno { get; set; }
        public double VarAnoReal { get; set; }
        public double VarAnoPerc { get; set; }
        public string SetaAno
        {
            get
            {
                var variacao_ano = VarAnoReal;
                return variacao_ano == 0 ? "" : (variacao_ano > 0 ? Addon.seta_pra_cima : Addon.seta_pra_baixo);

            }
        }

        public static GrupoContaFolha Clone(GrupoConta from, int catID)
        {
            return new GrupoContaFolha(catID)
            {
                TotalOrcadoMes = from.TotalOrcadoMes,
                TotalRealizadoMes = from.TotalRealizadoMes,
                VarMesReal = from.VarMesReal,


                TotalOrcadoAno = from.TotalOrcadoAno,
                TotalRealizadoAno = from.TotalRealizadoAno,
                VarAnoReal = from.VarAnoReal,

                VarMesPerc = ((GrupoContaFolha)from).VariacaoEmPercentual(from.TotalOrcadoMes, from.TotalRealizadoMes),
                VarAnoPerc = ((GrupoContaFolha)from).VariacaoEmPercentual(from.TotalOrcadoAno, from.TotalRealizadoAno)

            };
        }

        public static GrupoContaFolha Concatena(GrupoConta grupoFilho, GrupoConta grupoPai)
        {
            return new GrupoContaFolha(grupoPai.CatID)
            {
                TotalOrcadoMes = grupoFilho.TotalOrcadoMes + grupoPai.TotalOrcadoMes,
                TotalRealizadoMes = grupoFilho.TotalRealizadoMes + grupoPai.TotalRealizadoMes,
                VarMesReal = grupoFilho.VarMesReal + grupoPai.VarMesReal,


                TotalOrcadoAno = grupoFilho.TotalOrcadoAno + grupoPai.TotalOrcadoAno,
                TotalRealizadoAno = grupoFilho.TotalRealizadoAno + grupoPai.TotalRealizadoAno,
                VarAnoReal = grupoFilho.VarAnoReal + grupoPai.VarAnoReal,

                VarMesPerc = ((GrupoContaFolha)grupoPai).VariacaoEmPercentual(grupoFilho.TotalOrcadoMes + grupoPai.TotalOrcadoMes, grupoFilho.TotalRealizadoMes + grupoPai.TotalRealizadoMes),
                VarAnoPerc = ((GrupoContaFolha)grupoPai).VariacaoEmPercentual(grupoFilho.TotalOrcadoAno + grupoPai.TotalOrcadoAno, grupoFilho.TotalRealizadoAno + grupoPai.TotalRealizadoAno)
            };
        }
    }
}
