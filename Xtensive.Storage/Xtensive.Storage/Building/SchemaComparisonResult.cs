// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.01

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Comparison;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Storage.Resources;
using Xtensive.Core.Helpers;

namespace Xtensive.Storage.Building
{
  /// <summary>
  /// The result of schema comparison.
  /// </summary>
  [Serializable]
  public class SchemaComparisonResult
  {
    /// <summary>
    /// Gets the comparison status.
    /// </summary>
    public SchemaComparisonStatus Status { get; private set; }

    /// <summary>
    /// Gets upgrade hints.
    /// </summary>
    public HintSet Hints { get; private set; }

    /// <summary>
    /// Gets the schema difference.
    /// </summary>
    public Difference Difference { get; private set; }

    /// <summary>
    /// Gets upgrade actions.
    /// </summary>
    public ActionSequence UpgradeActions { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether extracted column types are different with target column types.
    /// </summary>
    public bool HasTypeChanges { get; private set; }

    /// <summary>
    /// Gets a value indicating whether possible to upgrade data types safely.
    /// </summary>
    public bool CanUpgradeTypesSafely { get; private set; }

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(Strings.SchemaComparisonResultFormat,
        Status
          + (HasTypeChanges
            ? (CanUpgradeTypesSafely
              ? ", " + Strings.CanUpgradeTypeSafely
              : ", " + Strings.CantUpgradeTypeSafely)
            : string.Empty),
        Hints!=null ? Hints.ToString().Indent(2) : string.Empty,
        Difference!=null ? Difference.ToString().Indent(2) : string.Empty,
        UpgradeActions!=null ? UpgradeActions.ToString().Indent(2) : string.Empty);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="status">The comparison status.</param>
    /// <param name="hints">The upgrade hints.</param>
    /// <param name="difference">The difference.</param>
    /// <param name="upgradeActions">The upgrade actions.</param>
    /// <param name="hasTypeChanges">if set to <see langword="true"/> extracted column type are 
    /// different with target column types.</param>
    /// <param name="canUpgradeTypesSafely">if set to <see langword="true"/> all types changes are safely.</param>
    public SchemaComparisonResult(SchemaComparisonStatus status, HintSet hints, 
      Difference difference, ActionSequence upgradeActions, bool hasTypeChanges, bool canUpgradeTypesSafely)
    {
      Difference = difference;
      UpgradeActions = upgradeActions;
      CanUpgradeTypesSafely = canUpgradeTypesSafely;
      HasTypeChanges = hasTypeChanges;
      Status = status;
      Hints = hints;
    }
  }
}