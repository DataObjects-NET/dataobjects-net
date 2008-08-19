// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.17

using System;
using System.Diagnostics;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Serialization;
using Xtensive.Core.Threading;

namespace Xtensive.Core.Serialization
{
  /// <summary>
  /// Default <see cref="ISerializer{T}"/> provider. 
  /// Provides default primitive serializer for the specified type.
  /// </summary>
  /// <remarks>
  /// <para id="About"><see cref="HasStaticDefaultDocTemplate" copy="true" /></para>
  /// </remarks>
  [Serializable]
  public class SerializerProvider : AssociateProvider,
    ISerializerProvider
  {
    private static readonly SerializerProvider @default = new SerializerProvider();
    private ThreadSafeDictionary<Type, ISerializer> serializers = 
      ThreadSafeDictionary<Type, ISerializer>.Create(new object());

    /// <see cref="HasStaticDefaultDocTemplate.Default" copy="true" />
    public static ISerializerProvider Default
    {
      [DebuggerStepThrough]
      get { return @default; }
    }

    #region ISerializerProvider members

    /// <inheritdoc/>
    public Serializer<T> GetObjectSerializer<T>()
    {
      return GetAssociate<T, ISerializer<T>, Serializer<T>>();
    }

    /// <inheritdoc/>
    public ISerializer GetObjectSerializer(Type type)
    {
      return serializers.GetValue(type, 
        (_type, _this) => _this
          .GetType()
          .GetMethod("GetObjectSerializer", ArrayUtils<Type>.EmptyArray)
          .GetGenericMethodDefinition()
          .MakeGenericMethod(new[] {_type})
          .Invoke(_this, null) 
          as ISerializer, 
        this);
    }

    #endregion

    #region Protected method overrides

    protected override TResult ConvertAssociate<TKey, TAssociate, TResult>(TAssociate associate)
    {
      if (ReferenceEquals(associate, null))
        return default(TResult);
      else
        return (TResult)(object)new Serializer<TKey>((ISerializer<TKey>)associate);
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    private SerializerProvider()
    {
      TypeSuffixes = new[] {"Serializer"};
      Type t = typeof (SerializerProvider);
      AddHighPriorityLocation(t.Assembly, t.Namespace);
    }
  }
}