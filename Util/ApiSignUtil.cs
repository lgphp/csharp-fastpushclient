using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EasyEncryption;

namespace FastLivePushClient.Util
{
    public class ApiSignUtil
    {
        internal static string MakeSign(object data, byte[] apiKey)
        {
            var ak = Convert.ToBase64String(apiKey);
            var fields = data.GetType().GetFields();
            var inDict = new Dictionary<string, string>();
            foreach (var filed in fields)
            {
                inDict.Add(filed.Name , filed.GetValue(data)+"");
            }
            string[] arrKeys = inDict.Keys.ToArray();
            Array.Sort(arrKeys, string.CompareOrdinal);
            var StrA = new StringBuilder();
            foreach (var key in arrKeys)
            {
                var value =  inDict[key];
                if (!String.IsNullOrEmpty(value)) //空值不参与签名
                {
                    StrA.Append(key + "=")
                        .Append(value + "&");
                }
            }
            StrA.Append("key=" + ak);  
            return MD5.ComputeMD5Hash(StrA.ToString()).ToUpper();
        }
    }
}