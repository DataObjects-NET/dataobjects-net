// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.15

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