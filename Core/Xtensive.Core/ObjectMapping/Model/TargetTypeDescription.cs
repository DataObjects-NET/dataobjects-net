// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.11

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Helpers;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.ObjectMapping.Model
{
  /// <summary>
  /// Description of target mapped type.
  /// </summary>
  [Serializable]
  public sealed class TargetTypeDescription : TypeDescription
  {
    private SourceTypeDescription sourceType;
    private HashSet<TargetTypeDescription> directDescendants;
    private TargetTypeDescription ancestor;

    /// <summary>
    /// Gets the provider of arguments for an algorithm of a new source 
    /// object creation. For example, it can provide arguments for a custom
    /// constructor or a key generator.
    /// </summary>
    public readonly Func<object, object[]> GeneratorArgumentsProvider;

    /// <summary>
    /// Gets the corresponding source type.
    /// </summary>
    public SourceTypeDescription SourceType
    {
      get { return sourceType; }
      internal set{
        this.EnsureNotLocked();
        sourceType = value;
      }
    }

    /// <summary>
    /// Gets the collection of primitive properties.
    /// </summary>
    public ReadOnlyDictionary<PropertyInfo, TargetPropertyDescription> PrimitiveProperties { get; private set; }

    /// <summary>
    /// Gets the collection of complex properties.
    /// </summary>
    public ReadOnlyDictionary<PropertyInfo, TargetPropertyDescription> ComplexProperties { get; private set; }

    /// <summary>
    /// Gets mutable properties.
    /// </summary>
    public ReadOnlyCollection<TargetPropertyDescription> MutableProperties { get; private set; }

    /// <summary>
    /// Gets direct descendants.
    /// </summary>
    public ReadOnlyCollection<TargetTypeDescription> DirectDescendants { get; private set; }

    /// <summary>
    /// Gets the ancestor.
    /// </summary>
    public TargetTypeDescription Ancestor {
      get { return ancestor; }
      internal set {
        this.EnsureNotLocked();
        if (ancestor != null)
          throw Exceptions.AlreadyInitialized("Ancestor");
        ancestor = value;
      }
    }

    /// <inheritdoc/>
    public new TargetPropertyDescription GetProperty(PropertyInfo propertyInfo)
    {
      return (TargetPropertyDescription) base.GetProperty(propertyInfo);
    }

    /// <summary>
    /// Adds the property.
    /// </summary>
    /// <param name="property">The property.</param>
    public void AddProperty(TargetPropertyDescription property)
    {
      base.AddProperty(property);
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      var primitiveProperties = new Dictionary<PropertyInfo, TargetPropertyDescription>();
      var properties = Properties.Select(pair => pair.Value).Cast<TargetPropertyDescription>()
        .Where(p => !p.IsIgnored);
      var primitiveFiltered = properties
        .Where(p => p.ValueType.ObjectKind==ObjectKind.Primitive && !p.IsCollection);
      foreach (var property in primitiveFiltered)
        primitiveProperties.Add(property.SystemProperty, property);
      PrimitiveProperties =
        new ReadOnlyDictionary<PropertyInfo, TargetPropertyDescription>(primitiveProperties, false);
      var complexProperties = new Dictionary<PropertyInfo, TargetPropertyDescription>();
      var complexFiltered = properties
        .Where(p => p.ValueType.ObjectKind!=ObjectKind.Primitive || p.IsCollection);
      foreach (var property in complexFiltered)
        complexProperties.Add(property.SystemProperty, property);
      ComplexProperties =
        new ReadOnlyDictionary<PropertyInfo, TargetPropertyDescription>(complexProperties, false);
      var mutableProperties = new List<TargetPropertyDescription>(properties.Where(p => !p.IsChangeTrackingDisabled));
      MutableProperties = new ReadOnlyCollection<TargetPropertyDescription>(mutableProperties, false);
      base.Lock(recursive);
    }

    internal void AddDirectDescendant(TargetTypeDescription descendant)
    {
      this.EnsureNotLocked();
      if (directDescendants == null) {
        directDescendants = new HashSet<TargetTypeDescription>();
        DirectDescendants = new ReadOnlyCollection<TargetTypeDescription>(directDescendants, false);
      }
      descendant.Ancestor = this;
      directDescendants.Add(descendant);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="systemType">The system type.</param>
    /// <param name="keyExtractor">The key extractor.</param>
    /// <param name="generatorArgumentsProvider">The provider of arguments for an algorithm of a new source 
    /// object creation. For example, it can provide arguments for a custom
    /// constructor or a key generator.</param>
    public TargetTypeDescription(Type systemType, Func<object, object> keyExtractor,
      Func<object, object[]> generatorArgumentsProvider)
      : base(systemType, keyExtractor)
    {
      GeneratorArgumentsProvider = generatorArgumentsProvider;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="systemType">The system type.</param>
    public TargetTypeDescription(Type systemType)
      : base(systemType)
    {}
  }
}