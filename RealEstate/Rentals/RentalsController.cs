using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.GridFS;
using MongoDB.Driver.Linq;
using RealEstate.App_Start;

namespace RealEstate.Rentals
{
    public class RentalsController : Controller
    {
        public readonly RealEstateContext Context;

        public RentalsController()
        {
            Context = new RealEstateContext();
        }

        public async Task<ActionResult> Index(RentalsFilter filters)
        {
            var filterDefinition = filters.ToFilterDefinition();

            //Find way
            //var rentals = await Context.Rentals
            //    .Find(filterDefinition)
            //    .SortBy(r => r.Price)
            //    .ThenByDescending(r => r.NumberOfRooms)
            //    .ToListAsync();
            //Linq

            var rentals = await FilterRentals(filters)
                .OrderBy(r => r.Price)
                .ThenByDescending(r => r.NumberOfRooms)
                .ToListAsync();
            var model = new RentalsList
            {
                Rentals = rentals,
                Filters = filters
            };
            return View(model);
        }

        private IMongoQueryable<Rental> FilterRentals(RentalsFilter filters)
        {
            IMongoQueryable<Rental> rentals = Context.Rentals.AsQueryable();

            if (filters.MinimumRooms.HasValue)
            {
                rentals = rentals.Where(r => r.NumberOfRooms >= filters.MinimumRooms);
            }

            if (filters.PriceLimit.HasValue)
            {
                rentals = rentals.Where(r => r.Price <= filters.PriceLimit);
            }

            return rentals;
        } 

        public ActionResult Post()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Post(PostRental postRental)
        {
            var rental = new Rental(postRental);
            Context.Rentals.InsertOne(rental);
            return RedirectToAction("Index");
        }

        public ActionResult AdjustPrice(string id)
        {
            var rental = GetRental(id);
            return View(rental);
        }

        private Rental GetRental(string id)
        {
            //var query = Builders<Rental>.Filter.Eq("_id", ObjectId.Parse(id));
            var rental = Context.Rentals.Find(r => r.Id == id).FirstOrDefault();
            return rental;
        }

        [HttpPost]
        public ActionResult AdjustPrice(string id, AdjustPrice adjustPrice)
        {
            var rental = GetRental(id);
            rental.AdjustPrice(adjustPrice);

            var filter = Builders<Rental>.Filter.Eq("_id", ObjectId.Parse(id));
            var options = new UpdateOptions {IsUpsert = true};
            Context.Rentals.ReplaceOne(r => r.Id == id, rental, options);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult AdjustPriceUsingModification(string id, AdjustPrice adjustPrice)
        {
            var rental = GetRental(id);
            var adjustment = new PriceAdjustment(adjustPrice, rental.Price);
            var modificationupdate = Builders<Rental>.Update
                .Push(r => r.Adjustments, adjustment)
                .Set(r => r.Price, adjustPrice.NewPrice);
            Context.Rentals.UpdateOne(r => r.Id == id, modificationupdate);
            return RedirectToAction("Index");
        }

        public ActionResult Delete(string id)
        {
            Context.Rentals.DeleteOne(x => x.Id == id);
            return RedirectToAction("Index");
        }

        public string PriceDistribution()
        {
            return new QueryPriceDistribution()
                //.Run(Context.Rentals)
                //.RunAggregationFluent(Context.Rentals)
                .RunAggregationLinq(Context.Rentals)
                .ToJson();
        }

        public ActionResult AttachImage(string id)
        {
            var rental = GetRental(id);
            return View(rental);
        }

        [HttpPost]
        public async Task<ActionResult> AttachImage(string id, HttpPostedFileBase file)
        {
            var rental = GetRental(id);
            if (rental.HasImage())
            {
                DeleteImage(rental);
            }
            await StoreImageAsync(file, id);

            return RedirectToAction("Index");
        }

        private async Task StoreImageAsync(HttpPostedFileBase file, string rentalId)
        {          
            var bucket = new GridFSBucket(Context.Database);
            var options = new GridFSUploadOptions
            {
                Metadata = new BsonDocument("contentType", file.ContentType)
            };
            var imageId = await bucket.UploadFromStreamAsync(file.FileName, file.InputStream, options);
            SetRentalImageId(rentalId, imageId.ToString());
        }

        public ActionResult JoinPreLookup()
        {
            // Joining without $lookup
            var rentals = Context.Rentals.Find(new BsonDocument()).ToList();
            var rentalZips = rentals.Select(r => r.ZipCode).Distinct().ToArray();

            var zipsById = Context.Database.GetCollection<ZipCode>("zips")
                .Find(z => rentalZips.Contains(z.Id))
                .ToList()
                .ToDictionary(d => d.Id);

            var report = rentals
                .Select(r => new
                {
                    Rental = r,
                    ZipCode = r.ZipCode != null && zipsById.ContainsKey(r.ZipCode) ? zipsById[r.ZipCode] : null
                });

            return Content(report.ToJson(new JsonWriterSettings {OutputMode = JsonOutputMode.Strict}), "application/json");
        }

        public ActionResult JoinWithLookup()
        {

            var rentals = Context.Rentals
                .Aggregate()
                .Lookup<Rental, ZipCode, RentalWithZipCodes>(Context.Database.GetCollection<ZipCode>("zips"),
                    r => r.ZipCode,
                    z => z.Id,
                    w => w.ZipCodes
                )
                .ToList();

            return Content(rentals.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.Strict }), "application/json");
        }
    }

    public class RentalWithZipCodes : Rental
    {
        public ZipCode[] ZipCodes { get; set; }
    }
}