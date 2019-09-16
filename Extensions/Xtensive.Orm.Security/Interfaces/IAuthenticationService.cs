using System.Security.Principal;
using Xtensive.Orm;

namespace Xtensive.Orm.Security
{
  /// <summary>
  /// Authentication service.
  /// </summary>
  public interface IAuthenticationService : ISessionService
  {
    /// <summary>
    /// Authenticates a user's credentials. Returns <see cref="IPrincipal"/> instance if the credentials are valid and <see langword="null" /> if they are not.
    /// </summary>
    /// <param name="identity">The identity.</param>
    /// <param name="args">The arguments to validate, e.g. password.</param>
    /// <returns><see cref="IPrincipal"/> instance if the credentials are valid and <see langword="null" /> if they are not.</returns>
    IPrincipal Authenticate(IIdentity identity, params object[] args);

    /// <summary>
    /// Authenticates a user's credentials. Returns <see cref="IPrincipal"/> instance if the credentials are valid and <see langword="null" /> if they are not.
    /// </summary>
    /// <param name="name">The user name.</param>
    /// <param name="args">The arguments to validate, e.g. password.</param>
    /// <returns><see cref="IPrincipal"/> instance if the credentials are valid and <see langword="null" /> if they are not.</returns>
    IPrincipal Authenticate(string name, params object[] args);
  }
}