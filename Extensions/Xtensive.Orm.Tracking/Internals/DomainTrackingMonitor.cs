using System;
using Xtensive.IoC;

namespace Xtensive.Orm.Tracking
{
  [Service(typeof (IDomainTrackingMonitor), Singleton = true)]
  internal sealed class DomainTrackingMonitor : TrackingMonitor, IDomainTrackingMonitor, IDomainService
  {
    private void OnOpenSession(object sender, SessionEventArgs args)
    {
      var session = args.Session;

      if (session.Configuration.Type==Configuration.SessionType.KeyGenerator)
        return;
      if (session.Configuration.Type==Configuration.SessionType.System)
        return;

      var monitor = session.Services.Get<ISessionTrackingMonitor>();

      monitor.TrackingCompleted += OnTrackingCompleted;
    }

    private void OnTrackingCompleted(object sender, TrackingCompletedEventArgs e)
    {
      RaiseTrackingCompleted(e);
    }

    [ServiceConstructor]
    public DomainTrackingMonitor(Domain domain)
    {
      ArgumentNullException.ThrowIfNull(domain);
      domain.SessionOpen += OnOpenSession;
    }
  }
}