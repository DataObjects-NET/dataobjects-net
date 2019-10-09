using Xtensive.Orm;

namespace TestCommon.Model
{
  public class Point : Structure
  {
    [Field]
    public int? X { get; set; }

    [Field]
    public int? Y { get; set; }

    public Point(Session session)
      : base(session)
    {}
  }
}
