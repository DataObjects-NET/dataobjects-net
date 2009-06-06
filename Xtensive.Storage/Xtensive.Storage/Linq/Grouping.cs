// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.03.17

using System;
using System.Linq;
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Linq.Expressions;

namespace Xtensive.Storage.Linq
{
  [Serializable]
  internal class Grouping<TKey, TElement> :
    SubQuery<TElement>,
    IGrouping<TKey, TElement>
  {
    public TKey Key { get; private set; }

    public Grouping(ProjectionExpression projectionExpression, TranslatedQuery translatedQuery, Parameter<Tuple> parameter, Tuple tuple, TKey key)
      : base(projectionExpression, translatedQuery, parameter, tuple)
    {
      Key = key;
    }
  }
}