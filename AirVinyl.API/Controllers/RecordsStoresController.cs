using AirVinyl.API.Helpers;
using AirVinyl.DataAccessLayer;
using AirVinyl.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;

namespace AirVinyl.API.Controllers
{
    public class RecordStoresController : ODataController
    {
        private AirVinylDbContext _ctx = new AirVinylDbContext();

        [EnableQuery]
        public IHttpActionResult Get()
        {
            return Ok(_ctx.RecordStores);
        }

        [EnableQuery]
        public IHttpActionResult Get([FromODataUri] int key)
        {
            var recordstore = _ctx.RecordStores.Where(p => p.RecordStoreId == key);

            if (!recordstore.Any())
            {
                return NotFound();
            }

            return Ok(SingleResult.Create(recordstore));
        }

        [HttpGet]
        [EnableQuery]
        [ODataRoute("RecordStores/AirVinyl.Model.SpecializedRecordStore")]
        public IHttpActionResult GetSpecializedSores()
        {
            var specializedSotres = _ctx.RecordStores.Where(r => r is SpecializedRecordStore);

            return Ok(specializedSotres.Select(s => s as SpecializedRecordStore));
        }

        [HttpGet]
        [EnableQuery]
        [ODataRoute("RecordStores({key})/AirVinyl.Model.SpecializedRecordStore")]
        public IHttpActionResult GetSpecializedSore([FromODataUri] int key)
        {
            var specializedSotres = _ctx.RecordStores
                .Where(r => r is SpecializedRecordStore && r.RecordStoreId == key);

            if (!specializedSotres.Any())
            {
                return NotFound();
            }

            return Ok(SingleResult.Create(specializedSotres.Select(s => s as SpecializedRecordStore)));
        }

        [HttpGet]
        [EnableQuery]
        [ODataRoute("RecordStores({key})/Tags")]
        public IHttpActionResult GetPersonProperty([FromODataUri] int key)
        {
            var recordstore = _ctx.RecordStores.FirstOrDefault(p => p.RecordStoreId == key);

            if (recordstore == null)
            {
                return NotFound();
            }

            var propertyToGet = Url.Request.RequestUri.Segments.Last();

            if (!recordstore.HasProperty(propertyToGet))
            {
                return NotFound();
            }

            var propertyValue = recordstore.GetValue(propertyToGet);

            if (propertyValue == null)
            {
                return StatusCode(HttpStatusCode.NoContent);
            }

            return this.CreateOKHttpActionResult(propertyValue);
        }

        [HttpPost]
        [ODataRoute("RecordStores")] //to pass child class use "@odata.type":"#AirVinyl.Model.SpecializedRecordStore",
        public IHttpActionResult CreateRecordStore(RecordStore store)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //add the recordstore
            _ctx.RecordStores.Add(store);
            _ctx.SaveChanges();

            //return the created store
            return Created(store);
        }

        [HttpPost]
        [ODataRoute("RecordStores({key})/AirVinyl.Actions.Rate")]
        public IHttpActionResult Rate([FromODataUri] int key, ODataActionParameters parameters)
        {
            var recordStore = _ctx.RecordStores.FirstOrDefault(r => r.RecordStoreId == key);

            if(recordStore == null)
            {
                return NotFound();
            }

            //from the param dictionary, get the rating & personId
            int rating, personId;
            object outputFromDictionary;

            if (!parameters.TryGetValue("rating", out outputFromDictionary))
            {
                return NotFound();
            }

            if (!int.TryParse(outputFromDictionary.ToString(),out rating))
            {
                return NotFound();
            }

            if (!int.TryParse(outputFromDictionary.ToString(), out personId))
            {
                return NotFound();
            }

            //the person must exist
            var person = _ctx.People
                .FirstOrDefault(p => p.PersonId == personId);

            if(person == null)
            {
                return NotFound();
            }

            //everything checks out, add the rating
            recordStore.Ratings.Add(new Model.Rating() { RatedBy = person, Value = rating });

            //save changes
            if(_ctx.SaveChanges() > -1)
            {
                //return true
                return this.CreateOKHttpActionResult(true);
            }
            else
            {
                //something went wrong - we expect our
                //action to return false ini that case.
                //the request is still successful, false
                //is a valid response
                return this.CreateOKHttpActionResult(false);
            }
        }

