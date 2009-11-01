// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Dom.Dml
{
  [Serializable]
  public abstract class SqlHint : SqlNode
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlHint"/> class.
    /// </summary>
    protected SqlHint() : base(SqlNodeType.Hint)
    {
    }
  }
}