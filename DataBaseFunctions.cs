using SAPbobsCOM;
using System;

namespace ITOneRelatorioDemonstracao
{
    public class DataBaseFunctions
    {
        public DataBaseFunctions(SAPbobsCOM.Company oCompany)
        {
            this.oCompany = oCompany;
        }

        #region :: DECLARAÇÕES

        // public SAPbouiCOM.Application SBO_Application;
        public SAPbobsCOM.Company oCompany;

        private SAPbobsCOM.Recordset sboRecordSet;
        private string strQuery;

        private String MsgErroDB;
        private int CodErroDB;

        public SAPbobsCOM.UserFieldsMD objUserFieldsMD;
        public SAPbobsCOM.UserTablesMD objUserTablesMD;
        public SAPbobsCOM.UserObjectsMD objUserObjectMD;

        #endregion


        #region :: Enumeração de Componentes

        public enum TipoCampo
        {
            tALPHA = 0,
            tMEMO = 1,
            tNUMBER = 2,
            tDATE = 3,
            tBOOL = 4,
            tPrice = 5,
            tPercent = 6,
            tSum = 7,
            tTime = 8
        }

        public enum TipoTabela
        {
            tUSER = 0,
            tSBO = 1
        }

        public enum TipoObjeto
        {
            tNOOBJECT = 0,
            tMASTERDATA = 1,
            tMASTERLINE = 2,
            tDOCUMENT = 3,
            tDOCUMENT_Line = 4
        }

        #endregion


        #region :: Documentação

        /// <summary>
        /// Funções universais para manipulação de campos. Com este recurso otimizamos tempo na criação de propriedades de usuários e do sistema SAP Businss one
        /// Todos os campos, criados, removidos ou atualizados utilizarão as funções listadas abaixo.
        /// </summary>
        /// <param name ="cTable">Tabela de usuário </param>
        /// <param name ="cDescription">Descrição de tabela ou campo de usuário</param>
        /// <param name ="cTipo">Tipo do Objeto do sistema (SAP ou Usuário)</param>
        /// <param name ="TipoTabela">Tipo de tabela do sistema (SAP ou Usuário)</param>
        /// <returns></returns>

        #endregion


        #region :: Criação de Funções

        //Verifica a existencia de tabela de usuário 
        public bool fExiteTabela(string cTable)
        {
            sboRecordSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            strQuery = "Select Count(*) from OUTB where TableName = '" + cTable + "'";
            sboRecordSet.DoQuery(strQuery);

            if (sboRecordSet.Fields.Item(0).Value <= 0)
            {
                sboRecordSet = null;
                strQuery = null;
                return false;
            }
            else
            {
                strQuery = null;
                return true;
            }
        }

        public bool fExiteTabelaUDO_OPRD(string cTable0, string cTable1, string cTable2, string cTable3, string cTable4, string cTable5, string cTable6)
        {
            sboRecordSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            strQuery = "Select Count(*) from OUTB where TableName in('" + cTable0 + "','" + cTable1 + "','" + cTable2 + "','" + cTable3 + "','" + cTable4 + "','" + cTable5 + "','" + cTable6 + "')";
            sboRecordSet.DoQuery(strQuery);

            if (sboRecordSet.Fields.Item(0).Value <= 0)
            {
                sboRecordSet = null;
                strQuery = null;
                return false;
            }
            else
            {
                strQuery = null;
                return true;
            }
        }


        //Verifica se UDO Existe
        public bool fExiteUDO(string cTable)
        {
            sboRecordSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            strQuery = "Select Count(*) from OUDO where TableName = '" + cTable + "'";
            sboRecordSet.DoQuery(strQuery);

            if (sboRecordSet.Fields.Item(0).Value <= 0)
            {
                sboRecordSet = null;
                strQuery = null;
                return false;
            }
            else
            {
                strQuery = null;
                return true;
            }
        }

