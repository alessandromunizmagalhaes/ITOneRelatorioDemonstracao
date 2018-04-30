using System;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace ITOneRelatorioDemonstracao
{
    static class ConfigXML
    {
        public static string InstanciaBanco { get; set; }
        public static string UsuarioBanco { get; set; }
        public static string SenhaBanco { get; set; }

        public static bool GetXMLData()
        {
            string xml_path = Application.StartupPath + "/config.xml";
            try
            {
                XmlDocument xml = new XmlDocument();
                if (File.Exists(xml_path))
                {
                    xml.Load(xml_path);

                    const string def_value_instancia = "_INSTANCIA_";
                    const string def_value_usuario = "_USUARIO_BANCO_";
                    const string def_value_senha = "_SENHA_BANCO_";
                    const string def_nome_tag = "_NOME_DA_TAG_";

                    const string def_key_instancia = "instanciaBanco";
                    const string def_key_usuario = "usuarioBanco";
                    const string def_key_senha = "senhaBanco";

                    string def_msg = $"Configuração inválida.\nCorrija o valor da tag '{def_nome_tag}'.\nDeseja abrir o arquivo? Tenha certeza que você está apto para isto.";

                    string instancia_banco = xml.SelectSingleNode($"/config/{def_key_instancia}").InnerText;
                    string usuario_banco = xml.SelectSingleNode($"/config/{def_key_usuario}").InnerText;
                    string senha_banco = xml.SelectSingleNode($"/config/{def_key_senha}").InnerText;

                    if (instancia_banco != def_value_instancia && usuario_banco != def_value_usuario && senha_banco != def_value_senha)
                    {
                        InstanciaBanco = instancia_banco;
                        UsuarioBanco = usuario_banco;
                        SenhaBanco = senha_banco;

                        return true;
                    }
                    else if (instancia_banco == def_value_instancia)
                    {
                        if (Dialogs.Confirm(def_msg.Replace(def_nome_tag, def_key_instancia)))
                        {
                            AbrirArquivoNaTela(xml_path);
                        }
                    }
                    else if (usuario_banco == def_value_usuario)
                    {
                        if (Dialogs.Confirm(def_msg.Replace(def_nome_tag, def_key_usuario)))
                        {
                            AbrirArquivoNaTela(xml_path);
                        }
                    }
                    else if (senha_banco == def_value_senha)
                    {
                        if (Dialogs.Confirm(def_msg.Replace(def_nome_tag, def_key_senha)))
                        {
                            AbrirArquivoNaTela(xml_path);
                        }
                    }
                }
                else
                {
                    Dialogs.Error($"O arquivo de configuração '{xml_path}' não foi encontrado.\nNão será possível realizar esta ação.", true);
                }
            }
            catch (Exception ex)
            {
                Dialogs.Error($"Erro ao buscar configurações no arquivo '{xml_path}'.\nErro: " + ex.Message, true);
            }

            return false;
        }

        private static void AbrirArquivoNaTela(string filename)
        {
            System.Diagnostics.Process.Start(filename);
        }
    }
}
