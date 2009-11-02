// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.04.28

using System;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Ddl
{
  [Serializable]
  public class SqlRenameAction : SqlAction
  {
    public Node Node { get; private set; }

    public string Name { get; private set; }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      SqlRenameAction clone = new SqlRenameAction(Node, Name);
      context.NodeMapping[this] = clone;

      return clone;
    }

    internal SqlRenameAction(Node node, string name)
    {
      Node = node;
      Name = name;
    }
  }
}