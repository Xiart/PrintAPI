using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using iText.Html2pdf;
using Spire.Pdf;

namespace PrintingAPI
{
    public static class Printing
    {   
        public static List<string> GetPrinters()
        {
            try
            {
                List<string> PName = new List<string>();
                for (int i = 0; i < PrinterSettings.InstalledPrinters.Count; i++)
                {
                    if (PrinterSettings.InstalledPrinters[i] != "Microsoft XPS Document Writer" && PrinterSettings.InstalledPrinters[i] != "Microsoft Print to PDF" && PrinterSettings.InstalledPrinters[i] != "Fax" && PrinterSettings.InstalledPrinters[i] != "AnyDesk Printer" && PrinterSettings.InstalledPrinters[i] != "OneNote for Windows 10")
                    {
                        PName.Add(PrinterSettings.InstalledPrinters[i]);
                    }
                }
                return PName;
            }
            catch(Exception ex)
            {
                return new List<string>() { ex.Message };
            }
            
        }

        public static string PrintHTML(string PrinterName, string HTML, string BaseURI = "")
        {
            try
            {
                FileInfo fout = new FileInfo("ToPrint.pdf");
                StreamWriter sw = new StreamWriter("ToPrint.html");
                sw.WriteLine(HTML);
                sw.Close();
                FileInfo fin = new FileInfo("ToPrint.html");
                ConverterProperties CP = new ConverterProperties();
                CP.SetBaseUri(BaseURI);
                HtmlConverter.ConvertToPdf(fin, fout, CP);
                return PrintDocument(PrinterName, "ToPrint.pdf");
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
           
        }
               
        public static string PrintDocument(string printer, string FileName)
        {
            try
            {
                PdfDocument pdf = new PdfDocument(FileName);
                pdf.PrintSettings.PrinterName = printer;
                pdf.Print();
                return "OK";
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
            
        }        
    }
}
