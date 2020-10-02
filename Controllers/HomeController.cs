using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ResponseBufferTest.Models;

namespace ResponseBufferTest.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task GetData(CancellationToken cancellationToken)
        {
            HttpContext.Features.Get<IHttpResponseBodyFeature>().DisableBuffering();
            HttpContext.Response.StatusCode = 200;
            HttpContext.Response.ContentLength = null;
            await using var bodyStream = HttpContext.Response.BodyWriter.AsStream();
            await HttpContext.Response.StartAsync(cancellationToken).ConfigureAwait(false);
            for (var i = 0; i < 10; i++)
            {
                await bodyStream.WriteAsync(Encoding.UTF8.GetBytes(Convert.ToBase64String(new byte[128*1024])), cancellationToken).ConfigureAwait(false);
                await bodyStream.FlushAsync(cancellationToken).ConfigureAwait(false);
                await Task.Delay(1_000, cancellationToken).ConfigureAwait(false);
            }

            await HttpContext.Response.CompleteAsync().ConfigureAwait(false);
        }
    }
}
