namespace DAIS.eBank.Services.PDF.Signer
{
    partial class FormSign
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
            if(disposing && (components != null))
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSign));
            this.lblPrintCertInfo = new System.Windows.Forms.Label();
            this.btnSign = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.pbDocs = new System.Windows.Forms.ProgressBar();
            this.lblPlsWait = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblPrintCertInfo
            // 
            resources.ApplyResources(this.lblPrintCertInfo, "lblPrintCertInfo");
            this.lblPrintCertInfo.Name = "lblPrintCertInfo";
            // 
            // btnSign
            // 
            resources.ApplyResources(this.btnSign, "btnSign");
            this.btnSign.Name = "btnSign";
            this.btnSign.UseVisualStyleBackColor = true;
            this.btnSign.Click += new System.EventHandler(this.Sign_Click);
            // 
            // btnClose
            // 
            resources.ApplyResources(this.btnClose, "btnClose");
            this.btnClose.Name = "btnClose";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.Close_Click);
            // 
            // pbDocs
            // 
            resources.ApplyResources(this.pbDocs, "pbDocs");
            this.pbDocs.Name = "pbDocs";
            this.pbDocs.Step = 1;
            // 
            // lblPlsWait
            // 
            resources.ApplyResources(this.lblPlsWait, "lblPlsWait");
            this.lblPlsWait.Name = "lblPlsWait";
            // 
            // FormSign
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblPlsWait);
            this.Controls.Add(this.pbDocs);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnSign);
            this.Controls.Add(this.lblPrintCertInfo);
            this.Name = "FormSign";
            this.Load += new System.EventHandler(this.FormSign_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblPrintCertInfo;
        private System.Windows.Forms.Button btnSign;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.ProgressBar pbDocs;
        private System.Windows.Forms.Label lblPlsWait;
    }
}

