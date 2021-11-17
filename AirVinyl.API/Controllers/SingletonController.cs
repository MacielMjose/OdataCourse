using AirVinyl.API.Helpers;
using AirVinyl.DataAccessLayer;
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
    public class SingletonController : ODataController
    {
        private AirVinylDbContext _ctx = new AirVinylDbContext();

        [HttpGet]
        [ODataRoute("Tim")]
        public IHttpActionResult GetSingletonTim()
        {
            var personTim = _ctx.People.FirstOrDefault(p => p.PersonId == 6 && p.FirstName == "Tim");

            return Ok(personTim);
        }

        [HttpGet]
        [ODataRoute("Tim/Email")]
        [ODataRoute("Tim/FirstName")]
        [ODataRoute("Tim/LastName")]
        [ODataRoute("Tim/DateOfBirth")]
        [ODataRoute("Tim/Gender")]
        public IHttpActionResult GetTimProperty()
        {
            var personTIm = _ctx.People.FirstOrDefault(p => p.PersonId == 6);

            if (personTIm == null)
            {
                return NotFound();
            }

            var propertyToGet = Url.Request.RequestUri.Segments.Last();

            if (!personTIm.HasProperty(propertyToGet))
            {
                return NotFound();
            }

            var propertyValue = personTIm.GetValue(propertyToGet);

            if (propertyValue == null)
            {
                return StatusCode(HttpStatusCode.NoContent);
            }

            return this.CreateOKHttpActionResult(propertyValue);
        }

        [HttpGet]
        [ODataRoute("Tim/Friends")]
        [ODataRoute("Tim/VinylRecords")]
        public IHttpActionResult GetPersonTimColletionProperty()
        {
            var collectionPropertyToGet = Url.Request.RequestUri.Segments.Last();
            var personTim = _ctx.People.Include(collectionPropertyToGet)
                .FirstOrDefault(p => p.PersonId == 6);

            if (personTim == null)
            {
                return NotFound();
            }

            var collectionPropertyValue = personTim.GetValue(collectionPropertyToGet);

            return this.CreateOKHttpActionResult(collectionPropertyValue);
        }

        protected override void Dispose(bool disposing)
        {
            _ctx.Dispose();
            base.Dispose(disposing);
        }

    }
}