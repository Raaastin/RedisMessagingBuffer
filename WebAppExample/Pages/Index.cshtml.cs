using Messaging.Buffer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using WebAppExample.Requests;

namespace WebAppExample.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}
