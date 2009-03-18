// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.18

using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;

namespace Xtensive.Modelling
{
  /// <summary>
  /// Property accessor.
  /// </summary>
  [Serializable]
  public sealed class PropertyAccessor : IDeserializationCallback
  {
    private Func<object, object> getter;
    private Action<object, object> setter;

    /// <summary>
    /// Gets <see cref="System.Reflection.PropertyInfo"/> of property 
    /// this accessor is bound to.
    /// </summary>
    public PropertyInfo PropertyInfo { get; private set; }

    /// <summary>
    /// Gets the property getter delegate.
    /// </summary>
    public Func<object, object> Getter {
      get { return getter; }
      private set { getter = value; }
    }

    /// <summary>
    /// Gets a value indicating whether this instance has getter.
    /// </summary>
    public bool HasGetter { get { return getter!=null; } }

    /// <summary>
    /// Gets the property setter delegate.
    /// </summary>
    public Action<object, object> Setter {
      get { return setter; }
      private set { setter = value; }
    }

    /// <summary>
    /// Gets a value indicating whether this instance has setter.
    /// </summary>
    public bool HasSetter { get { return setter!=null; } }

    private void Initialize()
    {
      var propertyInfo = PropertyInfo;
      var tType = propertyInfo.DeclaringType;
      var tProperty = propertyInfo.PropertyType;
      this.GetType()
        .GetMethod("Initialize", 
            BindingFlags.Instance | 
            BindingFlags.NonPublic, 
            null, ArrayUtils<Type>.EmptyArray, null)
        .GetGenericMethodDefinition()
        .MakeGenericMethod(new[] {tType, tProperty})
        .Invoke(this, null);
    }

    private void Initialize<TType, TProperty>()
    {
      var propertyInfo = PropertyInfo;
      if (propertyInfo.CanRead) {
        var d = DelegateHelper.CreateGetMemberDelegate<TType, TProperty>(PropertyInfo.Name);
        if (d!=null)
          getter = o => d((TType) o);
      }
      if (propertyInfo.CanRead) {
        var d = DelegateHelper.CreateSetMemberDelegate<TType, TProperty>(PropertyInfo.Name);
        if (d!=null)
          setter = (o,v) => d((TType) o, (TProperty) v);
      }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="propertyInfo">The property info this accessor is bound to.</param>
    public PropertyAccessor(PropertyInfo propertyInfo)
    {
      PropertyInfo = propertyInfo;
      Initialize();
    }

    // Deserialization

    /// <inheritdoc/>
    void IDeserializationCallback.OnDeserialization(object sender)
    {
      Initialize();
    }
  }
}