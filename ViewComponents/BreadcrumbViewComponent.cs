using Microsoft.AspNetCore.Mvc;

namespace DigitalSignage.ViewComponents
{
    public class BreadcrumbViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            // Get breadcrumb items from ViewBag (set by BaseController)
            var breadcrumbItems = ViewBag.BreadcrumbItems as List<BreadcrumbItem>
                ?? new List<BreadcrumbItem>();

            return View(breadcrumbItems);
        }
    }

    public class BreadcrumbItem
    {
        public string Text { get; set; } = string.Empty;
        public string? Url { get; set; }
        public bool IsActive { get; set; }
        public string Icon { get; set; } = string.Empty;
    }
}
