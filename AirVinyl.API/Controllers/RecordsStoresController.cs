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
            var recordstore = _ctx.RecordStores.FirstOrDefault(p => p.RecordStoreId == key);

            if (recordstore == null)
            {
                return NotFound();
            }

            return Ok(recordstore);
        }

        [HttpGet]
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

        protected override void Dispose(bool disposing)
        {
            _ctx.Dispose();
            base.Dispose(disposing);
        }
    }
}