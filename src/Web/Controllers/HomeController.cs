using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    public class HomeController: Controller
    {
        public string Version()
        {
            return typeof(HomeController).GetTypeInfo().Assembly.GetName().Version.ToString();
        }
    }
}
