// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.11

using System;
using System.Collections.Generic;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Helpers;
using Xtensive.Resources;

namespace Xtensive.ObjectMapping.Model
{
  /// <summary>
  /// Description of a mapping.
  /// </summary>
  [Serializable]
  public class MappingDescription : LockableBase
  {
    private readonly Dictionary<Type, TargetTypeDescription> targetTypes =
      new Dictionary<Type, TargetTypeDescription>();
    private readonly Dictionary<Type, SourceTypeDescription> sourceTypes =
      new Dictionary<Type, SourceTypeDescription>();

    /// <summary>
    /// Gets the target types.
    /// </summary>
    public virtual IEnumerable<TargetTypeDescription> TargetTypes { get { return targetTypes.Values; } }

    /// <summary>
    /// Gets the source types.
    /// </summary>
    public virtual IEnumerable<SourceTypeDescription> SourceTypes { get { return sourceTypes.Values; } }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      if (IsLocked)
        return;
      foreach (var type in sourceTypes)
        type.Value.Lock(true);
      foreach (var type in targetTypes)
        type.Value.Lock(true);
      base.Lock(recursive);
    }

    /// <summary>
    /// Gets the target type corresponding to <paramref name="sourceType"/>.
    /// </summary>
    /// <param name="sourceType">The source type.</param>
    /// <returns>The description of the target type.</returns>
    public virtual TargetTypeDescription GetMappedTargetType(Type sourceType)
    {
      return GetSourceType(sourceType).TargetType;
    }

    /// <summary>
    /// Gets the source type corresponding to <paramref name="targetType"/>.
    /// </summary>
    /// <param name="targetType">The source type.</param>
    /// <returns>The description of the source type.</returns>
    public virtual SourceTypeDescription GetMappedSourceType(Type targetType)
    {
      return GetTargetType(targetType).SourceType;
    }

    /// <summary>
    /// Gets the description of the target type.
    /// </summary>
    /// <param name="targetType">The target system type.</param>
    /// <returns>The description of the target type.</returns>
    public virtual TargetTypeDescription GetTargetType(Type targetType)
    {
      ArgumentValidator.EnsureArgumentNotNull(targetType, "targetType");
      TargetTypeDescription result;
      if (!targetTypes.TryGetValue(targetType, out result))
        ThrowTypeHasNotBeenRegistered(targetType);
      return result;
    }

    /// <summary>
    /// Gets the description of the source type.
    /// </summary>
    /// <param name="sourceType">The source system type.</param>
    /// <returns>The description of the source type.</returns>
    public virtual SourceTypeDescription GetSourceType(Type sourceType)
    {
      ArgumentValidator.EnsureArgumentNotNull(sourceType, "sourceType");
      SourceTypeDescription result;
      if (!sourceTypes.TryGetValue(sourceType, out result))
        ThrowTypeHasNotBeenRegistered(sourceType);
      return result;
    }

    /// <summary>
    /// Extracts a key from an object of a target type.
    /// </summary>
    /// <param name="target">The object of the target type.</param>
    /// <returns>The extracted key.</returns>
    public virtual object ExtractTargetKey(object target)
    {
      ArgumentValidator.EnsureArgumentNotNull(target, "target");
      var result = GetTargetType(target.GetType()).KeyExtractor.Invoke(target);
      ArgumentValidator.EnsureArgumentNotNull(result, "result");
      return result;
    }

