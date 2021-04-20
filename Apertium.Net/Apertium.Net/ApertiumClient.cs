using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RestSharp;

namespace Apertium.Net
{
    /// <summary>
    ///     An Apertium client.
    /// </summary>
    public class ApertiumClient
    {
        /// <summary>
        ///     (Immutable) the default API URL.
        /// </summary>
        private const string DefaultApiUrl = "https://www.apertium.org/apy/";

        /// <summary>
        ///     (Immutable) the get pairs request.
        /// </summary>
        private const string GetPairsRequest = "listPairs";

        /// <summary>
        ///     (Immutable) the translate text request prepended.
        /// </summary>
        private const string TranslateTextRequestPrepended = "translate?";

        /// <summary>
        ///     (Immutable) .
        /// </summary>
        private const string TranslateTextRequestFormat = "q={0}&langpair={1}%7C{2}";

        /// <summary>
        ///     URL of the base.
        /// </summary>
        private string _baseUrl;

        /// <summary>
        ///     The client.
        /// </summary>
        private RestClient _client;

        /// <summary>
        ///     The valid pairs.
        /// </summary>
        private HashSet<(string, string)> _validPairs;

        /// <summary>
        ///     From language to languages.
        /// </summary>
        private Dictionary<string, List<string>> _fromLanguageToLanguages;

        /// <summary>
        ///     To language from languages.
        /// </summary>
        private Dictionary<string, List<string>> _toLanguageFromLanguages;

        /// <summary>
        ///     The API key.
        /// </summary>
        private string _apiKey;

        /// <summary>
        ///     The translate text request.
        /// </summary>
        private string _translateTextRequest;

        /// <summary>
        ///     The default from language.
        /// </summary>
        private string _defaultFromLanguage = "eng";

        /// <summary>
        ///     The default to language.
        /// </summary>
        private string _defaultToLanguage = "spa";

        /// <summary>
        ///     Constructor.
        /// </summary>
        ///
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        ///
        /// <param name="baseApiUrl">                  (Optional) URL of the base API. </param>
        /// <param name="apiKey">
        ///     (Optional)
        ///     The API key.
        /// </param>
        /// <param name="autoLoadValidPairs">
        ///     (Optional) True to automatically load valid pairs.
        /// </param>
        /// <param name="defaultFromLanguage">
        ///     (Optional)
        ///     The default from language.
        /// </param>
        /// <param name="defaultToLanguage">
        ///     (Optional)
        ///     The default to language.
        /// </param>
        /// <param name="validateDefaultLanguagePair">
        ///     (Optional) True to validate default language pair.
        /// </param>
        public ApertiumClient(string baseApiUrl = null, string apiKey = null, bool autoLoadValidPairs = false, string defaultFromLanguage = "eng", string defaultToLanguage = "spa", bool validateDefaultLanguagePair = false)
        {
            _baseUrl = string.IsNullOrEmpty(baseApiUrl) ? DefaultApiUrl : baseApiUrl;
            _client = new RestClient(_baseUrl);
            _validPairs = null;
            _apiKey = apiKey;
            _translateTextRequest = string.IsNullOrEmpty(apiKey)
                ? $"{TranslateTextRequestPrepended}{TranslateTextRequestFormat}"
                : $"{TranslateTextRequestPrepended}key={apiKey}&{TranslateTextRequestFormat}";

            if (autoLoadValidPairs || validateDefaultLanguagePair)
            {
                GetValidPairs();

                if (validateDefaultLanguagePair)
                {
                    if (IsValidPair(defaultFromLanguage, defaultToLanguage))
                    {
                        throw new Exception("The default language pair is invalid!");
                    }
                }
            }
        }

        /// <summary>
        ///     Gets the default from language.
        /// </summary>
        ///
        /// <value>
        ///     The default from language.
        /// </value>
        public string DefaultFromLanguage => _defaultFromLanguage;

        /// <summary>
        ///     Gets the default to language.
        /// </summary>
        ///
        /// <value>
        ///     The default to language.
        /// </value>
        public string DefaultToLanguage => _defaultToLanguage;

        /// <summary>
        ///     Gets URL of the base API.
        /// </summary>
        ///
        /// <value>
        ///     The base API URL.
        /// </value>
        public string BaseApiUrl => _baseUrl;

