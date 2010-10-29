// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.10

using System.Collections.Generic;
using Xtensive.Internals.DocTemplates;
using Xtensive.ObjectMapping.Model;

namespace Xtensive.ObjectMapping
{
  /// <summary>
  /// Default concrete heir of <see cref="MapperBase{TComparisonResult}"/>.
  /// </summary>
  public class DefaultMapper : MapperBase<GraphComparisonResult>
  {
    private DefaultOperationLog operationLog;

    /// <inheritdoc/>
    protected override void OnObjectModified(Operation descriptor)
    {
      operationLog.Add(descriptor);
    }

    /// <inheritdoc/>
    protected override void InitializeComparison(object originalTarget, object modifiedTarget)
    {
      operationLog = new DefaultOperationLog();
    }

    /// <inheritdoc/>
    protected override GraphComparisonResult GetComparisonResult(Dictionary<object, object> originalObjects,
      Dictionary<object, object> modifiedObjects)
    {
      operationLog.Lock();
      return new GraphComparisonResult(operationLog, null);
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