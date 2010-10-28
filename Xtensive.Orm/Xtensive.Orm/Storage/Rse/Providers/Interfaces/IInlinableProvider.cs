// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.14

namespace Xtensive.Storage.Rse.Providers
{
  /// <summary>
  /// A compilable provider, which columns can be inlined during translation to SQL.
  /// </summary>
  public interface IInlinableProvider
  {
    /// <summary>
    /// Gets a value indicating whether columns of a provider should be inlined.
    /// For non-SQL storages this property has no effect on compilation.
    /// </summary>
    bool IsInlined { get; }
  }
}