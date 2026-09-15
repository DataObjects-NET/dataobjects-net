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
    public Func<T, IReadOnlyList<Key>> KeyExtractor { get; }

    public IReadOnlyList<BaseFieldNode> NestedNodes { get; }

    IReadOnlyList<Key> IHasNestedNodes.ExtractKeys(object target)
    {
      return ExtractKeys((T) target);
    }

    public IReadOnlyList<Key> ExtractKeys(T target)
    {
      return KeyExtractor.Invoke(target);
    }

    public IHasNestedNodes ReplaceNestedNodes(IReadOnlyList<BaseFieldNode> nestedNodes)
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

    public KeyExtractorNode(Func<T, IReadOnlyList<Key>> extractor, IReadOnlyList<BaseFieldNode> nestedNodes)
      : base("*")
    {
      KeyExtractor = extractor ?? throw new ArgumentNullException(nameof(extractor));
      NestedNodes = nestedNodes ?? throw new ArgumentNullException(nameof(nestedNodes));
    }
  }
}
