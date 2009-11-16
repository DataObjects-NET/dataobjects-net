// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.18

using System;
using System.Reflection;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;
using Xtensive.Modelling.Attributes;

namespace Xtensive.Modelling
{
  /// <summary>
  /// Property accessor.
  /// </summary>
  public sealed class PropertyAccessor
  {
    private Func<object, object> getter;
    private Action<object, object> setter;
    [NonSerialized]
    private bool isSystem;
    [NonSerialized]
    private int priority;
    [NonSerialized]
    private bool ignoreInComparison;
    [NonSerialized]
    private bool isImmutable;
    [NonSerialized]
    private bool isMutable;
    [NonSerialized]
    private Type dependencyRootType;

    /// <summary>
    /// Gets <see cref="System.Reflection.PropertyInfo"/> of property 
    /// this accessor is bound to.
    /// </summary>
    public PropertyInfo PropertyInfo { get; private set; }

    /// <summary>
    /// Gets a value indicating whether underlying property is system.
    /// </summary>
    public bool IsSystem {
      get { return isSystem; }
    }

    /// <summary>
    /// Gets the <see cref="PropertyAttribute.Priority"/> of the property.
    /// </summary>
    public int Priority {
      get { return priority; }
    }

    /// <summary>
    /// Gets a value indicating whether underlying property must be ignored in comparison.
    /// </summary>
    public bool IgnoreInComparison {
      get { return ignoreInComparison; }
    }

    /// <summary>
    /// Gets a value indicating whether underlying property value must be re-created
    /// rather than created &amp; processed as usual.
    /// </summary>
    public bool IsImmutable {
      get { return isImmutable; }
    }

    /// <summary>
    /// Gets a value indicating whether underlying property must be 
    /// ignored during recreation of parent immutable property.
    /// </summary>
    public bool IsMutable {
      get { return isMutable; }
    }

    /// <summary>
    /// Gets the dependency root type.
    /// </summary>
    public Type DependencyRootType {
      get { return dependencyRootType; }
    }

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
    /// Gets or sets the default property value.
    /// </summary>
    public object Default { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this instance has setter.
    /// </summary>
    public bool HasSetter { get { return setter!=null; } }

    /// <summary>
    /// Gets the dependency root object.
    /// </summary>
    public IPathNode GetDependencyRoot(IPathNode source) 
    {
      if (source==null)
        return null;
      if (DependencyRootType.IsAssignableFrom(source.GetType()))
        return source;
      var current = source;
      while (null != (current = current.Parent)) {
        if (DependencyRootType.IsAssignableFrom(current.GetType()))
          return current;
      }
      return null;
    }

    public bool IsDataDependent { get; private set; }

    private void Initialize()
    {
      var propertyInfo = PropertyInfo;
      var tType = propertyInfo.DeclaringType;
      var tProperty = propertyInfo.PropertyType;
      var sa = propertyInfo.GetAttribute<SystemPropertyAttribute>(AttributeSearchOptions.InheritNone);
      isSystem = sa!=null;
      ignoreInComparison = isSystem;
      isMutable = isSystem;
      isImmutable = false;
      var pa = propertyInfo.GetAttribute<PropertyAttribute>(AttributeSearchOptions.InheritNone);
      if (pa!=null) {
        priority = pa.Priority;
        ignoreInComparison |= pa.IgnoreInComparison;
        isMutable |= pa.IsMutable;
        isImmutable |= pa.IsImmutable;
        dependencyRootType = pa.DependencyRootType;
      }
      this.GetType()
        .GetMethod("InnerInitialize", 
            BindingFlags.Instance | 
            BindingFlags.NonPublic, 
            null, ArrayUtils<Type>.EmptyArray, null)
        .GetGenericMethodDefinition()
        .MakeGenericMethod(new[] {tType, tProperty})
        .Invoke(this, null);
    }

    private void InnerInitialize<TType, TProperty>()
    {
      Default = default(TProperty);
      var propertyInfo = PropertyInfo;
      if (propertyInfo.GetGetMethod()!=null) {
        var d = DelegateHelper.CreateGetMemberDelegate<TType, TProperty>(PropertyInfo.Name);
        if (d!=null)
          getter = o => d((TType) o);
      }
      if (propertyInfo.GetSetMethod()!=null) {
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
  }
}