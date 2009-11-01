using Xtensive.Storage.Attributes;

namespace Xtensive.Storage.Tests.Storage.Vehicles
{
  /// <summary>
  /// Address structure.
  /// </summary>
  public class Address : Structure
  {
    [Field]
    public string Street { get; set; }

    [Field]
    public string City { get; set; }

    [Field]
    public string Zip { get; set; }

    [Field]
    public Country Country { get; set; }
  }
}