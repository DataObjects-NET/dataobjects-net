// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.12.28

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Xtensive.Orm.Localization
{
  internal static class LocalizationExpressionBuilder
  {
    private static readonly MethodInfo BuildExpressionMethodInfo = typeof(LocalizationExpressionBuilder).GetMethod("BuildExpression", BindingFlags.NonPublic | BindingFlags.Static);

    public static LambdaExpression BuildExpression(TypeLocalizationInfo localizationInfo, MemberExpression me, string defaultCultureName)
    {
      return (LambdaExpression)BuildExpressionMethodInfo
        .MakeGenericMethod(localizationInfo.LocalizationTypeInfo.UnderlyingType)
        .Invoke(null, new object[] { me.Member.Name, defaultCultureName });

    }

    // Helper method for building lambda expression in generic way
    private static LambdaExpression BuildExpression<TLocalization>(string propertyName, string defaultCultureName)
      where TLocalization : Localization
    {
      Expression<Func<LocalizationSet<TLocalization>, string>> result = set => set
        .Where(localization => localization.CultureName == LocalizationContext.Current.CultureName)
        .Select(localization => (string)localization[propertyName])
        .FirstOrDefault()
          ??
          set
            .Where(localization => localization.CultureName == defaultCultureName)
            .Select(localization => (string)localization[propertyName])
            .FirstOrDefault();

      return result;
    }
  }
}