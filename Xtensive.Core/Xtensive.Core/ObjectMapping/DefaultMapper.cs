// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.10

using System.Collections.Generic;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.ObjectMapping.Model;

namespace Xtensive.Core.ObjectMapping
{
  /// <summary>
  /// Default concrete heir of <see cref="MapperBase{TComparisonResult}"/>.
  /// </summary>
  public class DefaultMapper : MapperBase<GraphComparisonResult>
  {
    private DefaultOperationSet operationSet;

    /// <inheritdoc/>
    protected override void OnObjectModified(OperationInfo descriptor)
    {
      operationSet.Add(descriptor);
    }

    /// <inheritdoc/>
    protected override void InitializeComparison(object originalTarget, object modifiedTarget)
    {
      operationSet = new DefaultOperationSet();
    }

    /// <inheritdoc/>
    protected override GraphComparisonResult GetComparisonResult(Dictionary<object, object> originalObjects,
      Dictionary<object, object> modifiedObjects)
    {
      operationSet.Lock();
      return new GraphComparisonResult(operationSet, null);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="mappingDescription">The mapping description.</param>
    public DefaultMapper(MappingDescription mappingDescription)
      : base(mappingDescription)
    {}

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="mappingDescription">The mapping description.</param>
    /// <param name="settings">The mapper settings.</param>
    public DefaultMapper(MappingDescription mappingDescription, MapperSettings settings)
      : base(mappingDescription, settings)
    {}
  }
}