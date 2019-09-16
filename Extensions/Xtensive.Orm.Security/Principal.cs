using System.Linq;
using System.Security.Principal;
using Xtensive.Orm.Validation;

namespace Xtensive.Orm.Security
{
  /// <summary>
  /// Base <see cref="IPrincipal"/> implementation.
  /// </summary>
  [Index("Name", Unique = true)]
  public abstract class Principal : Entity, IPrincipal
  {
    private IIdentity identity;

    #region IPrincipal Members

    /// <inheritdoc/>
    [NotNullConstraint(IsImmediate = true)]
    [Field(Length = 128)]
    public string Name { get; set; }

    /// <inheritdoc/>
    [Field]
    public EntitySet<IRole> Roles { get; private set; }

    /// <inheritdoc/>
    public virtual IIdentity Identity
    {
      get
      {
        if (identity != null)
          return identity;

        identity = new GenericIdentity(Name);
        return identity;
      }
    }

    /// <inheritdoc/>
    public bool IsInRole(string role)
    {
      return Roles.Any(r => r.Name == role);
    }

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="Principal"/> class.
    /// </summary>
    /// <param name="session">The session.</param>
    protected Principal(Session session)
      : base(session)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Principal"/> class.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="values">Key values.</param>
    protected Principal(Session session, params object[] values)
      : base(session, values)
    {
    }
  }
}