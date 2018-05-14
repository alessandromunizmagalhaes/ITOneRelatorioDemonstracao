using SAPbobsCOM;

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
    }
}
