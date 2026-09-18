// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.15

using System;


namespace Xtensive.Conversion
{
  /// <summary>
  /// Base class for any advanced converter.
  /// </summary>
  public abstract class AdvancedConverterBase :
    IAdvancedConverterBase
  {
    /// <inheritdoc/>
    public IAdvancedConverterProvider Provider { get; }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="provider">The provider this advanced converter is bound to.</param>
    public AdvancedConverterBase(IAdvancedConverterProvider provider)
    {
      Provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }
  }
}