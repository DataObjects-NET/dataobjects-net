// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.03.18

using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Linq
{
  internal class SubqueryParameterBindings
  {
    private readonly Dictionary<ParameterExpression, Parameter<Tuple>> bindings
      = new Dictionary<ParameterExpression, Parameter<Tuple>>();
    private readonly Stack<ParameterExpression> stack = new Stack<ParameterExpression>();

    public ParameterExpression CurrentParameter { get { return stack.Peek(); } }

    public void Bind(IEnumerable<ParameterExpression> keys)
    {
      foreach (ParameterExpression key in keys) {
        var parameter = new Parameter<Tuple>();
        bindings.Add(key, parameter);
        stack.Push(key);
      }
    }

    public void Unbind(IEnumerable<ParameterExpression> keys)
    {
      foreach (ParameterExpression key in keys) {
        bindings.Remove(key);
        stack.Pop();
      }
    }

    public Parameter<Tuple> GetBound(ParameterExpression key)
    {
      return bindings[key];
    }

    public bool TryGetBound(ParameterExpression key, out Parameter<Tuple> value)
    {
      return bindings.TryGetValue(key, out value);
    }
  }
}
