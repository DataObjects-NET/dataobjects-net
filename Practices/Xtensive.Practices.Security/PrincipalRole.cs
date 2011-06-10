using Xtensive.Orm;

namespace Xtensive.Practices.Security
{
  [HierarchyRoot]
  [KeyGenerator(KeyGeneratorKind.None)]
  public class PrincipalRole : Entity
  {
    [Field, Key(0)]
    public IPrincipal Principal { get; private set; }

    [Field(Length = 50), Key(1)]
    public string Name { get; private set; }

    public PrincipalRole (Session session, IPrincipal principal, string name)
      : base(session, principal, name) {}
  }
}
