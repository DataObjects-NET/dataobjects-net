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
  public class SqlGrant : SqlStatement, ISqlCompileUnit
  {
    private IList<IPermission> permissions = new Collection<IPermission>();
    //private ISecurable securableObject;
    private bool withGrantOption = false;
    private IList<User> grantees = new Collection<User>(); // ==null then PUBLIC

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

    public bool WithGrantOption {
      get {
        return withGrantOption;
      }
      set {
        withGrantOption = value;
      }
    }

    public IList<User> Grantees {
      get {
        return grantees;
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

    internal SqlGrant() : base(SqlNodeType.Grant)
    {
    }
  }
}
