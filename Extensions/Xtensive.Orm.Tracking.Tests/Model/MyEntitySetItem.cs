using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xtensive.Orm;

namespace Xtensive.Orm.Tracking.Tests.Model
{
  [HierarchyRoot]
  public class MyEntitySetItem : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field(Length = 100)]
    public string ItemText { get; set; }

    public MyEntitySetItem()
      : base()
    {
    }

    public MyEntitySetItem(Session session)
      : base(session)
    {
    }
  }
}