        //Excluir tabela de usuário
        public bool fExcluirTabela(string cTable)
        {
            GC.Collect();
            //SBO_Application.StatusBar.SetText("Removendo tabela [" + cTable + "]", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
            SAPbobsCOM.UserTablesMD objUserTablesMD;
            objUserTablesMD = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserTables);
            objUserTablesMD.TableName = cTable;
            objUserTablesMD.GetByKey(cTable);

            CodErroDB = objUserTablesMD.Remove();
            if (CodErroDB != 0)
            {
                oCompany.GetLastError(out CodErroDB, out string MsgErroDB);
                //SBO_Application.MessageBox("Erro ao remover tabela [" + cTable + "]. Motivo: " + MsgErroDB + "....");
                System.Runtime.InteropServices.Marshal.ReleaseComObject(objUserTablesMD);
                objUserTablesMD = null;
                return false;
            }
            else
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(objUserTablesMD);
                objUserTablesMD = null;
                return true;
            }

        }

        //Criar tabela de usuário

        private void AddUserTable(string Name, string Description, SAPbobsCOM.BoUTBTableType Type)
        {
            ////****************************************//**********************************
            // The UserTablesMD represents a meta-data object which allows us
            // to add\remove tables, change a table name etc.
            ////*********************//*********************//**********************************
            GC.Collect();
            SAPbobsCOM.UserTablesMD oUserTablesMD = null;
            ////*********************//*********************//**********************************
            // In any meta-data operation there should be no other object "alive"
            // but the meta-data object, otherwise the operation will fail.
            // This restriction is intended to prevent a collisions
            ////*********************//*********************//**********************************
            // the meta-data object needs to be initialized with a
            // regular UserTables object
            oUserTablesMD = ((SAPbobsCOM.UserTablesMD)(oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserTables)));
            ////*********************//*****************************
            // when adding user tables or fields to the SBO DB
            // use a prefix identifying your partner name space
            // this will prevent collisions between different
            // partners add-ons
            // SAP's name space prefix is "BE_"
            ////*********************//*****************************		
            // set the table parameters
            oUserTablesMD.TableName = Name;
            oUserTablesMD.TableDescription = Description;
            oUserTablesMD.TableType = Type;
            // Add the table
            // This action add an empty table with 2 default fields
            // 'Code' and 'Name' which serve as the key
            // in order to add your own User Fields
            // see the AddUserFields.frm in this project
            // a privat, user defined, key may be added
            // see AddPrivateKey.frm in this project
            CodErroDB = oUserTablesMD.Add();
            // check for errors in the process
            if (CodErroDB != 0)
            {
                if (CodErroDB == -1)
                {
                }
                else
                {
                    oCompany.GetLastError(out CodErroDB, out MsgErroDB);
                    // SBO_Application.StatusBar.SetText("Erro ao criar Tabela [" + Name + "] --> [" + Description + "] - " + MsgErroDB + "!", SAPbouiCOM.BoMessageTime.bmt_Long, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                }
            }
            else
            {
                // SBO_Application.StatusBar.SetText("Tabela [" + Name + "] --> [" + Description + "] - Criada com Sucesso!", SAPbouiCOM.BoMessageTime.bmt_Long, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            }
            oUserTablesMD = null;
            GC.Collect(); // Release the handle to the table
        }

        //Verificar existencia de campos de usuário
        public bool fExiteCampo(string cTable, string cField)
        {

            sboRecordSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            strQuery = string.Format("Select Count(*) From CUFD (NOLOCK) Where TableID='{0}' and AliasID='{1}'", cTable, cField);
            sboRecordSet.DoQuery(strQuery);
            sboRecordSet.MoveFirst();
            if (sboRecordSet.Fields.Item(0).Value <= 0)
            {
                sboRecordSet = null;
                return false;
            }
            else
            {
                sboRecordSet = null;
                return true;
            }

        }

        //Verificar se campo existe dentro de tabela
        public int fFieldId(string cTabela, string cCampo)
        {
            sboRecordSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            strQuery = " SELECT FieldID FROM CUFD (NOLOCK) ";
            strQuery += " Where TableID='" + cTabela + "' and ";
            strQuery += " AliasID='" + cCampo + "'";

            sboRecordSet.DoQuery(strQuery);
            sboRecordSet.MoveFirst();

            if (sboRecordSet.Fields.Item(0).Value >= 0)
            {
                return sboRecordSet.Fields.Item(0).Value;
            }
            else
            {
                sboRecordSet = null;
                return -1;
            }
        }

        // Verifica se exite valor valido atribuido a campo de usuario
        public bool fExisteValorValido(string cTabela, int cCampoID, string Valor)
        {
            sboRecordSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            strQuery = "Select Count(*) From UFD1 (NOLOCK) ";
            strQuery += "Where TableID='" + cTabela + "' and ";
            strQuery += "      FieldID='" + cCampoID.ToString() + "' and";
            strQuery += "      FldValue='" + Valor + "'";
            sboRecordSet.DoQuery(strQuery);
            sboRecordSet.MoveFirst();

            if (sboRecordSet.Fields.Item(0).Value <= 0)
            {
                sboRecordSet = null;
                return false;
            }
            else
            {
                sboRecordSet = null;
                return true;
            }



        }

        //Criação de campos de usuários em tabela de usuário ou em tabela do Sistema SAP
        public void fCriaCampo(string cTabela, string cCampo, string cDescricao, TipoCampo cTipo, short nsize, TipoTabela cTipoTabela)
        {
            string cTabelaAux;

            cTabelaAux = "";
            //TipoTabela: 0=user, 1=SAPB1
            if (cTipoTabela == 0)
            {
                cTabelaAux = "@" + cTabela;
            }
            else if (cTipoTabela == TipoTabela.tSBO)
            {
                cTabelaAux = cTabela;
            }

            if (!fExiteCampo(cTabelaAux, cCampo))
            {

                GC.Collect();
                objUserFieldsMD = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserFields);
                objUserFieldsMD.TableName = cTabela;
                objUserFieldsMD.Name = cCampo;
                objUserFieldsMD.Description = cDescricao;

                if (cTipo == TipoCampo.tPrice)
                {
                    objUserFieldsMD.Type = SAPbobsCOM.BoFieldTypes.db_Float;
                    objUserFieldsMD.SubType = SAPbobsCOM.BoFldSubTypes.st_Price;
                }
                else if (cTipo == TipoCampo.tPercent)
                {
                    objUserFieldsMD.Type = SAPbobsCOM.BoFieldTypes.db_Float;
                    objUserFieldsMD.SubType = SAPbobsCOM.BoFldSubTypes.st_Percentage;
                }
                else if (cTipo == TipoCampo.tSum)
                {
                    objUserFieldsMD.Type = SAPbobsCOM.BoFieldTypes.db_Float;
                    objUserFieldsMD.SubType = SAPbobsCOM.BoFldSubTypes.st_Sum;
                }
                else if (cTipo == TipoCampo.tALPHA)
                {
                    objUserFieldsMD.Type = SAPbobsCOM.BoFieldTypes.db_Alpha;
                }
                else if (cTipo == TipoCampo.tDATE)
                {
                    objUserFieldsMD.Type = SAPbobsCOM.BoFieldTypes.db_Date;
                }
                else if (cTipo == TipoCampo.tTime)
                {
                    objUserFieldsMD.Type = SAPbobsCOM.BoFieldTypes.db_Date;
                    objUserFieldsMD.SubType = SAPbobsCOM.BoFldSubTypes.st_Time;
                }
                else if (cTipo == TipoCampo.tNUMBER)
                {
                    objUserFieldsMD.Type = SAPbobsCOM.BoFieldTypes.db_Numeric;
                }
                else if (cTipo == TipoCampo.tMEMO)
                {
                    objUserFieldsMD.Type = SAPbobsCOM.BoFieldTypes.db_Memo;
                }
                objUserFieldsMD.EditSize = nsize;
                // Adding the Field to the Table
                CodErroDB = objUserFieldsMD.Add();

                // Check for errors
                if (CodErroDB != 0)
                {
                    oCompany.GetLastError(out int CodErroDB, out string MsgErroDB);
                    // SBO_Application.MessageBox("Erro na Criação do Campo: " + cCampo + " " + CodErroDB.ToString() + " - " + MsgErroDB);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(objUserFieldsMD);
                    objUserFieldsMD = null;

                }
                else
                {
                    // SBO_Application.StatusBar.SetText("Campo: [" + cCampo + "] da tabela [" + cTabela + "] criado com sucesso!", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(objUserFieldsMD);
                    objUserFieldsMD = null;

                }

            }
            else
            {

            }

        }

        //Remoção de campos de usuários em tabelas de usuário ou em tabela do Sistema SAP
        public bool fExcluiCampo(string cTabela, string cCampo, TipoTabela cTipoTabela)
        {
            string cTabelaAux = null;
            //TipoTabela: 0=user, 1=SAPB1
            if (cTipoTabela == TipoTabela.tUSER)
            {
                cTabelaAux = "@" + cTabela;
            }
            else if (cTipoTabela == TipoTabela.tSBO)
            {
                cTabelaAux = cTabela;
            }
            try
            {
                int FieldId = fFieldId(cTabelaAux, cCampo);
                SAPbobsCOM.UserFieldsMD oUserFieldsMD;

                GC.Collect();
                oUserFieldsMD = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserFields);
                if (oUserFieldsMD.GetByKey(cTabelaAux, FieldId) == false)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(objUserFieldsMD);
                    return false;
                }
                else
                {
                    //removendo campo de tabela
                    CodErroDB = oUserFieldsMD.Remove();
                    if (CodErroDB != 0)
                    {
                        oCompany.GetLastError(out int CodErroDB, out string MsgErroDB);
                        // SBO_Application.StatusBar.SetText("Campo: [" + cCampo + "] da tabela [" + cTabela + "] com erro em sua remoção! Codigo: " + CodErroDB.ToString() + " - Mensagem: " + MsgErroDB, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(objUserFieldsMD);
                        return false;
                    }
                    else
                    {
                        // SBO_Application.StatusBar.SetText("Campo: [" + cCampo + "] da tabela [" + cTabela + "] excluído com sucesso!", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(objUserFieldsMD);
                        return true;
                    }
                }

            }
            catch (Exception ex)
            {
                // SBO_Application.MessageBox("Erro de sistema: " + ex.Message);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(objUserFieldsMD);
                return false;
            }

        }

        // Seta valor com defalut em campo de usuário
        public bool fExisteValorDefaultSet(string cTabela, string cCampoID, string Valor)
        {
            sboRecordSet = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            strQuery = "Select Count(*) From CUFD (NOLOCK) ";
            strQuery += "Where TableID='" + cTabela + "' and ";
            strQuery += "      FieldID='" + cCampoID + "' and";
            strQuery += "      dflt='" + Valor + "'";
            sboRecordSet.DoQuery(strQuery);
            sboRecordSet.MoveFirst();

            if ((sboRecordSet.Fields.Item(0).Value) <= 0)
            {
                sboRecordSet = null;
                return false;
            }
            else
            {
                sboRecordSet = null;
                return true;
            }
        }

        //Adiciona valor valido a campo de usuário
        public void fAdicionaValorValido(string cTabela, string cCampo, string Valor, string Descricao, TipoTabela cTipoTabela)
        {
            //TipoTabela: 0=user, 1=SAPB1
            string cTabelaAux = null;
            if (cTipoTabela == TipoTabela.tUSER)
            {
                cTabelaAux = "@" + cTabela;
            }
            else
            {
                cTabelaAux = cTabela;
            }
            bool valorExiste = false;
            int campoID = fFieldId(cTabelaAux, cCampo);

            if (fExisteValorValido(cTabelaAux, campoID, Valor))
            {
                valorExiste = true;

            }
            else
            {
                GC.Collect();
                SAPbobsCOM.UserFieldsMD objUserFieldsMD;
                objUserFieldsMD = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserFields);
                objUserFieldsMD.GetByKey(cTabelaAux, campoID);
                SAPbobsCOM.ValidValuesMD oValidValues;
                oValidValues = objUserFieldsMD.ValidValues;
                if (!valorExiste)
                {
                    if (oValidValues.Value != "")
                    {
                        oValidValues.Add();
                        oValidValues.SetCurrentLine(oValidValues.Count - 1);
                        oValidValues.Value = Valor;
                        oValidValues.Description = Descricao;
                        CodErroDB = objUserFieldsMD.Update();
                    }
                    else
                    {
                        oValidValues.SetCurrentLine(oValidValues.Count - 1);
                        oValidValues.Value = Valor;
                        oValidValues.Description = Descricao;
                        CodErroDB = objUserFieldsMD.Update();
                    }

                    if (CodErroDB != 0)
                    {
                        oCompany.GetLastError(out int CodErroDB, out string MsgErroDB);
                        // SBO_Application.StatusBar.SetText("Valor válido para o campo: [" + cCampo + "] da tabela [" + cTabela + "] reportado com erro! " + CodErroDB + " - " + MsgErroDB, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);

                    }
                    else
                    {
                        // SBO_Application.StatusBar.SetText("Valor válido para o campo: [" + cCampo + "] da tabela [" + cTabela + "] criado com sucesso!", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);

                    }
                }
                else
                {
                    CodErroDB = objUserFieldsMD.Update();
                    if (CodErroDB != 0)
                    {
                        oCompany.GetLastError(out int CodErroDB, out string MsgErroDB);
                        // SBO_Application.StatusBar.SetText("Valor válido para o campo: [" + cCampo + "] da tabela [" + cTabela + "] reportado com erro! " + CodErroDB.ToString() + " - " + MsgErroDB, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);

                    }
                    else
                    {
                        // SBO_Application.StatusBar.SetText("Valor válido para o campo: [" + cCampo + "] da tabela [" + cTabela + "] atualziado com sucesso!", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);

                    }
                }
            }



        }

        //Aplica valor padrão a campo de usuário
        public bool fSetaValorPadrao(string cTabela, string cCampo, string Valor, TipoTabela cTipoTabela)
        {
            string cTabelaAux = null;
            //TipoTabela: 0=user, 1=SAPB1

            if (cTipoTabela == TipoTabela.tUSER)
            {
                cTabelaAux = "@" + cTabela;
            }
            else
            {
                cTabelaAux = cTabela;
            }

            bool valorExiste = false;
            int campoID = fFieldId(cTabelaAux, cCampo);

            if (fExisteValorValido(cTabelaAux, campoID, Valor))
            {
                valorExiste = true;
            }

            //se existe esse valor válido
            if (valorExiste && (fExisteValorDefaultSet(cTabelaAux, campoID.ToString(), Valor)) == false)
            {
                GC.Collect();
                SAPbobsCOM.UserFieldsMD objUserFieldsMD;
                objUserFieldsMD = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserFields);
                if (objUserFieldsMD.GetByKey(cTabelaAux, campoID))
                    objUserFieldsMD.DefaultValue = Valor;
                CodErroDB = objUserFieldsMD.Update();
                if (CodErroDB != 0)
                {
                    objUserFieldsMD = null;
                    oCompany.GetLastError(out int CodErroDB, out string MsgErroDB);
                    // SBO_Application.StatusBar.SetText("Valor padrão para o campo: [" + cCampo + "] da tabela [" + cTabela + "] reportado com erro! " + CodErroDB.ToString() + " - " + MsgErroDB, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return false;
                }
                else
                {
                    objUserFieldsMD = null;
                    // SBO_Application.StatusBar.SetText("Valor padrão para o campo: [" + cCampo + "] da tabela [" + cTabela + "] setado com sucesso!", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                    return true;
                }

            }
            else
            {
                return false;
            }



        }

        //Aplica campo como obrigatorio
        public bool fSetaCampoObrigatorio(string cTabela, string cCampo, TipoTabela cTipoTabela)
        {
            string cTabelaAux = null;
            //'TipoTabela: 0=user, 1=SAPB1

            if (cTipoTabela == TipoTabela.tUSER)
            {
                cTabelaAux = "@" + cTabela;
            }
            else
            {
                cTabelaAux = cTabela;
            }

            int campoID = fFieldId(cTabelaAux, cCampo);
            SAPbobsCOM.UserFieldsMD objUserFieldsMD;
            GC.Collect();
            objUserFieldsMD = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserFields);
            if (objUserFieldsMD.GetByKey(cTabelaAux, campoID))
            {
                objUserFieldsMD.Mandatory = SAPbobsCOM.BoYesNoEnum.tYES;
                CodErroDB = objUserFieldsMD.Update();
            }

            if (CodErroDB != 0)
            {
                oCompany.GetLastError(out int CodErroDB, out string MsgErroDB);
                // SBO_Application.StatusBar.SetText("Obrigatoriedade para o campo: [" + cCampo + "] da tabela [" + cTabela + "] reportado com sucesso! " + CodErroDB.ToString() + " - " + MsgErroDB, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return false;
            }
            else
            {
                // SBO_Application.StatusBar.SetText("Obrigatoriedade para o campo: [" + cCampo + "] da tabela [" + cTabela + "] setada com sucesso!", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                return true;
            }
        }

        #endregion


        #region :: Manipulações de Campos

        public void ManipulaCampos()
        {

            #region :: Variaveis Default

            string oTable = null;
            string oField = null;
            string oDescription = null;
            string oFieldValue1 = null;
            string oFieldValue2 = null;
            string oFieldValue3 = null;
            string oFieldValue4 = null;
            string oFieldValue5 = null;
            string oFieldValue6 = null;
            string oFieldValue7 = null;
            string oFieldValue8 = null;
            string oFieldValue9 = null;

            string oFieldDesc1 = null;
            string oFieldDesc2 = null;
            string oFieldDesc3 = null;
            string oFieldDesc4 = null;
            string oFieldDesc5 = null;
            string oFieldDesc6 = null;
            string oFieldDesc7 = null;
            string oFieldDesc8 = null;
            string oFieldDesc9 = null;

            short nsize = 0;

            #endregion


            #region :: Configuração de Dados do Cadastro do Item - CAMPO: Participa de Consolidação

            oTable = "OITM";
            oField = "UPD_CONSOL";
            oDescription = "UPD: Item p/ Consolidação";

            oFieldValue1 = "S";
            oFieldValue2 = "N";

            oFieldDesc1 = "Sim";
            oFieldDesc2 = "Não";
            nsize = 1;

            if (!fExiteCampo(oTable, oField))
            {
                fCriaCampo(oTable, oField, oDescription, TipoCampo.tALPHA, nsize, TipoTabela.tSBO);
                fAdicionaValorValido(oTable, oField, oFieldValue1, oFieldDesc1, TipoTabela.tSBO);
                fAdicionaValorValido(oTable, oField, oFieldValue2, oFieldDesc2, TipoTabela.tSBO);
                fSetaValorPadrao(oTable, oField, oFieldValue2, TipoTabela.tSBO);
            }

            #endregion


            #region :: Criação da Tabela UPD_USER_CCUSTO

            // tabela que armazena todas as consolidações que foram feitas.
            oTable = "UPD_USER_CCUSTO";
            oDescription = "UPD: Usuários - CCusto";
            if (!fExiteTabela(oTable))
            {
                AddUserTable(oTable, oDescription, BoUTBTableType.bott_NoObject);

                oField = "usuario";
                oDescription = "Usuário";
                nsize = 50;
                fCriaCampo(oTable, oField, oDescription, TipoCampo.tALPHA, nsize, TipoTabela.tUSER);

                oField = "ccusto";
                oDescription = "Centro Custo";
                nsize = 50;
                fCriaCampo(oTable, oField, oDescription, TipoCampo.tALPHA, nsize, TipoTabela.tUSER);
            }

            #endregion

        }

        #endregion
    }
}