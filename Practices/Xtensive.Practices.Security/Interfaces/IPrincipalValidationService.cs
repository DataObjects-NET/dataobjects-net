using System.Security.Principal;
using Xtensive.Orm;

namespace Xtensive.Practices.Security
{
  public interface IPrincipalValidationService : ISessionService
  {
    IPrincipal Validate(IIdentity identity, params object[] args);

    IPrincipal Validate(string name, params object[] args);
  }
}