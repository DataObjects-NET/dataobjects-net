// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2011.01.13

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Collections.ObjectModel;

namespace Xtensive.Orm.Internals.Prefetch
{
  internal class NodeAggregator<T> : NodeVisitor
  {
    public static IList<KeyExtractorNode<T>> Aggregate(IEnumerable<KeyExtractorNode<T>> source)
    {
      var aggregator = new NodeAggregator<T>();
      return source
        .GroupBy(ken => ken.Path, (path, @group) => 
          (Node) @group.First().ReplaceNestedNodes(
            new ReadOnlyCollection<FieldNode>(
              @group.SelectMany(ken => ken.NestedNodes).ToList())))
        .Select(aggregator.Visit)
        .Cast<KeyExtractorNode<T>>()
        .ToList();
    }

    public override ReadOnlyCollection<FieldNode> Visit(ReadOnlyCollection<FieldNode> nodes)
    {
      var result = new List<FieldNode>();
      foreach (var group in nodes.Where(n => n != null).GroupBy(n => n.Path)) {
        var node = group.First();
        var container = node as IHasNestedNodes;
        if (container == null)
          result.Add(node);
        else {
          var nodeToVisit = (FieldNode) container.ReplaceNestedNodes(
            new ReadOnlyCollection<FieldNode>(group.Cast<IHasNestedNodes>().SelectMany(c => c.NestedNodes).ToList()));
          result.Add((FieldNode)Visit(nodeToVisit));
        }
      }
      return new ReadOnlyCollection<FieldNode>(result);
    }
  }
}