// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.27

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Model.Resources;

namespace Xtensive.Storage.Model
{
  /// <summary>
  /// Represents an object describing any persistent type.
  /// </summary>
  [DebuggerDisplay("{underlyingType}")]
  [Serializable]
  public sealed class TypeInfo: MappingNode
  {
    /// <summary>
    /// "No <see cref="TypeId"/>" value (<see cref="TypeId"/> is unknown or undefined).
    /// Value is <see langword="0" />.
    /// </summary>
    public const int NoTypeId = 0;

    /// <summary>
    /// Minimal possible <see cref="TypeId"/> value.
    /// Value is <see langword="100" />.
    /// </summary>
    public const int MinTypeId = 100;

    private ColumnInfoCollection                            columns;
    private readonly FieldMap                               fieldMap;
    private readonly FieldInfoCollection                    fields;
    private readonly TypeIndexInfoCollection                indexes;
    private readonly NodeCollection<IndexInfo>              affectedIndexes;
    private readonly DomainModel                            model;
    private readonly TypeAttributes                         attributes;
    private ReadOnlyList<TypeInfo>                          ancestors;
    private ReadOnlyList<AssociationInfo>                   associations;
    private ReadOnlyList<AssociationInfo>                   outgoingAssociations;
    private Type                                            underlyingType;
    private HierarchyInfo                                   hierarchy;
    private int                                             typeId = NoTypeId;
    private TupleDescriptor                                 tupleDescriptor;

