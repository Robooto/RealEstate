using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace RealEstate.Tests
{
    [TestClass]
    public class BsonDocumentTest
    {

        public BsonDocumentTest()
        {
            JsonWriterSettings.Defaults.Indent = true;
        }

        [TestMethod]
        public void EmptyDocument()
        {
            var document = new BsonDocument();

            Console.WriteLine(document);
        }

        [TestMethod]
        public void AddElements()
        {
            var person = new BsonDocument
            {
                {"firstName", new BsonString("bob")},
                {"age", new BsonInt32(54) },
                {"isAlive", true }
            };

            Console.WriteLine(person);
        }

        [TestMethod]
        public void AddingArrays()
        {
            var person = new BsonDocument
            {
                {"address", new BsonArray(new[] {"101 Some Road", "Unit Test Ave", "Portland"})}
            };

            Console.WriteLine(person);
        }

        [TestMethod]
        public void AddDocument()
        {
            var person = new BsonDocument
            {
                {
                    "contact", new BsonDocument
                    {
                        {"phone", "123-434-3434"},
                        {"email", "whatev@document.com"}
                    }
                }
            };

            Console.WriteLine(person);
        }

        [TestMethod]
        public void BsonValueConversions()
        {
            var person = new BsonDocument
            {
                {"age", new BsonInt32(54)}
            };

            Console.WriteLine(person["age"].ToInt32() + 10);
            Console.WriteLine(person["age"].IsInt32);
        }

        [TestMethod]
        public void ToBson()
        {
            var person = new BsonDocument
            {
                {"firstname", "bob"}
            };

            var bson = person.ToBson();
            Console.WriteLine(BitConverter.ToString(bson));

            var deserializedPerson = BsonSerializer.Deserialize<BsonDocument>(bson);
            Console.WriteLine(deserializedPerson);
        }
    }
}
