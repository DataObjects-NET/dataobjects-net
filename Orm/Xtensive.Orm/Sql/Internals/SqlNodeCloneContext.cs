// Copyright (C) 2003-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;

namespace Xtensive.Sql
{
  internal readonly struct SqlNodeCloneContext
  {
    private readonly Dictionary<SqlNode, SqlNode> nodeMapping;
    
    public Dictionary<SqlNode, SqlNode> NodeMapping => nodeMapping;

    public T TryGet<T>(T node) where T : SqlNode =>
      NodeMapping.TryGetValue(node, out var clone)
        ? (T) clone
        : null;

    public T GetOrAdd<T>(T node, Func<T, SqlNodeCloneContext, T> factory) where T : SqlNode
    {
      if (NodeMapping.TryGetValue(node, out var clone)) {
        return (T)clone;
      }
      var result = factory(node, this);
      NodeMapping[node] = result;
      return result;
    }

    public SqlNodeCloneContext(bool _)
    {
      nodeMapping = new();
    }
  }
}
