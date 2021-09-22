// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.01

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xtensive.Core;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Comparison;
using Xtensive.Modelling.Comparison.Hints;

namespace Xtensive.Orm.Upgrade
{
  /// <summary>
  /// The result of schema comparison.
  /// </summary>
  public sealed class SchemaComparisonResult
  {
    /// <summary>
    /// Gets the comparison status.
    /// </summary>
    public SchemaComparisonStatus SchemaComparisonStatus { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether there are unsafe actions.
    /// </summary>
    public bool HasUnsafeActions { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether there are column type changes.
    /// </summary>
    public bool HasColumnTypeChanges { get; private set; }

    /// <summary>
    /// Indicates whether storage schema is compatible with domain model.
    /// </summary>
    public bool? IsCompatibleInLegacyMode { get; private set; }

    /// <summary>
    /// Gets the list of unsafe actions.
    /// </summary>
    public IReadOnlyList<NodeAction> UnsafeActions { get; private set;}

    #region Additional information

    /// <summary>
    /// Gets the upgrade hints.
    /// </summary>
    public HintSet Hints { get; private set; }

    /// <summary>
    /// Gets the schema difference.
    /// </summary>
    public Difference Difference { get; private set; }

    /// <summary>
    /// Gets all upgrade actions.
    /// </summary>
    public ActionSequence UpgradeActions { get; private set; }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(Strings.SchemaComparisonResultFormat,
        SchemaComparisonStatus,
        HasUnsafeActions.ToString().ToLower(),
        HasColumnTypeChanges.ToString().ToLower(),
        IsCompatibleInLegacyMode.HasValue ? IsCompatibleInLegacyMode.Value.ToString() : Strings.Unknown,
        UnsafeActions.Any() ? UnsafeActions.ToDelimitedString("\r\n").Indent(2) : string.Empty,
        Hints!=null ? Hints.ToString().Indent(2) : string.Empty,
        Difference!=null ? Difference.ToString().Indent(2) : string.Empty);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="schemaComparisonStatus">The comparison status.</param>
    /// <param name="hasColumnTypeChanges">Indicates whether there are column type changes.</param>
    /// <param name="isCompatibleInLegacyMode">Indicates whether schemes are compatible in legacy mode.</param>
    /// <param name="hints">The upgrade hints.</param>
    /// <param name="difference">The difference.</param>
    /// <param name="upgradeActions">The upgrade actions.</param>
    /// <param name="unsafeActions">The unsafe (breaking) actions.</param>
    public SchemaComparisonResult(
      SchemaComparisonStatus schemaComparisonStatus, 
      bool hasColumnTypeChanges, 
      bool? isCompatibleInLegacyMode, 
      HintSet hints, 
      Difference difference, 
      ActionSequence upgradeActions, 
      IList<NodeAction> unsafeActions)
    {
      SchemaComparisonStatus = schemaComparisonStatus;
      IsCompatibleInLegacyMode = isCompatibleInLegacyMode;
      HasColumnTypeChanges = hasColumnTypeChanges;
      Hints = hints;
      Difference = difference;
      UpgradeActions = upgradeActions;
      UnsafeActions = unsafeActions!=null 
        ? new ReadOnlyCollection<NodeAction>(unsafeActions) 
        : Array.Empty<NodeAction>();
      HasUnsafeActions = UnsafeActions.Any();
    }
  }
}