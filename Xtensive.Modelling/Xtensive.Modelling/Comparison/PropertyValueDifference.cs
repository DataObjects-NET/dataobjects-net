// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.28

using System;
using System.Diagnostics;
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
    /// <summary>
    /// Gets or sets the name of the changed property.
    /// </summary>
    public string PropertyName { get; private set; }

    /// <inheritdoc/>
    public override void Build(ActionSequence sequence)
    {
      var node = ((NodeDifference) Parent).Target;
      using (var scope = sequence.LogAction()) {
        var pca = new PropertyChangeAction() {Path = node.Path};
        pca.Properties.Add(PropertyName, Target);
        scope.Action = pca;
        scope.Commit();
      }
    }

    /// <inheritdoc/>
    protected override string ParametersToString()
    {
      return string.Empty;
    }


    // Constructors

    /// <inheritdoc/>
    public PropertyValueDifference(string propertyName, object source, object target)
      : base(source, target)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(propertyName, "propertyName");
      PropertyName = propertyName;
    }
  }
}