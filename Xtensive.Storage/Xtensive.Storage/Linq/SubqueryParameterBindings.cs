// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.03.18

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core.Disposing;
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

    public IDisposable Bind(IEnumerable<ParameterExpression> parameters)
    {
      Binding binding;
      foreach (var key in parameters)
        if (bindings.TryGetValue(key, out binding))
          binding.Cardinality++;
        else {
          binding = new Binding {Cardinality = 1, Parameter = new Parameter<Tuple>("subqueryParameter")};
          bindings.Add(key, binding);
        }
      return new Disposable<ParameterExpression[]> (parameters.ToArray(), Unbind);
    }

    public Parameter<Tuple> GetBound(ParameterExpression parameter)
    {
      return bindings[parameter].Parameter;
    }

    public bool TryGetBound(ParameterExpression parameter, out Parameter<Tuple> result)
    {
      Binding binding;
      if (bindings.TryGetValue(parameter, out binding)) {
        result = binding.Parameter;
        return true;
      }
      result = null;
      return false;
    }

    public bool IsBound(ParameterExpression parameter)
    {
      return bindings.ContainsKey(parameter);
    }

    public void InvalidateParameter(ParameterExpression parameter)
    {
      bindings[parameter].Parameter = new Parameter<Tuple>();
    }

    private void Unbind(bool disposing, ParameterExpression[] parameters)
    {
      if (!disposing)
        return;

      foreach (var key in parameters) {
        var binding = bindings[key];
        if (binding.Cardinality == 1)
          bindings.Remove(key);
        else
          binding.Cardinality--;
      }
    }
  }
}
