// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.01.11

using System;
using System.Collections.Generic;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Model
{
  [Serializable]
  public sealed class HierarchyInfo : MappingNode
  {
    private readonly TypeInfo root;
    private readonly InheritanceSchema schema;
    private readonly Type generator;
    private readonly DirectionCollection<FieldInfo> fields = new DirectionCollection<FieldInfo>();
    private readonly ColumnInfoCollection columns = new ColumnInfoCollection();
    private TupleDescriptor keyTupleDescriptor;

    /// <summary>
    /// Gets the columns that are included in the key.
    /// </summary>
    public ColumnInfoCollection Columns
    {
      get { return columns; }
    }

    /// <summary>
    /// Gets the fields that are included in the key.
    /// </summary>
    public DirectionCollection<FieldInfo> Fields
    {
      get { return fields; }
    }

    /// <summary>
    /// Gets or sets the type instance of which is responsible for key generation.
    /// </summary>
    public Type Generator
    {
      get { return generator; }
    }

    /// <summary>
    /// Gets the root of the hierarchy.
    /// </summary>
    public TypeInfo Root
    {
      get { return root; }
    }

    /// <summary>
    /// Gets the <see cref="InheritanceSchema"/> for this hierarchy.
    /// </summary>
    public InheritanceSchema Schema
    {
      get { return schema; }
    }

    /// <summary>
    /// Gets the tuple descriptor of the key.
    /// </summary>
    /// <value></value>
    public TupleDescriptor KeyTupleDescriptor
    {
      get { return keyTupleDescriptor; }
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      List<Type> columnTypes = new List<Type>();
      foreach (ColumnInfo column in Columns)
        columnTypes.Add(column.ValueType);
      keyTupleDescriptor = TupleDescriptor.Create(columnTypes);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="root">The hierarchy root.</param>
    /// <param name="schema">The schema.</param>
    /// <param name="keyProvider">The key provider.</param>
    public HierarchyInfo(TypeInfo root, InheritanceSchema schema, Type keyProvider)
    {
      this.root = root;
      this.generator = keyProvider;
      this.schema = schema;
    }
  }
}