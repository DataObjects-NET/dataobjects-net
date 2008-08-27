// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.17

using System;
using System.Diagnostics;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Threading;

namespace Xtensive.Core.Serialization.Implementation
{
  /// <summary>
  /// Default <see cref="IValueSerializer{T}"/> provider. 
  /// Provides default primitive serializer for the specified type.
  /// </summary>
  /// <typeparam name="TStream">Type of the stream to write to or read from.</typeparam>
  [Serializable]
  public class ValueSerializerProvider<TStream> : AssociateProvider,
    IValueSerializerProvider<TStream>
  {
    private ThreadSafeDictionary<Type, IValueSerializer<TStream>> serializers =
      ThreadSafeDictionary<Type, IValueSerializer<TStream>>.Create(new object());

    #region IValueSerializerProvider members

    /// <inheritdoc/>
    public virtual ValueSerializer<TStream, T> GetSerializer<T>() 
    {
      return GetAssociate<T, IValueSerializer<TStream, T>, ValueSerializer<TStream, T>>();
    }

    /// <inheritdoc/>
    public IValueSerializer<TStream> GetSerializer(Type type) 
    {
      return serializers.GetValue(type,
        (_type, _this) => _this
          .GetType()
          .GetMethod("InnerGetSerializer", ArrayUtils<Type>.EmptyArray)
          .GetGenericMethodDefinition()
          .MakeGenericMethod(new[] {_type})
          .Invoke(_this, null)
          as IValueSerializer<TStream>,
        this);
    }

    /// <inheritdoc/>
    public IValueSerializer<TStream> GetSerializerByInstance(object instance) 
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
      return (TResult) (object) new ValueSerializer<TStream, TKey>((IValueSerializer<TStream, TKey>) associate);
    }

    #endregion

    #region Private \ internal methods

    // ReSharper disable UnusedPrivateMember
    private IValueSerializer<TStream> InnerGetSerializer<T>()
    {
      return GetAssociate<T, IValueSerializer<TStream, T>, ValueSerializer<TStream, T>>().Implementation;
    }
    // ReSharper restore UnusedPrivateMember

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    protected ValueSerializerProvider() {
      TypeSuffixes = new[] {"ValueSerializer"};
      Type t = typeof (ValueSerializerProvider<TStream>);
      AddHighPriorityLocation(t.Assembly, t.Namespace);
    }
  }
}