using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


/*
 * Project Code		- major version of the project
 * Project version	- CodeName of the project
 *		1 - POSTBANK
 *		2 - WEASTEAST 
 *		3 - SGEB
 *		4 - Bulbank
 *		----------------
 *		19 - Societe Generale
 *		991 - GPLed Tool for PDFs
 * Build Number		- 1yymm release
 *					  2yymm debug
 * Revision			- 1ddhh
*/
#if DEBUG
[assembly: AssemblyVersion("5.991.21211.11616")]
#else  
[assembly: AssemblyVersion("5.991.11211.11616")] 
#endif


//[assembly: AssemblyTitle("")]
//[assembly: AssemblyDescription("")]
//[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("DAIS Software")]
[assembly: AssemblyProduct("eBank PDF Editor 2013 edition")]
[assembly: AssemblyCopyright("DAIS Software")] 
[assembly: AssemblyTrademark("DAIS Software")]
//[assembly: AssemblyCulture("en-US")]


//by default classes  visible to COM
[assembly: ComVisible(true)]
[assembly: CLSCompliant(false)]

