/********************************************************************
 Module:   			Main form
 Description: 		App main form
 Author:   			Ogniana Rainova
 Company:  			DAIS Software Ltd.
 Date:              2012.11.14
 Copyright:         2012 DAIS Software Ltd.
 NOTES:   			
    This file is part of the DAIS.eBank.Services.PDF.Editor library.

    DAIS.eBank.Services.PDF.Editor library is free software: you
    can redistribute it and/or modify it under the terms of the GNU
    General Public License as published by the Free Software Foundation,
    either version 3 of the License, or any later version.

    DAIS.eBank.Services.PDF.Editor library is distributed in
    the hope that it will be useful, but WITHOUT ANY WARRANTY; without
    even the implied warranty of MERCHANTABILITY or FITNESS FOR A
    PARTICULAR PURPOSE.
    See the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with DAIS.eBank.Services.PDF.Editor library.
    If not, see <http://www.gnu.org/licenses/>.

    For more information on DAIS.eBank.Services.PDF.Editor,
    please contact DAIS Software Ltd. at this address: software@dais-set.com
---------------------------------------------------------------------
 Usage:   	  
---------------------------------------------------------------------
 Revisions:  		
 
*********************************************************************/
using System;
using System.Collections.Specialized;
using System.Deployment.Application;
using System.Diagnostics;
using System.Reflection;
using System.Resources;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using DAIS.eBank.Services.PDF.Interfaces;
using DAIS.eBank.Services.PDF.Management;

namespace DAIS.eBank.Services.PDF.Signer
{
    public partial class FormSign : Form
    {
        #region Properties

        private static ResourceManager Resources = new ResourceManager("DAIS.eBank.Services.PDF.Signer.resources.Strings", typeof(FormSign).Assembly);

        private ChannelFactory<IDocumentServer> m_DocumentServerFactory = new ChannelFactory<IDocumentServer>(CConfig.PDFSERVER_WS);

        private bool m_hasCertificate = false;

        X509Certificate2 m_cert;

        private string m_Thumbprint;
        private string m_onlineFormID;

        private string m_BankUserID;
        private string m_SessionID;

        private NameValueCollection QueryString;

        private string m_doc2Sign;

        private Thread m_threadSign;

        private delegate void DisplayDelegate();
        private delegate void ProgressBarStartDelegate(int count);
        private delegate void ShowErrorDelegate(EnumErrors error);
        private delegate void SetPropertyThreadSafeDelegate(Control control, string propertyName, object propertyValue);

        #endregion Properties


        #region Form Events

        public FormSign()
        {
            if(CConfig.DebugBreak)
            {
                System.Diagnostics.Debugger.Break();
            }

            InitializeComponent();

            m_threadSign = new Thread(new ThreadStart(DoSign));
        }

        private void FormSign_Load(object sender, EventArgs e)
        {
            LoadProperties();

            GetClientCert(m_Thumbprint);

            if(m_cert != null && VerifyCertificate(m_cert, m_Thumbprint))
            {
                m_hasCertificate = true;
            }
            else
            {
                CConfig.WriteToEventLog(Resources.GetString("NO_CLIENT_CERTIFICATE"), CConfig.ApplicationName, CConfig.ApplicationName, EventLogEntryType.Error);

                ExitWithError(EnumErrors.Certificate);
            }
        }

        #endregion Form Events

        #region Form Actions

        public static void SetPropertyThreadSafe(Control control, string propertyName, object propertyValue)
        {
            if(control.InvokeRequired)
            {
                control.Invoke(new SetPropertyThreadSafeDelegate(SetPropertyThreadSafe), new object[] { control, propertyName, propertyValue });
            }
            else
            {
                control.GetType().InvokeMember(propertyName, BindingFlags.SetProperty, null, control, new object[] { propertyValue });
            }
        }

        #region Progress Bar

        private void StartProgressBar(int maxStep)
        {
            if(pbDocs.InvokeRequired)
            {//invoke it
                pbDocs.Invoke(new ProgressBarStartDelegate(StartProgressBar), new object[] { maxStep });
            }
            else
            {//do it
                //sets the max progress - add two to pretify it (for start and end)
                pbDocs.Maximum = maxStep + 2;

                //start the progress bar
                pbDocs.PerformStep();
            }
        }

        private void StepProgressBar()
        {
            if(pbDocs.InvokeRequired)
            {//invoke it
                pbDocs.Invoke(new DisplayDelegate(pbDocs.PerformStep));
            }
            else
            {//do it
                //new step on the progress bar
                pbDocs.PerformStep();
            }
        }

