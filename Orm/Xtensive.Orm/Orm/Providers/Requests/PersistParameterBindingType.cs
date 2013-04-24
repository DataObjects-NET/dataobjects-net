namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// Possible types of <see cref="PersistParameterBinding"/>.
  /// </summary>
  public enum PersistParameterBindingType
  {
    /// <summary>
    /// Regular parameter. Parameter value is obtained thru difference tuple.
    /// No special handling of null is performed.
    /// </summary>
    Regular = 0,

    /// <summary>
    /// Version parameter. Parameter value is obtained thru original tuple.
    /// Null values are treated specially similar to <see cref="QueryParameterBindingType.SmartNull"/>.
    /// </summary>
    VersionFilter = 1,
  }
}