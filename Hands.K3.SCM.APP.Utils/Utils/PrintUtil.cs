using O2S.Components.PDFRender4NET;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.DirectoryServices;
using System.Net;

namespace Hands.K3.SCM.APP.Utils.Utils
{
    public class PrintUtil
    {
        private static PrintDocument fPrintDocument = null;
        public static bool Print(string url, string printerName)
        {
            RawPrinterHelper.SendFileToPrinter(printerName,url);
            PrintDocument pd = new PrintDocument();
            pd.PrinterSettings.PrinterName = printerName;
            pd.DocumentName = url;
            

            pd.PrintController = new StandardPrintController();
            pd.Print();


            //PDFFile file = PDFFile.Open(url);
            //PrinterSettings settings = new PrinterSettings();
            //PrintDocument pd = new System.Drawing.Printing.PrintDocument();
            //settings.PrinterName = printerName;
            //settings.PrintToFile = false;

            //设置纸张大小（可以不设置，取默认设置）3.90 in,  8.65 in
            //PaperSize ps = new PaperSize("test", 4, 9);
            //ps.RawKind = 9; //如果是自定义纸张，就要大于118，（A4值为9，详细纸张类型与值的对照请看http://msdn.microsoft.com/zh-tw/library/system.drawing.printing.papersize.rawkind(v=vs.85).aspx）

            //O2S.Components.PDFRender4NET.Printing.PDFPrintSettings pdfPrintSettings = new O2S.Components.PDFRender4NET.Printing.PDFPrintSettings(settings);
            //pdfPrintSettings.PaperSize = ps;
            //pdfPrintSettings.PageScaling = O2S.Components.PDFRender4NET.Printing.PageScaling.FitToPrinterMarginsProportional;
            //pdfPrintSettings.PrinterSettings.Copies = 1;

            try
            {
                //file.Print(pdfPrintSettings);
                return true;
            }
            catch (Exception ex)
            {
                return false;
                throw;
            }
            finally
            {
                //file.Dispose();
            }

        }

        public static string GetDefaultPrinterName()
        {
            fPrintDocument = new PrintDocument();
            return fPrintDocument.PrinterSettings.PrinterName;
        }
        public static string GetLocalPrinter()
        {
            //whatthefuck("");
            //AvailablePrinters();
            ////GetDefaultPrinterName();
            ////PrinterHelper.GetPrinterList();
            //GetPrinters();
            //PrinterHelper_1.GetPrinterList(PrinterEnumFlags.PRINTER_ENUM_DEFAULT);
            //PrinterHelper_1.GetPrinterList(PrinterEnumFlags.PRINTER_ENUM_CONNECTIONS);
            //PrinterHelper_1.GetPrinterList(PrinterEnumFlags.PRINTER_ENUM_CONTAINER);
            //PrinterHelper_1.GetPrinterList(PrinterEnumFlags.PRINTER_ENUM_EXPAND);
            //PrinterHelper_1.GetPrinterList(PrinterEnumFlags.PRINTER_ENUM_FAVORITE);
            //PrinterHelper_1.GetPrinterList(PrinterEnumFlags.PRINTER_ENUM_HIDE);
            //PrinterHelper_1.GetPrinterList(PrinterEnumFlags.PRINTER_ENUM_ICON1);
            //PrinterHelper_1.GetPrinterList(PrinterEnumFlags.PRINTER_ENUM_ICON2);
            //PrinterHelper_1.GetPrinterList(PrinterEnumFlags.PRINTER_ENUM_ICON3);
            //PrinterHelper_1.GetPrinterList(PrinterEnumFlags.PRINTER_ENUM_ICON4);
            //PrinterHelper_1.GetPrinterList(PrinterEnumFlags.PRINTER_ENUM_ICON5);
            //PrinterHelper_1.GetPrinterList(PrinterEnumFlags.PRINTER_ENUM_ICON6);
            //PrinterHelper_1.GetPrinterList(PrinterEnumFlags.PRINTER_ENUM_ICON7);
            //PrinterHelper_1.GetPrinterList(PrinterEnumFlags.PRINTER_ENUM_ICON8);
            //PrinterHelper_1.GetPrinterList(PrinterEnumFlags.PRINTER_ENUM_ICONMASK);
            PrinterHelper_1.GetPrinterList(PrinterEnumFlags.PRINTER_ENUM_LOCAL);
            //PrinterHelper_1.GetPrinterList(PrinterEnumFlags.PRINTER_ENUM_NAME);
            PrinterHelper_1.GetPrinterList(PrinterEnumFlags.PRINTER_ENUM_NETWORK);
            PrinterHelper_1.GetPrinterList(PrinterEnumFlags.PRINTER_ENUM_REMOTE);
            PrinterHelper_1.GetPrinterList(PrinterEnumFlags.PRINTER_ENUM_SHARED);
            PrinterSearcher p = new PrinterSearcher();

            PrinterSearcher.PrinterInfo[] printers = p.Search(PRINTER_ENUM.SHARED);

            foreach (PrinterSearcher.PrinterInfo pi in printers)

            {

                Console.WriteLine("=====================================\n打印机名: {0}\n描叙:: {1}\n注释: {2}\n=====================================\n",

                pi.Name, pi.Description, pi.Comment);

            }
            foreach (string printerName in PrinterSettings.InstalledPrinters)
            {
                if (printerName.CompareTo("whatthefuck") == 0)
                {
                    return printerName;
                }
            }

            return @"\\10.2.0.141\FX DocuPrint M158 b";
        }

