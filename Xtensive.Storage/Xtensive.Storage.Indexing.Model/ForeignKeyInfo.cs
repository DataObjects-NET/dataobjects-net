// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.23

using System;
using System.Linq;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Modelling;
using Xtensive.Modelling.Attributes;

namespace Xtensive.Storage.Indexing.Model
{
  /// <summary>
  /// Foreign key.
  /// </summary>
  [Serializable]
  public class ForeignKeyInfo: NodeBase<TableInfo>
  {
    private ReferentialAction onUpdateAction;
    private ReferentialAction onRemoveAction;
    private PrimaryIndexInfo referencedIndex;
    private IndexInfo referencingIndex;


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
        using (var scope = LogPropertyChange("OnUpdateAction", value)) {
          onUpdateAction = value;
          scope.Commit();
        }
      }
    }

    /// <summary>
    /// Gets or sets the on remove action.
    /// </summary>
    /// <value>The on remove action.</value>
    [Property]
    public ReferentialAction OnRemoveAction
    {
      get { return onRemoveAction; }
      set
      {
        EnsureIsEditable();
        using (var scope = LogPropertyChange("OnUpdateAction", value)) {
          onRemoveAction = value;
          scope.Commit();
        }
      }
    }

    /// <summary>
    /// Gets or sets the foreign index.
    /// </summary>
    [Property]
    public PrimaryIndexInfo ReferencedIndex
    {
      get { return referencedIndex; }
      set
      {
        EnsureIsEditable();
        using (var scope = LogPropertyChange("ReferencedIndex", value)) {
          referencedIndex = value;
          scope.Commit();
        }
      }
    }

    /// <summary>
    /// Gets or sets the referencing index.
    /// </summary>
    [Property]
    public IndexInfo ReferencingIndex
    {
      get { return referencingIndex; }
      set
      {
        EnsureIsEditable();
        using (var scope = LogPropertyChange("ReferencingIndex", value))
        {
          referencingIndex = value;
          scope.Commit();
        }
      }
    }

    /// <inheritdoc/>
    protected override Nesting CreateNesting()
    {
      return new Nesting<ForeignKeyInfo, TableInfo, ForeignKeyCollection>(this, "ForeignKeys");
    }

    /// <inheritdoc/>
    /// <exception cref="IntegrityException">Validations errors.</exception>
    protected override void ValidateState()
    {
      using (var ea = new ExceptionAggregator()) {
        ea.Execute(base.ValidateState);

        if (ReferencedIndex==null)
          ea.Execute(() => { throw new IntegrityException(Resources.Strings.ExReferencedIndexNotDefined, Path); });
        if (ReferencingIndex==null)
          ea.Execute(() => { throw new IntegrityException(Resources.Strings.ExReferencingIndexIsNotDefined, Path); });

        if (ReferencedIndex==null || ReferencingIndex==null)
          return;
        var primaryKeyColumns = ReferencedIndex.KeyColumns.Select(
          columnRef => new {columnRef.Index, columnRef.Direction, columnRef.Value.ColumnType});
        var referencedKeyColumns = ReferencingIndex.KeyColumns.Select(
          columnRef => new {columnRef.Index, columnRef.Direction, columnRef.Value.ColumnType});
        if (primaryKeyColumns.Except(referencedKeyColumns)
          .Union(referencedKeyColumns.Except(primaryKeyColumns)).Count() > 0)
          ea.Execute(() => {
            throw new IntegrityException(
              Resources.Strings.ExReferencingIndexColumnsDoesNotMatchReferencedIndexKeyColumns, Path);
          });
      }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="parent">The parent table.</param>
    /// <param name="name">The name of foreign key.</param>
    /// <inheritdoc/>
    public ForeignKeyInfo(TableInfo parent, string name)
      : base(parent, name)
    {
    }
  }
}