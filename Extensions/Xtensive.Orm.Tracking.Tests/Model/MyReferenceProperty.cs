using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xtensive.Orm;

namespace Xtensive.Orm.Tracking.Tests.Model
{
  [HierarchyRoot]
  public class MyReferenceProperty : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public string Name { get; set; }

    public MyReferenceProperty()
      : base()
    {
    }

    public MyReferenceProperty(Session session)
      : base(session)
    {
    }
  }
}
