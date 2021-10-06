// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.21

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Xtensive.Core;


namespace Xtensive.Conversion
{
  /// <summary>
  /// Default <see cref="IAdvancedConverter{TFrom,TTo}"/> provider. 
  /// Provides default converter for specified types.
  /// </summary>
  [Serializable]
  public class AdvancedConverterProvider : AssociateProvider, IAdvancedConverterProvider
  {
    /// <summary>
    /// Zero time point (for conversion of <see cref="DateTime"/> to e.g. <see cref="int"/>).
    /// </summary>
    public  static readonly DateTime ZeroTime = new DateTime(2000, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
    private static readonly AdvancedConverterProvider @default = new AdvancedConverterProvider();

    private static readonly AsyncLocal<Dictionary<Pair<Type>, bool>> inProgressAsync = new AsyncLocal<Dictionary<Pair<Type>, bool>>();

    private static Dictionary<Pair<Type>, bool> InProgress
    {
      get {
        var returnedValue = inProgressAsync.Value;
        if (returnedValue==null)
          inProgressAsync.Value = returnedValue = new Dictionary<Pair<Type>, bool>();
        return returnedValue;
      }
    }

    /// <summary>
    /// Gets default instance of this type.
    /// </summary>
    public static IAdvancedConverterProvider Default
    {
      [DebuggerStepThrough]
      get { return @default; }
    }

    /// <inheritdoc/>
    public virtual AdvancedConverter<TFrom, TTo> GetConverter<TFrom, TTo>()
    {
      return GetAssociate<TFrom, TTo, IAdvancedConverter<TFrom, TTo>, AdvancedConverter<TFrom, TTo>>();
    }

    /// <inheritdoc/>
    public virtual DateTime BaseTime
    {
      [DebuggerStepThrough]
      get { return ZeroTime; }
    }

    /// <inheritdoc/>
    protected override TAssociate PreferAssociate<TKey1, TKey2, TAssociate>(
      TAssociate associate1, TAssociate associate2)
    {
      if (associate1 is IAdvancedConverter<TKey1, TKey2> && ((IAdvancedConverter<TKey1, TKey2>) associate1).IsRough) {
        if (!(associate2 is IAdvancedConverter<TKey2, TKey1> && ((IAdvancedConverter<TKey2, TKey1>) associate2).IsRough))
          return associate2;
      }
      else if (associate2 is IAdvancedConverter<TKey2, TKey1> && ((IAdvancedConverter<TKey2, TKey1>) associate2).IsRough)
        return associate1;
      return base.PreferAssociate<TKey1, TKey2, TAssociate>(associate1, associate2);
    }

    /// <inheritdoc/>
    protected override TAssociate CreateCustomAssociate<TKey1, TKey2, TAssociate>()
    {
      Pair<Type> keyTypePair = new Pair<Type>(typeof (TKey1), typeof(TKey2));
      if (!InProgress.TryAdd(keyTypePair, true)) {
        throw new InvalidOperationException(Strings.ExRecursiveAssociateLookupDetected);
      }
      try {
        TAssociate associate = base.CreateCustomAssociate<TKey1, TKey2, TAssociate>();
        if (associate!=null)
          return associate;
        IAdvancedConverterFactory<TKey1> f1 = base.GetAssociate<TKey1, IAdvancedConverterFactory<TKey1>, IAdvancedConverterFactory<TKey1>>();
        if (f1!=null) {
          associate = f1.CreateForwardConverter<TKey2>() as TAssociate;
          if (associate!=null)
            return associate;
        }
        IAdvancedConverterFactory<TKey2> f2 = base.GetAssociate<TKey2, IAdvancedConverterFactory<TKey2>, IAdvancedConverterFactory<TKey2>>();
        if (f2!=null) {
          associate = f2.CreateBackwardConverter<TKey1>() as TAssociate;
          if (associate!=null)
            return associate;
        }
        return null;
      }
      finally {
        InProgress.Remove(keyTypePair);
      }
    }

    /// <inheritdoc/>
    protected override TResult ConvertAssociate<TKey1, TKey2, TAssociate, TResult>(TAssociate associate)
    {
      if (ReferenceEquals(associate, null))
        return default(TResult);
      else
        return (TResult)(object)new AdvancedConverter<TKey1, TKey2>((IAdvancedConverter<TKey1, TKey2>)associate);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    protected AdvancedConverterProvider()
    {
      TypeSuffixes = new string[] {"AdvancedConverter", "RoughAdvancedConverter", "AdvancedConverterFactory"};
      Type t = typeof (AdvancedConverterProvider);
      AddHighPriorityLocation(t.Assembly, t.Namespace);
    }
  }
}