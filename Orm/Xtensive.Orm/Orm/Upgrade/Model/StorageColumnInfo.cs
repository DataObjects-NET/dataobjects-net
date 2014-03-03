// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;
using Xtensive.Core;
using Xtensive.Modelling;
using Xtensive.Modelling.Attributes;

namespace Xtensive.Orm.Upgrade.Model
{
  /// <summary>
  /// Column.
  /// </summary>
  [Serializable]
  public sealed class StorageColumnInfo : NodeBase<TableInfo>
  {
    private StorageTypeInfo type;
    private object defaultValue;
    private string defaultSqlExpression;

    /// <summary>
    /// Gets or sets the type of the column.
    /// </summary>
    [Property(Priority = -1000)]
    public StorageTypeInfo Type {
      get { return type; }
      set {
        EnsureIsEditable();
        using (var scope = LogPropertyChange("Type", value)) {
          type = value;
          scope.Commit();
        }
      }
    }

    /// <summary>
    /// Gets or sets the default column value.
    /// </summary>
    [Property(IgnoreInComparison = true)]
    public object DefaultValue
    {
      get { return defaultValue; }
      set {
        EnsureIsEditable();
        using (var scope = LogPropertyChange("DefaultValue", value)) {
          defaultValue = value;
          scope.Commit();
        }
      }
    }

    /// <summary>
    /// Gets or sets arbitrary SQL expression as default value for this column.
    /// This default value has higher priority than <see cref="StorageColumnInfo.DefaultValue"/>.
    /// </summary>
    [Property(IgnoreInComparison = true)]
    public string DefaultSqlExpression
    {
      get { return defaultSqlExpression; }
      set
      {
        EnsureIsEditable();
        using (var scope = LogPropertyChange("DefaultSqlExpression", value)) {
          defaultSqlExpression = value;
          scope.Commit();
        }
      }
    }

    /// <inheritdoc/>
    /// <exception cref="ValidationException">Validation error.</exception>
    protected override void ValidateState()
    {
      using (var ea = new ExceptionAggregator()) {
        ea.Execute(base.ValidateState);
        if (Type==null) {
          ea.Execute(() => {
            throw new ValidationException(
              string.Format(Strings.ExUndefinedTypeOfColumnX, Name),
              Path);
          });
        }
        ea.Complete();
      }
    }

    /// <inheritdoc/>
    protected override Nesting CreateNesting()
    {
      return new Nesting<StorageColumnInfo, TableInfo, ColumnInfoCollection>(this, "Columns");
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="table">The parent table.</param>
    /// <param name="name">The name.</param>
    public StorageColumnInfo(TableInfo table, string name)
      : base(table, name)
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="table">The parent table.</param>
    /// <param name="name">The column name.</param>
    /// <param name="type">Type of the column.</param>
    public StorageColumnInfo(TableInfo table, string name, StorageTypeInfo type)
      : this(table, name)
    {
      Type = type;
    }
  }
}