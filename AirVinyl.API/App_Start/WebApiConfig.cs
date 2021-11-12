﻿using AirVinyl.Model;
using Microsoft.OData.Edm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;

namespace AirVinyl.API
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapODataServiceRoute("ODataRoute", "odata", GetEdmModel());
            config.EnsureInitialized();
        }

        private static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.Namespace = "AirVinyl";
            builder.ContainerName = builder.Namespace + "Container";

            builder.EntitySet<Person>("People");
            builder.EntitySet<VinylRecord>("VinylRecords");

            return builder.GetEdmModel();
        }
    }
}
