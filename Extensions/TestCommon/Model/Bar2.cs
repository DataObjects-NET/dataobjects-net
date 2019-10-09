using System;
using Xtensive.Orm;

namespace TestCommon.Model
{
  [HierarchyRoot]
  [KeyGenerator(KeyGeneratorKind.None)]
  public class Bar2 : Entity
  {
    public Bar2(Session session, DateTime id, Guid id2)
      : base(session, id, id2)
    {
    }

    [Field]
    [Key(0)]
    public DateTime Id { get; set; }

    [Field]
    [Key(1)]
    public Guid Id2 { get; set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public int Count { get; set; }

    [Field]
    public string Description { get; set; }

    [Field]
    public DateTime Date { get; set; }

    [Field]
    [Association("Bar")]
    public EntitySet<Foo2> Foo { get; set; }
  }
}
