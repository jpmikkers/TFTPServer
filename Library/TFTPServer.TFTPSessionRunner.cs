using System;
using System.Threading;
using System.Threading.Tasks;

namespace CodePlex.JPMikkers.TFTP;

public partial class TFTPServer
{
    private class TFTPSessionRunner
    {
        public required ITFTPSession Session { get; init; }
        private Task? _task;

        public void Start(Action<ITFTPSession, Exception?> cleanupAction,CancellationToken cancellationToken)
        {
            _task = Task.Run(async () =>
            {
                try
                {
                    await Session.Run(cancellationToken);
                    cleanupAction(Session,null);
                }
                catch(Exception ex)
                {
                    cleanupAction(Session, ex);
                }
            }, cancellationToken);
        }
    }
}
