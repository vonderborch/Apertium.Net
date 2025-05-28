namespace Apertium.Net;

/// <summary>
///     An abstract class defining the functionality of an Apertium client for translation and language validation.
/// </summary>
public abstract class AApertiumClient : IDisposable
{
    /// <summary>
    /// A mapping from source languages to lists of target languages supported by the Apertium translation service.
    /// </summary>
    protected readonly Dictionary<string, List<string>> fromLanguageToLanguageMapping;

    /// <summary>
    /// A mapping from target languages to lists of source languages supported by the Apertium translation service.
    /// </summary>
    protected readonly Dictionary<string, List<string>> toLanguageFromLanguageMapping;

    /// <summary>
    /// A collection of valid (source language, target language) pairs supported by the Apertium translation service.
    /// </summary>
    protected readonly HashSet<(string, string)> validLanguagePairs;

    /// <summary>
    /// The HttpClient instance used to communicate with the Apertium translation service for making HTTP requests.
    /// </summary>
    protected HttpClient HttpClient;
    
    /// <summary>
    /// An abstract class defining the functionality of an Apertium client for translation and language validation.
    /// </summary>
    protected AApertiumClient(string defaultFromLanguage, string defaultToLanguage)
    {
        this.DefaultFromLanguage = defaultFromLanguage;
        this.DefaultToLanguage = defaultToLanguage;
        
        this.HttpClient = new();
        this.validLanguagePairs = new();
        this.fromLanguageToLanguageMapping = new();
        this.toLanguageFromLanguageMapping = new();
    }

    /// <summary>
    /// Sets the HTTP client used by the AApertiumClient.
    /// </summary>
    /// <param name="httpClient">The new HTTP client to use.</param>
    public void SetHttpClient(HttpClient httpClient)
    {
        this.HttpClient?.Dispose();
        this.HttpClient = httpClient;
    }
    
    /// <summary>
    ///     The default source language used when initiating translations without specifying a "from" language.
    /// </summary>
    public string DefaultFromLanguage { get; protected init; }

    /// <summary>
    ///     The default target language used when initiating translations without specifying a "to" language.
    /// </summary>
    public string DefaultToLanguage { get; protected init; }

    /// <summary>
    /// A dictionary mapping target languages to lists of source languages supported for translation.
    /// </summary>
    public Dictionary<string, List<string>> ToLanguageFromLanguageMapping => this.toLanguageFromLanguageMapping.ToDictionary(x => x.Key, x => new List<string>(x.Value));

    /// <summary>
    /// A dictionary mapping source languages to lists of target languages supported for translation. Each key represents a source language,
    /// and the corresponding value is a list of target languages that can be translated from it using the Apertium service.
    /// </summary>
    public Dictionary<string, List<string>> FromLanguageToLanguageMapping => this.fromLanguageToLanguageMapping.ToDictionary(x => x.Key, x => new List<string>(x.Value));

    /// <summary>
    /// A collection of valid language pairs supported by the Apertium client. Each pair consists of a source language and a target language.
    /// </summary>
    public HashSet<(string, string)> ValidLanguagePairs => new(this.validLanguagePairs);

    /// <summary>
    ///     Retrieves valid source languages for a specified target language from the Apertium API. This method checks the
    ///     internal mapping of language pairs
    ///     and returns a list of source languages associated with the provided target language. If forceRefresh is true, it
    ///     may refresh the cached data before retrieving.
    /// </summary>
    /// <param name="toLanguage">The target language for which valid source languages are being retrieved.</param>
    /// <param name="forceRefresh">If true, forces a refresh of the valid language pairs by re-fetching data from the API.</param>
    /// <returns>A list of strings representing valid source languages associated with the specified target language.</returns>
    public List<string> GetValidFromLanguages(string toLanguage, bool forceRefresh = false)
    {
        var result = GetValidFromLanguagesAsync(toLanguage, forceRefresh);
        result.Wait();
        return result.Result;
    }
    
