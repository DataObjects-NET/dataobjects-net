// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.04.23

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Linq.Expressions;
using Xtensive.Storage.Linq.Rewriters;

namespace Xtensive.Storage.Linq
{
  [Serializable]
  internal class SubQuery<T> : IQueryable<T>
  {
    private readonly ResultExpression resultExpression;


    public IEnumerator<T> GetEnumerator()
    {
        var result = resultExpression.GetResult<IEnumerable<T>>();
        foreach (var element in result)
          yield return element;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public Expression Expression
    {
      get
      {
        return resultExpression;
      }
    }

    public Type ElementType
    {
      get { return typeof (T); }
    }

    public IQueryProvider Provider
    {
      get { return QueryProvider.Current; }
    }

    public SubQuery(ResultExpression resultExpression, Tuple tuple, Parameter<Tuple> parameter)
    {
      var newRecordset = TupleParameterToTupleRewriter.Rewrite(
        resultExpression.RecordSet.Provider, 
        parameter, 
        tuple)
        .Result;
      this.resultExpression = new ResultExpression(
        resultExpression.Type, 
        newRecordset, 
        resultExpression.Mapping, 
        resultExpression.ItemProjector, 
        resultExpression.ResultType);
    }

  }
}