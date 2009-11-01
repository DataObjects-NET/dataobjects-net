// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System.Collections.Generic;

namespace Xtensive.Sql.Dom
{
  internal class SqlNodeCloneContext
  {
    private Dictionary<SqlNode, SqlNode> nodeMapping = new Dictionary<SqlNode, SqlNode>();

    public Dictionary<SqlNode, SqlNode> NodeMapping {
      get {
        return nodeMapping;
      }
    }
  }
}
