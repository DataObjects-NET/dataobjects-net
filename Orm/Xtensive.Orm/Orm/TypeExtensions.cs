// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.09.10

using System;
using Xtensive.Orm.Model;

namespace Xtensive.Orm
{
  /// <summary>
  /// Useful <see cref="Type"/>-related extensions.
  /// </summary>
  public static class TypeExtensions
  {
    /// <summary>
    /// Gets the corresponding <see cref="TypeInfo"/> instance.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns><see cref="TypeInfo"/> instance.</returns>
    /// <remarks>This method requires open <see cref="Session"/>.</remarks>
    public static TypeInfo GetTypeInfo(this Type type)
    {
      return GetTypeInfo(type, Domain.Demand());
    }

    /// <summary>
    /// Gets the corresponding <see cref="TypeInfo"/> instance.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="domain">The domain to look for <see cref="TypeInfo"/> within.</param>
    /// <returns><see cref="TypeInfo"/> instance.</returns>
    public static TypeInfo GetTypeInfo(this Type type, Domain domain)
    {
      return domain.Model.Types[type];
    }
  }
}