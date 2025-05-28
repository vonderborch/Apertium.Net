using NUnit.Framework;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq.Protected;

namespace Apertium.Net.Test;

[TestFixture]
public class TestAApertiumClient
{
    private Mock<AApertiumClient> _mockClient = null!;
    private const string DefaultFrom = "eng";
    private const string DefaultTo = "spa";

    [SetUp]
    public void Setup()
    {
        _mockClient = new Mock<AApertiumClient>(DefaultFrom, DefaultTo) { CallBase = true };
    }

    private void SetupMockLanguagePairs(HashSet<(string, string)> pairs)
    {
        _mockClient.Setup(c => c.GetValidLanguagePairsAsync(It.IsAny<bool>()))
                   .ReturnsAsync(pairs);

        var fromMapping = new Dictionary<string, List<string>>();
        var toMapping = new Dictionary<string, List<string>>();
        foreach (var (from, to) in pairs)
        {
            if (!fromMapping.ContainsKey(from))
            {
                fromMapping[from] = new List<string>();
            }
            fromMapping[from].Add(to);
            if (!toMapping.ContainsKey(to))
            {
                toMapping[to] = new List<string>();
            }
            toMapping[to].Add(from);
        }

        _mockClient.Object.GetType().GetField("validLanguagePairs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(_mockClient.Object, new HashSet<(string, string)>(pairs));
        _mockClient.Object.GetType().GetField("fromLanguageToLanguageMapping", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(_mockClient.Object, fromMapping);
        _mockClient.Object.GetType().GetField("toLanguageFromLanguageMapping", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(_mockClient.Object, toMapping);
    }

    [Test]
    public void Constructor_SetsDefaultLanguages()
    {
        Assert.That(_mockClient.Object.DefaultFromLanguage, Is.EqualTo(DefaultFrom));
        Assert.That(_mockClient.Object.DefaultToLanguage, Is.EqualTo(DefaultTo));
    }

    [Test]
    public async Task GetValidFromLanguagesAsync_ReturnsCorrectLanguages()
    {
        var pairs = new HashSet<(string, string)> { ("eng", "spa"), ("fra", "spa") };
        SetupMockLanguagePairs(pairs);

        var fromLanguages = await _mockClient.Object.GetValidFromLanguagesAsync("spa");

        Assert.That(fromLanguages.Count, Is.EqualTo(2));
        Assert.That(fromLanguages, Does.Contain("eng"));
        Assert.That(fromLanguages, Does.Contain("fra"));
    }

    [Test]
    public async Task IsValidPairAsync_ReturnsCorrectly()
    {
        var pairs = new HashSet<(string, string)> { ("eng", "spa") };
        SetupMockLanguagePairs(pairs);

        Assert.That(await _mockClient.Object.IsValidPairAsync("eng", "spa"), Is.True);
        Assert.That(await _mockClient.Object.IsValidPairAsync("eng", "fra"), Is.False);
    }

    [Test]
    public async Task TranslateAsync_DefaultLanguages_CallsAbstractTranslate()
    {
        var text = "hello";
        var translatedText = "hola";
        _mockClient.Protected().Setup<Task<string>>("InternalTranslateAsync", text, DefaultFrom, DefaultTo)
            .ReturnsAsync(translatedText);
        this._mockClient.Setup<Task<bool>>(x => x.IsValidPairAsync(DefaultFrom, DefaultTo, false))
            .ReturnsAsync(true);

        var result = await _mockClient.Object.TranslateAsync(text);

        Assert.That(result, Is.EqualTo(translatedText));
    }
}
