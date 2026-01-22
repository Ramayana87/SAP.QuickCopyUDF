using Apzon.Commons;
using Apzon.Commons.Constants;
using Apzon.Commons.Helper;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraTab;
using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SAP.QuickCopyUDF
{
    public partial class frm_CopyUDOManual : XtraForm
    {
        private XtraTabControl XtraTabLine = new XtraTabControl();
        public enum CopyType
        {
            UDT = 0,
            UDO = 1
        }
        CopyType _type = CopyType.UDT;

        public string SQLConnectionSource { get; set; }

        public string HanaConnectionSource { get; set; }

        public string HanaConnectionString { get; set; }

        public DatabaseHanaClient _httpClient;

        public DatabaseHanaClient _httpClientSource;

        public string sessionId { get; set; }

        public string sourceType { get; set; }

        /*
         
        SELECT * FROM OUTB WHERE "TableName" IN ('OORS','ORS1','ORS2') ORDER BY "TblNum";
        SELECT * FROM CUFD WHERE "TableID" IN ('@OORS','@ORS1','@ORS2') ORDER BY "TableID", "FieldID";
        SELECT * FROM UFD1 WHERE "TableID" IN ('@OORS','@ORS1','@ORS2') ORDER BY "TableID", "FieldID", "IndexID";


        SELECT * FROM OUDO WHERE "Code" = 'OORS';
        SELECT * FROM UDO1 WHERE "Code" = 'OORS' ORDER BY "SonNum";
        SELECT * FROM UDO2 WHERE "Code" = 'OORS' ORDER BY "ColumnNum";
        SELECT * FROM UDO3 WHERE "Code" = 'OORS' ORDER BY "ColumnNum", "SonNum";
        SELECT * FROM UDO4 WHERE "Code" = 'OORS' ORDER BY "SonNum", "ColumnNum";

        */

        public frm_CopyUDOManual()
        {
            InitializeComponent();
        }

        public frm_CopyUDOManual(string type, string sourceConnectionString, string hanaConnectionString, string sessionId)
        {
            sourceType = type;
            this.sessionId = sessionId;
            HanaConnectionString = hanaConnectionString;
            if (type.Equals("S"))

                SQLConnectionSource = sourceConnectionString;
            else
                HanaConnectionSource = sourceConnectionString;
            InitializeComponent();
        }

        private void frm_CopyUDOManual_Load(object sender, EventArgs e)
        {
            try
            {
                _httpClient = new DatabaseHanaClient(HanaConnectionString);
                if (sourceType.Equals("H"))
                {
                    _httpClientSource = new DatabaseHanaClient(HanaConnectionSource);
                }
                panelControl1.Controls.Add(XtraTabLine);
                XtraTabLine.Dock = DockStyle.Fill;
                LoadData();
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, ex.Message, Function.ToString(ex.InnerException), sourceType.Equals("S") ? SQLConnectionSource : HanaConnectionSource);
                MessageBox.Show(ex.Message);
            }
        }

        private static void view_CustomDrawRowIndicator(object sender, RowIndicatorCustomDrawEventArgs e)
        {
            if (e.Info.IsRowIndicator && e.RowHandle >= 0)
            {
                e.Info.DisplayText = (e.RowHandle + 1).ToString();
            }
        }

        private void btn_CopyData_Click(object sender, EventArgs e)
        {
            try
            {
                btn_CopyData.Enabled = false;
                btn_pasteFromClipboard.Enabled = false;
                txt_result.Text = "";
                if (_type == CopyType.UDT)
                {
                    // tạo UDT
                    GridControl grd_OUTB = (GridControl)XtraTabLine.TabPages[0].Controls[0];
                    var dt = (DataTable)grd_OUTB.DataSource;
                    foreach (DataRow item in dt.Rows)
                    {
                        if (CheckExistTable(Function.ToString(item["TableName"]))) continue;
                        var ret = FunctionHelper.AddUdt(item);
                        var error1 = txt_result.Text;
                        error1 += "\n" + ret;
                        txt_result.Text = error1;
                    }

                    // tạo UDF
                    GridControl grd_CUFD = (GridControl)XtraTabLine.TabPages[1].Controls[0];
                    var dt_CUFD = (DataTable)grd_CUFD.DataSource;
                    var dt_UFD1 = (DataTable)((GridControl)XtraTabLine.TabPages[2].Controls[0]).DataSource;
                    foreach (DataRow item in dt_CUFD.Rows)
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

                        var valid = dt_UFD1.AsEnumerable().Where(t => Function.ToString(t["TableID"]) == field.TableName && Function.ParseInt(t["FieldID"]) == field.FieldID);
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

                        var ret = FunctionHelper.AddUdf(field);
                        Logging.Write(Logging.WATCH, ret);
                        var error1 = txt_result.Text;
                        error1 += "\n" + ret;
                        txt_result.Text = error1;
                    }
                    MessageBox.Show("DONE");
                }
                else
                {
                    // tạo UDO
                    GridControl grd_UDO = (GridControl)XtraTabLine.TabPages[0].Controls[0];
                    var dt_UDO = (DataTable)grd_UDO.DataSource;
                    var dt_UDO1 = (DataTable)((GridControl)XtraTabLine.TabPages[1].Controls[0]).DataSource;
                    var dt_UDO2 = (DataTable)((GridControl)XtraTabLine.TabPages[2].Controls[0]).DataSource;
                    var dt_UDO3 = (DataTable)((GridControl)XtraTabLine.TabPages[3].Controls[0]).DataSource;
                    var dt_UDO4 = (DataTable)((GridControl)XtraTabLine.TabPages[4].Controls[0]).DataSource;

                    foreach (DataRow item in dt_UDO.Rows)
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

                        var Childs = dt_UDO1.AsEnumerable().Where(t => Function.ToString(t["Code"]) == udo.Code);
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

                        var FindColumns = dt_UDO2.AsEnumerable().Where(t => Function.ToString(t["Code"]) == udo.Code);
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

                        var FormColumns = dt_UDO3.AsEnumerable().Where(t => Function.ToString(t["Code"]) == udo.Code);
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

                        var EnhancedFormColumns = dt_UDO4.AsEnumerable().Where(t => Function.ToString(t["Code"]) == udo.Code);
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
                            var ret = FunctionHelper.AddUdo(udo);
                            Logging.Write(Logging.WATCH, ret);
                            var error1 = txt_result.Text;
                            error1 += "\n" + "ADD UDO" + "\t" + ret;
                            txt_result.Text = error1;
                        }
                        else
                        {
                            var ret = FunctionHelper.UpdateUdo(udo);
                            Logging.Write(Logging.WATCH, ret);
                            var error1 = txt_result.Text;
                            error1 += "\n" + "UPDATE UDO" + "\t" + ret;
                            txt_result.Text = error1;
                        }
                    }
                    MessageBox.Show("DONE");
                }

            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, ex.Message, Function.ToString(ex.InnerException), sourceType.Equals("S") ? SQLConnectionSource : HanaConnectionSource, HanaConnectionString);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                btn_CopyData.Enabled = true;
                btn_pasteFromClipboard.Enabled = true;
            }
        }

        private void btn_pasteFromClipboard_Click(object sender, EventArgs e)
        {
            try
            {
                // lấy ra tab đang focus
                var tabPage = XtraTabLine.SelectedTabPage;
                var grd_data = (GridControl)tabPage.Controls[0];
                var gv_data = (GridView)grd_data.MainView;
                PasteFromExcel(grd_data, gv_data);
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, ex.Message, Function.ToString(ex.InnerException), sourceType.Equals("S") ? SQLConnectionSource : HanaConnectionSource);
                MessageBox.Show(ex.Message);
            }
        }

        private void grd_data_EditorKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.V && e.Modifiers == Keys.Control)
            {
                // lấy ra tab đang focus
                var grd_data = (GridControl)sender;
                var gv_data = (GridView)grd_data.MainView;
                PasteFromExcel(grd_data, gv_data);
            }
        }

        private void btn_copyExcel_Click(object sender, EventArgs e)
        {
            try
            {
                // lấy ra tab đang focus
                var tabPage = XtraTabLine.SelectedTabPage;
                var grd_data = (GridControl)tabPage.Controls[0];
                var gv_data = (GridView)grd_data.MainView;
                gv_data.CopyToClipboard();
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, ex.Message);
                MessageBox.Show(ex.Message);
            }
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

        public static DataTable GetDataFromExcel(string clipboardData)
        {
            DataTable tbl = new DataTable();
            try
            {
                List<string> data = new List<string>(clipboardData.Split('\n'));
                bool firstRow = true;

                if (data.Count > 0 && string.IsNullOrWhiteSpace(data[data.Count - 1]))
                {
                    data.RemoveAt(data.Count - 1);
                }

                foreach (string iterationRow in data)
                {
                    string row = iterationRow;
                    if (row.EndsWith("\r"))
                    {
                        row = row.Substring(0, row.Length - "\r".Length);
                    }

                    string[] rowData = row.Split(new char[] { '\r', '\x09' });
                    DataRow newRow = tbl.NewRow();
                    if (firstRow)
                    {
                        int colNumber = 0;
                        foreach (string value in rowData)
                        {
                            if (string.IsNullOrWhiteSpace(value))
                            {
                                tbl.Columns.Add(string.Format("[BLANK{0}]", colNumber));
                            }
                            else if (!tbl.Columns.Contains(value))
                            {
                                tbl.Columns.Add(value);
                            }
                            else
                            {
                                tbl.Columns.Add(string.Format("Column {0}", colNumber));
                            }
                            colNumber++;
                        }
                        for (int i = 0; i < rowData.Length; i++)
                        {
                            if (i >= tbl.Columns.Count) break;
                            newRow[i] = rowData[i];
                        }
                        tbl.Rows.Add(newRow);
                        firstRow = false;
                    }
                    else
                    {
                        for (int i = 0; i < rowData.Length; i++)
                        {
                            if (i >= tbl.Columns.Count) break;
                            newRow[i] = rowData[i];
                        }
                        tbl.Rows.Add(newRow);
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, ex.Message);
                return new DataTable();
            }
            return tbl;

        }

        public static void PasteFromExcel(GridControl grd, GridView gv)
        {
            try
            {
                var data = (DataObject)Clipboard.GetDataObject();
                if (grd == null)
                {
                    return;
                }
                DataTable dt_clipBoard = null;
                if (data == null)
                {
                    return;
                }
                else
                {
                    if (data.GetData(DataFormats.UnicodeText) != null)
                    {
                        var clipboardData = data.GetData(DataFormats.UnicodeText).ToString();
                        dt_clipBoard = GetDataFromExcel(clipboardData);
                    }
                    else
                    {
                        return;
                    }
                }
                if (dt_clipBoard != null && (dt_clipBoard.Rows.Count > 1 || dt_clipBoard.Columns.Count > 1))
                {
                    var dt_data = grd.DataSource as DataTable;
                    dt_data.Rows.Clear();
                    var colCountClipboard = dt_clipBoard.Columns.Count;
                    for (var i = 0; i < dt_clipBoard.Rows.Count; i++)
                    {
                        var rowNew = dt_data.NewRow();
                        for (var j = 0; j < dt_data.Columns.Count && j < colCountClipboard; j++)
                        {
                            if (dt_clipBoard.Rows[i][j] == null || string.IsNullOrEmpty(dt_clipBoard.Rows[i][j].ToString()))
                            {
                                rowNew[j] = DBNull.Value;
                            }
                            else
                            {
                                rowNew[j] = dt_clipBoard.Rows[i][j];
                            }
                        }
                        dt_data.Rows.Add(rowNew);
                    }
                    dt_data.AcceptChanges();
                }
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, ex.Message);
                MessageBox.Show(ex.Message);
            }
        }

        private void rdo_Type_EditValueChanging(object sender, DevExpress.XtraEditors.Controls.ChangingEventArgs e)
        {
            try
            {
                _type = _type == CopyType.UDO ? CopyType.UDT : CopyType.UDO;
                LoadData();
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, ex.Message);
                MessageBox.Show(ex.Message);
            }
        }

        private void LoadData()
        {
            try
            {
                if (_type == CopyType.UDT)
                {
                    this.Text = "Copy UDT Manual";

                    string sql = "SELECT TOP 0 * FROM OUTB; ";
                    sql += "SELECT TOP 0 * FROM CUFD; ";
                    sql += "SELECT TOP 0 * FROM UFD1; ";

                    var ds = _httpClient.ExecuteData(sql);
                    if (ds.Tables.Count > 2)
                    {
                        // tạo 3 tab
                        XtraTabLine.TabPages.Clear();
                        XtraTabPage tab_OUTB = new XtraTabPage();
                        tab_OUTB.Text = "UDT - OUTB";
                        GridControl grd_OUTB = new GridControl();
                        GridView gv_OUTB = new GridView();
                        gv_OUTB.OptionsView.ShowGroupPanel = false;
                        gv_OUTB.CustomDrawRowIndicator += view_CustomDrawRowIndicator;
                        gv_OUTB.OptionsView.ColumnAutoWidth = true;
                        gv_OUTB.OptionsSelection.MultiSelect = true;
                        gv_OUTB.OptionsSelection.MultiSelectMode = GridMultiSelectMode.RowSelect;
                        grd_OUTB.Dock = DockStyle.Fill;
                        grd_OUTB.MainView = gv_OUTB;
                        grd_OUTB.DataSource = ds.Tables[0];
                        grd_OUTB.EditorKeyDown += grd_data_EditorKeyDown;
                        tab_OUTB.Controls.Add(grd_OUTB);

                        XtraTabPage tab_CUFD = new XtraTabPage();
                        tab_CUFD.Text = "UDT - CUFD";
                        GridControl grd_CUFD = new GridControl();
                        GridView gv_CUFD = new GridView();
                        gv_CUFD.OptionsView.ShowGroupPanel = false;
                        gv_CUFD.CustomDrawRowIndicator += view_CustomDrawRowIndicator;
                        gv_CUFD.OptionsView.ColumnAutoWidth = true;
                        gv_CUFD.OptionsSelection.MultiSelect = true;
                        gv_CUFD.OptionsSelection.MultiSelectMode = GridMultiSelectMode.RowSelect;
                        grd_CUFD.Dock = DockStyle.Fill;
                        grd_CUFD.MainView = gv_CUFD;
                        grd_CUFD.DataSource = ds.Tables[1];
                        grd_CUFD.EditorKeyDown += grd_data_EditorKeyDown;
                        tab_CUFD.Controls.Add(grd_CUFD);

                        XtraTabPage tab_UFD1 = new XtraTabPage();
                        tab_UFD1.Text = "UDT - UFD1";
                        GridControl grd_UFD1 = new GridControl();
                        GridView gv_UFD1 = new GridView();
                        gv_UFD1.OptionsView.ShowGroupPanel = false;
                        gv_UFD1.CustomDrawRowIndicator += view_CustomDrawRowIndicator;
                        gv_UFD1.OptionsView.ColumnAutoWidth = true;
                        gv_UFD1.OptionsSelection.MultiSelect = true;
                        gv_UFD1.OptionsSelection.MultiSelectMode = GridMultiSelectMode.RowSelect;
                        grd_UFD1.Dock = DockStyle.Fill;
                        grd_UFD1.MainView = gv_UFD1;
                        grd_UFD1.DataSource = ds.Tables[2];
                        grd_UFD1.EditorKeyDown += grd_data_EditorKeyDown;
                        tab_UFD1.Controls.Add(grd_UFD1);

                        XtraTabLine.TabPages.Add(tab_OUTB);
                        XtraTabLine.TabPages.Add(tab_CUFD);
                        XtraTabLine.TabPages.Add(tab_UFD1);
                    }
                }
                else
                {
                    this.Text = "Copy UDO Manual";
                    string sql = "SELECT TOP 0 * FROM OUDO; ";
                    sql += "SELECT TOP 0 * FROM UDO1; ";
                    sql += "SELECT TOP 0 * FROM UDO2; ";
                    sql += "SELECT TOP 0 * FROM UDO3; ";
                    sql += "SELECT TOP 0 * FROM UDO4; ";

                    var ds = _httpClient.ExecuteData(sql);

                    if (ds.Tables.Count > 4)
                    {
                        //tạo 5 tab tương ứng 5 bảng
                        XtraTabLine.TabPages.Clear();
                        XtraTabPage tab_ODO = new XtraTabPage();
                        tab_ODO.Text = "UDO - OUDO";
                        GridControl grd_ODO = new GridControl();
                        GridView gv_ODO = new GridView();
                        gv_ODO.OptionsView.ShowGroupPanel = false;
                        gv_ODO.CustomDrawRowIndicator += view_CustomDrawRowIndicator;
                        gv_ODO.OptionsView.ColumnAutoWidth = true;
                        gv_ODO.OptionsSelection.MultiSelect = true;
                        gv_ODO.OptionsSelection.MultiSelectMode = GridMultiSelectMode.RowSelect;
                        grd_ODO.Dock = DockStyle.Fill;
                        grd_ODO.MainView = gv_ODO;
                        grd_ODO.DataSource = ds.Tables[0];
                        grd_ODO.EditorKeyDown += grd_data_EditorKeyDown;
                        tab_ODO.Controls.Add(grd_ODO);

                        XtraTabPage tab_UDO1 = new XtraTabPage();
                        tab_UDO1.Text = "UDO - UDO1";
                        GridControl grd_UDO1 = new GridControl();
                        GridView gv_UDO1 = new GridView();
                        gv_UDO1.OptionsView.ShowGroupPanel = false;
                        gv_UDO1.CustomDrawRowIndicator += view_CustomDrawRowIndicator;
                        gv_UDO1.OptionsView.ColumnAutoWidth = true;
                        gv_UDO1.OptionsSelection.MultiSelect = true;
                        gv_UDO1.OptionsSelection.MultiSelectMode = GridMultiSelectMode.RowSelect;
                        grd_UDO1.Dock = DockStyle.Fill;
                        grd_UDO1.MainView = gv_UDO1;
                        grd_UDO1.DataSource = ds.Tables[1];
                        grd_UDO1.EditorKeyDown += grd_data_EditorKeyDown;
                        tab_UDO1.Controls.Add(grd_UDO1);

                        XtraTabPage tab_UDO2 = new XtraTabPage();
                        tab_UDO2.Text = "UDO - UDO2";
                        GridControl grd_UDO2 = new GridControl();
                        GridView gv_UDO2 = new GridView();
                        gv_UDO2.OptionsView.ShowGroupPanel = false;
                        gv_UDO2.CustomDrawRowIndicator += view_CustomDrawRowIndicator;
                        gv_UDO2.OptionsView.ColumnAutoWidth = true;
                        gv_UDO2.OptionsSelection.MultiSelect = true;
                        gv_UDO2.OptionsSelection.MultiSelectMode = GridMultiSelectMode.RowSelect;
                        grd_UDO2.Dock = DockStyle.Fill;
                        grd_UDO2.MainView = gv_UDO2;
                        grd_UDO2.DataSource = ds.Tables[2];
                        grd_UDO2.EditorKeyDown += grd_data_EditorKeyDown;
                        tab_UDO2.Controls.Add(grd_UDO2);

                        XtraTabPage tab_UDO3 = new XtraTabPage();
                        tab_UDO3.Text = "UDO - UDO3";
                        GridControl grd_UDO3 = new GridControl();
                        GridView gv_UDO3 = new GridView();
                        gv_UDO3.OptionsView.ShowGroupPanel = false;
                        gv_UDO3.CustomDrawRowIndicator += view_CustomDrawRowIndicator;
                        gv_UDO3.OptionsView.ColumnAutoWidth = true;
                        gv_UDO3.OptionsSelection.MultiSelect = true;
                        gv_UDO3.OptionsSelection.MultiSelectMode = GridMultiSelectMode.RowSelect;
                        grd_UDO3.Dock = DockStyle.Fill;
                        grd_UDO3.MainView = gv_UDO3;
                        grd_UDO3.DataSource = ds.Tables[3];
                        grd_UDO3.EditorKeyDown += grd_data_EditorKeyDown;
                        tab_UDO3.Controls.Add(grd_UDO3);

                        XtraTabPage tab_UDO4 = new XtraTabPage();
                        tab_UDO4.Text = "UDO - UDO4";
                        GridControl grd_UDO4 = new GridControl();
                        GridView gv_UDO4 = new GridView();
                        gv_UDO4.OptionsView.ShowGroupPanel = false;
                        gv_UDO4.CustomDrawRowIndicator += view_CustomDrawRowIndicator;
                        gv_UDO4.OptionsView.ColumnAutoWidth = true;
                        gv_UDO4.OptionsSelection.MultiSelect = true;
                        gv_UDO4.OptionsSelection.MultiSelectMode = GridMultiSelectMode.RowSelect;
                        grd_UDO4.Dock = DockStyle.Fill;
                        grd_UDO4.MainView = gv_UDO4;
                        grd_UDO4.DataSource = ds.Tables[4];
                        grd_UDO4.EditorKeyDown += grd_data_EditorKeyDown;
                        tab_UDO4.Controls.Add(grd_UDO4);

                        XtraTabLine.TabPages.Add(tab_ODO);
                        XtraTabLine.TabPages.Add(tab_UDO1);
                        XtraTabLine.TabPages.Add(tab_UDO2);
                        XtraTabLine.TabPages.Add(tab_UDO3);
                        XtraTabLine.TabPages.Add(tab_UDO4);
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, ex.Message);
                MessageBox.Show(ex.Message);
            }
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
                var dt = _httpClient.ExecuteDataTable(string.Format(@"SELECT 1 FROM CUFD WHERE ""TableID"" = N'{0}' AND ""AliasID"" = N'{1}';", tableName, fieldName));
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

    }
}