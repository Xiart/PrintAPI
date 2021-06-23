using System;
using Printing;
using System.Collections.Generic;

namespace TestPrinting
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> PList = Printing.Printing.GetPrinters();
            Printing.Printing.PrintHTML(PList[0], "<html><head></head><body><h1>Hello World... Day 4 Ziyad...</h1></body></html>");
        }
    }
}
