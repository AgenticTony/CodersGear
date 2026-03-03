using System.Security.Cryptography;
using System.Text;

namespace CodersGear.Utility
{
    public interface IWebhookSignatureVerifier
    {
        bool VerifyPrintifySignature(string payload, string signatureHeader, string secret);
    }

    public class WebhookSignatureVerifier : IWebhookSignatureVerifier
    {
        public bool VerifyPrintifySignature(string payload, string signatureHeader, string secret)
        {
            if (string.IsNullOrEmpty(payload) || string.IsNullOrEmpty(signatureHeader) || string.IsNullOrEmpty(secret))
            {
                return false;
            }

            // Printify sends signature in format: "sha256=<hex_signature>"
            // Extract the actual signature value
            var signatureParts = signatureHeader.Split('=');
            if (signatureParts.Length != 2 || signatureParts[0] != "sha256")
            {
                return false;
            }

            var providedSignature = signatureParts[1];

            // Compute HMAC-SHA256 of the payload using the webhook secret
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            var payloadBytes = Encoding.UTF8.GetBytes(payload);

            using var hmac = new HMACSHA256(keyBytes);
            var computedHash = hmac.ComputeHash(payloadBytes);
            var computedSignature = BitConverter.ToString(computedHash).Replace("-", "").ToLowerInvariant();

            // Use constant-time comparison to prevent timing attacks
            return CryptographicEquals(
                Encoding.UTF8.GetBytes(computedSignature),
                Encoding.UTF8.GetBytes(providedSignature)
            );
        }

        /// <summary>
        /// Constant-time comparison to prevent timing attacks
        /// </summary>
        private static bool CryptographicEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }

            int result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }

            return result == 0;
        }
    }
}
