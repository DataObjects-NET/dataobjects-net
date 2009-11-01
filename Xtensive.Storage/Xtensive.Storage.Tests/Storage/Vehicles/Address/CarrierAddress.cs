using Xtensive.Core;
using Xtensive.Storage.Attributes;

namespace Xtensive.Storage.Tests.Storage.Vehicles
{
  /// <summary>
  /// Structure represents carrier's address.
  /// </summary>
  public class CarrierAddress : CompanyAddress
  {
    /// <summary>
    /// Address of carrier's Headquater. 
    /// Attention! Potential loop!
    /// </summary>
    [Field]
    public Address HeadquaterAddress { get; set; }

    /// <summary>
    /// Phone. 
    /// Attention! Overrides base phone.
    /// </summary>
    [Field]
    public override string Phone
    {
      get { return base.Phone; }
      set
      {
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        base.Phone = value;
      }
    }
  }
}