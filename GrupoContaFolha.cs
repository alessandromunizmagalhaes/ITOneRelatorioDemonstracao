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

        public double CalcularRealizadoMes()
        {
            var res = 0.0;
            var sql =
                $@"
                SELECT  
	                (SUM(Debit - Credit)) as totalRealizado
                FROM JDT1  
                WHERE 1 = 1  
	                AND RefDate BETWEEN '{Addon.strDatainicial}' and '{Addon.strDataFinal}'
                    AND ProfitCode IN ({Addon.ccustos})   -- centro de custo
	                AND Account IN (SELECT AcctCode FROM FRC1 WHERE TemplateId = {Addon.modelo} AND CatId = {CatID})";

            Recordset rs = Addon.oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
            rs.DoQuery(sql);

            const string campo = "totalRealizado";
            if (rs.Fields.Item(campo).IsNull() == BoYesNoEnum.tNO)
            {
                res = rs.Fields.Item(campo).Value;
            }

            return res;
        }

        public double CalcularTotalRealizadoAno()
        {
            var res = 0.0;
            var sql =
                $@"
                SELECT  
	                (SUM(Debit - Credit)) as totalRealizado
                FROM JDT1  
                WHERE 1 = 1  
	                AND YEAR(RefDate) = {Addon.periodo}
                    AND ProfitCode IN ({Addon.ccustos})   -- centro de custo
	                AND Account IN (SELECT AcctCode FROM FRC1 WHERE TemplateId = {Addon.modelo} AND CatId = {CatID})";

            Recordset rs = Addon.oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
            rs.DoQuery(sql);

            const string campo = "totalRealizado";
            if (rs.Fields.Item(campo).IsNull() == BoYesNoEnum.tNO)
            {
                res = rs.Fields.Item(campo).Value;
            }

            return res;
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
	                AND Instance IN ( {Addon.cenarios_orcamento} )
	                AND AcctCode IN (SELECT AcctCode FROM FRC1 WHERE TemplateId = {Addon.modelo} AND CatId = {CatID})";

            if (verPor == VerPor.Mes)
            {
                sql += $"\nAND Line_ID BETWEEN (MONTH('{Addon.strDatainicial}')-1) AND (MONTH('{Addon.strDataFinal}')-1)";
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

        public double VariacaoEmReal(double totalOrcado, double totalRealizado)
        {
            if (totalRealizado < 0)
            {
                return ((totalRealizado - totalOrcado)*-1);
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

    public enum VerPor
    {
        Mes = 0,
        Ano = 1,
    }
}
