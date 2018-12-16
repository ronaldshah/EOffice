using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace EOffice
{
    public class Global : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

        }
        protected void Application_Error(object sender, EventArgs e)
        {
            Exception exception = Server.GetLastError();
            if (!Response.IsRequestBeingRedirected)
            {
                Response.Clear();
                Server.ClearError();
                Response.Redirect("/?" + exception.ToString());
            }
        }
    }
}