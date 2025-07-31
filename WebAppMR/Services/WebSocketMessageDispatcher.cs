using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace WebAppMR.Services
{
    public class WebSocketMessageDispatcher
    {
        private TaskCompletionSource<string?>? _pendingResponse;

        public Task<string?> WaitForResponseAsync(TimeSpan timeout)
        {
            if (_pendingResponse != null && !_pendingResponse.Task.IsCompleted)
                throw new InvalidOperationException("A pending response already exists.");
            var tcs = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);
            _pendingResponse = tcs;
            _ = Task.Run(async () =>
            {
                await Task.Delay(timeout);
                if (_pendingResponse == tcs)
                {
                    tcs.TrySetResult(null);
                    _pendingResponse = null;
                }
            });
            return tcs.Task;
        }

        public void SetResponse(string? response)
        {
            if (_pendingResponse != null && !_pendingResponse.Task.IsCompleted)
            {
                _pendingResponse.TrySetResult(response);
                _pendingResponse = null;
            }
        }
    }
}