        [HttpPost]
        [ODataRoute("RecordStores/AirVinyl.Actions.RemoveRatings")]
        public IHttpActionResult RemoveRatings(ODataActionParameters parameters)
        {
            //from the param dictionary, get the rating & personId
            int personId;
            object outputFromDictionary;

            if (!parameters.TryGetValue("personId", out outputFromDictionary))
            {
                return NotFound();
            }

            if (!int.TryParse(outputFromDictionary.ToString(), out personId))
            {
                return NotFound();
            }

            //get the recordStores that were rated by the person with personId
            var recordStoresRatedByCurrentPerson = _ctx.RecordStores
                .Include("Ratings")
                .Include("Ratings.RatedBy")
                .Where(p => p.Ratings.Any(r => r.RatedBy.PersonId == personId)).ToList();

            //remove those ratings
            foreach (var store in recordStoresRatedByCurrentPerson)
            {
                //get the ratings by the current person
                var ratingByCurrentPerson = store.Ratings
                    .Where(r => r.RatedBy.PersonId == personId).ToList();

                for(int i =0; i < ratingByCurrentPerson.Count; i++)
                {
                    store.Ratings.Remove(ratingByCurrentPerson[i]);
                }
            }

            //save changes
            if (_ctx.SaveChanges() > -1)
            {
                //return true
                return this.CreateOKHttpActionResult(true);
            }
            else
            {
                //something went wrong - we expect our
                //action to return false ini that case.
                //the request is still successful, false
                //is a valid response
                return this.CreateOKHttpActionResult(false);
            }
        }

        [HttpPost]
        [ODataRoute("RemoveRecordStoreRatings")]
        public IHttpActionResult RemoveRecordStoreRatings(ODataActionParameters parameters)
        {
            //from the param dictionary, get the rating & personId
            int personId;
            object outputFromDictionary;

            if (!parameters.TryGetValue("personId", out outputFromDictionary))
            {
                return NotFound();
            }

            if (!int.TryParse(outputFromDictionary.ToString(), out personId))
            {
                return NotFound();
            }

            //get the recordStores that were rated by the person with personId
            var recordStoresRatedByCurrentPerson = _ctx.RecordStores
                .Include("Ratings")
                .Include("Ratings.RatedBy")
                .Where(p => p.Ratings.Any(r => r.RatedBy.PersonId == personId)).ToList();

            //remove those ratings
            foreach (var store in recordStoresRatedByCurrentPerson)
            {
                //get the ratings by the current person
                var ratingByCurrentPerson = store.Ratings
                    .Where(r => r.RatedBy.PersonId == personId).ToList();

                for (int i = 0; i < ratingByCurrentPerson.Count; i++)
                {
                    store.Ratings.Remove(ratingByCurrentPerson[i]);
                }
            }

            //save changes
            if (_ctx.SaveChanges() > -1)
            {
                //return true
                return StatusCode(HttpStatusCode.OK);
            }
            else
            {
                //something went wrong - return internal server error
                return StatusCode(HttpStatusCode.InternalServerError);
            }
        }

        [HttpPatch]
        [ODataRoute("RecordStores({key})")]
        [ODataRoute("RecordStores({key})/AirVinyl.Model.SpecializedRecordStore")]
        public IHttpActionResult PatchRecordStore([FromODataUri] int key, Delta<RecordStore> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //find matchin recordStore
            var currentRecordStore = _ctx.RecordStores.FirstOrDefault(v => v.RecordStoreId == key);

            //return NotFound if the recordStore is not found
            if (currentRecordStore == null)
            {
                return NotFound();
            }

            //apply patch
            patch.Patch(currentRecordStore);
            _ctx.SaveChanges();

            //return noContent
            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpDelete]
        [ODataRoute("RecordStores({key})")]
        [ODataRoute("RecordStores({key})/AirVinyl.Model.SpecializedRecordStore")]
        public IHttpActionResult DeleteVinylRecordForPerson([FromODataUri] int key)
        {
            //does the recordStore exists?
            var recordStore = _ctx.RecordStores.Include("Ratings")
                .FirstOrDefault(p => p.RecordStoreId == key);

            if (recordStore == null)
            {
                return NotFound();
            }

            //apply delete
            recordStore.Ratings.Clear();
            _ctx.RecordStores.Remove(recordStore);
            _ctx.SaveChanges();

            //return no content
            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            _ctx.Dispose();
            base.Dispose(disposing);
        }
    }
}