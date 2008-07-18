// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.15

using System;
using System.Collections.Generic;
using System.Linq;

namespace Xtensive.Storage.Building.Internals
{
  internal class CircularReferenceFinder<TNode> 
    where TNode : class 
  {
    internal Stack<TNode> Path { get; private set; }
    private readonly Func<TNode, string> toString;

    public CircularReferenceFinderScope<TNode> Enter(TNode node)
    {
      if (Path.Contains(node)) {
        Path.Push(node);
        string result = String.Join(" -> ", Path.ToArray().Reverse().Select(toString).ToArray());
        Path.Pop();
        throw new DomainBuilderException(string.Format("Circular reference was detected ({0}).", result));
      }
      Path.Push(node);
      return new CircularReferenceFinderScope<TNode>(this, node);
    }

    public CircularReferenceFinder()
    {
      Path = new Stack<TNode>();
    }

    public CircularReferenceFinder(Func<TNode, string> toString)
      : this()
    {
      this.toString = toString;
    }
  }
}