// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.10.03

using Xtensive.Storage.Model;

namespace Xtensive.Storage.Building.Builders
{
  internal static class ColumnBuilder
  {
    public static ColumnInfo BuildColumn(FieldInfo field)
    {
      var column = field.ValueType==typeof(Key) ? 
        new ColumnInfo(field, typeof(string)) :
        new ColumnInfo(field);
      column.Name = BuildingContext.Current.NameBuilder.Build(field, column);
      column.IsNullable = field.IsNullable;

      return column;
    }

    public static ColumnInfo BuildInheritedColumn(FieldInfo field, ColumnInfo ancestor)
    {
      var column = ancestor.Clone();
      column.Field = field;
      column.Name = BuildingContext.Current.NameBuilder.Build(field, ancestor);
      column.IsDeclared = field.IsDeclared;
      column.IsPrimaryKey = field.IsPrimaryKey;
      column.IsNullable = field.IsNullable;
      column.IsSystem = field.IsSystem;

      return column;
    }

    public static ColumnInfo BuildInterfaceColumn(FieldInfo field, ColumnInfo implementorColumn)
    {
      var column = BuildInheritedColumn(field, implementorColumn);
      column.IsInherited = false;
      return column;
    }
  }
}