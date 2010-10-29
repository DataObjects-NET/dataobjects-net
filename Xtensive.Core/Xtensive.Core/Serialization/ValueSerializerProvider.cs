// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.17

using System;
using System.Diagnostics;
using System.Reflection;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Serialization.Binary;
using Xtensive.Internals.DocTemplates;
using Xtensive.IoC;
using Xtensive.Threading;

namespace Xtensive.Serialization
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
    private static readonly ValueSerializerProvider @default = 
      new ValueSerializerProvider();
    private ThreadSafeDictionary<Type, IValueSerializer> serializers =
      ThreadSafeDictionary<Type, IValueSerializer>.Create(new object());

    /// <see cref="HasStaticDefaultDocTemplate.Default" copy="true" />
    public static ValueSerializerProvider Default {
      [DebuggerStepThrough]
      get { return @default; }
    }

    /// <summary>
    /// The same as <see cref="GetSerializer{T}"/>, but non-virtual.
    /// </summary>
    /// <typeparam name="T">The type of serializer to get.</typeparam>
    /// <returns>Found serializer or <see langword="null" />.</returns>
    public ValueSerializer<T> FastGetSerializer<T>() 
    {
      return GetAssociate<T, IValueSerializer<T>, ValueSerializer<T>>();
    }

    #region IValueSerializerProvider members

    /// <inheritdoc/>
    public virtual ValueSerializer<T> GetSerializer<T>() 
    {
      return GetAssociate<T, IValueSerializer<T>, ValueSerializer<T>>();
    }

    /// <inheritdoc/>
    public IValueSerializer GetSerializer(Type type) 
    {
      return serializers.GetValue(type,
        (_type, _this) => _this
          .GetType()
          .GetMethod("InnerGetISerializer",
            BindingFlags.Instance | 
              BindingFlags.NonPublic, 
            null, ArrayUtils<Type>.EmptyArray, null)
          .GetGenericMethodDefinition()
          .MakeGenericMethod(new[] {_type})
          .Invoke(_this, null)
          as IValueSerializer,
        this);
    }

    /// <inheritdoc/>
    public IValueSerializer GetSerializerByInstance(object instance) 
    {
      ArgumentValidator.EnsureArgumentNotNull(instance, "instance");
      return GetSerializer(instance.GetType());
    }

    #endregion

    #region Protected method overrides

    /// <inheritdoc/>
    protected override TResult ConvertAssociate<TKey, TAssociate, TResult>(TAssociate associate) 
    {
      if (ReferenceEquals(associate, null))
        return default(TResult);
      return (TResult) (object) new ValueSerializer<TKey>((IValueSerializer<TKey>) associate);
    }

    #endregion

    #region Private \ internal methods

    protected IValueSerializer InnerIGetSerializer<T>()
    {
      var a = GetAssociate<T, IValueSerializer<T>, ValueSerializer<T>>();
      if (a!=null)
        return a.Implementation;
      else
        return null;
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    protected ValueSerializerProvider() 
    {
      TypeSuffixes = new[] {"ValueSerializer"};
      Type t = typeof (ArrayValueSerializer<>);
      AddHighPriorityLocation(t.Assembly, t.Namespace);
    }
  }
}