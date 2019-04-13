using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Utils.Utils
{
    public class EncryptUtil
    {
        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="input"></param>
        /// <param name="encode"></param>
        /// <returns></returns>
        //public static string MD5Encrypt(string input, Encoding encode)
        //{
        //    if (string.IsNullOrEmpty(input))
        //    {
        //        return null;
        //    }
        //    MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();
        //    byte[] data = md5Hasher.ComputeHash(encode.GetBytes(input));
        //    StringBuilder sBuilder = new StringBuilder();
        //    for (int i = 0; i < data.Length; i++)
        //    {
        //        sBuilder.Append(data[i].ToString("x2"));//16进制
        //    }
        //    return sBuilder.ToString();
        //}

        /// <summary>
        ///  MD5加密
        /// </summary>
        /// <param name="pToEncrypt"></param>
        /// <param name="sKey"></param>
        /// <returns></returns>
        //public static string MD5Encrypt(string pToEncrypt, string sKey)
        //{
        //    DESCryptoServiceProvider des = new DESCryptoServiceProvider();
        //    byte[] inputByteArray = Encoding.Default.GetBytes(pToEncrypt);
        //    des.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
        //    des.IV = ASCIIEncoding.ASCII.GetBytes(sKey);
        //    MemoryStream ms = new MemoryStream();
        //    CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
        //    cs.Write(inputByteArray, 0, inputByteArray.Length); cs.FlushFinalBlock();
        //    StringBuilder ret = new StringBuilder();
        //    foreach (byte b in ms.ToArray())
        //    {
        //        ret.AppendFormat("{0:X2}", b);
        //    }
        //    ret.ToString();
        //    return ret.ToString();
        //}
        /// <summary>
        /// MD5解密
        /// </summary>
        /// <param name="pToDecrypt"></param>
        /// <param name="sKey"></param>
        /// <returns></returns>
        //public static string MD5Decrypt(string pToDecrypt, string sKey)
        //{
        //    DESCryptoServiceProvider des = new DESCryptoServiceProvider();
        //    byte[] inputByteArray = new byte[pToDecrypt.Length / 2];
        //    for (int x = 0; x < pToDecrypt.Length / 2; x++)
        //    {
        //        int i = (Convert.ToInt32(pToDecrypt.Substring(x * 2, 2), 16));
        //        inputByteArray[x] = (byte)i;
        //    }
        //    des.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
        //    des.IV = ASCIIEncoding.ASCII.GetBytes(sKey);
        //    MemoryStream ms = new MemoryStream();
        //    CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
        //    cs.Write(inputByteArray, 0, inputByteArray.Length);
        //    cs.FlushFinalBlock();
        //    StringBuilder ret = new StringBuilder();
        //    return System.Text.Encoding.UTF8.GetString(ms.ToArray());
        //}
        public static string MD5Encrypt(string input,string key)
        {
            if (!string.IsNullOrWhiteSpace(input) && !string.IsNullOrWhiteSpace(key))
            {
                MD5 md5 = MD5.Create();
                byte[] b = md5.ComputeHash(Encoding.UTF8.GetBytes(input + key));
                return Convert.ToBase64String(b).ToUpper();
            }

            return string.Empty;
        }
        /// <summary>
        /// 基于Sha1的自定义加密字符串方法：输入一个字符串，返回一个由40个字符组成的十六进制的哈希散列（字符串）。
        /// </summary>
        /// <param name="str">要加密的字符串</param>
        /// <returns>加密后的十六进制的哈希散列（字符串）</returns>
        public static string Sha1(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return null;
            }

            var buffer = Encoding.UTF8.GetBytes(str);
            var data = SHA1.Create().ComputeHash(buffer);

            var sb = new StringBuilder();
            foreach (var t in data)
            {
                sb.Append(t.ToString("X2"));
            }

            return sb.ToString();
        }
    }
}
