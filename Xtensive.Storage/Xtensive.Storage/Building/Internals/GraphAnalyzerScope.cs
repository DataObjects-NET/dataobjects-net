// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.15

using System;

namespace Xtensive.Storage.Building.Internals
{
  internal class GraphAnalyzerScope<TNode> : IDisposable
    where TNode : class 
  {
    public TNode Node { get; private set; }
    public GraphAnalyzer<TNode> Analyzer { get; private set; }


    // Constructors

    public GraphAnalyzerScope(GraphAnalyzer<TNode> analyzer, TNode node)
    {
      Analyzer = analyzer;
      Node = node;
    }

    public void Dispose()
    {
      if (Analyzer.Path.Peek() == Node)
        Analyzer.Path.Pop();
    }
  }
}