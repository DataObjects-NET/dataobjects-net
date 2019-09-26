using Xtensive.Orm.Tracking;

namespace Xtensive.Orm
{
  /// <summary>
  /// <see cref="Domain"/> extensions for Xtensive.Orm.Tracking.
  /// </summary>
  public static class DomainExtensions
  {
    /// <summary>
    /// Gets the <see cref="IDomainTrackingMonitor"/> implementation.
    /// </summary>
    /// <param name="domain">The domain.</param>
    /// <returns></returns>
    public static IDomainTrackingMonitor GetTrackingMonitor(this Domain domain)
    {
      return domain.Services.Get<IDomainTrackingMonitor>();
    }
  }
}
