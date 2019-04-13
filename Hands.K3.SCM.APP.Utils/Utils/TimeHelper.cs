using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Utils.Utils
{
    public class TimeHelper
    {
        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public static string GetTimeStamp(System.DateTime time, int length = 10)
        {
            long ts = ConvertDateTimeToInt(time);
            return ts.ToString().Substring(0, length);
        }
        /// <summary>  
        /// 将c# DateTime时间格式转换为Unix时间戳格式  
        /// </summary>  
        /// <param name="time">时间</param>  
        /// <returns>long</returns>  
        public static long ConvertDateTimeToInt(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            long t = (time.Ticks - startTime.Ticks) / 10000;   //除10000调整为13位      
            return t;
        }
        /// <summary>        
        /// 时间戳转为C#格式时间        
        /// </summary>        
        /// <param name=”timeStamp”></param>        
        /// <returns></returns>        
        public static DateTime ConvertStringToDateTime(string timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000");
            TimeSpan toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);
        }

        /// <summary>
        /// 时间戳转为C#格式时间10位
        /// </summary>
        /// <param name="timeStamp">Unix时间戳格式</param>
        /// <returns>C#格式时间</returns>
        public static DateTime GetDateTimeFrom1970Ticks(long curSeconds)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            return dtStart.AddSeconds(curSeconds);
        }

        /// <summary>
        /// 验证时间戳
        /// </summary>
        /// <param name="time"></param>
        /// <param name="interval">差值（分钟）</param>
        /// <returns></returns>
        public static bool IsTime(long time, double interval)
        {
            DateTime dt = GetDateTimeFrom1970Ticks(time);
            //取现在时间
            DateTime dt1 = DateTime.Now.AddMinutes(interval);
            DateTime dt2 = DateTime.Now.AddMinutes(interval * -1);
            if (dt > dt2 && dt < dt1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 判断时间戳是否正确（验证前8位）
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static bool IsTime(string time)
        {
            string str = GetTimeStamp(DateTime.Now, 8);
            if (str.Equals(time))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 转换为当地时间
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static DateTime ConvertToLocalTime(DateTime dt)
        {
            DateTime localTime = default(DateTime);
            DateTime utcTime = TimeZoneInfo.ConvertTimeToUtc(dt);
            if (utcTime != null)
            {
                localTime = dt.ToUniversalTime().AddHours(8);
            }
            return localTime;
        }

        /// <summary>  
        /// 本地时间转成GMT时间  
        /// </summary>  
        public static string ToGMTString(DateTime dt)
        {
            return dt.ToUniversalTime().ToString("r");
        }

        /// <summary>  
        /// 本地时间转成GMT格式的时间  
        /// </summary>  
        public static string ToGMTFormat(DateTime dt)
        {
            return dt.ToString("r") + dt.ToString("zzz").Replace(":", "");
        }

        /// <summary>  
        /// GMT时间转成本地时间  
        /// </summary>  
        /// <param name="gmt">字符串形式的GMT时间</param>  
        /// <returns></returns>  
        public static DateTime GMT2Local(DateTime dt)
        {
            dt = DateTime.UtcNow;
            string gmt = ToGMTFormat(dt);
            try
            {
                string pattern = "";
                if (gmt.IndexOf("+0") != -1)
                {
                    gmt = gmt.Replace("GMT", "");
                    pattern = "ddd, dd MMM yyyy HH':'mm':'ss zzz";
                }
                if (gmt.ToUpper().IndexOf("GMT") != -1)
                {
                    pattern = "ddd, dd MMM yyyy HH':'mm':'ss 'GMT'";
                }
                if (pattern != "")
                {
                    dt = DateTime.ParseExact(gmt, pattern, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AdjustToUniversal);
                    dt = dt.ToLocalTime();
                }
                else
                {
                    dt = Convert.ToDateTime(gmt);
                }
            }
            catch
            {
            }
            return dt;
        }

        /// <summary>  
        /// 获取当前本地时间戳  
        /// </summary>  
        /// <returns></returns>        
        public long GetCurrentTimeUnix(DateTime dt)
        {
            if (dt != default(DateTime))
            {
                TimeSpan cha = (dt - TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)));
                long t = (long)cha.TotalSeconds;
                return t;
            }
            return default(long);
        }
        /// <summary>  
        /// 时间戳转换为本地时间对象  
        /// </summary>  
        /// <returns></returns>        
        public DateTime GetUnixDateTime(DateTime dt)
        {
            long unix = GetCurrentTimeUnix(dt);
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            DateTime newTime = dtStart.AddSeconds(unix);
            return newTime;
        }

        /// <summary>
        /// 获取本地时间
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static DateTime ToLocalTime(DateTime dt)
        {
            string zoneId = "CENTRAL TIME";
            TimeZoneInfo zoneInfo = null;
            DateTime utcTime = DateTime.UtcNow;
            try
            {
                zoneInfo = TimeZoneInfo.FindSystemTimeZoneById(zoneId);
                utcTime = TimeZoneInfo.ConvertTimeToUtc(dt, zoneInfo);//UTC(协调世界时)
            }
            catch (TimeZoneNotFoundException)
            {
                Console.WriteLine("Unable to find the {0} zone in the registry.", zoneId);
            }
            catch (InvalidTimeZoneException)
            {
                Console.WriteLine("Registry data on the {0} zone has been corrupted.", zoneId);
            }
            return utcTime.ToLocalTime();    //将UTC时间转为本地时间
        }
        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static long GetTimeStampLong(DateTime date)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (long)(date - startTime).TotalMilliseconds; // 相差秒数
        }

    }
}
