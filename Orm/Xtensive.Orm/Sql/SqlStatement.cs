// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql
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
