# PrintAPI
The Repository COnsists of 3 following basic units:
1. PrintingAPI it is a DLL project based on .NET Core 3.1 it is the key to print the task provided to it.
2. PrintingTray application is a background application resides on a system tray and will provide an API interface to programmers to directly call the printer using following API links:
 
    2.a : to Get all the Printers connected : http://localhost:57686/api/v1/GetAllPrinters/
 
    2.b : To Print HTML directly on printer : http://localhost:57686/api/v1/PrintHTML/ParamPrinterName&HTMLToPrint
 
    2.c : to print a document specified : http://localhost:57686/api/v1/PrintDocument/PrinterName&DocumentFullNamewithPath

3. TestPrinting is a simple console application to test the DLL project.
 
