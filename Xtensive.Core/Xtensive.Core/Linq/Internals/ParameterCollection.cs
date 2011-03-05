// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.06

using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core;

namespace Xtensive.Linq
{
  internal sealed class ParameterCollection
  {
    private readonly List<ParameterExpression> indexes = new List<ParameterExpression>();

    public int GetIndex(ParameterExpression parameter)
    {
      int result = indexes.IndexOf(parameter);
      if (result < 0)
        throw Exceptions.LambdaParameterIsOutOfScope(parameter);
      return result;
    }
    
    public void Add(ParameterExpression parameter)
    {
      indexes.Add(parameter);
    }

    public void AddRange(IEnumerable<ParameterExpression> parameters)
    {
      foreach (var p in parameters)
        Add(p);
    }
    
    public void Reset()
    {
      indexes.Clear();
    }
  }
}