/********************************************************************
 Module:   			PDF Custom footers
 Description: 		Modify a PDF document to add custom footer text
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
 
in cs :	
            MemoryStream msOut = new MemoryStream();

            iTextSharp.text.Document doc = new iTextSharp.text.Document(reader.GetPageSizeWithRotation(1));
            PdfWriter writer = iTextSharp.text.pdf.PdfWriter.GetInstance(doc, msOut);
 
            CustomCertificateFooter pageEventHandler = new CustomCertificateFooter(fontPath, cert_NET);

            //assign custom handler to pdf
            writer.PageEvent = pageEventHandler;
  
---------------------------------------------------------------------
 Revisions:  		
 
*********************************************************************/
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace DAIS.eBank.Services.PDF.Management
{
    public class CCustomFooter : PdfPageEventHelper
    {
        private const string ENCODING = "windows-1251";

        /// <summary>
        /// This is the contentbyte object of the writer
        /// </summary>
        protected PdfContentByte cb;

        /// <summary>
        /// we will put the final number of pages in a template
        /// </summary>
        protected PdfTemplate template;

        /// <summary>
        /// this is the BaseFont we are going to use for the header / footer
        /// </summary>
        protected BaseFont bf = null;

        /// <summary>
        /// Keeps track of the creation time
        /// </summary>
        protected DateTime PrintTime = DateTime.Now;

        #region Properties

        public string FooterText { get; set; }

        public CCustomFooter(string fontPath, string footerText)
            : this(fontPath, footerText, DateTime.Now)
        { }

        public CCustomFooter(string fontPath, string footerText, DateTime now)
        {
            iTextSharp.text.pdf.BaseFont m_iText_BaseFont = iTextSharp.text.pdf.BaseFont.CreateFont(fontPath, ENCODING, true);

            bf = m_iText_BaseFont;

            FooterText = footerText;

            PrintTime = now;
        }

        #endregion Properties

        public override void OnOpenDocument(PdfWriter writer, Document document)
        {
            cb = writer.DirectContent;
            template = cb.CreateTemplate(50, 50);
        }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            base.OnEndPage(writer, document);

            float len = bf.GetWidthPoint(FooterText, 8);

            Rectangle pageSize = document.PageSize;

            cb.SetRGBColorFill(100, 100, 100);

            cb.BeginText();

            cb.SetFontAndSize(bf, 8);

            cb.ShowTextAligned(PdfContentByte.ALIGN_RIGHT,
                FooterText,
                pageSize.GetRight(40),
                pageSize.GetBottom(12), 0);

            cb.EndText();
        }

        public override void OnCloseDocument(PdfWriter writer, Document document)
        {
            base.OnCloseDocument(writer, document);

            template.BeginText();
            template.SetFontAndSize(bf, 8);
            template.SetTextMatrix(0, 0);
            template.ShowText("" + (writer.PageNumber - 1));
            template.EndText();
        }

    }

    public class CustomTextFooter : CCustomFooter
    {
        public CustomTextFooter(string fontPath, string footerText)
            : base(fontPath, footerText)
        { }
    }


    public class CustomCertificateFooter : CCustomFooter
    {
        private const string FOOTER_TEXT = "Документът е електронно подписан / This document is electronically signed";

        #region Properties

        public X509Certificate2 UserCertificate { get; set; }

        public List<string> CertificateFooter;

        #endregion Properties

        public CustomCertificateFooter(string fontPath, X509Certificate2 cert)
            : this(fontPath, cert, DateTime.Now)
        { }

        public CustomCertificateFooter(string fontPath, X509Certificate2 cert, DateTime now)
            : base(fontPath, "")
        {
            UserCertificate = cert;

            PrintTime = now;

            GetSubjectLines(cert);
        }

        private void GetSubjectLines(X509Certificate2 cert)
        {
            string[] attribs = new string[] { "CN=", "C=", "O=", "OU=" };

            CertificateFooter = new List<string>(2);

            CertificateFooter.Add(FOOTER_TEXT);

            string[] lines = cert.SubjectName.Format(true).Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, string> dictSubject = new Dictionary<string, string>(attribs.Length);
            StringBuilder sb = new StringBuilder(attribs.Length + 2);

            sb.AppendFormat("Date: {0:dd.MM.yyyy}", PrintTime);
            sb.Append(" DN: ");
            foreach(var item in attribs)
            {
                foreach(string line in lines)
                {
                    int ix = line.IndexOf(item);
                    if(ix > -1)
                    {
                        dictSubject.Add(item, line);
                        break;
                    }
                }

                sb.AppendFormat("{0} ", dictSubject[item]);
            }

            CertificateFooter.Add(sb.ToString());
        }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            float size = 8;

            float len = bf.GetWidthPoint(CertificateFooter[0], size);

            Rectangle pageSize = document.PageSize;

            cb.SetRGBColorFill(100, 100, 100);

            cb.BeginText();

            cb.SetFontAndSize(bf, size);

            for(int i = CertificateFooter.Count - 1; i >= 0; i--)
            {
                cb.ShowTextAligned(PdfContentByte.ALIGN_CENTER,
                    CertificateFooter[CertificateFooter.Count - 1 - i],
                    pageSize.Width / 2,
                    pageSize.GetBottom(size * (i + 1)), 0);
            }

            cb.EndText();
        }

        public override void OnCloseDocument(PdfWriter writer, Document document)
        {
            base.OnCloseDocument(writer, document);

            template.BeginText();
            template.SetFontAndSize(bf, 8);
            template.SetTextMatrix(0, 0);
            template.ShowText("" + (writer.PageNumber - 1));
            template.EndText();
        }
    }
}
