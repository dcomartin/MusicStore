using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MusicStore.Models;

namespace MusicStore.Features.ShoppingCart
{

    public class AddToCartController : Controller
    {
        private readonly IMediator _mediator;

        public AddToCartController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("/ShoppingCart/AddToCart/{id:int}")]
        public async Task<IActionResult> AddToCart(int id, CancellationToken requestAborted)
        {
            var cartId = Models.ShoppingCart.GetCartId(HttpContext);
            await _mediator.SendAsync(new AddToCart(cartId, id), requestAborted);

            // Go back to the main store page for more shopping
            return RedirectToAction("Index", "ShoppingCart");
        }
    }

    public class AddToCart : ICancellableAsyncRequest<Unit>
    {
        public string CartId { get; }
        public int AlbumId { get; }

        public AddToCart(string cartId, int albumId)
        {
            CartId = cartId;
            AlbumId = albumId;
        }
    }

    public class AddToCartHandler : ICancellableAsyncRequestHandler<AddToCart, Unit>
    {
        private readonly IMediator _mediator;
        private readonly MusicStoreContext _dbContext;

        public AddToCartHandler(IMediator mediator, MusicStoreContext dbContext)
        {
            _mediator = mediator;
            _dbContext = dbContext;
        }

        public async Task<Unit> Handle(AddToCart message, CancellationToken cancellationToken)
        {
            // Retrieve the album from the database
            var addedAlbum = await _dbContext.Albums
                .SingleAsync(album => album.AlbumId == message.AlbumId, cancellationToken);

            // Add it to the shopping cart
            var cart = Models.ShoppingCart.GetCart(_dbContext, message.CartId);

            await cart.AddToCart(addedAlbum);

            await _dbContext.SaveChangesAsync(cancellationToken);

            _mediator.Publish(new AlbumAddedToCart(addedAlbum.AlbumId));

            return Unit.Value;
        }
    }

    public class AlbumAddedToCart : INotification
    {
        public int AlbumId { get; }

        public AlbumAddedToCart(int albumId)
        {
            AlbumId = albumId;
        }
    }

    public class AlbumAddedToCartHandler : INotificationHandler<AlbumAddedToCart>
    {
        private readonly ILogger<AddToCart> _logger;

        public AlbumAddedToCartHandler(ILogger<AddToCart> logger)
        {
            _logger = logger;
        }

        public void Handle(AlbumAddedToCart notification)
        {
            _logger.LogInformation("Album {albumId} was added to the cart.", notification.AlbumId);
        }
    }
}

