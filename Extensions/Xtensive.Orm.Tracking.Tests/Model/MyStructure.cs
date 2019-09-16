using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xtensive.Orm;

namespace Xtensive.Orm.Tracking.Tests.Model
{
  public class MyStructure : Structure
  {
    [Field]
    public MyEntitySubStructure SubStructure { get; set; }

    [Field]
    public int Z { get; set; }

    public MyStructure() : base() { }
    public MyStructure(Session session) : base(session) {}
    public override string ToString()
    {
      return string.Format("MyEntityStructure: Z={0}, SubStructure=({1})", Z, SubStructure);
    }
  }

  public class MyEntitySubStructure : Structure
  {
    [Field]
    public int X { get; set; }

    [Field]
    public int Y { get; set; }

    public MyEntitySubStructure() : base() { }
    public MyEntitySubStructure(Session session) : base(session) { }
    public override string ToString()
    {
      return string.Format("MyEntitySubStructure: X={0}, Y={1}", X, Y);
    }
  }
}
