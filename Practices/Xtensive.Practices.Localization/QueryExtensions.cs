// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.12.16

using System.Linq;
using Xtensive.Orm;

namespace Xtensive.Practices.Localization
{
  /// <summary>
  /// Query root for localizable & localization entities.
  /// </summary>
  public static class QueryExtensions
  {
    /// <summary>
    /// Starting point for every query for localizable entities.
    /// </summary>
    /// <typeparam name="TTarget">The type of the target.</typeparam>
    /// <typeparam name="TLocalization">The type of the localization.</typeparam>
    /// <returns></returns>
    public static IQueryable<LocalizationPair<TTarget, TLocalization>> All<TTarget, TLocalization>(this Session.QueryEndpoint query) where TTarget: Entity where TLocalization: Localization<TTarget>
    {
      return from target in query.All<TTarget>()
      join localization in query.All<TLocalization>()
        on target equals localization.Target
      where localization.CultureName==LocalizationContext.Current.CultureName
      select new LocalizationPair<TTarget, TLocalization>(target, localization);
    }
  }
}