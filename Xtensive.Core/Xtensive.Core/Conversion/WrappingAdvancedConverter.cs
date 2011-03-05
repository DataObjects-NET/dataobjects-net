// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.13

using System;

namespace Xtensive.Conversion
{
  /// <summary>
  /// Base class for any wrapping <see cref="IAdvancedConverter{TFrom,TTo}"/>s.
  /// </summary>
  [Serializable]
  public abstract class WrappingAdvancedConverter<TFrom, TFromBase, TTo, TToBase> : AdvancedConverterBase,
    IAdvancedConverter<TFrom, TTo>
  {
    /// <summary>
    /// Converter delegates for <typeparamref name="TFromBase"/>-<typeparamref name="TToBase"/> types.
    /// </summary>
    protected AdvancedConverterStruct<TFromBase, TToBase> BaseConverter;

    ///<summary>
    /// Converts specified value of <typeparamref name="TFrom"/> type
    /// to <typeparamref name="TTo"/> type.
    ///</summary>
    ///<param name="value">The value to convert.</param>
    ///<returns>Converted value.</returns>
    public abstract TTo Convert(TFrom value);

    /// <summary>
    /// Gets <see langword="true"/> if converter is rough, otherwise gets <see langword="false"/>.
    /// </summary>
    public virtual bool IsRough
    {
      get { return BaseConverter.IsRough; }
    }


    // Constructors

    /// <summary>
    /// Creates new instance of <see cref="WrappingAdvancedConverter{TFrom,TFromBase,TTo,TToBase}"/>.
    /// </summary>
    /// <param name="provider">Converter provider this converter is bound to.</param>
    public WrappingAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
      BaseConverter = provider.GetConverter<TFromBase, TToBase>();
    }
  }
}