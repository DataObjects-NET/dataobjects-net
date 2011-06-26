using System.Security.Principal;
using Xtensive.Orm;

namespace Xtensive.Practices.Security
{
  public interface IAuthenticationService : ISessionService
  {
    IPrincipal Authenticate(IIdentity identity, params object[] args);

    IPrincipal Authenticate(string name, params object[] args);
  }
}