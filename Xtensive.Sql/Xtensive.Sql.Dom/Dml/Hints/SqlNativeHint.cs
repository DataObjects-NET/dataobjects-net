// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Dom.Dml
{
  [Serializable]
  public class SqlNativeHint : SqlHint
  {
    private string hintText;

    /// <summary>
    /// Gets the hint text.
    /// </summary>
    /// <value>The hint text.</value>
    public string HintText
    {
      get { return hintText; }
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      SqlNativeHint clone = new SqlNativeHint(hintText);

      context.NodeMapping[this] = clone;
      return clone;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlNativeHint(string hintText)
    {
      this.hintText = hintText;
    }
  }
}
