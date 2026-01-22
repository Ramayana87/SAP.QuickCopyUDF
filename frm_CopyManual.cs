using Apzon.Commons;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SAP.QuickCopyUDF
{
    public partial class frm_CopyManual : DevExpress.XtraEditors.XtraForm
    {
        public string SQLConnectionSource { get; set; }

        public string HanaConnectionSource { get; set; }

        public string HanaConnectionString { get; set; }

        public DatabaseHanaClient _httpClient;

        public DatabaseHanaClient _httpClientSource;

        public string sourceType { get; set; }

        public frm_CopyManual()
        {
            InitializeComponent();
        }

        public frm_CopyManual(string type, string sourceConnectionString, string hanaConnectionString)
        {
            sourceType = type;
            HanaConnectionString = hanaConnectionString;
            if (type.Equals("S"))

                SQLConnectionSource = sourceConnectionString;
            else
                HanaConnectionSource = sourceConnectionString;
            InitializeComponent();
        }

        private void frm_CopyManual_Load(object sender, EventArgs e)
        {
            try
            {
                _httpClient = new DatabaseHanaClient(HanaConnectionString);
                if (sourceType.Equals("H"))
                {
                    _httpClientSource = new DatabaseHanaClient(HanaConnectionSource);
                }
                gv_data.CustomDrawRowIndicator += view_CustomDrawRowIndicator;
                gv_data.OptionsView.ColumnAutoWidth = true;
                gv_data.OptionsSelection.MultiSelect = true;
                gv_data.OptionsSelection.MultiSelectMode = GridMultiSelectMode.RowSelect;
                txt_query.Focus();
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

        private void btn_Execute_Click(object sender, EventArgs e)
        {
            try
            {
                btn_Execute.Enabled = false;
                btn_CopyData.Enabled = false;
                if (string.IsNullOrEmpty(txt_query.Text))
                    MessageBox.Show("Query is empty!");
                grd_data.DataSource = new DataTable();
                grd_data.RefreshDataSource();
                gv_data.Columns.Clear();
                var dt = new DataTable();
                if (sourceType.Equals("S"))
                    dt = ExecuteDataTable(txt_query.Text);
                else
                {
                    var sql = txt_query.Text;
                    if (!sql.EndsWith(";"))
                        sql += ";";
                    dt = _httpClientSource.ExecuteDataTable(sql);
                }

                if (null != dt)
                {
                    grd_data.DataSource = dt;
                    grd_data.RefreshDataSource();
                }
                else
                {
                    MessageBox.Show("Data is empty!");
                }
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, ex.Message, Function.ToString(ex.InnerException), sourceType.Equals("S") ? SQLConnectionSource : HanaConnectionSource);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                btn_Execute.Enabled = true;
                btn_CopyData.Enabled = true;
            }
        }

        private void btn_CopyData_Click(object sender, EventArgs e)
        {
            try
            {
                btn_Execute.Enabled = false;
                btn_CopyData.Enabled = false;
                if (string.IsNullOrEmpty(txt_TableTarget.Text))
                {
                    MessageBox.Show("Table target is empty!");
                    return;
                }
                var dt = (DataTable)grd_data.DataSource;
                if (null == dt || dt.Rows.Count == 0)
                {
                    MessageBox.Show("Data is empty!");
                    return;
                }
                if (XtraMessageBox.Show("Do you want to save change?", "Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }
                var ret = _httpClient.BulkCopy(dt, txt_TableTarget.Text);

                if (string.IsNullOrEmpty(ret))
                {
                    grd_data.DataSource = new DataTable();
                    grd_data.RefreshDataSource();
                    MessageBox.Show("Copy Success!");
                }
                else
                {
                    MessageBox.Show(ret);
                }
            }
            catch (Exception ex)
            {
                Logging.Write(Logging.ERROR, ex.Message, Function.ToString(ex.InnerException), sourceType.Equals("S") ? SQLConnectionSource : HanaConnectionSource, HanaConnectionString);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                btn_Execute.Enabled = true;
                btn_CopyData.Enabled = true;
            }
        }

        private void btn_pasteFromClipboard_Click(object sender, EventArgs e)
        {
            PasteFromExcel(grd_data, gv_data);
        }

        private void grd_data_EditorKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.V && e.Modifiers == Keys.Control)
            {
                PasteFromExcel(grd_data, gv_data);
            }
        }

        private void btn_copyExcel_Click(object sender, EventArgs e)
        {
            try
            {
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

    }
}