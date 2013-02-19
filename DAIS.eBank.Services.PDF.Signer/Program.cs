/********************************************************************
 Module:   			Program
 Description: 		App main thread
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
using System.Windows.Forms;

namespace DAIS.eBank.Services.PDF.Signer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormSign());
        }
    }
}
