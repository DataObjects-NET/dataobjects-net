// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.01.21

using System;
using Xtensive.Sql;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// An attribute that must be applied to <see cref="HandlerFactory"/>
  /// to make it available for the storage.
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  public sealed class ProviderAttribute : Attribute
  {
    /// <summary>
    /// Gets provider name.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets <see cref="SqlDriverFactory"/> implementation.
    /// </summary>
    public Type DriverFactory { get; private set; }


    // Constructors

    public ProviderAttribute(string name, Type driverFactory)
    {
      Name = name;
      DriverFactory = driverFactory;
    }
  }
}