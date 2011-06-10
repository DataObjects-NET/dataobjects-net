// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.05.22

using System.Linq;
using System.Runtime.Serialization;
using Xtensive.Core;
using Xtensive.Orm;
using Xtensive.Orm.Model;

namespace Xtensive.Practices.Security
{
  public class PrincipalRoleSet : EntitySet<PrincipalRole>
  {
    public bool Add(Role role)
    {
      ArgumentValidator.EnsureArgumentNotNull(role, "role");

      if (Contains(role))
        return true;

      return Add(new PrincipalRole(Session, (IPrincipal) Owner, role.Name));
    }

    public bool Remove(Role role)
    {
      ArgumentValidator.EnsureArgumentNotNull(role, "role");

      var item = Find(role);
      if (item == null)
        return true;

      return Remove(item);
    }

    public bool Contains(Role role)
    {
      ArgumentValidator.EnsureArgumentNotNull(role, "role");
      return Find(role) != null;
    }

    private PrincipalRole Find(Role role)
    {
      return this.Where(ur => ur.Name == role.Name).SingleOrDefault();
    }

    protected PrincipalRoleSet(Entity owner, FieldInfo field) : base(owner, field)
    {}

    protected PrincipalRoleSet(SerializationInfo info, StreamingContext context) : base(info, context)
    {}
  }
}