using AirVinyl.Model;
using Microsoft.OData.Edm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.OData.Batch;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;

namespace AirVinyl.API
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            
            config.MapODataServiceRoute("ODataRoute", "odata", GetEdmModel()
                ,new DefaultODataBatchHandler(GlobalConfiguration.DefaultServer));
            config.EnableCors();
            config.EnsureInitialized();
        }

        private static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.Namespace = "AirVinyl";
            builder.ContainerName = builder.Namespace + "Container";

            builder.EntitySet<Person>("People");
            //builder.EntitySet<VinylRecord>("VinylRecords"); Contained in person only
            builder.EntitySet<RecordStore>("RecordStores");

            //Actions - can have side effects

            var rateAction = builder.EntityType<RecordStore>().Action("Rate");
            rateAction.Returns<bool>();
            rateAction.Parameter<int>("rating");
            rateAction.Parameter<int>("personId");
            rateAction.Namespace = "AirVinyl.Actions";

            var removeRatingAction = builder.EntityType<RecordStore>().Collection.Action("RemoveRatings");
            removeRatingAction.Returns<bool>();
            removeRatingAction.Parameter<int>("personId");
            removeRatingAction.Namespace = "AirVinyl.Actions";

            var removeRecordStoreRatingsAction = builder.Action("RemoveRecordStoreRatings");
            removeRecordStoreRatingsAction.Parameter<int>("personId");
            removeRatingAction.Namespace = "AirVinyl.Actions";

            //singletons - Tim
            builder.Singleton<Person>("Tim");

            return builder.GetEdmModel();
        }
    }
}
