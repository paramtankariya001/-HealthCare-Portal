using Microsoft.AspNetCore.Mvc;

namespace HealthCare_Portal.Controllers;

public class PortalController : Controller
{
    public IActionResult Index() => View();
}
