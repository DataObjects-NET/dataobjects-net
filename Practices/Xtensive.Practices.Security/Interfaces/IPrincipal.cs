using Xtensive.Orm;

namespace Xtensive.Practices.Security
{
  public interface IPrincipal : IEntity, System.Security.Principal.IPrincipal
  {
    [Field]
    string Name { get; }

    [Field]
    [Association(PairTo = "Principal", OnOwnerRemove = OnRemoveAction.Cascade)]
    PrincipalRoleSet PrincipalRoles { get; }

    bool IsInRole(Role role);
  }
}