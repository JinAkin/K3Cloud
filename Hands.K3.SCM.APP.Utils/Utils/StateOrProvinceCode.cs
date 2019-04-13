using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Utils.Utils
{
    /// <summary>
    /// K3国家或省份编码与FedEX的对照表
    /// </summary>
    public class StateOrProvinceCode
    {
        public static string GetStateOrProvinceCode(string k3StateOrProvince)
        {
            if (!string.IsNullOrWhiteSpace(k3StateOrProvince))
            {
                ///Canada Province Codes
                if (k3StateOrProvince.ToUpper().Contains("Alberta".ToUpper()))
                {
                    return "AB";
                }
                if (k3StateOrProvince.ToUpper().Contains("British Columbia".ToUpper()))
                {
                    return "BC";
                }
                if (k3StateOrProvince.ToUpper().Contains("Manitoba".ToUpper()))
                {
                    return "MB";
                }
                if (k3StateOrProvince.ToUpper().Contains("Manitoba".ToUpper()))
                {
                    return "MB";
                }
                if (k3StateOrProvince.ToUpper().Contains("New Brunswick".ToUpper()))
                {
                    return "NB";
                }
                if (k3StateOrProvince.ToUpper().Contains("Newfoundland".ToUpper()))
                {
                    return "NT";
                }
                if (k3StateOrProvince.ToUpper().Contains("Nova Scotia".ToUpper()))
                {
                    return "NS";
                }
                if (k3StateOrProvince.ToUpper().Contains("Nunavut".ToUpper()))
                {
                    return "NU";
                }
                if (k3StateOrProvince.ToUpper().Contains("Nunavut".ToUpper()))
                {
                    return "ON";
                }
                if (k3StateOrProvince.ToUpper().Contains("Prince Edward Island".ToUpper()))
                {
                    return "PE";
                }
                if (k3StateOrProvince.ToUpper().Contains("Quebec".ToUpper()))
                {
                    return "QC";
                }
                if (k3StateOrProvince.ToUpper().Contains("Saskatchewan".ToUpper()))
                {
                    return "SK";
                }
                if (k3StateOrProvince.ToUpper().Contains("Yukon".ToUpper()))
                {
                    return "YT";
                }
                ///India State Codes
                if (k3StateOrProvince.ToUpper().Contains("Andaman & Nicobar (U.T)".ToUpper()))
                {
                    return "AN";
                }
                if (k3StateOrProvince.ToUpper().Contains("Andhra Pradesh".ToUpper()))
                {
                    return "AP";
                }
                if (k3StateOrProvince.ToUpper().Contains("Arunachal Pradesh".ToUpper()))
                {
                    return "AR";
                }
                if (k3StateOrProvince.ToUpper().Contains("Assam".ToUpper()))
                {
                    return "AS";
                }
                if (k3StateOrProvince.ToUpper().Contains("Bihar".ToUpper()))
                {
                    return "BR";
                }
                if (k3StateOrProvince.ToUpper().Contains("Chattisgarh".ToUpper()))
                {
                    return "CG";
                }
                if (k3StateOrProvince.ToUpper().Contains("Chandigarh (U.T.)".ToUpper()))
                {
                    return "CH";
                }
                if (k3StateOrProvince.ToUpper().Contains("Daman & Diu (U.T.)".ToUpper()))
                {
                    return "DD";
                }
                if (k3StateOrProvince.ToUpper().Contains("Delhi (U.T.)".ToUpper()))
                {
                    return "DL";
                }
                if (k3StateOrProvince.ToUpper().Contains("Dadra and Nagar Haveli (U.T.)".ToUpper()))
                {
                    return "DN";
                }
                if (k3StateOrProvince.ToUpper().Contains("Goa".ToUpper()))
                {
                    return "GA";
                }
                if (k3StateOrProvince.ToUpper().Contains("Gujarat".ToUpper()))
                {
                    return "GJ";
                }
                if (k3StateOrProvince.ToUpper().Contains("Himachal Pradesh".ToUpper()))
                {
                    return "HP";
                }
                if (k3StateOrProvince.ToUpper().Contains("Haryana".ToUpper()))
                {
                    return "HR";
                }
                if (k3StateOrProvince.ToUpper().Contains("Jharkhand".ToUpper()))
                {
                    return "JH";
                }
                if (k3StateOrProvince.ToUpper().Contains("Jammu & Kashmir".ToUpper()))
                {
                    return "JK";
                }
                if (k3StateOrProvince.ToUpper().Contains("Karnataka".ToUpper()))
                {
                    return "KA";
                }
                if (k3StateOrProvince.ToUpper().Contains("Kerala".ToUpper()))
                {
                    return "KL";
                }
                if (k3StateOrProvince.ToUpper().Contains("Lakshadweep (U.T)".ToUpper()))
                {
                    return "LD";
                }
                if (k3StateOrProvince.ToUpper().Contains("Maharashtra".ToUpper()))
                {
                    return "MH";
                }
                if (k3StateOrProvince.ToUpper().Contains("Meghalaya".ToUpper()))
                {
                    return "ML";
                }
                if (k3StateOrProvince.ToUpper().Contains("Manipur".ToUpper()))
                {
                    return "MN";
                }
                if (k3StateOrProvince.ToUpper().Contains("Madhya Pradesh".ToUpper()))
                {
                    return "MP";
                }
                if (k3StateOrProvince.ToUpper().Contains("Mizoram".ToUpper()))
                {
                    return "MZ";
                }
                if (k3StateOrProvince.ToUpper().Contains("Nagaland".ToUpper()))
                {
                    return "NL";
                }
                if (k3StateOrProvince.ToUpper().Contains("Orissa".ToUpper()))
                {
                    return "OR";
                }
                if (k3StateOrProvince.ToUpper().Contains("Punjab".ToUpper()))
                {
                    return "PB";
                }
                if (k3StateOrProvince.ToUpper().Contains("Puducherry (U.T.)".ToUpper()))
                {
                    return "PY";
                }
                if (k3StateOrProvince.ToUpper().Contains("Rajasthan".ToUpper()))
                {
                    return "RJ";
                }
                if (k3StateOrProvince.ToUpper().Contains("Sikkim".ToUpper()))
                {
                    return "SK";
                }
                if (k3StateOrProvince.ToUpper().Contains("Tamil Nadu".ToUpper()))
                {
                    return "TN";
                }
                if (k3StateOrProvince.ToUpper().Contains("Tripura".ToUpper()))
                {
                    return "TR";
                }
                if (k3StateOrProvince.ToUpper().Contains("Uttaranchal".ToUpper()))
                {
                    return "UA";
                }
                if (k3StateOrProvince.ToUpper().Contains("Uttar Pradesh".ToUpper()))
                {
                    return "UP";
                }
                if (k3StateOrProvince.ToUpper().Contains("West Bengal".ToUpper()))
                {
                    return "WB";
                }
                ///Mexico State Codes
                if (k3StateOrProvince.ToUpper().Contains("Aguascalientes".ToUpper()))
                {
                    return "AG";
                }
                if (k3StateOrProvince.ToUpper().Contains("Baja California".ToUpper()))
                {
                    return "BC";
                }
                if (k3StateOrProvince.ToUpper().Contains("Baja California Sur".ToUpper()))
                {
                    return "BS";
                }
                if (k3StateOrProvince.ToUpper().Contains("Campeche".ToUpper()))
                {
                    return "CM";
                }
                if (k3StateOrProvince.ToUpper().Contains("Chiapas".ToUpper()))
                {
                    return "CS";
                }
                if (k3StateOrProvince.ToUpper().Contains("Morelos".ToUpper()))
                {
                    return "MO";
                }
                if (k3StateOrProvince.ToUpper().Contains("Nayarit".ToUpper()))
                {
                    return "NA";
                }
                if (k3StateOrProvince.ToUpper().Contains("Nayarit".ToUpper()))
                {
                    return "MO";
                }
                if (k3StateOrProvince.ToUpper().Contains("Nuevo León".ToUpper()))
                {
                    return "NL";
                }
                if (k3StateOrProvince.ToUpper().Contains("Oaxaca".ToUpper()))
                {
                    return "OA";
                }
                if (k3StateOrProvince.ToUpper().Contains("Puebla".ToUpper()))
                {
                    return "PU";
                }
                if (k3StateOrProvince.ToUpper().Contains("Chihuahua".ToUpper()))
                {
                    return "CH";
                }
                if (k3StateOrProvince.ToUpper().Contains("Coahuila".ToUpper()))
                {
                    return "CO";
                }
                if (k3StateOrProvince.ToUpper().Contains("Colima".ToUpper()))
                {
                    return "CL";
                }
                if (k3StateOrProvince.ToUpper().Contains("Ciudad de México".ToUpper()))
                {
                    return "DF";
                }
                if (k3StateOrProvince.ToUpper().Contains("Durango".ToUpper()))
                {
                    return "DG";
                }
                if (k3StateOrProvince.ToUpper().Contains("Guanajuato".ToUpper()))
                {
                    return "GT";
                }
                if (k3StateOrProvince.ToUpper().Contains("Guerrero".ToUpper()))
                {
                    return "GR";
                }
                if (k3StateOrProvince.ToUpper().Contains("Jalisco".ToUpper()))
                {
                    return "JA";
                }
                if (k3StateOrProvince.ToUpper().Contains("Jalisco".ToUpper()))
                {
                    return "JA";
                }
                if (k3StateOrProvince.ToUpper().Contains("Estado de México".ToUpper()))
                {
                    return "EM";
                }
                if (k3StateOrProvince.ToUpper().Contains("Michoacán".ToUpper()))
                {
                    return "MI";
                }
                if (k3StateOrProvince.ToUpper().Contains("Querétaro".ToUpper()))
                {
                    return "QE";
                }
                if (k3StateOrProvince.ToUpper().Contains("Quintana Roo".ToUpper()))
                {
                    return "QR";
                }
                if (k3StateOrProvince.ToUpper().Contains("San Luis Potosí".ToUpper()))
                {
                    return "SL";
                }
                if (k3StateOrProvince.ToUpper().Contains("Sonora".ToUpper()))
                {
                    return "SO";
                }
                if (k3StateOrProvince.ToUpper().Contains("Tabasco".ToUpper()))
                {
                    return "TB";
                }
                if (k3StateOrProvince.ToUpper().Contains("Tamaulipas".ToUpper()))
                {
                    return "TM";
                }
                if (k3StateOrProvince.ToUpper().Contains("Tlaxcala".ToUpper()))
                {
                    return "TL";
                }
                if (k3StateOrProvince.ToUpper().Contains("Veracruz".ToUpper()))
                {
                    return "VE";
                }
                if (k3StateOrProvince.ToUpper().Contains("Yucatán".ToUpper()))
                {
                    return "YU";
                }
                if (k3StateOrProvince.ToUpper().Contains("Zacatecas".ToUpper()))
                {
                    return "ZA";
                }
                ///U.S. State Codes
                if (k3StateOrProvince.ToUpper().Contains("Alabama".ToUpper()))
                {
                    return "AL";
                }
                if (k3StateOrProvince.ToUpper().Contains("Alaska".ToUpper()))
                {
                    return "AK";
                }
                if (k3StateOrProvince.ToUpper().Contains("Alaska".ToUpper()))
                {
                    return "AK";
                }
                if (k3StateOrProvince.ToUpper().Contains("Arizona".ToUpper()))
                {
                    return "AZ";
                }
                if (k3StateOrProvince.ToUpper().Contains("Arkansas".ToUpper()))
                {
                    return "AR";
                }
                if (k3StateOrProvince.ToUpper().Contains("California".ToUpper()))
                {
                    return "CA";
                }
                if (k3StateOrProvince.ToUpper().Contains("Colorado".ToUpper()))
                {
                    return "CO";
                }
                if (k3StateOrProvince.ToUpper().Contains("Delaware".ToUpper()))
                {
                    return "CT";
                }
                if (k3StateOrProvince.ToUpper().Contains("Connecticut".ToUpper()))
                {
                    return "DE";
                }
                if (k3StateOrProvince.ToUpper().Contains("District of Columbia".ToUpper()))
                {
                    return "DC";
                }
                if (k3StateOrProvince.ToUpper().Contains("Florida".ToUpper()))
                {
                    return "FL";
                }
                if (k3StateOrProvince.ToUpper().Contains("Georgia".ToUpper()))
                {
                    return "GA";
                }
                if (k3StateOrProvince.ToUpper().Contains("Hawaii".ToUpper()))
                {
                    return "HI";
                }
                if (k3StateOrProvince.ToUpper().Contains("Idaho".ToUpper()))
                {
                    return "ID";
                }
                if (k3StateOrProvince.ToUpper().Contains("Illinois".ToUpper()))
                {
                    return "IL";
                }
                if (k3StateOrProvince.ToUpper().Contains("Indiana".ToUpper()))
                {
                    return "IN";
                }
                if (k3StateOrProvince.ToUpper().Contains("Iowa".ToUpper()))
                {
                    return "IA";
                }
                if (k3StateOrProvince.ToUpper().Contains("Kansas".ToUpper()))
                {
                    return "KS";
                }
                if (k3StateOrProvince.ToUpper().Contains("Kentucky".ToUpper()))
                {
                    return "KY";
                }
                if (k3StateOrProvince.ToUpper().Contains("Louisiana".ToUpper()))
                {
                    return "LA";
                }
                if (k3StateOrProvince.ToUpper().Contains("Maine".ToUpper()))
                {
                    return "ME";
                }
                if (k3StateOrProvince.ToUpper().Contains("Maryland".ToUpper()))
                {
                    return "MD";
                }
                if (k3StateOrProvince.ToUpper().Contains("Massachusetts".ToUpper()))
                {
                    return "MA";
                }
                if (k3StateOrProvince.ToUpper().Contains("Michigan".ToUpper()))
                {
                    return "MI";
                }
                if (k3StateOrProvince.ToUpper().Contains("Minnesota".ToUpper()))
                {
                    return "MN";
                }
                if (k3StateOrProvince.ToUpper().Contains("Mississippi".ToUpper()))
                {
                    return "MS";
                }
                if (k3StateOrProvince.ToUpper().Contains("Missouri".ToUpper()))
                {
                    return "MO";
                }
                if (k3StateOrProvince.ToUpper().Contains("Montana".ToUpper()))
                {
                    return "MT";
                }
                if (k3StateOrProvince.ToUpper().Contains("Nebraska".ToUpper()))
                {
                    return "NE";
                }
                if (k3StateOrProvince.ToUpper().Contains("Nevada".ToUpper()))
                {
                    return "NV";
                }
                if (k3StateOrProvince.ToUpper().Contains("New Hampshire".ToUpper()))
                {
                    return "NH";
                }
                if (k3StateOrProvince.ToUpper().Contains("New Jersey".ToUpper()))
                {
                    return "NJ";
                }
                if (k3StateOrProvince.ToUpper().Contains("New Mexico".ToUpper()))
                {
                    return "NM";
                }
                if (k3StateOrProvince.ToUpper().Contains("New York".ToUpper()))
                {
                    return "NY";
                }
                if (k3StateOrProvince.ToUpper().Contains("North Carolina".ToUpper()))
                {
                    return "NC";
                }
                if (k3StateOrProvince.ToUpper().Contains("North Dakota".ToUpper()))
                {
                    return "ND";
                }
                if (k3StateOrProvince.ToUpper().Contains("Ohio".ToUpper()))
                {
                    return "OH";
                }
                if (k3StateOrProvince.ToUpper().Contains("Oklahoma".ToUpper()))
                {
                    return "OK";
                }
                if (k3StateOrProvince.ToUpper().Contains("Oregon".ToUpper()))
                {
                    return "OR";
                }
                if (k3StateOrProvince.ToUpper().Contains("Pennsylvania".ToUpper()))
                {
                    return "PA";
                }
                if (k3StateOrProvince.ToUpper().Contains("Rhode Island".ToUpper()))
                {
                    return "RI";
                }
                if (k3StateOrProvince.ToUpper().Contains("South Carolina".ToUpper()))
                {
                    return "SC";
                }
                if (k3StateOrProvince.ToUpper().Contains("South Dakota".ToUpper()))
                {
                    return "SD";
                }
                if (k3StateOrProvince.ToUpper().Contains("Tennessee".ToUpper()))
                {
                    return "TN";
                }
                if (k3StateOrProvince.ToUpper().Contains("Texas".ToUpper()))
                {
                    return "TX";
                }
                if (k3StateOrProvince.ToUpper().Contains("Utah".ToUpper()))
                {
                    return "UT";
                }
                if (k3StateOrProvince.ToUpper().Contains("Vermont".ToUpper()))
                {
                    return "VT";
                }
                if (k3StateOrProvince.ToUpper().Contains("Virginia".ToUpper()))
                {
                    return "VA";
                }
                if (k3StateOrProvince.ToUpper().Contains("Washington State".ToUpper()))
                {
                    return "WA";
                }
                if (k3StateOrProvince.ToUpper().Contains("West Virginia".ToUpper()))
                {
                    return "WV";
                }
                if (k3StateOrProvince.ToUpper().Contains("Wisconsin".ToUpper()))
                {
                    return "WI";
                }
                if (k3StateOrProvince.ToUpper().Contains("Wyoming".ToUpper()))
                {
                    return "WY";
                }
                if (k3StateOrProvince.ToUpper().Contains("Puerto Rico".ToUpper()))
                {
                    return "PR";
                }
                ///United Arab Emirates (UAE) State Codes
                if (k3StateOrProvince.ToUpper().Contains("Abu Dhabi".ToUpper()))
                {
                    return "AB";
                }
                if (k3StateOrProvince.ToUpper().Contains("Ajman".ToUpper()))
                {
                    return "AJ";
                }
                if (k3StateOrProvince.ToUpper().Contains("Dubai".ToUpper()))
                {
                    return "DU";
                }
                if (k3StateOrProvince.ToUpper().Contains("Fujairah".ToUpper()))
                {
                    return "FU";
                }
                if (k3StateOrProvince.ToUpper().Contains("Ras al-Khaimah".ToUpper()))
                {
                    return "RA";
                }
                if (k3StateOrProvince.ToUpper().Contains("Sharjah".ToUpper()))
                {
                    return "SH";
                }
                if (k3StateOrProvince.ToUpper().Contains("Umm al-Qaiwain".ToUpper()))
                {
                    return "UM";
                }
                ///China Provinces and Regions
                if (k3StateOrProvince.ToUpper().Contains("Anhui".ToUpper()) || k3StateOrProvince.Contains("安徽"))
                {
                    return "Anhui";
                }
                if (k3StateOrProvince.ToUpper().Contains("Beijing".ToUpper()) || k3StateOrProvince.Contains("北京"))
                {
                    return "Beijing";
                }
                if (k3StateOrProvince.ToUpper().Contains("Chongqing".ToUpper()) || k3StateOrProvince.Contains("重庆"))
                {
                    return "Chongqing";
                }
                if (k3StateOrProvince.ToUpper().Contains("Fujian".ToUpper()) || k3StateOrProvince.Contains("福建"))
                {
                    return "Fujian";
                }
                if (k3StateOrProvince.ToUpper().Contains("Gansu".ToUpper()) || k3StateOrProvince.Contains("甘肃"))
                {
                    return "Gansu";
                }
                if (k3StateOrProvince.ToUpper().Contains("Guangdong".ToUpper()) || k3StateOrProvince.Contains("广东"))
                {
                    return "Guangdong";
                }
                if (k3StateOrProvince.ToUpper().Contains("Hainan".ToUpper()) || k3StateOrProvince.Contains("海南"))
                {
                    return "Hainan";
                }
                if (k3StateOrProvince.ToUpper().Contains("Hebei".ToUpper()) || k3StateOrProvince.Contains("河北"))
                {
                    return "Hebei";
                }
                if (k3StateOrProvince.ToUpper().Contains("Heilongjiang".ToUpper()) || k3StateOrProvince.Contains("黑龙江"))
                {
                    return "Heilongjiang";
                }
                if (k3StateOrProvince.ToUpper().Contains("Henan".ToUpper()) || k3StateOrProvince.Contains("河南"))
                {
                    return "Henan";
                }
                if (k3StateOrProvince.ToUpper().Contains("Hubei".ToUpper()) || k3StateOrProvince.Contains("湖北"))
                {
                    return "Hubei";
                }
                if (k3StateOrProvince.ToUpper().Contains("Hunan".ToUpper()) || k3StateOrProvince.Contains("湖南"))
                {
                    return "Hunan";
                }
                if (k3StateOrProvince.ToUpper().Contains("Jiangsu".ToUpper()) || k3StateOrProvince.Contains("江苏"))
                {
                    return "Jiangsu";
                }
                if (k3StateOrProvince.ToUpper().Contains("Jiangxi".ToUpper()) || k3StateOrProvince.Contains("江西"))
                {
                    return "Jiangxi";
                }
                if (k3StateOrProvince.ToUpper().Contains("Jilin".ToUpper()) || k3StateOrProvince.Contains("吉林"))
                {
                    return "Jilin";
                }
                if (k3StateOrProvince.ToUpper().Contains("Liaoning".ToUpper()) || k3StateOrProvince.Contains("辽宁"))
                {
                    return "Liaoning";
                }
                if (k3StateOrProvince.ToUpper().Contains("Nei Mongol".ToUpper()) || k3StateOrProvince.Contains("内蒙古"))
                {
                    return "Nei Mongol";
                }
                if (k3StateOrProvince.ToUpper().Contains("Qinghai".ToUpper()) || k3StateOrProvince.Contains("青海"))
                {
                    return "Qinghai";
                }
                if (k3StateOrProvince.ToUpper().Contains("Shaanxi".ToUpper()) || k3StateOrProvince.Contains("陕西"))
                {
                    return "Shaanxi";
                }
                if (k3StateOrProvince.ToUpper().Contains("Shandong".ToUpper()) || k3StateOrProvince.Contains("山东"))
                {
                    return "Shandong";
                }
                if (k3StateOrProvince.ToUpper().Contains("Shanghai".ToUpper()) || k3StateOrProvince.Contains("上海"))
                {
                    return "Shanghai";
                }
                if (k3StateOrProvince.ToUpper().Contains("Shanxi".ToUpper()) || k3StateOrProvince.Contains("山西"))
                {
                    return "Shanxi";
                }
                if (k3StateOrProvince.ToUpper().Contains("Sichuan".ToUpper()) || k3StateOrProvince.Contains("四川"))
                {
                    return "Sichuan";
                }
                if (k3StateOrProvince.ToUpper().Contains("Tianjin".ToUpper()) || k3StateOrProvince.Contains("天津"))
                {
                    return "Tianjin";
                }
                if (k3StateOrProvince.ToUpper().Contains("Xinjiang".ToUpper()) || k3StateOrProvince.Contains("新疆"))
                {
                    return "Xinjiang";
                }
                if (k3StateOrProvince.ToUpper().Contains("Yunnan".ToUpper()) || k3StateOrProvince.Contains("云南"))
                {
                    return "Yunnan";
                }
                if (k3StateOrProvince.ToUpper().Contains("Zhejiang".ToUpper()) || k3StateOrProvince.Contains("浙江"))
                {
                    return "Zhejiang";
                }
            }

            return k3StateOrProvince;
        }
    }
}
