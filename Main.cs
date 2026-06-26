using Apzon.Commons;
using Apzon.Commons.Helper;
using DevExpress.Utils.Extensions;
using DevExpress.XtraEditors;
using Newtonsoft.Json;
using RestSharp;
using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SAP.QuickCopyUDF
{
    public partial class frm_Main : Form
    {
        public string SessionId { get; set; }

        private string _serviceAddress;

        public string SQLConnectionSource { get; set; }

        public string HanaConnectionString { get; set; }

        public string HanaConnectionSource { get; set; }

        public DatabaseHanaClient _httpClient;

        public DatabaseHanaClient _httpClientSource;

        public string sourceType { get; set; }

        public frm_Main()
        {
            InitializeComponent();
        }

        private void frm_Home_Load(object sender, EventArgs e)
        {
            try
            {
                var dtServerType = new DataTable();
                dtServerType.Columns.Add("Code", typeof(string));
                dtServerType.Columns.Add("Name", typeof(string));
                var rowNew = dtServerType.NewRow();
                rowNew[0] = "S";
                rowNew[1] = "SQL SERVER";
                dtServerType.Rows.Add(rowNew);
                var rowNewH = dtServerType.NewRow();
                rowNewH[0] = "H";
                rowNewH[1] = "HANA SERVER";
                dtServerType.Rows.Add(rowNewH);
                txtServerType.Properties.DataSource = dtServerType;
                txtServerType.EditValue = "S";

                txt_Sql_Server.Text = Function.ToString(ConfigurationManager.AppSettings["Source_Server"]);
                txt_Sql_Database.Text = Function.ToString(ConfigurationManager.AppSettings["Source_DB"]);
                txt_Sql_User.Text = Function.ToString(ConfigurationManager.AppSettings["Source_User"]);
                txt_Sql_Pass.Text = Function.ToString(ConfigurationManager.AppSettings["Source_Pass"]);

                txt_Hana_Database.Text = Function.ToString(ConfigurationManager.AppSettings["CompanyDB"]);
                txt_HanaUser.Text = Function.ToString(ConfigurationManager.AppSettings["HanaUser"]);
                txt_HanaPass.Text = Function.ToString(ConfigurationManager.AppSettings["HanaPass"]);
                txt_ServiceAddress.Text = Function.ToString(ConfigurationManager.AppSettings["ServiceAddress"]);
                txt_ServerHana.Text = Function.ToString(ConfigurationManager.AppSettings["ServerHana"]);
                txt_SapUser.Text = Function.ToString(ConfigurationManager.AppSettings["SapUser"]);
                txt_SapPass.Text = Function.ToString(ConfigurationManager.AppSettings["SapPass"]);

                btn_CreateUDT.Enabled = false;
                btn_CreateUDF.Enabled = false;
                btn_CreateUDO.Enabled = false;
                btn_LinkUDF.Enabled = false;
                btn_UpdateUDF.Enabled = false;
                btn_DelUDF.Enabled = false;
                btn_LogOut.Enabled = false;

                btn_CopyUDOManual.Enabled = false;
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, ex);
                MessageBox.Show(ex.Message);
            }
        }

        private async void login_Click(object sender, EventArgs e)
        {
            try
            {
                btn_Login.Enabled = false;
                GetConnectionString();
                var companyDb = txt_Hana_Database.Text;
                var sapPass = txt_SapPass.Text;
                var sapUser = txt_SapUser.Text;
                var ret = await Task.Run(() => Login(companyDb, sapPass, sapUser));
                richTextBox1.Text = ret + "\t" + SessionId;
                if (!string.IsNullOrEmpty(SessionId))
                {
                    btn_CreateUDT.Enabled = true;
                    btn_CreateUDF.Enabled = true;
                    btn_CreateUDO.Enabled = true;
                    btn_LinkUDF.Enabled = true;
                    btn_UpdateUDF.Enabled = true;
                    btn_DelUDF.Enabled = true;
                    btn_LogOut.Enabled = true;
                    btn_CopyUDOManual.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, ex);
            }
            finally
            {
                btn_Login.Enabled = true;
            }
        }

        public string Login(string companyDb, string sapPass, string sapUser)
        {
            var jsonString = string.Format(@"{{""CompanyDB"": ""{0}"",""Password"":""{1}"",""UserName"":""{2}""}}", companyDb, sapPass, sapUser);
            var response = GetResponseService("Login", jsonString);
            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.Created)
            {
                var result = JsonConvert.DeserializeObject<SessionObj>(response.Content);
                if (string.IsNullOrEmpty(result.SessionId))
                {
                    return "Can not login to server. Session response is null";
                }
                SessionId = result.SessionId;
                FunctionHelper.sessionId = result.SessionId;
                FunctionHelper.serviceAdress = _serviceAddress;
                return string.Empty;
            }
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var result = JsonConvert.DeserializeObject<LayerMessageObject>(response.Content);
                return string.Format("{0}-{1}", result.error.code, result.error.message.value);
            }
            return string.Format("Login failed. {0}", response.ErrorMessage);
        }

        private async void btn_CreateUDT_Click(object sender, EventArgs e)
        {
            try
            {
                btn_CreateUDT.Enabled = false;
                richTextBox1.Clear();
                GetConnectionString();
                var tableNameText = txt_TableName.Text;
                await Task.Run(() =>
                {
                    var ds = new DataSet();
                    if (!string.IsNullOrEmpty(tableNameText))
                    {
                        var lstTable = tableNameText.Split(',').ToList();
                        var tableNames = string.Join(",", lstTable.Select(m => "'" + m + "'").ToArray());
                        if (sourceType.Equals("S"))
                            ds = ExecuteData(string.Format("SELECT * FROM OUTB WHERE TableName IN ({0}); ", tableNames));
                        else
                        {
                            var dtOUTB = _httpClientSource.ExecuteDataTable(string.Format("SELECT * FROM OUTB WHERE \"TableName\" IN ({0}); ", tableNames));
                            ds.Tables.Add(dtOUTB);
                        }
                    }
                    else
                    {
                        if (sourceType.Equals("S"))
                            ds = ExecuteData("SELECT * FROM OUTB;");
                        else
                            ds = _httpClientSource.ExecuteData("SELECT * FROM OUTB;");
                    }
                    if (ds.Tables.Count > 0)
                    {
                        var dt = ds.Tables[0];
                        foreach (DataRow item in dt.Rows)
                        {
                            if (CheckExistTable(Function.ToString(item["TableName"]))) continue;
                            var ret = AddUdt(item);
                            AppendLog(ret);
                        }
                    }
                });
                MessageBox.Show("DONE");
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, ex);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                btn_CreateUDT.Enabled = true;
            }
        }

        protected string AddUdt(DataRow row)
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

        private async void btn_CreateUDF_Click(object sender, EventArgs e)
        {
            try
            {
                btn_CreateUDF.Enabled = false;
                richTextBox1.Clear();
                GetConnectionString();
                var tableNameText = txt_TableName.Text;
                await Task.Run(() =>
                {
                var ds = new DataSet();
                if (!string.IsNullOrEmpty(tableNameText))
                {
                    var lstTable = tableNameText.Split(',').ToList();
                    var tableNames = string.Join(",", lstTable.Select(m => "'" + m + "'").ToArray());
                    if (sourceType.Equals("S"))
                        ds = ExecuteData(string.Format("SELECT * FROM CUFD WHERE TableID IN ({0}); SELECT * FROM UFD1 WHERE TableID IN ({0}); ", tableNames));
                    else
                    {
                        var dtCUFD = _httpClientSource.ExecuteDataTable(string.Format("SELECT * FROM CUFD WHERE \"TableID\" IN ({0}); ", tableNames));
                        var dtUFD1 = _httpClientSource.ExecuteDataTable(string.Format("SELECT * FROM UFD1 WHERE \"TableID\" IN ({0}); ", tableNames));
                        ds.Tables.Add(dtCUFD);
                        ds.Tables.Add(dtUFD1);
                    }
                }
                else
                {
                    if (sourceType.Equals("S"))
                        ds = ExecuteData(@"
                        SELECT * FROM CUFD WHERE TableID NOT IN ( 'ODLN','ORDN','ORDR','OPCH','ORPC','OPDN','ORPD','OPOR','OIGN','OIGE','ODRF','OTSI','OTPI','OPRQ','OPQT','ODPI','ODPO','ORRR','OWTQ'
										                        ,'OCIN','OCPI','OCPV','OCSI','OCSV','OIEI','OINV','OOEI','OPRR','ORIN','OSFC','OSFI','OWTR'	,'BTNT','BTNT1','OSRI','OIBT'
										                        ,'CIN1','CPI1','CPV1','CSI1','CSV1','DLN1','DPI1','DPO1','DRF1','IEI1','IGE1','IGN1','INV1','OEI1','PCH1','PDN1','POR1','PQT1'
										                        ,'PRQ1','PRR1','RDN1','RDR1','RIN1','RPC1','RPD1','RRR1','SFC1','SFI1','WTQ1','WTR1','ODBN','ODSN','OPDF')
                            AND TableID NOT LIKE 'A%' AND TableID NOT LIKE 'U%' AND TableID NOT LIKE 'S%' AND TableID NOT LIKE 'PDF%' AND ( ( TableID LIKE '@APZ_%' ) OR ( TableID NOT LIKE '@APZ_%' AND TableID NOT LIKE '@A%') );
                        SELECT * FROM UFD1 WHERE TableID NOT IN ( 'ODLN','ORDN','ORDR','OPCH','ORPC','OPDN','ORPD','OPOR','OIGN','OIGE','ODRF','OTSI','OTPI','OPRQ','OPQT','ODPI','ODPO','ORRR','OWTQ'
										                        ,'OCIN','OCPI','OCPV','OCSI','OCSV','OIEI','OINV','OOEI','OPRR','ORIN','OSFC','OSFI','OWTR'	,'BTNT','BTNT1','OSRI','OIBT'
										                        ,'CIN1','CPI1','CPV1','CSI1','CSV1','DLN1','DPI1','DPO1','DRF1','IEI1','IGE1','IGN1','INV1','OEI1','PCH1','PDN1','POR1','PQT1'
										                        ,'PRQ1','PRR1','RDN1','RDR1','RIN1','RPC1','RPD1','RRR1','SFC1','SFI1','WTQ1','WTR1','ODBN','ODSN','OPDF')
                            AND TableID NOT LIKE 'A%' AND TableID NOT LIKE 'U%' AND TableID NOT LIKE 'S%' AND TableID NOT LIKE 'PDF%' AND ( ( TableID LIKE '@APZ_%' ) OR ( TableID NOT LIKE '@APZ_%' AND TableID NOT LIKE '@A%') );
                        ");
                    else
                        ds = _httpClientSource.ExecuteData(@"
                        SELECT * FROM CUFD WHERE ""TableID"" NOT IN ( 'ODLN','ORDN','ORDR','OPCH','ORPC','OPDN','ORPD','OPOR','OIGN','OIGE','ODRF','OTSI','OTPI','OPRQ','OPQT','ODPI','ODPO','ORRR','OWTQ'
										                            ,'OCIN','OCPI','OCPV','OCSI','OCSV','OIEI','OINV','OOEI','OPRR','ORIN','OSFC','OSFI','OWTR'	,'BTNT','BTNT1','OSRI','OIBT'
										                            ,'CIN1','CPI1','CPV1','CSI1','CSV1','DLN1','DPI1','DPO1','DRF1','IEI1','IGE1','IGN1','INV1','OEI1','PCH1','PDN1','POR1','PQT1'
										                            ,'PRQ1','PRR1','RDN1','RDR1','RIN1','RPC1','RPD1','RRR1','SFC1','SFI1','WTQ1','WTR1')
                            AND ""TableID"" NOT LIKE 'A%' AND ""TableID"" NOT LIKE 'U%' AND ""TableID"" NOT LIKE 'S%' AND ( ( ""TableID"" LIKE '@APZ_%' ) OR ( ""TableID"" NOT LIKE '@APZ_%' AND ""TableID"" NOT LIKE '@A%') );
                        SELECT * FROM UFD1 WHERE ""TableID"" NOT IN ( 'ODLN','ORDN','ORDR','OPCH','ORPC','OPDN','ORPD','OPOR','OIGN','OIGE','ODRF','OTSI','OTPI','OPRQ','OPQT','ODPI','ODPO','ORRR','OWTQ'
										                            ,'OCIN','OCPI','OCPV','OCSI','OCSV','OIEI','OINV','OOEI','OPRR','ORIN','OSFC','OSFI','OWTR'	,'BTNT','BTNT1','OSRI','OIBT'
										                            ,'CIN1','CPI1','CPV1','CSI1','CSV1','DLN1','DPI1','DPO1','DRF1','IEI1','IGE1','IGN1','INV1','OEI1','PCH1','PDN1','POR1','PQT1'
										                            ,'PRQ1','PRR1','RDN1','RDR1','RIN1','RPC1','RPD1','RRR1','SFC1','SFI1','WTQ1','WTR1')
                            AND ""TableID"" NOT LIKE 'A%' AND ""TableID"" NOT LIKE 'U%' AND ""TableID"" NOT LIKE 'S%' AND ( ( ""TableID"" LIKE '@APZ_%' ) OR ( ""TableID"" NOT LIKE '@APZ_%' AND ""TableID"" NOT LIKE '@A%') );
                        ");
                }
                if (ds.Tables.Count > 0)
                {
                    var dt = ds.Tables[0];
                    foreach (DataRow item in dt.Rows)
                    {
                        var field = new UserFieldsImpl();
                        field.TableName = Function.ToString(item["TableID"]);
                        field.Name = Function.ToString(item["AliasID"]);
                        if (CheckExistsUdfColumn(field.TableName, field.Name)) continue;
                        field.Size = Function.ParseInt(item["SizeID"]);
                        field.Description = Function.ToString(item["Descr"]);
                        field.FieldID = Function.ParseInt(item["FieldID"]);
                        field.Mandatory = Function.ToString(item["NotNull"]) == "Y" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
                        field.EditSize = Function.ParseInt(item["EditSize"]);
                        var type = Function.ToString(item["TypeID"]);
                        switch (type)
                        {
                            case "B": field.Type = BoFieldTypes.db_Float; break;
                            case "M": field.Type = BoFieldTypes.db_Memo; break;
                            case "D": field.Type = BoFieldTypes.db_Date; break;
                            case "N": field.Type = BoFieldTypes.db_Numeric; break;
                            default:
                                field.Type = BoFieldTypes.db_Alpha;
                                break;
                        }
                        var subType = Function.ToString(item["EditType"]);
                        //if (new[] { BoFieldTypes.db_Float, BoFieldTypes.db_Date, BoFieldTypes.db_Alpha }.Contains(field.Type))
                        //{
                        switch (subType)
                        {
                            case "S": field.SubType = BoFldSubTypes.st_Sum; break;
                            case "P": field.SubType = BoFldSubTypes.st_Price; break;
                            case "%": field.SubType = BoFldSubTypes.st_Percentage; break;
                            case "Q": field.SubType = BoFldSubTypes.st_Quantity; break;
                            case "T": field.SubType = BoFldSubTypes.st_Time; break;
                            case "A": field.SubType = BoFldSubTypes.st_Address; break;
                            case "M": field.SubType = BoFldSubTypes.st_Measurement; break;
                            case "L": field.SubType = BoFldSubTypes.st_Link; break;
                            case "R": field.SubType = BoFldSubTypes.st_Rate; break;
                            case "B": field.SubType = BoFldSubTypes.st_Link; break;

                            default:
                                field.SubType = BoFldSubTypes.st_None;
                                break;
                        }
                        //}

                        field.LinkedTable = Function.ToString(item["RTable"]);
                        field.DefaultValue = Function.ToString(item["Dflt"]);
                        // chưa link udo khi chưa có UDO
                        //field.LinkedUDO = Function.ToString(item["RelUDO"]);
                        field.LinkedObject = Function.ToString(item["RelSO"]);

                        var valid = ds.Tables[1].AsEnumerable().Where(t => Function.ToString(t["TableID"]) == field.TableName && Function.ParseInt(t["FieldID"]) == field.FieldID);
                        if (valid.Count() > 0)
                        {
                            field.ValidValues = new List<ValidValuesMDImpl>();
                            foreach (var v in valid)
                            {
                                var values = new ValidValuesMDImpl();
                                values.Description = Function.ToString(v["Descr"]);
                                values.Value = Function.ToString(v["FldValue"]);
                                field.ValidValues.Add(values);
                            }
                        }

                        var ret = AddUdf(field);
                        AppendLog(ret);
                    }
                }
                });
                MessageBox.Show("DONE");
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, ex);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                btn_CreateUDF.Enabled = true;
            }
        }

        public string AddUdf(UserFieldsImpl field)
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

        private async void btn_LinkUDF_Click(object sender, EventArgs e)
        {
            try
            {
                btn_LinkUDF.Enabled = false;
                richTextBox1.Clear();
                GetConnectionString();
                var tableNameText = txt_TableName.Text;
                await Task.Run(() =>
                {
                var ds = new DataSet();
                if (!string.IsNullOrEmpty(tableNameText))
                {
                    var lstTable = tableNameText.Split(',').ToList();
                    var tableNames = string.Join(",", lstTable.Select(m => "'" + m + "'").ToArray());
                    if (sourceType.Equals("S"))
                        ds = ExecuteData(string.Format("SELECT * FROM CUFD WHERE TableID IN ({0}) AND RelUDO <> '';", tableNames));
                    else
                        ds = _httpClientSource.ExecuteData(string.Format("SELECT * FROM CUFD WHERE \"TableID\" IN ({0}) AND \"RelUDO\" <> '';", tableNames));
                }
                else
                {
                    if (sourceType.Equals("S"))
                        ds = ExecuteData(@"
                        SELECT * FROM CUFD WHERE TableID NOT IN( 'ODLN','ORDN','ORDR','OPCH','ORPC','OPDN','ORPD','OPOR','OIGN','OIGE','ODRF','OTSI','OTPI','OPRQ','OPQT','ODPI','ODPO','ORRR','OWTQ'
										                        ,'OCIN','OCPI','OCPV','OCSI','OCSV','OIEI','OINV','OOEI','OPRR','ORIN','OSFC','OSFI','OWTR'	,'BTNT','BTNT1','OSRI','OIBT'
										                        ,'CIN1','CPI1','CPV1','CSI1','CSV1','DLN1','DPI1','DPO1','DRF1','IEI1','IGE1','IGN1','INV1','OEI1','PCH1','PDN1','POR1','PQT1'
										                        ,'PRQ1','PRR1','RDN1','RDR1','RIN1','RPC1','RPD1','RRR1','SFC1','SFI1','WTQ1','WTR1','ODBN','ODSN','OPDF')
                            AND ( RelUDO <> '' )
                            AND TableID NOT LIKE 'A%' AND TableID NOT LIKE 'U%' AND TableID NOT LIKE 'S%' AND TableID NOT LIKE 'PDF%'
					        AND ( ( TableID LIKE '@APZ_%' ) OR ( TableID NOT LIKE '@APZ_%' AND TableID NOT LIKE '@A%') );");
                    else
                    {
                        var dtCUFD = _httpClientSource.ExecuteDataTable(@"
                            SELECT * FROM CUFD WHERE ""TableID"" NOT IN( 'ODLN','ORDN','ORDR','OPCH','ORPC','OPDN','ORPD','OPOR','OIGN','OIGE','ODRF','OTSI','OTPI','OPRQ','OPQT','ODPI','ODPO','ORRR','OWTQ'
										                        ,'OCIN','OCPI','OCPV','OCSI','OCSV','OIEI','OINV','OOEI','OPRR','ORIN','OSFC','OSFI','OWTR'	,'BTNT','BTNT1','OSRI','OIBT'
										                        ,'CIN1','CPI1','CPV1','CSI1','CSV1','DLN1','DPI1','DPO1','DRF1','IEI1','IGE1','IGN1','INV1','OEI1','PCH1','PDN1','POR1','PQT1'
										                        ,'PRQ1','PRR1','RDN1','RDR1','RIN1','RPC1','RPD1','RRR1','SFC1','SFI1','WTQ1','WTR1','ODBN','ODSN','OPDF')
                                AND ( ""RelUDO"" <> '' )
                                AND ""TableID"" NOT LIKE 'A%' AND ""TableID"" NOT LIKE 'U%' AND ""TableID"" NOT LIKE 'S%' AND TableID NOT LIKE 'PDF%'
					            AND ( ( ""TableID"" LIKE '@APZ_%' ) OR ( ""TableID"" NOT LIKE '@APZ_%' AND ""TableID"" NOT LIKE '@A%') );");
                        ds.Tables.Add(dtCUFD);
                    }
                }

                if (ds.Tables.Count > 0)
                {
                    var dt = ds.Tables[0];
                    foreach (DataRow item in dt.Rows)
                    {
                        var field = new UserFieldsImpl();
                        field.TableName = Function.ToString(item["TableID"]);
                        field.Name = Function.ToString(item["AliasID"]);
                        field.LinkedUDO = Function.ToString(item["RelUDO"]);

                        // lấy ra filedID bên target do: có thể ID đã nhảy sang số khác
                        var dtField = GetFiledIDUdf(field.TableName, field.Name);
                        if (dtField.IsNotNull())
                        {
                            field.FieldID = Function.ParseInt(dtField.Rows[0]["FieldID"]);
                        }
                        else
                        {
                            AppendLog(field.TableName + "\t" + field.Name + " not exists!");
                            continue;
                        }

                        var ret = LinkUdf(field);
                        AppendLog(ret);
                    }
                }
                });
                MessageBox.Show("DONE");
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, ex);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                btn_LinkUDF.Enabled = true;
            }
        }

        public string LinkUdf(UserFieldsImpl field)
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

        private async void btn_UpdateUDF_Click(object sender, EventArgs e)
        {
            try
            {
                btn_UpdateUDF.Enabled = false;
                richTextBox1.Clear();
                GetConnectionString();
                var tableNameText = txt_TableName.Text;
                await Task.Run(() =>
                {
                var ds = new DataSet();
                if (!string.IsNullOrEmpty(tableNameText))
                {
                    var lstTable = tableNameText.Split(',').ToList();
                    var tableNames = string.Join(",", lstTable.Select(m => "'" + m.Trim() + "'").ToArray());
                    if (sourceType.Equals("S"))
                        ds = ExecuteData(string.Format("SELECT * FROM CUFD WHERE TableID IN ({0}); SELECT * FROM UFD1 WHERE TableID IN ({0}); ", tableNames));
                    else
                    {
                        var dtCUFD = _httpClientSource.ExecuteDataTable(string.Format("SELECT * FROM CUFD WHERE \"TableID\" IN ({0}); ", tableNames));
                        var dtUFD1 = _httpClientSource.ExecuteDataTable(string.Format("SELECT * FROM UFD1 WHERE \"TableID\" IN ({0}); ", tableNames));
                        ds.Tables.Add(dtCUFD);
                        ds.Tables.Add(dtUFD1);
                    }
                }
                if (ds.Tables.Count > 0)
                {
                    var dt = ds.Tables[0];
                    foreach (DataRow item in dt.Rows)
                    {
                        var field = new UserFieldsImpl();
                        field.TableName = Function.ToString(item["TableID"]);
                        field.Name = Function.ToString(item["AliasID"]);
                        // lấy ra filedID bên target do: có thể ID đã nhảy sang số khác
                        var dtField = GetFiledIDUdf(field.TableName, field.Name);
                        if (dtField.IsNotNull())
                        {
                            field.FieldID = Function.ParseInt(dtField.Rows[0]["FieldID"]);
                        }
                        field.Size = Function.ParseInt(item["SizeID"]);
                        field.Description = Function.ToString(item["Descr"]);
                        field.Mandatory = Function.ToString(item["NotNull"]) == "Y" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
                        field.EditSize = Function.ParseInt(item["EditSize"]);
                        var type = Function.ToString(item["TypeID"]);
                        switch (type)
                        {
                            case "B": field.Type = BoFieldTypes.db_Float; break;
                            case "M": field.Type = BoFieldTypes.db_Memo; break;
                            case "D": field.Type = BoFieldTypes.db_Date; break;
                            case "N": field.Type = BoFieldTypes.db_Numeric; break;
                            default:
                                field.Type = BoFieldTypes.db_Alpha;
                                break;
                        }
                        var subType = Function.ToString(item["EditType"]);
                        switch (subType)
                        {
                            case "S": field.SubType = BoFldSubTypes.st_Sum; break;
                            case "P": field.SubType = BoFldSubTypes.st_Price; break;
                            case "%": field.SubType = BoFldSubTypes.st_Percentage; break;
                            case "Q": field.SubType = BoFldSubTypes.st_Quantity; break;
                            case "T": field.SubType = BoFldSubTypes.st_Time; break;
                            case "A": field.SubType = BoFldSubTypes.st_Address; break;
                            case "M": field.SubType = BoFldSubTypes.st_Measurement; break;
                            case "L": field.SubType = BoFldSubTypes.st_Link; break;
                            case "R": field.SubType = BoFldSubTypes.st_Rate; break;
                            case "B": field.SubType = BoFldSubTypes.st_Link; break;

                            default:
                                field.SubType = BoFldSubTypes.st_None;
                                break;
                        }

                        field.LinkedTable = Function.ToString(item["RTable"]);
                        field.DefaultValue = Function.ToString(item["Dflt"]);
                        field.LinkedUDO = Function.ToString(item["RelUDO"]);
                        field.LinkedObject = Function.ToString(item["RelSO"]);

                        var valid = ds.Tables[1].AsEnumerable().Where(t => Function.ToString(t["TableID"]) == field.TableName && Function.ParseInt(t["FieldID"]) == field.FieldID);
                        if (valid.Count() > 0)
                        {
                            field.ValidValues = new List<ValidValuesMDImpl>();
                            foreach (var v in valid)
                            {
                                // kiểm tra valivalue đã có thì bỏ qua do sap báo lỗi "Duplicate value "
                                var exists = CheckExistsValidValue(field.TableName, field.FieldID, Function.ToString(v["FldValue"]));
                                if (exists) continue;
                                var values = new ValidValuesMDImpl();
                                values.Description = Function.ToString(v["Descr"]);
                                values.Value = Function.ToString(v["FldValue"]);
                                field.ValidValues.Add(values);
                            }
                        }

                        var ret = UpdateUdf(field);
                        AppendLog(ret);
                    }
                }
                else
                    AppendLog("Nhập danh sách bảng cần cập nhật UDF! lưu ý: ValidValue chỉ thêm được value chứ không cập nhật giá trị cũ");
                });
                MessageBox.Show("DONE");
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, ex);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                btn_UpdateUDF.Enabled = true;
            }
        }

        public string UpdateUdf(UserFieldsImpl field)
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

        private async void btn_DelUDF_Click(object sender, EventArgs e)
        {
            try
            {
                btn_DelUDF.Enabled = false;
                richTextBox1.Clear();
                GetConnectionString();
                var tableNameText = txt_TableName.Text;
                await Task.Run(() =>
                {
                    var ds = new DataSet();
                    if (!string.IsNullOrEmpty(tableNameText))
                    {
                        var lstTable = tableNameText.Split(',').ToList();
                        var tableNames = string.Join(",", lstTable.Select(m => "'" + m.Trim() + "'").ToArray());
                        if (sourceType.Equals("S"))
                            ds = ExecuteData(string.Format("SELECT * FROM CUFD WHERE \"TableID\" IN ({0});", tableNames));
                        else
                            ds = _httpClientSource.ExecuteData(string.Format("SELECT * FROM CUFD WHERE \"TableID\" IN ({0});", tableNames));
                    }
                    if (ds.Tables.Count > 0)
                    {
                        var dt = ds.Tables[0];
                        foreach (DataRow item in dt.Rows)
                        {
                            var field = new UserFieldsImpl();
                            field.Name = Function.ToString(item["AliasID"]);
                            field.TableName = Function.ToString(item["TableID"]);
                            field.FieldID = Function.ParseInt(item["FieldID"]);

                            var ret = DeleteUdf(field);
                            AppendLog(ret);
                        }
                    }
                });
                MessageBox.Show("DONE");
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, ex);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                btn_DelUDF.Enabled = true;
            }
        }

        public string DeleteUdf(UserFieldsImpl field)
        {
            try
            {
                var key = string.Format("(TableName='{0}',FieldID={1})", field.TableName, field.FieldID);

                var response = GetResponseService("UserFieldsMD", "", Method.DELETE, key);

                return field.TableName + "\t" + field.Name + "\t" + response.Content + (string.IsNullOrEmpty(response.Content) ? "" : "\n" + key);
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, new StackTrace(new StackFrame(0)).ToString().Substring(5, new StackTrace(new StackFrame(0)).ToString().Length - 5), ex.Message);
                return field.TableName + "." + field.Name + "\t" + ex.Message;
            }
        }

        private async void btn_CreateUDO_Click(object sender, EventArgs e)
        {
            try
            {
                btn_CreateUDO.Enabled = false;
                richTextBox1.Clear();
                GetConnectionString();
                var tableNameText = txt_TableName.Text;
                await Task.Run(() =>
                {
                var ds = new DataSet();
                if (!string.IsNullOrEmpty(tableNameText))
                {
                    var lstTable = tableNameText.Split(',').ToList();
                    var tableNames = string.Join(",", lstTable.Select(m => "'" + m + "'").ToArray());
                    if (sourceType.Equals("S"))
                        ds = ExecuteData(string.Format(@"SELECT * FROM OUDO WHERE Code IN ({0}); " +
                                                        "SELECT * FROM UDO1 WHERE Code IN ({0}); " +
                                                        "SELECT * FROM UDO2 WHERE Code IN ({0}); " +
                                                        "SELECT * FROM UDO3 WHERE Code IN ({0}); " +
                                                        "SELECT * FROM UDO4 WHERE Code IN ({0});", tableNames));
                    else
                        ds = _httpClientSource.ExecuteData(string.Format(@"SELECT * FROM OUDO WHERE ""Code"" IN ({0}); 
                                                                        SELECT * FROM UDO1 WHERE ""Code"" IN ({0}); 
                                                                        SELECT * FROM UDO2 WHERE ""Code"" IN ({0}); 
                                                                        SELECT * FROM UDO3 WHERE ""Code"" IN ({0}); 
                                                                        SELECT * FROM UDO4 WHERE ""Code"" IN ({0});
                                                                        ", tableNames));
                }
                else
                {
                    if (sourceType.Equals("S"))
                        ds = ExecuteData(@"
                                        SELECT * FROM OUDO;
                                        SELECT * FROM UDO1;
                                        SELECT * FROM UDO2;
                                        SELECT * FROM UDO3;
                                        SELECT * FROM UDO4;
                                        ");
                    else
                    {
                        var dtOUDO = _httpClientSource.ExecuteDataTable(@"SELECT * FROM OUDO;");
                        var dtUDO1 = _httpClientSource.ExecuteDataTable(@"SELECT * FROM UDO1;");
                        var dtUDO2 = _httpClientSource.ExecuteDataTable(@"SELECT * FROM UDO2;");
                        var dtUDO3 = _httpClientSource.ExecuteDataTable(@"SELECT * FROM UDO3;");
                        var dtUDO4 = _httpClientSource.ExecuteDataTable(@"SELECT * FROM UDO4;");
                        ds.Tables.Add(dtOUDO);
                        ds.Tables.Add(dtUDO1);
                        ds.Tables.Add(dtUDO2);
                        ds.Tables.Add(dtUDO3);
                        ds.Tables.Add(dtUDO4);
                    }
                }

                if (ds.Tables.Count > 0)
                {
                    var dt = ds.Tables[0];
                    foreach (DataRow item in dt.Rows)
                    {
                        var udo = new UserObjectsImpl();
                        udo.Code = Function.ToString(item["Code"]);
                        udo.Name = Function.ToString(item["Name"]);
                        udo.TableName = Function.ToString(item["TableName"]);
                        udo.LogTableName = Function.ToString(item["LogTable"]);
                        udo.ObjectType = Function.ToString(item["TYPE"]) == "1" ? BoUDOObjType.boud_MasterData : BoUDOObjType.boud_Document;
                        udo.ManageSeries = Function.ToString(item["MngSeries"]) == "Y" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
                        udo.CanDelete = Function.ToString(item["CanDelete"]) == "Y" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
                        udo.CanClose = Function.ToString(item["CanClose"]) == "Y" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
                        udo.CanCancel = Function.ToString(item["CanCancel"]) == "Y" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
                        udo.CanFind = Function.ToString(item["CanFind"]) == "Y" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
                        udo.CanYearTransfer = Function.ToString(item["CanYrTrnsf"]) == "Y" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
                        udo.CanCreateDefaultForm = Function.ToString(item["CanDefForm"]) == "Y" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
                        udo.CanLog = Function.ToString(item["CanLog"]) == "Y" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
                        udo.OverwriteDllfile = Function.ToString(item["OvrWrtDll"]) == "Y" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
                        udo.UseUniqueFormType = Function.ToString(item["UIDFormat"]) == "Y" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
                        udo.CanArchive = Function.ToString(item["CanArchive"]) == "Y" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
                        udo.MenuItem = Function.ToString(item["MenuItem"]) == "Y" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
                        udo.MenuCaption = Function.ToString(item["MenuCapt"]);
                        udo.FatherMenuID = Function.ParseInt(item["FatherMenu"]);
                        udo.Position = Function.ParseInt(item["Position"]);
                        udo.EnableEnhancedForm = Function.ToString(item["CanNewForm"]) == "Y" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
                        udo.RebuildEnhancedForm = Function.ToString(item["IsRebuild"]) == "Y" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
                        udo.FormSRF = Function.ToString(item["NewFormSrf"]);
                        udo.MenuUID = Function.ToString(item["MenuUid"]);

                        var Childs = ds.Tables[1].AsEnumerable().Where(t => Function.ToString(t["Code"]) == udo.Code);
                        if (Childs.Count() > 0)
                        {
                            udo.ChildTables = new List<UserObjectChildTableImpl>();
                            foreach (var child in Childs)
                            {
                                var obj = new UserObjectChildTableImpl();
                                obj.Code = Function.ToString(child["Code"]);
                                obj.SonNumber = Function.ParseInt(child["SonNum"]);
                                obj.TableName = Function.ToString(child["TableName"]);
                                obj.LogTableName = Function.ToString(child["LogName"]);
                                obj.ObjectName = Function.ToString(child["SonName"]);

                                udo.ChildTables.Add(obj);
                            }
                        }

                        var FindColumns = ds.Tables[2].AsEnumerable().Where(t => Function.ToString(t["Code"]) == udo.Code);
                        if (FindColumns.Count() > 0)
                        {
                            udo.FindColumns = new List<UserObjectFindColumnImpl>();
                            foreach (var find in FindColumns)
                            {
                                var obj = new UserObjectFindColumnImpl();
                                obj.Code = Function.ToString(find["Code"]);
                                obj.ColumnNumber = Function.ParseInt(find["ColumnNum"]);
                                obj.ColumnAlias = Function.ToString(find["ColAlias"]);
                                obj.ColumnDescription = Function.ToString(find["ColumnDesc"]);

                                udo.FindColumns.Add(obj);
                            }
                        }

                        var FormColumns = ds.Tables[3].AsEnumerable().Where(t => Function.ToString(t["Code"]) == udo.Code);
                        if (FormColumns.Count() > 0)
                        {
                            udo.FormColumns = new List<UserObjectFormColumnImpl>();
                            foreach (var form in FormColumns)
                            {
                                var obj = new UserObjectFormColumnImpl();
                                obj.Code = Function.ToString(form["Code"]);
                                obj.SonNumber = Function.ParseInt(form["SonNum"]);
                                obj.FormColumnNumber = Function.ParseInt(form["ColumnNum"]);
                                obj.FormColumnAlias = Function.ToString(form["ColAlias"]);
                                obj.FormColumnDescription = Function.ToString(form["ColDesc"]);
                                obj.Editable = Function.ToString(form["ColEdit"]) == "Y" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;

                                udo.FormColumns.Add(obj);
                            }
                        }

                        var EnhancedFormColumns = ds.Tables[4].AsEnumerable().Where(t => Function.ToString(t["Code"]) == udo.Code);
                        if (EnhancedFormColumns.Count() > 0)
                        {
                            udo.EnhancedFormColumns = new List<UserObjectEnhancedFormColumnImpl>();
                            foreach (var enchanced in EnhancedFormColumns)
                            {
                                var obj = new UserObjectEnhancedFormColumnImpl();
                                obj.Code = Function.ToString(enchanced["Code"]);
                                obj.ChildNumber = Function.ParseInt(enchanced["SonNum"]);
                                obj.ColumnNumber = Function.ParseInt(enchanced["ColumnNum"]);
                                obj.ColumnAlias = Function.ToString(enchanced["ColAlias"]);
                                obj.ColumnDescription = Function.ToString(enchanced["ColDesc"]);
                                obj.ColumnIsUsed = Function.ToString(enchanced["ColIsUsed"]) == "Y" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
                                obj.Editable = Function.ToString(enchanced["ColEdit"]) == "Y" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;

                                udo.EnhancedFormColumns.Add(obj);
                            }
                        }
                        //kiểm tra UDO đã có hay chưa, nếu chưa thì add
                        if (!CheckExistUDO(udo.Code))
                        {
                            var ret = AddUdo(udo);
                            AppendLog("ADD UDO\t" + ret);
                        }
                        else
                        {
                            var ret = UpdateUdo(udo);
                            AppendLog("UPDATE UDO\t" + ret);
                        }
                    }
                }
                });
                MessageBox.Show("DONE");
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, ex);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                btn_CreateUDO.Enabled = true;
            }
        }

        public string AddUdo(UserObjectsImpl udo)
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

        public string UpdateUdo(UserObjectsImpl udo)
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

        private void btn_Clear_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = "";
        }

        private void AppendLog(string text)
        {
            if (richTextBox1.InvokeRequired)
                richTextBox1.Invoke((Action)(() => richTextBox1.AppendText("\n" + text)));
            else
                richTextBox1.AppendText("\n" + text);
        }

        private IRestResponse GetResponseService(string serviceName, string jsonString, Method method = Method.POST, string key = "")
        {
            var ServiceAddress = _serviceAddress;
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var uri = string.Format("https://{0}/b1s/v1/{1}", ServiceAddress, serviceName);
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
            if (!string.IsNullOrEmpty(SessionId))
                request.AddCookie("B1SESSION", SessionId);
            if (!string.IsNullOrEmpty(jsonString))
                request.AddParameter("undefined", jsonString, ParameterType.RequestBody);
            return client.Execute(request);
        }

        private async void btn_CopyData_Click(object sender, EventArgs e)
        {
            try
            {
                btn_CopyData.Enabled = false;
                richTextBox1.Clear();
                GetConnectionString();
                var tableNames = txt_TableName.Text;
                await Task.Run(() =>
                {
                    var lstTable = tableNames.Split(',').ToList();
                    foreach (var table in lstTable)
                    {
                        var dt = new DataTable();
                        if (sourceType.Equals("S"))
                            dt = ExecuteDataTable(string.Format("SELECT * FROM \"{0}\"; ", table));
                        else
                            dt = _httpClientSource.ExecuteDataTable(string.Format("SELECT * FROM \"{0}\"; ", table));

                        if (dt.IsNotNull())
                        {
                            var ret = _httpClient.BulkCopy(dt, table);
                            AppendLog(table + "\t" + dt.Rows.Count + "\t" + ret);
                        }
                        else
                        {
                            AppendLog(table + "\tData is Empty!");
                        }
                    }
                });
                MessageBox.Show("DONE");
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, ex);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                btn_CopyData.Enabled = true;
            }
        }

        private async void btn_DelCopy_Click(object sender, EventArgs e)
        {
            if (XtraMessageBox.Show("Do you want to save change?", "Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;
            try
            {
                btn_CopyData.Enabled = false;
                btn_DelCopy.Enabled = false;
                richTextBox1.Clear();
                GetConnectionString();
                var tableNames = txt_TableName.Text;
                await Task.Run(() =>
                {
                    var lstTable = tableNames.Split(',').ToList();
                    foreach (var table in lstTable)
                    {
                        var dt = new DataTable();
                        if (sourceType.Equals("S"))
                            dt = ExecuteDataTable(string.Format("SELECT * FROM \"{0}\"; ", table));
                        else
                            dt = _httpClientSource.ExecuteDataTable(string.Format("SELECT * FROM \"{0}\"; ", table));

                        if (dt.IsNotNull())
                        {
                            var rowDel = _httpClient.ExecuteNonQuery(string.Format("DELETE FROM \"{0}\";", table));
                            AppendLog(table + "\tDeleted rows:\t" + rowDel);

                            var ret = _httpClient.BulkCopy(dt, table);
                            AppendLog(table + "\t" + dt.Rows.Count + "\t" + ret);
                        }
                        else
                        {
                            AppendLog(table + "\tData is Empty!");
                        }
                    }
                });
                MessageBox.Show("DONE");
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, ex);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                btn_CopyData.Enabled = true;
                btn_DelCopy.Enabled = true;
            }
        }

        public void GetConnectionString()
        {
            try
            {
                sourceType = txtServerType.EditValue.ToString();
                _serviceAddress = txt_ServiceAddress.Text;
                HanaConnectionString = string.Format("Server = {0};CS={1}; UserID = {2}; Password = {3}; Pooling=false;Max Pool Size=50;Min Pool Size=5", txt_ServerHana.Text, txt_Hana_Database.Text, txt_HanaUser.Text, txt_HanaPass.Text);
                _httpClient = new DatabaseHanaClient(HanaConnectionString);
                if (sourceType.Equals("S"))
                {
                    if (string.IsNullOrEmpty(txt_Sql_User.Text))
                    {
                        SQLConnectionSource = string.Format(@"Data Source={0};Initial Catalog={1};Integrated Security=SSPI; ", txt_Sql_Server.Text, txt_Sql_Database.Text);
                    }
                    else
                    {
                        SQLConnectionSource = string.Format(@"Data Source={0}; Initial Catalog={3}; Persist Security Info=True; User ID={1}; Password={2}; ",
                            txt_Sql_Server.Text, txt_Sql_User.Text, txt_Sql_Pass.Text, txt_Sql_Database.Text);
                    }
                }
                else if (sourceType.Equals("H"))
                {
                    HanaConnectionSource = string.Format("Server = {0};CS={1}; UserID = {2}; Password = {3}; Pooling=false;Max Pool Size=50;Min Pool Size=5", txt_Sql_Server.Text, txt_Sql_Database.Text, txt_Sql_User.Text, txt_Sql_Pass.Text);
                    _httpClientSource = new DatabaseHanaClient(HanaConnectionSource);
                }
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, ex);
                MessageBox.Show(ex.Message);
            }
        }

        public DataSet ExecuteData(string query, CommandType commandType = CommandType.Text,
            params SqlParameter[] parameters)
        {
            var dtSet = new DataSet();
            using (var conn = new SqlConnection(SQLConnectionSource))
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }
                using (SqlCommand command = conn.CreateCommand())
                {
                    command.CommandTimeout = 60000;
                    command.CommandText = query;
                    command.CommandType = commandType;
                    command.TryAddParameters(parameters);
                    var adapter = new SqlDataAdapter(command);
                    adapter.Fill(dtSet);
                    command.Parameters.Clear();
                }
            }
            return dtSet;
        }

        public DataTable ExecuteDataTable(string query, CommandType commandType = CommandType.Text, params SqlParameter[] parameters)
        {
            var dtSet = new DataTable();
            using (var conn = new SqlConnection(SQLConnectionSource))
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }
                using (SqlCommand command = conn.CreateCommand())
                {
                    command.CommandTimeout = 60000;
                    command.CommandText = query;
                    command.CommandType = commandType;
                    command.TryAddParameters(parameters);
                    var adapter = new SqlDataAdapter(command);
                    adapter.Fill(dtSet);
                    command.Parameters.Clear();
                }
            }
            return dtSet;
        }

        public bool CheckExistTable(string tableName)
        {
            try
            {
                string sql = string.Format(@"SELECT * FROM OUTB WHERE ""TableName"" = '{0}';", tableName);
                var dt = _httpClient.ExecuteDataTable(sql);
                return dt.IsNotNull();
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool CheckExistsUdfColumn(string tableName, string fieldName)
        {
            try
            {
                var dt = _httpClient.ExecuteDataTable(string.Format(@"SELECT * FROM CUFD WHERE ""TableID"" = N'{0}' AND ""AliasID"" = N'{1}';", tableName, fieldName));
                return dt.IsNotNull();
            }
            catch (Exception)
            {
                return false;
            }
        }

        public DataTable GetFiledIDUdf(string tableName, string fieldName)
        {
            try
            {
                return _httpClient.ExecuteDataTable(string.Format(@"SELECT * FROM CUFD WHERE ""TableID"" = N'{0}' AND ""AliasID"" = N'{1}';", tableName, fieldName));
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, ex.Message, Function.ToString(ex.InnerException), ex.Message);
                return null;
            }
        }

        public bool CheckExistsValidValue(string tableName, long fieldId, string value)
        {
            try
            {
                var dt = _httpClient.ExecuteDataTable(string.Format(@"SELECT * FROM UFD1 WHERE ""TableID"" = N'{0}' AND ""FieldID"" = {1} AND ""FldValue"" = N'{2}';", tableName, fieldId, value));
                return dt.IsNotNull();
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool CheckExistUDO(string code)
        {
            try
            {
                string sql = string.Format(@"SELECT * FROM OUDO WHERE ""Code"" = '{0}';", code);
                var dt = _httpClient.ExecuteDataTable(sql);
                return dt.IsNotNull();
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void btn_TestSource_Click(object sender, EventArgs e)
        {
            try
            {
                btn_TestSource.Enabled = false;
                GetConnectionString();
                if (sourceType.Equals("S"))
                    ExecuteData("SELECT 1 Code");
                else
                    _httpClientSource.ExecuteData(@"SELECT 1 ""Code"" FROM DUMMY;");
                MessageBox.Show("SUCCESS");
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, ex.Message, Function.ToString(ex.InnerException), sourceType.Equals("S") ? SQLConnectionSource : HanaConnectionSource);
                MessageBox.Show(ex.Message);
            }
            btn_TestSource.Enabled = true;
        }

        private void btn_TestTarget_Click(object sender, EventArgs e)
        {
            try
            {
                btn_TestTarget.Enabled = false;
                GetConnectionString();
                _httpClient.ExecuteData(@"SELECT 1 ""Code"" FROM DUMMY;");
                MessageBox.Show("SUCCESS");
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, ex.Message, Function.ToString(ex.InnerException), HanaConnectionString);
                MessageBox.Show(ex.Message);
            }
            btn_TestTarget.Enabled = true;
        }

        private void btn_CopyManual_Click(object sender, EventArgs e)
        {
            try
            {
                GetConnectionString();
                var frm = new frm_CopyManual(sourceType, sourceType.Equals("S") ? SQLConnectionSource : HanaConnectionSource, HanaConnectionString);
                frm.ShowDialog();
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, ex.Message, Function.ToString(ex.InnerException), sourceType.Equals("S") ? SQLConnectionSource : HanaConnectionSource);
                MessageBox.Show(ex.Message);
            }
        }

        private void btn_LogOut_Click(object sender, EventArgs e)
        {
            try
            {
                btn_LogOut.Enabled = false;
                GetConnectionString();
                if (!string.IsNullOrEmpty(SessionId))
                {
                    var response = GetResponseService("Logout", "");
                    if (string.IsNullOrEmpty(response.Content))
                    {
                        richTextBox1.Text = "LogOut" + "\t" + SessionId + "\t" + "SUCCESS!";
                        SessionId = "";
                        btn_CreateUDT.Enabled = false;
                        btn_CreateUDF.Enabled = false;
                        btn_CreateUDO.Enabled = false;
                        btn_LinkUDF.Enabled = false;
                        btn_UpdateUDF.Enabled = false;
                        btn_DelUDF.Enabled = false;
                    }
                    else
                    {
                        richTextBox1.Text = "LogOut" + "\t" + SessionId + "\t" + response.Content;
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, ex);
            }
            btn_LogOut.Enabled = true;
        }

        private void txtServerType_EditValueChanged(object sender, EventArgs e)
        {
            try
            {
                sourceType = txtServerType.EditValue.ToString();
                if (sourceType.Equals("S"))
                {
                    txt_Sql_Server.Text = Function.ToString(ConfigurationManager.AppSettings["Source_Server"]);
                    txt_Sql_Database.Text = Function.ToString(ConfigurationManager.AppSettings["Source_DB"]);
                    txt_Sql_User.Text = Function.ToString(ConfigurationManager.AppSettings["Source_User"]);
                    txt_Sql_Pass.Text = Function.ToString(ConfigurationManager.AppSettings["Source_Pass"]);
                }
                else if (sourceType.Equals("H"))
                {
                    txt_Sql_Server.Text = Function.ToString(ConfigurationManager.AppSettings["ServerHana"]);
                    txt_Sql_Database.Text = Function.ToString(ConfigurationManager.AppSettings["CompanyDB"]);
                    txt_Sql_User.Text = Function.ToString(ConfigurationManager.AppSettings["HanaUser"]);
                    txt_Sql_Pass.Text = Function.ToString(ConfigurationManager.AppSettings["HanaPass"]);
                }
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, ex);
                MessageBox.Show(ex.Message);
            }
        }

        private void btn_CopyUDOManual_Click(object sender, EventArgs e)
        {
            try
            {
                GetConnectionString();
                var frm = new frm_CopyUDOManual(sourceType, sourceType.Equals("S") ? SQLConnectionSource : HanaConnectionSource, HanaConnectionString, SessionId);
                frm.ShowDialog();
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, ex.Message, Function.ToString(ex.InnerException), sourceType.Equals("S") ? SQLConnectionSource : HanaConnectionSource);
                MessageBox.Show(ex.Message);
            }
        }
    }

}
