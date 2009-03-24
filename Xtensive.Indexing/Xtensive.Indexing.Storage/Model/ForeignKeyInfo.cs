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
    /// <summary>
    /// Gets or sets the on update action.
    /// </summary>
    /// <value>The on update action.</value>
    [Property]
    public ReferentialAction OnUpdateAction { get; set; }

    /// <summary>
    /// Gets or sets the on remove action.
    /// </summary>
    /// <value>The on remove action.</value>
    [Property]
    public ReferentialAction OnRemoveAction { get; set; }

    /// <summary>
    /// Gets or sets the index of the referenced.
    /// </summary>
    /// <value>The index of the referenced.</value>
    [Property]
    public PrimaryIndexInfo ReferencedIndex { get; set; }

    /// <summary>
    /// Gets the key columns.
    /// </summary>
    [Property]
    public ColumnInfoRefCollection<ForeignKeyColumnRef> KeyColumns { get; private set; }

    /// <inheritdoc/>
    protected override void Initialize()
    {
      base.Initialize();
      KeyColumns = new ColumnInfoRefCollection<ForeignKeyColumnRef>(this, "KeyColumns");
    }

    /// <inheritdoc/>
    protected override Nesting CreateNesting()
    {
      return new Nesting<ForeignKeyInfo, PrimaryIndexInfo, ForeignKeyCollection>(this, "ForeignKeys");
    }

    protected override void ValidateState()
    {
      base.ValidateState();

      if (KeyColumns.Count == 0)
        throw new ModelIntegrityException("Empty key columns collection.", Path);

      if (ReferencedIndex == null)
        throw new ModelIntegrityException("Referenced index not defined.", Path);

      var keyColumns = new List<ColumnInfo>(KeyColumns.Select(keyColumn => keyColumn.Value));
      var referencedColumns = new List<ColumnInfo>(ReferencedIndex.KeyColumns.Select(keyColumn => keyColumn.Value));

      var xx = keyColumns.Join(referencedColumns, ok => ok, ik => ik, (ok, ik) => new {ok, ik});
    }


    //Constructors

    public ForeignKeyInfo(PrimaryIndexInfo parent, string name)
      : base(parent, name)
    {
    }

  }
}