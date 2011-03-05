// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.15

namespace Xtensive.Conversion
{
  /// <summary>
  /// Tagging interface for any converter supported by
  /// <see cref="AdvancedConverterProvider"/>.
  /// </summary>
  public interface IAdvancedConverterBase
  {
    /// <summary>
    /// Gets the provider this converter is associated with.
    /// </summary>
    IAdvancedConverterProvider Provider { get; }
  }
}