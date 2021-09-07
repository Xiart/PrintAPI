using System;
using PrintingAPI;
using System.Collections.Generic;

namespace TestPrinting
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> PList = Printing.GetPrinters();
            Printing.PrintHTML(PList[2], "<html><head></head><body><h1>Hello World... Day 4 Ziyad...</h1></body></html>");
        }
    }
}
