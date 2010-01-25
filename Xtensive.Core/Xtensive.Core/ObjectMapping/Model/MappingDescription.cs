// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.11

using System;
using System.Collections.Generic;
using System.Reflection;
using Xtensive.Core.Collections;
using Xtensive.Core.Helpers;
using Xtensive.Core.Resources;

namespace Xtensive.Core.ObjectMapping.Model
{
  /// <summary>
  /// Description of a mapping.
  /// </summary>
  [Serializable]
  public sealed class MappingDescription : LockableBase
  {
    private readonly Dictionary<Type, TargetTypeDescription> targetTypes =
      new Dictionary<Type, TargetTypeDescription>();
    private readonly Dictionary<Type, SourceTypeDescription> sourceTypes =
      new Dictionary<Type, SourceTypeDescription>();

    /// <summary>
    /// Gets the target types.
    /// </summary>
    public ReadOnlyDictionary<Type, TargetTypeDescription> TargetTypes { get; private set; }

    /// <summary>
    /// Gets the source types.
    /// </summary>
    public ReadOnlyDictionary<Type, SourceTypeDescription> SourceTypes { get; private set; }

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
    public TargetTypeDescription GetMappedTargetType(Type sourceType)
    {
      ArgumentValidator.EnsureArgumentNotNull(sourceType, "sourceType");
      SourceTypeDescription description;
      if (!sourceTypes.TryGetValue(sourceType, out description))
        ThrowTypeHasNotBeenRegistered(sourceType);
      return description.TargetType;
    }

    /// <summary>
    /// Gets the source type corresponding to <paramref name="targetType"/>.
    /// </summary>
    /// <param name="targetType">The source type.</param>
    /// <returns>The description of the source type.</returns>
    public SourceTypeDescription GetMappedSourceType(Type targetType)
    {
      ArgumentValidator.EnsureArgumentNotNull(targetType, "targetType");
      TargetTypeDescription description;
      if (!targetTypes.TryGetValue(targetType, out description))
        ThrowTypeHasNotBeenRegistered(targetType);
      return description.SourceType;
    }

    /// <summary>
    /// Extracts a key from an object of a target type.
    /// </summary>
    /// <param name="target">The object of the target type.</param>
    /// <returns>The extracted key.</returns>
    public object ExtractTargetKey(object target)
    {
      ArgumentValidator.EnsureArgumentNotNull(target, "target");
      var type = target.GetType();
      EnsureTargetTypeIsRegistered(type);
      return targetTypes[type].KeyExtractor.Invoke(target);
    }

    /// <summary>
    /// Extracts a key from an object of a source type.
    /// </summary>
    /// <param name="source">The object of the source type.</param>
    /// <returns>The extracted key.</returns>
    public object ExtractSourceKey(object source)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      var type = source.GetType();
      EnsureSourceTypeIsRegistered(type);
      return sourceTypes[type].KeyExtractor.Invoke(source);
    }

    #region Private / internal methods

    internal void Register(Type source, Func<object, object> sourceKeyExtractor, Type target,
      Func<object, object> targetKeyExtractor)
    {
      this.EnsureNotLocked();
      EnsureTypesAreNotRegistered(source, target);
      var sourceDesc = new SourceTypeDescription(source, sourceKeyExtractor);
      var targetDesc = new TargetTypeDescription(target, targetKeyExtractor);
      BindTypes(source, sourceDesc, target, targetDesc);
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
      EnsureTypesAreNotRegistered(source, target);
      var sourceDesc = new SourceTypeDescription(source);
      var targetDesc = new TargetTypeDescription(target);
      BindTypes(source, sourceDesc, target, targetDesc);
    }

    internal void RegisterInherited(PropertyInfo source, Func<object, SourcePropertyDescription, object> converter,
      PropertyInfo target)
    {
      this.EnsureNotLocked();
      var targetType = targetTypes[target.ReflectedType];
      var propertyDesc = new TargetPropertyDescription(target, targetType) { Converter = converter };
      targetType.AddProperty(propertyDesc);
    }

    internal void RegisterDefaultPrimitiveTypes()
    {
      foreach (var type in MappingHelper.PrimitiveTypes) {
        sourceTypes.Add(type, new SourceTypeDescription(type));
        targetTypes.Add(type, new TargetTypeDescription(type));
      }
    }

    internal void RegisterEnumTypes(Dictionary<Type, TargetTypeDescription> enumTypes)
    {
      foreach (var pair in enumTypes) {
        var sourceType = new SourceTypeDescription(pair.Key);
        var targetType = pair.Value;
        sourceType.TargetType = targetType;
        targetType.SourceType = sourceType;
        sourceTypes.Add(pair.Key, sourceType);
        targetTypes.Add(pair.Key, targetType);
      }
    }

    internal void Ignore(PropertyInfo propertyInfo)
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

    internal void SkipChangeDetection(PropertyInfo propertyInfo)
    {
      this.EnsureNotLocked();
      var targetType = targetTypes[propertyInfo.ReflectedType];
      PropertyDescription propertyDescription;
      if (!targetType.Properties.TryGetValue(propertyInfo, out propertyDescription)) {
        propertyDescription = new TargetPropertyDescription(propertyInfo, targetType);
        targetType.AddProperty((TargetPropertyDescription) propertyDescription);
      }
      ((TargetPropertyDescription) propertyDescription).IsDetectionSkipped = true;
    }

    internal TargetTypeDescription GetTargetTypeDescription(Type targetType)
    {
      TargetTypeDescription result;
      if (!targetTypes.TryGetValue(targetType, out result))
        ThrowTypeHasNotBeenRegistered(targetType);
      return result;
    }

    internal SourceTypeDescription GetSourceTypeDescription(Type sourceType)
    {
      SourceTypeDescription result;
      if (!sourceTypes.TryGetValue(sourceType, out result))
        ThrowTypeHasNotBeenRegistered(sourceType);
      return result;
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

    private static void ThrowTypeHasNotBeenRegistered(Type type)
    {
      throw new InvalidOperationException(String.Format(Strings.ExTypeXHasNotBeenRegistered,
        type.FullName));
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

    private void BindTypes(Type source, SourceTypeDescription sourceDesc, Type target,
      TargetTypeDescription targetDesc)
    {
      sourceDesc.TargetType = targetDesc;
      targetDesc.SourceType = sourceDesc;
      sourceTypes.Add(source, sourceDesc);
      targetTypes.Add(target, targetDesc);
    }

    #endregion


    // Constructor

    internal MappingDescription()
    {
      SourceTypes = new ReadOnlyDictionary<Type, SourceTypeDescription>(sourceTypes, false);
      TargetTypes = new ReadOnlyDictionary<Type, TargetTypeDescription>(targetTypes, false);
    }
  }
}