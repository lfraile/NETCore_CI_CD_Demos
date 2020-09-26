using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.eShopWeb.Web.Services;
using Microsoft.eShopWeb.Web.ViewModels;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Microsoft.eShopWeb.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ICatalogViewModelService _catalogViewModelService;
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ICatalogViewModelService catalogViewModelService, TelemetryClient telemetryClient, ILogger<IndexModel> logger)
        {
            _catalogViewModelService = catalogViewModelService;
            _telemetryClient = telemetryClient;
            _logger = logger;
        }

        public CatalogIndexViewModel CatalogModel { get; set; } = new CatalogIndexViewModel();

        public async Task OnGet(CatalogIndexViewModel catalogModel, int? pageId)
        {
            _telemetryClient.TrackEvent("Catalog event");
            _logger.LogWarning("Catalog event {ticks}", DateTime.UtcNow.Ticks);
            CatalogModel = await _catalogViewModelService.GetCatalogItems(pageId ?? 0, Constants.ITEMS_PER_PAGE, catalogModel.BrandFilterApplied, catalogModel.TypesFilterApplied);
        }
    }
}
