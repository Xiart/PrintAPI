using Microsoft.Win32;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using PrintingAPI;
using System.Collections.Generic;
using System.Xml;

namespace PrintingTray
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ApplicationStartUp());
        }
    }

    public class ApplicationStartUp : Form
    {
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;
        public static bool IsStart;
        Thread t = new Thread(new ThreadStart(StartServer));
        private void InitializeComponent()
        {
            try
            {
                var path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
                RegistryKey key = Registry.CurrentUser.OpenSubKey(path, true);
                key.SetValue("Printing_JSMU_IT", Application.ExecutablePath.ToString());

                trayMenu = new ContextMenuStrip();
                trayMenu.Items.Add("Exit", null, OnExit);
                trayMenu.Items.Add("Restart", null, OnRestart);
                trayMenu.Items.Add("Start", null, OnStart);
                //trayMenu.MenuItems.Add("Stop", OnStop);
                trayIcon = new NotifyIcon();
                trayIcon.Text = "Printing";
                trayIcon.Icon = new Icon(global::PrintingTray.Properties.Resources.favicon, 40, 40);
                trayIcon.ContextMenuStrip = trayMenu;
                trayIcon.Visible = true;
                OnStart(null, null);
            }
            catch (Exception ex)
            {
                StreamWriter sw = new StreamWriter(@"D:\Output.txt", true);
                sw.WriteLine(ex.Message);
                sw.Close();
            }

        }

        //Ctor
        public ApplicationStartUp()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false;
            ShowInTaskbar = false;
            base.OnLoad(e);
        }

        private void OnExit(object sender, EventArgs e)
        {
            // Release the icon resource.
            IsStart = false;
            t.Abort();
            trayIcon.Dispose();
            Application.Exit();
        }


        private void OnRestart(object sender, EventArgs e)
        {
            // Release the icon resource.
            IsStart = false;
            t.Abort();
            OnStart(null, null);
        }

        private void OnStart(object sender, EventArgs e)
        {
            // Release the icon resource.
            IsStart = true;
            if (t.ThreadState == ThreadState.Suspended)
            {
                t.Resume();
            }
            else if (t.ThreadState == ThreadState.Unstarted)
            {
                t.Start();
            }

            else if (t.ThreadState == ThreadState.Aborted)
            {
                Thread t = new Thread(new ThreadStart(StartServer));
                t.Start();
            }
        }

        private void OnStop(object sender, EventArgs e)
        {
            try
            {
                IsStart = false;
                t.Abort();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }



        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                // Release the icon resource.
                trayIcon.Dispose();
            }
            base.Dispose(isDisposing);
        }

        public static void StartServer()
        {
            using (HttpListener server = new HttpListener())
            {
                server.Prefixes.Add("http://127.0.0.1:57686/");
                server.Prefixes.Add("http://localhost:57686/");
                server.Prefixes.Add("http://localhost:57686/api/v1/GetAllPrinters/");
                server.Prefixes.Add("http://localhost:57686/api/v1/PrintHTML/");
                server.Prefixes.Add("http://localhost:57686/api/v1/PrintDocument/");


                server.Start();
                while (IsStart)
                {
                    try
                    {

                        HttpListenerContext context = server.GetContext();
                        HttpListenerRequest request = context.Request;
                        HttpListenerResponse response = context.Response;
                        context.Response.AddHeader("Access-Control-Allow-Origin", "*");
                        context.Response.AddHeader("Access-Control-Allow-Headers", "X-Requested-With");
                        context.Response.AddHeader("Access-Control-Allow-Private-Network", "*");


                        string[] URL = context.Request.Url.ToString().Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                        if (URL.Length >= 5)
                        {
                            string URLMethod = URL[4];
                            switch (URLMethod)
                            {
                                case "PrintDocument":
                                    {
                                        string[] Params = new string[] { "", "" };
                                        if (URL.Length > 5)
                                        {
                                            //1[0]. Printer Name, 2[1]. File Location/Name complete
                                            Params = URL[5].Replace("%2F", "/").Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                                            string reply = Printing.PrintDocument(Params[0], Params[1]);
                                            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(reply);
                                            response.ContentLength64 = buffer.Length;
                                            response.OutputStream.Write(buffer, 0, buffer.Length);
                                            context.Response.Close();
                                        }

                                        break;
                                    }
                                case "GetAllPrinters":
                                    {
                                        List<string> PrinterList = Printing.GetPrinters();
                                        context.Response.ContentType = "application/xml";
                                        string obj = "";
                                        System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(PrinterList.GetType());
                                        using (var sww = new StringWriter())
                                        {
                                            using (XmlWriter writer = XmlWriter.Create(sww))
                                            {
                                                x.Serialize(writer, PrinterList);
                                                obj = sww.ToString(); // Your XML
                                            }
                                        }
                                        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(obj);
                                        response.ContentLength64 = buffer.Length;
                                        response.OutputStream.Write(buffer, 0, buffer.Length);
                                        context.Response.Close();
                                        break;
                                    }
                                case "PrintHTML":
                                    {
                                        string[] Params = new string[] { "", "", "", "" };
                                        if (request.HttpMethod == "GET")
                                        {
                                            //Params = URL[5].Replace("%5C", "\\").Replace("%2F", "/").Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                                            Params[0] = context.Request.QueryString[0];
                                            Params[1] = context.Request.QueryString[1];
                                            Params[2] = context.Request.QueryString[2];
                                        }
                                        else if (request.HttpMethod == "POST")
                                        {                                            
                                            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                                            {
                                                string t = reader.ReadToEnd();
                                                t = System.Web.HttpUtility.UrlDecode(t);
                                                Params = t.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                                                for(int i = 0; i < Params.Length; i++)
                                                {
                                                    Params[i] = Params[i].Substring(Params[i].IndexOf('=')+1);
                                                }
                                            }
                                        }
                                        string reply = "";
                                        if (Params.Length <= 3)
                                        {
                                            reply = Printing.PrintHTML(Params[0], Params[1], Convert.ToInt32(Params[2]));
                                        }
                                        else
                                        {
                                            reply = Printing.PrintHTML(Params[0], Params[1], Convert.ToInt32(Params[2]), Params[3]);
                                        }
                                        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(reply);
                                        response.ContentLength64 = buffer.Length;
                                        response.OutputStream.Write(buffer, 0, buffer.Length);
                                        context.Response.Close();
                                        break;
                                    }
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }



                }


            }


        }

    }
}
