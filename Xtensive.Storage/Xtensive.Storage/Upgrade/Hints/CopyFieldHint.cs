// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Kryuchkov
// Created:    2009.05.29

using System;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;

namespace Xtensive.Storage.Upgrade.Hints
{
  /// <summary>
  /// Copy field hint.
  /// </summary>
  [Serializable]
  public class CopyFieldHint : UpgradeHint
  {
    private const string ToStringFormat = "Copy field: {0}.{1} -> {2}.{3}";

    /// <summary>
    /// Gets the source type.
    /// </summary>
    public string SourceType { get; private set; }
    /// <summary>
    /// Gets the source field.
    /// </summary>
    public string SourceField { get; private set; }
    /// <summary>
    /// Gets the destination type.
    /// </summary>
    public Type DestinationType { get; private set; }
    /// <summary>
    /// Gets the destination field.
    /// </summary>
    public string DestinationField { get; private set; }

    public override string ToString()
    {
      return string.Format(ToStringFormat,
        SourceType, SourceField, DestinationType.GetFullName(), DestinationField);
    }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="sourceType">Value for <see cref="SourceType"/>.</param>
    /// <param name="sourceField">Value for <see cref="SourceField"/>.</param>
    /// <param name="destinationType">Value for <see cref="DestinationType"/>.</param>
    /// <param name="destinationField">Value for <see cref="DestinationField"/>.</param>
    public CopyFieldHint(string sourceType, string sourceField, Type destinationType, string destinationField)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(sourceType, "sourceType");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(sourceField, "sourceField");
      ArgumentValidator.EnsureArgumentNotNull(destinationType, "destinationType");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(destinationField, "destinationField");
      SourceType = sourceType;
      SourceField = sourceField;
      DestinationType = destinationType;
      DestinationField = destinationField;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="sourceType">Value for <see cref="SourceType"/>.</param>
    /// <param name="sourceField">Value for <see cref="SourceField"/>.</param>
    /// <param name="destinationType">Value for <see cref="DestinationType"/>.</param>
    public CopyFieldHint(string sourceType, string sourceField, Type destinationType)
      : this(sourceType, sourceField, destinationType, sourceField)
    {
    }
  }
}