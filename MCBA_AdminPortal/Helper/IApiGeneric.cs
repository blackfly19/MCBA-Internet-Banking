namespace MCBA_AdminPortal.Helper;

public interface IApiGeneric
{
    T Get<T>(string endpoint);
    bool Put<T>(string endpoint, T data);
    bool Post(string endpoint);
}