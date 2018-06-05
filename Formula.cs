namespace ITOneRelatorioDemonstracao
{
    public class Formula
    {
        public int CatID { get; set; }
        public TipoOperacao Operacao { get; set; }

        public Formula(int catID, TipoOperacao operacao = TipoOperacao.Nenhuma)
        {
            CatID = catID;
            Operacao = operacao;
        }
    }

    public enum TipoOperacao
    {
        Nenhuma, Soma, Subtracao
    }
}
