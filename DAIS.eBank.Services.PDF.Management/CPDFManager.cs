/********************************************************************
 Module:   			PDF Manager
 Description: 		Modify or sign a PDF document
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
            X509Certificate2 certificate;
            byte[] pdfBytes = CPDFManager.WindowsSign(docPDFBytes, certificate);
  
---------------------------------------------------------------------
 Revisions:  		
 
*********************************************************************/
using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace DAIS.eBank.Services.PDF.Management
{
    public class CPDFManager
    {        
        private const string ENCODING = "windows-1251";

        public static byte[] WindowsSign(byte[] pdfData, X509Certificate2 cert_NET, bool allowChanges = false)
        {
            Org.BouncyCastle.X509.X509Certificate cert_JAVA = Org.BouncyCastle.Security.DotNetUtilities.FromX509Certificate(cert_NET);
            Org.BouncyCastle.X509.X509Certificate[] chain = new Org.BouncyCastle.X509.X509Certificate[] { cert_JAVA };

            RSACryptoServiceProvider rsa = (RSACryptoServiceProvider)cert_NET.PrivateKey;
            RSAParameters rsaParam = rsa.ExportParameters(true);
            Org.BouncyCastle.Crypto.Parameters.RsaPrivateCrtKeyParameters BCKeyParms = new Org.BouncyCastle.Crypto.Parameters.RsaPrivateCrtKeyParameters(
new Org.BouncyCastle.Math.BigInteger(1, rsaParam.Modulus),/*modulus*/
new Org.BouncyCastle.Math.BigInteger(1, rsaParam.Exponent),/*publicExponent*/
new Org.BouncyCastle.Math.BigInteger(1, rsaParam.D),/*privateExponent*/
new Org.BouncyCastle.Math.BigInteger(1, rsaParam.P),/*p*/
new Org.BouncyCastle.Math.BigInteger(1, rsaParam.Q),/*q*/
new Org.BouncyCastle.Math.BigInteger(1, rsaParam.DP),/*dP*/
new Org.BouncyCastle.Math.BigInteger(1, rsaParam.DQ),/*dQ*/
new Org.BouncyCastle.Math.BigInteger(1, rsaParam.InverseQ));/*qInv*/

            PdfReader reader = new PdfReader(pdfData);
            reader.Appendable = true;

            MemoryStream msOut = new MemoryStream();

            PdfStamper st = PdfStamper.CreateSignature(reader, msOut, '\0', null, true);

            PdfSignatureAppearance sap = st.SignatureAppearance;

            if(allowChanges)
            {
                sap.CertificationLevel = PdfSignatureAppearance.CERTIFIED_FORM_FILLING_AND_ANNOTATIONS;
            }
            else
            {
            sap.CertificationLevel = PdfSignatureAppearance.CERTIFIED_NO_CHANGES_ALLOWED;
            }

            sap.SetCrypto(BCKeyParms, chain, null, PdfSignatureAppearance.WINCER_SIGNED);

            sap.PreClose();

            PdfSigGenericPKCS sig = sap.SigStandard;
            PdfLiteral lit = (PdfLiteral)sig.Get(PdfName.CONTENTS);
            int totalBuf = (lit.PosLength - 2) / 2;
            byte[] buf = new byte[8192];
            int n;
            Stream inp = sap.RangeStream;
            while((n = inp.Read(buf, 0, buf.Length)) > 0)
            {
                sig.Signer.Update(buf, 0, n);
            }
            buf = new byte[totalBuf];
            byte[] bsig = sig.SignerContents;
            Array.Copy(bsig, 0, buf, 0, bsig.Length);
            PdfString str = new PdfString(buf);
            str.SetHexWriting(true);
            PdfDictionary dic = new PdfDictionary();
            dic.Put(PdfName.CONTENTS, str);
            sap.Close(dic);
            st.Reader.Close();

            return msOut.ToArray();
        }

        public static byte[] SetDocumentFont(byte[] pdfData, string fontPath, float size)
        {
            iTextSharp.text.pdf.BaseFont baseFont = iTextSharp.text.pdf.BaseFont.CreateFont(fontPath, ENCODING, true);

            PdfReader reader = new PdfReader(pdfData);

            MemoryStream msOut = new MemoryStream();

            iTextSharp.text.Document doc = new iTextSharp.text.Document(reader.GetPageSizeWithRotation(1));
            PdfWriter writer = iTextSharp.text.pdf.PdfWriter.GetInstance(doc, msOut);

            //open the document
            doc.Open();

            PdfContentByte cb = writer.DirectContent;
            PdfImportedPage page;
            int rotation;
            int i = 0;

            while(i < reader.NumberOfPages)
            {//add pdf content
                i++;
                doc.SetPageSize(reader.GetPageSizeWithRotation(i));
                doc.NewPage();
                page = writer.GetImportedPage(reader, i);

                //set page font
                cb.SetFontAndSize(baseFont, size);

                rotation = reader.GetPageRotation(i);
                if(rotation == 90 || rotation == 270)
                {
                    cb.AddTemplate(page, 0, -1f, 1f, 0, 0, reader.GetPageSizeWithRotation(i).Height);
                }
                else
                {
                    cb.AddTemplate(page, 1f, 0, 0, 1f, 0, 0);
                }
            }

            doc.Close();

            return msOut.ToArray();
        }

        public static byte[] AddPageFooter(byte[] pdfData, string text, string fontPath)
        {
            PdfReader reader = new PdfReader(pdfData);

            MemoryStream msOut = new MemoryStream();

            iTextSharp.text.Document doc = new iTextSharp.text.Document(reader.GetPageSizeWithRotation(1));
            PdfWriter writer = iTextSharp.text.pdf.PdfWriter.GetInstance(doc, msOut);

            // Custom Footer is done using Event Handler
            CustomTextFooter pageEventHandler = new CustomTextFooter(fontPath, text);

            //assign custom handler to pdf
            writer.PageEvent = pageEventHandler;

            //open the document
            doc.Open();

            PdfContentByte cb = writer.DirectContent;
            PdfImportedPage page;
            int rotation;
            int i = 0;

            while(i < reader.NumberOfPages)
            {//add pdf content
                i++;
                doc.SetPageSize(reader.GetPageSizeWithRotation(i));
                doc.NewPage();
                page = writer.GetImportedPage(reader, i);

                rotation = reader.GetPageRotation(i);
                if(rotation == 90 || rotation == 270)
                {
                    cb.AddTemplate(page, 0, -1f, 1f, 0, 0, reader.GetPageSizeWithRotation(i).Height);
                }
                else
                {
                    cb.AddTemplate(page, 1f, 0, 0, 1f, 0, 0);
                }
            }

            doc.Close();

            return msOut.ToArray();
        }

        public static byte[] AddPageFooter(byte[] pdfData, string fontPath, X509Certificate2 cert_NET)
        {
            PdfReader reader = new PdfReader(pdfData);

            MemoryStream msOut = new MemoryStream();

            iTextSharp.text.Document doc = new iTextSharp.text.Document(reader.GetPageSizeWithRotation(1));
            PdfWriter writer = iTextSharp.text.pdf.PdfWriter.GetInstance(doc, msOut);

            // Custom Footer is done using Event Handler
            CustomCertificateFooter pageEventHandler = new CustomCertificateFooter(fontPath, cert_NET);

            //assign custom handler to pdf
            writer.PageEvent = pageEventHandler;

            //open the document
            doc.Open();

            PdfContentByte cb = writer.DirectContent;
            PdfImportedPage page;
            int rotation;
            int i = 0;

            while(i < reader.NumberOfPages)
            {//add pdf content
                i++;
                doc.SetPageSize(reader.GetPageSizeWithRotation(i));
                doc.NewPage();
                page = writer.GetImportedPage(reader, i);

                rotation = reader.GetPageRotation(i);
                if(rotation == 90 || rotation == 270)
                {
                    cb.AddTemplate(page, 0, -1f, 1f, 0, 0, reader.GetPageSizeWithRotation(i).Height);
                }
                else
                {
                    cb.AddTemplate(page, 1f, 0, 0, 1f, 0, 0);
                }
            }

            doc.Close();

            return msOut.ToArray();
        }

        public static byte[] AddPageLogo(byte[] pdfData, string imagePath)
        {
            PdfReader reader = new PdfReader(pdfData);

            MemoryStream msOut = new MemoryStream();

            iTextSharp.text.Document doc = new iTextSharp.text.Document(reader.GetPageSizeWithRotation(1));
            PdfWriter writer = iTextSharp.text.pdf.PdfWriter.GetInstance(doc, msOut);

            //open the document
            doc.Open();

            PdfContentByte cb = writer.DirectContent;
            PdfImportedPage page;
            int rotation;
            int i = 0;

            while(i < reader.NumberOfPages)
            {//add pdf content
                i++;
                doc.SetPageSize(reader.GetPageSizeWithRotation(i));
                doc.NewPage();
                page = writer.GetImportedPage(reader, i);

                if(File.Exists(imagePath))
                {
                    Image logo = iTextSharp.text.Image.GetInstance(imagePath);
                    //b/c the moronic lib scales the img by some magical value: account for it
                    logo.ScalePercent(100 * 61f / 94f);
                    logo.Border = 0;
                    logo.Alignment = Image.UNDERLYING;

                    float absoluteX = page.Width - 31.7f - logo.ScaledWidth;
                    float absoluteY = page.Height - 6f - logo.ScaledHeight;
                    logo.SetAbsolutePosition(absoluteX, absoluteY);

                    //add header image
                    cb.AddImage(logo);
                }

                rotation = reader.GetPageRotation(i);
                if(rotation == 90 || rotation == 270)
                {
                    cb.AddTemplate(page, 0, -1f, 1f, 0, 0, reader.GetPageSizeWithRotation(i).Height);
                }
                else
                {
                    cb.AddTemplate(page, 1f, 0, 0, 1f, 0, 0);
                }
            }

            doc.Close();

            return msOut.ToArray();
        }
    }

}
