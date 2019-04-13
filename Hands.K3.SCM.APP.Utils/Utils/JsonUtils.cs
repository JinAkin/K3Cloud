
using Hands.K3.SCM.APP.Utils.Utils;
using HS.K3.Common.Abbott;
using Kingdee.BOS;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hands.K3.SCM.APP.Utils.Utils
{
    public class JsonUtils
    {
        /// <summary>
        /// 根据JObject的属性名获取相应的值
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetFieldValue(JObject obj, string fieldName)
        {
            if (!string.IsNullOrWhiteSpace(fieldName))
            {
                if (obj != null)
                {
                    if (obj.Property(fieldName) != null)
                    {
                        return ConvertObjectToString(obj[fieldName]).Trim();
                    }
                }
            }
            return default(string);
        }

        /// <summary>
        /// 根据JToken的属性名获取相应的值
        /// </summary>
        /// <param name="jtoken"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetFieldValue(JToken jtoken, string fileName)
        {
            JObject jObj = jtoken as JObject;

            if (!string.IsNullOrWhiteSpace(fileName))
            {
                if (jObj.Property(fileName) != null)
                {
                    return ConvertObjectToString(jObj[fileName]);
                }
            }
            return default(string);
        }

        /// <summary>
        /// 解析JArray数组元素为字符串（主要针对SQL查询出来的值）
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ConvertObjectToString(object obj)
        {
            return obj == null ? default(string) : obj.ToString().Trim();
        }

        public static string DealNullString(object obj)
        {
            return obj == null ? "" : obj.ToString();
        }

        /// <summary>
        /// 解析json字符串
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="json"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public static JObject ParseJson2JObj(Context ctx, SynchroDataType dataType, string json)
        {
            JArray jArr = null;
            JObject jObj = null;

            try
            {
                if (!string.IsNullOrEmpty(json))
                {
                    jArr = JArray.Parse(json);

                    if (jArr != null && jArr.Count > 0)
                    {
                        return JObject.Parse(jArr[0].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                LogUtils.WriteSynchroLog(ctx, dataType, dataType + json + "json解析出现异常：" + System.Environment.NewLine + ex.Message);
                return null;
            }
            return jObj;
        }

        public static T ParseJson<T>(Context ctx, SynchroDataType dataType, string json)
        {

            if (!string.IsNullOrWhiteSpace(json))
            {
                if (json.Substring(0, 1).CompareTo("[") == 0 && json.Substring(json.Length - 1, 1).CompareTo("]") == 0 && json.Length > 2)
                {
                    JArray jArr = JArray.Parse(json);
                    return (T)(Object)jArr;
                }
                if (json.Substring(0, 1).CompareTo("{") == 0 && json.Substring(json.Length - 1, 1).CompareTo("}") == 0 && json.Length > 2)
                {
                    JObject jObj = JObject.Parse(json);
                    return (T)(Object)jObj;
                }
            }
            else
            {
                LogUtils.WriteSynchroLog(ctx, dataType, "JSON为空！");
            }

            return default(T);
        }

        /// <summary>
        /// html字符替换或转义
        /// </summary>
        /// <param name="srcStr"></param>
        /// <returns></returns>
        public static string HtmlESC(string srcStr)
        {
            if (!string.IsNullOrWhiteSpace(srcStr))
            {
                if (srcStr.Contains("<table>") && srcStr.Contains("</table>") && srcStr.Contains("<tr>")
                    && srcStr.Contains("</tr>") && srcStr.Contains("<td>") && srcStr.Contains("</td>"))

                {
                    srcStr = srcStr.Replace("<table>", "")
                                 .Replace("</table>", "")
                                 .Replace("<tr>", "")
                                 .Replace("</tr>", "")
                                 .Replace("<td>", "")
                                 .Replace("</td>", "");
                }
                else if (srcStr.Contains("<table>") && srcStr.Contains("</table>") && srcStr.Contains("<tr>")
                    && srcStr.Contains("</tr>") && srcStr.Contains("<td>") && srcStr.Contains("</td>")
                    && srcStr.Contains("<br>"))
                {
                    srcStr = srcStr.Replace("<table>", "")
                                .Replace("</table>", "")
                                .Replace("<tr>", "")
                                .Replace("</tr>", "")
                                .Replace("<td>", "")
                                .Replace("</td>", "")
                                .Replace("<br>", System.Environment.NewLine);
                }
                else if (srcStr.Contains("<br>"))
                {
                    srcStr = srcStr.Replace("<br>", System.Environment.NewLine);
                }
                else if (srcStr.Contains("X"))
                {
                    srcStr = srcStr.Replace("X", "");
                }
                else if (srcStr.Contains("x"))
                {
                    srcStr = srcStr.Replace("x", "");
                }
                else if (srcStr.Contains("&nbsp;"))
                {
                    srcStr = srcStr.Replace("&nbsp;", " ");
                }
                else if (srcStr.Contains("&lt;"))
                {
                    srcStr = srcStr.Replace("&lt;", "<");
                }
                else if (srcStr.Contains("&gt;"))
                {
                    srcStr = srcStr.Replace("&gt;", ">");
                }
                else if (srcStr.Contains("&amp;"))
                {
                    srcStr = srcStr.Replace("&amp;", "&");
                }
                else if (srcStr.Contains("&quot;"))
                {
                    srcStr = srcStr.Replace("&quot;", "\"");
                }
                else if (srcStr.Contains("&apos;"))
                {
                    srcStr = srcStr.Replace("&apos;", "\'");
                }
                else if (srcStr.Contains("&cent;"))
                {
                    srcStr = srcStr.Replace("&cent;", "￠");
                }
                else if (srcStr.Contains("&pound;"))
                {
                    srcStr = srcStr.Replace("&pound;", "£");
                }
                else if (srcStr.Contains("&yen;"))
                {
                    srcStr = srcStr.Replace("&yen;", "¥");
                }
                else if (srcStr.Contains("&euro;"))
                {
                    srcStr = srcStr.Replace("&euro;", "€");
                }
                else if (srcStr.Contains("&sect;"))
                {
                    srcStr = srcStr.Replace("&sect;", "§");
                }
                else if (srcStr.Contains("&copy;"))
                {
                    srcStr = srcStr.Replace("&copy;", "©");
                }
                else if (srcStr.Contains("&reg;"))
                {
                    srcStr = srcStr.Replace("&reg;", "®");
                }
                else if (srcStr.Contains("&trade;"))
                {
                    srcStr = srcStr.Replace("&trade;", "™");
                }
                else if (srcStr.Contains("&times;"))
                {
                    srcStr = srcStr.Replace("&times;", "×");
                }
                else if (srcStr.Contains("&divide;"))
                {
                    srcStr = srcStr.Replace("&divide;", "÷");
                }
                else if (srcStr.Contains("'"))
                {
                    srcStr = srcStr.Replace("'", "''");
                }
                else if (srcStr.Contains("\""))
                {
                    srcStr = srcStr.Replace("\"", "");
                }

            }

            return srcStr;
        }

        /// <summary>
        /// 比较字符串是否相等
        /// </summary>
        /// <param name="oStr"></param>
        /// <param name="cStr"></param>
        /// <returns></returns>
        public static bool CompareString(string oStr, string cStr)
        {
            if (oStr != default(string) && cStr != default(string))
            {
                if (oStr.ToUpper().CompareTo(cStr.ToUpper()) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 替换双引号
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static string ReplaceDoubleQuotes(string json)
        {
            if (!string.IsNullOrWhiteSpace(json))
            {
                if (json.Contains("\"") && !json.Contains("\\\""))
                {
                    json = json.Replace("\"", "\\\"");
                    return json;
                }
            }
            return null;
        }

        /// <summary>
        /// 对想序列化为字符
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string SerializeObject<T>(Context ctx, T t)
        {
            try
            {
                if (t != null)
                {
                    return JsonConvert.SerializeObject(t);
                }
            }
            catch (Exception ex)
            {
                LogUtils.WriteSynchroLog(ctx, SynchroDataType.None, "序列化对象发生异常:" + ex.Message + Environment.NewLine + ex.StackTrace);
            }

            return null;
        }

        public static Dictionary<string, string> GetItems(string txt, char separator)
        {
            Dictionary<string, string> dicStr = null;

            if (!string.IsNullOrWhiteSpace(txt) && !char.IsWhiteSpace(separator))
            {
                if (txt.Contains(separator.ToString()))
                {
                    string[] arrTxt = txt.Split(separator);

                    if (arrTxt != null && arrTxt.Length > 0)
                    {
                        dicStr = new Dictionary<string, string>();

                        for (int i = 0; i < arrTxt.Length; i++)
                        {
                            if (!string.IsNullOrWhiteSpace(arrTxt[i]))
                            {
                                if (arrTxt.Length == 13)
                                {
                                    if (i == 0)
                                    {
                                        dicStr.Add(i.ToString(), arrTxt[i]);
                                    }
                                    if (i == 1)
                                    {
                                        dicStr.Add("USL0", arrTxt[i]);
                                    }
                                    if (i == 2)
                                    {
                                        dicStr.Add("AUL0", arrTxt[i]);
                                    }
                                    if (i == 3)
                                    {
                                        dicStr.Add("JPL0", arrTxt[i]);
                                    }
                                    if (i == 4)
                                    {
                                        dicStr.Add("KRL0", arrTxt[i]);
                                    }
                                    if (i > 4)
                                    {
                                        dicStr.Add((i - 4).ToString(), arrTxt[i]);
                                    }
                                }
                                else
                                {
                                    dicStr.Add(i.ToString(), arrTxt[i]);
                                }
                            }
                        }
                    }
                }
            }

            return dicStr;
        }


        public static bool IsChoice(HashSet<string> choices, string level)
        {
            if (choices != null && choices.Count > 0)
            {
                var choice = from c in choices
                             where c != null && c.CompareTo(level) == 0
                             select c;

                if (choice != null && choice.Count() > 0)
                {
                    return true;
                }
            }

            return false;
        }

    }
}