        /// <summary>
        ///     Gets the API key.
        /// </summary>
        ///
        /// <value>
        ///     The API key.
        /// </value>
        public string ApiKey => _apiKey;

        /// <summary>
        ///     Updates the default language pairs.
        /// </summary>
        ///
        /// <param name="defaultFromLanguage"> The default from language. </param>
        /// <param name="defaultToLanguage"> The default to language. </param>
        ///
        /// <returns>
        ///     True if it succeeds, false if it fails.
        /// </returns>
        public bool UpdateDefaultLanguages(string defaultFromLanguage, string defaultToLanguage)
        {
            if (IsValidPair(defaultFromLanguage, defaultToLanguage))
            {
                _defaultFromLanguage = defaultFromLanguage;
                _defaultToLanguage = defaultToLanguage;
            }

            return false;
        }

        /// <summary>
        ///     Updates the configured apertium server.
        /// </summary>
        ///
        /// <param name="baseApiUrl"> URL of the base API. </param>
        /// <param name="apiKey">
        ///     (Optional)
        ///     The API key.
        /// </param>
        public void UpdateApertiumServer(string baseApiUrl, string apiKey)
        {
            _baseUrl = string.IsNullOrEmpty(baseApiUrl) ? DefaultApiUrl : baseApiUrl;
            _client = new RestClient(_baseUrl);
            _validPairs = null;
            _apiKey = apiKey;
            _translateTextRequest = string.IsNullOrEmpty(apiKey)
                ? $"{TranslateTextRequestPrepended}{TranslateTextRequestFormat}"
                : $"{TranslateTextRequestPrepended}key={apiKey}&{TranslateTextRequestFormat}";
        }

        /// <summary>
        ///     Clears the valid pairs cache.
        /// </summary>
        public void ClearValidPairsCache()
        {
            _validPairs = null;
        }

