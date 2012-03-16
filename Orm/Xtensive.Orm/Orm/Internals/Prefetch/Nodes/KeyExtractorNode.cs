// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
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
    public Func<T, IEnumerable<Key>> KeyExtractor { get; private set; }

    public ReadOnlyCollection<BaseFieldNode> NestedNodes { get; private set; }

    IEnumerable<Key> IHasNestedNodes.ExtractKeys(object target)
    {
      return ExtractKeys((T) target);
    }

    public IEnumerable<Key> ExtractKeys(T target)
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
      return string.Format(string.Format("KeyExtraction<{0}>", typeof (T).Name));
    }

    public KeyExtractorNode(Func<T, IEnumerable<Key>> extractor, ReadOnlyCollection<BaseFieldNode> nestedNodes)
      : base("*")
    {
      ArgumentValidator.EnsureArgumentNotNull(extractor, "extractor");
      ArgumentValidator.EnsureArgumentNotNull(nestedNodes, "nestedNodes");

      KeyExtractor = extractor;
      NestedNodes = nestedNodes;
    }
  }
}