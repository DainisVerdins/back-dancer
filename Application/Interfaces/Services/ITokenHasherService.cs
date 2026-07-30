namespace Application.Interfaces.Services;

public interface ITokenHasherService
{
    public string Hash(string textToHash);
}
