using System.Text.Json.Nodes;

namespace Apertium.Net;

/// <summary>
///     A client for interacting with the Apertium API to perform language translation and manage valid language pairs.
/// </summary>
public class ApertiumClient : AApertiumClient
{
    public const string DEFAULT_API_URL = "https://www.apertium.org/apy/";

    private const string GetPairsRequest = "listPairs";

    private const string TranslateTextRequestPrepended = "translate?";

    private const string TranslateTextRequestFormat = "q={0}&langpair={1}%7C{2}";

    private readonly string baseUrl;

    private readonly string translateTextRequestParams;

    /// <summary>
    ///     A client for interacting with the Apertium API to perform language translation and manage valid language pairs.
    /// </summary>
    public ApertiumClient() : this(DEFAULT_API_URL, string.Empty, false)
    {
    }

    /// <summary>
    ///     A client for interacting with the Apertium API to perform language translation and manage valid language pairs.
    /// </summary>
    /// <param name="baseApiUrl">The base URL of the Apertium API.</param>
    /// <param name="apiKey">The API key for authentication (if required).</param>
    /// <param name="autoLoadValidPairs">Whether to automatically load valid language pairs on initialization.</param>
    /// <param name="defaultFromLanguage">The default source language for translations.</param>
    /// <param name="defaultToLanguage">The default target language for translations.</param>
    /// <param name="validateDefaultLanguagePair">Whether to validate the default language pair against valid pairs.</param>
    public ApertiumClient(string baseApiUrl, string apiKey, bool autoLoadValidPairs, string defaultFromLanguage = "eng",
        string defaultToLanguage = "spa", bool validateDefaultLanguagePair = false) : base(defaultFromLanguage, defaultToLanguage)
    {
        this.baseUrl = baseApiUrl;
        if (!this.baseUrl[^1].Equals('/'))
        {
            this.baseUrl += '/';
        }

        this.translateTextRequestParams = string.IsNullOrWhiteSpace(apiKey)
            ? $"{TranslateTextRequestPrepended}{TranslateTextRequestFormat}"
            : $"{TranslateTextRequestPrepended}key={apiKey}&{TranslateTextRequestFormat}";

        if (autoLoadValidPairs || validateDefaultLanguagePair)
        {
            HashSet<(string, string)> pairs = GetValidLanguagePairs();
            if (pairs.Count == 0)
            {
                throw new Exception("No valid language pairs found.");
            }

            if (validateDefaultLanguagePair)
            {
                if (!pairs.Contains((defaultFromLanguage, defaultToLanguage)))
                {
                    throw new Exception($"Invalid default language pair: {defaultFromLanguage} -> {defaultToLanguage}");
                }
            }
        }
    }
    
    /// <summary>
    ///     Asynchronously retrieves valid language pairs from the Apertium API. This method fetches the list of language pairs
    ///     from the API, clears any existing cached data if forceRefresh is true, and returns them as a HashSet of tuples.
    ///     Each tuple
    ///     represents a source and target language pair.
    /// </summary>
    /// <param name="forceRefresh">If true, forces a refresh of the valid language pairs by re-fetching data from the API.</param>
    /// <returns>A HashSet containing tuples of valid language pairs, where each tuple represents a source and target language.</returns>
    public override async Task<HashSet<(string, string)>> GetValidLanguagePairsAsync(bool forceRefresh = false)
    {
        if (this.validLanguagePairs.Count == 0 || forceRefresh)
        {
            this.validLanguagePairs.Clear();
            this.fromLanguageToLanguageMapping.Clear();
            this.toLanguageFromLanguageMapping.Clear();
            JsonArray pairs = await ListPairsAsync();
            foreach (JsonNode pair in pairs)
            {
                var fromLang = pair["sourceLanguage"].ToString();
                var toLang = pair["targetLanguage"].ToString();
                this.validLanguagePairs.Add((fromLang, toLang));

                if (!this.fromLanguageToLanguageMapping.ContainsKey(fromLang))
                {
                    this.fromLanguageToLanguageMapping[fromLang] = new List<string>();
                }
                this.fromLanguageToLanguageMapping[fromLang].Add(toLang);
                
                if (!this.toLanguageFromLanguageMapping.ContainsKey(toLang))
                {
                    this.toLanguageFromLanguageMapping[toLang] = new List<string>();
                }
                this.toLanguageFromLanguageMapping[toLang].Add(fromLang);
            }
        }

        return this.validLanguagePairs;
    }

    /// <summary>
    ///     Internal method to asynchronously translates the given text from the specified source language to the
    ///     specified target language using the Apertium API.
    /// </summary>
    /// <param name="text">The text to be translated.</param>
    /// <param name="fromLanguage">The source language code.</param>
    /// <param name="toLanguage">The target language code.</param>
    /// <returns>The translated text as a string.</returns>
    /// <exception cref="Exception">
    ///     Thrown if the source or target languages are invalid or if the server response is
    ///     malformed.
    /// </exception>
    protected override async Task<string> InternalTranslateAsync(string text, string fromLanguage, string toLanguage)
    {
        var encodedText = Uri.EscapeDataString(text);
        var urlParams = string.Format(this.translateTextRequestParams, encodedText, fromLanguage, toLanguage);
        HttpResponseMessage response = await GetRequest(urlParams);
        response.EnsureSuccessStatusCode();

        var translatedText = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(translatedText))
        {
            throw new Exception("Empty response from server.");
        }

        JsonObject? jsonObject = JsonNode.Parse(translatedText)?.AsObject();
        if (jsonObject == null)
        {
            throw new Exception("Invalid JSON response from server.");
        }

        if (!jsonObject.ContainsKey("responseData"))
        {
            throw new Exception($"Invalid JSON response from server: {jsonObject}");
        }

        JsonObject responseData = (JsonObject)jsonObject["responseData"];
        if (!responseData.ContainsKey("translatedText"))
        {
            throw new Exception($"Invalid JSON response from server: {jsonObject}");
        }

        var translatedTextResult = responseData["translatedText"].ToString();
        if (string.IsNullOrWhiteSpace(translatedTextResult))
        {
            throw new Exception("Empty translated text in response.");
        }

        return translatedTextResult;
    }

    /// <summary>
    ///     Sends an HTTP GET request to the specified URL.
    /// </summary>
    /// <param name="urlParams">The URL parameters to append to the base URL.</param>
    /// <returns>The HTTP response message from the GET request.</returns>
    private async Task<HttpResponseMessage> GetRequest(string urlParams)
    {
        var url = $"{this.baseUrl}{urlParams}";
        HttpResponseMessage response = await this.HttpClient.GetAsync(url);
        return response;
    }

    /// <summary>
    ///     Retrieves language pairs from the Apertium API. This method sends an HTTP GET request to the
    ///     "listPairs" endpoint and returns the JSON array containing the language pair data.
    /// </summary>
    /// <returns>A JsonArray representing the language pairs retrieved from the API.</returns>
    private async Task<JsonArray> ListPairsAsync()
    {
        HttpResponseMessage response = await GetRequest(GetPairsRequest);
        response.EnsureSuccessStatusCode();

        var translatedText = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(translatedText))
        {
            throw new Exception("Empty response from server.");
        }

        JsonObject? jsonObject = JsonNode.Parse(translatedText)?.AsObject();
        if (jsonObject == null)
        {
            throw new Exception("Invalid JSON response from server.");
        }

        if (!jsonObject.ContainsKey("responseData"))
        {
            throw new Exception($"Invalid JSON response from server: {jsonObject}");
        }

        JsonArray array = (JsonArray)jsonObject["responseData"];
        return array;
    }
}
