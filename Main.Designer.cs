
namespace SAP.QuickCopyUDF
{
    partial class frm_Main
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
            
            // Release COM object using helper method
            ReleaseCompanyObject();
            
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btn_CreateUDT = new System.Windows.Forms.Button();
            this.btn_CreateUDF = new System.Windows.Forms.Button();
            this.btn_Login = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.btn_CreateUDO = new System.Windows.Forms.Button();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.txt_Sql_Server = new System.Windows.Forms.TextBox();
            this.btn_Clear = new System.Windows.Forms.Button();
            this.txt_Sql_Database = new System.Windows.Forms.TextBox();
            this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
            this.txt_Sql_Pass = new System.Windows.Forms.TextBox();
            this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
            this.txt_Sql_User = new System.Windows.Forms.TextBox();
            this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
            this.txt_HanaPass = new System.Windows.Forms.TextBox();
            this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
            this.txt_HanaUser = new System.Windows.Forms.TextBox();
            this.labelControl6 = new DevExpress.XtraEditors.LabelControl();
            this.txt_Hana_Database = new System.Windows.Forms.TextBox();
            this.labelControl7 = new DevExpress.XtraEditors.LabelControl();
            this.txt_ServiceAddress = new System.Windows.Forms.TextBox();
            this.labelControl8 = new DevExpress.XtraEditors.LabelControl();
            this.btn_LinkUDF = new System.Windows.Forms.Button();
            this.txt_TableName = new System.Windows.Forms.TextBox();
            this.labelControl9 = new DevExpress.XtraEditors.LabelControl();
            this.btn_CopyData = new System.Windows.Forms.Button();
            this.btn_DelUDF = new System.Windows.Forms.Button();
            this.labelControl10 = new DevExpress.XtraEditors.LabelControl();
            this.txtServerType = new DevExpress.XtraEditors.LookUpEdit();
            this.txt_SapPass = new System.Windows.Forms.TextBox();
            this.labelControl11 = new DevExpress.XtraEditors.LabelControl();
            this.txt_SapUser = new System.Windows.Forms.TextBox();
            this.labelControl12 = new DevExpress.XtraEditors.LabelControl();
            this.txt_ServerHana = new System.Windows.Forms.TextBox();
            this.lbl_ServerHana = new DevExpress.XtraEditors.LabelControl();
            this.btn_TestSource = new System.Windows.Forms.Button();
            this.btn_TestTarget = new System.Windows.Forms.Button();
            this.btn_CopyManual = new System.Windows.Forms.Button();
            this.btn_LogOut = new System.Windows.Forms.Button();
            this.btn_DelCopy = new System.Windows.Forms.Button();
            this.btn_UpdateUDF = new System.Windows.Forms.Button();
            this.btn_CopyUDOManual = new System.Windows.Forms.Button();
            this.txtTargetServiceType = new DevExpress.XtraEditors.LookUpEdit();
            this.labelControl13 = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)(this.txtServerType.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtTargetServiceType.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // btn_CreateUDT
            // 
            this.btn_CreateUDT.Location = new System.Drawing.Point(127, 174);
            this.btn_CreateUDT.Name = "btn_CreateUDT";
            this.btn_CreateUDT.Size = new System.Drawing.Size(105, 23);
            this.btn_CreateUDT.TabIndex = 1;
            this.btn_CreateUDT.Text = "Create UDT";
            this.btn_CreateUDT.UseVisualStyleBackColor = true;
            this.btn_CreateUDT.Click += new System.EventHandler(this.btn_CreateUDT_Click);
            // 
            // btn_CreateUDF
            // 
            this.btn_CreateUDF.Location = new System.Drawing.Point(238, 174);
            this.btn_CreateUDF.Name = "btn_CreateUDF";
            this.btn_CreateUDF.Size = new System.Drawing.Size(105, 23);
            this.btn_CreateUDF.TabIndex = 2;
            this.btn_CreateUDF.Text = "Create UDF";
            this.btn_CreateUDF.UseVisualStyleBackColor = true;
            this.btn_CreateUDF.Click += new System.EventHandler(this.btn_CreateUDF_Click);
            // 
            // btn_Login
            // 
            this.btn_Login.Location = new System.Drawing.Point(16, 174);
            this.btn_Login.Name = "btn_Login";
            this.btn_Login.Size = new System.Drawing.Size(105, 23);
            this.btn_Login.TabIndex = 3;
            this.btn_Login.Text = "Login";
            this.btn_Login.UseVisualStyleBackColor = true;
            this.btn_Login.Click += new System.EventHandler(this.login_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBox1.Location = new System.Drawing.Point(0, 203);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(1066, 529);
            this.richTextBox1.TabIndex = 5;
            this.richTextBox1.Text = "";
            // 
            // btn_CreateUDO
            // 
            this.btn_CreateUDO.Location = new System.Drawing.Point(349, 174);
            this.btn_CreateUDO.Name = "btn_CreateUDO";
            this.btn_CreateUDO.Size = new System.Drawing.Size(105, 23);
            this.btn_CreateUDO.TabIndex = 6;
            this.btn_CreateUDO.Text = "Create UDO";
            this.btn_CreateUDO.UseVisualStyleBackColor = true;
            this.btn_CreateUDO.Click += new System.EventHandler(this.btn_CreateUDO_Click);
            // 
            // labelControl1
            // 
            this.labelControl1.Location = new System.Drawing.Point(25, 45);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(68, 13);
            this.labelControl1.TabIndex = 7;
            this.labelControl1.Text = "Source Server";
            // 
            // txt_Sql_Server
            // 
            this.txt_Sql_Server.Location = new System.Drawing.Point(106, 38);
            this.txt_Sql_Server.Name = "txt_Sql_Server";
            this.txt_Sql_Server.Size = new System.Drawing.Size(205, 20);
            this.txt_Sql_Server.TabIndex = 8;
            // 
            // btn_Clear
            // 
            this.btn_Clear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_Clear.Location = new System.Drawing.Point(973, 174);
            this.btn_Clear.Name = "btn_Clear";
            this.btn_Clear.Size = new System.Drawing.Size(80, 23);
            this.btn_Clear.TabIndex = 9;
            this.btn_Clear.Text = "Clear";
            this.btn_Clear.UseVisualStyleBackColor = true;
            this.btn_Clear.Click += new System.EventHandler(this.btn_Clear_Click);
            // 
            // txt_Sql_Database
            // 
            this.txt_Sql_Database.Location = new System.Drawing.Point(106, 64);
            this.txt_Sql_Database.Name = "txt_Sql_Database";
            this.txt_Sql_Database.Size = new System.Drawing.Size(205, 20);
            this.txt_Sql_Database.TabIndex = 11;
            // 
            // labelControl2
            // 
            this.labelControl2.Location = new System.Drawing.Point(25, 71);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(68, 13);
            this.labelControl2.TabIndex = 10;
            this.labelControl2.Text = "SQL Database";
            // 
            // txt_Sql_Pass
            // 
            this.txt_Sql_Pass.Location = new System.Drawing.Point(106, 116);
            this.txt_Sql_Pass.Name = "txt_Sql_Pass";
            this.txt_Sql_Pass.Size = new System.Drawing.Size(205, 20);
            this.txt_Sql_Pass.TabIndex = 15;
            // 
            // labelControl3
            // 
            this.labelControl3.Location = new System.Drawing.Point(25, 123);
            this.labelControl3.Name = "labelControl3";
            this.labelControl3.Size = new System.Drawing.Size(44, 13);
            this.labelControl3.TabIndex = 14;
            this.labelControl3.Text = "SQL Pass";
            // 
            // txt_Sql_User
            // 
            this.txt_Sql_User.Location = new System.Drawing.Point(106, 90);
            this.txt_Sql_User.Name = "txt_Sql_User";
            this.txt_Sql_User.Size = new System.Drawing.Size(205, 20);
            this.txt_Sql_User.TabIndex = 13;
            // 
            // labelControl4
            // 
            this.labelControl4.Location = new System.Drawing.Point(25, 97);
            this.labelControl4.Name = "labelControl4";
            this.labelControl4.Size = new System.Drawing.Size(44, 13);
            this.labelControl4.TabIndex = 12;
            this.labelControl4.Text = "SQL User";
            // 
            // txt_HanaPass
            // 
            this.txt_HanaPass.Location = new System.Drawing.Point(701, 116);
            this.txt_HanaPass.Name = "txt_HanaPass";
            this.txt_HanaPass.Size = new System.Drawing.Size(205, 20);
            this.txt_HanaPass.TabIndex = 23;
            // 
            // labelControl5
            // 
            this.labelControl5.Location = new System.Drawing.Point(652, 123);
            this.labelControl5.Name = "labelControl5";
            this.labelControl5.Size = new System.Drawing.Size(37, 13);
            this.labelControl5.TabIndex = 22;
            this.labelControl5.Text = "db Pass";
            // 
            // txt_HanaUser
            // 
            this.txt_HanaUser.Location = new System.Drawing.Point(442, 116);
            this.txt_HanaUser.Name = "txt_HanaUser";
            this.txt_HanaUser.Size = new System.Drawing.Size(205, 20);
            this.txt_HanaUser.TabIndex = 21;
            // 
            // labelControl6
            // 
            this.labelControl6.Location = new System.Drawing.Point(361, 123);
            this.labelControl6.Name = "labelControl6";
            this.labelControl6.Size = new System.Drawing.Size(50, 13);
            this.labelControl6.TabIndex = 20;
            this.labelControl6.Text = "Hana User";
            // 
            // txt_Hana_Database
            // 
            this.txt_Hana_Database.Location = new System.Drawing.Point(442, 64);
            this.txt_Hana_Database.Name = "txt_Hana_Database";
            this.txt_Hana_Database.Size = new System.Drawing.Size(205, 20);
            this.txt_Hana_Database.TabIndex = 19;
            // 
            // labelControl7
            // 
            this.labelControl7.Location = new System.Drawing.Point(361, 71);
            this.labelControl7.Name = "labelControl7";
            this.labelControl7.Size = new System.Drawing.Size(58, 13);
            this.labelControl7.TabIndex = 18;
            this.labelControl7.Text = "CompanyDB";
            // 
            // txt_ServiceAddress
            // 
            this.txt_ServiceAddress.Location = new System.Drawing.Point(442, 38);
            this.txt_ServiceAddress.Name = "txt_ServiceAddress";
            this.txt_ServiceAddress.Size = new System.Drawing.Size(205, 20);
            this.txt_ServiceAddress.TabIndex = 17;
            // 
            // labelControl8
            // 
            this.labelControl8.Location = new System.Drawing.Point(361, 45);
            this.labelControl8.Name = "labelControl8";
            this.labelControl8.Size = new System.Drawing.Size(74, 13);
            this.labelControl8.TabIndex = 16;
            this.labelControl8.Text = "ServiceAddress";
            // 
            // btn_LinkUDF
            // 
            this.btn_LinkUDF.Location = new System.Drawing.Point(460, 174);
            this.btn_LinkUDF.Name = "btn_LinkUDF";
            this.btn_LinkUDF.Size = new System.Drawing.Size(90, 23);
            this.btn_LinkUDF.TabIndex = 24;
            this.btn_LinkUDF.Text = "UDF Link UDO";
            this.btn_LinkUDF.UseVisualStyleBackColor = true;
            this.btn_LinkUDF.Click += new System.EventHandler(this.btn_LinkUDF_Click);
            // 
            // txt_TableName
            // 
            this.txt_TableName.Location = new System.Drawing.Point(106, 142);
            this.txt_TableName.Name = "txt_TableName";
            this.txt_TableName.Size = new System.Drawing.Size(541, 20);
            this.txt_TableName.TabIndex = 26;
            // 
            // labelControl9
            // 
            this.labelControl9.Location = new System.Drawing.Point(25, 149);
            this.labelControl9.Name = "labelControl9";
            this.labelControl9.Size = new System.Drawing.Size(76, 13);
            this.labelControl9.TabIndex = 25;
            this.labelControl9.Text = "Table Bulk Copy";
            // 
            // btn_CopyData
            // 
            this.btn_CopyData.Location = new System.Drawing.Point(653, 140);
            this.btn_CopyData.Name = "btn_CopyData";
            this.btn_CopyData.Size = new System.Drawing.Size(66, 23);
            this.btn_CopyData.TabIndex = 27;
            this.btn_CopyData.Text = "Copy Data";
            this.btn_CopyData.UseVisualStyleBackColor = true;
            this.btn_CopyData.Click += new System.EventHandler(this.btn_CopyData_Click);
            // 
            // btn_DelUDF
            // 
            this.btn_DelUDF.Location = new System.Drawing.Point(644, 174);
            this.btn_DelUDF.Name = "btn_DelUDF";
            this.btn_DelUDF.Size = new System.Drawing.Size(90, 23);
            this.btn_DelUDF.TabIndex = 28;
            this.btn_DelUDF.Text = "Delete UDF";
            this.btn_DelUDF.UseVisualStyleBackColor = true;
            this.btn_DelUDF.Click += new System.EventHandler(this.btn_DelUDF_Click);
            // 
            // labelControl10
            // 
            this.labelControl10.Location = new System.Drawing.Point(25, 19);
            this.labelControl10.Name = "labelControl10";
            this.labelControl10.Size = new System.Drawing.Size(60, 13);
            this.labelControl10.TabIndex = 29;
            this.labelControl10.Text = "Source Type";
            // 
            // txtServerType
            // 
            this.txtServerType.Location = new System.Drawing.Point(106, 9);
            this.txtServerType.Name = "txtServerType";
            this.txtServerType.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.txtServerType.Properties.Columns.AddRange(new DevExpress.XtraEditors.Controls.LookUpColumnInfo[] {
            new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Name", "Name"),
            new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Code", "Code", 20, DevExpress.Utils.FormatType.None, "", false, DevExpress.Utils.HorzAlignment.Default, DevExpress.Data.ColumnSortOrder.None, DevExpress.Utils.DefaultBoolean.Default)});
            this.txtServerType.Properties.DisplayMember = "Name";
            this.txtServerType.Properties.NullText = "";
            this.txtServerType.Properties.ValueMember = "Code";
            this.txtServerType.Size = new System.Drawing.Size(205, 20);
            this.txtServerType.TabIndex = 34;
            this.txtServerType.EditValueChanged += new System.EventHandler(this.txtServerType_EditValueChanged);
            // 
            // txt_SapPass
            // 
            this.txt_SapPass.Location = new System.Drawing.Point(701, 90);
            this.txt_SapPass.Name = "txt_SapPass";
            this.txt_SapPass.Size = new System.Drawing.Size(205, 20);
            this.txt_SapPass.TabIndex = 38;
            // 
            // labelControl11
            // 
            this.labelControl11.Location = new System.Drawing.Point(652, 97);
            this.labelControl11.Name = "labelControl11";
            this.labelControl11.Size = new System.Drawing.Size(43, 13);
            this.labelControl11.TabIndex = 37;
            this.labelControl11.Text = "Sap Pass";
            // 
            // txt_SapUser
            // 
            this.txt_SapUser.Location = new System.Drawing.Point(442, 90);
            this.txt_SapUser.Name = "txt_SapUser";
            this.txt_SapUser.Size = new System.Drawing.Size(205, 20);
            this.txt_SapUser.TabIndex = 36;
            // 
            // labelControl12
            // 
            this.labelControl12.Location = new System.Drawing.Point(361, 97);
            this.labelControl12.Name = "labelControl12";
            this.labelControl12.Size = new System.Drawing.Size(43, 13);
            this.labelControl12.TabIndex = 35;
            this.labelControl12.Text = "Sap User";
            // 
            // txt_ServerHana
            // 
            this.txt_ServerHana.Location = new System.Drawing.Point(442, 9);
            this.txt_ServerHana.Name = "txt_ServerHana";
            this.txt_ServerHana.Size = new System.Drawing.Size(205, 20);
            this.txt_ServerHana.TabIndex = 40;
            // 
            // lbl_ServerHana
            // 
            this.lbl_ServerHana.Location = new System.Drawing.Point(361, 16);
            this.lbl_ServerHana.Name = "lbl_ServerHana";
            this.lbl_ServerHana.Size = new System.Drawing.Size(60, 13);
            this.lbl_ServerHana.TabIndex = 39;
            this.lbl_ServerHana.Text = "Server Hana";
            // 
            // btn_TestSource
            // 
            this.btn_TestSource.Location = new System.Drawing.Point(701, 7);
            this.btn_TestSource.Name = "btn_TestSource";
            this.btn_TestSource.Size = new System.Drawing.Size(153, 23);
            this.btn_TestSource.TabIndex = 41;
            this.btn_TestSource.Text = "Test Connection Source";
            this.btn_TestSource.UseVisualStyleBackColor = true;
            this.btn_TestSource.Click += new System.EventHandler(this.btn_TestSource_Click);
            // 
            // btn_TestTarget
            // 
            this.btn_TestTarget.Location = new System.Drawing.Point(701, 36);
            this.btn_TestTarget.Name = "btn_TestTarget";
            this.btn_TestTarget.Size = new System.Drawing.Size(153, 23);
            this.btn_TestTarget.TabIndex = 42;
            this.btn_TestTarget.Text = "Test Connection Target";
            this.btn_TestTarget.UseVisualStyleBackColor = true;
            this.btn_TestTarget.Click += new System.EventHandler(this.btn_TestTarget_Click);
            // 
            // btn_CopyManual
            // 
            this.btn_CopyManual.Location = new System.Drawing.Point(851, 140);
            this.btn_CopyManual.Name = "btn_CopyManual";
            this.btn_CopyManual.Size = new System.Drawing.Size(80, 23);
            this.btn_CopyManual.TabIndex = 43;
            this.btn_CopyManual.Text = "Copy Manual";
            this.btn_CopyManual.UseVisualStyleBackColor = true;
            this.btn_CopyManual.Click += new System.EventHandler(this.btn_CopyManual_Click);
            // 
            // btn_LogOut
            // 
            this.btn_LogOut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_LogOut.Location = new System.Drawing.Point(862, 174);
            this.btn_LogOut.Name = "btn_LogOut";
            this.btn_LogOut.Size = new System.Drawing.Size(105, 23);
            this.btn_LogOut.TabIndex = 44;
            this.btn_LogOut.Text = "Log Out";
            this.btn_LogOut.UseVisualStyleBackColor = true;
            this.btn_LogOut.Click += new System.EventHandler(this.btn_LogOut_Click);
            // 
            // btn_DelCopy
            // 
            this.btn_DelCopy.Location = new System.Drawing.Point(725, 140);
            this.btn_DelCopy.Name = "btn_DelCopy";
            this.btn_DelCopy.Size = new System.Drawing.Size(120, 23);
            this.btn_DelCopy.TabIndex = 45;
            this.btn_DelCopy.Text = "Delete + Copy Data";
            this.btn_DelCopy.UseVisualStyleBackColor = true;
            this.btn_DelCopy.Click += new System.EventHandler(this.btn_DelCopy_Click);
            // 
            // btn_UpdateUDF
            // 
            this.btn_UpdateUDF.Location = new System.Drawing.Point(556, 174);
            this.btn_UpdateUDF.Name = "btn_UpdateUDF";
            this.btn_UpdateUDF.Size = new System.Drawing.Size(82, 23);
            this.btn_UpdateUDF.TabIndex = 46;
            this.btn_UpdateUDF.Text = "Update UDF";
            this.btn_UpdateUDF.UseVisualStyleBackColor = true;
            this.btn_UpdateUDF.Click += new System.EventHandler(this.btn_UpdateUDF_Click);
            // 
            // btn_CopyUDOManual
            // 
            this.btn_CopyUDOManual.Location = new System.Drawing.Point(937, 140);
            this.btn_CopyUDOManual.Name = "btn_CopyUDOManual";
            this.btn_CopyUDOManual.Size = new System.Drawing.Size(125, 23);
            this.btn_CopyUDOManual.TabIndex = 48;
            this.btn_CopyUDOManual.Text = "Copy Manual UDO";
            this.btn_CopyUDOManual.UseVisualStyleBackColor = true;
            this.btn_CopyUDOManual.Click += new System.EventHandler(this.btn_CopyUDOManual_Click);
            // 
            // txtTargetServiceType
            // 
            this.txtTargetServiceType.Location = new System.Drawing.Point(442, 64);
            this.txtTargetServiceType.Name = "txtTargetServiceType";
            this.txtTargetServiceType.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.txtTargetServiceType.Properties.Columns.AddRange(new DevExpress.XtraEditors.Controls.LookUpColumnInfo[] {
            new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Name", "Name")});
            this.txtTargetServiceType.Properties.DisplayMember = "Name";
            this.txtTargetServiceType.Properties.NullText = "";
            this.txtTargetServiceType.Properties.ValueMember = "Code";
            this.txtTargetServiceType.Size = new System.Drawing.Size(205, 20);
            this.txtTargetServiceType.TabIndex = 49;
            // 
            // labelControl13
            // 
            this.labelControl13.Location = new System.Drawing.Point(361, 71);
            this.labelControl13.Name = "labelControl13";
            this.labelControl13.Size = new System.Drawing.Size(61, 13);
            this.labelControl13.TabIndex = 50;
            this.labelControl13.Text = "Service Type";
            // 
            // frm_Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1066, 732);
            this.Controls.Add(this.labelControl13);
            this.Controls.Add(this.txtTargetServiceType);
            this.Controls.Add(this.btn_CopyUDOManual);
            this.Controls.Add(this.btn_UpdateUDF);
            this.Controls.Add(this.btn_DelCopy);
            this.Controls.Add(this.btn_LogOut);
            this.Controls.Add(this.btn_CopyManual);
            this.Controls.Add(this.btn_TestTarget);
            this.Controls.Add(this.btn_TestSource);
            this.Controls.Add(this.txt_ServerHana);
            this.Controls.Add(this.lbl_ServerHana);
            this.Controls.Add(this.txt_SapPass);
            this.Controls.Add(this.labelControl11);
            this.Controls.Add(this.txt_SapUser);
            this.Controls.Add(this.labelControl12);
            this.Controls.Add(this.txtServerType);
            this.Controls.Add(this.labelControl10);
            this.Controls.Add(this.btn_DelUDF);
            this.Controls.Add(this.btn_CopyData);
            this.Controls.Add(this.txt_TableName);
            this.Controls.Add(this.labelControl9);
            this.Controls.Add(this.btn_LinkUDF);
            this.Controls.Add(this.txt_HanaPass);
            this.Controls.Add(this.labelControl5);
            this.Controls.Add(this.txt_HanaUser);
            this.Controls.Add(this.labelControl6);
            this.Controls.Add(this.txt_Hana_Database);
            this.Controls.Add(this.labelControl7);
            this.Controls.Add(this.txt_ServiceAddress);
            this.Controls.Add(this.labelControl8);
            this.Controls.Add(this.txt_Sql_Pass);
            this.Controls.Add(this.labelControl3);
            this.Controls.Add(this.txt_Sql_User);
            this.Controls.Add(this.labelControl4);
            this.Controls.Add(this.txt_Sql_Database);
            this.Controls.Add(this.labelControl2);
            this.Controls.Add(this.btn_Clear);
            this.Controls.Add(this.txt_Sql_Server);
            this.Controls.Add(this.labelControl1);
            this.Controls.Add(this.btn_CreateUDO);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.btn_Login);
            this.Controls.Add(this.btn_CreateUDF);
            this.Controls.Add(this.btn_CreateUDT);
            this.Name = "frm_Main";
            this.Text = "Create UDF, UDT, UDO";
            this.Load += new System.EventHandler(this.frm_Home_Load);
            ((System.ComponentModel.ISupportInitialize)(this.txtServerType.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtTargetServiceType.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btn_CreateUDT;
        private System.Windows.Forms.Button btn_CreateUDF;
        private System.Windows.Forms.Button btn_Login;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button btn_CreateUDO;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private System.Windows.Forms.TextBox txt_Sql_Server;
        private System.Windows.Forms.Button btn_Clear;
        private System.Windows.Forms.TextBox txt_Sql_Database;
        private DevExpress.XtraEditors.LabelControl labelControl2;
        private System.Windows.Forms.TextBox txt_Sql_Pass;
        private DevExpress.XtraEditors.LabelControl labelControl3;
        private System.Windows.Forms.TextBox txt_Sql_User;
        private DevExpress.XtraEditors.LabelControl labelControl4;
        private System.Windows.Forms.TextBox txt_HanaPass;
        private DevExpress.XtraEditors.LabelControl labelControl5;
        private System.Windows.Forms.TextBox txt_HanaUser;
        private DevExpress.XtraEditors.LabelControl labelControl6;
        private System.Windows.Forms.TextBox txt_Hana_Database;
        private DevExpress.XtraEditors.LabelControl labelControl7;
        private System.Windows.Forms.TextBox txt_ServiceAddress;
        private DevExpress.XtraEditors.LabelControl labelControl8;
        private System.Windows.Forms.Button btn_LinkUDF;
        private System.Windows.Forms.TextBox txt_TableName;
        private DevExpress.XtraEditors.LabelControl labelControl9;
        private System.Windows.Forms.Button btn_CopyData;
        private System.Windows.Forms.Button btn_DelUDF;
        private DevExpress.XtraEditors.LabelControl labelControl10;
        private DevExpress.XtraEditors.LookUpEdit txtServerType;
        private System.Windows.Forms.TextBox txt_SapPass;
        private DevExpress.XtraEditors.LabelControl labelControl11;
        private System.Windows.Forms.TextBox txt_SapUser;
        private DevExpress.XtraEditors.LabelControl labelControl12;
        private System.Windows.Forms.TextBox txt_ServerHana;
        private DevExpress.XtraEditors.LabelControl lbl_ServerHana;
        private System.Windows.Forms.Button btn_TestSource;
        private System.Windows.Forms.Button btn_TestTarget;
        private System.Windows.Forms.Button btn_CopyManual;
        private System.Windows.Forms.Button btn_LogOut;
        private System.Windows.Forms.Button btn_DelCopy;
        private System.Windows.Forms.Button btn_UpdateUDF;
        private System.Windows.Forms.Button btn_CopyUDOManual;
        private DevExpress.XtraEditors.LookUpEdit txtTargetServiceType;
        private DevExpress.XtraEditors.LabelControl labelControl13;
    }
}

