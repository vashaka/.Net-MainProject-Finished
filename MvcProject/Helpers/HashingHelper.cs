using System.Text;

using System.Security.Cryptography;

namespace MvcProject.Helpers
{
    public static class HashingHelper
    {
        public static string GenerateSHA256Hash(string amount, string transactionId, string secretKey, string MerchantId)
        {
            var concatenatedString = string.Join("+", new[] { amount, transactionId, secretKey, MerchantId });
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(concatenatedString));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}
