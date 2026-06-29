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

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                if (MessageBox.Show("Bạn có chắc muốn thoát ứng dụng?", "Xác nhận thoát",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    e.Cancel = true;
                    base.OnFormClosing(e);
                    return;
                }

                // Hủy tác vụ đang chạy (nếu có) trước khi thoát
                if (_cts != null && !_cts.IsCancellationRequested)
                    _cts.Cancel();

                // Đã login mà chưa logout -> tự logout trước khi tắt
                if (!string.IsNullOrEmpty(SessionId))
                {
                    try
                    {
                        GetConnectionString();
                        GetResponseService("Logout", "");
                        SessionId = "";
                    }
                    catch (Exception ex)
                    {
                        Logging.Write(Logging.ERROR, ex);
                    }
                }
            }
            base.OnFormClosing(e);
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
            if (MessageBox.Show("Tạo User Defined Table từ DB nguồn sang DB đích?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;
            try
            {
                BeginTask();
                GetConnectionString();
                var tableNameText = txt_TableName.Text;
                await Task.Run(() =>
                {
                    Log("START CreateUDT" + (string.IsNullOrEmpty(tableNameText) ? " - All tables" : " - " + tableNameText));
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
                        Log(string.Format("LOAD {0} UDT(s) from source", dt.Rows.Count));
                        int created = 0, skipped = 0, udfAdded = 0;
                        foreach (DataRow item in dt.Rows)
                        {
                            if (Cancelled) { Log("CANCELLED bởi người dùng"); break; }
                            var tableName = Function.ToString(item["TableName"]);
                            if (CheckExistTable(tableName))
                            {
                                Log(string.Format("SKIP UDT [{0}] already exists - checking UDFs...", tableName));
                                var added = SyncUdfForTable(tableName);
                                udfAdded += added;
                                Log(string.Format("SYNC UDT [{0}] -> {1} UDF(s) added", tableName, added));
                                skipped++;
                                continue;
                            }
                            Log(string.Format("CREATE UDT [{0}]...", tableName));
                            var ret = AddUdt(item);
                            Log("RESULT\t" + ret);
                            created++;
                        }
                        Log(string.Format("DONE CreateUDT: {0} created, {1} skipped, {2} UDF(s) synced", created, skipped, udfAdded));
                    }
                    else
                    {
                        Log("WARN No UDT data found in source");
                    }
                });
                MessageBox.Show(Cancelled ? "Đã hủy tác vụ" : "DONE");
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, ex);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                EndTask();
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

        private void EnsureUdtExists(string tableName)
        {
            if (string.IsNullOrEmpty(tableName)) return;
            if (CheckExistTable(tableName))
            {
                Log(string.Format("  UDT [{0}] already exists - syncing UDFs...", tableName));
                var existsAdded = SyncUdfForTable(tableName);
                Log(string.Format("  SYNC UDT [{0}] -> {1} UDF(s) added", tableName, existsAdded));
                return;
            }

            DataRow udtRow = null;
            if (sourceType.Equals("S"))
            {
                var ds = ExecuteData(string.Format("SELECT * FROM OUTB WHERE TableName = '{0}';", tableName));
                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    udtRow = ds.Tables[0].Rows[0];
            }
            else
            {
                var dt = _httpClientSource.ExecuteDataTable(string.Format("SELECT * FROM OUTB WHERE \"TableName\" = '{0}';", tableName));
                if (dt != null && dt.Rows.Count > 0)
                    udtRow = dt.Rows[0];
            }

            if (udtRow == null)
            {
                Log(string.Format("  WARN UDT [{0}] not found in source - skipping", tableName));
                return;
            }

            Log(string.Format("  CREATE UDT [{0}]...", tableName));
            var ret = AddUdt(udtRow);
            Log(string.Format("  RESULT {0}", ret));

            var added = SyncUdfForTable(tableName);
            Log(string.Format("  SYNC UDT [{0}] -> {1} UDF(s) added", tableName, added));
        }

        private int SyncUdfForTable(string tableName)
        {
            int added = 0;
            try
            {
                // CUFD/UFD1 lưu TableID có "@", OUTB/OUDO lưu TableName không có "@"
                var dbTableName = tableName.StartsWith("@") ? tableName : "@" + tableName;

                DataTable dtCufd, dtUfd1;
                if (sourceType.Equals("S"))
                {
                    var ds = ExecuteData(string.Format(
                        "SELECT * FROM CUFD WHERE TableID = '{0}'; SELECT * FROM UFD1 WHERE TableID = '{0}';",
                        dbTableName));
                    if (ds.Tables.Count < 2) return 0;
                    dtCufd = ds.Tables[0];
                    dtUfd1 = ds.Tables[1];
                }
                else
                {
                    dtCufd = _httpClientSource.ExecuteDataTable(string.Format(
                        "SELECT * FROM CUFD WHERE \"TableID\" = '{0}';", dbTableName));
                    dtUfd1 = _httpClientSource.ExecuteDataTable(string.Format(
                        "SELECT * FROM UFD1 WHERE \"TableID\" = '{0}';", dbTableName));
                }

                if (dtCufd == null || dtCufd.Rows.Count == 0)
                {
                    Log(string.Format("  INFO No UDFs in source for [{0}]", tableName));
                    return 0;
                }

                Log(string.Format("  LOAD {0} UDF(s) in source for [{1}]", dtCufd.Rows.Count, tableName));
                int skipped = 0;
                foreach (DataRow row in dtCufd.Rows)
                {
                    if (Cancelled) break;
                    var field = new UserFieldsImpl
                    {
                        TableName = Function.ToString(row["TableID"]),
                        Name      = Function.ToString(row["AliasID"])
                    };

                    if (CheckExistsUdfColumn(field.TableName, field.Name))
                    {
                        skipped++;
                        continue;
                    }

                    field.Size         = Function.ParseInt(row["SizeID"]);
                    field.Description  = Function.ToString(row["Descr"]);
                    field.FieldID      = Function.ParseInt(row["FieldID"]);
                    field.Mandatory    = Function.ToString(row["NotNull"]) == "Y" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
                    field.EditSize     = Function.ParseInt(row["EditSize"]);
                    field.Type         = MapUdfType(Function.ToString(row["TypeID"]));
                    field.SubType      = MapUdfSubType(Function.ToString(row["EditType"]));
                    field.LinkedTable  = Function.ToString(row["RTable"]);
                    field.DefaultValue = Function.ToString(row["Dflt"]);
                    field.LinkedObject = Function.ToString(row["RelSO"]);

                    if (dtUfd1 != null)
                    {
                        var valid = dtUfd1.AsEnumerable().Where(t =>
                            Function.ToString(t["TableID"]) == field.TableName &&
                            Function.ParseInt(t["FieldID"]) == field.FieldID);
                        if (valid.Any())
                        {
                            field.ValidValues = valid.Select(v => new ValidValuesMDImpl
                            {
                                Description = Function.ToString(v["Descr"]),
                                Value       = Function.ToString(v["FldValue"])
                            }).ToList();
                        }
                    }

                    Log(string.Format("  ADD UDF [{0}.{1}]...", field.TableName, field.Name));
                    var ret = AddUdf(field);
                    Log("  RESULT\t" + ret);
                    added++;
                }
                if (skipped > 0)
                    Log(string.Format("  SKIP {0} UDF(s) already exists in [{1}]", skipped, tableName));
            }
            catch (Exception ex)
            {
                Log(string.Format("  ERROR SyncUDF [{0}]: {1}", tableName, ex.Message), isError: true);
                Logging.Write(Logging.ERROR, ex);
            }
            return added;
        }

        private async void btn_CreateUDF_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Tạo User Defined Field từ DB nguồn sang DB đích?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;
            try
            {
                BeginTask();
                GetConnectionString();
                var tableNameText = txt_TableName.Text;
                var udfNameText = txt_UDFName.Text;
                await Task.Run(() =>
                {
                Log("START CreateUDF" + (string.IsNullOrEmpty(tableNameText) ? " - All tables" : " - " + tableNameText));
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
                    if (!string.IsNullOrEmpty(udfNameText))
                    {
                        var udfFilter = ParseUdfNames(udfNameText);
                        var matchRows = dt.AsEnumerable().Where(r => udfFilter.Contains(Function.ToString(r["AliasID"]))).ToList();
                        Log(string.Format("FILTER UDF names: {0} match(es) of {1}", matchRows.Count, dt.Rows.Count));
                        dt = matchRows.Count > 0 ? matchRows.CopyToDataTable() : dt.Clone();
                    }
                    Log(string.Format("LOAD {0} UDF(s) from source", dt.Rows.Count));
                    int added = 0, skipped = 0;
                    foreach (DataRow item in dt.Rows)
                    {
                        if (Cancelled) { Log("CANCELLED bởi người dùng"); break; }
                        var field = new UserFieldsImpl();
                        field.TableName = Function.ToString(item["TableID"]);
                        field.Name = Function.ToString(item["AliasID"]);
                        if (CheckExistsUdfColumn(field.TableName, field.Name))
                        {
                            Log(string.Format("SKIP UDF [{0}.{1}] already exists", field.TableName, field.Name));
                            skipped++;
                            continue;
                        }
                        field.Size = Function.ParseInt(item["SizeID"]);
                        field.Description = Function.ToString(item["Descr"]);
                        field.FieldID = Function.ParseInt(item["FieldID"]);
                        field.Mandatory = Function.ToString(item["NotNull"]) == "Y" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
                        field.EditSize = Function.ParseInt(item["EditSize"]);
                        field.Type = MapUdfType(Function.ToString(item["TypeID"]));
                        field.SubType = MapUdfSubType(Function.ToString(item["EditType"]));

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

                        Log(string.Format("ADD UDF [{0}.{1}]...", field.TableName, field.Name));
                        var ret = AddUdf(field);
                        Log("RESULT\t" + ret);
                        added++;
                    }
                    Log(string.Format("DONE CreateUDF: {0} added, {1} skipped", added, skipped));
                }
                else
                {
                    Log("WARN No UDF data found in source");
                }
                });
                MessageBox.Show(Cancelled ? "Đã hủy tác vụ" : "DONE");
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, ex);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                EndTask();
            }
        }

        // SAP Service Layer hay trả -2004 "Table not found" ngay sau khi tạo UDT
        // (bảng vật lý chưa kịp đăng ký trong ODBC). Retry vài lần thì hết.
        private const int UdfRetryMax = 10;
        private const int UdfRetryDelayMs = 5000;

        private static bool IsTableNotFound(string content)
        {
            return !string.IsNullOrEmpty(content)
                && (content.Contains("-2004") || content.Contains("Table not found"));
        }

        public string AddUdf(UserFieldsImpl field)
        {
            try
            {
                var json = BuildUdfJson(field);
                IRestResponse response = null;
                for (int attempt = 1; attempt <= UdfRetryMax; attempt++)
                {
                    response = GetResponseService("UserFieldsMD", json);
                    if (!IsTableNotFound(response.Content))
                        break;
                    if (Cancelled) break;
                    if (attempt < UdfRetryMax)
                    {
                        Log(string.Format("  RETRY UDF [{0}.{1}] table chưa sẵn sàng, thử lại {2}/{3} sau {4}s...",
                            field.TableName, field.Name, attempt, UdfRetryMax - 1, UdfRetryDelayMs / 1000));
                        System.Threading.Thread.Sleep(UdfRetryDelayMs);
                    }
                }
                return field.TableName + "\t" + field.Name + "\t" + response.Content + (string.IsNullOrEmpty(response.Content) ? "" : "\n" + json);
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, new StackTrace(new StackFrame(0)).ToString().Substring(5, new StackTrace(new StackFrame(0)).ToString().Length - 5), ex.Message);
                return field.TableName + "." + field.Name + "\t" + ex.Message;
            }
        }

        private async void btn_LinkUDF_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Liên kết UDF với UDO từ DB nguồn sang DB đích?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;
            try
            {
                BeginTask();
                GetConnectionString();
                var tableNameText = txt_TableName.Text;
                var udfNameText = txt_UDFName.Text;
                await Task.Run(() =>
                {
                Log("START LinkUDF" + (string.IsNullOrEmpty(tableNameText) ? " - All tables" : " - " + tableNameText));
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
                    if (!string.IsNullOrEmpty(udfNameText))
                    {
                        var udfFilter = ParseUdfNames(udfNameText);
                        var matchRows = dt.AsEnumerable().Where(r => udfFilter.Contains(Function.ToString(r["AliasID"]))).ToList();
                        Log(string.Format("FILTER UDF names: {0} match(es) of {1}", matchRows.Count, dt.Rows.Count));
                        dt = matchRows.Count > 0 ? matchRows.CopyToDataTable() : dt.Clone();
                    }
                    Log(string.Format("LOAD {0} UDF(s) with UDO link from source", dt.Rows.Count));
                    int linked = 0, notFound = 0;
                    foreach (DataRow item in dt.Rows)
                    {
                        if (Cancelled) { Log("CANCELLED bởi người dùng"); break; }
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
                            Log(string.Format("NOT FOUND UDF [{0}.{1}] in target", field.TableName, field.Name));
                            notFound++;
                            continue;
                        }

                        Log(string.Format("LINK UDF [{0}.{1}] -> UDO [{2}]...", field.TableName, field.Name, field.LinkedUDO));
                        var ret = LinkUdf(field);
                        Log("RESULT\t" + ret);
                        linked++;
                    }
                    Log(string.Format("DONE LinkUDF: {0} linked, {1} not found", linked, notFound));
                }
                else
                {
                    Log("WARN No UDF with UDO link found in source");
                }
                });
                MessageBox.Show(Cancelled ? "Đã hủy tác vụ" : "DONE");
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, ex);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                EndTask();
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
            if (MessageBox.Show("Cập nhật User Defined Field trên DB đích?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;
            try
            {
                BeginTask();
                GetConnectionString();
                var tableNameText = txt_TableName.Text;
                var udfNameText = txt_UDFName.Text;
                await Task.Run(() =>
                {
                Log("START UpdateUDF" + (string.IsNullOrEmpty(tableNameText) ? "" : " - " + tableNameText));
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
                    if (!string.IsNullOrEmpty(udfNameText))
                    {
                        var udfFilter = ParseUdfNames(udfNameText);
                        var matchRows = dt.AsEnumerable().Where(r => udfFilter.Contains(Function.ToString(r["AliasID"]))).ToList();
                        Log(string.Format("FILTER UDF names: {0} match(es) of {1}", matchRows.Count, dt.Rows.Count));
                        dt = matchRows.Count > 0 ? matchRows.CopyToDataTable() : dt.Clone();
                    }
                    Log(string.Format("LOAD {0} UDF(s) from source", dt.Rows.Count));
                    int updated = 0;
                    foreach (DataRow item in dt.Rows)
                    {
                        if (Cancelled) { Log("CANCELLED bởi người dùng"); break; }
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
                        field.Type = MapUdfType(Function.ToString(item["TypeID"]));
                        field.SubType = MapUdfSubType(Function.ToString(item["EditType"]));

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

                        Log(string.Format("UPDATE UDF [{0}.{1}]...", field.TableName, field.Name));
                        var ret = UpdateUdf(field);
                        Log("RESULT\t" + ret);
                        updated++;
                    }
                    Log(string.Format("DONE UpdateUDF: {0} updated", updated));
                }
                else
                {
                    Log("WARN Nhập danh sách bảng cần cập nhật UDF! lưu ý: ValidValue chỉ thêm được value chứ không cập nhật giá trị cũ");
                }
                });
                MessageBox.Show(Cancelled ? "Đã hủy tác vụ" : "DONE");
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, ex);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                EndTask();
            }
        }

        public string UpdateUdf(UserFieldsImpl field)
        {
            try
            {
                var json = BuildUdfJson(field);
                var key = string.Format("(TableName='{0}',FieldID={1})", field.TableName, field.FieldID);
                var response = GetResponseService("UserFieldsMD", json, Method.PATCH, key);
                return field.TableName + "\t" + field.Name + "\t" + response.Content + (string.IsNullOrEmpty(response.Content) ? "" : "\n" + json);
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, new StackTrace(new StackFrame(0)).ToString().Substring(5, new StackTrace(new StackFrame(0)).ToString().Length - 5), ex.Message);
                return field.TableName + "." + field.Name + "\t" + ex.Message;
            }
        }

        private async void btn_DelUDF_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("XÓA User Defined Field trên DB đích? Thao tác này không thể hoàn tác!",
                "Xác nhận XÓA", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No) return;
            try
            {
                BeginTask();
                GetConnectionString();
                var tableNameText = txt_TableName.Text;
                var udfNameText = txt_UDFName.Text;
                await Task.Run(() =>
                {
                    Log("START DeleteUDF" + (string.IsNullOrEmpty(tableNameText) ? "" : " - " + tableNameText));
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
                        if (!string.IsNullOrEmpty(udfNameText))
                        {
                            var udfFilter = ParseUdfNames(udfNameText);
                            var matchRows = dt.AsEnumerable().Where(r => udfFilter.Contains(Function.ToString(r["AliasID"]))).ToList();
                            Log(string.Format("FILTER UDF names: {0} match(es) of {1}", matchRows.Count, dt.Rows.Count));
                            dt = matchRows.Count > 0 ? matchRows.CopyToDataTable() : dt.Clone();
                        }
                        Log(string.Format("LOAD {0} UDF(s) from source", dt.Rows.Count));
                        int deleted = 0;
                        foreach (DataRow item in dt.Rows)
                        {
                            if (Cancelled) { Log("CANCELLED bởi người dùng"); break; }
                            var field = new UserFieldsImpl();
                            field.Name = Function.ToString(item["AliasID"]);
                            field.TableName = Function.ToString(item["TableID"]);
                            field.FieldID = Function.ParseInt(item["FieldID"]);

                            Log(string.Format("DELETE UDF [{0}.{1}]...", field.TableName, field.Name));
                            var ret = DeleteUdf(field);
                            Log("RESULT\t" + ret);
                            deleted++;
                        }
                        Log(string.Format("DONE DeleteUDF: {0} deleted", deleted));
                    }
                    else
                    {
                        Log("WARN No UDF data found");
                    }
                });
                MessageBox.Show(Cancelled ? "Đã hủy tác vụ" : "DONE");
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, ex);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                EndTask();
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
            if (MessageBox.Show("Tạo User Defined Object từ DB nguồn sang DB đích?\n(Sẽ tự động tạo UDT và UDF nếu chưa có)", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;
            try
            {
                BeginTask();
                GetConnectionString();
                var tableNameText = txt_TableName.Text;
                await Task.Run(() =>
                {
                Log("START CreateUDO" + (string.IsNullOrEmpty(tableNameText) ? " - All" : " - " + tableNameText));
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
                    Log(string.Format("LOAD {0} UDO(s) from source", dt.Rows.Count));
                    int created = 0, updated = 0;
                    foreach (DataRow item in dt.Rows)
                    {
                        if (Cancelled) { Log("CANCELLED bởi người dùng"); break; }
                        var code = Function.ToString(item["Code"]);
                        var udoExists = CheckExistUDO(code);

                        // Đảm bảo UDT tồn tại trước khi tạo UDO
                        var udtTableName = Function.ToString(item["TableName"]);
                        Log(string.Format("CHECK UDTs for UDO [{0}]...", code));
                        EnsureUdtExists(udtTableName);
                        foreach (DataRow childRow in ds.Tables[1].AsEnumerable()
                            .Where(t => Function.ToString(t["Code"]) == code))
                            EnsureUdtExists(Function.ToString(childRow["TableName"]));

                        var udo = new UserObjectsImpl();
                        udo.Code = code;
                        udo.Name = Function.ToString(item["Name"]);
                        udo.TableName = udtTableName;
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
                        string ret;
                        if (udoExists)
                        {
                            Log(string.Format("UPDATE UDO [{0}] (already exists)...", code));
                            ret = UpdateUdo(udo);
                            updated++;
                        }
                        else
                        {
                            Log(string.Format("CREATE UDO [{0}]...", code));
                            ret = AddUdo(udo);
                            created++;
                        }
                        Log("RESULT\t" + ret);
                    }
                    Log(string.Format("DONE CreateUDO: {0} created, {1} updated", created, updated));
                }
                else
                {
                    Log("WARN No UDO data found in source");
                }
                });
                MessageBox.Show(Cancelled ? "Đã hủy tác vụ" : "DONE");
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, ex);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                EndTask();
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

        private void Log(string message, bool isError = false)
        {
            var line = string.Format("[{0:HH:mm:ss}] {1}", DateTime.Now, message);
            AppendLog(line);
            Logging.Write(isError ? Logging.ERROR : Logging.TRACE, line);
        }

        // Quản lý trạng thái tác vụ đang chạy + hủy
        private System.Threading.CancellationTokenSource _cts;
        private System.Collections.Generic.Dictionary<Button, bool> _savedBtnState;

        private bool Cancelled
        {
            get { return _cts != null && _cts.IsCancellationRequested; }
        }

        private Button[] OperationButtons
        {
            get
            {
                return new[]
                {
                    btn_CreateUDT, btn_CreateUDF, btn_CreateUDO, btn_LinkUDF,
                    btn_UpdateUDF, btn_DelUDF, btn_CopyData, btn_DelCopy,
                    btn_Login, btn_LogOut, btn_CopyUDOManual,
                    btn_TestSource, btn_TestTarget, btn_CopyManual
                };
            }
        }

        private void SetBusy(bool busy)
        {
            if (InvokeRequired) { Invoke((Action)(() => SetBusy(busy))); return; }
            if (busy)
            {
                _savedBtnState = OperationButtons.ToDictionary(b => b, b => b.Enabled);
                foreach (var b in OperationButtons) b.Enabled = false;
                btn_Cancel.Enabled = true;
            }
            else
            {
                btn_Cancel.Enabled = false;
                if (_savedBtnState != null)
                    foreach (var kv in _savedBtnState) kv.Key.Enabled = kv.Value;
            }
        }

        private void BeginTask()
        {
            _cts = new System.Threading.CancellationTokenSource();
            richTextBox1.Clear();
            SetBusy(true);
        }

        private void EndTask()
        {
            SetBusy(false);
            if (_cts != null) { _cts.Dispose(); _cts = null; }
        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            if (_cts == null || _cts.IsCancellationRequested) return;
            if (MessageBox.Show("Bạn có chắc muốn hủy tác vụ đang chạy?", "Xác nhận hủy",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No) return;
            _cts.Cancel();
            btn_Cancel.Enabled = false;
            Log("CANCEL - đang dừng tác vụ, vui lòng đợi bước hiện tại hoàn tất...");
        }

        private static BoFieldTypes MapUdfType(string typeCode)
        {
            switch (typeCode)
            {
                case "B": return BoFieldTypes.db_Float;
                case "M": return BoFieldTypes.db_Memo;
                case "D": return BoFieldTypes.db_Date;
                case "N": return BoFieldTypes.db_Numeric;
                default:  return BoFieldTypes.db_Alpha;
            }
        }

        private static BoFldSubTypes MapUdfSubType(string subTypeCode)
        {
            switch (subTypeCode)
            {
                case "S": return BoFldSubTypes.st_Sum;
                case "P": return BoFldSubTypes.st_Price;
                case "%": return BoFldSubTypes.st_Percentage;
                case "Q": return BoFldSubTypes.st_Quantity;
                case "T": return BoFldSubTypes.st_Time;
                case "A": return BoFldSubTypes.st_Address;
                case "M": return BoFldSubTypes.st_Measurement;
                case "L": return BoFldSubTypes.st_Link;
                case "R": return BoFldSubTypes.st_Rate;
                case "B": return BoFldSubTypes.st_Link;
                default:  return BoFldSubTypes.st_None;
            }
        }

        private static string BuildUdfJson(UserFieldsImpl field)
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
                sb.Append(Function.GetJsonString("LinkedTable", field.LinkedTable));
            if (!string.IsNullOrEmpty(field.DefaultValue))
                sb.Append(Function.GetJsonString("DefaultValue", field.DefaultValue));
            if (!string.IsNullOrEmpty(field.LinkedUDO))
                sb.Append(Function.GetJsonString("LinkedUDO", field.LinkedUDO));
            if (!string.IsNullOrEmpty(field.LinkedObject))
                sb.Append(Function.GetJsonString("LinkedSystemObject", field.LinkedObject));
            if (field.ValidValues != null && field.ValidValues.Count > 0)
            {
                sb.Append(@"""ValidValuesMD"":[");
                foreach (var item in field.ValidValues)
                {
                    sb.Append(@"{");
                    if (field.SubType == BoFldSubTypes.st_Time)
                        sb.Append(Function.GetJsonString("Value", Function.ParseInt(item.Value.Replace(":", "")).ToString("0000")));
                    else
                        sb.Append(Function.GetJsonString("Value", item.Value));
                    sb.Append(Function.GetJsonString("Description", item.Description));
                    sb.Append(@"},");
                }
                sb.Remove(sb.Length - 1, 1);
                sb.Append("]");
            }
            sb.Append(@"}");
            return sb.ToString();
        }

        private static HashSet<string> ParseUdfNames(string text)
        {
            return new HashSet<string>(
                text.Split(',')
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .Select(s => s.StartsWith("U_", StringComparison.OrdinalIgnoreCase) ? s.Substring(2) : s),
                StringComparer.OrdinalIgnoreCase);
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
            if (MessageBox.Show("Copy dữ liệu từ DB nguồn sang DB đích?\nDữ liệu đích sẽ bị ghi đè!", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;
            try
            {
                BeginTask();
                GetConnectionString();
                var tableNames = txt_TableName.Text;
                await Task.Run(() =>
                {
                    Log("START CopyData - " + tableNames);
                    var lstTable = tableNames.Split(',').ToList();
                    foreach (var table in lstTable)
                    {
                        if (Cancelled) { Log("CANCELLED bởi người dùng"); break; }
                        var dt = new DataTable();
                        Log(string.Format("LOAD [{0}] from source...", table));
                        if (sourceType.Equals("S"))
                            dt = ExecuteDataTable(string.Format("SELECT * FROM \"{0}\"; ", table));
                        else
                            dt = _httpClientSource.ExecuteDataTable(string.Format("SELECT * FROM \"{0}\"; ", table));

                        if (dt.IsNotNull())
                        {
                            Log(string.Format("COPY [{0}] {1} row(s)...", table, dt.Rows.Count));
                            var ret = _httpClient.BulkCopy(dt, table);
                            Log(string.Format("RESULT [{0}]\t{1} rows\t{2}", table, dt.Rows.Count, ret));
                        }
                        else
                        {
                            Log(string.Format("SKIP [{0}] Data is Empty!", table));
                        }
                    }
                    Log("DONE CopyData");
                });
                MessageBox.Show(Cancelled ? "Đã hủy tác vụ" : "DONE");
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, ex);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                EndTask();
            }
        }

        private async void btn_DelCopy_Click(object sender, EventArgs e)
        {
            if (XtraMessageBox.Show("Do you want to save change?", "Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;
            try
            {
                BeginTask();
                GetConnectionString();
                var tableNames = txt_TableName.Text;
                await Task.Run(() =>
                {
                    Log("START DelCopy - " + tableNames);
                    var lstTable = tableNames.Split(',').ToList();
                    foreach (var table in lstTable)
                    {
                        if (Cancelled) { Log("CANCELLED bởi người dùng"); break; }
                        var dt = new DataTable();
                        Log(string.Format("LOAD [{0}] from source...", table));
                        if (sourceType.Equals("S"))
                            dt = ExecuteDataTable(string.Format("SELECT * FROM \"{0}\"; ", table));
                        else
                            dt = _httpClientSource.ExecuteDataTable(string.Format("SELECT * FROM \"{0}\"; ", table));

                        if (dt.IsNotNull())
                        {
                            Log(string.Format("DELETE [{0}] target rows...", table));
                            var rowDel = _httpClient.ExecuteNonQuery(string.Format("DELETE FROM \"{0}\";", table));
                            Log(string.Format("DELETED [{0}]\t{1} rows", table, rowDel));

                            Log(string.Format("COPY [{0}] {1} row(s)...", table, dt.Rows.Count));
                            var ret = _httpClient.BulkCopy(dt, table);
                            Log(string.Format("RESULT [{0}]\t{1} rows\t{2}", table, dt.Rows.Count, ret));
                        }
                        else
                        {
                            Log(string.Format("SKIP [{0}] Data is Empty!", table));
                        }
                    }
                    Log("DONE DelCopy");
                });
                MessageBox.Show(Cancelled ? "Đã hủy tác vụ" : "DONE");
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, ex);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                EndTask();
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
