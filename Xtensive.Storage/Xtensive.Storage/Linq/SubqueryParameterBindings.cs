// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.03.18

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Linq
{
  internal class SubqueryParameterBindings
  {
    private readonly Dictionary<object, Parameter<Tuple>> bindings = new Dictionary<object, Parameter<Tuple>>();
    private readonly Stack<object> stack = new Stack<object>();

    public object CurrentKey { get { return stack.Peek(); } }

    public void Bind(IEnumerable keys)
    {
      foreach (object key in keys) {
        var parameter = new Parameter<Tuple>();
        bindings.Add(key, parameter);
        stack.Push(key);
      }
    }

    public void Unbind(IEnumerable keys)
    {
      foreach (object key in keys) {
        bindings.Remove(key);
        stack.Pop();
      }
    }

    public Parameter<Tuple> GetBound(object key)
    {
      return bindings[key];
    }

    public bool TryGetBound(object key, out Parameter<Tuple> value)
    {
      return bindings.TryGetValue(key, out value);
    }
  }
}
