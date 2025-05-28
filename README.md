# Apertium.Net

A simple library to help with using [Apertium](https://apertium.org/) for machine translation.

# Installation

A nuget package is available: [Apertium.Net](https://www.nuget.org/packages/Apertium.Net/)

# Basic Usage

```csharp
var client = new ApertiumClient();

var translatedText = client.Translate("Hello World!", "eng", "spa");
Console.WriteLine(translatedText); // Outputs: "Hola Mundo!"

```

# Configuring the Client

There are two constructors available for the `ApertiumClient` class:
- `ApertiumClient()`: Uses the default Apertium server at `https://www.apertium.org/apy/` and does not require an API key. The default language pair is English to Spanish (`eng` to `spa`).
- `ApertiumClient(string baseApiUrl, string apiKey = null, bool autoLoadValidPairs = false, string defaultFromLanguage = "eng", string defaultToLanguage = "spa", bool validateDefaultLanguagePair = false)`: Allows you to specify a custom Apertium server URL, an API key (if required), and other configuration options.

For the second constructor, the following parameters are available:

| Parameter                   | Required | Default Value | Description                                                                                                 |
|-----------------------------|----------|---------------|-------------------------------------------------------------------------------------------------------------|
| baseApiUrl                  | Yes      |               | The Apertium server to use for translations                                                                 |
| apiKey                      | Yes      |               | The API key to use                                                                                          |
| autoLoadValidPairs          | Yes      |               | Whether to auto-load all valid language pairs. Will also happen if _validateDefaultLanguagePair_ is enabled |
| defaultFromLanguage         | No       | "eng"         | The default language to translate text from when calling the Translate commands with no fromLanguage        |
| defaultToLanguage           | No       | "spa"         | The default language to translate text to when calling the Translate commands with no toLanguage            |
| validateDefaultLanguagePair | No       | false         | Whether to validate the defaultFrom and defaultTo language pair. Will auto load all valid pairs             |

# Methods Available on the Client

**NOTE: Synchronous (`[MethodName]`) and asynchronous (`[MethodName]sync`) of the methods are available**

## GetValidFromLanguages

Gets a list of languages that the provided `toLanguage` can be translated from. As an example, this provides a way to
check if we can translate from English (the "from" language) to Spanish (the "to" language), or if we can translate from
French to Spanish.

### Variants

- `GetValidFromLanguages(string toLanguage, bool forceRefresh = false)`: synchronous version
- `GetValidFromLanguagesAsync(string toLanguage, bool forceRefresh = false)`: asynchronous version

### Parameters

| Parameter    | Type   | Required | Description                                                               |
|--------------|--------|----------|---------------------------------------------------------------------------|
| toLanguage   | string | Yes      | The language we are trying to translate "to" (e.g., "spa" for Spanish)    |
| forceRefresh | bool   | No       | If true, forces a refresh of the valid languages cache. Defaults to false |

### Returns

`List<string>`: A list of languages that the configured Apertium server supports translating from the specified
`toLanguage`.

## GetValidLanguagePairs

Gets a hashset of all valid language pairs that the configured Apertium server supports. This is useful for checking if
a specific language pair is supported before attempting a translation.

### Variants

- `GetValidLanguagePairs(bool forceRefresh = false)`: synchronous version
- `GetValidLanguagePairsAsync(bool forceRefresh = false)`: asynchronous version

### Parameters

| Parameter    | Type   | Required | Description                                                               |
|--------------|--------|----------|---------------------------------------------------------------------------|
| forceRefresh | bool   | No       | If true, forces a refresh of the valid languages cache. Defaults to false |

### Returns

`HashSet<(string, string)>`: A hashset of tuples representing valid language pairs, where each tuple contains the "from"
and "to" languages (e.g., `("eng", "spa")` for English to Spanish).

## GetValidToLanguages

Gets a list of languages that the provided `fromLanguage` can be translated to. As an example, this provides a way to
check if we can translate from English (the "from" language) to Spanish (the "to" language), or if we can translate from
French to Spanish.

### Variants

- `GetValidToLanguages(string fromLanguage, bool forceRefresh = false)`: synchronous version
- `GetValidToLanguagesAsync(string fromLanguage, bool forceRefresh = false)`: asynchronous version

### Parameters

| Parameter    | Type   | Required | Description                                                               |
|--------------|--------|----------|---------------------------------------------------------------------------|
| fromLanguage | string | Yes      | The language we are trying to translate "from" (e.g., "spa" for Spanish)  |
| forceRefresh | bool   | No       | If true, forces a refresh of the valid languages cache. Defaults to false |

### Returns

`List<string>`: A list of languages that the configured Apertium server supports translating to from the specified
`fromLanguage`.

## IsValidFromLanguage

Checks if the provided `language` has any translations available on the configured Apertium server. This is useful for
checking if a specific language can be used as a "from" language in _any_ translation.

### Variants

- `IsValidFromLanguage(string language, bool forceRefresh = false)`: synchronous version
- `IsValidFromLanguageAsync(string language, bool forceRefresh = false)`: asynchronous version

### Parameters

| Parameter    | Type   | Required | Description                                                                                             |
|--------------|--------|----------|---------------------------------------------------------------------------------------------------------|
| language     | string | Yes      | The language we are checking for whether there are any translations available (e.g., "spa" for Spanish) |
| forceRefresh | bool   | No       | If true, forces a refresh of the valid languages cache. Defaults to false                               |

### Returns

`bool`: Returns true if the specified `language` can be used as a "from" language in any translation.

## IsValidPair

Checks if the provided `fromLanguage` and `toLanguage` pair is valid for translation on the configured Apertium server.
This is useful for checking if a specific language pair is supported before attempting a translation, although it is
configured to be checked automatically when calling the `Translate` methods.

### Variants

- `IsValidPair(string fromLanguage, string toLanguage, bool forceRefresh = false)`: synchronous version
- `IsValidPairAsync(string fromLanguage, string toLanguage, bool forceRefresh = false)`: asynchronous version

### Parameters

| Parameter    | Type   | Required | Description                                                               |
|--------------|--------|----------|---------------------------------------------------------------------------|
| fromLanguage | string | Yes      | The language we are trying to translate "from" (e.g., "eng" for English)  |
| toLanguage   | string | Yes      | The language we are trying to translate "to" (e.g., "spa" for Spanish)    |
| forceRefresh | bool   | No       | If true, forces a refresh of the valid languages cache. Defaults to false |

### Returns

`bool`: Returns true if the language pair is valid for translation on the configured Apertium server.

## IsValidToLanguage

Checks if the provided `language` has any translations available on the configured Apertium server. This is useful for
checking if a specific language can be used as a "to" language in _any_ translation.

### Variants

- `IsValidToLanguage(string language, bool forceRefresh = false)`: synchronous version
- `IsValidToLanguageAsync(string language, bool forceRefresh = false)`: asynchronous version

### Parameters

| Parameter    | Type   | Required | Description                                                               |
|--------------|--------|----------|---------------------------------------------------------------------------|
| language     | string | Yes      | The language we are trying to translate "to" (e.g., "spa" for Spanish)    |
| forceRefresh | bool   | No       | If true, forces a refresh of the valid languages cache. Defaults to false |

### Returns

`bool`: Returns true if the specified `language` can be used as a "to" language in any translation.

## Translate

Attempts to translate the provided `text` from the specified `fromLanguage` to the specified `toLanguage`.

### Variants

#### Base Translate

- `Translate(string text)`: synchronous version using the default language pair configured for the client
- `TranslateAsync(string text)`: asynchronous version using the default language pair configured for the client

#### With ToLanguage

- `Translate(string text, string toLanguage)`: synchronous version specifying the "to" language
- `TranslateAsync(string text, string toLanguage)`: asynchronous version specifying the "to" language

#### With FromLanguage and ToLanguage

- `Translate(string text, string fromLanguage, string toLanguage)`: synchronous version specifying both "from" and "to"
    languages
  - `TranslateAsync(string text, string fromLanguage, string toLanguage)`: asynchronous version specifying both "from" and
  "to" languages

### Parameters

#### Base Translate

| Parameter | Type   | Required | Description                                  |
|-----------|--------|----------|----------------------------------------------|
| text      | string | Yes      | The text we are attempting to get translated |

#### With ToLanguage

| Parameter  | Type   | Required | Description                                                            |
|------------|--------|----------|------------------------------------------------------------------------|
| text       | string | Yes      | The text we are attempting to get translated                           |
| toLanguage | string | Yes      | The language we are trying to translate "to" (e.g., "spa" for Spanish) |

#### With FromLanguage and ToLanguage

| Parameter    | Type   | Required | Description                                                              |
|--------------|--------|----------|--------------------------------------------------------------------------|
| text         | string | Yes      | The text we are attempting to get translated                             |
| fromLanguage | string | Yes      | The language we are trying to translate "from" (e.g., "eng" for English) |
| toLanguage   | string | Yes      | The language we are trying to translate "to" (e.g., "spa" for Spanish)   |

### Returns

`bool`: Returns true if the specified `language` can be used as a "to" language in any translation.

## SetHttpClient

Sets a custom `HttpClient` to be used by the `ApertiumClient`. This is useful if you need to configure the client with
custom headers, authentication, or other settings.

### Variants

n/a

### Parameters

| Parameter | Type         | Required | Description                                                                 |
|-----------|--------------|----------|-----------------------------------------------------------------------------|
| httpClient | HttpClient | Yes      | The custom `HttpClient` to be used by the `ApertiumClient` for making requests |

### Returns

n/a

# Future Plans

See list of issues under the Milestones: https://github.com/vonderborch/Apertium.Net/milestones

# Apertium

https://github.com/apertium

I have no association with the fine developers of Apertium. Please make sure that your project makes sure to provide
proper attribution to them!
