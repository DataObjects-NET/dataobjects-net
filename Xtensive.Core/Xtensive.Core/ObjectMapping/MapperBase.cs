// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.09

using System;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Linq;
using Xtensive.Core.ObjectMapping.Model;
using Xtensive.Core.Resources;

namespace Xtensive.Core.ObjectMapping
{
  /// <summary>
  /// A base class for O2O-mapper implementations.
  /// </summary>
  public abstract class MapperBase : IMappingBuilder
  {
    private GraphTransformer transformer;
    private GraphComparer comparer;

    /// <summary>
    /// Gets the mapping description.
    /// </summary>
    public MappingDescription MappingDescription { get; private set; }

    internal ModelBuilder ModelBuilder { get; private set; }

    /// <inheritdoc/>
    public IMappingBuilderAdapter<TSource, TTarget> MapType<TSource, TTarget, TKey>(
      Func<TSource, TKey> sourceKeyExtractor, Expression<Func<TTarget, TKey>> targetKeyExtractor)
    {
      var compiledTargetKeyExtractor = targetKeyExtractor.CachingCompile();
      PropertyInfo targetProperty;
      var isPropertyExtracted = MappingHelper.TryExtractProperty(targetKeyExtractor, "targetKeyExtractor",
        out targetProperty);
      ModelBuilder.Register(typeof (TSource), source => sourceKeyExtractor.Invoke((TSource) source),
        typeof (TTarget), target => compiledTargetKeyExtractor.Invoke((TTarget) target));
      if (isPropertyExtracted)
        ModelBuilder.Register(null, source => sourceKeyExtractor.Invoke((TSource) source), targetProperty);
      return new MapperAdapter<TSource, TTarget>(this);
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
    /// <returns>The set of operations describing found changes.</returns>
    public IOperationSet Compare(object originalTarget, object modifiedTarget)
    {
      if (MappingDescription == null)
        throw new InvalidOperationException(Strings.ExMappingConfigurationHasNotBeenCompleted);
      InitializeComparison(originalTarget, modifiedTarget);
      comparer.Compare(originalTarget, modifiedTarget);
      return GetComparisonResult(originalTarget, modifiedTarget);
    }

    /// <summary>
    /// Called when a difference in object graphs has been found.
    /// </summary>
    /// <param name="descriptor">The descriptor of operation.</param>
    protected abstract void OnObjectModified(OperationInfo descriptor);

    /// <summary>
    /// Initializes a comparison process.
    /// </summary>
    /// <param name="originalTarget">The original object graph.</param>
    /// <param name="modifiedTarget">The modified object graph.</param>
    protected abstract void InitializeComparison(object originalTarget, object modifiedTarget);

    /// <summary>
    /// Gets a set of operations that may be used to apply found modifications
    /// to source objects.
    /// </summary>
    /// <param name="originalTarget">The original target.</param>
    /// <param name="modifiedTarget">The modified target.</param>
    /// <returns></returns>
    protected abstract IOperationSet GetComparisonResult(object originalTarget, object modifiedTarget);

    internal void Complete()
    {
      MappingDescription = ModelBuilder.Build();
      ModelBuilder = null;
      transformer = new GraphTransformer(MappingDescription);
      comparer = new GraphComparer(MappingDescription, OnObjectModified, new DefaultExistanceInfoProvider());
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected MapperBase()
    {
      ModelBuilder = new ModelBuilder();
    }
  }
}