        public static void AvailablePrinters()
        {
            string printerName = string.Empty;
            ManagementScope ms = new ManagementScope(ManagementPath.DefaultPath);
            ms.Connect();

            SelectQuery sq = new SelectQuery
            {
                QueryString = @"SELECT Name FROM Win32_Printer"
            };

            ManagementObjectSearcher mos =
               new ManagementObjectSearcher(ms, sq);
            ManagementObjectCollection oObjectCollection = mos.Get();

            foreach (ManagementObject mo in oObjectCollection)
            {
                printerName = mo["Name"].ToString();
            }

        }

        public static void Whatthefuck(string printerName)
        {
            ManagementObjectSearcher MgmtSearcher;
            ManagementObjectCollection MgmtCollection;
            //Perform the search for printers and return the listing as a collection
            MgmtSearcher = new ManagementObjectSearcher(@"SELECT Name FROM Win32_Printer");
            MgmtCollection = MgmtSearcher.Get();

            foreach (ManagementObject objWMI in MgmtCollection)
            {

                string name = objWMI["Name"].ToString().ToLower();

                if (name.Equals(printerName.ToLower()))
                {

                    int state = Int32.Parse(objWMI["ExtendedPrinterStatus"].ToString());
                    if ((state == 1) || //Other
                            (state == 2) || //Unknown
                            (state == 7) || //Offline
                            (state == 9) || //error
                            (state == 11) //Not Available
                            )
                    {
                        throw new ApplicationException("hope you are finally offline");
                    }

                    state = Int32.Parse(objWMI["DetectedErrorState"].ToString());
                    if (state != 2) //No error
                    {
                        throw new ApplicationException("hope you are finally offline");
                    }

                }

            }
        }

        public static List<string> GetPrinters()
        {
            //List<string> printers = new List<string>();
            //DirectoryEntry printer = new DirectoryEntry("WinNT:", null, null, AuthenticationTypes.ServerBind);
            //Console.WriteLine(printer.Path);
            //PropertyCollection pcoll = printer.Properties;
            //try
            //{
            //    foreach (string sc in pcoll.PropertyNames)
            //    {

            //        printers.Add((pcoll[sc])[0].ToString());
            //    }
            //}
            //catch (COMException ex)
            //{

            //}
            //return printers;

            using (DirectoryEntry root = new DirectoryEntry("WinNT:"))
            {
                foreach (DirectoryEntry domain in root.Children)
                {
                    Console.WriteLine("Domain | WorkGroup:\t" + domain.Name);
                    foreach (DirectoryEntry computer in domain.Children)
                    {
                        if (computer.Name.CompareTo("HANDSGROUP025") == 0)
                        {
                            foreach (var fuck in computer.Children)
                            {

                            }
                        }

                    }
                    foreach (DirectoryEntry computer in domain.Properties)
                    {
                        Console.WriteLine("Computer:\t" + computer.Name);
                    }
                }
            }
            return null;
        }
        public static List<string> GetPrinterList()
        {
            List<string> printers = new List<string>();

            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                printers.Add(printer);
            }
            return printers;
        }

        public static List<string> GetPrintList()
        {
            PrinterSearcher p = new PrinterSearcher();
            PrinterSearcher.PrinterInfo[] printers = p.Search(PRINTER_ENUM.SHARED);

            List<string> printerNames = new List<string>();
            foreach (PrinterSearcher.PrinterInfo pi in printers)
            {
                printerNames.Add(pi.Name);
            }

            return printerNames;
        }
    }
}
