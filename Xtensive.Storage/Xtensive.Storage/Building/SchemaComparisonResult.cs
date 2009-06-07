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

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(Strings.SchemaComparisonResultFormat,
        Status,
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
    /// <param name="canPerform">if set to <see langword="true"/> [can perform].</param>
    /// <param name="canPerformSafely">if set to <see langword="true"/> [can perform safely].</param>
    public SchemaComparisonResult(SchemaComparisonStatus status, HintSet hints, 
      Difference difference, ActionSequence upgradeActions)
    {
      Difference = difference;
      UpgradeActions = upgradeActions;
      Status = status;
      Hints = hints;
    }
  }
}