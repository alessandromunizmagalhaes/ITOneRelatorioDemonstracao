using SAPbobsCOM;
using SAPbouiCOM;

namespace ITOneRelatorioDemonstracao
{
    public class GrupoContaFolha : GrupoConta
    {
        public GrupoContaFolha(int catID)
        {
            CatID = catID;
        }

        public double CalcularTotalOrcadoMes()
        {
            return _TotalOrcado(VerPor.Mes);
        }

        public double CalcularTotalOrcadoAno()
        {
            return _TotalOrcado(VerPor.Ano);
        }

        private double _TotalOrcado(VerPor verPor)
        {
            var res = 0.0;
            var sql =
                $@"
                SELECT 
	                SUM(-CredLTotal + DebLTotal) as totalOrcado
                FROM BGT1
                WHERE 1 = 1
	                AND Instance IN ( {Addon._cenarios_orcamento} )
	                AND AcctCode IN (SELECT AcctCode FROM FRC1 WHERE TemplateId = {Addon._modelo} AND CatId = {CatID})";

            if (verPor == VerPor.Mes)
            {
                sql += $"\nAND Line_ID BETWEEN (MONTH('{Addon._strDatainicial}')-1) AND (MONTH('{Addon._strDataFinal}')-1)";
            }

            Recordset rs = Addon.oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
            rs.DoQuery(sql);

            const string campo = "totalOrcado";
            if (rs.Fields.Item(campo).IsNull() == BoYesNoEnum.tNO)
            {
                res = rs.Fields.Item(campo).Value;
            }

            return res;
        }

        public double CalcularRealizadoMes(Empresa empresa)
        {
            return _TotalRealizado(VerPor.Mes, empresa);
        }

        public double CalcularTotalRealizadoAno(Empresa empresa)
        {
            return _TotalRealizado(VerPor.Ano, empresa);
        }

        private double _TotalRealizado(VerPor verPor, Empresa empresa)
        {
            const string campo = "totalRealizado";
            var res = 0.0;
            var filtroDataSQL =
                verPor == VerPor.Mes ? $"RefDate BETWEEN '{Addon._strDatainicial}' AND '{Addon._strDataFinal}'" : $"YEAR(RefDate) = {Addon._periodo}";

            var sql = string.Empty;

            switch (empresa)
            {
                case Empresa.Todas:
                    sql =
                        $@"
                        SELECT  
	                        (SUM(Debit - Credit)) as {campo}
                        FROM JDT1  
                        WHERE 1 = 1  
	                        AND {filtroDataSQL}
                            AND ProfitCode IN ({Addon._ccustos})   -- centro de custo
	                        AND Account IN (SELECT AcctCode FROM FRC1 WHERE TemplateId = {Addon._modelo} AND CatId = {CatID})

                        UNION ALL

                        SELECT  
	                        (SUM(Debit - Credit)) as {campo}
                        FROM [IT_PS_PRD]..JDT1
                        WHERE 1 = 1  
	                        AND {filtroDataSQL}
                            AND OcrCode2 IN ({Addon._ccustos})   -- centro de custo
	                        AND Account IN (SELECT AcctCode FROM FRC1 WHERE TemplateId = {Addon._modelo} AND CatId = {CatID})
                        ";
                    break;
                case Empresa.ITOne:
                    sql =
                        $@"
                        SELECT  
	                        (SUM(Debit - Credit)) as {campo}
                        FROM JDT1  
                        WHERE 1 = 1  
	                        AND {filtroDataSQL}
                            AND ProfitCode IN ({Addon._ccustos})   -- centro de custo
	                        AND Account IN (SELECT AcctCode FROM FRC1 WHERE TemplateId = {Addon._modelo} AND CatId = {CatID})";
                    break;
                case Empresa.ITPS:
                    sql =
                        $@"
                        SELECT  
	                        (SUM(Debit - Credit)) as {campo}
                        FROM [IT_PS_PRD]..JDT1
                        WHERE 1 = 1  
	                        AND {filtroDataSQL}
                            AND OcrCode2 IN ({Addon._ccustos})   -- centro de custo
	                        AND Account IN (SELECT AcctCode FROM FRC1 WHERE TemplateId = {Addon._modelo} AND CatId = {CatID})";
                    break;
                default:
                    break;
            }

            Recordset rs = Addon.oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
            rs.DoQuery(sql);

            while (!rs.EoF)
            {
                res += rs.Fields.Item(campo).Value;

                rs.MoveNext();
            }

            return res;
        }

        public void OrganizarFormulas(DataTable dt, int row)
        {
            if (dt.GetValue("SubSum", row) == "N")
            {
                return;
            }

            for (int i = 1; i < Addon._quantidade_campos_contas_formula; i++)
            {
                var catid_param = dt.GetValue(Addon._prefixo_campos_contas_formula + i, row);
                if (catid_param == 0)
                {
                    continue;
                }

                var str_operacao = dt.GetValue(Addon._prefixo_campos_operacao_formula + i, row);
                var operacao = str_operacao == "+" ? TipoOperacao.Soma : (str_operacao == "-" ? TipoOperacao.Subtracao : TipoOperacao.Nenhuma);

                Formula formula = new Formula(catid_param, operacao);
                Formulas.Add(formula);
            };
        }
    }
}
