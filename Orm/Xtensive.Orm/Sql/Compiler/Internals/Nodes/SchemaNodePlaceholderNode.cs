// Copyright (C) 2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using Xtensive.Sql.Model;

namespace Xtensive.Sql.Compiler
{
  internal sealed class SchemaNodePlaceholderNode : PlaceholderNode
  {
    public readonly SchemaNode SchemaNode;

    public readonly SqlHelper.EscapeSetup EscapeSetup;

    public readonly bool DbQualified;

    internal override void AcceptVisitor(NodeVisitor visitor) => base.AcceptVisitor(visitor);

    public SchemaNodePlaceholderNode(SchemaNode table, in SqlHelper.EscapeSetup escapeSetup, bool requireDatabaseName)
      : base(table)
    {
      SchemaNode = table;
      EscapeSetup = escapeSetup;
      DbQualified = requireDatabaseName;
    }
  }
}