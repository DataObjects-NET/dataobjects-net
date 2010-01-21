// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.10

using Xtensive.Core.Internals.DocTemplates;

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
    protected override GraphComparisonResult GetComparisonResult(
      object originalTarget, object modifiedTarget)
    {
      operationSet.Lock();
      return new GraphComparisonResult(operationSet, null);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public DefaultMapper()
    {}

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="settings">The mapper settings.</param>
    public DefaultMapper(MapperSettings settings)
      : base(settings)
    {}
  }
}