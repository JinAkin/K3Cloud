using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Hands.K3.SCM.APP.Entity.K3WebApi
{
    public class HttpClient_
    {
        /// <summary>
        /// Seivice URL
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }

        public bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {  // 总是接受  
            return true;
        }

        /// <summary>
        /// POST数据
        /// </summary>
        public string PostData()
        {
            if(Url.StartsWith("https",StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(CheckValidationResult);
            }
            HttpWebRequest httpRequest = HttpWebRequest.Create(Url) as HttpWebRequest;
            httpRequest.Method = "POST";
            httpRequest.ContentType = "application/x-www-form-urlencoded";
            httpRequest.Timeout = 1000 * 60 ;//1min

            using (Stream reqStream = httpRequest.GetRequestStream())
            {
                var bytes = UnicodeEncoding.UTF8.GetBytes(Content);
                reqStream.Write(bytes, 0, bytes.Length);
                reqStream.Flush();
            }

            using (var repStream = httpRequest.GetResponse().GetResponseStream())
            {
                using (var reader = new StreamReader(repStream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// GET数据
        /// </summary>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public string GetData(IDictionary<string, string> parameters)
        {
            if (!(parameters == null || parameters.Count == 0))
            {
                StringBuilder buffer = new StringBuilder();
                int i = 0;
                foreach (string key in parameters.Keys)
                {
                    if (i > 0)
                    {
                        buffer.AppendFormat("&{0}={1}", key, parameters[key]);
                    }
                    else
                    {
                        buffer.AppendFormat("{0}={1}", key, parameters[key]);
                        i++;
                    }
                }

                if (Url.Contains("?"))   //是否已经有其它参数
                {
                    Url = string.Concat(Url,"&", buffer);
                }
                else
                {
                    Url = string.Concat(Url, "?", buffer);
                }
            }

            if (Url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(CheckValidationResult);
            }

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(Url);
            httpWebRequest.Method = "GET";
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";

            HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse();

            using (var repStream = response.GetResponseStream())
            {
                using (var reader = new StreamReader(repStream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
       
    }
}
