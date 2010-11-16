namespace Xtensive.Storage.Tests.Issues.Issue0754_CopyFieldHint_MoveFieldHint.ModelVersion2
{
  [HierarchyRoot]
  public class X : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }

  [HierarchyRoot]
  public class A : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public int Reference { get; set; }
  }

  public class B : A
  {
  }
}