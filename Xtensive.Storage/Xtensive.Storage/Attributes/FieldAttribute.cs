// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.07.04

using System;
using System.Collections.Generic;
using System.Reflection;
using PostSharp.Aspects;
using PostSharp.Extensibility;
using Xtensive.Core.Aspects;
using Xtensive.Core.Reflection;
using Xtensive.Integrity.Aspects;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage
{
  /// <summary>
  /// Indicates that property is persistent field
  /// and defines its persistence-related properties.
  /// </summary>
  [Serializable]
  [MulticastAttributeUsage(MulticastTargets.Property, AllowMultiple = false, Inheritance = MulticastInheritance.Strict, PersistMetaData = true)]
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
  public sealed class FieldAttribute : Aspect,
    IAspectProvider
  {
    private const string HandlerMethodSuffix = "FieldValue";
    internal int? length;
    internal int? scale;
    internal int? precision;
    internal bool? nullable;
    internal bool? indexed;

    /// <summary>
    /// Gets or sets whether the field should be indexed.
    /// </summary>
    public bool Indexed
    {
      get { return indexed.HasValue ? indexed.Value : false; }
      set { indexed = value; }
    }

    /// <summary>
    /// Gets or sets the length of the field.
    /// </summary>
    /// <remarks>
    /// This property can be specified for <see cref="string"/> or array of <see cref="byte"/> fields.
    /// </remarks>
    public int Length
    {
      get { return length.HasValue ? length.Value : 0; }
      set { length = value; }
    }

    /// <summary>
    /// Gets or sets the scale of the field.
    /// </summary>
    /// <remarks>
    /// This property can be specified for <see cref="decimal"/> type.
    /// </remarks>
    public int Scale
    {
      get { return scale.HasValue ? scale.Value : 0; }
      set { scale = value; }
    }

    /// <summary>
    /// Gets or sets the precision of the field.
    /// </summary>
    /// <remarks>
    /// This property can be specified for <see cref="decimal"/> type.
    /// </remarks>
    public int Precision
    {
      get { return precision.HasValue ? precision.Value : 0; }
      set { precision = value; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this field is nullable.
    /// </summary>
    /// <remarks>
    /// Note that this should be applied to reference fields only. For value-based fields
    /// consider using Nullable&lt;T&gt; approach.
    /// </remarks>
    /// <value>
    /// <see langword="true"/> if field nullable; otherwise, <see langword="false"/>.
    /// </value>
    public bool Nullable
    {
      get { return nullable.HasValue ? nullable.Value : false; }
      set { nullable = value; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether value of this field should be loaded on demand.
    /// </summary>
    /// <remarks>
    /// Usually lazy loading is used for byte-arrays, large string fields or <see cref="Structure">structures</see>.
    /// <see cref="Entity"/> and <see cref="EntitySet{TItem}"/> fields are always loaded on demand.
    /// </remarks>
    public bool LazyLoad { get; set; }
  
    public IEnumerable<AspectInstance> ProvideAspects(object targetElement)
    {
      var result = new List<AspectInstance>();
      var propertyInfo = (PropertyInfo) targetElement;
      var type = propertyInfo.DeclaringType;
      if (!typeof(Persistent).IsAssignableFrom(type))
        return result;

      var keyAttribute = propertyInfo.GetAttribute<KeyAttribute>(AttributeSearchOptions.InheritNone);
      var getter = propertyInfo.GetGetMethod(true);
      var setter = propertyInfo.GetSetMethod(true);
      var replacer = new ReplaceAutoProperty(HandlerMethodSuffix);
      if (getter != null)
        result.Add(new AspectInstance(getter, replacer));
      if (setter != null) {
        if (keyAttribute != null) {
          var errorMessage = string.Format(Strings.ExKeyFieldXInTypeYShouldNotHaveSetAccessor, propertyInfo.Name, type.Name);
          var notSupportedAspect = new NotSupportedAttribute(errorMessage);
          result.Add(new AspectInstance(setter, notSupportedAspect));
        }
        result.Add(new AspectInstance(setter, replacer));

        // If there are constraints, we must "wrap" setter into transaction
        var constraints = propertyInfo.GetAttributes<PropertyConstraintAspect>(AttributeSearchOptions.InheritNone);
        bool hasConstraints = !(constraints == null || constraints.Length == 0);
        if (hasConstraints) {
          var transactionalAspect = new TransactionalAttribute();
          result.Add(new AspectInstance(setter, transactionalAspect));
        }
      }
      return result;
    }
  }
}