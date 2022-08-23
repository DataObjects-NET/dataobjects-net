// Copyright (C) 2011-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2011.01.14

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Xtensive.Orm.Internals.Prefetch
{
  internal interface IHasNestedNodes
  {
    IReadOnlyList<BaseFieldNode> NestedNodes { get; }

    IReadOnlyCollection<Key> ExtractKeys(object target);

    IHasNestedNodes ReplaceNestedNodes(IReadOnlyList<BaseFieldNode> nestedNodes);
  }
}
