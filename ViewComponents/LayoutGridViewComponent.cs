using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using DigitalSignage.Services;
using DigitalSignage.ViewModels;

namespace DigitalSignage.ViewComponents
{
    public class LayoutGridViewComponent : ViewComponent
    {
        private readonly ILayoutService _layoutService;
        private readonly IMapper _mapper;

        public LayoutGridViewComponent(ILayoutService layoutService, IMapper mapper)
        {
            _layoutService = layoutService;
            _mapper = mapper;
        }

        public async Task<IViewComponentResult> InvokeAsync(int layoutId)
        {
            var layout = await _layoutService.GetLayoutWithSectionsAsync(layoutId);
            if (layout == null)
                return Content("Layout not found");

            var viewModel = _mapper.Map<DynamicLayoutViewModel>(layout);
            return View(viewModel);
        }
    }
}
