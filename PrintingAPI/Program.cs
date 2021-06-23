using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;

namespace PrintingAPI
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> PList = Printing.GetPrinters();
            Printing.PrintHTML(PList[0], "<html><head></head><body><h1>Hello WOrldyyyyyy!! ZIYAD here</h1></body></html>");
        }

        
    }
}
