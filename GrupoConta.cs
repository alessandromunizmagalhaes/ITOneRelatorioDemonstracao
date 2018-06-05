using System.Collections.Generic;

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

        public List<Formula> Formulas { get; } = new List<Formula>() { };

        public double VariacaoEmReal(double totalOrcado, double totalRealizado)
        {
            if (totalRealizado < 0)
            {
                return ((totalRealizado - totalOrcado) * -1);
            }
            else
            {
                return totalOrcado - totalRealizado;
            }
        }

        public double VariacaoEmPercentual(double totalOrcado, double totalRealizado)
        {
            if (totalOrcado == 0)
                return 0.0;

            var variacao = VariacaoEmReal(totalOrcado, totalRealizado);
            return ((variacao * 100) / totalOrcado);
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
                VarAnoReal = grupoFilho.VarAnoReal + grupoPai.VarAnoReal
            };
        }
    }
}
