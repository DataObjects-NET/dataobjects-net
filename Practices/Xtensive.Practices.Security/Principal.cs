using System;
using System.Linq;
using System.Security.Principal;
using Xtensive.Aspects;
using Xtensive.Orm;
using Xtensive.Orm.Validation;

namespace Xtensive.Practices.Security
{
  [Index("Name", Unique = true)]
  public abstract class Principal : Entity, IPrincipal
  {
    private IIdentity identity;

    #region IPrincipal Members

    [NotNullConstraint(Mode = ConstrainMode.OnSetValue)]
    [Field(Length = 128)]
    public string Name { get; set; }

    [Field]
    public EntitySet<IRole> Roles { get; private set; }

    [Infrastructure]
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

    public bool IsInRole(string role)
    {
      return Roles.Any(r => r.Name == role);
    }

    #endregion

    protected Principal(Session session)
      : base(session)
    {}
  }
}