    /// <summary>
    ///     Retrieves valid source languages for a specified target language from the Apertium API. This method checks the
    ///     internal mapping of language pairs
    ///     and returns a list of source languages associated with the provided target language. If forceRefresh is true, it
    ///     may refresh the cached data before retrieving.
    /// </summary>
    /// <param name="toLanguage">The target language for which valid source languages are being retrieved.</param>
    /// <param name="forceRefresh">If true, forces a refresh of the valid language pairs by re-fetching data from the API.</param>
    /// <returns>A list of strings representing valid source languages associated with the specified target language.</returns>
    public async Task<List<string>> GetValidFromLanguagesAsync(string toLanguage, bool forceRefresh = false)
    {
        var mapping = this.ToLanguageFromLanguageMapping;
        if (mapping.Count == 0 || forceRefresh)
        {
            await GetValidLanguagePairsAsync(true);
            mapping = this.ToLanguageFromLanguageMapping;
        }

        return mapping.TryGetValue(toLanguage, out List<string>? value) ? value : new List<string>();
    }

    /// <summary>
    ///     Retrieves valid language pairs from the Apertium API. This method fetches the list of language pairs
    ///     from the API, caches them, and returns them as a HashSet of tuples. If forceRefresh is true, the cached
    ///     data is cleared before retrieving new data.
    /// </summary>
    /// <param name="forceRefresh">If true, forces a refresh of the valid language pairs by re-fetching data from the API.</param>
    /// <returns>A HashSet containing tuples of valid language pairs, where each tuple represents a source and target language.</returns>
    public HashSet<(string, string)> GetValidLanguagePairs(bool forceRefresh = false)
    {
        Task<HashSet<(string, string)>> result = GetValidLanguagePairsAsync(forceRefresh);
        result.Wait();
        return result.Result;
    }

    /// <summary>
    ///     Asynchronously retrieves valid language pairs from the Apertium API. This method fetches the list of language pairs
    ///     from the API, clears any existing cached data if forceRefresh is true, and returns them as a HashSet of tuples.
    ///     Each tuple
    ///     represents a source and target language pair.
    /// </summary>
    /// <param name="forceRefresh">If true, forces a refresh of the valid language pairs by re-fetching data from the API.</param>
    /// <returns>A HashSet containing tuples of valid language pairs, where each tuple represents a source and target language.</returns>
    public abstract Task<HashSet<(string, string)>> GetValidLanguagePairsAsync(bool forceRefresh = false);

    /// <summary>
    ///     Retrieves the list of valid target languages for the specified source language based on stored mappings.
    /// </summary>
    /// <param name="fromLanguage">The source language to check.</param>
    /// <param name="forceRefresh">If true, forces a refresh of the valid language pairs before retrieving.</param>
    /// <returns>A list of valid target languages for the specified source language.</returns>
    public List<string> GetValidToLanguages(string fromLanguage, bool forceRefresh = false)
    {
        var result = GetValidToLanguagesAsync(fromLanguage, forceRefresh);
        result.Wait();
        return result.Result;
    }

    /// <summary>
    ///     Retrieves the list of valid target languages for the specified source language based on stored mappings.
    /// </summary>
    /// <param name="fromLanguage">The source language to check.</param>
    /// <param name="forceRefresh">If true, forces a refresh of the valid language pairs before retrieving.</param>
    /// <returns>A list of valid target languages for the specified source language.</returns>
    public async Task<List<string>> GetValidToLanguagesAsync(string fromLanguage, bool forceRefresh = false)
    {
        var mapping = this.FromLanguageToLanguageMapping;
        if (mapping.Count == 0 || forceRefresh)
        {
            await GetValidLanguagePairsAsync(true);
            mapping = this.FromLanguageToLanguageMapping;
        }

        return mapping.TryGetValue(fromLanguage, out List<string>? value) ? value : new List<string>();
    }
    
