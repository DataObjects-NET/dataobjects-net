// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.09.03

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Sql.Dom.Resources;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  public class ComparisonRegistry
  {
    private readonly Dictionary<Pair<Node, Node>, IComparisonResult> registry = new Dictionary<Pair<Node, Node>, IComparisonResult>();

    public bool TryGetValue<T>(T originalNode, T newNode, out IComparisonResult comparisonResult)
      where T : Node
    {
      return registry.TryGetValue(new Pair<Node, Node>(originalNode, newNode), out comparisonResult);
    }

    public void Register(Node originalNode, Node newNode, IComparisonResult comparisonResult)
    {
      IComparisonResult currentResult;
      var key = new Pair<Node, Node>(originalNode, newNode);
      if (!registry.TryGetValue(key, out currentResult))
        registry.Add(key, comparisonResult);
      else {
        if (currentResult!=comparisonResult)
          throw new InvalidOperationException(Strings.ExComparisonResultAlreadyRegistered);
      }
    }
  }
}