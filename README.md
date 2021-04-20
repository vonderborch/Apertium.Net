# Apertium.Net
A simple library to help with using [Apertium](https://apertium.org/) for machine translation.

# Installation
A nuget package is available: [Apertium.Net](https://www.nuget.org/packages/Apertium.Net/)

# Basic Usage
```
var client = new ApertiumClient();

var translatedText = client.Translate("Hello World!", "eng", "spa");
```

# Configuring the Client
When creating a new `ApertiumClient` a number of parameters are available to configure it:

Parameter | Required/Optional | Default Value | Description
--------- | ----------------- | ------------- | -----------
baseApiUrl | Optional | "https://www.apertium.org/apy/" | The Apertium server to use for translations
apiKey | Optional | _null_ | The API key to use
autoLoadValidPairs | Optional | false | Whether to auto-load all valid language pairs. Will also happen if _validateDefaultLanguagePair_ is enabled
defaultFromLanguage | Optional | "eng" | The default language to translate text from when calling the Translate commands with no fromLanguage
defaultToLanguage | Optional | "spa" | The default language to translate text to when calling the Translate commands with no toLanguage
validateDefaultLanguagePair | Optional | false | Whether to validate the defaultFrom and defaultTo language pair. Will auto load all valid pairs

# Methods Available on the Client
_Synchronous (MethodName) and asynchronous (MethodNameAsync) of most metehods are available_
- Translate: Translates a specific text from a particular language to another language
- ListPairs: Returns a JsonArray representing all language pairs supported by the configured Apertium server
- GetValidPairs: Returns a HashSet representing all valid language pairs supported by the configured Apertium server. Item1 in each tuple is the from langauge and Item2 is the to language
- IsValidPair: Returns a boolean true/false if the provided language pair is supported by the configured Apertium server
- GetValidToLanguages: Returns back a list of all languages the configured Apertium server supports translating a particular langauge to
- GetValidFromLanguages: Returns back a list of all languages the configured Apertium server supports translating a particular langauge from
- UpdateDefaultLanguages: Updates the default languages used if none are provided in the Translate command
- ClearValidPairsCache: Clears the valid language pairs cache
- UpdateApertiumServer: Updates the client's configured Apertium server

# Future Plans
See list of issues under the Milestones: https://github.com/vonderborch/Apertium.Net/milestones

# Apertium
https://github.com/apertium

I have no association with the fine developers of Apertium. Please make sure that your project makes sure to provide proper attribution to them!
