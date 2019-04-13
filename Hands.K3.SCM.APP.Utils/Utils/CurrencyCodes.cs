using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Utils.Utils
{
    /// <summary>
    /// K3货币编码与FedEx货币编码对照表
    /// </summary>
    public class CurrencyCodes
    {
        public static string GetCurrCodes(string k3CurrCode)
        {
            if (!string.IsNullOrWhiteSpace(k3CurrCode))
            {
                switch (k3CurrCode)
                {
                    case "JPY":
                        return "JYE";
                    case "GBP":
                        return "UKL";
                    case "SGD":
                        return "SID";
                    case "TWD":
                        return "NTD";
                }

                return k3CurrCode;
            }

            return null;
        }
    }
}
