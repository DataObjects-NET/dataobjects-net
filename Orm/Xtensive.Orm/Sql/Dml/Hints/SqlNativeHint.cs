// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public class SqlNativeHint : SqlHint
  {
    /// <summary>
    /// Gets the hint text.
    /// </summary>
    /// <value>The hint text.</value>
    public string HintText { get; private set; }

    internal override object Clone(SqlNodeCloneContext context) =>
      context.NodeMapping.TryGetValue(this, out var clone)
        ? clone
        : context.NodeMapping[this] = new SqlNativeHint(HintText);

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    // Constructors

    internal SqlNativeHint(string hintText)
    {
      HintText = hintText;
    }
  }
}
