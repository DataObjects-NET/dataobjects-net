// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.17

using System;
using System.Reflection;
using Xtensive.Collections;
using Xtensive.Internals.DocTemplates;
using Xtensive.IoC;
using Xtensive.Serialization.Internals;
using Xtensive.Threading;

namespace Xtensive.Serialization
{
  /// <summary>
  /// Base class for any <see cref="IObjectSerializer{T}"/> provider. 
  /// </summary>
  [Serializable]
  public abstract class ObjectSerializerProviderBase : AssociateProvider,
    IObjectSerializerProvider
  {
    private ThreadSafeDictionary<Type, IObjectSerializer> serializers =
      ThreadSafeDictionary<Type, IObjectSerializer>.Create(new object());
    private ThreadSafeCached<IObjectSerializer> objectSerializer = 
      ThreadSafeCached<IObjectSerializer>.Create(new object());

    #region IObjectSerializerProvider members

    /// <inheritdoc/>
    public virtual ObjectSerializer<T> GetSerializer<T>() 
    {
      return GetAssociate<T, IObjectSerializer<T>, ObjectSerializer<T>>();
    }

    /// <inheritdoc/>
    public IObjectSerializer GetSerializer(Type type) 
    {
      return serializers.GetValue(type,
        (_type, _this) => _this
          .GetType()
          .GetMethod("InnerGetSerializer",
            BindingFlags.Instance | 
            BindingFlags.NonPublic, 
            null, ArrayUtils<Type>.EmptyArray, null)
          .GetGenericMethodDefinition()
          .MakeGenericMethod(new[] {_type})
          .Invoke(_this, null) as IObjectSerializer,
        this);
    }

    /// <inheritdoc/>
    public IObjectSerializer GetSerializerByInstance(object instance) 
    {
      if (instance == null)
        return objectSerializer.GetValue(
          _this => _this.GetSerializer<object>().Implementation, 
          this);
      else
        return GetSerializer(instance.GetType());
    }

    public IValueSerializerProvider ValueSerializerProvider { get; protected set; }

    #endregion

    #region Protected method overrides

    /// <inheritdoc/>
    protected override TResult ConvertAssociate<TKey, TAssociate, TResult>(TAssociate associate) {
      if (ReferenceEquals(associate, null))
        return default(TResult);
      return (TResult) (object) new ObjectSerializer<TKey>((IObjectSerializer<TKey>) associate);
    }

    #endregion

    #region Private \ internal methods

    protected IObjectSerializer InnerGetSerializer<T>()
    {
      var a = GetAssociate<T, IObjectSerializer<T>, ObjectSerializer<T>>();
      if (a!=null)
        return a.Implementation;
      else
        return null;
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="valueSerializerProvider">The <see cref="ValueSerializerProvider"/> property value.</param>
    protected ObjectSerializerProviderBase(IValueSerializerProvider valueSerializerProvider) 
    {
      TypeSuffixes = new[] {"Serializer"};
      Type t = typeof (ReferenceSerializer);
      AddHighPriorityLocation(t.Assembly, t.Namespace);
      ValueSerializerProvider = valueSerializerProvider;
    }
  }
}