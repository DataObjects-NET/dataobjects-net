// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.02.13

using System;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model.Resources;
using Xtensive.Core.Helpers;

namespace Xtensive.Storage.Model
{
  [Serializable]
  public sealed class KeyInfo : Node
  {
    private int typeIdFieldIndex;
    private int typeIdColumnIndex;
    private GeneratorInfo generatorInfo;

    /// <summary>
    /// Gets the fields that are included in the key.
    /// </summary>
    public DirectionCollection<FieldInfo> Fields { get; private set; }

    /// <summary>
    /// Gets the columns that are included in the key.
    /// </summary>
    public ColumnInfoCollection Columns { get; private set; }

    /// <summary>
    /// Gets the index of the field with <see cref="FieldInfo.IsTypeId"/>==<see langword="true" />.
    /// If there is no such field, returns <see langword="-1" />.
    /// </summary>
    public int TypeIdFieldIndex {
      get {
        return IsLocked ? typeIdFieldIndex : GetTypeIdFieldIndex();
      }
    }

    /// <summary>
    /// Gets the index of the column related to field with <see cref="FieldInfo.IsTypeId"/>==<see langword="true" />.
    /// If there is no such field, returns <see langword="-1" />.
    /// </summary>
    public int TypeIdColumnIndex {
      get {
        return IsLocked ? typeIdColumnIndex : GetTypeIdColumnIndex();
      }
    }

    /// <summary>
    /// Gets the length of the key.
    /// </summary>
    public int Length
    {
      get { return Columns.Count; }
    }

    /// <summary>
    /// Gets the tuple descriptor of the key.
    /// </summary>
    /// <value></value>
    public TupleDescriptor TupleDescriptor { get; private set; }

    /// <summary>
    /// Gets or sets the generator info.
    /// </summary>
    /// <value>The generator info.</value>
    public GeneratorInfo GeneratorInfo
    {
      get
      {
        return generatorInfo;
      }
      set
      {
        this.EnsureNotLocked();
        generatorInfo = value;
      }
    }

    /// <inheritdoc/>
    public override void UpdateState(bool recursive)
    {
      base.UpdateState(recursive);
      TupleDescriptor = TupleDescriptor.Create(
        from c in Columns select c.ValueType);
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      typeIdFieldIndex  = GetTypeIdFieldIndex();
      typeIdColumnIndex = GetTypeIdColumnIndex();
    }

    #region Private \ internal methods

    private int GetTypeIdFieldIndex()
    {
      var fields = Fields.Keys;
      int result = -1;
      for (int i = fields.Count - 1; i >= 0; i--) {
        var field = fields[i];
        if (field.IsTypeId) {
          if (result>=0)
            throw new InvalidOperationException(Strings.ExKeyContainsMultipleFieldsWithIsTypeIdTrueFlag);
          result = i;
        }
      }
      return result;
    }

    private int GetTypeIdColumnIndex()
    {
      int fieldIndex = GetTypeIdFieldIndex();
      if (fieldIndex<0)
        return -1;
      var field = Fields.Keys[fieldIndex];
      int i = 0;
      foreach (var column in Columns) {
        if (column.Field==field)
          return i;
        i++;
      }
      return -1;
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public KeyInfo()
    {
      Columns = new ColumnInfoCollection();
      Fields = new DirectionCollection<FieldInfo>();
    }
  }
}