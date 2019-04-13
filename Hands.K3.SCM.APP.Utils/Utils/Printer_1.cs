using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Utils.Utils
{
    public class Printer_1
    {
        //static void Main(string[] args)

        //{

        //    Console.WriteLine("请输入你想搜索类型的序号:");

        //    Console.WriteLine("1. Default printer(only Win95,Win98,WinME)\n2. Enumerates the locally installed printers;\n3. Enumerates the list of printers to which the user has made previous connections;\n4. Enumerates the printer identified by Name.;\n5. Enumerates network printers and print servers in the computer's domain;\n6. Enumerates printers that have the shared attribute;\n7. Enumerates network printers in the computer's domain;\n======================================================================");



        //    int pt = 0;

        //    try

        //    {

        //        pt = Int32.Parse(Console.ReadLine());

        //    }

        //    catch (Exception e)

        //    {

        //        Console.WriteLine("错误信息: {0}", e.Message);

        //        return;

        //    }

        //    PRINTER_ENUM printerKind = GetPrinterConType(pt);

        //    PrinterSearcher p = new PrinterSearcher();

        //    PrinterSearcher.PrinterInfo[] printers = p.Search(printerKind);

        //    foreach (PrinterSearcher.PrinterInfo pi in printers)

        //    {

        //        Console.WriteLine("=====================================\n打印机名: {0}\n描叙:: {1}\n注释: {2}\n=====================================\n",

        //        pi.Name, pi.Description, pi.Comment);

        //    }

        //}

        static PRINTER_ENUM GetPrinterConType(int ins)

        {

            switch (ins)

            {

                case 1:

                    return PRINTER_ENUM.DEFAULT;

                case 2:

                    return PRINTER_ENUM.LOCAL;

                case 3:

                    return PRINTER_ENUM.CONNECTIONS;

                case 4:

                    return PRINTER_ENUM.NAME;

                case 5:

                    return PRINTER_ENUM.REMOTE;

                case 6:

                    return PRINTER_ENUM.SHARED;

                case 7:

                    return PRINTER_ENUM.NETWORK;

                default:

                    return PRINTER_ENUM.LOCAL;

            }

        }

    }

    #region 打印机位置状态枚举 PRINTER_ENUM

    public enum PRINTER_ENUM

    {

        DEFAULT = 0x01,

        LOCAL = 0x02,

        CONNECTIONS = 0x04,

        NAME = 0x08,

        REMOTE = 0x10,

        SHARED = 0x20,

        NETWORK = 0x40

    }

    #endregion

    #region 异常派生 EnumPrinterException

    [Serializable]

    public class EnumPrinterException : ApplicationException

    {

        public EnumPrinterException() { }

        public EnumPrinterException(string message) : base(message) { }

        public EnumPrinterException(string message, Exception inner) :

        base(message, inner)
        { }

        protected EnumPrinterException(SerializationInfo info,

        StreamingContext context) : base(info, context)

        { }

    }

    #endregion

    //加上这个属性可以按导出到非托管对像的顺序排序

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]

    public class PRINTER_INFO_1

    {

        public int flags;

        public IntPtr pDescription;

        public IntPtr pName;

        public IntPtr pComment;

    }

    public class PrinterSearcher

    {

        #region Search

        public PrinterInfo[] Search(PRINTER_ENUM printerKind)

        {

            PrinterInfo[] pInfo = new PrinterInfo[0];

            uint iNeeded = 0, iReturned = 0, iSize = 0;

            IntPtr printers = IntPtr.Zero;



            if (!EnumPrinters(printerKind, String.Empty, 1, printers, 0, ref iNeeded, ref iReturned))

            {

                //返回由上一个非托管函数返回的错误代码，该函数是使用设置了 -

                //DllImport属性中SetLastError=true 标志的平台调用来调用的

                int err = Marshal.GetLastWin32Error();

                if (err != Win32Error.ERROR_INSUFFICIENT_BUFFER)

                    ThrowEnumPrinterException();

            }

            iSize = iNeeded;

            if (iNeeded != 0)

            {

                try

                {

                    //使用AllocHGlobal分配非托管内块

                    printers = Marshal.AllocHGlobal(new IntPtr(iSize));



                    //如果调用不成功抛出异常

                    if (!EnumPrinters(printerKind, String.Empty, 1, printers, iSize, ref iNeeded, ref iReturned))

                    {

                        ThrowEnumPrinterException();

                    }

                    else

                    {

                        pInfo = GetPrinterInfoFromMemory(printers, iReturned);

                    }

                }

                finally

                {

                    //释放分配的内存块

                    if (printers != IntPtr.Zero)

                        Marshal.FreeHGlobal(printers);

                }

            }

            return pInfo;

        }

        #endregion

        #region PrinterInfo

        public struct PrinterInfo

        {

            public string Name;

            public string Description;

            public string Comment;

        }



        public sealed class Win32Error

        {

            private Win32Error() { }

            public const int ERROR_INSUFFICIENT_BUFFER = 122;

        }

        #endregion

        #region EnumPrinters 

        [DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Auto)]

        [return: MarshalAs(UnmanagedType.Bool)]

        private static extern bool EnumPrinters([MarshalAs(UnmanagedType.U4)] PRINTER_ENUM flags,

      [MarshalAs(UnmanagedType.LPStr)] string sName,

      uint iLevel,

      IntPtr pPrinterDesc,

      uint iSize,

      [MarshalAs(UnmanagedType.U4)] ref uint iNeeded,

      [MarshalAs(UnmanagedType.U4)] ref uint iReturned

      );

        #endregion

        #region GetPrinterInfoFromMemory

        private PrinterInfo[] GetPrinterInfoFromMemory(IntPtr prInfo, uint numPrinters)

        {

            PRINTER_INFO_1 pi = new PRINTER_INFO_1();



            PrinterInfo[] pInfo = new PrinterInfo[numPrinters];

            for (int i = 0; i < numPrinters; i++)

            {

                //把数据从非托管内存传送到到托管内存

                Marshal.PtrToStructure(prInfo, pi);



                pInfo[i].Name = Marshal.PtrToStringAuto(pi.pName);

                pInfo[i].Description = Marshal.PtrToStringAuto(pi.pDescription);

                pInfo[i].Comment = Marshal.PtrToStringAuto(pi.pComment);

                prInfo = new IntPtr(prInfo.ToInt32() + Marshal.SizeOf(typeof(PRINTER_INFO_1)));

            }

            return pInfo;

        }

        private void ThrowEnumPrinterException()

        {

            throw new EnumPrinterException(string.Format("LastErrorCode: {0}",

            Marshal.GetLastWin32Error()));

        }

        #endregion

    }
}
