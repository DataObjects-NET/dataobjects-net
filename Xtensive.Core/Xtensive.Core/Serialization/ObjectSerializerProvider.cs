// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.17

using System;
using System.Diagnostics;
using System.Reflection;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Threading;

namespace Xtensive.Core.Serialization
{
  /// <summary>
  /// Default <see cref="IObjectSerializer{T}"/> provider. 
  /// Provides default serializer for the specified type.
  /// </summary>
  /// <remarks>
  /// <para id="About"><see cref="HasStaticDefaultDocTemplate" copy="true" /></para>
  /// </remarks>
  [Serializable]
  public class ObjectSerializerProvider : AssociateProvider,
    IObjectSerializerProvider
  {
    private static readonly ObjectSerializerProvider @default = new ObjectSerializerProvider();
    private ThreadSafeDictionary<Type, IObjectSerializer> serializers =
      ThreadSafeDictionary<Type, IObjectSerializer>.Create(new object());
    private ThreadSafeCached<IObjectSerializer> objectSerializer = 
      ThreadSafeCached<IObjectSerializer>.Create(new object());

    /// <see cref="HasStaticDefaultDocTemplate.Default" copy="true" />
    public static ObjectSerializerProvider Default {
      [DebuggerStepThrough]
      get { return @default; }
    }

    #region ISerializerProvider members

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

    // ReSharper disable UnusedPrivateMember
    private IObjectSerializer InnerGetSerializer<T>()
    {
      var a = GetAssociate<T, IObjectSerializer<T>, ObjectSerializer<T>>();
      if (a!=null)
        return a.Implementation;
      else
        return null;
    }
    // ReSharper restore UnusedPrivateMember

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    private ObjectSerializerProvider() 
    {
      TypeSuffixes = new[] {"Serializer"};
      Type t = typeof (ObjectSerializerProvider);
      AddHighPriorityLocation(t.Assembly, t.Namespace);
    }
  }
}