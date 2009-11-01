// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.09.21

using System;
using System.Globalization;
using Xtensive.Core;
using Xtensive.Core.Helpers;

namespace Xtensive.Storage.Rse
{
  [Serializable]
  public sealed class RecordColumn : LockableBase, IEquatable<RecordColumn> 
  {
    private ColumnInfoRef columnInfoRef;
    private string name;
    private int index;
    private Type type;
    private ColumnType columnType;

    public ColumnInfoRef ColumnInfoRef {
      get { return columnInfoRef; }
      set {
        this.EnsureNotLocked();
        columnInfoRef = value;
      }
    }

    public string Name {
      get { return name; }
      set {
        this.EnsureNotLocked();
        name = value;
      }
    }

    public int Index {
      get { return index; }
      set {
        this.EnsureNotLocked();
        index = value;
      }
    }

    public Type Type {
      get { return type; }
      set {
        this.EnsureNotLocked();
        type = value;
      }
    }

    public ColumnType ColumnType
    {
      get { return columnType; }
      set {
        this.EnsureNotLocked();
        columnType = value;
      }
    }

    public static bool operator ==(RecordColumn left, RecordColumn right)
    {
      return Equals(left, right);
    }

    public static bool operator !=(RecordColumn left, RecordColumn right)
    {
      return !Equals(left, right);
    }

    public bool Equals(RecordColumn recordColumn)
    {
      if (recordColumn == null)
        return false;
      if (ColumnInfoRef == null) {
        if (!Equals(Name, recordColumn.Name))
          return false;
      }
      else if (!ColumnInfoRef.Equals(recordColumn.ColumnInfoRef))
        return false;
      if (!Equals(Type, recordColumn.Type))
        return false;
      return true;
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(this, obj))
        return true;
      return Equals(obj as RecordColumn);
    }

    public override int GetHashCode()
    {
      int result = ColumnInfoRef != null ? ColumnInfoRef.GetHashCode() : 0;
      if (result == 0)
        result = 29*result + Name.GetHashCode();
      result = 29*result + Type.GetHashCode();
      return result;
    }

    // Constructors

    public RecordColumn(string name, int index, Type type)
      : this(name, index, type, ColumnType.Unbound)
    {}

    public RecordColumn(string name, int index, Type type, ColumnType columnType)
    {
      this.name = name;
      this.index = index;
      this.type = type;
      this.columnType = columnType;
    }

    public RecordColumn(ColumnInfoRef columnInfoRef, int index, Type type, ColumnType columnType)
    {
      this.columnInfoRef = columnInfoRef;
      Name = columnInfoRef.ColumnName;
      this.index = index;
      this.type = type;
      this.columnType = columnType;
    }

    public RecordColumn(RecordColumn recordColumn, string alias)
    {
      name = string.Concat(alias, ".", recordColumn.Name);
      columnInfoRef = recordColumn.ColumnInfoRef;
      type = recordColumn.Type;
      columnType = recordColumn.ColumnType;
      index = recordColumn.Index;
    }

    public RecordColumn(RecordColumn recordColumn, int index)
    {
      columnInfoRef = recordColumn.ColumnInfoRef;
      name = recordColumn.Name;
      type = recordColumn.Type;
      columnType = recordColumn.ColumnType;
      this.index = index;
    }
  }
}