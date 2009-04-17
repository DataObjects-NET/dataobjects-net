// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.23

using System;
using System.Linq;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Modelling.Attributes;
using Xtensive.Modelling.Tests.IndexingModel.Resources;

namespace Xtensive.Modelling.Tests.IndexingModel
{
  /// <summary>
  /// Foreign key.
  /// </summary>
  [Serializable]
  public sealed class ForeignKeyInfo: NodeBase<TableInfo>
  {
    private ReferentialAction onUpdateAction;
    private ReferentialAction onRemoveAction;
    private PrimaryIndexInfo primaryKey;
    private IndexInfo foreignKey;


    /// <summary>
    /// Gets or sets the foreign index.
    /// </summary>
    [Property(Priority = -1100)]
    public PrimaryIndexInfo PrimaryKey {
      get { return primaryKey; }
      set {
        EnsureIsEditable();
        using (var scope = LogPropertyChange("PrimaryKey", value)) {
          primaryKey = value;
          scope.Commit();
        }
      }
    }

    /// <summary>
    /// Gets or sets the referencing index.
    /// </summary>
    [Property(Priority = -1000)]
    public IndexInfo ForeignKey {
      get { return foreignKey; }
      set {
        EnsureIsEditable();
        using (var scope = LogPropertyChange("ForeignKey", value)) {
          foreignKey = value;
          scope.Commit();
        }
      }
    }

    /// <summary>
    /// Gets or sets the "on remove" action.
    /// </summary>
    [Property(Priority = -110)]
    public ReferentialAction OnRemoveAction {
      get { return onRemoveAction; }
      set {
        EnsureIsEditable();
        using (var scope = LogPropertyChange("OnUpdateAction", value)) {
          onRemoveAction = value;
          scope.Commit();
        }
      }
    }

    /// <summary>
    /// Gets or sets the "on update" action.
    /// </summary>
    [Property(Priority = -100)]
    public ReferentialAction OnUpdateAction {
      get { return onUpdateAction; }
      set {
        EnsureIsEditable();
        using (var scope = LogPropertyChange("OnUpdateAction", value)) {
          onUpdateAction = value;
          scope.Commit();
        }
      }
    }

    /// <inheritdoc/>
    /// <exception cref="ValidationException">Validations errors.</exception>
    protected override void ValidateState()
    {
      using (var ea = new ExceptionAggregator()) {
        ea.Execute(base.ValidateState);

        if (PrimaryKey==null)
          ea.Execute(() => {
            throw new ValidationException(Strings.ExUndefinedPrimaryKey, Path);
          });
        if (ForeignKey==null)
          ea.Execute(() => {
            throw new ValidationException(Strings.ExUndefinedForeignKey, Path);
          });

        if (PrimaryKey==null || ForeignKey==null)
          return;
        var primaryKeyColumns = PrimaryKey.KeyColumns.Select(
          columnRef => new {columnRef.Index, columnRef.Direction, ColumnType = columnRef.Value.Type});
        var referencedKeyColumns = ForeignKey.KeyColumns.Select(
          columnRef => new {columnRef.Index, columnRef.Direction, ColumnType = columnRef.Value.Type});
        if (primaryKeyColumns.Except(referencedKeyColumns)
          .Union(referencedKeyColumns.Except(primaryKeyColumns)).Count() > 0)
          ea.Execute(() => {
            throw new ValidationException(
              Strings.ExInvalidForeignKeyStructure, Path);
          });
      }
    }

    /// <inheritdoc/>
    protected override Nesting CreateNesting()
    {
      return new Nesting<ForeignKeyInfo, TableInfo, ForeignKeyCollection>(this, "ForeignKeys");
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