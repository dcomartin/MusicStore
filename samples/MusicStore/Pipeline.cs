using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace MusicStore
{
    public interface IPostRequestHandler<in TRequest, in TResponse> where TRequest : ICancellableAsyncRequest<TResponse>
    {
        void Handle(TRequest request, TResponse response);
    }

    public class Pipeline<TRequest, TResponse> 
        : ICancellableAsyncRequestHandler<TRequest, TResponse>
            where TRequest : ICancellableAsyncRequest<TResponse>
    {
        private readonly ICancellableAsyncRequestHandler<TRequest, TResponse> _inner;
        private readonly IPostRequestHandler<TRequest, TResponse>[] _postHandlers;

        public Pipeline(
            ICancellableAsyncRequestHandler<TRequest, TResponse> inner,
            IPostRequestHandler<TRequest, TResponse>[] postHandlers
            )
        {
            _inner = inner;
            _postHandlers = postHandlers;
        }

        public async Task<TResponse> Handle(TRequest message, CancellationToken cancellationToken)
        {
            var response = await _inner.Handle(message, cancellationToken);

            foreach (var postHandler in _postHandlers)
            {
                 postHandler.Handle(message, response);
            }
            return response;
        }
    }
}
