/********************************************************************
 Module:   			EnumErrors
 Description: 		Define supported errors enumeration
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
            if((err & EnumErrors.NoDocument) == EnumErrors.NoDocument)
            {
                MessageBox.Show(Resources.GetString("ERR_NO_DOCS"),
                    Resources.GetString("ERR_NO_DOCS_TITLE"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
  
---------------------------------------------------------------------
 Revisions:  		
 
*********************************************************************/
using System;

namespace DAIS.eBank.Services.PDF.Signer
{
    [Flags]
    enum EnumErrors
    {
        Certificate = 1 << 0,
        User = 1 << 1,
        Form = 1 << 2,
        Service = 1 << 3,
        NoDocument = 1 << 4,

        All = Certificate | User | Form | Service | NoDocument
    }
}
