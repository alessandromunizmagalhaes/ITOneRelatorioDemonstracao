namespace ITOneRelatorioDemonstracao
{
    public static class Dialogs
    {
        public static SAPbouiCOM.Application SBOApplication;

        public static void Info(string msg, bool popup = false)
        {
            SBOApplication.StatusBar.SetText(msg, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
            if (popup)
            {
                SBOApplication.MessageBox(msg);
            }
        }

        public static void Success(string msg, bool popup = false)
        {
            SBOApplication.StatusBar.SetText(msg, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            if (popup)
            {
                SBOApplication.MessageBox(msg);
            }
        }

        public static void Error(string msg, bool popup = false)
        {
            SBOApplication.StatusBar.SetText(msg, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            if (popup)
            {
                SBOApplication.MessageBox(msg);
            }
        }

        public static bool Confirm(string msg)
        {
            return SBOApplication.MessageBox(msg, 1, "Sim", "Não") == 1;
        }
    }
}
