using AirVinyl.API.Helpers;
using AirVinyl.DataAccessLayer;
using AirVinyl.Model;
using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;

namespace AirVinyl.API.Controllers
{
    public class PeopleController : ODataController
    {
        private AirVinylDbContext _ctx = new AirVinylDbContext();

        [EnableQuery]
        public IHttpActionResult Get()
        {
            return Ok(_ctx.People);
        }

        [EnableQuery]
        public IHttpActionResult Get([FromODataUri]int key)
        {
            var person = _ctx.People.FirstOrDefault(p => p.PersonId == key);

            if (person == null)
            {
                return NotFound();
            }
            //return the collection
            return Ok(_ctx.VinylRecords.Include("DynamicVinylRecordProperties")
                .Where(v => v.Person.PersonId == key));
        }

        [HttpGet]
        [EnableQuery]
        [ODataRoute("People({key})/VinylRecords({vinylRecordKey})")]
        public IHttpActionResult GetVinylRecordForPerson([FromODataUri] int key, int vinylRecordKey)
        {
            var person = _ctx.People.FirstOrDefault(p => p.PersonId == key);

            if (person == null)
            {
                return NotFound();
            }

            //Queryable, no first or default
            var vinylRecords = _ctx.VinylRecords.Include("DynamicVinylRecordProperties")
                .Where(v => v.Person.PersonId == key && v.VinylRecordId == vinylRecordKey);

            if (!vinylRecords.Any())
            {
                return NotFound();
            }

            return Ok(SingleResult.Create(vinylRecords));
        }

        [HttpPost]
        [ODataRoute("People({key})/VinylRecords")]
        public IHttpActionResult PostVinylRecordForPerson([FromODataUri] int key, VinylRecord vinylRecord)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //does the person exists?
            var person = _ctx.People.FirstOrDefault(p => p.PersonId == key);

            if (person == null)
            {
                return NotFound();
            }

            //link the person to the VinylRecord(also avoids an invalid person)
            //key on the passed in record key from the URI wins
            vinylRecord.Person = person;

            //add the vinyl record
            _ctx.VinylRecords.Add(vinylRecord);
            _ctx.SaveChanges();

            //return the created VinylRecord
            return Created(vinylRecord);
        }

        [HttpPatch]
        [ODataRoute("People({key})/VinylRecords({vinylRecordKey})")]
        public IHttpActionResult PatchVinylRecordForPerson([FromODataUri] int key,[FromODataUri] int vinylRecordKey
            ,Delta<VinylRecord> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //does the person exists?
            var person = _ctx.People.FirstOrDefault(p => p.PersonId == key);

            if (person == null)
            {
                return NotFound();
            }

            //find matchin vinyl record
            var currentVinylRecord = _ctx.VinylRecords.FirstOrDefault(v => v.VinylRecordId == vinylRecordKey
            && v.Person.PersonId == key);

            //return NotFound if the vinylRecord is not found
            if(currentVinylRecord == null)
            {
                return NotFound();
            }

            //apply patch
            patch.Patch(currentVinylRecord);
            _ctx.SaveChanges();
          
            //return the created VinylRecord
            return StatusCode(HttpStatusCode.NoContent);
        }


        [HttpDelete]
        [ODataRoute("People({key})/VinylRecords({vinylRecordKey})")]
        public IHttpActionResult DeleteVinylRecordForPerson([FromODataUri] int key, [FromODataUri] int vinylRecordKey)
        {

            //does the person exists?
            var person = _ctx.People.FirstOrDefault(p => p.PersonId == key);

            if (person == null)
            {
                return NotFound();
            }

            //find matchin vinyl record
            var currentVinylRecord = _ctx.VinylRecords.FirstOrDefault(v => v.VinylRecordId == vinylRecordKey
            && v.Person.PersonId == key);

            //return NotFound if the vinylRecord is not found
            if (currentVinylRecord == null)
            {
                return NotFound();
            }

            //apply patch
            _ctx.VinylRecords.Remove(currentVinylRecord);
            _ctx.SaveChanges();

            //return the created VinylRecord
            return StatusCode(HttpStatusCode.NoContent);
        }


        [HttpGet]
        [ODataRoute("People({key})/Email")]
        [ODataRoute("People({key})/FirstName")]
        [ODataRoute("People({key})/LastName")]
        [ODataRoute("People({key})/DateOfBirth")]
        [ODataRoute("People({key})/Gender")]
        public IHttpActionResult GetPersonProperty([FromODataUri] int key)
        {
            var person = _ctx.People.FirstOrDefault(p => p.PersonId == key);

            if (person == null)
            {
                return NotFound();
            }

            var propertyToGet = Url.Request.RequestUri.Segments.Last();

            if (!person.HasProperty(propertyToGet))
            {
                return NotFound();
            }

            var propertyValue = person.GetValue(propertyToGet);

            if(propertyValue == null)
            {
                return StatusCode(HttpStatusCode.NoContent);
            }

            return this.CreateOKHttpActionResult(propertyValue);
        }

