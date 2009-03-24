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
    private class Binding
    {
      public int Cardinality;
      public Parameter<Tuple> Parameter;
    }

    private readonly Dictionary<ParameterExpression, Binding> bindings = new Dictionary<ParameterExpression, Binding>();
    private readonly Stack<ParameterExpression> stack = new Stack<ParameterExpression>();

    public ParameterExpression CurrentParameter { get { return stack.Peek(); } }

    public void Bind(IEnumerable<ParameterExpression> keys)
    {
      Binding binding;
      foreach (ParameterExpression key in keys)
        if (bindings.TryGetValue(key, out binding))
          binding.Cardinality++;
        else {
          binding = new Binding { Cardinality = 1, Parameter = new Parameter<Tuple>()};
          bindings.Add(key, binding);
          stack.Push(key);
        }
    }

    public void Unbind(IEnumerable<ParameterExpression> keys)
    {
      foreach (ParameterExpression key in keys) {
        var binding = bindings[key];
        if (binding.Cardinality == 1)
          bindings.Remove(key);
        else
          binding.Cardinality--;
      }
    }

    public Parameter<Tuple> GetBound(ParameterExpression key)
    {
      return bindings[key].Parameter;
    }

    public bool TryGetBound(ParameterExpression key, out Parameter<Tuple> value)
    {
      Binding binding;
      if (bindings.TryGetValue(key, out binding)) {
        value = binding.Parameter;
        return true;
      }
      value = null;
      return false;
    }
  }
}
