using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using RealEstate.Rentals;

namespace RealEstate.Tests.Rentals
{
    [TestClass]
    public class RentalsTests
    {
        [TestMethod]
        public void ToDocument_RentalWithPrice_PriceRepresentedAsDouble()
        {
            // Arrage
            var rental = new Rental {Price = 1};

            //Act
            var result = rental.ToBsonDocument();

            //Assert
            Assert.AreEqual(result["Price"].BsonType, BsonType.Double);
        }

        [TestMethod]
        public void ToDocument_RentalWithAnId_IdIsRepresentedAsAnObjectId()
        {
            // Arragne
            var rental = new Rental()
            {
                Id = ObjectId.GenerateNewId().ToString()
            };

            // Act
            var result = rental.ToBsonDocument();

            // Assert
            Assert.AreEqual(result["_id"].BsonType, BsonType.ObjectId);
        }
    }
}
