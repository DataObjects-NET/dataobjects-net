// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.12.11

using System.Linq.Expressions;

namespace Xtensive.Orm.Linq.Model
{
  internal static class QueryFactory
  {
    public static MethodCallExpression GroupBy(GroupByQuery groupBy)
    {
      var itemType = QueryHelper.GetSequenceElementType(groupBy.Source.Type);
      if (groupBy.ResultSelector!=null && groupBy.ElementSelector!=null) {
        var method = QueryableMethodInfo.GroupByWithElementAndResultSelectors
          .MakeGenericMethod(
            itemType,
            groupBy.KeySelector.Body.Type,
            groupBy.ElementSelector.Body.Type,
            groupBy.ResultSelector.Body.Type);
        return Expression.Call(method,
          groupBy.Source,
          groupBy.KeySelector,
          groupBy.ElementSelector,
          groupBy.ResultSelector);
      }
      if (groupBy.ResultSelector!=null && groupBy.ElementSelector==null) {
          var method = QueryableMethodInfo.GroupByWithResultSelector
            .MakeGenericMethod(
              itemType,
              groupBy.KeySelector.Body.Type,
              groupBy.ResultSelector.Body.Type);
          return Expression.Call(method,
            groupBy.Source,
            groupBy.KeySelector,
            groupBy.ResultSelector);
      }
      if (groupBy.ResultSelector==null && groupBy.ElementSelector!=null) {
          var method = QueryableMethodInfo.GroupByWithElementSelector
            .MakeGenericMethod(
              itemType,
              groupBy.KeySelector.Body.Type,
              groupBy.ElementSelector.Body.Type);
          return Expression.Call(method,
            groupBy.Source,
            groupBy.KeySelector,
            groupBy.ElementSelector);
      }
      {
        var method = QueryableMethodInfo.GroupBy
          .MakeGenericMethod(
            itemType,
            groupBy.KeySelector.Body.Type);
        return Expression.Call(method, groupBy.Source, groupBy.KeySelector);
      }
    }

    public static MethodCallExpression Select(Expression source, LambdaExpression projection)
    {
      var sourceItemType = projection.Parameters[0].Type;
      var resultItemType = projection.Body.Type;
      var method = QueryableMethodInfo.Select.MakeGenericMethod(sourceItemType, resultItemType);
      return Expression.Call(method, source, projection);
    }
  }
}