using System;
using System.Linq;
using System.Security.Principal;
using Xtensive.Aspects;
using Xtensive.Orm;
using Xtensive.Orm.Validation;

namespace Xtensive.Practices.Security
{
  public abstract class Principal : Entity, IPrincipal
  {
    private IIdentity identity;

    #region IPrincipal Members

    public bool IsInRole(Role role)
    {
      return PrincipalRoles.Contains(role);
    }

    [NotNullConstraint(Mode = ConstrainMode.OnSetValue)]
    [Field(Length = 50, Indexed = true)]
    [Infrastructure]
    public string Name
    {
      get { return GetFieldValue<string>("Name"); }
      set
      {
        if (!string.IsNullOrEmpty(Name) && PersistenceState != PersistenceState.New)
          throw new InvalidOperationException("Name property can't be changed");
        SetFieldValue("Name", value);
        identity = null;
      }
    }

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

    [Field]
    public PrincipalRoleSet PrincipalRoles { get; private set; }

    public bool IsInRole(string role)
    {
      return PrincipalRoles.Any(r => r.Name == role);
    }

    #endregion

    protected Principal(Session session)
      : base(session)
    {}
  }
}