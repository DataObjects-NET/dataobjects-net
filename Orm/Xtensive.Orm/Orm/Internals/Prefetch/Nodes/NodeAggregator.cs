// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2011.01.13

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Xtensive.Orm.Internals.Prefetch
{
  internal sealed class NodeAggregator<T> : NodeVisitor
  {
    public static IList<KeyExtractorNode<T>> Aggregate(IEnumerable<KeyExtractorNode<T>> source)
    {
      var aggregator = new NodeAggregator<T>();
      var result = source
        .GroupBy(ken => ken.Path, (path, @group) => 
          (Node) @group.First().ReplaceNestedNodes(
            new ReadOnlyCollection<BaseFieldNode>(
              @group.SelectMany(ken => ken.NestedNodes).ToList())))
        .Select(aggregator.Visit)
        .Cast<KeyExtractorNode<T>>()
        .ToList();
      return result;
    }

    public override ReadOnlyCollection<BaseFieldNode> VisitNodeList(ReadOnlyCollection<BaseFieldNode> nodes)
    {
      var result = new List<BaseFieldNode>();
      foreach (var group in nodes.Where(n => n!=null).GroupBy(n => n.Path)) {
        var node = group.First();
        var container = node as IHasNestedNodes;
        if (container==null)
          result.Add(node);
        else {
          var nodeToVisit = (BaseFieldNode) container.ReplaceNestedNodes(
            new ReadOnlyCollection<BaseFieldNode>(group.Cast<IHasNestedNodes>().SelectMany(c => c.NestedNodes).ToList()));
          result.Add((BaseFieldNode) Visit(nodeToVisit));
        }
      }
      return new ReadOnlyCollection<BaseFieldNode>(result);
    }

    // Constructor

    private NodeAggregator()
    {
    }
  }
}