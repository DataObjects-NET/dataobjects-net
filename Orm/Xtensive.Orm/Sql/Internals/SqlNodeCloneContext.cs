// Copyright (C) 2003-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System.Collections.Generic;

namespace Xtensive.Sql
{
  internal class SqlNodeCloneContext
  {
    public Dictionary<SqlNode, SqlNode> NodeMapping { get; } = new();

    public T TryGet<T>(T node) where T : SqlNode =>
      NodeMapping.TryGetValue(node, out var clone)
        ? (T) clone
        : null;

    public T Add<T>(T node, T clone) where T : SqlNode 
    {
      NodeMapping[node] = clone;
      return clone;
    }
  }
}
