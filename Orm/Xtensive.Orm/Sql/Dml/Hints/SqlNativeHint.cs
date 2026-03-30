// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

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

    internal override SqlNativeHint Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlNativeHint(t.HintText));

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