    /// <summary>
    /// Extracts a key from an object of a source type.
    /// </summary>
    /// <param name="source">The object of the source type.</param>
    /// <returns>The extracted key.</returns>
    public virtual object ExtractSourceKey(object source)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      var result = GetSourceType(source.GetType()).KeyExtractor.Invoke(source);
      ArgumentValidator.EnsureArgumentNotNull(result, "result");
      return result;
    }

    #region Private / internal methods

    internal void Register(Type source, Func<object, object> sourceKeyExtractor, Type target,
      Func<object, object> targetKeyExtractor, Func<object, object[]> instanceGenerator)
    {
      this.EnsureNotLocked();
      EnsureTypesCanBeRegistered(source, target);
      var sourceDesc = new SourceTypeDescription(source, sourceKeyExtractor);
      var targetDesc = new TargetTypeDescription(target, targetKeyExtractor, instanceGenerator);
      BindTypes(sourceDesc, targetDesc);
    }

    internal void RegisterProperty(PropertyInfo source, Func<object, object> converter, PropertyInfo target)
    {
      this.EnsureNotLocked();
      var targetType = targetTypes[target.ReflectedType];
      var propertyDesc = new TargetPropertyDescription(target, targetType) {
        Converter = (sourceObj, sourceProperty) => converter.Invoke(sourceObj)
      };
      targetType.AddProperty(propertyDesc);
    }

    internal void RegisterStructure(Type source, Type target)
    {
      this.EnsureNotLocked();
      EnsureTypesCanBeRegistered(source, target);
      var sourceDesc = new SourceTypeDescription(source);
      var targetDesc = new TargetTypeDescription(target);
      BindTypes(sourceDesc, targetDesc);
    }

    internal void RegisterInherited(PropertyInfo source,
      Func<object, SourcePropertyDescription, object> converter, PropertyInfo target)
    {
      this.EnsureNotLocked();
      var targetType = targetTypes[target.ReflectedType];
      var propertyDesc = new TargetPropertyDescription(target, targetType) { Converter = converter };
      targetType.AddProperty(propertyDesc);
    }

    internal void RegisterDefaultPrimitiveTypes()
    {
      this.EnsureNotLocked();
      foreach (var type in MappingHelper.PrimitiveTypes) {
        sourceTypes.Add(type, new SourceTypeDescription(type));
        targetTypes.Add(type, new TargetTypeDescription(type));
      }
    }

    internal void RegisterEnumTypes(Dictionary<Type, TargetTypeDescription> enumTypes)
    {
      this.EnsureNotLocked();
      foreach (var pair in enumTypes) {
        var sourceType = new SourceTypeDescription(pair.Key);
        var targetType = pair.Value;
        sourceType.TargetType = targetType;
        targetType.SourceType = sourceType;
        sourceTypes.Add(pair.Key, sourceType);
        targetTypes.Add(pair.Key, targetType);
      }
    }

    internal void IgnoreProperty(PropertyInfo propertyInfo)
    {
      this.EnsureNotLocked();
      var targetType = targetTypes[propertyInfo.ReflectedType];
      PropertyDescription propertyDescription;
      if (!targetType.Properties.TryGetValue(propertyInfo, out propertyDescription)) {
        propertyDescription = new TargetPropertyDescription(propertyInfo, targetType);
        targetType.AddProperty((TargetPropertyDescription) propertyDescription);
      }
      ((TargetPropertyDescription) propertyDescription).IsIgnored = true;
    }

    internal void TrackChanges(PropertyInfo propertyInfo, bool isEnabled)
    {
      this.EnsureNotLocked();
      var targetType = targetTypes[propertyInfo.ReflectedType];
      PropertyDescription propertyDescription;
      if (!targetType.Properties.TryGetValue(propertyInfo, out propertyDescription)) {
        propertyDescription = new TargetPropertyDescription(propertyInfo, targetType);
        targetType.AddProperty((TargetPropertyDescription) propertyDescription);
      }
      ((TargetPropertyDescription) propertyDescription).IsChangeTrackingDisabled = !isEnabled;
    }

    internal bool TryGetTargetType(Type type, out TargetTypeDescription result)
    {
      return targetTypes.TryGetValue(type, out result);
    }

    internal virtual bool TryGetSourceType(Type type, out SourceTypeDescription result)
    {
      return sourceTypes.TryGetValue(type, out result);
    }

    internal void AddTargetType(TargetTypeDescription targetDesc)
    {
      targetTypes.Add(targetDesc.SystemType, targetDesc);
    }

    internal void AddSourceType(SourceTypeDescription sourceDesc)
    {
      sourceTypes.Add(sourceDesc.SystemType, sourceDesc);
    }
    
    internal void EnsureTargetTypeIsRegistered(Type targetType)
    {
      if (!targetTypes.ContainsKey(targetType))
        ThrowTypeHasNotBeenRegistered(targetType);
    }

    internal void EnsureSourceTypeIsRegistered(Type sourceType)
    {
      if (!sourceTypes.ContainsKey(sourceType))
        ThrowTypeHasNotBeenRegistered(sourceType);
    }

    internal static void ThrowTypeHasNotBeenRegistered(Type type)
    {
      throw new InvalidOperationException(String.Format(Strings.ExTypeXHasNotBeenRegistered,
        type.FullName));
    }

    private void EnsureTypesCanBeRegistered(Type source, Type target)
    {
      EnsureBothOfTypesAreNotObject(source, target);
      EnsureTypesAreNotRegistered(source, target);
    }

    private void EnsureTypesAreNotRegistered(Type source, Type target)
    {
      if (sourceTypes.ContainsKey(source))
        throw new InvalidOperationException(String.Format(Strings.ExTypeXHasAlreadyBeenRegistered,
          target.FullName));
      if (targetTypes.ContainsKey(target))
        throw new InvalidOperationException(String.Format(Strings.ExTypeXHasAlreadyBeenRegistered,
          target.FullName));
    }

    private void BindTypes(SourceTypeDescription sourceDesc, TargetTypeDescription targetDesc)
    {
      sourceDesc.TargetType = targetDesc;
      targetDesc.SourceType = sourceDesc;
      AddSourceType(sourceDesc);
      AddTargetType(targetDesc);
    }

    private static void EnsureBothOfTypesAreNotObject(Type source, Type target)
    {
      if (source==typeof (object) || target==typeof (object))
        throw new InvalidOperationException(
          String.Format(Strings.ExTypeXCanNotBeTransformed, typeof (object)));
    }

    #endregion


    // Constructors

    internal MappingDescription()
    {}
  }
}