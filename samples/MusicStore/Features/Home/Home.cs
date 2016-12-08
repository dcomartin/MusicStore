using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MusicStore.Models;

namespace MusicStore.Features
{
    public class HomeController : Controller
    {
        private readonly IMediator _mediator;

        public HomeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("/")]
        public async Task<IActionResult> Home()
        {
            var viewModel = await _mediator.SendAsync(new Home());
            return View(viewModel);
        }
    }

    public class Home : IAsyncRequest<List<Album>> { }

    public class HomeHandler : IAsyncRequestHandler<Home, List<Album>>
    {
        private readonly MusicStoreContext _dbContext;
        private readonly IMemoryCache _cache;
        private readonly IOptions<AppSettings> _options;

        public HomeHandler(MusicStoreContext dbContext, IMemoryCache cache, IOptions<AppSettings> options)
        {
            _dbContext = dbContext;
            _cache = cache;
            _options = options;
        }

        public async Task<List<Album>> Handle(Home message)
        {
            // Get most popular albums
            var cacheKey = "topselling";
            List<Album> albums;
            if (!_cache.TryGetValue(cacheKey, out albums))
            {
                albums = await _dbContext.Albums
                            .OrderByDescending(a => a.OrderDetails.Count)
                            .Take(6)
                            .ToListAsync();

                if (albums != null && albums.Count > 0)
                {
                    if (_options.Value.CacheDbResults)
                    {
                        // Refresh it every 10 minutes.
                        // Let this be the last item to be removed by cache if cache GC kicks in.
                        _cache.Set(
                            cacheKey,
                            albums,
                            new MemoryCacheEntryOptions()
                            .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
                            .SetPriority(CacheItemPriority.High));
                    }
                }
            }
            return albums;
        }
    }
}
