/********************************************************************
 Module:   			Config interface
 Description: 		Define app configuration
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
            if(CConfig.DebugBreak)
            {
                System.Diagnostics.Debugger.Break();
            }
  
---------------------------------------------------------------------
 Revisions:  		
 
*********************************************************************/
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;

namespace DAIS.eBank.Services.PDF.Signer
{
    public class CConfig
    {
        public static string ApplicationName
        {
            get
            {
                return "DAIS.eBank.Services.PDF.Signer";
            }
        }

        public static bool DebugBreak
        {
            get
            {
                bool debugBreak;
                bool.TryParse(ConfigurationManager.AppSettings["DebugBreak"], out debugBreak);

                return debugBreak;
            }
        }

        public static bool HAS_FOOTER_TEXT
        {
            get
            {
                return !string.IsNullOrEmpty(APPEND_FOOTER_TEXT);
            }
        }

        public static string APPEND_FOOTER_TEXT
        {
            get
            {
                string text = ConfigurationManager.AppSettings["APPEND_FOOTER_TEXT"];

                return text;
            }
        }

        public static string PDFSERVER_WS
        {
            get
            {
                return "WSHttpBinding_IDocumentServer";
            }
        }

        public static string FONT_PATH
        {
            get
            {
                string font = System.Environment.SystemDirectory + "\\..\\Fonts\\" + ConfigurationManager.AppSettings["DEFAULT_FONT"];
                if(string.IsNullOrEmpty(ConfigurationManager.AppSettings["DEFAULT_FONT"]) || !File.Exists(font))
                {
                    string defaultFont = System.Environment.SystemDirectory + "\\..\\Fonts\\arial.ttf";

                    WriteToEventLog(string.Format("Cannot find selected font {0}, falling back to default {1}", font, defaultFont), CConfig.ApplicationName, CConfig.ApplicationName, EventLogEntryType.Warning);

                    return defaultFont;
                }

                return font;
            }
        }

        /// <summary>
        /// Writes the message as an entry in the Windows EventLog
        /// </summary>
        /// <param name="logEntry">message to write as an entry</param>
        /// <param name="catchException">true if you want the function to catch any exceptions ocuuring when writing to the EventLog</param>
        /// <param name="eventSource">name for the source in the EventLog</param>
        /// <param name="logName">log name used by the EventLog</param>
        /// <param name="logEntryType">entry type to log event as</param>
        public static void WriteToEventLog(string logEntry, string eventSource, string logName, EventLogEntryType logEntryType)
        {
            //Register the App as an Event Source
            if(!EventLog.SourceExists(eventSource))
            {
                try
                {//creating an event source requires Adminitsrative privileges, may throw an exception
                    EventLog.CreateEventSource(eventSource, logName);
                }
                catch(Exception)//InvalidOperationException e)
                {//fallback to default
                    eventSource = "Application";
                }
            }
            EventLog.WriteEntry(eventSource, logEntry, logEntryType);
        }
    }
}
