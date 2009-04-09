// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.03.17

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Linq.Expressions;

namespace Xtensive.Storage.Linq
{
  [Serializable]
  internal class Grouping<TKey, TElement> :
    IGrouping<TKey, TElement>
  {
    private readonly TKey key;
    private readonly ResultExpression resultExpression;
    private readonly Parameter<Tuple> tupleParameter;
    private readonly Tuple keyTuple;

    public TKey Key
    {
      get { return key; }
    }

    public IEnumerator<TElement> GetEnumerator()
    {
      using (new ParameterScope()) {
        tupleParameter.Value = keyTuple;
        var result = resultExpression.GetResult<IEnumerable<TElement>>();
        foreach (var element in result)
          yield return element;
      }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public Grouping(TKey key, Tuple keyTuple, ResultExpression resultExpression, Parameter<Tuple> tupleParameter)
    {
      this.resultExpression = resultExpression;
      this.tupleParameter = tupleParameter;
      this.keyTuple = keyTuple;
      this.key = key;
    }
  }
}