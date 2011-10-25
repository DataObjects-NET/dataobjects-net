// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.24

using System;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Modelling.Attributes;

namespace Xtensive.Storage.Model
{
  /// <summary>
  /// The base abstract class for all indexes.
  /// </summary>
  [Serializable]
  public abstract class IndexInfo : NodeBase<TableInfo>
  {
    private bool isUnique;
    private bool isClustered;

    /// <summary>
    /// Gets a value indicating whether this instance is unique.
    /// </summary>
    /// <exception cref="NotSupportedException">Already initialized.</exception>
    [Property(Priority = -1100)]
    public bool IsUnique
    {
      get { return isUnique; }
      set
      {
        EnsureIsEditable();
        if (IsPrimary && !value)
          throw Exceptions.AlreadyInitialized("IsUnique");
        using (var scope = LogPropertyChange("IsUnique", value)) {
          isUnique = value;
          scope.Commit();
        }
      }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is primary.
    /// </summary>
    [Property(IgnoreInComparison = true)]
    public bool IsPrimary { get; private set; }

    /// <summary>
    /// Gets key columns.
    /// </summary>
    [Property(Priority = -1000)]
    public KeyColumnRefCollection KeyColumns { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this instance is clustered.
    /// </summary>
    [Property(Priority = -900)]
    public bool IsClustered
    {
      get { return isClustered; }
      set
      {
        EnsureIsEditable();
        using (var scope = LogPropertyChange("IsClustered", value)) {
          isClustered = value;
          scope.Commit();
        }
      }
    }

    /// <inheritdoc/>
    protected override void Initialize()
    {
      base.Initialize();
      IsPrimary = this is PrimaryIndexInfo;
      if (IsPrimary)
        isUnique = true;
      if (KeyColumns == null)
        KeyColumns = new KeyColumnRefCollection(this);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="parent">The parent table.</param>
    /// <param name="name">The index.</param>
    /// <inheritdoc/>
    protected IndexInfo(TableInfo parent, string name)
      : base(parent, name)
    {
    }
  }
}