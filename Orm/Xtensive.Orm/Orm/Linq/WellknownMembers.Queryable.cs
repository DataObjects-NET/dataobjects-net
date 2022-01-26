// Copyright (C) 2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Xtensive.Reflection;

namespace Xtensive.Orm.Linq
{
  internal static partial class WellKnownMembers
  {
    public static class Queryable
    {
      // public static readonly MethodInfo Aggregate;
      public static readonly MethodInfo All;
      public static readonly MethodInfo Any;
      public static readonly MethodInfo AnyWithPredicate;
      // public static readonly MethodInfo Append;
      public static readonly MethodInfo AsQueryable;
      public static readonly IReadOnlyDictionary<Type, MethodInfo> AverageMethodInfos;
      public static readonly IReadOnlyDictionary<Type, MethodInfo> AverageWithSelectorMethodInfos;
      public static readonly MethodInfo Cast;
      public static readonly MethodInfo Concat;
      public static readonly MethodInfo Contains;
      public static readonly MethodInfo Count;
      public static readonly MethodInfo CountWithPredicate;
      public static readonly MethodInfo DefaultIfEmpty;
      public static readonly MethodInfo DefaultIfEmptyWithDefaultValue;
      public static readonly MethodInfo Distinct;
      public static readonly MethodInfo ElementAt;
      public static readonly MethodInfo ElementAtOrDefault;
      public static readonly MethodInfo Except;
      public static readonly MethodInfo First;
      public static readonly MethodInfo FirstWithPredicate;
      public static readonly MethodInfo FirstOrDefault;
      public static readonly MethodInfo FirstOrDefaultWithPredicate;
      public static readonly MethodInfo GroupBy;
      public static readonly MethodInfo GroupByWithElementSelector;
      public static readonly MethodInfo GroupByWithElementAndResultSelector;
      public static readonly MethodInfo GroupByWithResultSelector;
      public static readonly MethodInfo GroupJoin;
      public static readonly MethodInfo Intersect;
      public static readonly MethodInfo Join;
      public static readonly MethodInfo Last;
      public static readonly MethodInfo LastWithPredicate;
      public static readonly MethodInfo LastOrDefault;
      public static readonly MethodInfo LastOrDefaultWithPredicate;
      public static readonly MethodInfo LongCount;
      public static readonly MethodInfo LongCountWithPredicate;
      public static readonly MethodInfo Max;
      public static readonly MethodInfo MaxWithSelector;
      public static readonly MethodInfo Min;
      public static readonly MethodInfo MinWithSelector;
      public static readonly MethodInfo OfType;
      public static readonly MethodInfo OrderBy;
      public static readonly MethodInfo OrderByDescending;
      public static readonly MethodInfo Reverse;
      public static readonly MethodInfo Select;
      public static readonly MethodInfo SelectMany;
      public static readonly MethodInfo Single;
      public static readonly MethodInfo SingleWithPredicate;
      public static readonly MethodInfo SingleOrDefault;
      public static readonly MethodInfo SingleOrDefaultWithPredicate;
      public static readonly MethodInfo Skip;
      public static readonly MethodInfo SkipLast;
      public static readonly MethodInfo SkipWhile;
      public static readonly IReadOnlyDictionary<Type, MethodInfo> SumMethodInfos;
      public static readonly IReadOnlyDictionary<Type, MethodInfo> SumWithSelectorMethodInfos;
      public static readonly MethodInfo Take;
      public static readonly MethodInfo TakeLast;
      public static readonly MethodInfo TakeWhile;
      public static readonly MethodInfo ThenBy;
      public static readonly MethodInfo ThenByDescending;
      public static readonly MethodInfo Union;
      public static readonly MethodInfo Where;

