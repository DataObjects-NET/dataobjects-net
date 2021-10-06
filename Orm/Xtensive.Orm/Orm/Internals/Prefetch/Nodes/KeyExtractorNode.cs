// Copyright (C) 2011-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2011.01.13

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xtensive.Core;

namespace Xtensive.Orm.Internals.Prefetch
{
  internal class KeyExtractorNode<T> : Node, IHasNestedNodes
  {
    public Func<T, IReadOnlyCollection<Key>> KeyExtractor { get; }

    public ReadOnlyCollection<BaseFieldNode> NestedNodes { get; }

    IReadOnlyCollection<Key> IHasNestedNodes.ExtractKeys(object target)
    {
      return ExtractKeys((T) target);
    }

    public IReadOnlyCollection<Key> ExtractKeys(T target)
    {
      return KeyExtractor.Invoke(target);
    }

    public IHasNestedNodes ReplaceNestedNodes(ReadOnlyCollection<BaseFieldNode> nestedNodes)
    {
      return new KeyExtractorNode<T>(KeyExtractor, nestedNodes);
    }

    public override Node Accept(NodeVisitor visitor)
    {
      return visitor.VisitKeyExtractorNode(this);
    }

    protected override string GetDescription()
    {
      return $"KeyExtraction<{typeof(T).Name}>";
    }

    public KeyExtractorNode(Func<T, IReadOnlyCollection<Key>> extractor, ReadOnlyCollection<BaseFieldNode> nestedNodes)
      : base("*")
    {
      ArgumentValidator.EnsureArgumentNotNull(extractor, nameof(extractor));
      ArgumentValidator.EnsureArgumentNotNull(nestedNodes, nameof(nestedNodes));

      KeyExtractor = extractor;
      NestedNodes = nestedNodes;
    }
  }
}
