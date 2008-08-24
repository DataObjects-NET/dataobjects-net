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

namespace Xtensive.Core.Serialization
{
  /// <summary>
  /// Default <see cref="IObjectSerializer{T}"/> provider. 
  /// Provides default primitive serializer for the specified type.
  /// </summary>
  /// <remarks>
  /// <para id="About"><see cref="HasStaticDefaultDocTemplate" copy="true" /></para>
  /// </remarks>
  [Serializable]
  public class ObjectSerializerProvider :
    AssociateProvider,
    IObjectSerializerProvider
  {
    private static readonly ObjectSerializerProvider @default = new ObjectSerializerProvider();

    private ThreadSafeDictionary<Type, IObjectSerializer> serializers =
      ThreadSafeDictionary<Type, IObjectSerializer>.Create(new object());

    /// <see cref="HasStaticDefaultDocTemplate.Default" copy="true" />
    [DebuggerHidden]
    public static IObjectSerializerProvider Default {
      get { return @default; }
    }

    #region ISerializerProvider members

    /// <inheritdoc/>
    public virtual IObjectSerializer<T> GetSerializer<T>() {
      return GetAssociate<T, IObjectSerializer<T>, IObjectSerializer<T>>();
    }

    /// <inheritdoc/>
    public IObjectSerializer GetSerializer(Type type) {
      return serializers.GetValue(type,
        (_type, _this) => _this
          .GetType()
          .GetMethod("GetSerializer", ArrayUtils<Type>.EmptyArray)
          .GetGenericMethodDefinition()
          .MakeGenericMethod(new[] {_type})
          .Invoke(_this, null) as IObjectSerializer,
        this);
    }

    /// <inheritdoc/>
    public IObjectSerializer GetSerializerByInstance(object instance) {
      ArgumentValidator.EnsureArgumentNotNull(instance, "instance");
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

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    private ObjectSerializerProvider() {
      TypeSuffixes = new[] {"Serializer"};
      Type t = typeof (ObjectSerializerProvider);
      AddHighPriorityLocation(t.Assembly, t.Namespace);
    }
  }
}