        [HttpGet]
        [ODataRoute("People({key})/Friends")]
        [ODataRoute("People({key})/VinylRecords")]
        public IHttpActionResult GetPersonColletionProperty([FromODataUri] int key)
        {
            var collectionPropertyToGet = Url.Request.RequestUri.Segments.Last();
            var person = _ctx.People.Include(collectionPropertyToGet)
                .FirstOrDefault(p => p.PersonId == key);

            if (person == null)
            {
                return NotFound();
            }

            var collectionPropertyValue = person.GetValue(collectionPropertyToGet);

            return this.CreateOKHttpActionResult(collectionPropertyValue);
        }

        [HttpPost]
        public IHttpActionResult Post(Person person)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _ctx.People.Add(person);
            _ctx.SaveChanges();

            return Created(person);
        }

        [HttpPut]
        public IHttpActionResult Put([FromODataUri]int key, Person person)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentPerson =_ctx.People.FirstOrDefault(c => c.PersonId == key);

            if (currentPerson == null)
            {
                return NotFound();
            }

            person.PersonId = currentPerson.PersonId;

            _ctx.Entry(currentPerson).CurrentValues.SetValues(person);
            _ctx.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpPatch]//must be the prefered mean to update
        public IHttpActionResult Patch([FromODataUri] int key, Delta<Person> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentPerson = _ctx.People.FirstOrDefault(c => c.PersonId == key);

            if (currentPerson == null)
            {
                return NotFound();
            }

            patch.Patch(currentPerson);
            _ctx.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpDelete]
        public IHttpActionResult Delete([FromODataUri] int key)
        {
            var currentPerson = _ctx.People.Include("Friends").FirstOrDefault(p => p.PersonId == key);

            if(currentPerson == null)
            {
                return NotFound();
            }

            var peopleWithCurrentPersonAsFriend = _ctx.People.Include("Friends")
                .Where(p => p.Friends.Select(f => f.PersonId).AsQueryable().Contains(key));

            foreach (var person in peopleWithCurrentPersonAsFriend.ToList())
            {
                person.Friends.Remove(currentPerson);
            }

            _ctx.People.Remove(currentPerson);
            _ctx.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpPost]
        [ODataRoute("People({key})/Friends/$ref")]
        public IHttpActionResult CreateLinkToFriend([FromODataUri]int key, [FromBody] Uri link)
        {
            var currentPerson = _ctx.People.Include("Friends").FirstOrDefault(p => p.PersonId == key);
            
            if(currentPerson == null)
            {
                return NotFound();
            }

            int keyOfFriendToAdd = Request.GetKeyValue<int>(link);

            if(currentPerson.Friends.Any(p => p.PersonId == keyOfFriendToAdd))
            {
                return BadRequest(string.Format("the person with id {0} is already linked to the person with id {1}", key, keyOfFriendToAdd));
            }

            var friendToLinkTo = _ctx.People.FirstOrDefault(p => p.PersonId == keyOfFriendToAdd);

            if(friendToLinkTo == null)
            {
                return NotFound();
            }

            currentPerson.Friends.Add(friendToLinkTo);
            _ctx.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpPut]
        [ODataRoute("People({key})/Friends({relatedKey})/$ref")]
        public IHttpActionResult UpdateLinkToFriend([FromODataUri] int key, [FromODataUri] int relatedKey,
            [FromBody] Uri link)
        {
            var currentePerson = _ctx.People.Include("Friends").FirstOrDefault(p => p.PersonId == key);

            if(currentePerson == null)
            {
                return NotFound();
            }

            var currentFriend = _ctx.People.FirstOrDefault(f => f.PersonId == relatedKey);

            if(currentFriend == null)
            {
                return NotFound();
            }

            int keyOfFriendToAdd = Request.GetKeyValue<int>(link);

            if (currentePerson.Friends.Any(p => p.PersonId == keyOfFriendToAdd))
            {
                return BadRequest(
                    string.Format("the person with id {0} is already linked to the person with id {1}", key, keyOfFriendToAdd));
            }

            var friendToLinkTo = _ctx.People.FirstOrDefault(p => p.PersonId == keyOfFriendToAdd);

            if(friendToLinkTo == null)
            {
                return NotFound();
            }

            currentePerson.Friends.Remove(currentFriend);
            currentePerson.Friends.Add(friendToLinkTo);
            _ctx.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpDelete]
        [ODataRoute("People({key})/Friends({relatedKey})/$ref")]
        public IHttpActionResult DeleteLinkToFriend([FromODataUri] int key, [FromODataUri] int relatedKey)
        {
            var currentPerson = _ctx.People.Include("Friends").FirstOrDefault(p => p.PersonId == key);

            if (currentPerson == null)
            {
                return NotFound();
            }

            var friend = currentPerson.Friends.FirstOrDefault(f => f.PersonId == relatedKey);

            if (friend == null)
            {
                return NotFound();
            }

            currentPerson.Friends.Remove(friend);
            _ctx.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            _ctx.Dispose();
            base.Dispose(disposing);
        }
    }
}