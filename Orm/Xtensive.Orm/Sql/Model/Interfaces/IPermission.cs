// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System.Collections.Generic;

namespace Xtensive.Sql.Model
{
  public interface IPermission
  {
    Action Action { get; set; }
    // INSERT, UPDATE, REFERENCES
    IList<TableColumn> PrivilegeColumns { get; }
    bool Deny { get; set; }
  }
}
