using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Xtensive.Storage.Attributes;

namespace Xtensive.Storage.Tests.Storage.Vehicles
{
  public class Logo : Structure
  {
    [Field(LazyLoad = true)]
    public Bitmap Image
    {
      get
      {
        MemoryStream ms = new MemoryStream(GetValue<byte[]>("Image"));
        return (Bitmap) System.Drawing.Image.FromStream(ms);
      }
      set
      {
        MemoryStream ms = new MemoryStream();
        value.Save(ms, ImageFormat.Jpeg);
        SetValue("Image", ms.ToArray());
        Height = value.Height;
        Width = value.Width;
      }
    }

    [Field]
    public int Height { get; private set; }

    [Field]
    public int Width { get; private set; }

    public Logo(Bitmap image)
    {
      Image = image;
    }
  }
}