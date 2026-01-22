
using DevExpress.XtraEditors;

namespace SAP.QuickCopyUDF
{
    partial class frm_CopyUDOManual
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
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            this.panelControl2 = new DevExpress.XtraEditors.PanelControl();
            this.rdo_Type = new DevExpress.XtraEditors.RadioGroup();
            this.btn_pasteFromClipboard = new System.Windows.Forms.Button();
            this.btn_CopyData = new System.Windows.Forms.Button();
            this.txt_TableTarget = new System.Windows.Forms.TextBox();
            this.labelControl6 = new DevExpress.XtraEditors.LabelControl();
            this.txt_result = new DevExpress.XtraEditors.MemoEdit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl2)).BeginInit();
            this.panelControl2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.rdo_Type.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txt_result.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // panelControl1
            // 
            this.panelControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelControl1.Location = new System.Drawing.Point(0, 112);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(963, 500);
            this.panelControl1.TabIndex = 1;
            // 
            // panelControl2
            // 
            this.panelControl2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelControl2.Controls.Add(this.txt_result);
            this.panelControl2.Controls.Add(this.rdo_Type);
            this.panelControl2.Controls.Add(this.btn_pasteFromClipboard);
            this.panelControl2.Controls.Add(this.btn_CopyData);
            this.panelControl2.Controls.Add(this.txt_TableTarget);
            this.panelControl2.Controls.Add(this.labelControl6);
            this.panelControl2.Location = new System.Drawing.Point(0, -4);
            this.panelControl2.Name = "panelControl2";
            this.panelControl2.Size = new System.Drawing.Size(963, 110);
            this.panelControl2.TabIndex = 2;
            // 
            // rdo_Type
            // 
            this.rdo_Type.EditValue = 0;
            this.rdo_Type.Location = new System.Drawing.Point(77, 5);
            this.rdo_Type.Name = "rdo_Type";
            this.rdo_Type.Properties.Items.AddRange(new DevExpress.XtraEditors.Controls.RadioGroupItem[] {
            new DevExpress.XtraEditors.Controls.RadioGroupItem(0, "Add UDT"),
            new DevExpress.XtraEditors.Controls.RadioGroupItem(1, "Add UDO")});
            this.rdo_Type.Size = new System.Drawing.Size(210, 26);
            this.rdo_Type.TabIndex = 31;
            this.rdo_Type.EditValueChanging += new DevExpress.XtraEditors.Controls.ChangingEventHandler(this.rdo_Type_EditValueChanging);
            // 
            // btn_pasteFromClipboard
            // 
            this.btn_pasteFromClipboard.Location = new System.Drawing.Point(130, 64);
            this.btn_pasteFromClipboard.Name = "btn_pasteFromClipboard";
            this.btn_pasteFromClipboard.Size = new System.Drawing.Size(136, 23);
            this.btn_pasteFromClipboard.TabIndex = 30;
            this.btn_pasteFromClipboard.Text = "Paste from Clipboard";
            this.btn_pasteFromClipboard.UseVisualStyleBackColor = true;
            this.btn_pasteFromClipboard.Click += new System.EventHandler(this.btn_pasteFromClipboard_Click);
            // 
            // btn_CopyData
            // 
            this.btn_CopyData.Location = new System.Drawing.Point(19, 64);
            this.btn_CopyData.Name = "btn_CopyData";
            this.btn_CopyData.Size = new System.Drawing.Size(105, 23);
            this.btn_CopyData.TabIndex = 28;
            this.btn_CopyData.Text = "Copy to HANA";
            this.btn_CopyData.UseVisualStyleBackColor = true;
            this.btn_CopyData.Click += new System.EventHandler(this.btn_CopyData_Click);
            // 
            // txt_TableTarget
            // 
            this.txt_TableTarget.Location = new System.Drawing.Point(77, 37);
            this.txt_TableTarget.Name = "txt_TableTarget";
            this.txt_TableTarget.Size = new System.Drawing.Size(210, 21);
            this.txt_TableTarget.TabIndex = 23;
            // 
            // labelControl6
            // 
            this.labelControl6.Location = new System.Drawing.Point(10, 40);
            this.labelControl6.Name = "labelControl6";
            this.labelControl6.Size = new System.Drawing.Size(61, 13);
            this.labelControl6.TabIndex = 22;
            this.labelControl6.Text = "Table Target";
            // 
            // txt_result
            // 
            this.txt_result.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_result.Location = new System.Drawing.Point(293, 5);
            this.txt_result.Name = "txt_result";
            this.txt_result.Size = new System.Drawing.Size(665, 101);
            this.txt_result.TabIndex = 32;
            // 
            // frm_CopyUDOManual
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(963, 614);
            this.Controls.Add(this.panelControl2);
            this.Controls.Add(this.panelControl1);
            this.IconOptions.ShowIcon = false;
            this.Name = "frm_CopyUDOManual";
            this.Text = "Copy Manual";
            this.Load += new System.EventHandler(this.frm_CopyUDOManual_Load);
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl2)).EndInit();
            this.panelControl2.ResumeLayout(false);
            this.panelControl2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.rdo_Type.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txt_result.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private DevExpress.XtraEditors.PanelControl panelControl1;
        private DevExpress.XtraEditors.PanelControl panelControl2;
        private System.Windows.Forms.TextBox txt_TableTarget;
        private DevExpress.XtraEditors.LabelControl labelControl6;
        private System.Windows.Forms.Button btn_CopyData;
        private System.Windows.Forms.Button btn_pasteFromClipboard;
        private RadioGroup rdo_Type;
        private MemoEdit txt_result;
    }
}