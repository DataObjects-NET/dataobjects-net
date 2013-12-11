// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.12.11

using System;
using System.Linq.Expressions;
using Xtensive.Core;

namespace Xtensive.Orm.Linq.Model
{
  internal static class QueryParser
  {
    public static GroupByQuery ParseGroupBy(MethodCallExpression mc)
    {
      var method = mc.Method.GetGenericMethodDefinition();

      if (method==QueryableMethodInfo.GroupBy)
        return new GroupByQuery {
          Source = mc.Arguments[0],
          KeySelector = mc.Arguments[1].StripQuotes(),
        };

      if (method==QueryableMethodInfo.GroupByWithElementSelector)
        return new GroupByQuery {
          Source = mc.Arguments[0],
          KeySelector = mc.Arguments[1].StripQuotes(),
          ElementSelector = mc.Arguments[2].StripQuotes(),
        };

      if (method==QueryableMethodInfo.GroupByWithResultSelector)
        return new GroupByQuery {
            Source = mc.Arguments[0],
            KeySelector = mc.Arguments[1].StripQuotes(),
            ResultSelector = mc.Arguments[2].StripQuotes(),
          };

      if (method==QueryableMethodInfo.GroupByWithElementAndResultSelectors)
        return new GroupByQuery {
          Source = mc.Arguments[0],
          KeySelector = mc.Arguments[1].StripQuotes(),
          ElementSelector = mc.Arguments[2].StripQuotes(),
          ResultSelector = mc.Arguments[3].StripQuotes()
        };

      throw new NotSupportedException(string.Format(
        Strings.ExGroupByOverloadXIsNotSupported,
        mc.ToString(true)));
    }
  }
}