using System;
using Xtensive.Orm;

namespace TestCommon.Model
{
  [HierarchyRoot]
  public class Bar : Entity
  {
    [Field]
    [Key]
    public int Id { get; set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public int Count { get; set; }

    [Field]
    public string Description { get; set; }

    [Field]
    public DateTime Date { get; set; }

    [Field]
    public Rectangle Rectangle { get; set; }

    [Field]
    [Association("Bar")]
    public EntitySet<Foo> Foo { get; set; }

    public Bar(Session session)
      : base(session)
    {
    }

    public Bar(Session session, int id)
      : base(session, id)
    {
    }
  }
}