// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.09

using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.ObjectMapping.Model;

namespace Xtensive.ObjectMapping
{
  /// <summary>
  /// A base class for O2O-mapper implementations.
  /// </summary>
  /// <typeparam name="TComparisonResult">The type of graphs comparison result.</typeparam>
  public abstract class MapperBase<TComparisonResult>
    where TComparisonResult : GraphComparisonResult
  {
    private readonly GraphTransformer transformer;
    private readonly GraphComparer comparer;
    private readonly ObjectExtractor objectExtractor;

    /// <summary>
    /// Gets the mapping description.
    /// </summary>
    public MappingDescription MappingDescription { get; private set; }

    /// <summary>
    /// Gets or sets the mapper settings.
    /// </summary>
    public MapperSettings Settings { get; private set; }

    /// <summary>
    /// Transforms an object of the source type to an object of the target type.
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <returns>The transformed object.</returns>
    public object Transform(object source)
    {
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
    protected abstract void OnObjectModified(Operation descriptor);

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


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="mappingDescription">The mapping description.</param>
    protected MapperBase(MappingDescription mappingDescription)
      : this(mappingDescription, new MapperSettings())
    {}

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="mappingDescription">The mapping description.</param>
    /// <param name="settings">The mapper settings.</param>
    protected MapperBase(MappingDescription mappingDescription, MapperSettings settings)
    {
      ArgumentValidator.EnsureArgumentNotNull(mappingDescription, "mappingDescription");
      ArgumentValidator.EnsureArgumentNotNull(settings, "settings");
      MappingDescription = mappingDescription;
      settings.Lock();
      Settings = settings;
      var description = settings.EnableDynamicSourceHierarchies
        ? new DynamicMappingDescription(MappingDescription)
        : MappingDescription;
      transformer = new GraphTransformer(description, Settings);
      comparer = new GraphComparer(description, OnObjectModified, new DefaultExistanceInfoProvider());
      objectExtractor = new ObjectExtractor(description);
    }
  }
}