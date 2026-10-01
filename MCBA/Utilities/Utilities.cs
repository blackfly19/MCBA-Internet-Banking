namespace MCBA.Utilities;
using Newtonsoft.Json;

public static class Utilities
{
    public static T LoadJson<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json);
    }
}