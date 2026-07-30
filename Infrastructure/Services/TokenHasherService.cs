using Application.Interfaces.Services;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Services;

public class TokenHasherService : ITokenHasherService
{
    public string Hash(string textToHash)
    {
        if (string.IsNullOrEmpty(textToHash))
            throw new ArgumentNullException(nameof(textToHash));

        var inputBytes = Encoding.UTF8.GetBytes(textToHash);
        var inputHash = SHA256.HashData(inputBytes);

        return Convert.ToHexString(inputHash);
    }
}
