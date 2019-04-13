using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Utils.Utils
{
    public class FormatUtils
    {
        /// <summary>
        /// 格式化单据编码
        /// </summary>
        /// <param name="numbers"></param>
        /// <param name="useType"></param>
        /// <returns></returns>
        public static string FormatNumber(List<string> numbers,UseType useType)
        {
            string FNumber = "";
            if (numbers != null && numbers.Count > 0)
            {
                for (int i = 0; i < numbers.Count; i++)
                {
                    if (useType == UseType.SQL)
                    {
                        if (i < numbers.Count - 1)
                        {
                            FNumber += "\'" + numbers[i] + "\',";
                        }
                        else if (i == numbers.Count - 1)
                        {
                            FNumber += "\'" + numbers[i] + "\'";
                        }
                    }
                    else if (useType == UseType.Log)
                    {
                        if (i < numbers.Count - 1)
                        {
                            FNumber += numbers[i] + ",";
                        }
                        else if (i == numbers.Count - 1)
                        {
                            FNumber += numbers[i];
                        }
                    }
                    
                }
                return FNumber;
            }

            return null;
        }
    }

    /// <summary>
    /// 应用类型
    /// </summary>
    public enum UseType
    {
        /// <summary>
        /// SQL查询
        /// </summary>
        SQL,
        /// <summary>
        /// 日志记录
        /// </summary>
        Log
    }
}



