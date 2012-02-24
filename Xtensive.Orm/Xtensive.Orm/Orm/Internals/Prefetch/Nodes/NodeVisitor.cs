// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2011.01.14

using System.Collections.ObjectModel;

namespace Xtensive.Orm.Internals.Prefetch
{
  internal abstract class NodeVisitor
  {
    public virtual Node Visit(Node node)
    {
      if (node==null)
        return null;
      return node.Accept(this);
    }

    public virtual ReadOnlyCollection<BaseFieldNode> VisitNodeList(ReadOnlyCollection<BaseFieldNode> nodes)
    {
      BaseFieldNode[] list = null;
      var index = 0;
      var count = nodes.Count;
      while (index < count) {
        var node = nodes[index];
        var visited = (BaseFieldNode) Visit(node);
        if (list!=null)
          list[index] = visited;
        else if (!ReferenceEquals(visited, node)) {
          list = new BaseFieldNode[count];
          for (var i = 0; i < index; i++)
            list[i] = nodes[i];
          list[index] = visited;
        }
        index++;
      }
      return list==null
        ? nodes
        : new ReadOnlyCollection<BaseFieldNode>(list);
    }

    public virtual Node VisitKeyExtractorNode<T>(KeyExtractorNode<T> keyExtractorNode)
    {
      var nestedNodes = VisitNodeList(keyExtractorNode.NestedNodes);
      if (ReferenceEquals(keyExtractorNode.NestedNodes, nestedNodes))
        return keyExtractorNode;
      return new KeyExtractorNode<T>(keyExtractorNode.KeyExtractor, nestedNodes);
    }

    public virtual Node VisitFieldNode(FieldNode fieldNode)
    {
      return fieldNode;
    }

    public virtual Node VisitReferenceNode(ReferenceNode referenceNode)
    {
      var nestedNodes = VisitNodeList(referenceNode.NestedNodes);
      if (ReferenceEquals(referenceNode.NestedNodes, nestedNodes))
        return referenceNode;
      return new ReferenceNode(
        referenceNode.Path, 
        referenceNode.Field, 
        referenceNode.ReferenceType, nestedNodes);
    }

    public virtual Node VisitSetNode(SetNode setNode)
    {
      var nestedNodes = VisitNodeList(setNode.NestedNodes);
      if (ReferenceEquals(setNode.NestedNodes, nestedNodes))
        return setNode;
      return new SetNode(
        setNode.Path,
        setNode.Field,
        setNode.ElementType, nestedNodes);
    }
  }
}