        /// <summary>
        ///     Translates a specific text from a particular language to another language.
        /// </summary>
        ///
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        ///
        /// <param name="text">         The text to translate. </param>
        /// <param name="fromLanguage"> (Optional) The language to translate the text from. Defaults to the configured DefaultFromLanguage. </param>
        /// <param name="toLanguage">   (Optional) The language to translate the text to. Defaults to the configured DefaultToLanguage. </param>
        ///
        /// <returns>
        ///     The translated text.
        /// </returns>
        public async Task<string> TranslateAsync(string text, string fromLanguage = null, string toLanguage = null)
        {
            try
            {
                // Prepare data and send request
                text = text.Replace(" ", "%20");
                var requestMethod = string.Format(_translateTextRequest, text, fromLanguage ?? _defaultFromLanguage, toLanguage ?? _defaultToLanguage);
                var request = new RestRequest(requestMethod, DataFormat.Json);
                var response = _client.Get(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var content = response.Content;
                    var json = (JsonObject)SimpleJson.DeserializeObject(content);

                    var responseCode = (long)json["responseStatus"];
                    var responseData = (JsonObject)json["responseData"];
                    switch (responseCode)
                    {
                        case 200:
                            return responseData["translatedText"].ToString();
                        case 400:
                            throw new Exception($"Bad parameters; a compulsory argument is missing, or there is an argument with wrong format. Details: {json["responseDetails"]}");
                        case 451:
                            throw new Exception("Unsupported pair; the translation engine can't translate with the requested language pair.");
                        case 452:
                            throw new Exception("Unsupported format; the translation engine doesn't recognize the requested format.");
                        case 500:
                            throw new Exception($"Unexpected error; an unexpected error happened. Details: {json["responseDetails"]}");
                        case 552:
                            throw new Exception("The traffic limit for your IP or your user has been reached.");
                        default:
                            throw new Exception("Error parsing response.");
                    }
                }
                else
                {
                    throw new Exception($"HTTP Error {response.StatusCode}! {response.ErrorMessage}", response.ErrorException);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        ///     Translates a specific text from a particular language to another language.
        /// </summary>
        ///
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        ///
        /// <param name="text">         The text to translate. </param>
        /// <param name="fromLanguage"> (Optional) The language to translate the text from. Defaults to the configured DefaultFromLanguage. </param>
        /// <param name="toLanguage">   (Optional) The language to translate the text to. Defaults to the configured DefaultToLanguage. </param>
        ///
        /// <returns>
        ///     The translated text.
        /// </returns>
        public string Translate(string text, string fromLanguage = null, string toLanguage = null)
        {
            var result = TranslateAsync(text, fromLanguage, toLanguage);
            result.Wait();
            return result.Result;
        }

        /// <summary>
        ///     Gets a list of all language pairs supported by the configured Apertium server.
        /// </summary>
        ///
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        ///
        /// <returns>
        ///     The language pairs.
        /// </returns>
        public async Task<JsonArray> ListPairsAsync()
        {
            try
            {
                var request = new RestRequest(GetPairsRequest, DataFormat.Json);
                var response = _client.Get(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var content = response.Content;
                    var json = (JsonObject)SimpleJson.DeserializeObject(content);

                    return (JsonArray)json["responseData"];
                }
                else
                {
                    throw new Exception($"HTTP Error {response.StatusCode}! {response.ErrorMessage}", response.ErrorException);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        ///     Gets a list of all language pairs supported by the configured Apertium server.
        /// </summary>
        ///
        /// <exception cref="Exception"> Thrown when an exception error condition occurs. </exception>
        ///
        /// <returns>
        ///     The language pairs.
        /// </returns>
        public JsonArray ListPairs()
        {
            var result = ListPairsAsync();
            result.Wait();
            return result.Result;
        }

        /// <summary>
        ///     Gets all valid language pairs supported by the configured Apertium server.
        /// </summary>
        ///
        /// <returns>
        ///     The valid language pairs.
        /// </returns>
        public HashSet<(string, string)> GetValidPairs()
        {
            var request = ListPairs();
            _validPairs = new HashSet<(string, string)>();
            _fromLanguageToLanguages = new Dictionary<string, List<string>>();
            _toLanguageFromLanguages = new Dictionary<string, List<string>>();
            for (var i = 0; i < request.Count; i++)
            {
                var pair = (JsonObject) request[i];
                var sourceLanguage = pair["sourceLanguage"].ToString();
                var targetLanguage = pair["targetLanguage"].ToString();
                _validPairs.Add((sourceLanguage, targetLanguage));

                if (!_fromLanguageToLanguages.ContainsKey(sourceLanguage))
                {
                    _fromLanguageToLanguages.Add(sourceLanguage, new List<string>());
                }
                if (!_toLanguageFromLanguages.ContainsKey(targetLanguage))
                {
                    _toLanguageFromLanguages.Add(targetLanguage, new List<string>());
                }

                _fromLanguageToLanguages[sourceLanguage].Add(targetLanguage);
                _toLanguageFromLanguages[targetLanguage].Add(sourceLanguage);
            }

            return _validPairs;
        }

        /// <summary>
        ///     Determines if the provided language pair is valid for the configured Apertium server
        /// </summary>
        ///
        /// <param name="fromLanguage"> The language to translate text from. </param>
        /// <param name="toLanguage">   The language to translate text to. </param>
        ///
        /// <returns>
        ///     True if valid pair, false if not.
        /// </returns>
        public bool IsValidPair(string fromLanguage, string toLanguage)
        {
            if (_validPairs == null)
            {
                GetValidPairs();
            }

            return _validPairs.Contains((fromLanguage, toLanguage));
        }

        /// <summary>
        ///     Returns a list of all languages that the provided language can be translated to
        /// </summary>
        ///
        /// <param name="fromLanguage"> From language. </param>
        ///
        /// <returns>
        ///     The valid to languages.
        /// </returns>
        public List<string> GetValidToLanguages(string fromLanguage)
        {
            if (_validPairs == null)
            {
                GetValidPairs();
            }

            return new List<string>(_fromLanguageToLanguages[fromLanguage]);
        }

        /// <summary>
        ///     Returns a list of all languages that the provided language can be translated from
        /// </summary>
        ///
        /// <param name="toLanguage"> To language. </param>
        ///
        /// <returns>
        ///     The valid from languages.
        /// </returns>
        public List<string> GetValidFromLanguages(string toLanguage)
        {
            if (_validPairs == null)
            {
                GetValidPairs();
            }

            return new List<string>(_toLanguageFromLanguages[toLanguage]);
        }
    }
}
