// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xtensive.Sql.Dom.Database;

namespace Xtensive.Sql.Dom.Dcl
{
  [Serializable]
  public class SqlRevoke : SqlStatement, ISqlCompileUnit
  {
    private bool revokeGrantOption = false;
    private IList<IPermission> permissions = new Collection<IPermission>();
    //private ISecurable securableObject;
    private IList<User> grantees = new Collection<User>(); // ==null then PUBLIC
    private bool cascade = false;

    public bool RevokeGrantOption {
      get {
        return revokeGrantOption;
      }
      set {
        revokeGrantOption = value;
      }
    }

    public IList<IPermission> Permissions {
      get {
        return permissions;
      }
    }

    //public ISecurable SecurableObject {
    //  get {
    //    return securableObject;
    //  }
    //  set {
    //    securableObject = value;
    //  }
    //}

    public IList<User> Grantees {
      get {
        return grantees;
      }
    }

    public bool Cascade {
      get {
        return cascade;
      }
      set {
        cascade = value;
      }
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      throw new NotImplementedException();
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      throw new NotImplementedException();
    }

    public SqlRevoke() : base(SqlNodeType.Revoke)
    {
    }
  }
}
