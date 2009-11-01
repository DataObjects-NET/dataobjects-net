// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.17

using System;
using System.Reflection;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Serialization;
using Xtensive.Core.Serialization.Binary;

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
    private readonly object _lock = new object();
    private ThreadSafeDictionary<Type, ISerializer> cache = ThreadSafeDictionary<Type, ISerializer>.Create();

    /// <see cref="HasStaticDefaultDocTemplate.Default" copy="true" />
    public static ISerializerProvider Default
    {
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
      ISerializer result = cache.GetValue(type);
      if (result!=null)
        return result;
      lock (_lock) {
        result = cache.GetValue(type);
        if (result!=null)
          return result;
        MethodInfo methodInfo =
          GetType()
            .GetMethod("GetObjectSerializer", ArrayUtils<Type>.EmptyArray)
            .GetGenericMethodDefinition()
            .MakeGenericMethod(new Type[] {type});
        result = (ISerializer)methodInfo.Invoke(this, null);
        cache.SetValue(type, result);
        return result;
      }
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
      TypeSuffixes = new string[] {"Serializer"};
      Type t = typeof (SerializerProvider);
      AddHighPriorityLocation(t.Assembly, t.Namespace);
    }
  }
}