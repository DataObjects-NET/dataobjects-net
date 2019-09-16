using Xtensive.Orm;

namespace TestCommon.Model
{
  [HierarchyRoot]
  [Index("Name", Unique = true)]
  public class Foo : Entity
  {
    [Field]
    [Key]
    public int Id { get; set; }

    [Field(Length = 150)]
    public string Name { get; set; }

    [Field]
    [Association("Foo")]
    public Bar Bar { get; set; }

    public string Name1 { get; set; }

    public Foo(Session session)
      : base(session)
    {
    }

    public Foo(Session session, int id)
      : base(session, id)
    {
    }
  }
}