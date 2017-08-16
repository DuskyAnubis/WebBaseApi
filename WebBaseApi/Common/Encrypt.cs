using System.Security.Cryptography;
using System.Text;

namespace WebBaseApi.Common
{
    public static class Encrypt
    {
        public static string Md5Encrypt(string input)
        {
            MD5 md5 = MD5.Create();
            byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sb.Append(data[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}
