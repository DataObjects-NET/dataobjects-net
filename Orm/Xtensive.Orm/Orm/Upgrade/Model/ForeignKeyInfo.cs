// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.23

using System;
using System.Linq;
using Xtensive.Core;
using Xtensive.Helpers;

using Xtensive.Reflection;
using Xtensive.Modelling;
using Xtensive.Modelling.Attributes;
using Xtensive.Collections;

namespace Xtensive.Orm.Upgrade.Model
{
  /// <summary>
  /// Foreign key.
  /// </summary>
  [Serializable]
  [DataDependent]
  public sealed class ForeignKeyInfo : NodeBase<TableInfo>
  {
    private ReferentialAction onUpdateAction;
    private ReferentialAction onRemoveAction;
    private PrimaryIndexInfo primaryKey;

    /// <summary>
    /// Gets or sets the foreign index.
    /// </summary>
    [Property(Priority = -1100)]
    public PrimaryIndexInfo PrimaryKey
    {
      get { return primaryKey; }
      set
      {
        EnsureIsEditable();
        using (var scope = LogPropertyChange("PrimaryKey", value)) {
          primaryKey = value;
          scope.Commit();
        }
      }
    }

    /// <summary>
    /// Gets foreign key columns.
    /// </summary>
    [Property]
    public ForeignKeyColumnCollection ForeignKeyColumns { get; private set; }

    /// <summary>
    /// Gets or sets the "on remove" action.
    /// </summary>
    [Property(Priority = -110)]
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
    /// Gets or sets the "on update" action.
    /// </summary>
    [Property(Priority = -100)]
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

    
    /// <exception cref="ValidationException">Validations errors.</exception>
    protected override void ValidateState()
    {
      using (var ea = new ExceptionAggregator()) {
        ea.Execute(base.ValidateState);

        if (PrimaryKey == null) {
          ea.Execute(() => {
            throw new ValidationException(Strings.ExUndefinedPrimaryKey, Path);
          });
          ea.Complete();
          return;
        }

        var pkColumns = PrimaryKey.KeyColumns;
        var fkColumns = ForeignKeyColumns;

        if (pkColumns.Count()!=pkColumns.Zip(fkColumns).Where(p => CompareKeyColumns(p.First, p.Second)).Count()) {
          ea.Execute(() => {
            throw new ValidationException(
              Strings.ExInvalidForeignKeyStructure, Path);
          });
        }

        // var pkTypes = PrimaryKey.KeyColumns.Select(c => c.Value.Type);
        // var fkTypes = ForeignKeyColumns.Select(c => c.Value.Type);
        // if (pkTypes.Count()!=pkTypes.Zip(fkTypes).Where(p => p.First==p.Second).Count())
        //  ea.Execute(() => {
        //    throw new ValidationException(
        //      Strings.ExInvalidForeignKeyStructure, Path);
        //  });

        ea.Complete();
      }
    }

    
    protected override Nesting CreateNesting()
    {
      return new Nesting<ForeignKeyInfo, TableInfo, ForeignKeyCollection>(this, "ForeignKeys");
    }

    
    protected override void Initialize()
    {
      base.Initialize();
      if (ForeignKeyColumns == null)
        ForeignKeyColumns = new ForeignKeyColumnCollection(this, "ForeignKeyColumns");
    }


    private static bool CompareKeyColumns(KeyColumnRef first, ForeignKeyColumnRef second)
    {
      if (first == null || second == null)
        return false;

      return first.Index==second.Index 
        && CompareTypes(first.Value.Type, second.Value.Type);
    }

    private static bool CompareTypes(StorageTypeInfo first, StorageTypeInfo second)
    {
      if (first==null || second==null)
        return false;
      if (first.IsTypeUndefined || second.IsTypeUndefined)
        return true;

      return first.Type.ToNullable()==second.Type.ToNullable()
        && first.Length==second.Length
          && first.Scale==second.Scale
            && first.Precision==second.Precision
              && first.Culture==second.Culture;
    }

    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="parent">The parent table.</param>
    /// <param name="name">The name of foreign key.</param>
    
    public ForeignKeyInfo(TableInfo parent, string name)
      : base(parent, name)
    {
    }
  }
}