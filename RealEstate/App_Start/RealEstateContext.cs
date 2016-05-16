using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RealEstate.Properties;
using RealEstate.Rentals;

namespace RealEstate.App_Start
{
    public class RealEstateContext
    {
        public IMongoDatabase Database;

        public RealEstateContext()
        {
            var connectionString = Settings.Default.RealEstateConnectionString;
            var settings = MongoClientSettings.FromUrl(new MongoUrl(connectionString));
            settings.ClusterConfigurator = builder => builder.Subscribe(new Log4NetMongoEvnets());
            var client = new MongoClient(settings);
            Database = client.GetDatabase(Settings.Default.RealEstateDatabaseName);
        }

        public IMongoCollection<Rental> Rentals
        {
            get { return Database.GetCollection<Rental>("rentals"); }
        }
    }
}