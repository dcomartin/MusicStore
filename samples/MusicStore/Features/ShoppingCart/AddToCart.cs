using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MusicStore.Models;

namespace MusicStore.Features.ShoppingCart
{
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
        private readonly MusicStoreContext _dbContext;

        public AddToCartHandler(MusicStoreContext dbContext)
        {
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
            
            return Unit.Value;
        }
    }

    public class AddToCartLogHandler : IPostRequestHandler<AddToCart, Unit>
    {
        private readonly ILogger<AddToCart> _logger;

        public AddToCartLogHandler(ILogger<AddToCart> logger)
        {
            _logger = logger;
        }

        public void Handle(AddToCart request, Unit response)
        {
            _logger.LogInformation("Album {albumId} was added to the cart.", request.AlbumId);
        }
    }
}
