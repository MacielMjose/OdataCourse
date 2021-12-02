using AirVinyl.Model;
using AirVinyl.WebClient.Models;
using Microsoft.OData.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AirVinyl.WebClient.Controllers
{
    public class AirVinylController : Controller
    {
        // GET: AirVinyl
        public ActionResult Index()
        {
            var context = new AirVinylContainer(new Uri("https://localhost:44363/odata"));

            //var peopleResponse = context.People
            //    .IncludeTotalCount()
            //    .Expand(p => p.VinylRecords)
            //    .Execute() as QueryOperationResponse<Person>;

            //var peopleAsList = peopleResponse.ToList();

            //DataServiceQueryContinuation<Person> token = peopleResponse.GetContinuation();
            //peopleResponse = context.Execute(token);
            //peopleAsList = peopleResponse.ToList();

            //string aditionalData = "Total Count " + peopleResponse.TotalCount.ToString();

            //var personResponse = context.People
            //    .ByKey(1)
            //    .GetValue();

            //var peopleResponse = context.People
            //   .IncludeTotalCount()
            //   .Expand(p => p.VinylRecords)
            //   .Where(p => p.FirstName.EndsWith("n".ToLower()))
            //   .OrderByDescending(p => p.FirstName)
            //   .Skip(1)
            //   .Take(1);

            //var peopleAsList = peopleResponse.ToList();

            //var selectFromPeople = context.People.Select(p => new { p.FirstName, p.LastName });
            //string aditionalData = "";
            //foreach (var partialPerson in selectFromPeople)
            //{
            //    aditionalData += partialPerson.FirstName + " " + partialPerson.LastName + "\n";
            //}
            //var personResponse = context.People.ByKey(1).GetValue();


            //creating a  new person, POST

            var newPerson = new Person()
            {
                FirstName = "Maggie",
                LastName = "Smith"
            };

            context.AddToPeople(newPerson);
            
            context.SaveChanges(); //here the api is called

            newPerson.FirstName = "Violet";
            context.UpdateObject(newPerson);

            context.SaveChanges();

            context.DeleteObject(newPerson);

            context.SaveChanges();

            var peopleResponse = context.People.OrderByDescending(p => p.PersonId);
            var peopleAsList = peopleResponse.ToList();

            return View
                (
                    new AirVinylViewModel()
                    {
                        People = peopleAsList,
                        //Person = personResponse,
                        //AditionalData = aditionalData
                    }
                );
        }
    }
}