using Xtensive.Orm.Tracking;

namespace Xtensive.Orm
{
  /// <summary>
  /// <see cref="Session"/> extensions for Xtensive.Orm.Tracking.
  /// </summary>
  public static class SessionExtensions
  {
    /// <summary>
    /// Gets the <see cref="ISessionTrackingMonitor"/> implementation.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <returns></returns>
    public static ISessionTrackingMonitor GetTrackingMonitor(this Session session)
    {
      return session.Services.Get<ISessionTrackingMonitor>();
    }
  }
}
