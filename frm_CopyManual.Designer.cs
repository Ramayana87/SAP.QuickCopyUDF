
namespace SAP.QuickCopyUDF
{
    partial class frm_CopyManual
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txt_query = new DevExpress.XtraEditors.MemoEdit();
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            this.grd_data = new DevExpress.XtraGrid.GridControl();
            this.gv_data = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.panelControl2 = new DevExpress.XtraEditors.PanelControl();
            this.btn_pasteFromClipboard = new System.Windows.Forms.Button();
            this.btn_copyExcel = new System.Windows.Forms.Button();
            this.btn_CopyData = new System.Windows.Forms.Button();
            this.txt_TableTarget = new System.Windows.Forms.TextBox();
            this.labelControl6 = new DevExpress.XtraEditors.LabelControl();
            this.btn_Execute = new System.Windows.Forms.Button();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            ((System.ComponentModel.ISupportInitialize)(this.txt_query.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grd_data)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gv_data)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl2)).BeginInit();
            this.panelControl2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // txt_query
            // 
            this.txt_query.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_query.Location = new System.Drawing.Point(0, 5);
            this.txt_query.Name = "txt_query";
            this.txt_query.Size = new System.Drawing.Size(963, 101);
            this.txt_query.TabIndex = 0;
            // 
            // panelControl1
            // 
            this.panelControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelControl1.Controls.Add(this.grd_data);
            this.panelControl1.Location = new System.Drawing.Point(0, 142);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(963, 470);
            this.panelControl1.TabIndex = 1;
            // 
            // grd_data
            // 
            this.grd_data.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grd_data.Location = new System.Drawing.Point(2, 2);
            this.grd_data.MainView = this.gv_data;
            this.grd_data.Name = "grd_data";
            this.grd_data.Size = new System.Drawing.Size(959, 466);
            this.grd_data.TabIndex = 0;
            this.grd_data.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gv_data,
            this.gridView1});
            this.grd_data.EditorKeyDown += new System.Windows.Forms.KeyEventHandler(this.grd_data_EditorKeyDown);
            // 
            // gv_data
            // 
            this.gv_data.GridControl = this.grd_data;
            this.gv_data.Name = "gv_data";
            this.gv_data.OptionsView.ShowGroupPanel = false;
            // 
            // panelControl2
            // 
            this.panelControl2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelControl2.Controls.Add(this.btn_pasteFromClipboard);
            this.panelControl2.Controls.Add(this.btn_copyExcel);
            this.panelControl2.Controls.Add(this.btn_CopyData);
            this.panelControl2.Controls.Add(this.txt_TableTarget);
            this.panelControl2.Controls.Add(this.labelControl6);
            this.panelControl2.Controls.Add(this.btn_Execute);
            this.panelControl2.Controls.Add(this.txt_query);
            this.panelControl2.Location = new System.Drawing.Point(0, -4);
            this.panelControl2.Name = "panelControl2";
            this.panelControl2.Size = new System.Drawing.Size(963, 140);
            this.panelControl2.TabIndex = 2;
            // 
            // btn_pasteFromClipboard
            // 
            this.btn_pasteFromClipboard.Location = new System.Drawing.Point(134, 112);
            this.btn_pasteFromClipboard.Name = "btn_pasteFromClipboard";
            this.btn_pasteFromClipboard.Size = new System.Drawing.Size(136, 23);
            this.btn_pasteFromClipboard.TabIndex = 30;
            this.btn_pasteFromClipboard.Text = "Paste from Clipboard";
            this.btn_pasteFromClipboard.UseVisualStyleBackColor = true;
            this.btn_pasteFromClipboard.Click += new System.EventHandler(this.btn_pasteFromClipboard_Click);
            // 
            // btn_copyExcel
            // 
            this.btn_copyExcel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_copyExcel.Location = new System.Drawing.Point(822, 112);
            this.btn_copyExcel.Name = "btn_copyExcel";
            this.btn_copyExcel.Size = new System.Drawing.Size(136, 23);
            this.btn_copyExcel.TabIndex = 29;
            this.btn_copyExcel.Text = "Copy table to Clipboard";
            this.btn_copyExcel.UseVisualStyleBackColor = true;
            this.btn_copyExcel.Click += new System.EventHandler(this.btn_copyExcel_Click);
            // 
            // btn_CopyData
            // 
            this.btn_CopyData.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_CopyData.Location = new System.Drawing.Point(711, 112);
            this.btn_CopyData.Name = "btn_CopyData";
            this.btn_CopyData.Size = new System.Drawing.Size(105, 23);
            this.btn_CopyData.TabIndex = 28;
            this.btn_CopyData.Text = "Copy to HANA";
            this.btn_CopyData.UseVisualStyleBackColor = true;
            this.btn_CopyData.Click += new System.EventHandler(this.btn_CopyData_Click);
            // 
            // txt_TableTarget
            // 
            this.txt_TableTarget.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_TableTarget.Location = new System.Drawing.Point(606, 114);
            this.txt_TableTarget.Name = "txt_TableTarget";
            this.txt_TableTarget.Size = new System.Drawing.Size(99, 21);
            this.txt_TableTarget.TabIndex = 23;
            // 
            // labelControl6
            // 
            this.labelControl6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelControl6.Location = new System.Drawing.Point(539, 117);
            this.labelControl6.Name = "labelControl6";
            this.labelControl6.Size = new System.Drawing.Size(61, 13);
            this.labelControl6.TabIndex = 22;
            this.labelControl6.Text = "Table Target";
            // 
            // btn_Execute
            // 
            this.btn_Execute.Location = new System.Drawing.Point(12, 112);
            this.btn_Execute.Name = "btn_Execute";
            this.btn_Execute.Size = new System.Drawing.Size(105, 23);
            this.btn_Execute.TabIndex = 4;
            this.btn_Execute.Text = "Execute";
            this.btn_Execute.UseVisualStyleBackColor = true;
            this.btn_Execute.Click += new System.EventHandler(this.btn_Execute_Click);
            // 
            // gridView1
            // 
            this.gridView1.GridControl = this.grd_data;
            this.gridView1.Name = "gridView1";
            // 
            // frm_CopyManual
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(963, 614);
            this.Controls.Add(this.panelControl2);
            this.Controls.Add(this.panelControl1);
            this.IconOptions.ShowIcon = false;
            this.Name = "frm_CopyManual";
            this.Text = "Copy Manual";
            this.Load += new System.EventHandler(this.frm_CopyManual_Load);
            ((System.ComponentModel.ISupportInitialize)(this.txt_query.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grd_data)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gv_data)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl2)).EndInit();
            this.panelControl2.ResumeLayout(false);
            this.panelControl2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private DevExpress.XtraEditors.MemoEdit txt_query;
        private DevExpress.XtraEditors.PanelControl panelControl1;
        private DevExpress.XtraEditors.PanelControl panelControl2;
        private DevExpress.XtraGrid.GridControl grd_data;
        private DevExpress.XtraGrid.Views.Grid.GridView gv_data;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private System.Windows.Forms.Button btn_Execute;
        private System.Windows.Forms.TextBox txt_TableTarget;
        private DevExpress.XtraEditors.LabelControl labelControl6;
        private System.Windows.Forms.Button btn_CopyData;
        private System.Windows.Forms.Button btn_copyExcel;
        private System.Windows.Forms.Button btn_pasteFromClipboard;
    }
}