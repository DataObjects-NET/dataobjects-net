// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.22

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.IoC;

namespace Xtensive.Arithmetic
{
  /// <summary>
  /// Default <see cref="IArithmetic{T}"/> provider. 
  /// Provides default arithmetic for the specified type.
  /// </summary>
  /// <remarks>
  /// <para id="About"><see cref="HasStaticDefaultDocTemplate" copy="true" /></para>
  /// </remarks>
  /// <assert>
  /// <summary>
  /// Default <see cref="IArithmetic{T}"/> provider. 
  /// Provides default arithmetic for the specified type.
  /// </summary>
  /// <remarks>
  /// <para id="About">
  /// This class has default instance - use its <see cref="Default"/>
  /// property to get it.
  /// </para>
  /// </remarks>
  /// </assert>
  [Serializable]
  public class ArithmeticProvider : AssociateProvider,
    IArithmeticProvider
  {
    private static readonly ArithmeticProvider @default = new ArithmeticProvider();

    /// <see cref="HasStaticDefaultDocTemplate.Default" copy="true" />
    public static IArithmeticProvider Default
    {
      [DebuggerStepThrough]
      get { return @default; }
    }

    #region IArithmeticProvider members

    /// <summary>
    /// Gets <see cref="IArithmetic{T}"/> for the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Type to get the arithmetic for.</typeparam>
    /// <returns><see cref="IArithmetic{T}"/> for the specified type <typeparamref name="T"/>.</returns>
    public Arithmetic<T> GetArithmetic<T>()
    {
      return GetAssociate<T, IArithmetic<T>, Arithmetic<T>>();
    }

    #endregion

    #region Protected method overrides

    /// <inheritdoc/>
    protected override TResult ConvertAssociate<TKey, TAssociate, TResult>(TAssociate associate)
    {
      if (ReferenceEquals(associate, null))
        return default(TResult);
      else
        return (TResult) (object) new Arithmetic<TKey>((IArithmetic<TKey>) associate);
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    protected ArithmeticProvider()
    {
      TypeSuffixes = new[] {"Arithmetic"};
      ConstructorParams = new object[] {this, new ArithmeticRules(NullBehavior.Default, OverflowBehavior.Default)};
      Type type = typeof (ArithmeticProvider);
      AddHighPriorityLocation(type.Assembly, type.Namespace);
    }
  }
}