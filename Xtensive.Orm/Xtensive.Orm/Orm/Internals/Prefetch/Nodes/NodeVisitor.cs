// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2011.01.14

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Linq;

namespace Xtensive.Orm.Internals.Prefetch
{
  internal class NodeVisitor
  {
    public virtual Node Visit(Node node)
    {
      if (node == null)
        return null;
      return node.Accept(this);
    }

    public virtual ReadOnlyCollection<FieldNode> Visit(ReadOnlyCollection<FieldNode> nodes)
    {
      FieldNode[] list = null;
      var index = 0;
      var count = nodes.Count;
      while (index < count) {
        var node = nodes[index];
        var visited = (FieldNode) Visit(node);
        if (list != null)
          list[index] = visited;
        else if (!ReferenceEquals(visited, node)) {
          list = new FieldNode[count];
          for (var i = 0; i < index; i++)
            list[i] = nodes[i];
          list[index] = visited;
        }
        index++;
      }
      return list == null 
        ? nodes 
        : new ReadOnlyCollection<FieldNode>(list);
    }

    protected internal virtual Node VisitKeyExtractorNode<T>(KeyExtractorNode<T> keyExtractorNode)
    {
      var nestedNodes = Visit(keyExtractorNode.NestedNodes);
      if (ReferenceEquals(keyExtractorNode.NestedNodes, nestedNodes))
        return keyExtractorNode;
      return new KeyExtractorNode<T>(keyExtractorNode.Path, keyExtractorNode.ExtractKeys, nestedNodes);
    }

    protected internal virtual Node VisitFieldNode(FieldNode fieldNode)
    {
      return fieldNode;
    }

    protected internal virtual Node VisitReferenceNode(ReferenceNode referenceNode)
    {
      var nestedNodes = Visit(referenceNode.NestedNodes);
      if (ReferenceEquals(referenceNode.NestedNodes, nestedNodes))
        return referenceNode;
      return new ReferenceNode(
        referenceNode.Path, 
        referenceNode.ElementType, 
        referenceNode.Field, 
        nestedNodes);
    }

    protected internal virtual Node VisitSetNode(SetNode setNode)
    {
      var nestedNodes = Visit(setNode.NestedNodes);
      if (ReferenceEquals(setNode.NestedNodes, nestedNodes))
        return setNode;
      return new SetNode(
        setNode.Path,
        setNode.ElementType,
        setNode.Field,
        setNode.Top,
        nestedNodes);
    }
  }
}