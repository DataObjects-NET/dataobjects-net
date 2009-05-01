// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.01

using System;
using System.Linq;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Comparison;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Storage.Indexing.Model;

namespace Xtensive.Storage.Upgrade
{
  
  /// <summary>
  /// Result of model comparison.
  /// </summary>
  [Serializable]
  public class ModelComparisonResult
  {
    /// <summary>
    /// Gets the model difference.
    /// </summary>
    public Difference ModelDifference { get; private set; }

    /// <summary>
    /// Gets upgrade actions.
    /// </summary>
    public ActionSequence UpgradeActions { get; private set; }

    /// <summary>
    /// Gets upgrade hints.
    /// </summary>
    public HintSet UpgradeHints { get; private set; }

    /// <summary>
    /// Gets the storage model conformity.
    /// </summary>
    public SchemaComparisonStatus SchemaComparison { get; private set; }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="modelDifference">The model difference.</param>
    /// <param name="upgradeActions">The upgrade actions.</param>
    /// <param name="upgradeHints">The upgrade hints.</param>
    /// <param name="schemaComparison">The storage model conformity.</param>
    public ModelComparisonResult(Difference modelDifference,
      ActionSequence upgradeActions, HintSet upgradeHints, 
      SchemaComparisonStatus schemaComparison)
    {
      ModelDifference = modelDifference;
      UpgradeActions = upgradeActions;
      SchemaComparison = schemaComparison;
      UpgradeHints = upgradeHints;
    }
  }
}