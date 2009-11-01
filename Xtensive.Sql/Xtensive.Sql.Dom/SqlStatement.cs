// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Dom
{
  /// <summary>
  /// Base class for SQL statements.
  /// </summary>
  [Serializable]
  public abstract class SqlStatement : SqlNode
  {
    protected SqlStatement(SqlNodeType nodeType) : base(nodeType)
    {
    }
  }
}
