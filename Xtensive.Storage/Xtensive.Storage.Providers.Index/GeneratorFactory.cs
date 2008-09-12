// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.11

using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Providers.Index
{
  /// <summary>
  /// Generator factory
  /// </summary>
  public class GeneratorFactory : Providers.GeneratorFactory
  {
    /// <inheritdoc/>
    protected override Generator CreateGenerator<TFieldType>(HierarchyInfo hierarchy)
    {
      return new IncrementalGenerator<TFieldType>(hierarchy);
    }
  }
}