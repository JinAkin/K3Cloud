using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Utils.Utils
{
    public class CodingUtil
    {
        public static byte[] Encode(string str)
        {
            if (!string.IsNullOrWhiteSpace(str))
            {
                Encoder encoder = Encoding.UTF8.GetEncoder();

                str = ToUnicodeString(str);
                Char[] chars = str.ToCharArray();
                byte[] bytes = new byte [ encoder.GetByteCount(chars, 0, chars.Length, true) ] ;
                encoder.GetBytes(chars,0,chars.Length,bytes,0,true);

                return bytes;
            }
            return null;
        }

        public static string Decode(string str)
        {
            if (!string.IsNullOrWhiteSpace(str))
            {
                Decoder decoder = Encoding.UTF8.GetDecoder();

                int charSize = decoder.GetCharCount(Encode(str),0,Encode(str).Length);
                Char[] chs = new char[charSize];

                string result = new string(chs);
                return result;
            }

            return null;
        }

        public static string ToUnicodeString(string str)
        {
            StringBuilder strResult = new StringBuilder();
            if (!string.IsNullOrEmpty(str))
            {
                for (int i = 0; i < str.Length; i++)
                {
                    strResult.Append("\\u");
                    strResult.Append(((int)str[i]).ToString("x"));
                }
            }
            return strResult.ToString();
        }

        public static string FromUnicodeString(string str)
        {
            //最直接的方法Regex.Unescape(str);
            StringBuilder strResult = new StringBuilder();
            if (!string.IsNullOrEmpty(str))
            {
                string[] strlist = str.Replace("\\", "").Split('u');
                try
                {
                    for (int i = 1; i < strlist.Length; i++)
                    {
                        int charCode = Convert.ToInt32(strlist[i], 16);
                        strResult.Append((char)charCode);
                    }
                }
                catch (FormatException ex)
                {
                    return Regex.Unescape(str);
                }
            }
            return strResult.ToString();
        }

        public static string DecodeString(string str)
        {
            if (!string.IsNullOrWhiteSpace(str))
            {
                // Create two different encodings.
                Encoding utf8 = Encoding.UTF8;
                Encoding defaultCode = Encoding.Default;

                // Convert the string into a byte[].
                byte[] utf8Bytes = utf8.GetBytes(str);

                // Perform the conversion from one encoding to the other.
                byte[] defaultBytes = Encoding.Convert(utf8, defaultCode, utf8Bytes);

                // Convert the new byte[] into a char[] and then into a string.
                // This is a slightly different approach to converting to illustrate
                // the use of GetCharCount/GetChars.
                char[] defaultChars = new char[defaultCode.GetCharCount(defaultBytes, 0, defaultBytes.Length)];
                defaultCode.GetChars(defaultBytes, 0, defaultBytes.Length, defaultChars, 0);
                string defaultString = new string(defaultChars);

                return defaultString;
            }

            return null;
        }

        public static string Decodes(string str)
        {
            if (!string.IsNullOrWhiteSpace(str))
            {
                byte[] buffer = Encoding.UTF8.GetBytes(str);
                string keyword = Encoding.GetEncoding("GB2312").GetString(buffer);

                Byte[] chars = Encoding.Default.GetBytes(keyword);
                keyword = Encoding.UTF8.GetString(chars);

                return keyword;
            }

            return null;
        }

        public static string De(string str)
        {
            if(!string.IsNullOrWhiteSpace(str))
            {
                byte[] bs = Encoding.GetEncoding("UTF-8").GetBytes(str);
                bs = Encoding.Convert(Encoding.GetEncoding("UTF-8"), Encoding.GetEncoding("GB2312"), bs);
                return Encoding.GetEncoding("GB2312").GetString(bs);
            }
            return null;
        }

    }
}