    /// <summary>
    /// Gets a value indicating whether this instance is entity.
    /// </summary>
    public bool IsEntity
    {
      [DebuggerStepThrough]
      get { return (attributes & TypeAttributes.Entity) > 0; }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is abstract entity.
    /// </summary>
    public bool IsAbstract
    {
      [DebuggerStepThrough]
      get { return (attributes & TypeAttributes.Abstract) > 0; }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is interface.
    /// </summary>
    public bool IsInterface
    {
      [DebuggerStepThrough]
      get { return (attributes & TypeAttributes.Interface) > 0; }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is structure.
    /// </summary>
    public bool IsStructure
    {
      [DebuggerStepThrough]
      get { return (attributes & TypeAttributes.Structure) > 0; }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is system type.
    /// </summary>
    public bool IsSystem
    {
      [DebuggerStepThrough]
      get { return (attributes & TypeAttributes.System) > 0; }
    }

    /// <summary>
    /// Gets or sets the underlying system type.
    /// </summary>
    public Type UnderlyingType
    {
      [DebuggerStepThrough]
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
      [DebuggerStepThrough]
      get { return attributes; }
    }


    /// <summary>
    /// Gets the columns contained in this instance.
    /// </summary>
    public ColumnInfoCollection Columns
    {
      [DebuggerStepThrough]
      get { return columns; }
    }

    /// <summary>
    /// Gets the indexes for this instance.
    /// </summary>
    public TypeIndexInfoCollection Indexes
    {
      [DebuggerStepThrough]
      get { return indexes; }
    }

    public NodeCollection<IndexInfo> AffectedIndexes
    {
      [DebuggerStepThrough]
      get { return affectedIndexes; }
    }

    /// <summary>
    /// Gets the fields contained in this instance.
    /// </summary>
    public FieldInfoCollection Fields
    {
      [DebuggerStepThrough]
      get { return fields; }
    }

    /// <summary>
    /// Gets the field map for implemented interfaces.
    /// </summary>
    public FieldMap FieldMap
    {
      [DebuggerStepThrough]
      get { return fieldMap; }
    }

    /// <summary>
    /// Gets the <see cref="DomainModel"/> this instance belongs to.
    /// </summary>
    public DomainModel Model
    {
      [DebuggerStepThrough]
      get { return model; }
    }

    /// <summary>
    /// Gets or sets the hierarchy.
    /// </summary>
    public HierarchyInfo Hierarchy
    {
      [DebuggerStepThrough]
      get { return hierarchy; }
      set
      {
        this.EnsureNotLocked();
        hierarchy = value;
      }
    }

    /// <summary>
    /// Gets or sets the type id.
    /// </summary>
    /// <value></value>
    public int TypeId
    {
      [DebuggerStepThrough]
      get { return typeId; }
      set
      {
        SetTypeId(value, null);
      }
    }

    /// <summary>
    /// Sets the type id for this type, even if model is already locked.
    /// </summary>
    /// <param name="value">The type id value.</param>
    /// <param name="unlockLockKey">The unlock key, that can be obtained by <see cref="DomainModel.GetUnlockKey"/>.</param>
    public void SetTypeId(int value, object unlockLockKey)    
    {
      if (unlockLockKey!=Model.unlockKey)
        this.EnsureNotLocked();
      SetTypeId(value);
    }

    private void SetTypeId(int value)
    {
      if (typeId != 0)
        throw new InvalidOperationException(
          string.Format(Strings.TypeIdForTypeXIsAlreadyAssigned, underlyingType.Name));
      typeId = value;
      BuildTuplePrototype();
    }

    /// <summary>
    /// Gets the tuple descriptor.
    /// </summary>
    /// <value></value>
    public TupleDescriptor TupleDescriptor
    {
      [DebuggerStepThrough]
      get { return tupleDescriptor; }
    }

    /// <summary>
    /// Gets the persistent type prototype.
    /// </summary>
    public Tuple TuplePrototype { get; private set; }

    /// <summary>
    /// Gets the primary key injection transform.
    /// </summary>
    public MapTransform PrimaryKeyInjector { get; private set; }

    /// <summary>
    /// Creates the tuple prototype with specified <paramref name="primaryKey"/>.
    /// </summary>
    /// <param name="primaryKey">The primary key to use.</param>
    /// <returns>
    /// The <see cref="TuplePrototype"/> with "injected"
    /// (see <see cref="PrimaryKeyInjector"/>) <paramref name="primaryKey"/>.
    /// </returns>
    public Tuple CreateEntityTuple(Tuple primaryKey)
    {
      return PrimaryKeyInjector.Apply(TupleTransformType.TransformedTuple, primaryKey, TuplePrototype);
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
      return model.Types.FindInterfaces(this, recursive);
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
    public IList<TypeInfo> GetAncestors()
    {
      if (IsLocked)
        return ancestors;

      var result = new List<TypeInfo>();
      var ancestor = model.Types.FindAncestor(this);
      // TODO: Refactor
      while (ancestor!=null) {
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
    public IList<AssociationInfo> GetAssociations()
    {
      if (IsLocked)
        return associations;

      return model.Associations.Find(this).ToList();
    }

    /// <summary>
    /// Gets the associations this instance is participating in.
    /// </summary>
    public IList<AssociationInfo> GetOutgoingAssociations()
    {
      if (IsLocked)
        return outgoingAssociations;

      return model.Associations.FindOutgoingAssocitions(this).ToList();
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      ancestors = new ReadOnlyList<TypeInfo>(GetAncestors());
      associations = new ReadOnlyList<AssociationInfo>(GetAssociations());
      outgoingAssociations = new ReadOnlyList<AssociationInfo>(GetOutgoingAssociations());
      base.Lock(recursive);
      if (recursive) {
        affectedIndexes.Lock(true);
        indexes.Lock(true);
        columns.Lock(true);
        fieldMap.Lock(true);
      }
      if (IsInterface) {
        if (recursive)
          fields.Lock(true);
        return;
      }
      CreateTupleDescriptor();

      columns.Lock(true);      
      fields.Lock(true);

      if (IsEntity || IsStructure) {
        BuildTuplePrototype();
      }
    }

    private void CreateTupleDescriptor()
    {
      var orderedColumns = columns.OrderBy(c => c.Field.MappingInfo.Offset).ToList();
      columns = new ColumnInfoCollection();
      columns.AddRange(orderedColumns);
      tupleDescriptor = TupleDescriptor.Create(
        from c in Columns select c.ValueType);
    }

    private void BuildTuplePrototype()
    {
      // Building nullable map
      var nullableMap = new BitArray(TupleDescriptor.Count);
      int i = 0;
      foreach (var column in Columns)
        nullableMap[i++] = column.IsNullable;

      // Building TuplePrototype
      var tuple = Tuple.Create(TupleDescriptor);
      tuple.Initialize(nullableMap);
      if (IsEntity){
        var typeIdField = Fields.Where(f => f.IsTypeId).First();
        tuple.SetValue(typeIdField.MappingInfo.Offset, TypeId);

        // Building primary key injector
        var fieldCount = TupleDescriptor.Count;
        var keyFieldCount = Hierarchy.KeyInfo.TupleDescriptor.Count;
        var keyFieldMap = new Pair<int, int>[fieldCount];
        for (i = 0; i < fieldCount; i++)
          keyFieldMap[i] = new Pair<int, int>((i < keyFieldCount) ? 0 : 1, i);
        PrimaryKeyInjector = new MapTransform(true, TupleDescriptor, keyFieldMap);
      }
      TuplePrototype = IsEntity ? tuple.ToFastReadOnly() : tuple;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      return underlyingType.Name;
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="typeAttributes">The type attributes.</param>
    public TypeInfo(DomainModel model, TypeAttributes typeAttributes)
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