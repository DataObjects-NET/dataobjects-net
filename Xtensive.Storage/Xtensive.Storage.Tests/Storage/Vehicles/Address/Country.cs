using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Tests.Storage.Internals;

namespace Xtensive.Storage.Tests.Storage.Vehicles
{
  /// <summary>
  /// Company. Has name and 3-digit code
  /// </summary>
  [HierarchyRoot(typeof (StringProvider), "Name")]
  public sealed class Country : Entity
  {
    /// <summary>
    /// Company name.
    /// </summary>
    [Field(Length = 300)]
    public string Name { get; set; }

    /// <summary>
    /// Standard company code. "RUS" for Russia for example.
    /// </summary>
    [Field(Length = 3)]
    public string Code { get; set; }

    /// <summary>
    /// Additional data. Just byte array for tests.
    /// </summary>
    // [Field(Length = 100)]
    public byte[] OptionalData { get; set; }

    /// <summary>
    /// Country flag.
    /// </summary>
    
    public Bitmap Flag
    {
      get
      {        
        return (Bitmap) Image.FromStream(new MemoryStream(FlagBytes));
      }
      set
      {
        MemoryStream ms = new MemoryStream();
        value.Save(ms, ImageFormat.Jpeg);
        FlagBytes = ms.ToArray();        
      }
    }

    [Field(LazyLoad = true, Length = 100, IsNullable = true)]
    private byte[] FlagBytes { get; set;}    

    public Country(string countryName)
      : base(Core.Tuples.Tuple.Create(countryName))
    {
    }
  }
}