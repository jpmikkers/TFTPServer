using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CodePlex.JPMikkers.TFTP
{
    static class IUDPSocketExtensions
    {
        public static async Task<UDPMessage> ReceiveWithTimeout(this IUDPSocket socket, CancellationToken userCT, TimeSpan timeout)
        {
            if(timeout <= TimeSpan.Zero) throw new TimeoutException();

            using var timeoutCTS = CancellationTokenSource.CreateLinkedTokenSource(userCT);

            try
            {
                timeoutCTS.CancelAfter(timeout);
                return await socket.Receive(timeoutCTS.Token);
            }
            catch(OperationCanceledException) when(timeoutCTS.IsCancellationRequested && !userCT.IsCancellationRequested)
            {
                throw new TimeoutException();
            }
        }
    }
}
