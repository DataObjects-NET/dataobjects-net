// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.09

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.ObjectMapping.Model;
using Xtensive.Core.Resources;

namespace Xtensive.Core.ObjectMapping
{
  /// <summary>
  /// A base class for O2O-mapper implementations.
  /// </summary>
  /// <typeparam name="TComparisonResult">The type of graphs comparison result.</typeparam>
  public abstract class MapperBase<TComparisonResult> : IMappingBuilder
    where TComparisonResult : GraphComparisonResult
  {
    private GraphTransformer transformer;
    private GraphComparer comparer;
    private ModelBuilder modelBuilder;
    private ObjectExtractor objectExtractor;

    /// <summary>
    /// Gets the mapping description.
    /// </summary>
    public MappingDescription MappingDescription { get; private set; }

    /// <summary>
    /// Gets or sets the mapper settings.
    /// </summary>
    public MapperSettings Settings { get; private set; }

    internal ModelBuilder ModelBuilder
    {
      get {
        if (modelBuilder==null)
          throw new InvalidOperationException(Strings.ExMappingConfigurationHasBeenAlreadyCompleted);
        return modelBuilder;
      }
      private set { modelBuilder = value; }
    }

    /// <inheritdoc/>
    public IMappingBuilderAdapter<TSource, TTarget> MapType<TSource, TTarget, TKey>(
      Func<TSource, TKey> sourceKeyExtractor, Expression<Func<TTarget, TKey>> targetKeyExtractor)
    {
      ArgumentValidator.EnsureArgumentNotNull(sourceKeyExtractor, "sourceKeyExtractor");
      ArgumentValidator.EnsureArgumentNotNull(targetKeyExtractor, "targetKeyExtractor");
      var compiledTargetKeyExtractor = targetKeyExtractor.Compile();
      PropertyInfo targetProperty;
      var isPropertyExtracted = MappingHelper.TryExtractProperty(targetKeyExtractor, "targetKeyExtractor",
        out targetProperty);
      ModelBuilder.Register(typeof (TSource), source => sourceKeyExtractor.Invoke((TSource) source),
        typeof (TTarget), target => compiledTargetKeyExtractor.Invoke((TTarget) target));
      if (isPropertyExtracted)
        ModelBuilder.RegisterProperty(null, source => sourceKeyExtractor.Invoke((TSource) source), targetProperty);
      return new MapperAdapter<TSource, TTarget, TComparisonResult>(this);
    }

    /// <inheritdoc/>
    public IMappingBuilderAdapter<TSource, TTarget> MapStructure<TSource, TTarget>()
      where TTarget : struct
    {
      ModelBuilder.RegisterStructure(typeof (TSource), typeof (TTarget));
      return new MapperAdapter<TSource, TTarget, TComparisonResult>(this);
    }

    /// <summary>
    /// Transforms an object of the source type to an object of the target type.
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <returns>The transformed object.</returns>
    public object Transform(object source)
    {
      if (MappingDescription == null)
        throw new InvalidOperationException(Strings.ExMappingConfigurationHasNotBeenCompleted);
      return transformer.Transform(source);
    }

    /// <summary>
    /// Compares two object graphs of the target type and generates a set of operations
    /// which may be used to apply found modifications to source objects.
    /// </summary>
    /// <param name="originalTarget">The original object graph.</param>
    /// <param name="modifiedTarget">The modified object graph.</param>
    /// <returns>The <see cref="GraphComparisonResult"/>.</returns>
    public TComparisonResult Compare(object originalTarget, object modifiedTarget)
    {
      if (MappingDescription == null)
        throw new InvalidOperationException(Strings.ExMappingConfigurationHasNotBeenCompleted);
      InitializeComparison(originalTarget, modifiedTarget);
      var modifiedObjects = new Dictionary<object, object>();
      var originalObjects = new Dictionary<object, object>();
      if (originalTarget!=null || modifiedTarget!=null) {
        objectExtractor.Extract(modifiedTarget, modifiedObjects);
        objectExtractor.Extract(originalTarget, originalObjects);
        comparer.Compare(originalObjects, modifiedObjects);
      }
      return GetComparisonResult(originalObjects, modifiedObjects);
    }

    /// <summary>
    /// Called when a difference in object graphs has been found.
    /// </summary>
    /// <param name="descriptor">The descriptor of operation.</param>
    protected abstract void OnObjectModified(OperationInfo descriptor);

    /// <summary>
    /// Initializes comparison process.
    /// </summary>
    /// <param name="originalTarget">The original object graph.</param>
    /// <param name="modifiedTarget">The modified object graph.</param>
    protected abstract void InitializeComparison(object originalTarget, object modifiedTarget);

    /// <summary>
    /// Gets the set of operations describing found changes and the mapping from surrogate
    /// keys to real keys for new objects.
    /// </summary>
    /// <param name="originalObjects">The set of objects from the original graph.</param>
    /// <param name="modifiedObjects">The set of objects from the modified graph.</param>
    /// <returns>The <see cref="GraphComparisonResult"/>.</returns>
    protected abstract TComparisonResult GetComparisonResult(Dictionary<object, object> originalObjects,
      Dictionary<object, object> modifiedObjects);

    internal void Complete()
    {
      MappingDescription = ModelBuilder.Build();
      ModelBuilder = null;
      transformer = new GraphTransformer(MappingDescription, Settings);
      comparer = new GraphComparer(MappingDescription, OnObjectModified, new DefaultExistanceInfoProvider());
      objectExtractor = new ObjectExtractor(MappingDescription);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected MapperBase()
      : this(new MapperSettings())
    {}

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="settings">The mapper settings.</param>
    protected MapperBase(MapperSettings settings)
    {
      ArgumentValidator.EnsureArgumentNotNull(settings, "settings");
      ModelBuilder = new ModelBuilder();
      settings.Lock();
      Settings = settings;
    }
  }
}