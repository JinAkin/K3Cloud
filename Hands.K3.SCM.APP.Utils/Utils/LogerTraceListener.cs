

using HS.K3.Common.Abbott;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Utils.Utils
{
    class LogerTraceListener : TraceListener
    {
        /// <summary>
        /// FileName
        /// </summary>
        private string m_fileName;

        private static LogerTraceListener tracer;
        private static readonly object locker = new object();

        /// <summary>
        /// Constructor
        /// </summary>
        private LogerTraceListener(SynchroDataType dataType, string billNo)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory + "\\SynData\\" + GetBasePath(dataType);
            if (!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);
            this.m_fileName = basePath +
                string.Format("{0}.txt", billNo);
        }
        private LogerTraceListener()
        {
            
        }
        public static LogerTraceListener CreateInstance(SynchroDataType dataType, string billNo)
        {
            if (tracer == null)
            {
                lock (locker)
                {
                    if (tracer == null)
                    {
                        return new LogerTraceListener(dataType, billNo);
                    }
                }
            }
            return tracer;
        }
        public static LogerTraceListener CreateInstance()
        {
            if (tracer == null)
            {
                lock (locker)
                {
                    if (tracer == null)
                    {
                        return new LogerTraceListener();
                    }
                }
            }
            return tracer;
        }
        /// <summary>
        /// Write
        /// </summary>
        public override void Write(string message)
        {
            message = Format(message, "");
            File.AppendAllText(m_fileName, message);
        }

        /// <summary>
        /// Write
        /// </summary>
        public override void Write(object obj)
        {
            string message = Format(obj, "");
            File.AppendAllText(m_fileName, message);
        }

        /// <summary>
        /// WriteLine
        /// </summary>
        public override void WriteLine(object obj)
        {
            string message = Format(obj, "");
            File.AppendAllText(m_fileName, message);
        }

        /// <summary>
        /// WriteLine
        /// </summary>
        public override void WriteLine(string message)
        {
            message = Format(message, "");
            File.AppendAllText(m_fileName, message);
        }

        /// <summary>
        /// WriteLine
        /// </summary>
        public override void WriteLine(object obj, string category)
        {
            string message = Format(obj, category);
            File.AppendAllText(m_fileName, message);
        }

        /// <summary>
        /// WriteLine
        /// </summary>
        public override void WriteLine(string message, string category)
        {
            message = Format(message, category);
            File.AppendAllText(m_fileName, message);
        }

        /// <summary>
        /// Format
        /// </summary>
        private string Format(object obj, string category)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("{0} ", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            if (!string.IsNullOrEmpty(category))
                builder.AppendFormat("[{0}] ", category);
            if (obj is Exception)
            {
                var ex = (Exception)obj;
                builder.Append(ex.Message + "\r\n");
                builder.Append(ex.StackTrace + "\r\n");
            }
            else
            {
                builder.Append(obj.ToString() + "\r\n");
            }

            return builder.ToString();
        }

        private string GetBasePath(SynchroDataType dataType)
        {

            switch (dataType)
            {
                case SynchroDataType.SaleOrder:
                    return "SalOrder\\";
                case SynchroDataType.SaleOrderStatus:
                    return "SaleOrderStatus\\";
                case SynchroDataType.Customer:
                    return "Customer\\";
                case SynchroDataType.CustomerAddress:
                    return "CustomerAddress\\";
                case SynchroDataType.DelCustomerAddress:
                    return "DelCustomerAddress\\";
                
            }
            return null;
        }

    }
}
