
using HS.K3.Common.Abbott;
using System;

namespace Hands.K3.SCM.APP.Entity.CommonObject
{
    /// <summary>
    /// 同步日志
    /// </summary>
    public class SynchroLog
    {
        public SynchroLog()
        {
            ErrInfor = "";
        }

        public long logId
        {
            get;
            set;
        }


        public string GroupId
        {
            get;
            set;
        }


        /// <summary>
        /// 源单类型
        /// </summary>
        public SynchroDataType FDataSourceType { get; set; }

        /// <summary>
        /// 原始数据来源类型：K3cloud
        public string FDataSourceTypeDesc { get; set; }


        /// <summary>
        /// K3CloudId
        /// </summary>
        public string K3CloudId { get; set; }

        /// <summary>
        /// K3CloudId
        /// </summary>
        public string K3BillNo { get; set; }


        /// <summary>
        /// 源单id
        /// </summary>
        public string sourceId { get; set; }

        /// <summary>
        /// 源单编码（编号）
        /// </summary>
        public string sourceNo { get; set; }

        public DateTime BeginTime
        {
            get;
            set;
        }

        public int IsSuccess
        {
            get;
            set;
        }

        string err = "";
        /// <summary>
        /// 提示信息
        /// </summary>
        public string ErrInfor
        {
            get
            {
                if (err.Length > 1999)
                {
                    return err.Substring(0, 1999);
                }

                return err;
            }
            set
            {
                err = value;
            }
        }


        /// <summary>
        /// 对应的文件名称(全名称，数据源为文件式的时候记录)
        /// </summary>
        public string FileName
        {
            get;
            set;
        }

        public string FOperateId { get; set;}

    }





}
