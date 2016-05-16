using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;

namespace RealEstate.Rentals
{
    public class QueryPriceDistribution
    {
        public IAggregateFluent<BsonDocument> Run(IMongoCollection<Rental> rentals)
        {
            var priceRange = new BsonDocument(
                "$subtract",
                new BsonArray
                {
                    "$Price",
                    new BsonDocument(
                        "$mod",
                        new BsonArray {"$Price", 500})
                });
            var grouping = new BsonDocument(
                "$group",
                new BsonDocument
                {
                    {"_id", priceRange},
                    {"count", new BsonDocument("$sum",1)}
                });
            var sort = new BsonDocument(
                "$sort",
                new BsonDocument("_id", 1)
                );
            var args = new AggregateArgs
            {
                Pipeline = new[] {grouping, sort}
            };

            return rentals.Aggregate().Group(grouping).Sort(sort);
        }

        public IEnumerable RunAggregationFluent(IMongoCollection<Rental> rentals)
        {
            //fluent aggreator
            var distributions = rentals.Aggregate()
                .Project(r => new {r.Price, PriceRange = (double)r.Price - ((double)r.Price % 500) })
                .Group(r => r.PriceRange, g => new {  GroupPriceRange = g.Key, Count = g.Count() })
                .SortBy( r => r.GroupPriceRange)
                .ToList();

            return distributions;
        }

        public IEnumerable RunAggregationLinq(IMongoCollection<Rental> rentals)
        {
            var distributions = rentals.AsQueryable()
                .Select(r => new { r.Price, PriceRange = (double)r.Price - ((double)r.Price % 500) })
                .GroupBy(r => r.PriceRange)
                .Select(g => new { GroupPriceRange = g.Key, Count = g.Count() })
                .OrderBy(r => r.GroupPriceRange)
                .ToList();

            return distributions;
        }
    }
}