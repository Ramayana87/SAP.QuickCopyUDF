using Apzon.Commons;
using DevExpress.Xpo;
using RestSharp;
using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SAP.QuickCopyUDF
{
    /// <summary>
    /// Helper class for SAP Business One operations using DI Service (Service Layer)
    /// This class uses REST API calls to Service Layer - NOT DI API
    /// Note: SAPbobsCOM is only used for enum types (BoFieldTypes, BoFldSubTypes, etc.)
    /// </summary>
    public static class FunctionHelper
    {
        public static string serviceAdress = "";
        public static string sessionId = "";

        public static string AddUdt(DataRow row)
        {
            try
            {
                var json = @"{";
                json += Function.GetJsonString("TableName", Function.ToString(row["TableName"]));
                json += Function.GetJsonString("TableDescription", Function.ToString(row["Descr"]));
                switch (Function.ParseInt(row["ObjectType"]))
                {
                    case 0:
                        json += Function.GetJsonString("TableType", "bott_NoObject");
                        break;
                    case 1:
                        json += Function.GetJsonString("TableType", "bott_MasterData");
                        break;
                    case 2:
                        json += Function.GetJsonString("TableType", "bott_MasterDataLines");
                        break;
                    case 3:
                        json += Function.GetJsonString("TableType", "bott_Document");
                        break;
                    case 4:
                        json += Function.GetJsonString("TableType", "bott_DocumentLines");
                        break;
                }

                if (Function.ToString(row["Archivable"]).Equals("Y"))
                {
                    json += Function.GetJsonString("Archivable", Function.ParseInt(row["ObjectType"]) == 0 ? "tNO" : "tYES");
                }
                else
                {
                    json += Function.GetJsonString("Archivable", "tNO");
                }
                json += Function.GetJsonString("ArchiveDateField", Function.ToString(row["ArchivDate"]));
                json += @"}";

                var response = GetResponseService("UserTablesMD", json);

                return Function.ToString(row["TableName"]) + "\t" + response.Content;
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, new StackTrace(new StackFrame(0)).ToString().Substring(5, new StackTrace(new StackFrame(0)).ToString().Length - 5), ex.Message);
                return Function.ToString(row["TableName"]) + "\t" + ex.Message;
            }
        }

        public static string AddUdf(UserFieldsImpl field)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append("{");
                sb.Append(Function.GetJsonString("Name", field.Name));
                sb.Append(Function.GetJsonString("Size", field.Size));
                sb.Append(Function.GetJsonString("Description", field.Description));
                sb.Append(Function.GetJsonString("TableName", field.TableName));
                sb.Append(Function.GetJsonString("FieldID", field.FieldID));
                sb.Append(Function.GetJsonString("Mandatory", field.Mandatory));
                sb.Append(Function.GetJsonString("Type", field.Type.ToString()));
                sb.Append(Function.GetJsonString("SubType", field.SubType.ToString()));
                sb.Append(Function.GetJsonString("EditSize", field.EditSize));

                if (!string.IsNullOrEmpty(field.LinkedTable))
                {
                    sb.Append(Function.GetJsonString("LinkedTable", field.LinkedTable));
                }
                if (!string.IsNullOrEmpty(field.DefaultValue))
                {
                    sb.Append(Function.GetJsonString("DefaultValue", field.DefaultValue));
                }
                if (!string.IsNullOrEmpty(field.LinkedUDO))
                {
                    sb.Append(Function.GetJsonString("LinkedUDO", field.LinkedUDO));
                }
                if (!string.IsNullOrEmpty(field.LinkedObject))
                {
                    sb.Append(Function.GetJsonString("LinkedSystemObject", field.LinkedObject));
                }
                if (field.ValidValues != null && field.ValidValues.Count > 0)
                {
                    sb.Append(@"""ValidValuesMD"":[");
                    foreach (var item in field.ValidValues)
                    {
                        sb.Append(@"{");
                        if (field.SubType == BoFldSubTypes.st_Time)
                        {
                            sb.Append(Function.GetJsonString("Value", Function.ParseInt(item.Value.Replace(":", "")).ToString("0000")));
                        }
                        else
                        {
                            sb.Append(Function.GetJsonString("Value", item.Value));
                        }

                        sb.Append(Function.GetJsonString("Description", item.Description));
                        sb.Append(@"},");
                    }
                    sb.Remove(sb.Length - 1, 1);
                    sb.Append("]");
                }
                sb.Append(@"}");

                var response = GetResponseService("UserFieldsMD", sb.ToString());

                return field.TableName + "\t" + field.Name + "\t" + response.Content + (string.IsNullOrEmpty(response.Content) ? "" : "\n" + sb.ToString());
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, new StackTrace(new StackFrame(0)).ToString().Substring(5, new StackTrace(new StackFrame(0)).ToString().Length - 5), ex.Message);
                return field.TableName + "." + field.Name + "\t" + ex.Message;
            }
        }


        public static string LinkUdf(UserFieldsImpl field)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append("{");
                sb.Append(Function.GetJsonString("TableName", field.TableName));
                sb.Append(Function.GetJsonString("FieldID", field.FieldID));

                if (!string.IsNullOrEmpty(field.LinkedUDO))
                {
                    sb.Append(Function.GetJsonString("LinkedUDO", field.LinkedUDO));
                }
                sb.Remove(sb.Length - 1, 1);
                sb.Append(@"}");
                var key = string.Format("(TableName='{0}',FieldID={1})", field.TableName, field.FieldID);

                var response = GetResponseService("UserFieldsMD", sb.ToString(), Method.PATCH, key);

                return field.TableName + "\t" + field.Name + "\t" + response.Content + (string.IsNullOrEmpty(response.Content) ? "" : "\n" + sb.ToString());
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, new StackTrace(new StackFrame(0)).ToString().Substring(5, new StackTrace(new StackFrame(0)).ToString().Length - 5), ex.Message);
                return field.TableName + "." + field.Name + "\t" + ex.Message;
            }
        }

        public static string UpdateUdf(UserFieldsImpl field)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append("{");
                sb.Append(Function.GetJsonString("Name", field.Name));
                sb.Append(Function.GetJsonString("Size", field.Size));
                sb.Append(Function.GetJsonString("Description", field.Description));
                sb.Append(Function.GetJsonString("TableName", field.TableName));
                sb.Append(Function.GetJsonString("FieldID", field.FieldID));
                sb.Append(Function.GetJsonString("Mandatory", field.Mandatory));
                sb.Append(Function.GetJsonString("Type", field.Type.ToString()));
                sb.Append(Function.GetJsonString("SubType", field.SubType.ToString()));
                sb.Append(Function.GetJsonString("EditSize", field.EditSize));

                if (!string.IsNullOrEmpty(field.LinkedTable))
                {
                    sb.Append(Function.GetJsonString("LinkedTable", field.LinkedTable));
                }
                if (!string.IsNullOrEmpty(field.DefaultValue))
                {
                    sb.Append(Function.GetJsonString("DefaultValue", field.DefaultValue));
                }
                if (!string.IsNullOrEmpty(field.LinkedUDO))
                {
                    sb.Append(Function.GetJsonString("LinkedUDO", field.LinkedUDO));
                }
                if (!string.IsNullOrEmpty(field.LinkedObject))
                {
                    sb.Append(Function.GetJsonString("LinkedSystemObject", field.LinkedObject));
                }

                if (field.ValidValues != null && field.ValidValues.Count > 0)
                {
                    sb.Append(@"""ValidValuesMD"":[");
                    foreach (var item in field.ValidValues)
                    {
                        sb.Append(@"{");
                        if (field.SubType == BoFldSubTypes.st_Time)
                        {
                            sb.Append(Function.GetJsonString("Value", Function.ParseInt(item.Value.Replace(":", "")).ToString("0000")));
                        }
                        else
                        {
                            sb.Append(Function.GetJsonString("Value", item.Value));
                        }

                        sb.Append(Function.GetJsonString("Description", item.Description));
                        sb.Append(@"},");
                    }
                    sb.Remove(sb.Length - 1, 1);
                    sb.Append("]");
                }

                sb.Append(@"}");

                var key = string.Format("(TableName='{0}',FieldID={1})", field.TableName, field.FieldID);

                var response = GetResponseService("UserFieldsMD", sb.ToString(), Method.PATCH, key);

                return field.TableName + "\t" + field.Name + "\t" + response.Content + (string.IsNullOrEmpty(response.Content) ? "" : "\n" + sb.ToString());
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, new StackTrace(new StackFrame(0)).ToString().Substring(5, new StackTrace(new StackFrame(0)).ToString().Length - 5), ex.Message);
                return field.TableName + "." + field.Name + "\t" + ex.Message;
            }
        }

        public static string AddUdo(UserObjectsImpl udo)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append("{");
                sb.Append(Function.GetJsonString("Code", udo.Code));
                sb.Append(Function.GetJsonString("Name", udo.Name));
                sb.Append(Function.GetJsonString("TableName", udo.TableName));
                sb.Append(Function.GetJsonString("LogTableName", udo.LogTableName));
                sb.Append(Function.GetJsonString("ObjectType", udo.ObjectType));
                sb.Append(Function.GetJsonString("ManageSeries", udo.ManageSeries));
                sb.Append(Function.GetJsonString("CanDelete", udo.CanDelete));
                sb.Append(Function.GetJsonString("CanClose", udo.CanClose));
                sb.Append(Function.GetJsonString("CanCancel", udo.CanCancel));
                sb.Append(Function.GetJsonString("CanFind", udo.CanFind));
                sb.Append(Function.GetJsonString("CanYearTransfer", udo.CanYearTransfer));
                sb.Append(Function.GetJsonString("CanCreateDefaultForm", udo.CanCreateDefaultForm));
                sb.Append(Function.GetJsonString("CanLog", udo.CanLog));
                sb.Append(Function.GetJsonString("OverwriteDllfile", udo.OverwriteDllfile));
                sb.Append(Function.GetJsonString("UseUniqueFormType", udo.UseUniqueFormType));
                sb.Append(Function.GetJsonString("CanArchive", udo.CanArchive));
                sb.Append(Function.GetJsonString("MenuItem", udo.MenuItem));
                sb.Append(Function.GetJsonString("MenuCaption", udo.MenuCaption));
                sb.Append(Function.GetJsonString("FatherMenuID", udo.FatherMenuID));
                sb.Append(Function.GetJsonString("Position", udo.Position));
                sb.Append(Function.GetJsonString("EnableEnhancedForm", udo.EnableEnhancedForm));
                sb.Append(Function.GetJsonString("RebuildEnhancedForm", udo.RebuildEnhancedForm));
                sb.Append(Function.GetJsonString("FormSRF", udo.FormSRF));
                sb.Append(Function.GetJsonString("MenuUID", udo.MenuUID));

                if (udo.ChildTables != null && udo.ChildTables.Count > 0)
                {
                    sb.Append(@"""UserObjectMD_ChildTables"":[");
                    foreach (var item in udo.ChildTables)
                    {
                        sb.Append(@"{");
                        sb.Append(Function.GetJsonString("Code", item.Code));
                        sb.Append(Function.GetJsonString("SonNumber", item.SonNumber));
                        sb.Append(Function.GetJsonString("TableName", item.TableName));
                        sb.Append(Function.GetJsonString("LogTableName", item.LogTableName));
                        sb.Append(Function.GetJsonString("ObjectName", item.ObjectName));
                        sb.Remove(sb.Length - 1, 1);
                        sb.Append(@"},");
                    }
                    sb.Remove(sb.Length - 1, 1);
                    sb.Append("],");
                }

                if (udo.FindColumns != null && udo.FindColumns.Count > 0)
                {
                    sb.Append(@"""UserObjectMD_FindColumns"":[");
                    foreach (var item in udo.FindColumns)
                    {
                        sb.Append(@"{");
                        sb.Append(Function.GetJsonString("Code", item.Code));
                        sb.Append(Function.GetJsonString("ColumnNumber", item.ColumnNumber));
                        sb.Append(Function.GetJsonString("ColumnAlias", item.ColumnAlias));
                        sb.Append(Function.GetJsonString("ColumnDescription", item.ColumnDescription));
                        sb.Remove(sb.Length - 1, 1);
                        sb.Append(@"},");
                    }
                    sb.Remove(sb.Length - 1, 1);
                    sb.Append("],");
                }

                if (udo.FormColumns != null && udo.FormColumns.Count > 0)
                {
                    sb.Append(@"""UserObjectMD_FormColumns"":[");
                    foreach (var item in udo.FormColumns)
                    {
                        sb.Append(@"{");
                        sb.Append(Function.GetJsonString("Code", item.Code));
                        sb.Append(Function.GetJsonString("FormColumnNumber", item.FormColumnNumber));
                        sb.Append(Function.GetJsonString("SonNumber", item.SonNumber));
                        sb.Append(Function.GetJsonString("FormColumnAlias", item.FormColumnAlias));
                        sb.Append(Function.GetJsonString("FormColumnDescription", item.FormColumnDescription));
                        sb.Append(Function.GetJsonString("Editable", item.Editable));
                        sb.Remove(sb.Length - 1, 1);
                        sb.Append(@"},");
                    }
                    sb.Remove(sb.Length - 1, 1);
                    sb.Append("],");
                }

                if (udo.EnhancedFormColumns != null && udo.EnhancedFormColumns.Count > 0)
                {
                    sb.Append(@"""UserObjectMD_EnhancedFormColumns"":[");
                    foreach (var item in udo.EnhancedFormColumns)
                    {
                        sb.Append(@"{");
                        sb.Append(Function.GetJsonString("Code", item.Code));
                        sb.Append(Function.GetJsonString("ColumnNumber", item.ColumnNumber));
                        sb.Append(Function.GetJsonString("ChildNumber", item.ChildNumber));
                        sb.Append(Function.GetJsonString("ColumnAlias", item.ColumnAlias));
                        sb.Append(Function.GetJsonString("ColumnDescription", item.ColumnDescription));
                        sb.Append(Function.GetJsonString("ColumnIsUsed", item.ColumnIsUsed));
                        sb.Append(Function.GetJsonString("Editable", item.Editable));
                        sb.Remove(sb.Length - 1, 1);
                        sb.Append(@"},");
                    }
                    sb.Remove(sb.Length - 1, 1);
                    sb.Append("],");
                }
                sb.Remove(sb.Length - 1, 1);
                sb.Append(@"}");

                var response = GetResponseService("UserObjectsMD", sb.ToString());

                return udo.Code + "\t" + udo.Name + "\t" + response.Content + (string.IsNullOrEmpty(response.Content) ? "" : "\n" + sb.ToString());
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, new StackTrace(new StackFrame(0)).ToString().Substring(5, new StackTrace(new StackFrame(0)).ToString().Length - 5), ex.Message);
                return udo.Code + "\t" + udo.Name + "\t" + ex.Message;
            }
        }

        public static string UpdateUdo(UserObjectsImpl udo)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append("{");
                sb.Append(Function.GetJsonString("Name", udo.Name));
                sb.Append(Function.GetJsonString("TableName", udo.TableName));
                sb.Append(Function.GetJsonString("LogTableName", udo.LogTableName));
                sb.Append(Function.GetJsonString("ObjectType", udo.ObjectType));
                sb.Append(Function.GetJsonString("ManageSeries", udo.ManageSeries));
                sb.Append(Function.GetJsonString("CanDelete", udo.CanDelete));
                sb.Append(Function.GetJsonString("CanClose", udo.CanClose));
                sb.Append(Function.GetJsonString("CanCancel", udo.CanCancel));
                sb.Append(Function.GetJsonString("CanFind", udo.CanFind));
                sb.Append(Function.GetJsonString("CanYearTransfer", udo.CanYearTransfer));
                sb.Append(Function.GetJsonString("CanCreateDefaultForm", udo.CanCreateDefaultForm));
                sb.Append(Function.GetJsonString("CanLog", udo.CanLog));
                sb.Append(Function.GetJsonString("OverwriteDllfile", udo.OverwriteDllfile));
                sb.Append(Function.GetJsonString("UseUniqueFormType", udo.UseUniqueFormType));
                sb.Append(Function.GetJsonString("CanArchive", udo.CanArchive));
                sb.Append(Function.GetJsonString("MenuItem", udo.MenuItem));
                sb.Append(Function.GetJsonString("MenuCaption", udo.MenuCaption));
                sb.Append(Function.GetJsonString("FatherMenuID", udo.FatherMenuID));
                sb.Append(Function.GetJsonString("Position", udo.Position));
                sb.Append(Function.GetJsonString("EnableEnhancedForm", udo.EnableEnhancedForm));
                sb.Append(Function.GetJsonString("RebuildEnhancedForm", udo.RebuildEnhancedForm));
                sb.Append(Function.GetJsonString("FormSRF", udo.FormSRF));
                sb.Append(Function.GetJsonString("MenuUID", udo.MenuUID));

                if (udo.ChildTables != null && udo.ChildTables.Count > 0)
                {
                    sb.Append(@"""UserObjectMD_ChildTables"":[");
                    foreach (var item in udo.ChildTables)
                    {
                        sb.Append(@"{");
                        //sb.Append(Function.GetJsonString("Code", item.Code));
                        sb.Append(Function.GetJsonString("SonNumber", item.SonNumber));
                        sb.Append(Function.GetJsonString("TableName", item.TableName));
                        sb.Append(Function.GetJsonString("LogTableName", item.LogTableName));
                        sb.Append(Function.GetJsonString("ObjectName", item.ObjectName));
                        sb.Remove(sb.Length - 1, 1);
                        sb.Append(@"},");
                    }
                    sb.Remove(sb.Length - 1, 1);
                    sb.Append("],");
                }

                if (udo.FindColumns != null && udo.FindColumns.Count > 0)
                {
                    sb.Append(@"""UserObjectMD_FindColumns"":[");
                    foreach (var item in udo.FindColumns)
                    {
                        sb.Append(@"{");
                        //sb.Append(Function.GetJsonString("Code", item.Code));
                        sb.Append(Function.GetJsonString("ColumnNumber", item.ColumnNumber));
                        sb.Append(Function.GetJsonString("ColumnAlias", item.ColumnAlias));
                        sb.Append(Function.GetJsonString("ColumnDescription", item.ColumnDescription));
                        sb.Remove(sb.Length - 1, 1);
                        sb.Append(@"},");
                    }
                    sb.Remove(sb.Length - 1, 1);
                    sb.Append("],");
                }

                if (udo.FormColumns != null && udo.FormColumns.Count > 0)
                {
                    sb.Append(@"""UserObjectMD_FormColumns"":[");
                    foreach (var item in udo.FormColumns)
                    {
                        sb.Append(@"{");
                        //sb.Append(Function.GetJsonString("Code", item.Code));
                        sb.Append(Function.GetJsonString("FormColumnNumber", item.FormColumnNumber));
                        sb.Append(Function.GetJsonString("SonNumber", item.SonNumber));
                        sb.Append(Function.GetJsonString("FormColumnAlias", item.FormColumnAlias));
                        sb.Append(Function.GetJsonString("FormColumnDescription", item.FormColumnDescription));
                        sb.Append(Function.GetJsonString("Editable", item.Editable));
                        sb.Remove(sb.Length - 1, 1);
                        sb.Append(@"},");
                    }
                    sb.Remove(sb.Length - 1, 1);
                    sb.Append("],");
                }

                if (udo.EnhancedFormColumns != null && udo.EnhancedFormColumns.Count > 0)
                {
                    sb.Append(@"""UserObjectMD_EnhancedFormColumns"":[");
                    foreach (var item in udo.EnhancedFormColumns)
                    {
                        sb.Append(@"{");
                        //sb.Append(Function.GetJsonString("Code", item.Code));
                        sb.Append(Function.GetJsonString("ColumnNumber", item.ColumnNumber));
                        sb.Append(Function.GetJsonString("ChildNumber", item.ChildNumber));
                        sb.Append(Function.GetJsonString("ColumnAlias", item.ColumnAlias));
                        sb.Append(Function.GetJsonString("ColumnDescription", item.ColumnDescription));
                        sb.Append(Function.GetJsonString("ColumnIsUsed", item.ColumnIsUsed));
                        sb.Append(Function.GetJsonString("Editable", item.Editable));
                        sb.Remove(sb.Length - 1, 1);
                        sb.Append(@"},");
                    }
                    sb.Remove(sb.Length - 1, 1);
                    sb.Append("],");
                }
                sb.Remove(sb.Length - 1, 1);
                sb.Append(@"}");

                var key = string.Format("('{0}')", udo.Code);

                var response = GetResponseService("UserObjectsMD", sb.ToString(), Method.PATCH, key);

                return udo.Code + "\t" + udo.Name + "\t" + response.Content + (string.IsNullOrEmpty(response.Content) ? "" : "\n" + sb.ToString());
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, new StackTrace(new StackFrame(0)).ToString().Substring(5, new StackTrace(new StackFrame(0)).ToString().Length - 5), ex.Message);
                return udo.Code + "\t" + udo.Name + "\t" + ex.Message;
            }
        }

        /// <summary>
        /// Make REST API calls to SAP Business One DI Service (Service Layer)
        /// This uses Service Layer endpoints (/b1s/v1/...) - NOT DI API
        /// </summary>
        /// <param name="serviceName">Service Layer resource name (e.g., "UserFieldsMD", "UserTablesMD")</param>
        /// <param name="jsonString">JSON payload for the request</param>
        /// <param name="method">HTTP method (POST, PATCH, DELETE)</param>
        /// <param name="key">OData key for PATCH/DELETE operations</param>
        /// <returns>REST response from Service Layer</returns>
        private static IRestResponse GetResponseService(string serviceName, string jsonString, Method method = Method.POST, string key = "")
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            // DI Service (Service Layer) endpoint - NOT DI API
            var uri = string.Format("https://{0}/b1s/v1/{1}", serviceAdress, serviceName);
            if (method != Method.POST && !string.IsNullOrEmpty(key))
            {
                uri += key;
            }
            var client = new RestClient(uri);
            var request = new RestRequest(method);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Prefer", "return-no-content");
            request.AddHeader("Content-Type", "application/json;odata=minimalmetadata;charset=utf8");
            request.AddHeader("B1S-ReplaceCollectionsOnPatch", "true");
            request.Timeout = 1800000;
            if (!string.IsNullOrEmpty(sessionId))
                request.AddCookie("B1SESSION", sessionId);
            if (!string.IsNullOrEmpty(jsonString))
                request.AddParameter("undefined", jsonString, ParameterType.RequestBody);
            return client.Execute(request);
        }


    }
}
