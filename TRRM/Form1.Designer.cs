﻿namespace TRRM
{
    partial class Form1
    {
        /// <summary>
        /// Variable del diseñador requerida.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén utilizando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben eliminar; false en caso contrario, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido del método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonOpen = new System.Windows.Forms.Button();
            this.buttonExtract = new System.Windows.Forms.Button();
            this.saveDialog = new System.Windows.Forms.SaveFileDialog();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.viewWireframe = new System.Windows.Forms.CheckBox();
            this.btnTestPARM = new System.Windows.Forms.Button();
            this.btnAll = new System.Windows.Forms.Button();
            this.btnSearch = new System.Windows.Forms.Button();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.btnTest = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.filesTV = new System.Windows.Forms.TreeView();
            this.chooseFolderDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.cameraZ = new System.Windows.Forms.NumericUpDown();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cameraZ)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonOpen
            // 
            this.buttonOpen.Location = new System.Drawing.Point(6, 19);
            this.buttonOpen.Name = "buttonOpen";
            this.buttonOpen.Size = new System.Drawing.Size(120, 24);
            this.buttonOpen.TabIndex = 0;
            this.buttonOpen.Text = "Select Data folder";
            this.buttonOpen.UseVisualStyleBackColor = true;
            this.buttonOpen.Click += new System.EventHandler(this.buttonOpen_Click);
            // 
            // buttonExtract
            // 
            this.buttonExtract.Location = new System.Drawing.Point(132, 19);
            this.buttonExtract.Name = "buttonExtract";
            this.buttonExtract.Size = new System.Drawing.Size(120, 24);
            this.buttonExtract.TabIndex = 5;
            this.buttonExtract.Text = "Extract Selected File";
            this.buttonExtract.UseVisualStyleBackColor = true;
            this.buttonExtract.Click += new System.EventHandler(this.buttonExtract_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox1.Controls.Add(this.cameraZ);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.viewWireframe);
            this.groupBox1.Controls.Add(this.btnTestPARM);
            this.groupBox1.Controls.Add(this.btnAll);
            this.groupBox1.Controls.Add(this.btnSearch);
            this.groupBox1.Controls.Add(this.txtSearch);
            this.groupBox1.Controls.Add(this.btnTest);
            this.groupBox1.Controls.Add(this.buttonOpen);
            this.groupBox1.Controls.Add(this.buttonExtract);
            this.groupBox1.Location = new System.Drawing.Point(12, 468);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(600, 91);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Commands";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(255, 68);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 13);
            this.label1.TabIndex = 13;
            this.label1.Text = "Camera Zoom:";
            // 
            // viewWireframe
            // 
            this.viewWireframe.AutoSize = true;
            this.viewWireframe.Location = new System.Drawing.Point(445, 64);
            this.viewWireframe.Name = "viewWireframe";
            this.viewWireframe.Size = new System.Drawing.Size(74, 17);
            this.viewWireframe.TabIndex = 12;
            this.viewWireframe.Text = "Wireframe";
            this.viewWireframe.UseVisualStyleBackColor = true;
            this.viewWireframe.CheckedChanged += new System.EventHandler(this.viewWireframe_CheckedChanged);
            // 
            // btnTestPARM
            // 
            this.btnTestPARM.Location = new System.Drawing.Point(87, 62);
            this.btnTestPARM.Name = "btnTestPARM";
            this.btnTestPARM.Size = new System.Drawing.Size(80, 23);
            this.btnTestPARM.TabIndex = 10;
            this.btnTestPARM.Text = "Check PARM";
            this.btnTestPARM.UseVisualStyleBackColor = true;
            this.btnTestPARM.Click += new System.EventHandler(this.btnTestPARM_Click);
            // 
            // btnAll
            // 
            this.btnAll.Location = new System.Drawing.Point(173, 62);
            this.btnAll.Name = "btnAll";
            this.btnAll.Size = new System.Drawing.Size(75, 23);
            this.btnAll.TabIndex = 9;
            this.btnAll.Text = "Draw Cube";
            this.btnAll.UseVisualStyleBackColor = true;
            this.btnAll.Click += new System.EventHandler(this.btnAll_Click);
            // 
            // btnSearch
            // 
            this.btnSearch.Location = new System.Drawing.Point(445, 20);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(49, 23);
            this.btnSearch.TabIndex = 8;
            this.btnSearch.Text = "Search";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // txtSearch
            // 
            this.txtSearch.Location = new System.Drawing.Point(258, 21);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(181, 20);
            this.txtSearch.TabIndex = 7;
            this.txtSearch.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyUp);
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(6, 62);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(75, 23);
            this.btnTest.TabIndex = 6;
            this.btnTest.Text = "Test Chunk";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.filesTV);
            this.groupBox2.Location = new System.Drawing.Point(12, 6);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(600, 456);
            this.groupBox2.TabIndex = 9;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "GLM Files";
            // 
            // filesTV
            // 
            this.filesTV.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.filesTV.Location = new System.Drawing.Point(6, 19);
            this.filesTV.Name = "filesTV";
            this.filesTV.Size = new System.Drawing.Size(588, 431);
            this.filesTV.TabIndex = 0;
            this.filesTV.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.filesTV_AfterSelect);
            // 
            // chooseFolderDialog
            // 
            this.chooseFolderDialog.ShowNewFolderButton = false;
            // 
            // cameraZ
            // 
            this.cameraZ.DecimalPlaces = 2;
            this.cameraZ.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.cameraZ.Location = new System.Drawing.Point(337, 63);
            this.cameraZ.Maximum = new decimal(new int[] {
            25,
            0,
            0,
            0});
            this.cameraZ.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.cameraZ.Name = "cameraZ";
            this.cameraZ.Size = new System.Drawing.Size(58, 20);
            this.cameraZ.TabIndex = 14;
            this.cameraZ.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.cameraZ.Value = new decimal(new int[] {
            175,
            0,
            0,
            131072});
            this.cameraZ.ValueChanged += new System.EventHandler(this.cameraZ_ValueChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 567);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "Form1";
            this.Text = "Tabula Rasa Resource Manager";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cameraZ)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonOpen;
        private System.Windows.Forms.Button buttonExtract;
        private System.Windows.Forms.SaveFileDialog saveDialog;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TreeView filesTV;
        private System.Windows.Forms.FolderBrowserDialog chooseFolderDialog;
        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Button btnAll;
        private System.Windows.Forms.Button btnTestPARM;
        private System.Windows.Forms.CheckBox viewWireframe;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown cameraZ;
    }
}

