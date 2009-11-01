using Xtensive.Storage.Attributes;

namespace Xtensive.Storage.Tests.Storage.Vehicles
{
  /// <summary>
  /// <see cref="Fleet"/> of trucks.
  /// </summary>
  public sealed class TruckFleet : Fleet
  {
    /// <summary>
    /// Gets fleet type. 
    /// Attention! Overrides <see cref="Persistent.Type"/> by "new" keyword.
    /// </summary>
    [Field]
    public new TruckFleetType Type
    {
      get { return GetValue<TruckFleetType>("Type"); }
      set { SetValue("Type", value); }
    }
  }
}