using Xtensive.Orm;

namespace Xtensive.Practices.Security
{
  public interface IPrincipal : IEntity, System.Security.Principal.IPrincipal
  {
    [Field]
    string Name { get; }

    [Field]
    EntitySet<IRole> Roles { get; }
  }
}