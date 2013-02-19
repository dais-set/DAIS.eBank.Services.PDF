/********************************************************************
 Module:   			PDF DocumentServer interface
 Description: 		Define a document server interface
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
  
---------------------------------------------------------------------
 Revisions:  		
 
*********************************************************************/
using System.ServiceModel;

namespace DAIS.eBank.Services.PDF.Interfaces
{
    [ServiceContract]
    public interface IDocumentServer
    {
        [OperationContract]
        string GetDocumentToSign(string onlineFormID, string bankUserID, string sessionID);

        [OperationContract]
        void UpdateSignedDocument(string signedDocument, string onlineFormID, string bankUserID, string sessionID);
    }
}
