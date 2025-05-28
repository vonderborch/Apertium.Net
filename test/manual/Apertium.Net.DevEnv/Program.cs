namespace Apertium.Net.DevEnv;

class Program
{
    static void Main(string[] args)
    {
        TestApertiumClient();
        Console.WriteLine("ApertiumClient tests passed successfully.");
    }

    private static void TestApertiumClient()
    {
        var client = new ApertiumClient();
        var pairs = client.GetValidLanguagePairs();
        if (pairs.Count == 0)
        {
            throw new Exception("No valid language pairs found.");
        }
        
        if (!pairs.Contains(("eng", "spa")))
        {
            throw new Exception("Default language pair (eng|spa) is not valid.");
        }
        
        var translatedText = client.Translate("Hello World!", "spa");
        if (string.IsNullOrWhiteSpace(translatedText))
        {
            throw new Exception("Translation failed or returned empty text.");
        }
        if (translatedText != "Hola Mundo!")
        {
            throw new Exception($"Unexpected translation result: {translatedText}");
        }
    }
}
