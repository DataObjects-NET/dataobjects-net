// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.03.17

using System;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Linq.Expressions;
using Xtensive.Orm.Linq.Materialization;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Linq
{
  [Serializable]
  internal class Grouping<TKey, TElement> :
    SubQuery<TElement>,
    IGrouping<TKey, TElement>
  {
    public TKey Key { get; private set; }

    public Grouping(ProjectionExpression projectionExpression, TranslatedQuery translatedQuery, Parameter<Tuple> parameter, Tuple tuple, TKey key, ItemMaterializationContext context)
      : base(projectionExpression, translatedQuery, parameter, tuple, context)
    {
      Key = key;
    }
  }
}