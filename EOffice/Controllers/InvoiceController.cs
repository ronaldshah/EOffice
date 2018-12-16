using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Rotativa;


namespace EOffice.Controllers
{
    public class InvoiceController : Controller
    {
        // GET: Invoice
        public ActionResult Invoice()
        {
            return new ActionAsPdf("Invoice");
        }
    }
}