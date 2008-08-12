// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.17

using System;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Serialization.Binary
{
  /// <summary>
  /// Default <see cref="IValueSerializer{T}"/> provider. 
  /// Provides default primitive serializer for the specified type.
  /// </summary>
  /// <remarks>
  /// <para id="About"><see cref="HasStaticDefaultDocTemplate" copy="true" /></para>
  /// </remarks>
  [Serializable]
  public class ValueSerializerProvider : AssociateProvider,
    IValueSerializerProvider
  {
    private static readonly ValueSerializerProvider @default = new ValueSerializerProvider();

    /// <see cref="HasStaticDefaultDocTemplate.Default" copy="true" />
    [DebuggerStepThrough]
    public static IValueSerializerProvider Default
    {
      get { return @default; }
    }

    #region ISerializerProvider members

    /// <inheritdoc/>
    public virtual ValueSerializer<T> GetSerializer<T>()
    {
      return base.GetAssociate<T, IValueSerializer<T>, ValueSerializer<T>>();
    }

    #endregion

    #region Protected method overrides

    /// <inheritdoc/>
    protected override TResult ConvertAssociate<TKey, TAssociate, TResult>(TAssociate associate)
    {
      if (ReferenceEquals(associate, null))
        return default(TResult);
      else
        return (TResult)(object)new ValueSerializer<TKey>((IValueSerializer<TKey>)associate);
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    private ValueSerializerProvider()
    {
      TypeSuffixes = new string[] {"ValueSerializer"};
      Type t = typeof (ValueSerializerProvider);
      AddHighPriorityLocation(t.Assembly, t.Namespace);
    }
  }
}