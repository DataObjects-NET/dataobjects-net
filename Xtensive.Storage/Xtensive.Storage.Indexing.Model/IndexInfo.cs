// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.24

using System;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Modelling.Attributes;

namespace Xtensive.Storage.Indexing.Model
{
  /// <summary>
  /// The base abstract class for all indexes.
  /// </summary>
  [Serializable]
  public abstract class IndexInfo : NodeBase<TableInfo>
  {
    private bool isUnique;

    /// <summary>
    /// Gets a value indicating whether this instance is unique.
    /// </summary>
    /// <exception cref="NotSupportedException">Already initialized.</exception>
    [Property]
    public bool IsUnique
    {
      get { return isUnique; }
      set
      {
        EnsureIsEditable();
        if (IsPrimary && !value)
          throw Exceptions.AlreadyInitialized("IsUnique");
        using (var scope = LogPropertyChange("IsUnique", value))
        {
          isUnique = value;
          scope.Commit();
        }
      }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is primary.
    /// </summary>
    [Property]
    public bool IsPrimary { get; private set; }

    /// <summary>
    /// Gets key columns.
    /// </summary>
    [Property]
    public KeyColumnRefCollection KeyColumns { get; private set; }

    /// <summary>
    /// Gets value columns.
    /// </summary>
    [Property]
    public ValueColumnRefCollection ValueColumns { get; private set; }

    /// <inheritdoc/>
    protected override void Initialize()
    {
      base.Initialize();
      IsPrimary = this is PrimaryIndexInfo;
      if (KeyColumns==null)
        KeyColumns = new KeyColumnRefCollection(this);
      if (ValueColumns==null)
        ValueColumns = new ValueColumnRefCollection(this);
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