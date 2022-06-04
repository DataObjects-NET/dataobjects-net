// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Xtensive.Sql.Dml
{
  /// <summary>
  /// Base class for DML statements.
  /// </summary>
  [Serializable]
  public abstract class SqlQueryStatement : SqlStatement
  {
    private IList<SqlHint> hints;

    /// <summary>
    /// Gets the collection of join hints.
    /// </summary>
    /// <value>The collection of join hints.</value>
    public IList<SqlHint> Hints => hints ??= new Collection<SqlHint>();

    // Constructors

    protected SqlQueryStatement(SqlNodeType nodeType) : base(nodeType)
    {
    }
  }
}
