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
    public class ProductController : ApiController
    {
        private MyMoridgeServerModelContainer1 db = new MyMoridgeServerModelContainer1();

        // GET api/Product
        public IEnumerable<Product> GetProducts()
        {
            db.Configuration.ProxyCreationEnabled = false;

            return db.Products.AsEnumerable();
        }

        // GET api/Product/5
        public Product GetProduct(int id)
        {
            Product product = db.Products.Find(id);
            if (product == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return product;
        }

        // PUT api/Product/5
        public HttpResponseMessage PutProduct(int id, Product product)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            if (id != product.Id)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            db.Entry(product).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // POST api/Product
        public HttpResponseMessage PostProduct(Product product)
        {
            if (ModelState.IsValid)
            {
                db.Products.Add(product);
                db.SaveChanges();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, product);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = product.Id }));
                return response;
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
        }

        // DELETE api/Product/5
        public HttpResponseMessage DeleteProduct(int id)
        {
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            db.Products.Remove(product);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK, product);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}