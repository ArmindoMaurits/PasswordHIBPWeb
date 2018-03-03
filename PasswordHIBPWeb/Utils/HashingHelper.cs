namespace PasswordHIBPWeb.Utils
{
    using System.Security.Cryptography;
    using System.Text;

    public class HashingHelper
    {
        public static string Hash(string input)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                var stringBuilder = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    stringBuilder.Append(b.ToString("X2"));
                }

                return stringBuilder.ToString();
            }
        }
    }
}
