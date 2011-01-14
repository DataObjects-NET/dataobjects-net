// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2011.01.13

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Collections.ObjectModel;
using Xtensive.Core;

namespace Xtensive.Orm.Internals.Prefetch
{
  internal class KeyExtractorNode<T> : Node, IHasNestedNodes
  {
    private Func<T, IEnumerable<Key>> extractKeys;
    private Expression<Func<T, IEnumerable<Key>>> extractKeysExpression;
    
    public Func<T, IEnumerable<Key>> ExtractKeys
    {
      get { return extractKeys ?? (extractKeys = extractKeysExpression.CachingCompile()); }
    }

    public ReadOnlyCollection<FieldNode> NestedNodes { get; private set; }

    public IHasNestedNodes ReplaceNestedNodes(ReadOnlyCollection<FieldNode> nestedNodes)
    {
      return new KeyExtractorNode<T>(Path, extractKeysExpression, nestedNodes);
    }

    protected internal override Node Accept(NodeVisitor visitor)
    {
      return visitor.VisitKeyExtractorNode(this);
    }

    public KeyExtractorNode(string path, Func<T, IEnumerable<Key>> extractor, ReadOnlyCollection<FieldNode> nestedNodes)
      : base(path)
    {
      ArgumentValidator.EnsureArgumentNotNull(path, "path");
      ArgumentValidator.EnsureArgumentNotNull(extractor, "extractor");
      ArgumentValidator.EnsureArgumentNotNull(nestedNodes, "nestedNodes");

      extractKeys = extractor;
      NestedNodes = nestedNodes;
    }

    public KeyExtractorNode(string path, Expression<Func<T, IEnumerable<Key>>> extractor, ReadOnlyCollection<FieldNode> nestedNodes)
      : base(path)
    {
      extractKeysExpression = extractor;
      NestedNodes = nestedNodes;
    }
  }
}