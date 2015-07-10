using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using MyMoridgeServer.Models;

namespace MyMoridgeServer.Controllers
{
    public class VehicleController : ApiController
    {
        private MyMoridgeServerModelContainer1 db = new MyMoridgeServerModelContainer1();

        // GET api/Vehicle
        public IEnumerable<CustomerVehicle> GetCustomerVehicles()
        {
            return db.CustomerVehicles.AsEnumerable();
        }

        // GET api/Vehicle/5
        public IEnumerable<CustomerVehicle> GetCustomerVehicle(string id)
        {
            IEnumerable<CustomerVehicle> customerVehicles = db.CustomerVehicles.Where(vehicle => vehicle.CustomerOrgNo == id);
            if (customerVehicles == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return customerVehicles;
        }

        // PUT api/Vehicle/5
        public HttpResponseMessage PutCustomerVehicle(string id, CustomerVehicle customervehicle)
        {
            if (ModelState.IsValid && id == customervehicle.CustomerOrgNo)
            {
                db.Entry(customervehicle).State = EntityState.Modified;

                try
                {
                    db.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

        // POST api/Vehicle
        public HttpResponseMessage PostCustomerVehicle(CustomerVehicle customervehicle)
        {
            if (ModelState.IsValid)
            {
                db.CustomerVehicles.Add(customervehicle);
                db.SaveChanges();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, customervehicle);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = customervehicle.CustomerOrgNo }));
                return response;
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

        // DELETE api/Vehicle/5
        public HttpResponseMessage DeleteCustomerVehicle(string id)
        {
            CustomerVehicle customervehicle = db.CustomerVehicles.Find(id);
            if (customervehicle == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            db.CustomerVehicles.Remove(customervehicle);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            return Request.CreateResponse(HttpStatusCode.OK, customervehicle);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}