        private void FinishProgress()
        {
            if(pbDocs.InvokeRequired)
            {//invoke it
                pbDocs.Invoke(new DisplayDelegate(FinishProgress));
            }
            else
            {//do it
                //finish the progress bar
                pbDocs.PerformStep();

                lblPlsWait.Visible = false;
            }
        }

        #endregion Progress Bar

        private void ShowSuccessMessage()
        {
            if(this.InvokeRequired)
            {//invoke it
                this.Invoke(new DisplayDelegate(ShowSuccessMessage));
            }
            else
            {//do it
                //success, show msg
                MessageBox.Show(Resources.GetString("SIGNED_SUCCESS"), "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ShowError(EnumErrors err)
        {
            if(this.InvokeRequired)
            {//invoke it
                this.Invoke(new ShowErrorDelegate(this.ShowError), new object[] { err });
            }
            else
            {//do it
                ExitWithError(err);
            }
        }

        private void CloseForm()
        {
            if(this.InvokeRequired)
            {//invoke it
                this.Invoke(new DisplayDelegate(this.CloseForm));
            }
            else
            {//do it
                this.Close();
            }
        }

        private void DoSign()
        {
            try
            {
                if(m_hasCertificate)
                {
                    //call the method to retrieve form docs to be signed, supply userid to verify user rights

                    if(GetDocument())
                    {//successfully retrieved document to be signed
                        if(!string.IsNullOrEmpty(m_doc2Sign))
                        {//doc is not empty
                            StartProgressBar(1);

                            if(CConfig.HAS_FOOTER_TEXT)
                            {//add footer text
                                m_doc2Sign = Convert.ToBase64String(CPDFManager.AddPageFooter(Convert.FromBase64String(m_doc2Sign), CConfig.FONT_PATH, m_cert));
                            }

                            m_doc2Sign = Convert.ToBase64String(CPDFManager.WindowsSign(Convert.FromBase64String(m_doc2Sign), m_cert));

                            //step the progress bar
                            StepProgressBar();

                            if(UpdateDocument())
                            {
                                //final step for the progress bar and close the form
                                FinishProgress();

                                //success, show msg
                                ShowSuccessMessage();
                            }
                            else
                            {
                                CConfig.WriteToEventLog(Resources.GetString("EX_UPDATE_DOC"), CConfig.ApplicationName, CConfig.ApplicationName, EventLogEntryType.Error);

                                ShowError(EnumErrors.Service);
                            }
                        }
                        else
                        {
                            CConfig.WriteToEventLog(Resources.GetString("EX_EMPTY_DOC"), CConfig.ApplicationName, CConfig.ApplicationName, EventLogEntryType.Information);

                            ShowError(EnumErrors.NoDocument);
                        }
                    }
                    else
                    {
                        CConfig.WriteToEventLog(Resources.GetString("EX_GET_DOCS"), CConfig.ApplicationName, CConfig.ApplicationName, EventLogEntryType.Warning);

                        ShowError(EnumErrors.Service);
                    }
                }
                else
                {
                    CConfig.WriteToEventLog(Resources.GetString("EX_NO_CLIENT_CERT"), CConfig.ApplicationName, CConfig.ApplicationName, EventLogEntryType.Error);

                    ShowError(EnumErrors.Certificate);
                }
            }
            catch(Exception ex)
            {//swallow ex, show error
                CConfig.WriteToEventLog(ex.ToString(), CConfig.ApplicationName, CConfig.ApplicationName, EventLogEntryType.Error);

                ShowError(EnumErrors.Service);
            }

            CloseForm();
        }

        private void Sign_Click(object sender, EventArgs e)
        {
            if(m_threadSign.ThreadState == System.Threading.ThreadState.Unstarted)
            {
                lock(this)
                {
                    if(m_threadSign.ThreadState == System.Threading.ThreadState.Unstarted)
                    {
                        lblPlsWait.Visible = true;
                        btnSign.Enabled = false;
                        btnClose.Enabled = false;

                        m_threadSign.Start();
                    }
                }
            }
        }

        private bool GetDocument()
        {
            IDocumentServer docServerSvc = m_DocumentServerFactory.CreateChannel();

            try
            {
                m_doc2Sign = docServerSvc.GetDocumentToSign(m_onlineFormID, m_BankUserID, m_SessionID);

                return true;
            }
            catch(Exception e)
            {
                CConfig.WriteToEventLog(string.Format(Resources.GetString("UNABLE_TO_GET_DOCS_EX"), e.ToString()), CConfig.ApplicationName, CConfig.ApplicationName, EventLogEntryType.Error);

                return false;
            }
            finally
            {
                ((IClientChannel)docServerSvc).Close();
            }
        }

        private bool UpdateDocument()
        {
            IDocumentServer docServerSvc = m_DocumentServerFactory.CreateChannel();

            try
            {
                docServerSvc.UpdateSignedDocument(m_doc2Sign, m_onlineFormID, m_BankUserID, m_SessionID);

                return true;
            }
            catch(Exception e)
            {
                CConfig.WriteToEventLog(string.Format(Resources.GetString("UNABLE_TO_UPDATE_DOCS_EX"), e.ToString()), CConfig.ApplicationName, CConfig.ApplicationName, EventLogEntryType.Error);

                return false;
            }
            finally
            {
                ((IClientChannel)docServerSvc).Close();
            }
        }

        private void Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #endregion Form Actions

        private X509Certificate2 GetClientCert(string thumbprint)
        {
            if(m_cert == null)
            {
                try
                {
                    X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                    store.Open(OpenFlags.ReadOnly);

                    X509Certificate2Collection certificates = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, true);

                    if(certificates.Count == 0)
                    {
                        certificates = X509Certificate2UI.SelectFromCollection(store.Certificates, Resources.GetString("CLIENT_CERTIFICATE"), Resources.GetString("SELECT_CLIENT_CERTIFICATE"), X509SelectionFlag.SingleSelection);

                        if(certificates.Count == 0)
                        {
                            return null;
                        }
                    }

                    m_cert = certificates[0];
                }
                catch(Exception e)
                {
                    CConfig.WriteToEventLog(string.Format(Resources.GetString("CLIENT_CERTIFICATE_EX"), e.ToString()), CConfig.ApplicationName, CConfig.ApplicationName, EventLogEntryType.Error);
                }
            }//if(m_cert==null)

            if(m_cert != null)
            {//assign client certificate
                m_DocumentServerFactory.Credentials.ClientCertificate.Certificate = m_cert;
            }

            return m_cert;
        }

        private NameValueCollection GetQueryStringParameters()
        {
            NameValueCollection nameValueTable = new NameValueCollection();

            if(ApplicationDeployment.IsNetworkDeployed)
            {
                string queryString = ApplicationDeployment.CurrentDeployment.ActivationUri.Query;
                nameValueTable = HttpUtility.ParseQueryString(queryString);
            }
            else
            {
                string queryString = System.Environment.CommandLine;
                nameValueTable = HttpUtility.ParseQueryString(queryString);
            }

            return (nameValueTable);
        }

        private bool VerifyCertificate(X509Certificate2 m_cert, string Thumbprint)
        {
            bool valid = false;

            if(m_cert != null && !string.IsNullOrEmpty(m_cert.Thumbprint))
            {
                if(m_cert.Thumbprint.Equals(Thumbprint))
                {//the certificate the user presented reflects the one we supposedly require (or so does the web param say)
                    //thus we are certain that the user really is the one the certificate says he is from the Active Directory
                    valid = true;
                }
            }

            return valid;
        }

        private void LoadProperties()
        {
            QueryString = GetQueryStringParameters();

            m_onlineFormID = QueryString["guid"];
            m_Thumbprint = QueryString["thumb"];

            m_BankUserID = QueryString["user_id"];
            m_SessionID = QueryString["session_id"];
        }


        private void ExitWithError(EnumErrors err)
        {
            if((err & EnumErrors.Certificate) == EnumErrors.Certificate)
            {
                MessageBox.Show(Resources.GetString("ERR_CERT"), Resources.GetString("ERR_NO_CERTIFICATE_TITLE"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if((err & EnumErrors.User) == EnumErrors.User)
            {
                MessageBox.Show(Resources.GetString("ERR_NO_RIGHTS_USER"), Resources.GetString("ERR_NO_RIGHTS_TITLE"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if((err & EnumErrors.Form) == EnumErrors.Form)
            {
                MessageBox.Show(Resources.GetString("ERR_NO_RIGHTS_FORM"), Resources.GetString("ERR_NO_RIGHTS_TITLE"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if((err & EnumErrors.Service) == EnumErrors.Service)
            {
                MessageBox.Show(Resources.GetString("ERR_COMM"), Resources.GetString("ERR_COMM_TITLE"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if((err & EnumErrors.NoDocument) == EnumErrors.NoDocument)
            {
                MessageBox.Show(Resources.GetString("ERR_NO_DOCS"), Resources.GetString("ERR_NO_DOCS_TITLE"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            this.Close();
        }
    }
}
