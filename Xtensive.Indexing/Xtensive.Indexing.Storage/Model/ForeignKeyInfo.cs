// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.23

using System;
using System.Diagnostics;
using System.Linq;
using Xtensive.Indexing.Storage.Exceptions;
using Xtensive.Modelling;
using Xtensive.Modelling.Attributes;
using System.Collections.Generic;

namespace Xtensive.Indexing.Storage.Model
{
  /// <summary>
  /// Describes a single foreign key.
  /// </summary>
  [Serializable]
  public class ForeignKeyInfo: NodeBase<PrimaryIndexInfo>
  {
    private ReferentialAction onUpdateAction;
    private ReferentialAction onRemoveAction;
    private PrimaryIndexInfo referencedIndex;

    /// <summary>
    /// Gets or sets the on update action.
    /// </summary>
    [Property]
    public ReferentialAction OnUpdateAction
    {
      get { return onUpdateAction; }
      set
      {
        EnsureIsEditable();
        onUpdateAction = value;
      }
    }

    /// <summary>
    /// Gets or sets the on remove action.
    /// </summary>
    /// <value>The on remove action.</value>
    [Property]
    public ReferentialAction OnRemoveAction
    {
      get
      {
        EnsureIsEditable();
        return onRemoveAction;
      }
      set { onRemoveAction = value; }
    }

    /// <summary>
    /// Gets or sets the index of the referenced.
    /// </summary>
    /// <value>The index of the referenced.</value>
    [Property]
    public PrimaryIndexInfo ReferencedIndex
    {
      get
      {
        EnsureIsEditable();
        return referencedIndex;
      }
      set { referencedIndex = value; }
    }

    /// <summary>
    /// Gets the key columns.
    /// </summary>
    [Property]
    public ColumnInfoRefCollection<ForeignKeyInfo> KeyColumns { get; private set; }

    /// <inheritdoc/>
    protected override void Initialize()
    {
      base.Initialize();
      KeyColumns = new ColumnInfoRefCollection<ForeignKeyInfo>(this, "KeyColumns");
    }

    /// <inheritdoc/>
    protected override Nesting CreateNesting()
    {
      return new Nesting<ForeignKeyInfo, PrimaryIndexInfo, ForeignKeyCollection>(this, "ForeignKeys");
    }

    /// <inheritdoc/>
    protected override void ValidateState()
    {
      base.ValidateState();
      
      if (KeyColumns.Count==0)
        throw new ModelIntegrityException("Empty key columns collection.", Path);

      if (ReferencedIndex==null)
        throw new ModelIntegrityException("Referenced index not defined.", Path);

      var keyColumns = new List<ColumnInfo>(KeyColumns.Select(columnRef => columnRef.Value));
      var referencedColumns = new List<ColumnInfo>(ReferencedIndex.KeyColumns.Select(keyColumn => keyColumn.Value));

      if (keyColumns.Except(referencedColumns).Union(referencedColumns.Except(keyColumns)).Count() > 0) {
        throw new ModelIntegrityException("Foreign key columns definition does not match referenced index key columns.", Path);
      }

      foreach (var column in keyColumns.Except(Parent.Columns)) {
        throw new ModelIntegrityException(
          string.Format("Referenced column '{0}' does not belong to parent index '{1}'.", column.Name, Parent.Name),
          Path);
      }
    }


    //Constructors

    public ForeignKeyInfo(PrimaryIndexInfo parent, string name)
      : base(parent, name)
    {
    }

  }
}