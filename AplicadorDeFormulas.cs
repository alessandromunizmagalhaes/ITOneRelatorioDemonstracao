using System.Collections.Generic;

namespace ITOneRelatorioDemonstracao
{
    public static class AplicadorDeFormulas
    {
        public static void Aplicar(GrupoConta conta, Dictionary<int, GrupoConta> catid_grupo_conta)
        {
            if (conta.Formulas.Count > 1)
            {
                var primeiraFormula = conta.Formulas[0];
                GrupoConta primeiraConta = catid_grupo_conta[primeiraFormula.CatID];

                var orcadoMes = primeiraConta.TotalOrcadoMes;
                var realizadoMes = primeiraConta.TotalRealizadoMes;
                var varMesReal = primeiraConta.VarMesReal;
                var varMesPerc = primeiraConta.VarMesPerc;

                var orcadoAno = primeiraConta.TotalOrcadoAno;
                var realizadoAno = primeiraConta.TotalRealizadoAno;
                var varAnoReal = primeiraConta.VarAnoReal;
                var varAnoPerc = primeiraConta.VarAnoPerc;

                var ultimaOperacao = primeiraFormula.Operacao;

                for (int i = 1; i < conta.Formulas.Count; i++)
                {
                    var currentFormula = conta.Formulas[i];
                    GrupoConta grupoContaParam = catid_grupo_conta[currentFormula.CatID];

                    if (ultimaOperacao == TipoOperacao.Soma)
                    {
                        orcadoMes += grupoContaParam.TotalOrcadoMes;
                        realizadoMes += grupoContaParam.TotalRealizadoMes;
                        varMesReal += grupoContaParam.VarMesReal;

                        orcadoAno += grupoContaParam.TotalOrcadoAno;
                        realizadoAno += grupoContaParam.TotalRealizadoAno;
                        varAnoReal += grupoContaParam.VarAnoReal;
                    }
                    else if (ultimaOperacao == TipoOperacao.Subtracao)
                    {
                        orcadoMes -= grupoContaParam.TotalOrcadoMes;
                        realizadoMes -= grupoContaParam.TotalRealizadoMes;
                        varMesReal -= grupoContaParam.VarMesReal;

                        orcadoAno -= grupoContaParam.TotalOrcadoAno;
                        realizadoAno -= grupoContaParam.TotalRealizadoAno;
                        varAnoReal -= grupoContaParam.VarAnoReal;
                    }
                }

                conta.TotalOrcadoMes = orcadoMes;
                conta.TotalRealizadoMes = realizadoMes;
                conta.VarMesReal = varMesReal;

                conta.TotalOrcadoAno = orcadoAno;
                conta.TotalRealizadoAno = realizadoAno;
                conta.VarAnoReal = varAnoReal;
            }
        }
    }
}
