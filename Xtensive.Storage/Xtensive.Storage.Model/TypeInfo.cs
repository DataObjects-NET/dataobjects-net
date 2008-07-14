// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.27

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Model
{
  /// <summary>
  /// Represents an object describing any persistent type.
  /// </summary>
  [DebuggerDisplay("{underlyingType}")]
  [Serializable]
  public sealed class TypeInfo: MappingNode
  {
    private readonly ColumnInfoCollection columns;
    private readonly FieldMap fieldMap;
    private readonly FieldInfoCollection fields;
    private readonly TypeIndexInfoCollection indexes;
    private readonly NodeCollection<IndexInfo> affectedIndexes;
    private readonly DomainInfo model;
    private readonly TypeAttributes attributes;
    private Type underlyingType;
    private HierarchyInfo hierarchy;
    private int typeId;
    private TupleDescriptor tupleDescriptor;

    /// <summary>
    /// Gets a value indicating whether this instance is entity.
    /// </summary>
    public bool IsEntity
    {
      get { return (attributes & TypeAttributes.Entity) > 0; }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is abstract entity.
    /// </summary>
    public bool IsAbstract
    {
      get { return (attributes & TypeAttributes.Abstract) > 0; }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is interface.
    /// </summary>
    public bool IsInterface
    {
      get { return (attributes & TypeAttributes.Interface) > 0; }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is structure.
    /// </summary>
    public bool IsStructure
    {
      get { return (attributes & TypeAttributes.Structure) > 0; }
    }

    /// <summary>
    /// Gets or sets the underlying system type.
    /// </summary>
    public Type UnderlyingType
    {
      get { return underlyingType; }
      set
      {
        this.EnsureNotLocked();
        underlyingType = value;
      }
    }

    /// <summary>
    /// Gets the attributes.
    /// </summary>
    public TypeAttributes Attributes
    {
      get { return attributes; }
    }


    /// <summary>
    /// Gets the columns contained in this instance.
    /// </summary>
    public ColumnInfoCollection Columns
    {
      get { return columns; }
    }

    /// <summary>
    /// Gets the indexes for this instance.
    /// </summary>
    public TypeIndexInfoCollection Indexes
    {
      get { return indexes; }
    }

    public NodeCollection<IndexInfo> AffectedIndexes
    {
      get { return affectedIndexes; }
    }

    /// <summary>
    /// Gets the fields contained in this instance.
    /// </summary>
    public FieldInfoCollection Fields
    {
      get { return fields; }
    }

    /// <summary>
    /// Gets the field map for implemented interfaces.
    /// </summary>
    public FieldMap FieldMap
    {
      get { return fieldMap; }
    }

    /// <summary>
    /// Gets the <see cref="DomainInfo"/> this instance belongs to.
    /// </summary>
    public DomainInfo Model
    {
      get { return model; }
    }

    /// <summary>
    /// Gets or sets the hierarchy.
    /// </summary>
    public HierarchyInfo Hierarchy
    {
      get { return hierarchy; }
      set
      {
        this.EnsureNotLocked();
        hierarchy = value;
      }
    }

    public int TypeId
    {
      get { return typeId; }
      set
      {
        this.EnsureNotLocked();
        typeId = value;
      }
    }

    /// <summary>
    /// Gets the tuple descriptor.
    /// </summary>
    /// <value></value>
    public TupleDescriptor TupleDescriptor
    {
      get { return tupleDescriptor; }
    }

    /// <summary>
    /// Gets the direct descendants of this instance.
    /// </summary>
    public IEnumerable<TypeInfo> GetDescendants()
    {
      return GetDescendants(false);
    }

    /// <summary>
    /// Gets descendants of this instance.
    /// </summary>
    /// <param name="recursive">if set to <see langword="true"/> then both direct and nested descendants will be returned.</param>
    /// <returns></returns>
    public IEnumerable<TypeInfo> GetDescendants(bool recursive)
    {
      return model.Types.FindDescendants(this, recursive);
    }

    /// <summary>
    /// Gets the direct persistent interfaces this instance implements.
    /// </summary>
    public IEnumerable<TypeInfo> GetInterfaces()
    {
      return model.Types.FindInterfaces(this);
    }

    /// <summary>
    /// Gets the persistent interfaces this instance implements.
    /// </summary>
    /// <param name="recursive">if set to <see langword="true"/> then both direct and non-direct implemented interfaces will be returned.</param>
    public IEnumerable<TypeInfo> GetInterfaces(bool recursive)
    {
      return model.Types.FindInterfaces(this, true);
    }

    /// <summary>
    /// Gets the direct implementors of this instance.
    /// </summary>
    public IEnumerable<TypeInfo> GetImplementors()
    {
      return model.Types.FindImplementors(this);
    }

    /// <summary>
    /// Gets the direct implementors of this instance.
    /// </summary>
    /// <param name="recursive">if set to <see langword="true"/> then both direct and non-direct implementors will be returned.</param>
    public IEnumerable<TypeInfo> GetImplementors(bool recursive)
    {
      return model.Types.FindImplementors(this, recursive);
    }

    /// <summary>
    /// Gets the ancestor.
    /// </summary>
    /// <returns>The ancestor</returns>
    public TypeInfo GetAncestor()
    {
      return model.Types.FindAncestor(this);
    }

    /// <summary>
    /// Gets the ancestors recursively. Root-to-inheritor order.
    /// </summary>
    /// <returns>The ancestor</returns>
    public List<TypeInfo> GetAncestors()
    {
      var result = new List<TypeInfo>();
      var ancestor = model.Types.FindAncestor(this);
      while(ancestor != null) {
        result.Add(ancestor);
        ancestor = model.Types.FindAncestor(ancestor);
      }
      result.Reverse();
      return result;
    }

    /// <summary>
    /// Gets the root of the hierarchy.
    /// </summary>
    /// <returns>The hierarchy root.</returns>
    public TypeInfo GetRoot()
    {
      return model.Types.FindRoot(this);
    }

    /// <summary>
    /// Gets the associations this instance is participating in.
    /// </summary>
    public IEnumerable<AssociationInfo> GetAssociations()
    {
      return model.Associations.FindAssociations(this);
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (recursive) {
        affectedIndexes.Lock(true);
        indexes.Lock(true);
        columns.Lock(true);
        fieldMap.Lock(true);
        fields.Lock(true);
      }
      if (IsInterface)
        return;
      List<Type> columnTypes = new List<Type>();
      if (IsEntity)
        foreach (ColumnInfo column in Indexes.PrimaryIndex.Columns)
          columnTypes.Add(column.ValueType);
      else if (IsStructure)
        foreach (ColumnInfo column in Columns)
          columnTypes.Add(column.ValueType);
      tupleDescriptor = TupleDescriptor.Create(columnTypes);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="typeAttributes">The type attributes.</param>
    public TypeInfo(DomainInfo model, TypeAttributes typeAttributes)
    {
      this.model = model;
      attributes = typeAttributes;
      columns = new ColumnInfoCollection();
      fields = new FieldInfoCollection();
      fieldMap = IsEntity ? new FieldMap() : FieldMap.Empty;
      indexes = new TypeIndexInfoCollection();
      affectedIndexes = new NodeCollection<IndexInfo>();
    }
  }
}