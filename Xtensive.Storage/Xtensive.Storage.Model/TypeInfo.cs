// Copyright (C) 2003-2010 Xtensive LLC.
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
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Tuple = Xtensive.Core.Tuples.Tuple;
using Xtensive.Core.Tuples.Transform;

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

    private ColumnInfoCollection               columns;
    private readonly FieldMap                  fieldMap;
    private readonly FieldInfoCollection       fields;
    private readonly TypeIndexInfoCollection   indexes;
    private readonly NodeCollection<IndexInfo> affectedIndexes;
    private readonly DomainModel               model;
    private TypeAttributes                     attributes;
    private ReadOnlyList<TypeInfo>             ancestors;
    private ReadOnlyList<AssociationInfo>      targetAssociations;
    private ReadOnlyList<AssociationInfo>      ownerAssociations;
    private ReadOnlyList<AssociationInfo>      removalSequence;
    private ReadOnlyList<FieldInfo>            versionFields;
    private ReadOnlyList<Pair<ColumnInfo, int>> versionColumns;
    private Type                               underlyingType;
    private HierarchyInfo                      hierarchy;
    private int                                typeId = NoTypeId;
    private MapTransform                       primaryKeyInjector;
    private bool                               isLeaf;
    private KeyInfo                    key;
    private bool                               hasVersionRoots;
    private Dictionary<Pair<FieldInfo>, FieldInfo> structureFieldMapping;

    #region IsXxx properties

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
    /// Gets a value indicating whether this instance is materialized interface.
    /// </summary>
    public bool IsMaterialized
    {
      [DebuggerStepThrough]
      get { return (attributes & TypeAttributes.Materialized) > 0; }
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
    /// Gets a value indicating whether this instance is a leaf type,
    /// i.e. its <see cref="GetDescendants()"/> method returns <see langword="0" />.
    /// </summary>
    public bool IsLeaf
    {
      [DebuggerStepThrough]
      get { return IsLocked ? isLeaf : GetIsLeaf(); }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is auxiliary type.
    /// </summary>
    public bool IsAuxiliary
    {
      [DebuggerStepThrough]
      get { return (attributes & TypeAttributes.Auxiliary) == TypeAttributes.Auxiliary; }
      set {
        this.EnsureNotLocked();
        attributes = value
          ? attributes | TypeAttributes.Auxiliary
          : attributes & ~TypeAttributes.Auxiliary;
      }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is generic type definition.
    /// </summary>
    public bool IsGenericTypeDefinition
    {
      get { return (attributes & TypeAttributes.GenericTypeDefinition) > 0; }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is automatically registered generic type instance.
    /// </summary>
    public bool IsAutoGenericInstance
    {
      get { return (attributes & TypeAttributes.AutoGenericInstance) > 0; }
    }

    #endregion

    /// <summary>
    /// Gets or sets the type identifier uniquely identifying the type in the domain model.
    /// </summary>
    /// <exception cref="NotSupportedException">Property is already initialized.</exception>
    public int TypeId {
      [DebuggerStepThrough]
      get { return typeId; }
      set {
        if (typeId != NoTypeId)
          throw Exceptions.AlreadyInitialized("TypeId");
        typeId = value;
        BuildTuplePrototype();
      }
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
    /// Gets full-text index if any, otherwise gets <see langword="null"/>.
    /// </summary>
    public FullTextIndexInfo FullTextIndex
    {
      [DebuggerStepThrough]
      get
      {
        FullTextIndexInfo fullTextIndexInfo;
        model.FullTextIndexes.TryGetValue(this, out fullTextIndexInfo);
        return fullTextIndexInfo;
      }
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

    private object typeDiscriminatorValue;

    public object TypeDiscriminatorValue
    {
      get { return typeDiscriminatorValue; }
      set { typeDiscriminatorValue = value; }
    }

    /// <summary>
    /// Gets the tuple descriptor.
    /// </summary>
    public TupleDescriptor TupleDescriptor { get; private set; }

    /// <summary>
    /// Gets the persistent type prototype.
    /// </summary>
    public Tuple TuplePrototype { get; private set; }

    public KeyInfo Key
    {
      get { return IsLocked ? key : GetKey(); }
    }

    /// <summary>
    /// Gets the version tuple extractor.
    /// </summary>
    public MapTransform VersionInfoTupleExtractor { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this instance has version fields.
    /// </summary>
    public bool HasVersionFields { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance has version roots.
    /// </summary>
    public bool HasVersionRoots {
      [DebuggerStepThrough]
      get { return hasVersionRoots; }
      [DebuggerStepThrough]
      set {
        this.EnsureNotLocked();
        hasVersionRoots = value;
      }
    }

    /// <summary>
    /// Gets the structure field mapping.
    /// </summary>
    /// <value>The structure field mapping.</value>
    public Dictionary<Pair<FieldInfo>, FieldInfo> StructureFieldMapping
    {
      get {
        return structureFieldMapping ?? BuildStructureFieldMapping();
      }
    }

    /// <summary>
    /// Creates the tuple prototype with specified <paramref name="primaryKey"/>.
    /// </summary>
    /// <param name="primaryKey">The primary key to use.</param>
    /// <returns>
    /// The <see cref="TuplePrototype"/> with "injected"
    /// (see <see cref="primaryKeyInjector"/>) <paramref name="primaryKey"/>.
    /// </returns>
    public Tuple CreateEntityTuple(Tuple primaryKey)
    {
      return primaryKeyInjector.Apply(TupleTransformType.Tuple, primaryKey, TuplePrototype);
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
    /// Gets the associations this instance is participating in as target (it is referenced by other entities).
    /// </summary>
    public IList<AssociationInfo> GetTargetAssociations()
    {
      if (IsLocked)
        return targetAssociations;

      return model.Associations.Find(this, true).ToList();
    }

    /// <summary>
    /// Gets the associations this instance is participating in as owner (it has references to other entities).
    /// </summary>
    public IList<AssociationInfo> GetOwnerAssociations()
    {
      if (IsLocked)
        return ownerAssociations;

      return model.Associations.Find(this, false).ToList();
    }

    /// <summary>
    /// Gets the association sequence for entity removal.
    /// </summary>
    /// <returns></returns>
    public IList<AssociationInfo> GetRemovalAssociationSequence()
    {
      return removalSequence;
    }

    /// <summary>
    /// Gets the version field sequence.
    /// </summary>
    /// <returns>The version field sequence.</returns>
    public IList<FieldInfo> GetVersionFields()
    {
      if (IsLocked)
        return versionFields;

      return Fields.Where(field => field.IsVersion).ToList();
    }

    /// <summary>
    /// Gets the version columns.
    /// </summary>
    /// <returns>The version columns.</returns>
    public IList<Pair<ColumnInfo, int>> GetVersionColumns()
    {
      if (IsLocked)
        return versionColumns;

      var versionFields = GetVersionFields();
      if (versionFields.Count==0)
        versionFields = (
          from field in Fields
          where field.IsPrimitive &&
            !field.IsPrimaryKey && !field.IsSystem && !field.IsLazyLoad && !field.ValueType.IsArray &&
            !((field.IsTypeId || field.IsTypeDiscriminator) && field.Parent==null)
          select field
          ).ToList();

      return (
        from field in versionFields
        from column in field.Columns
        select new Pair<ColumnInfo, int>(column, Columns.IndexOf(column))
        into pair
          orderby pair.Second
          select pair
        ).ToList();
    }

    /// <inheritdoc/>
    public override void UpdateState(bool recursive)
    {
      base.UpdateState(recursive);
      ancestors = new ReadOnlyList<TypeInfo>(GetAncestors());
      targetAssociations = new ReadOnlyList<AssociationInfo>(GetTargetAssociations());
      ownerAssociations = new ReadOnlyList<AssociationInfo>(GetOwnerAssociations());
      
      int adapterIndex = 0;
      foreach (FieldInfo field in Fields)
        if (field.IsStructure || field.IsEntitySet)
          field.AdapterIndex = adapterIndex++;

      if (recursive) {
        affectedIndexes.UpdateState(true);
        indexes.UpdateState(true);
        columns.UpdateState(true);
      }

      CreateTupleDescriptor();

      columns.UpdateState(true);
      fields.UpdateState(true);

      structureFieldMapping = BuildStructureFieldMapping();

      if (IsEntity || IsStructure)
        BuildTuplePrototype();
      
      if (IsEntity) {
        if (!HasVersionRoots) {
          versionFields = new ReadOnlyList<FieldInfo>(GetVersionFields());
          versionColumns = new ReadOnlyList<Pair<ColumnInfo, int>>(GetVersionColumns());
          CreateVersionExtractor();
        }
        else {
          versionFields = new ReadOnlyList<FieldInfo>(new List<FieldInfo>());
          versionColumns = new ReadOnlyList<Pair<ColumnInfo, int>>(new List<Pair<ColumnInfo, int>>());
        }
        HasVersionFields = versionFields.Count > 0;
      }
        
      // Selecting master parts from paired associations & single associations
      var associations = model.Associations.Find(this).Where(a => a.IsMaster).ToList();

      if (associations.Count == 0) {
        removalSequence = ReadOnlyList<AssociationInfo>.Empty;
        return;
      }

      var sequence = new List<AssociationInfo>(associations.Count);

      var items = associations.Where(a => a.OnOwnerRemove == OnRemoveAction.Deny && a.OwnerType.UnderlyingType.IsAssignableFrom(UnderlyingType));
      if (items != null) sequence.AddRange(items);
      items = associations.Where(a => a.OnTargetRemove == OnRemoveAction.Deny && a.TargetType.UnderlyingType.IsAssignableFrom(UnderlyingType));
      if (items != null) sequence.AddRange(items);
      items = associations.Where(a => a.OnOwnerRemove == OnRemoveAction.Clear && a.OwnerType.UnderlyingType.IsAssignableFrom(UnderlyingType));
      if (items != null) sequence.AddRange(items);
      items = associations.Where(a => a.OnTargetRemove == OnRemoveAction.Clear && a.TargetType.UnderlyingType.IsAssignableFrom(UnderlyingType));
      if (items != null) sequence.AddRange(items);
      items = associations.Where(a => a.OnOwnerRemove == OnRemoveAction.Cascade && a.OwnerType.UnderlyingType.IsAssignableFrom(UnderlyingType));
      if (items != null) sequence.AddRange(items);
      items = associations.Where(a => a.OnTargetRemove == OnRemoveAction.Cascade && a.TargetType.UnderlyingType.IsAssignableFrom(UnderlyingType));
      if (items != null) sequence.AddRange(items);

      removalSequence = new ReadOnlyList<AssociationInfo>(sequence.ToList());
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);

      int currentFieldId = FieldInfo.MinFieldId;
      foreach (var field in fields)
        field.FieldId = currentFieldId++;
      isLeaf = GetIsLeaf();
      key = GetKey();

      if (!recursive)
        return;
      affectedIndexes.Lock(true);
      indexes.Lock(true);
      columns.Lock(true);
      fieldMap.Lock(true);
      fields.Lock(true);
    }

    #region Private \ internal methods

    private KeyInfo GetKey()
    {
      if (Hierarchy == null)
        return IsInterface 
          ? GetImplementors().First().Hierarchy.Key 
          : null;
      return Hierarchy.Key;
    }

    private bool GetIsLeaf()
    {
      return IsEntity && !GetDescendants().Any();
    }

    private void CreateTupleDescriptor()
    {
      var orderedColumns = columns.OrderBy(c => c.Field.MappingInfo.Offset).ToList();
      columns = new ColumnInfoCollection();
      columns.AddRange(orderedColumns);
      TupleDescriptor = TupleDescriptor.Create(
        from c in Columns select c.ValueType);
    }

    private void BuildTuplePrototype()
    {
      // Building nullable map
      var nullabilityMap = new BitArray(TupleDescriptor.Count);
      int i = 0;
      foreach (var column in Columns)
        nullabilityMap[i++] = column.IsNullable;

      // fixing reference fields that are marked as not nullable
      foreach (var field in Fields.Where(f => f.IsEntity && !f.IsPrimaryKey && f.IsNullable == false)) {
        var segment = field.MappingInfo;
        for (int j = segment.Offset; j < segment.EndOffset; j++) {
          nullabilityMap[j] = true;
        }
      }

      // Building TuplePrototype
      var tuple = Tuple.Create(TupleDescriptor);
      tuple.Initialize(nullabilityMap);
      if (IsEntity) {

        // Setting TypeId column
        var typeIdField = Fields.Where(f => f.IsTypeId).FirstOrDefault();
        if (typeIdField != null)
          tuple.SetValue(typeIdField.MappingInfo.Offset, TypeId);

        // Setting type discriminator column
        if (Hierarchy.TypeDiscriminatorMap != null) {
          tuple.SetValue(Hierarchy.TypeDiscriminatorMap.Field.MappingInfo.Offset, typeDiscriminatorValue);
        }

        // Building primary key injector
        var fieldCount = TupleDescriptor.Count;
        var keyFieldCount = Key.TupleDescriptor.Count;
        var keyFieldMap = new Pair<int, int>[fieldCount];
        for (i = 0; i < fieldCount; i++)
          keyFieldMap[i] = new Pair<int, int>((i < keyFieldCount) ? 0 : 1, i);
        primaryKeyInjector = new MapTransform(false, TupleDescriptor, keyFieldMap);
      }
      TuplePrototype = IsEntity ? tuple.ToFastReadOnly() : tuple;
    }

    private void CreateVersionExtractor()
    {
      // Building version tuple extractor
      var versionColumns = GetVersionColumns();
      if (versionColumns.Count==0) {
        VersionInfoTupleExtractor = null;
        return;
      }
      var types = versionColumns.Select(pair => pair.First.ValueType);
      var map = versionColumns.Select(pair => pair.Second).ToArray();
      var versionTupleDescriptor = TupleDescriptor.Create(types.ToArray());
      VersionInfoTupleExtractor = new MapTransform(true, versionTupleDescriptor, map);
    }

    private Dictionary<Pair<FieldInfo>, FieldInfo> BuildStructureFieldMapping()
    {
      var result = new Dictionary<Pair<FieldInfo>, FieldInfo>();
      var structureFields = Fields.Where(f => f.IsStructure && f.Parent == null);
      foreach (var structureField in structureFields) {
        var structureTypeInfo = Model.Types[structureField.ValueType];
        foreach (var pair in structureTypeInfo.Fields.Zip(structureField.Fields))
          result.Add(new Pair<FieldInfo>(structureField, pair.First), pair.Second);
      }
      return result;
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return Name;
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