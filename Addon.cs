using CrystalDecisions.CrystalReports.Engine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ITOneRelatorioDemonstracao
{
    class Addon : IDisposable
    {
        #region :: Propriedades Globais

        public static SAPbouiCOM.Application SBOApplication;
        public static SAPbobsCOM.Company oCompany;
        private string AddonName = "ITOne - Relatório de Demonstração de Resultados";
        private SAPbouiCOM.EventFilters oEventFilters = null;

        private int _corVerde = ColorTranslator.ToWin32(Color.Green);
        private int _corVermelha = ColorTranslator.ToWin32(Color.Red);
        private const int _corCelulaDefault = -1;

        public static string seta_pra_cima = ((char)0x21E7).ToString();//char.ConvertFromUtf32(0x2191);
        public static string seta_pra_baixo = ((char)0x21E9).ToString(); //char.ConvertFromUtf32(0x2193);
        List<string> ccustos_atribuidos = new List<string>() { };
        List<string> ccustos_selecionados_ids = new List<string>() { };
        List<string> ccustos_selecionados_nomes = new List<string>() { };
        int currentFormType = 0;
        int currentFormTypeCount = 0;

        public static string _ccustos;
        public static string _modelo;
        public static string _periodo;
        public static Empresa _empresa;
        public static string _strDatainicial;
        public static string _strDataFinal;
        public static string _cenarios_orcamento;

        public static string _campos_contas_formula;
        public const string _prefixo_campos_contas_formula = "Param_";
        public const int _quantidade_campos_contas_formula = 25;

        public static string _campos_operacao_formula;
        public const string _prefixo_campos_operacao_formula = "OP_";
        public const int _quantidade_campos_operacao_formula = 24;

        #endregion


        #region :: Inicialização 

        public Addon()
        {
            SetApplication();
            Dialogs.SBOApplication = SBOApplication;

            Dialogs.Info(":: " + AddonName + " :: Iniciando ...");

            Dialogs.Info(":: " + AddonName + " :: Conectando com DI API ...");
            if (SetConnectionContext() != 0)
            {
                Dialogs.Error(":: " + AddonName + " :: Falha ao conectar com DI API ", true);
                System.Windows.Forms.Application.Exit();
            }

            Dialogs.Info(":: " + AddonName + " :: Conectando com Banco de Dados ...");
            if (ConnectToCompany() != 0)
            {
                Dialogs.Error(":: " + AddonName + " :: Falha ao conectar com o Banco de Dados", true);
                System.Windows.Forms.Application.Exit();
            }

            Dialogs.Info(":: " + AddonName + " :: Criando estruturas de tabelas ...");

            new DataBaseFunctions(oCompany).ManipulaCampos();

            Dialogs.Success(":: " + AddonName + " :: Inicializado com sucesso");

            CriaMenus();
            SetFilters();
            IniciarCamposParaFormula();
        }

        private static void IniciarCamposParaFormula()
        {
            for (int i = 1; i <= _quantidade_campos_contas_formula; i++)
            {
                _campos_contas_formula += $",{_prefixo_campos_contas_formula}" + i;
            }
            for (int i = 1; i <= _quantidade_campos_operacao_formula; i++)
            {
                _campos_operacao_formula += $",{_prefixo_campos_operacao_formula}" + i;
            }
        }

        private void SetApplication()
        {
            SAPbouiCOM.SboGuiApi sboGuiApi;
            string connectionString = null;
            sboGuiApi = new SAPbouiCOM.SboGuiApi();

            try
            {
                if (Environment.GetCommandLineArgs().Length > 1)
                    connectionString = Environment.GetCommandLineArgs().GetValue(1).ToString();
            }
            catch (Exception e)
            {
                MessageBox.Show("Não foi possível buscar a string de conexão com o SAP.\nErro: " + e.Message);
            }

            try
            {
                sboGuiApi.Connect(connectionString);
            }
            catch (Exception e)
            {
                MessageBox.Show("Não foi possível estabelecer uma conexão com o SAP.\nErro: " + e.Message);
            }
            SBOApplication = sboGuiApi.GetApplication();
        }

        private int SetConnectionContext()
        {
            string cookie;
            string connectionContext = "";

            try
            {
                // First initialize the Company object
                oCompany = new SAPbobsCOM.Company();

                // Acquire the connection context cookie from the DI API.
                cookie = oCompany.GetContextCookie();

                // Retrieve the connection context string from the UI API using the acquired cookie.
                connectionContext = SBOApplication.Company.GetConnectionContext(cookie);

                // before setting the SBO Login Context make sure the company is not connected
                if (oCompany.Connected)
                {
                    oCompany.Disconnect();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Falha ao tentar conectar com a DI API.\nErro: " + e.Message);
            }

            // Set the connection context information to the DI API.
            return oCompany.SetSboLoginContext(connectionContext);
        }

        private int ConnectToCompany()
        {
            // Establish the connection to the company database.
            return oCompany.Connect();
        }

        #endregion


        #region :: Gestão de Menu

        private void CriaMenus()
        {
            SAPbouiCOM.Menus menus;
            SAPbouiCOM.MenuItem menuItem = null;
            string updateMenu = "UPDTMENU";
            string menuID = "43520";

            //Coleção menus da aplicação...
            menus = SBOApplication.Menus;
            SAPbouiCOM.MenuCreationParams oCreationPackage;
            oCreationPackage = SBOApplication.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_MenuCreationParams);

            //id do menu de módulos...
            menuItem = BuscaMenu(menuID);
            menus = menuItem.SubMenus;

            try
            {
                //identifica o tipo do menu como nó ou folha da árvore...
                oCreationPackage.Type = SAPbouiCOM.BoMenuType.mt_POPUP;
                oCreationPackage.UniqueID = updateMenu;
                oCreationPackage.Image = Application.StartupPath + "/UpdateIcon.png";
                oCreationPackage.String = "Relatório de Centro de Custo";
                //atribui o novo menu na quarta posição...
                oCreationPackage.Position = 3;
                menus.AddEx(oCreationPackage);
            }
            catch (Exception e)// Menu already exists
            { }

            menuItem = BuscaMenu(updateMenu);
            menus = menuItem.SubMenus;

            try
            {
                oCreationPackage.Type = SAPbouiCOM.BoMenuType.mt_STRING;
                oCreationPackage.UniqueID = "MnuRelDemoConsolidada";
                oCreationPackage.String = "Relatório de DRE - Centro de Custo";
                oCreationPackage.Enabled = true;
                oCreationPackage.Position = menus.Count + 1;
                menus.AddEx(oCreationPackage);
            }
            catch (Exception e) // Menu already exists
            { }
        }

        private SAPbouiCOM.MenuItem BuscaMenu(object menuID)
        {
            SAPbouiCOM.MenuItem retorno = null;
            try
            {
                retorno = SBOApplication.Menus.Item(menuID);
            }
            catch { }
            return retorno;
        }

        #endregion


        #region :: Captura de Eventos

        public void SetFilters()
        {
            const string ousr_form_uid = "20700";
            const string relatorio_form_uid = "FrmRelDemoConsolidada";
            const string atribuicao_form_uid = "FrmAtribuicaoCCusto";

            oEventFilters = new SAPbouiCOM.EventFilters();
            oEventFilters.Add(SAPbouiCOM.BoEventTypes.et_FORM_LOAD).AddEx(ousr_form_uid);

            SAPbouiCOM.EventFilter eventFilter = oEventFilters.Add(SAPbouiCOM.BoEventTypes.et_CLICK);
            eventFilter.AddEx(ousr_form_uid);
            eventFilter.AddEx(atribuicao_form_uid);
            eventFilter.AddEx(relatorio_form_uid);

            oEventFilters.Add(SAPbouiCOM.BoEventTypes.et_FORM_DATA_ADD).AddEx(ousr_form_uid);
            oEventFilters.Add(SAPbouiCOM.BoEventTypes.et_FORM_DATA_UPDATE).AddEx(ousr_form_uid);

            oEventFilters.Add(SAPbouiCOM.BoEventTypes.et_MENU_CLICK);

            oEventFilters.Add(SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED).AddEx(relatorio_form_uid);
            oEventFilters.Add(SAPbouiCOM.BoEventTypes.et_CHOOSE_FROM_LIST).AddEx(relatorio_form_uid);
            oEventFilters.Add(SAPbouiCOM.BoEventTypes.et_COMBO_SELECT).AddEx(relatorio_form_uid);

            SBOApplication.MenuEvent += SBOApplication_MenuEvent;
            SBOApplication.ItemEvent += SBOApplication_ItemEvent;
            SBOApplication.FormDataEvent += SBOApplication_FormDataEvent;

            SBOApplication.SetFilter(oEventFilters);
        }

        private void SBOApplication_FormDataEvent(ref SAPbouiCOM.BusinessObjectInfo pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;
            if (EventoDepois(pVal) && EventoEmCadastroUsuarios(pVal) && pVal.ActionSuccess)
            {
                const string table_name = "[@UPD_USER_CCUSTO]";
                string insert_values = "";
                SAPbouiCOM.Form oForm = SBOApplication.Forms.Item(pVal.FormUID);
                string user_code = oForm.DataSources.DBDataSources.Item("OUSR").GetValue("USER_CODE", 0).Trim();


                for (int i = 0; i < ccustos_atribuidos.Count; i++)
                {
                    string ccusto = ccustos_atribuidos[i];
                    insert_values += $",(@table_id + {i},@table_id + {i}, '{user_code}', '{ccusto}' )";
                }

                SAPbobsCOM.Recordset rs = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                rs.DoQuery($"DELETE FROM {table_name} WHERE U_usuario = '{user_code}';");

                if (!String.IsNullOrEmpty(insert_values))
                {
                    insert_values = insert_values.Remove(0, 1);

                    string insert =
                        $@" {SQLDeclareTableID(table_name)}
                            INSERT INTO {table_name} VALUES {insert_values} ";

                    rs.DoQuery(insert);
                }
            }
        }

        private void SBOApplication_ItemEvent(string FormUID, ref SAPbouiCOM.ItemEvent pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;
            string frmreldemoconsolidadauid = "FrmRelDemoConsolidada";
            if (EventoDepois(pVal) && pVal.FormUID == frmreldemoconsolidadauid && pVal.ActionSuccess && pVal.ItemUID == "cmbModelo" && pVal.EventType == SAPbouiCOM.BoEventTypes.et_COMBO_SELECT)
            {
                SAPbouiCOM.Form oForm = SBOApplication.Forms.GetForm(FormUID, pVal.FormTypeCount);
                CarregarDatatableMatriz(pVal, oForm);
            }
            else if (EventoDepois(pVal) && pVal.FormUID == frmreldemoconsolidadauid && pVal.ActionSuccess && pVal.ItemUID == "btnSearch" && pVal.EventType == SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED)
            {
                SAPbouiCOM.Form oForm = SBOApplication.Forms.GetForm(FormUID, pVal.FormTypeCount);
                CarregarDatatableMatriz(pVal, oForm);
            }
            else if (EventoDepois(pVal) && pVal.FormUID == frmreldemoconsolidadauid && pVal.ActionSuccess && pVal.EventType == SAPbouiCOM.BoEventTypes.et_CHOOSE_FROM_LIST)
            {
                SAPbouiCOM.Form oForm = SBOApplication.Forms.GetForm(FormUID, pVal.FormTypeCount);
                SAPbouiCOM.ChooseFromListEvent cflevent = (SAPbouiCOM.ChooseFromListEvent)pVal;
                SAPbouiCOM.ChooseFromList oCFLEvento = default(SAPbouiCOM.ChooseFromList);
                string strUid = cflevent.ChooseFromListUID;
                oCFLEvento = oForm.ChooseFromLists.Item(strUid);
                SAPbouiCOM.DataTable oDataTable = cflevent.SelectedObjects;

                if (oDataTable != null)
                {
                    ccustos_selecionados_ids.Clear();
                    string ccustos = "", ccustos_desc = "";
                    for (int i = 0; i < oDataTable.Rows.Count; i++)
                    {
                        string ccusto = oDataTable.GetValue("PrcCode", i), ccusto_desc = oDataTable.GetValue("PrcName", i);
                        if (!ccustos_selecionados_ids.Contains(ccusto))
                        {
                            ccustos_selecionados_ids.Add(ccusto);
                            ccustos_selecionados_nomes.Add(ccusto_desc);
                        }

                        ccustos += "," + ccusto;
                        ccustos_desc += "," + ccusto_desc + Environment.NewLine;
                    }

                    try
                    {
                        oForm.DataSources.UserDataSources.Item("UDCFL").Value = ccustos.Remove(0, 1);
                        oForm.DataSources.UserDataSources.Item("UDDesc").Value = ccustos_desc.Remove(0, 1);
                    }
                    catch (Exception)
                    {
                        Dialogs.Error("Erro ao inserir valor vindo do ChooseFromList.", true);
                    }

                    CarregarDatatableMatriz(pVal, oForm);
                }
            }
            else if (pVal.EventType == SAPbouiCOM.BoEventTypes.et_FORM_LOAD && EventoAntes(pVal) && EventoEmCadastroUsuarios(pVal))
            {
                SAPbouiCOM.Form oForm = SBOApplication.Forms.GetFormByTypeAndCount(pVal.FormType, pVal.FormTypeCount);

                string refitemID = "10000116";
                string btnAtribuirCCustosID = "btnCCusto";

                SAPbouiCOM.Item refItem = oForm.Items.Item(refitemID);

                SAPbouiCOM.Item btnAtribuirCCusto = oForm.Items.Add(btnAtribuirCCustosID, SAPbouiCOM.BoFormItemTypes.it_BUTTON);

                btnAtribuirCCusto.FromPane = 0;
                btnAtribuirCCusto.ToPane = 0;
                btnAtribuirCCusto.Top = refItem.Top - 5;
                btnAtribuirCCusto.Left = refItem.Width + 50;
                btnAtribuirCCusto.Width = 150;

                SAPbouiCOM.Button btnSelecaoAutomatica = btnAtribuirCCusto.Specific;

                btnSelecaoAutomatica.Caption = "Centros de Custo Atribuidos";
            }
            else if (pVal.EventType == SAPbouiCOM.BoEventTypes.et_CLICK && EventoDepois(pVal) && EventoEmCadastroUsuarios(pVal) && pVal.ItemUID == "btnCCusto")
            {
                SAPbouiCOM.Form oFormUser = SBOApplication.Forms.GetFormByTypeAndCount(pVal.FormType, pVal.FormTypeCount);
                string user_code = oFormUser.DataSources.DBDataSources.Item("OUSR").GetValue("USER_CODE", 0);
                currentFormType = pVal.FormType;
                currentFormTypeCount = pVal.FormTypeCount;

                SAPbouiCOM.FormCreationParams creationPackage = null;
                System.Xml.XmlDocument oXMLDoc = null;
                oXMLDoc = new System.Xml.XmlDocument();

                string formtype = "FrmAtribuicaoCCusto";
                string uniqueid = "FrmAtribuicaoCCusto";

                creationPackage = SBOApplication.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_FormCreationParams);
                creationPackage.FormType = formtype;
                creationPackage.UniqueID = uniqueid;
                // creationPackage.Modality = SAPbouiCOM.BoFormModality.fm_Modal;


                oXMLDoc.Load($@"{AppDomain.CurrentDomain.BaseDirectory}\FrmAtribuicaoCCusto.srf");
                creationPackage.XmlData = oXMLDoc.InnerXml;
                SAPbouiCOM.Form oFormModal = SBOApplication.Forms.AddEx(creationPackage);

                oFormModal.Visible = true;

                try
                {
                    oFormModal.Freeze(true);
                    SAPbouiCOM.DataTable dt = oFormModal.DataSources.DataTables.Item("DT_CCusto");
                    dt.ExecuteQuery(
                        $@"SELECT 
	                        CASE WHEN tb2.Code IS NOT NULL THEN 'Y' ELSE 'N' END as 'check'
	                        , tb1.PrcCode
	                        , tb1.PrcName
                        FROM OPRC tb1
                        LEFT JOIN [@UPD_USER_CCUSTO] tb2 ON (tb2.U_ccusto = tb1.PrcCode AND tb2.U_usuario = '{user_code}')"
                    );

                    SAPbouiCOM.Matrix mtx = oFormModal.Items.Item("mtxSel").Specific;
                    SAPbouiCOM.Columns oColumns = mtx.Columns;

                    oColumns.Item("check").DataBind.Bind(dt.UniqueID, "check");
                    oColumns.Item("PrcCode").DataBind.Bind(dt.UniqueID, "PrcCode");
                    oColumns.Item("PrcName").DataBind.Bind(dt.UniqueID, "PrcName");

                    mtx.LoadFromDataSourceEx();
                    mtx.AutoResizeColumns();
                }
                finally
                {
                    oFormModal.Freeze(false);
                }
            }
            else if (pVal.EventType == SAPbouiCOM.BoEventTypes.et_CLICK && EventoDepois(pVal) && pVal.FormUID == "FrmAtribuicaoCCusto" && pVal.ItemUID == "btnAtrib")
            {
                SAPbouiCOM.Form oFormModal = SBOApplication.Forms.Item(pVal.FormUID);
                ccustos_atribuidos.Clear();

                try
                {
                    ((SAPbouiCOM.Matrix)(oFormModal.Items.Item("mtxSel").Specific)).FlushToDataSource();
                    SAPbouiCOM.DataTable dt = oFormModal.DataSources.DataTables.Item("DT_CCusto");
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        Dialogs.Success($"Atribuindo {i + 1} de {dt.Rows.Count}...");
                        if (dt.GetValue("check", i) == "Y")
                        {
                            string ccusto = dt.GetValue("PrcCode", i);
                            ccustos_atribuidos.Add(ccusto);
                        }
                    }

                    Dialogs.Success("Ok.");
                }
                catch (Exception e)
                {
                    Dialogs.Error("Erro ao atribuir centro de custo ao usúarios.\nErro: " + e.Message, true);
                }
                finally
                {
                    oFormModal.Freeze(false);
                }


                try
                {
                    SAPbouiCOM.Form oFormUser = SBOApplication.Forms.GetFormByTypeAndCount(currentFormType, currentFormTypeCount);

                    if (oFormUser.Mode == SAPbouiCOM.BoFormMode.fm_OK_MODE)
                    {
                        oFormUser.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
                    }
                }
                catch (Exception e)
                {
                    Dialogs.Error("Erro interno. Form de Usuários não encontrado.\nErro: " + e.Message, true);
                }

                oFormModal.Close();

            }
            else if (pVal.EventType == SAPbouiCOM.BoEventTypes.et_CLICK && EventoDepois(pVal) && pVal.FormUID == "FrmAtribuicaoCCusto" && pVal.ItemUID == "btnSelect")
            {
                SAPbouiCOM.Form oFormModal = SBOApplication.Forms.Item(pVal.FormUID);
                try
                {
                    SAPbouiCOM.Matrix mtx = oFormModal.Items.Item("mtxSel").Specific;
                    mtx.FlushToDataSource();
                    SAPbouiCOM.DataTable dt = oFormModal.DataSources.DataTables.Item("DT_CCusto");
                    string valOnOff = dt.GetValue("check", 0) == "Y" ? "N" : "Y";
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        Dialogs.Success($"Marcando/Desmarcando {i + 1} de {dt.Rows.Count}...");
                        dt.SetValue("check", i, valOnOff);
                    }

                    mtx.LoadFromDataSourceEx();
                    Dialogs.Success("Ok.");
                }
                catch (Exception e)
                {
                    Dialogs.Error("Erro ao marcar/desmarcar todos.\nErro: " + e.Message, true);
                }
            }
            else if (pVal.EventType == SAPbouiCOM.BoEventTypes.et_CLICK && EventoDepois(pVal) && pVal.FormUID == frmreldemoconsolidadauid && pVal.ItemUID == "btnPrint")
            {
                if (!Dialogs.Confirm("Confirma a impressão do relatório?"))
                {
                    return;
                }

                Dialogs.Success("Buscando dados para impressão...");

                SAPbouiCOM.Form oForm = SBOApplication.Forms.Item(pVal.FormUID);
                SAPbouiCOM.DataTable dt = oForm.DataSources.DataTables.Item("DT_RELDEMO");

                string strDatainicial = oForm.Items.Item("edMesIni").Specific.value;
                DateTime datainicial = Convert.ToDateTime(DateTime.ParseExact(strDatainicial, "yyyyMMdd", null).ToString("yyyy-MM-dd"));

                string strDataFinal = oForm.Items.Item("edMesFim").Specific.value;
                DateTime datafinal = Convert.ToDateTime(DateTime.ParseExact(strDataFinal, "yyyyMMdd", null).ToString("yyyy-MM-dd"));

                string ccustos = "";
                for (int i = 0; i < ccustos_selecionados_ids.Count; i++)
                {
                    ccustos += $", {ccustos_selecionados_ids[i]} - {ccustos_selecionados_nomes[i]}";
                }

                if (!String.IsNullOrEmpty(ccustos))
                {
                    ccustos = ccustos.Remove(0, 2);
                }

                if (!dt.IsEmpty)
                {
                    string select = "";
                    string sql =
                        $@"IF OBJECT_ID('dbo.rel_dre_ccusto', 'V') IS NOT NULL DROP VIEW dbo.rel_dre_ccusto; EXEC('CREATE VIEW dbo.rel_dre_ccusto AS ";
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        Dialogs.Success($"Buscando dados para impressão... {i} de {dt.Rows.Count}");

                        string descricao = dt.GetValue("Descricao", i);
                        string orcado_mes = FormataMoedaParaSQL(dt.GetValue("Orcado_mes", i));
                        string realizado_mes = FormataMoedaParaSQL(dt.GetValue("Realizado_mes", i));
                        string var_em_real_mes = FormataMoedaParaSQL(dt.GetValue("Variacao_em_real_mes", i));
                        string var_em_perc_mes = FormataPercentualParaSQL(dt.GetValue("Variacao_em_perc_mes", i));
                        string var_mes = dt.GetValue("varmes", i);
                        string orcado_ano = FormataMoedaParaSQL(dt.GetValue("Orcado_ano", i));
                        string realizado_ano = FormataMoedaParaSQL(dt.GetValue("Realizado_ano", i));
                        string var_em_real_ano = FormataMoedaParaSQL(dt.GetValue("Variacao_em_real_ano", i));
                        string var_em_perc_ano = FormataPercentualParaSQL(dt.GetValue("Variacao_em_perc_ano", i));
                        string var_ano = dt.GetValue("varano", i);

                        select +=
                            $@"SELECT ''{descricao}'' as Descricao
                                , {orcado_mes} as Orcado_mes
                                , {realizado_mes} as Realizado_mes
                                , {var_em_real_mes} as Variacao_em_real_mes
                                , {var_em_perc_mes} as Variacao_em_perc_mes
                                , ''{var_mes}'' as varmes
                                , {orcado_ano} as Orcado_ano
                                , {realizado_ano} as Realizado_ano
                                , {var_em_real_ano} as Variacao_em_real_ano
                                , {var_em_perc_ano} as Variacao_em_perc_ano
                                , ''{var_mes}'' as varano
                                ";

                        if (dt.Rows.Count > (i + 1))
                        {
                            select += " UNION ALL ";
                        }
                    }

                    sql += select + " ');";

                    SAPbobsCOM.Recordset rs = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                    rs.DoQuery(sql);

                    SBOApplication.RemoveWindowsMessage(SAPbouiCOM.BoWindowsMessageType.bo_WM_TIMER, true);

                    Dialogs.Success("Imprimindo...");

                    string ReportPath = Application.StartupPath + "/Layout.rpt";

                    if (System.IO.File.Exists(ReportPath))
                    {
                        try
                        {
                            SBOApplication.Menus.Item("4873").Activate();
                            SAPbouiCOM.Form oFormViewPrint = SBOApplication.Forms.ActiveForm;
                            if (oFormViewPrint.TypeEx == "410000003")
                            {
                                ((SAPbouiCOM.EditText)(oFormViewPrint.Items.Item("410000004").Specific)).Value = ReportPath;
                                oFormViewPrint.Items.Item("410000001").Click();
                                oFormViewPrint.Items.Item("410000002").Click();

                                SAPbouiCOM.Form oFormParamsPrint = SBOApplication.Forms.ActiveForm;

                                ((SAPbouiCOM.EditText)(oFormParamsPrint.Items.Item("1000003").Specific)).Value = strDatainicial;
                                ((SAPbouiCOM.EditText)(oFormParamsPrint.Items.Item("1000009").Specific)).Value = strDataFinal;
                                ((SAPbouiCOM.EditText)(oFormParamsPrint.Items.Item("1000015").Specific)).Value = ccustos;

                                oFormParamsPrint.Items.Item("1").Click();
                                oFormParamsPrint.Items.Item("2").Click();
                            }
                        }
                        catch (Exception ex)
                        {
                            Dialogs.Error($"Erro ao imprimir relatório.\nErro: {ex.Message}", true);
                        }
                    }
                    else
                    {
                        Dialogs.Error($"Erro interno. Arquivo {ReportPath} não encontrado.", true);
                    }
                }
                else
                {
                    Dialogs.Error("Nenhum dado encontrado para ser impresso.", true);
                }
            }
        }

        private void ImprimirComCrystal(DateTime datainicial, DateTime datafinal, string ccustos, string ReportPath)
        {
            using (ReportDocument crReport = new ReportDocument())
            {
                crReport.Load(ReportPath);

                //PrinterSettings printerSettings = new PrinterSettings();

                //printerSettings.PrinterName = printername;
                crReport.Refresh();
                crReport.SetParameterValue("De", datainicial.ToShortDateString());
                crReport.SetParameterValue("Ate", datafinal.ToShortDateString());
                crReport.SetParameterValue("CCustos", ccustos);
                crReport.DataSourceConnections[0].SetConnection(ConfigXML.InstanciaBanco, oCompany.CompanyDB, ConfigXML.UsuarioBanco, ConfigXML.SenhaBanco);
                crReport.DataSourceConnections[0].IntegratedSecurity = false;
                crReport.DataSourceConnections[0].SetLogon(ConfigXML.UsuarioBanco, ConfigXML.SenhaBanco);//1q2w3e$R%T

                //crReport.PrintToPrinter(printerSettings, new PageSettings(), false);
                crReport.PrintToPrinter(1, false, 0, 0);

                crReport.Close();

                Dialogs.Success("Relatório impresso com sucesso.");
            }
        }

        private void SBOApplication_MenuEvent(ref SAPbouiCOM.MenuEvent pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;
            if (EventoAntes(pVal) && pVal.MenuUID == "MnuRelDemoConsolidada")
            {
                try
                {
                    try
                    {
                        SAPbouiCOM.Form oForm = SBOApplication.Forms.Item("FrmRelDemoConsolidada");

                        oForm = SBOApplication.Forms.GetForm("FrmRelDemoConsolidada", 0);
                        oForm.Select();
                    }
                    catch (Exception e)
                    {
                        SAPbouiCOM.FormCreationParams creationPackage = null;
                        System.Xml.XmlDocument oXMLDoc = null;
                        oXMLDoc = new System.Xml.XmlDocument();

                        string formtype = "FrmRelDemoConsolidada";
                        string uniqueid = "FrmRelDemoConsolidada";

                        creationPackage = SBOApplication.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_FormCreationParams);
                        creationPackage.FormType = formtype;
                        creationPackage.UniqueID = uniqueid;
                        creationPackage.BorderStyle = SAPbouiCOM.BoFormBorderStyle.fbs_Sizable;

                        oXMLDoc.Load($@"{AppDomain.CurrentDomain.BaseDirectory}\FrmRelDemoConsolidada.srf");
                        creationPackage.XmlData = oXMLDoc.InnerXml;
                        SAPbouiCOM.Form oForm = SBOApplication.Forms.AddEx(creationPackage);

                        oForm.Visible = true;

                        SAPbouiCOM.ChooseFromList oCFL = oForm.ChooseFromLists.Item("CFLCCusto");
                        SAPbouiCOM.Conditions oConds = oCFL.GetConditions();

                        string sql = $"SELECT U_ccusto FROM [@upd_user_ccusto] WHERE U_usuario = '{oCompany.UserName}'";
                        SAPbobsCOM.Recordset rs = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                        rs.DoQuery(sql);
                        int i = 0;
                        while (!rs.EoF)
                        {
                            i++;
                            string ccusto = rs.Fields.Item("U_ccusto").Value;

                            SAPbouiCOM.Condition oCond = oConds.Add();

                            oCond.Alias = "PrcCode";
                            oCond.Operation = SAPbouiCOM.BoConditionOperation.co_EQUAL;
                            oCond.CondVal = ccusto;

                            // põe OR em todos, menos no último.
                            if (i < rs.RecordCount)
                            {
                                oCond.Relationship = SAPbouiCOM.BoConditionRelationship.cr_OR;
                            }

                            rs.MoveNext();
                        }

                        if (rs.RecordCount == 0)
                        {
                            Dialogs.Error("Nenhum centro de custo foi atribuído para o Usuário logado.\nConfigure corretamente no cadastro de usuários.", true);

                            SAPbouiCOM.Condition oCond = oConds.Add();

                            oCond.Alias = "PrcCode";
                            oCond.Operation = SAPbouiCOM.BoConditionOperation.co_EQUAL;
                            oCond.CondVal = "-1";
                        }

                        oCFL.SetConditions(oConds);

                        PopularComboBox("cmbModelo", oForm, "SELECT AbsId, Name FROM OFRT WHERE DocType = 'P' ORDER BY Name ASC");
                        PopularComboBox("cmbPeriodo", oForm, "select distinct category,category from OFPR");
                        try
                        {
                            oForm.Freeze(true);
                            SAPbouiCOM.Matrix mtx = oForm.Items.Item("mtxRel").Specific;
                            mtx.Columns.Item("catid").Visible = false;
                            mtx.Columns.Item("fathernum").Visible = false;
                            mtx.Columns.Item("dummy").Visible = false;

                            mtx.AutoResizeColumns();
                        }
                        finally
                        {
                            oForm.Freeze(false);
                        }
                    }
                }

                catch (Exception e)
                {
                    Dialogs.Error("Erro ao abrir o relatório de demonstração.\nErro: " + e.Message);
                }
            }
        }

        #endregion


        #region :: Dispose

        public void Dispose()
        {
            SBOApplication = null;
            oCompany = null;
        }

        #endregion


        #region :: Utils

        public bool EventoAntes(SAPbouiCOM.BusinessObjectInfo pVal)
        {
            return pVal.BeforeAction;
        }

        public bool EventoDepois(SAPbouiCOM.BusinessObjectInfo pVal)
        {
            return !pVal.BeforeAction;
        }

        public bool EventoAntes(SAPbouiCOM.MenuEvent pVal)
        {
            return pVal.BeforeAction;
        }

        public bool EventoDepois(SAPbouiCOM.MenuEvent pVal)
        {
            return !pVal.BeforeAction;
        }

        public bool EventoAntes(SAPbouiCOM.ItemEvent pVal)
        {
            return pVal.BeforeAction;
        }

        public bool EventoDepois(SAPbouiCOM.ItemEvent pVal)
        {
            return !pVal.BeforeAction;
        }

        private bool EventoEmCadastroUsuarios(SAPbouiCOM.ItemEvent pVal)
        {
            return pVal.FormType == 20700;
        }

        private bool EventoEmCadastroUsuarios(SAPbouiCOM.BusinessObjectInfo pVal)
        {
            return pVal.Type == ((int)SAPbobsCOM.BoObjectTypes.oUsers).ToString();
        }

        private void PopularComboBox(string comboID, SAPbouiCOM.Form oForm, string sql)
        {
            SAPbouiCOM.ComboBox oCombo = oForm.Items.Item(comboID).Specific;
            if (oCombo != null)
            {
                // removendo todos os itens do combo
                for (int i = 1; i < oCombo.ValidValues.Count; i++)
                {
                    oCombo.ValidValues.Remove(i, SAPbouiCOM.BoSearchKey.psk_Index);
                }

                // adicionando todos os itens do combo
                SAPbobsCOM.Recordset rs = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                rs.DoQuery(sql);

                while (!rs.EoF)
                {
                    string id = rs.Fields.Item(0).Value.ToString();
                    string descricao = rs.Fields.Item(1).Value.ToString();
                    oCombo.ValidValues.Add(id, descricao);

                    rs.MoveNext();
                }
            }
            else
            {
                Dialogs.Error("Erro interno. Combo " + comboID + " não encontrado.", true);
            }
        }

        private void __CarregaDatatableMatriz(SAPbouiCOM.ItemEvent pVal, SAPbouiCOM.Form oForm)
        {
            try
            {
                oForm.Freeze(true);
                SBOApplication.StatusBar.SetText("Buscando informações ... Aguarde...", SAPbouiCOM.BoMessageTime.bmt_Long, SAPbouiCOM.BoStatusBarMessageType.smt_Success);

                _strDatainicial = oForm.Items.Item("edMesIni").Specific.value;
                DateTime datainicial = Convert.ToDateTime(DateTime.ParseExact(_strDatainicial, "yyyyMMdd", null).ToString("yyyy-MM-dd"));

                _strDataFinal = oForm.Items.Item("edMesFim").Specific.value;
                DateTime datafinal = Convert.ToDateTime(DateTime.ParseExact(_strDataFinal, "yyyyMMdd", null).ToString("yyyy-MM-dd"));

                SAPbouiCOM.DataTable dt = oForm.DataSources.DataTables.Item("DT_RELDEMO");
                dt.Clear();

                _ccustos = String.Join(",", ccustos_selecionados_ids.Select(x => "'" + x + "'"));

                string sql =
                    $@"
                    SELECT 
	                    REPLICATE('_',ISNULL(tb0.IndentChar,1)) + tb0.Name as 'Descricao'
	                    , tb0.CatId
	                    , tb0.FatherNum
	                    , '     ' as varmes
	                    , '     ' as varano
	
	                    , FORMAT(0, 'C', 'pt-br') as 'Orcado_mes'
	                    , FORMAT(0, 'C', 'pt-br') as 'Realizado_mes'
	                    , FORMAT(0, 'C', 'pt-br')  as 'Variacao_em_real_mes'
	                    , FORMAT(0, 'N', 'pt-br') + '%'  as 'Variacao_em_perc_mes'

	                    , FORMAT(0, 'C', 'pt-br') as 'Orcado_ano'
	                    , FORMAT(0, 'C', 'pt-br') as 'Realizado_ano'
	                    , FORMAT(0, 'C', 'pt-br')  as 'Variacao_em_real_ano'
	                    , FORMAT(0, 'N', 'pt-br') + '%'  as 'Variacao_em_perc_ano'
                        , SubSum
                        , Dummy
                        {_campos_contas_formula}
                        {_campos_operacao_formula}

                    FROM [dbo].[OFRC] tb0 
                    WHERE tb0.[TemplateId] = '{_modelo}'      -- modelo financeiro		  
                    ORDER BY tb0.[VisOrder]";

                dt.ExecuteQuery(sql);

                SAPbouiCOM.Matrix mtx = oForm.Items.Item("mtxRel").Specific;
                SAPbouiCOM.Columns oColumns = mtx.Columns;

                oColumns.Item("descricao").DataBind.Bind(dt.UniqueID, "Descricao");
                oColumns.Item("orcado_mes").DataBind.Bind(dt.UniqueID, "Orcado_mes");
                oColumns.Item("real_mes").DataBind.Bind(dt.UniqueID, "Realizado_mes");
                oColumns.Item("var_s_mes").DataBind.Bind(dt.UniqueID, "Variacao_em_real_mes");
                oColumns.Item("var_p_mes").DataBind.Bind(dt.UniqueID, "Variacao_em_perc_mes");
                oColumns.Item("orcado_ano").DataBind.Bind(dt.UniqueID, "Orcado_ano");
                oColumns.Item("real_ano").DataBind.Bind(dt.UniqueID, "Realizado_ano");
                oColumns.Item("var_s_ano").DataBind.Bind(dt.UniqueID, "Variacao_em_real_ano");
                oColumns.Item("var_p_ano").DataBind.Bind(dt.UniqueID, "Variacao_em_perc_ano");
                oColumns.Item("catid").DataBind.Bind(dt.UniqueID, "catid");
                oColumns.Item("fathernum").DataBind.Bind(dt.UniqueID, "fathernum");
                oColumns.Item("varmes").DataBind.Bind(dt.UniqueID, "varmes");
                oColumns.Item("varano").DataBind.Bind(dt.UniqueID, "varano");
                oColumns.Item("dummy").DataBind.Bind(dt.UniqueID, "Dummy");

                SBOApplication.RemoveWindowsMessage(SAPbouiCOM.BoWindowsMessageType.bo_WM_TIMER, true);

                Dictionary<int, GrupoConta> catid_grupo_conta = new Dictionary<int, GrupoConta>() { };
                int r = 0;
                for (int i = (dt.Rows.Count - 1); i >= 0; i--)
                {
                    r++;
                    Dialogs.Success($"Organizando dados... {r} de {dt.Rows.Count + 1}");

                    int catID = dt.GetValue("CatId", i);
                    int fatherNum = dt.GetValue("FatherNum", i);

                    var grupo = new GrupoContaFolha(catID);
                    grupo.TotalOrcadoMes = grupo.CalcularTotalOrcadoMes();
                    grupo.TotalOrcadoAno = grupo.CalcularTotalOrcadoAno();

                    grupo.TotalRealizadoMes = grupo.CalcularRealizadoMes(_empresa);
                    grupo.TotalRealizadoAno = grupo.CalcularTotalRealizadoAno(_empresa);

                    grupo.VarMesReal = grupo.VariacaoEmReal(grupo.TotalOrcadoMes, grupo.TotalRealizadoMes);
                    grupo.VarAnoReal = grupo.VariacaoEmReal(grupo.TotalOrcadoAno, grupo.TotalRealizadoAno);

                    grupo.RowInDataSource = i;

                    if (!catid_grupo_conta.ContainsKey(catID))
                    {
                        catid_grupo_conta.Add(catID, grupo);
                    }
                    else
                    {
                        catid_grupo_conta[catID].RowInDataSource = i;
                    }

                    // guardando quais formulas serão aplicadas na conta corrente
                    grupo.OrganizarFormulas(dt, i);

                    if (fatherNum == 0)
                    {
                        continue;
                    }

                    if (!catid_grupo_conta.ContainsKey(fatherNum))
                    {
                        var grupoPai = GrupoConta.Clone(catid_grupo_conta[catID], fatherNum);
                        catid_grupo_conta.Add(fatherNum, grupoPai);
                    }
                    else
                    {
                        var grupoPai = GrupoConta.Concatena(catid_grupo_conta[catID], catid_grupo_conta[fatherNum]);
                        catid_grupo_conta[fatherNum] = grupoPai;
                    }

                    SBOApplication.RemoveWindowsMessage(SAPbouiCOM.BoWindowsMessageType.bo_WM_TIMER, true);
                }


                // inserindo totalizadores no datatable
                var c = 1;
                foreach (var catIDConta in catid_grupo_conta)
                {
                    Dialogs.Success($"Criando totalizadores... {c} de {catid_grupo_conta.Count}");
                    var conta = catIDConta.Value;

                    AplicadorDeFormulas.Aplicar(conta, catid_grupo_conta);

                    conta.VarMesPerc = conta.VariacaoEmPercentual(conta.TotalOrcadoMes, conta.TotalRealizadoMes);
                    conta.VarAnoPerc = conta.VariacaoEmPercentual(conta.TotalOrcadoAno, conta.TotalRealizadoAno);

                    dt.SetValue("Orcado_mes", conta.RowInDataSource, FormataDoubleParaMoeda(conta.TotalOrcadoMes));
                    dt.SetValue("Realizado_mes", conta.RowInDataSource, FormataDoubleParaMoeda(conta.TotalRealizadoMes));
                    dt.SetValue("Variacao_em_real_mes", conta.RowInDataSource, FormataDoubleParaMoeda(conta.VarMesReal));
                    dt.SetValue("Variacao_em_perc_mes", conta.RowInDataSource, FormataDoubleParaPercentual(conta.VarMesPerc));

                    dt.SetValue("varmes", conta.RowInDataSource, conta.SetaMes);

                    dt.SetValue("Orcado_ano", conta.RowInDataSource, FormataDoubleParaMoeda(conta.TotalOrcadoAno));
                    dt.SetValue("Realizado_ano", conta.RowInDataSource, FormataDoubleParaMoeda(conta.TotalRealizadoAno));
                    dt.SetValue("Variacao_em_real_ano", conta.RowInDataSource, FormataDoubleParaMoeda(conta.VarAnoReal));
                    dt.SetValue("Variacao_em_perc_ano", conta.RowInDataSource, FormataDoubleParaPercentual(conta.VarAnoPerc));

                    dt.SetValue("varano", conta.RowInDataSource, conta.SetaAno);
                    c++;
                }

                SBOApplication.RemoveWindowsMessage(SAPbouiCOM.BoWindowsMessageType.bo_WM_TIMER, true);

                mtx.LoadFromDataSourceEx();
                mtx.AutoResizeColumns();

                for (int i = 1; i <= mtx.RowCount; i++)
                {
                    Dialogs.Success($"Colorindo as colunas... {i} de {mtx.RowCount}");

                    string str_realizado_mes = mtx.GetCellSpecific("real_mes", i).Value;
                    string str_variacao_real_mes = mtx.GetCellSpecific("var_s_mes", i).Value;
                    string str_variacao_perc_mes = mtx.GetCellSpecific("var_p_mes", i).Value;

                    string str_realizado_ano = mtx.GetCellSpecific("real_ano", i).Value;
                    string str_variacao_real_ano = mtx.GetCellSpecific("var_s_ano", i).Value;
                    string str_variacao_perc_ano = mtx.GetCellSpecific("var_p_ano", i).Value;

                    double realizado_mes = ConvertStringMonetarioFormatadoDouble(str_realizado_mes);
                    double variacao_real_mes = ConvertStringMonetarioFormatadoDouble(str_variacao_real_mes);
                    double variacao_perc_mes = ConvertStringPercentualFormatadoDouble(str_variacao_perc_mes);

                    double realizado_ano = ConvertStringMonetarioFormatadoDouble(str_realizado_ano);
                    double variacao_real_ano = ConvertStringMonetarioFormatadoDouble(str_variacao_real_ano);
                    double variacao_perc_ano = ConvertStringPercentualFormatadoDouble(str_variacao_perc_ano);

                    int corRealMes = GetColor(variacao_real_mes);
                    mtx.CommonSetting.SetCellFontColor(i, 4, corRealMes);
                    mtx.CommonSetting.SetCellFontColor(i, 6, corRealMes);

                    int corPercMes = GetColor(variacao_perc_mes);
                    mtx.CommonSetting.SetCellFontColor(i, 5, corPercMes);

                    int corRealAno = GetColor(variacao_real_ano);
                    mtx.CommonSetting.SetCellFontColor(i, 9, corRealAno);
                    mtx.CommonSetting.SetCellFontColor(i, 11, corRealAno);

                    int corPercAno = GetColor(variacao_perc_ano);
                    mtx.CommonSetting.SetCellFontColor(i, 10, corPercAno);

                    if(mtx.GetCellSpecific("dummy", i).Value == "Y")
                    {
                        mtx.DeleteRow(i);
                    }
                }

                SBOApplication.RemoveWindowsMessage(SAPbouiCOM.BoWindowsMessageType.bo_WM_TIMER, true);

                Dialogs.Success("Ok.");
            }
            catch (Exception e)
            {
                Dialogs.Error("Erro ao carregar dados do relatório.\nErro: " + e.Message);
            }
            finally
            {
                oForm.Freeze(false);
            }
        }

        private int GetColor(double valor)
        {
            return valor == 0 ? _corCelulaDefault : (valor > 0 ? _corVerde : _corVermelha);
        }

        private void CarregarDatatableMatriz(SAPbouiCOM.ItemEvent pVal, SAPbouiCOM.Form oForm)
        {
            LimparMatriz(oForm);
            SAPbouiCOM.ComboBox oComboModelo = oForm.Items.Item("cmbModelo").Specific;
            SAPbouiCOM.ComboBox oComboPeriodo = oForm.Items.Item("cmbPeriodo").Specific;
            SAPbouiCOM.ComboBox oComboEmpresa = oForm.Items.Item("cmbEmpresa").Specific;

            _modelo = oComboModelo.Value;
            _periodo = oComboPeriodo.Value;
            var empresa = oComboEmpresa.Value;
            _empresa = empresa == "1" ? Empresa.ITOne : (empresa == "2" ? Empresa.ITPS : Empresa.Todas);

            if (!String.IsNullOrEmpty(_modelo) && ccustos_selecionados_ids.Count > 0 && !string.IsNullOrEmpty(_periodo))
            {
                _cenarios_orcamento = "";
                bool cenarios_ok = true;
                for (int i = 0; i < ccustos_selecionados_ids.Count; i++)
                {
                    string ccusto = ccustos_selecionados_ids[i];
                    string cenario_orcamento = RetornaCenarioOrcamento(ccusto, _periodo);
                    if (!String.IsNullOrEmpty(cenario_orcamento))
                    {
                        _cenarios_orcamento += "," + cenario_orcamento;
                    }
                    else
                    {
                        cenarios_ok = false;
                        Dialogs.Error($"Nenhum cenário do orçamento foi definido para o centro de custo {ccusto}.", true);
                        break;
                    }
                }

                if (cenarios_ok)
                {
                    _cenarios_orcamento = _cenarios_orcamento.Remove(0, 1);
                    __CarregaDatatableMatriz(pVal, oForm);
                }
            }
        }

        private static string FormatarCampoMonetarioSQL(string campo)
        {
            return $"FORMAT(ISNULL({campo},0), 'C', 'pt-br')";
        }

        private static string FormatarCampoPercentualSQL(string campo)
        {
            return $"CONCAT(FORMAT(ISNULL({campo},0), 'N', 'pt-br'), '%')";
        }

        private string RetornaCenarioOrcamento(string ccusto, string periodo)
        {
            SAPbobsCOM.Recordset rs = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            rs.DoQuery($"SELECT TOP(1) AbsId FROM OBGS WHERE DATEPART(YEAR,FinancYear) = '{periodo}'  AND OcrCode = '{ccusto}' ORDER BY AbsId ASC");

            if (rs.RecordCount > 0)
            {
                return rs.Fields.Item("AbsId").Value.ToString();
            }
            else
            {
                return "";
            }
        }

        private void LimparMatriz(SAPbouiCOM.Form oForm)
        {
            try
            {
                oForm.Freeze(true);
                oForm.Items.Item("mtxRel").Specific.Clear();
            }
            catch (Exception e)
            {
                Dialogs.Error("Erro ao limpar matriz.\nErro: " + e.Message);
            }
            finally
            {
                oForm.Freeze(false);
            }
        }

        private double ConvertStringMonetarioFormatadoDouble(string valor)
        {
            double res = 0;
            Double.TryParse(valor.Replace("R$ ", "").Replace(".", ""), out res);
            return res;
        }

        private double ConvertStringPercentualFormatadoDouble(string valor)
        {
            double res = 0;
            Double.TryParse(valor.Replace("%", "").Replace(".", ""), out res);
            return res;
        }

        private string FormataDoubleParaMoeda(double valor)
        {
            return string.Format(CultureInfo.GetCultureInfo("pt-BR"), "{0:C}", valor);
        }

        private string FormataDoubleParaPercentual(double valor)
        {
            return string.Format(CultureInfo.GetCultureInfo("pt-BR"), "{0:0,0.00}%", valor);
        }

        private string FormataMoedaParaSQL(string valor)
        {
            return valor.Replace("R$ ", "").Replace(".", "").Replace(",", ".");
        }

        private string FormataPercentualParaSQL(string valor)
        {
            return valor.Replace("%", "").Replace(".", "").Replace(",", ".");
        }

        public string SQLDeclareTableID(string nome_tabela)
        {
            return $@"DECLARE @table_id INT;
					IF(SELECT COUNT(*) FROM {nome_tabela}) > 0
						SELECT @table_id = (SELECT TOP (1) (CAST(Code as INT) + 1) FROM {nome_tabela} ORDER BY CAST(Code as INT) DESC) 
					ELSE
						SELECT @table_id = '1'; ";
        }

        private void SalvarTXT(string data)
        {
            string fullpath = Application.StartupPath + "\\sql_dre.txt";

            try
            {
                using (StreamWriter sw = new StreamWriter(fullpath, false))
                {
                    sw.WriteLine(data);
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                Dialogs.Error("Erro interno. Erro ao salvar sql em arquivo de texto para validação.\nErro: " + e.Message, true);
            }
        }



        #endregion
    }
}