// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;

namespace Xtensive.Sql
{
  internal class SqlNodeCloneContext
  {
    private readonly Dictionary<SqlNode, SqlNode> nodeMapping = new();

    public Dictionary<SqlNode, SqlNode> NodeMapping => nodeMapping;

    public T GetOrAdd<T>(T node, Func<T, SqlNodeCloneContext, T> factory) where T : SqlNode
    {
      if (NodeMapping.TryGetValue(node, out var clone)) {
        return (T) clone;
      }
      var result = factory(node, this);
      NodeMapping[node] = result;
      return result;
    }

    public SqlNodeCloneContext()
    {
    }
  }
}
