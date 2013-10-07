// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.15

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Xtensive.Core;


namespace Xtensive.Conversion
{
  /// <summary>
  /// Base class for any advanced converter.
  /// </summary>
  [Serializable]
  public abstract class AdvancedConverterBase :
    IAdvancedConverterBase,
    IDeserializationCallback
  {
    private IAdvancedConverterProvider provider;

    /// <inheritdoc/>
    public IAdvancedConverterProvider Provider
    {
      [DebuggerStepThrough]
      get { return provider; }
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="provider">The provider this advanced converter is bound to.</param>
    public AdvancedConverterBase(IAdvancedConverterProvider provider)
    {
      ArgumentValidator.EnsureArgumentNotNull(provider, "provider");
      this.provider = provider;
    }

    /// <see cref="SerializableDocTemplate.OnDeserialization"/>
    public virtual void OnDeserialization(object sender)
    {
      if (provider==null || provider.GetType()==typeof (AdvancedConverterProvider))
        provider = AdvancedConverterProvider.Default;
    }
  }
}