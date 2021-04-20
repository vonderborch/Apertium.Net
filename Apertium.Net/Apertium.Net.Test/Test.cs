using NUnit.Framework;

namespace Apertium.Net.Test
{
    class ApertiumTest
    {

        private ApertiumClient _client;

        [SetUp]
        public void Setup()
        {
            _client = new ApertiumClient();
        }

        [Test]
        public void TestTranslateGood()
        {
            var results = _client.Translate("Hello World!", "eng", "spa");

            Assert.AreEqual("Hola Mundo!", results);
        }

        [Test]
        public void Test()
        {
            var get = _client.GetValidPairs();

            if (true) ;
        }
    }
}