    /// <summary>
    ///     Checks whether the specified language is valid as a source language based on the stored mappings.
    /// </summary>
    /// <param name="language">The source language to check.</param>
    /// <param name="forceRefresh">If true, forces a refresh of the valid language pairs before checking.</param>
    /// <returns>True if the language is valid as a source language; otherwise, false.</returns>
    public bool IsValidFromLanguage(string language, bool forceRefresh = false)
    {
        Task<bool> result = IsValidFromLanguageAsync(language, forceRefresh);
        result.Wait();
        return result.Result;
    }

    /// <summary>
    ///     Checks whether the specified language is valid as a source language based on the stored mappings.
    /// </summary>
    /// <param name="language">The source language to check.</param>
    /// <param name="forceRefresh">If true, forces a refresh of the valid language pairs before checking.</param>
    /// <returns>True if the language is valid as a source language; otherwise, false.</returns>
    public async Task<bool> IsValidFromLanguageAsync(string language, bool forceRefresh = false)
    {
        var mapping = this.FromLanguageToLanguageMapping;
        if (mapping.Count == 0 || forceRefresh)
        {
            await GetValidLanguagePairsAsync(true);
            mapping = this.FromLanguageToLanguageMapping;
        }
        
        return mapping.ContainsKey(language);
    }
    
    /// <summary>
    ///     Checks whether the specified language pair is valid based on the stored language mappings.
    /// </summary>
    /// <param name="fromLanguage">The source language of the pair.</param>
    /// <param name="toLanguage">The target language of the pair.</param>
    /// <param name="forceRefresh">If true, forces a refresh of the valid language pairs before checking.</param>
    /// <returns>True if the language pair is valid; otherwise, false.</returns>
    public bool IsValidPair(string fromLanguage, string toLanguage, bool forceRefresh = false)
    {
        var result = IsValidPairAsync(fromLanguage, toLanguage, forceRefresh);
        result.Wait();
        return result.Result;
    }
    
    /// <summary>
    ///     Checks whether the specified language pair is valid based on the stored language mappings.
    /// </summary>
    /// <param name="fromLanguage">The source language of the pair.</param>
    /// <param name="toLanguage">The target language of the pair.</param>
    /// <param name="forceRefresh">If true, forces a refresh of the valid language pairs before checking.</param>
    /// <returns>True if the language pair is valid; otherwise, false.</returns>
    public virtual async Task<bool> IsValidPairAsync(string fromLanguage, string toLanguage, bool forceRefresh = false)
    {
        var mapping = this.ValidLanguagePairs;
        if (mapping.Count == 0 || forceRefresh)
        {
            await GetValidLanguagePairsAsync(true);
            mapping = this.ValidLanguagePairs;
        }

        return mapping.Contains((fromLanguage, toLanguage));
    }

    /// <summary>
    ///     Checks whether the specified language is valid as a target language based on the stored mappings.
    /// </summary>
    /// <param name="language">The target language to check.</param>
    /// <param name="forceRefresh">If true, forces a refresh of the valid language pairs before checking.</param>
    /// <returns>True if the language is valid as a target language; otherwise, false.</returns>
    public bool IsValidToLanguage(string language, bool forceRefresh = false)
    {
        Task<bool> result = IsValidToLanguageAsync(language, forceRefresh);
        result.Wait();
        return result.Result;
    }

    /// <summary>
    ///     Checks whether the specified language is valid as a target language based on the stored mappings.
    /// </summary>
    /// <param name="language">The target language to check.</param>
    /// <param name="forceRefresh">If true, forces a refresh of the valid language pairs before checking.</param>
    /// <returns>True if the language is valid as a target language; otherwise, false.</returns>
    public async Task<bool> IsValidToLanguageAsync(string language, bool forceRefresh = false)
    {
        var mapping = this.ToLanguageFromLanguageMapping;
        if (mapping.Count == 0 || forceRefresh)
        {
            await GetValidLanguagePairsAsync(true);
            mapping = this.ToLanguageFromLanguageMapping;
        }

        return mapping.ContainsKey(language);
    }