      // Queryable extensions
      public static readonly MethodInfo ExtensionCount = GetQueryableExtensionsMethod(nameof(QueryableExtensions.Count), 0, 1);
      public static readonly MethodInfo ExtensionLeftJoin = GetQueryableExtensionsMethod(nameof(QueryableExtensions.LeftJoin), 4, 5);
      public static readonly MethodInfo ExtensionLock = GetQueryableExtensionsMethod(nameof(QueryableExtensions.Lock), 1, 3);
      public static readonly MethodInfo ExtensionTake = GetQueryableExtensionsMethod(nameof(QueryableExtensions.Take), 1, 2);
      public static readonly MethodInfo ExtensionSkip = GetQueryableExtensionsMethod(nameof(QueryableExtensions.Skip), 1, 2);
      public static readonly MethodInfo ExtensionElementAt = GetQueryableExtensionsMethod(nameof(QueryableExtensions.ElementAt), 1, 2);
      public static readonly MethodInfo ExtensionElementAtOrDefault = GetQueryableExtensionsMethod(nameof(QueryableExtensions.ElementAtOrDefault), 1, 2);
      public static readonly MethodInfo ExtensionTag = GetQueryableExtensionsMethod(nameof(QueryableExtensions.Tag), 1, 2);

      static Queryable()
      {
        var averageMethodInfos = new Dictionary<Type, MethodInfo>();
        var averageWithSelectorMethodInfos = new Dictionary<Type, MethodInfo>();
        var sumMethodInfos = new Dictionary<Type, MethodInfo>();
        var sumWithSelectorMethodInfos = new Dictionary<Type, MethodInfo>();

        var queryableMethods = typeof(System.Linq.Queryable)
          .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
        foreach (var methodInfo in queryableMethods) {
          var parameters = methodInfo.GetParameters();
          switch (methodInfo.Name) {
            case nameof(System.Linq.Queryable.Aggregate):
              break;
            case nameof(System.Linq.Queryable.All):
              if (parameters.Length == 2) {
                All = methodInfo;
              }
              break;
            case nameof(System.Linq.Queryable.Any):
              switch (parameters.Length) {
                case 1:
                  Any = methodInfo;
                  break;
                case 2:
                  AnyWithPredicate = methodInfo;
                  break;
              }
              break;
            case nameof(System.Linq.Queryable.Append):
              break;
            case nameof(System.Linq.Queryable.AsQueryable):
              if (methodInfo.IsGenericMethod && parameters.Length == 1) {
                AsQueryable = methodInfo;
              }
              break;
            case nameof(System.Linq.Queryable.Average):
              switch (parameters.Length) {
                case 1:
                  var itemType = parameters[0].ParameterType.GetGenericArguments()[0];
                  averageMethodInfos.Add(itemType, methodInfo);
                  break;
                case 2:
                  itemType = GetLambdaFuncGenericArguments(parameters[1].ParameterType)[1];
                  averageWithSelectorMethodInfos.Add(itemType, methodInfo);
                  break;
              }
              break;
            case nameof(System.Linq.Queryable.Cast):
              if (parameters.Length == 1) {
                Cast = methodInfo;
              }
              break;
            case nameof(System.Linq.Queryable.Concat):
              if (parameters.Length == 2) {
                Concat = methodInfo;
              }
              break;
            case nameof(System.Linq.Queryable.Contains):
              if (parameters.Length == 2) {
                Contains = methodInfo;
              }
              break;
            case nameof(System.Linq.Queryable.Count):
              switch (parameters.Length) {
                case 1:
                  Count = methodInfo;
                  break;
                case 2:
                  CountWithPredicate = methodInfo;
                  break;
              }
              break;
            case nameof(System.Linq.Queryable.DefaultIfEmpty):
              switch (parameters.Length) {
                case 1:
                  DefaultIfEmpty = methodInfo;
                  break;
                case 2:
                  DefaultIfEmptyWithDefaultValue = methodInfo;
                  break;
              }
              break;
            case nameof(System.Linq.Queryable.Distinct):
              if (parameters.Length == 1) {
                Distinct = methodInfo;
              }
              break;
            case nameof(System.Linq.Queryable.ElementAt):
              if (parameters.Length == 2) {
                ElementAt = methodInfo;
              }
              break;
            case nameof(System.Linq.Queryable.ElementAtOrDefault):
              if (parameters.Length == 2) {
                ElementAtOrDefault = methodInfo;
              }
              break;
            case nameof(System.Linq.Queryable.Except):
              if (parameters.Length == 2) {
                Except = methodInfo;
              }
              break;
            case nameof(System.Linq.Queryable.First):
              switch (parameters.Length) {
                case 1:
                  First = methodInfo;
                  break;
                case 2:
                  FirstWithPredicate = methodInfo;
                  break;
              }
              break;
            case nameof(System.Linq.Queryable.FirstOrDefault):
              switch (parameters.Length) {
                case 1:
                  FirstOrDefault = methodInfo;
                  break;
                case 2:
                  FirstOrDefaultWithPredicate = methodInfo;
                  break;
              }
              break;
            case nameof(System.Linq.Queryable.GroupBy):
              switch (parameters.Length) {
                case 2:
                  GroupBy = methodInfo;
                  break;
                case 3:
                  var lambdaFuncGenericArguments = GetLambdaFuncGenericArguments(parameters[2].ParameterType);
                  if (lambdaFuncGenericArguments.Length == 2) {
                    if (lambdaFuncGenericArguments[1].IsGenericType) {
                      GroupByWithResultSelector = methodInfo;
                    }
                    else {
                      GroupByWithElementSelector = methodInfo;
                    }
                  }
                  break;
                case 4:
                  lambdaFuncGenericArguments = GetLambdaFuncGenericArguments(parameters[3].ParameterType);
                  if (lambdaFuncGenericArguments.Length == 3) {
                    GroupByWithElementAndResultSelector = methodInfo;
                  }
                  break;
              }
              break;
            case nameof(System.Linq.Queryable.GroupJoin):
              if (parameters.Length == 5) {
                GroupJoin = methodInfo;
              }
              break;
            case nameof(System.Linq.Queryable.Intersect):
              if (parameters.Length == 2) {
                Intersect = methodInfo;
              }
              break;
            case nameof(System.Linq.Queryable.Join):
              if (parameters.Length == 5) {
                Join = methodInfo;
              }
              break;
            case nameof(System.Linq.Queryable.Last):
              switch (parameters.Length) {
                case 1:
                  Last = methodInfo;
                  break;
                case 2:
                  LastWithPredicate = methodInfo;
                  break;
              }
              break;
            case nameof(System.Linq.Queryable.LastOrDefault):
              switch (parameters.Length) {
                case 1:
                  LastOrDefault = methodInfo;
                  break;
                case 2:
                  LastOrDefaultWithPredicate = methodInfo;
                  break;
              }
              break;
            case nameof(System.Linq.Queryable.LongCount):
              switch (parameters.Length) {
                case 1:
                  LongCount = methodInfo;
                  break;
                case 2:
                  LongCountWithPredicate = methodInfo;
                  break;
              }
              break;
            case nameof(System.Linq.Queryable.Max):
              switch (parameters.Length) {
                case 1:
                  Max = methodInfo;
                  break;
                case 2:
                  MaxWithSelector = methodInfo;
                  break;
              }
              break;
            case nameof(System.Linq.Queryable.Min):
              switch (parameters.Length) {
                case 1:
                  Min = methodInfo;
                  break;
                case 2:
                  MinWithSelector = methodInfo;
                  break;
              }
              break;
            case nameof(System.Linq.Queryable.OfType):
              if (parameters.Length == 1) {
                OfType = methodInfo;
              }
              break;
            case nameof(System.Linq.Queryable.OrderBy):
              if (parameters.Length == 2) {
                OrderBy = methodInfo;
              }
              break;
            case nameof(System.Linq.Queryable.OrderByDescending):
              if (parameters.Length == 2) {
                OrderByDescending = methodInfo;
              }
              break;
            case nameof(System.Linq.Queryable.Prepend):
              break;
            case nameof(System.Linq.Queryable.Reverse):
              if (parameters.Length == 1) {
                Reverse = methodInfo;
              }
              break;
            case nameof(System.Linq.Queryable.Select):
              if (parameters.Length == 2) {
                var lambdaFuncGenericArguments = GetLambdaFuncGenericArguments(parameters[1].ParameterType);
                if (lambdaFuncGenericArguments.Length == 2) {
                  Select = methodInfo;
                }
              }
              break;
            case nameof(System.Linq.Queryable.SelectMany):
              switch (parameters.Length) {
                case 2:
                  var lambdaFuncGenericArguments = GetLambdaFuncGenericArguments(parameters[1].ParameterType);
                  if (lambdaFuncGenericArguments.Length == 2) {
                    SelectMany = methodInfo;
                  }
                  break;
                case 3:
                  lambdaFuncGenericArguments = GetLambdaFuncGenericArguments(parameters[1].ParameterType);
                  if (lambdaFuncGenericArguments.Length == 2) {
                    SelectMany = methodInfo;
                  }
                  break;
              }
              break;
            case nameof(System.Linq.Queryable.SequenceEqual):
              break;
            case nameof(System.Linq.Queryable.Single):
              switch (parameters.Length) {
                case 1:
                  Single = methodInfo;
                  break;
                case 2:
                  SingleWithPredicate = methodInfo;
                  break;
              }
              break;
            case nameof(System.Linq.Queryable.SingleOrDefault):
              switch (parameters.Length) {
                case 1:
                  SingleOrDefault = methodInfo;
                  break;
                case 2:
                  SingleOrDefaultWithPredicate = methodInfo;
                  break;
              }
              break;
            case nameof(System.Linq.Queryable.Skip):
              if (parameters.Length == 2) {
                Skip = methodInfo;
              }
              break;
            case nameof(System.Linq.Queryable.SkipLast):
              if (parameters.Length == 2) {
                SkipLast = methodInfo;
              }
              break;
            case nameof(System.Linq.Queryable.SkipWhile):
              if (parameters.Length == 2) {
                var lambdaFuncGenericArguments = GetLambdaFuncGenericArguments(parameters[1].ParameterType);
                if (lambdaFuncGenericArguments.Length == 2) {
                  SkipWhile = methodInfo;
                }
              }
              break;
            case nameof(System.Linq.Queryable.Sum):
              switch (parameters.Length) {
                case 1:
                  var itemType = parameters[0].ParameterType.GetGenericArguments()[0];
                  sumMethodInfos.Add(itemType, methodInfo);
                  break;
                case 2:
                  itemType = GetLambdaFuncGenericArguments(parameters[1].ParameterType)[1];
                  sumWithSelectorMethodInfos.Add(itemType, methodInfo);
                  break;
              }
              break;
            case nameof(System.Linq.Queryable.Take):
              if (parameters.Length == 2) {
                Take = methodInfo;
              }
              break;
            case nameof(System.Linq.Queryable.TakeLast):
              if (parameters.Length == 2) {
                TakeLast = methodInfo;
              }
              break;
            case nameof(System.Linq.Queryable.TakeWhile):
              if (parameters.Length == 2) {
                var lambdaFuncGenericArguments = GetLambdaFuncGenericArguments(parameters[1].ParameterType);
                if (lambdaFuncGenericArguments.Length == 2) {
                  TakeWhile = methodInfo;
                }
              }
              break;
            case nameof(System.Linq.Queryable.ThenBy):
              if (parameters.Length == 2) {
                ThenBy = methodInfo;
              }
              break;
            case nameof(System.Linq.Queryable.ThenByDescending):
              if (parameters.Length == 2) {
                ThenByDescending = methodInfo;
              }
              break;
            case nameof(System.Linq.Queryable.Union):
              if (parameters.Length == 2) {
                Union = methodInfo;
              }
              break;
            case nameof(System.Linq.Queryable.Where):
              if (parameters.Length == 2) {
                var lambdaFuncGenericArguments = GetLambdaFuncGenericArguments(parameters[1].ParameterType);
                if (lambdaFuncGenericArguments.Length == 2) {
                  Where = methodInfo;
                }
              }
              break;
            case nameof(System.Linq.Queryable.Zip):
              break;
          }
        }
        AverageMethodInfos = new ReadOnlyDictionary<Type, MethodInfo>(averageMethodInfos);
        AverageWithSelectorMethodInfos = new ReadOnlyDictionary<Type, MethodInfo>(averageWithSelectorMethodInfos);
        SumMethodInfos = new ReadOnlyDictionary<Type, MethodInfo>(sumMethodInfos);
        SumWithSelectorMethodInfos = new ReadOnlyDictionary<Type, MethodInfo>(sumWithSelectorMethodInfos);
      }

      private static Type[] GetLambdaFuncGenericArguments(Type selectorType)
      {
        var lambdaFuncType = selectorType.IsOfGenericType(WellKnownTypes.ExpressionOfT)
          ? selectorType.GetGenericArguments()[0]
          : null;
        var lambdaFuncArguments = lambdaFuncType?.IsGenericType == true
          ? lambdaFuncType.GetGenericArguments()
          : Array.Empty<Type>();
        return lambdaFuncArguments;
      }
    }
  }
}