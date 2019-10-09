namespace Xtensive.Orm.Tracking.Tests.Model
{
  public class MyChildEntity : MyEntity
  {
    [Field]
    public string ChildField { get; set; }

    public MyChildEntity(Session session)
      : base(session)
    {
    }

    public MyChildEntity(Session session, int id)
      : base(session, id)
    {
    }
  }
}