    /// <summary>
    ///     Translates the given text from the default source language to the default target language using the Apertium API.
    /// </summary>
    /// <param name="text">The text to be translated.</param>
    /// <returns>The translated text as a string.</returns>
    public string Translate(string text)
    {
        Task<string> result = TranslateAsync(text);
        result.Wait();
        return result.Result;
    }

    /// <summary>
    ///     Translates the given text from the default source language to the specified target language using the Apertium API.
    /// </summary>
    /// <param name="text">The text to be translated.</param>
    /// <param name="toLanguage">The target language code for the translation.</param>
    /// <returns>The translated text as a string.</returns>
    public string Translate(string text, string toLanguage)
    {
        Task<string> result = TranslateAsync(text, toLanguage);
        result.Wait();
        return result.Result;
    }

    /// <summary>
    ///     Translates the given text from the specified source language to the specified target language using the Apertium
    ///     API.
    /// </summary>
    /// <param name="text">The text to be translated.</param>
    /// <param name="fromLanguage">The source language code for the translation.</param>
    /// <param name="toLanguage">The target language code for the translation.</param>
    /// <returns>The translated text as a string.</returns>
    public string Translate(string text, string fromLanguage, string toLanguage)
    {
        Task<string> result = TranslateAsync(text, fromLanguage, toLanguage);
        result.Wait();
        return result.Result;
    }

    /// <summary>
    ///     Asynchronously translates the given text from the default source language to the default language using the
    ///     Apertium API.
    /// </summary>
    /// <param name="text">The text to be translated.</param>
    /// <returns>The translated text as a string.</returns>
    /// <exception cref="Exception">
    ///     Thrown if the source or target languages are invalid or if the server response is
    ///     malformed.
    /// </exception>
    public async Task<string> TranslateAsync(string text)
    {
        var result = await TranslateAsync(text, this.DefaultFromLanguage, this.DefaultToLanguage);
        return result;
    }

    /// <summary>
    ///     Asynchronously translates the given text from the default source language to the specified target language using
    ///     the Apertium API.
    /// </summary>
    /// <param name="text">The text to be translated.</param>
    /// <param name="toLanguage">The target language code.</param>
    /// <returns>The translated text as a string.</returns>
    /// <exception cref="Exception">
    ///     Thrown if the source or target languages are invalid or if the server response is
    ///     malformed.
    /// </exception>
    public async Task<string> TranslateAsync(string text, string toLanguage)
    {
        var result = await TranslateAsync(text, this.DefaultFromLanguage, toLanguage);
        return result;
    }

    /// <summary>
    ///     Asynchronously translates the given text from the specified source language to the specified target language using
    ///     the Apertium API.
    /// </summary>
    /// <param name="text">The text to be translated.</param>
    /// <param name="fromLanguage">The source language code.</param>
    /// <param name="toLanguage">The target language code.</param>
    /// <returns>The translated text as a string.</returns>
    /// <exception cref="Exception">
    ///     Thrown if the source or target languages are invalid or if the server response is
    ///     malformed.
    /// </exception>
    public async Task<string> TranslateAsync(string text, string fromLanguage, string toLanguage)
    {
        if (!await IsValidPairAsync(fromLanguage, toLanguage))
        {
            throw new Exception($"Invalid language pair: {fromLanguage} -> {toLanguage}");
        }
        
        string translatedText = await InternalTranslateAsync(text, fromLanguage, toLanguage);
        return translatedText;
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
    protected abstract Task<string> InternalTranslateAsync(string text, string fromLanguage, string toLanguage);
    
    /// <summary>
    /// Releases the resources held by the AApertiumClient instance, ensuring proper cleanup of allocated resources.
    /// </summary>
    public void Dispose()
    {
        this.HttpClient.Dispose();
    }
}
