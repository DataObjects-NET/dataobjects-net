// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.28

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Modelling.Actions;

namespace Xtensive.Modelling.Comparison
{
  /// <summary>
  /// Simple value comparison result.
  /// </summary>
  [Serializable]
  public class PropertyValueDifference : Difference
  {
    /// <inheritdoc/>
    public override void Build(IList<NodeAction> sequence)
    {
      var targetNode = ((NodeDifference) Parent).Target;
      var pca = new PropertyChangeAction() {Path = targetNode.Path};
      pca.Properties.Add(PropertyName, PathNodeReference.Get(Target));
      sequence.Add(pca);
    }

    /// <inheritdoc/>
    protected override string ParametersToString()
    {
      return string.Empty;
    }


    // Constructors

    /// <inheritdoc/>
    public PropertyValueDifference(string propertyName, object source, object target)
      : base(propertyName, source, target)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(propertyName, "propertyName");
    }
  }
}