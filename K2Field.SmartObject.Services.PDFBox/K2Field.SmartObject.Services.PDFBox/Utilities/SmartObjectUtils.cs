using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SourceCode.SmartObjects.Client;
using SourceCode.Hosting.Client.BaseAPI;

namespace K2Field.SmartObject.Services.PDFBox.Utilities
{
    public static class SmartObjectUtils
    {
        public static string CreateDataFromPDFForm(string smartobjectName, string method, string returnProperty, Data.PDFInfo info, Dictionary<string, Data.PDFField> fields)
        {
            SourceCode.SmartObjects.Client.SmartObject smoReturn = null;
            SmartObjectClientServer smoSvr = new SmartObjectClientServer();

            Dictionary<string, string> Settings = new Dictionary<string, string>();
            string returnId = string.Empty;

            try
            {
                smoSvr.CreateConnection();
                smoSvr.Connection.Open(GetSmOConnection().ToString());

                SourceCode.SmartObjects.Client.SmartObject smoParam = smoSvr.GetSmartObject(smartobjectName);

                foreach (KeyValuePair<string, Data.PDFField> field in fields)
                {
                    smoParam.Properties[field.Key.Replace(" ", "_")].Value = field.Value.FieldValue;
                }

                smoParam.MethodToExecute = method;

                smoReturn = smoSvr.ExecuteScalar(smoParam);

                returnId = smoReturn.Properties[returnProperty].Value;

            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (smoSvr.Connection.IsConnected)
                {
                    smoSvr.Connection.Close();
                }
                smoSvr.Connection.Dispose();
            }
            return returnId;
        }


        public static Dictionary<string, string> GetAllSettings(string[] SettingKeys)
        {
            SourceCode.SmartObjects.Client.SmartObject smoReturn = null;
            SmartObjectClientServer smoSvr = new SmartObjectClientServer();

            Dictionary<string, string> Settings = new Dictionary<string, string>();
            
            try
            {
                smoSvr.CreateConnection();
                smoSvr.Connection.Open(GetSmOConnection().ToString());

                foreach (string setting in SettingKeys)
                {
                    SourceCode.SmartObjects.Client.SmartObject smoParam = smoSvr.GetSmartObject("K2_Generic_Settings_Shared_Setting");
                    smoParam.Properties["SectionName"].Value = "Yammer";
                    smoParam.Properties["SettingKey"].Value = setting;
                    smoParam.MethodToExecute = "Load";

                    smoReturn = smoSvr.ExecuteScalar(smoParam);

                    Settings.Add(setting, GetValue(smoReturn));
                    smoReturn = null;
                }

            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (smoSvr.Connection.IsConnected)
                {
                    smoSvr.Connection.Close();   
                }
                smoSvr.Connection.Dispose();
            }
            return Settings;
        }

        public static string GetValue(SourceCode.SmartObjects.Client.SmartObject smo)
        {
            if (smo == null || smo.IsEmpty)
            {
                return "";
            }
            else
            {
                return smo.Properties["SettingValue"] != null ? smo.Properties["SettingValue"].Value : "";
            }
        }

        public static string GetSettingValue(string section, string key)
        {
            SourceCode.SmartObjects.Client.SmartObject smoReturn = null;

            SmartObjectClientServer smoSvr = new SmartObjectClientServer();
            try
            {
                smoSvr.CreateConnection();
                smoSvr.Connection.Open(GetSmOConnection().ToString());

                SourceCode.SmartObjects.Client.SmartObject smoParam = smoSvr.GetSmartObject("K2_Generic_Settings_Shared_Setting");

                smoParam.Properties["SectionName"].Value = section;
                smoParam.Properties["SettingKey"].Value = key;

                smoParam.MethodToExecute = "Load";

                smoReturn = smoSvr.ExecuteScalar(smoParam);

                if (smoReturn.IsEmpty)
                {
                    return "";
                }
                else
                {
                    return smoReturn.Properties["SettingValue"] != null ? smoReturn.Properties["SettingValue"].Value : "";
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                smoSvr.Connection.Close();
                smoSvr.Connection.Dispose();
            }
        }

        private static SmartObjectClientServer SmartObjectClientServer()
        {
            SmartObjectClientServer smoSvr = new SmartObjectClientServer();
            smoSvr.CreateConnection();
            smoSvr.Connection.Open(GetSmOConnection().ToString());
            
            return smoSvr;
        }

        private static SCConnectionStringBuilder GetSmOConnection()
        {
            SCConnectionStringBuilder scbuilder = new SCConnectionStringBuilder();
            scbuilder.Authenticate = true;
            scbuilder.Host = "localhost";
            scbuilder.Integrated = true;
            scbuilder.IsPrimaryLogin = true;
            scbuilder.Port = 5555;
            //scbuilder.WindowsDomain = "Denallix";
            //scbuilder.UserID = "k2service";
            //scbuilder.Password = "K2pass!";
            //scbuilder.SecurityLabelName = "K2";

            return scbuilder;
        }
    }
}
