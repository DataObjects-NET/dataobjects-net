// Copyright (C) 2007-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2007.08.27

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Validation;
using Xtensive.Tuples;
using Xtensive.Tuples.Transform;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Model
{
  /// <summary>
  /// Represents an object describing any persistent type.
  /// </summary>
  [DebuggerDisplay("{underlyingType}")]
  [Serializable]
  public sealed class TypeInfo : SchemaMappedNode
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

    private static readonly IReadOnlySet<TypeInfo> EmptyTypes = new HashSet<TypeInfo>();

    private readonly ColumnInfoCollection columns;
    private readonly FieldMap fieldMap;
    private readonly FieldInfoCollection fields;
    private readonly TypeIndexInfoCollection indexes;
    private readonly NodeCollection<IndexInfo> affectedIndexes;
    private readonly DomainModel model;
    private TypeAttributes attributes;
    private IReadOnlyList<AssociationInfo> targetAssociations;
    private IReadOnlyList<AssociationInfo> ownerAssociations;
    private IReadOnlyList<AssociationInfo> removalSequence;
    private IReadOnlyList<FieldInfo> versionFields;
    private IReadOnlyList<ColumnInfo> versionColumns;
    private IList<IObjectValidator> validators;
    private Type underlyingType;
    private HierarchyInfo hierarchy;
    private int typeId = NoTypeId;
    private object typeDiscriminatorValue;
    private MapTransform primaryKeyInjector;
    private bool isLeaf;
    private bool isOutboundOnly;
    private bool isInboundOnly;
    private KeyInfo key;
    private bool hasVersionRoots;
    private IDictionary<Pair<FieldInfo>, FieldInfo> structureFieldMapping;
    private List<AssociationInfo> overridenAssociations;
    private FieldInfo typeIdField;

    public TypeInfo Ancestor { get; internal set; }

    /// <summary>
    /// Gets the ancestors recursively. Inheritor-to-root order.
    /// </summary>
    /// <returns>The ancestor</returns>
    public IEnumerable<TypeInfo> AncestorChain
    {
      get {
        for (var ancestor = Ancestor; ancestor != null; ancestor = ancestor.Ancestor) {
          yield return ancestor;
        }
      }
    }

    private IReadOnlyList<TypeInfo> ancestors;

    /// <summary>
    /// Gets the ancestors recursively. Root-to-inheritor order. Reverse of AncestorChain.
    /// </summary>
    /// <returns>The ancestor</returns>
    public IReadOnlyList<TypeInfo> Ancestors => ancestors ??= AncestorChain.Reverse().ToList();

    private HashSet<TypeInfo> descendants;
    public IReadOnlySet<TypeInfo> Descendants => descendants ?? EmptyTypes;

    private IReadOnlySet<TypeInfo> recursiveDescendants;
    public IReadOnlySet<TypeInfo> RecursiveDescendants
    {
      get {
        if (recursiveDescendants == null) {
          if (Descendants.Count == 0) {
            recursiveDescendants = Descendants;
          }
          else {
            var set = new HashSet<TypeInfo>(Descendants);
            set.UnionWith(Descendants.SelectMany(static o => o.RecursiveDescendants));
            recursiveDescendants = set;
          }
        }
        return recursiveDescendants;
      }
    }

    private HashSet<TypeInfo> interfaces;
    public IReadOnlySet<TypeInfo> Interfaces => interfaces ?? EmptyTypes;

    private IReadOnlyList<TypeInfo> recursiveInterfaces;
    public IReadOnlyList<TypeInfo> RecursiveInterfaces =>
      recursiveInterfaces ??= (IsInterface ? Interfaces : Interfaces.Concat(AncestorChain.SelectMany(static o => o.Interfaces))).ToList();

    private HashSet<TypeInfo> implementors;

    /// <summary>
    /// Gets the direct implementors of this instance.
    /// </summary>
    public IReadOnlySet<TypeInfo> Implementors => implementors ?? EmptyTypes;

    private IReadOnlyList<TypeInfo> recursiveImplementors;
    public IReadOnlyList<TypeInfo> RecursiveImplementors
    {
      get {
        if (recursiveImplementors == null) {
          if (Implementors.Count == 0) {
            recursiveImplementors = Array.Empty<TypeInfo>();
          }
          else {
            var list = new List<TypeInfo>(Implementors.Count);
            foreach (var item in Implementors) {
              list.Add(item);
              if (!item.IsInterface) {
                list.AddRange(item.RecursiveDescendants);
              }
            }
            recursiveImplementors = list;
          }
        }
        return recursiveImplementors;
      }
    }

    /// <summary>
    /// Gets the direct implementors of this instance.
    /// </summary>
    /// <param name="recursive">if set to <see langword="true"/> then both direct and non-direct implementors will be returned.</param>
    [Obsolete("Use Implementors/RecursiveImplementors properties instead")]
    public IEnumerable<TypeInfo> GetImplementors(bool recursive = false) => recursive ? RecursiveImplementors : Implementors;

    /// <summary>
    /// Gets the persistent interfaces this instance implements.
    /// </summary>
    /// <param name="recursive">if set to <see langword="true"/> then both direct and non-direct implemented interfaces will be returned.</param>
    [Obsolete("Use Interfaces/RecursiveInterfaces properties instead")]
    public IEnumerable<TypeInfo> GetInterfaces(bool recursive = false) => recursive ? RecursiveInterfaces : Interfaces;

    /// <summary>
    /// Gets descendants of this instance.
    /// </summary>
    /// <param name="recursive">if set to <see langword="true"/> then both direct and nested descendants will be returned.</param>
    /// <returns></returns>
    [Obsolete("Use Descendants/RecursiveDescendants properties instead")]
    public IEnumerable<TypeInfo> GetDescendants(bool recursive) => recursive ? RecursiveDescendants : Descendants;

    /// <summary>
    /// Gets the ancestors recursively. Root-to-inheritor order.
    /// </summary>
    /// <returns>The ancestor</returns>
    [Obsolete("Use Ancestors property instead")]
    public IReadOnlyList<TypeInfo> GetAncestors() => Ancestors;

    private IReadOnlySet<TypeInfo> typeWithAncestorsAndInterfaces;
    public IReadOnlySet<TypeInfo> TypeWithAncestorsAndInterfaces
    {
      get {
        if (typeWithAncestorsAndInterfaces == null) {
          var candidates = new HashSet<TypeInfo>(Ancestors);
          candidates.UnionWith(RecursiveInterfaces);
          candidates.Add(this);
          typeWithAncestorsAndInterfaces = candidates;
        }
        return typeWithAncestorsAndInterfaces;
      }
    }

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

    ///<summary>
    /// Gets or sets a value indicating whether this instance is outbound only
    /// i.e. it's has only outgoing references
    /// </summary>
    public bool IsOutboundOnly
    {
      get { return isOutboundOnly; }
      set {
        EnsureNotLocked();
        isOutboundOnly = value;
      }
    }

    ///<summary>
    /// Gets or sets a value indicating whether this instance is inbound only
    /// i.e. it's has only incoming references
    /// </summary>
    public bool IsInboundOnly
    {
      get { return isInboundOnly; }
      set {
        EnsureNotLocked();
        isInboundOnly = value;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is auxiliary type.
    /// </summary>
    public bool IsAuxiliary
    {
      [DebuggerStepThrough]
      get { return (attributes & TypeAttributes.Auxiliary) == TypeAttributes.Auxiliary; }
      set {
        EnsureNotLocked();
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
    public int TypeId
    {
      [DebuggerStepThrough]
      get { return typeId; }
      set {
        if (typeId != NoTypeId)
          throw Exceptions.AlreadyInitialized("TypeId");
        typeId = value;
      }
    }

    /// <summary>
    /// Gets or sets the underlying system type.
    /// </summary>
    public Type UnderlyingType
    {
      [DebuggerStepThrough]
      get { return underlyingType; }
      set {
        EnsureNotLocked();
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
      get {
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
      set {
        EnsureNotLocked();
        hierarchy = value;
      }
    }

    /// <summary>
    /// Gets <see cref="KeyInfo"/> for this type.
    /// </summary>
    public KeyInfo Key
    {
      get { return IsLocked ? key : GetKey(); }
    }

    /// <summary>
    /// Gets or sets the type discriminator value.
    /// </summary>
    public object TypeDiscriminatorValue
    {
      get { return typeDiscriminatorValue; }
      set {
        EnsureNotLocked();
        typeDiscriminatorValue = value;
      }
    }

    /// <summary>
    /// Gets the tuple descriptor.
    /// </summary>
    public TupleDescriptor TupleDescriptor { get; private set; }

    /// <summary>
    /// Gets the persistent type prototype.
    /// </summary>
    public Tuple TuplePrototype { get; private set; }

    /// <summary>
    /// Gets the version tuple extractor.
    /// </summary>
    public MapTransform VersionExtractor { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this instance has version fields.
    /// </summary>
    public bool HasVersionFields { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this instance has explicit version fields.
    /// </summary>
    public bool HasExplicitVersionFields { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance has version roots.
    /// </summary>
    public bool HasVersionRoots
    {
      [DebuggerStepThrough]
      get { return hasVersionRoots; }
      [DebuggerStepThrough]
      set {
        EnsureNotLocked();
        hasVersionRoots = value;
      }
    }

    /// <summary>
    /// Gets the structure field mapping.
    /// </summary>
    /// <value>The structure field mapping.</value>
    public IDictionary<Pair<FieldInfo>, FieldInfo> StructureFieldMapping
    {
      get {
        return structureFieldMapping ?? BuildStructureFieldMapping();
      }
    }

    /// <summary>
    /// Gets <see cref="IObjectValidator"/> instances
    /// associated with this type.
    /// </summary>
    public IList<IObjectValidator> Validators
    {
      get { return validators; }
      internal set {
        EnsureNotLocked();
        validators = value;
      }
    }

    /// <summary>
    /// Gets value indicating if this type has validators (including field validators).
    /// </summary>
    public bool HasValidators { get; private set; }

    internal FieldAccessorProvider Accessors { get; private set; }

    /// <summary>
    /// Creates the tuple prototype with specified <paramref name="primaryKey"/>.
    /// </summary>
    /// <param name="primaryKey">The primary key to use.</param>
    /// <param name="typeIdValue">Identifier of <see cref="Entity"/> type.</param>
    /// <returns>
    /// The <see cref="TuplePrototype"/> with "injected" <paramref name="primaryKey"/>.
    /// </returns>
    public Tuple CreateEntityTuple(Tuple primaryKey, int typeIdValue)
    {
      var result = primaryKeyInjector.Apply(TupleTransformType.Tuple, primaryKey, TuplePrototype);
      if (typeIdField != null)
        result.SetValue(typeIdField.MappingInfo.Offset, typeIdValue);
      return result;
    }

    /// <summary>
    /// Injects the primary key into specified <paramref name="entityTuple"/>
    /// </summary>
    /// <param name="entityTuple">A <see cref="Tuple"/> instance where to inject
    /// the specified <paramref name="primaryKey"/></param>
    /// <param name="primaryKey">The primary key to inject.</param>
    /// <returns>
    /// The <paramref name="entityTuple"/> with "injected" <paramref name="primaryKey"/>.
    /// </returns>
    public Tuple InjectPrimaryKey(Tuple entityTuple, Tuple primaryKey)
    {
      return primaryKeyInjector.Apply(TupleTransformType.Tuple, primaryKey, entityTuple);
    }


    /// </summary>
    /// <returns>The hierarchy root.</returns>
    public TypeInfo GetRoot() =>
      IsInterface || IsStructure
        ? null
        : (Ancestors.FirstOrDefault() ?? this);

    public IEnumerable<AssociationInfo> GetTargetAssociations()
    {
      if (targetAssociations == null) {
        var result = model.Associations.Find(this, true);
        if (!IsLocked) {
          return result;
        }
        targetAssociations = result.ToList().AsReadOnly();
      }
      return targetAssociations;
    }

    /// <summary>
    /// Gets the associations this instance is participating in as owner (it has references to other entities).
    /// </summary>
    public IEnumerable<AssociationInfo> GetOwnerAssociations()
    {
      if (ownerAssociations == null) {
        var result = model.Associations.Find(this, false);
        if (!IsLocked) {
          return result;
        }
        ownerAssociations = result.ToList().AsReadOnly();
      }
      return ownerAssociations;
    }

    /// <summary>
    /// Gets the association sequence for entity removal.
    /// </summary>
    /// <returns></returns>
    public IReadOnlyList<AssociationInfo> GetRemovalAssociationSequence()
    {
      return removalSequence;
    }

    /// <summary>
    /// Gets the version field sequence.
    /// </summary>
    /// <returns>The version field sequence.</returns>
    public IEnumerable<FieldInfo> GetVersionFields()
    {
      if (versionFields == null) {
        var result = InnerGetVersionFields();
        if (!IsLocked) {
          return result;
        }
        versionFields = result.ToList();
      }
      return versionFields;
    }

    private IEnumerable<FieldInfo> InnerGetVersionFields()
    {
      var fields = Fields
        .Where(field => field.IsPrimitive && (field.AutoVersion || field.ManualVersion))
        .ToList();
      return fields.Count > 0
        ? fields
        : Fields.Where(f => f.IsPrimitive
          && !f.IsSystem
          && !f.IsPrimaryKey
          && !f.IsLazyLoad
          && !f.IsTypeId
          && !f.IsTypeDiscriminator
          && !f.ValueType.IsArray
          && !f.SkipVersion);
    }

    /// <summary>
    /// Gets the version columns.
    /// </summary>
    /// <returns>The version columns.</returns>
    public IEnumerable<ColumnInfo> GetVersionColumns()
    {
      if (versionColumns == null) {
        var result = InnerGetVersionColumns();
        if (!IsLocked) {
          return result;
        }
        versionColumns = result.ToList();
      }
      return versionColumns;
    }

    private IEnumerable<ColumnInfo> InnerGetVersionColumns() =>
      InnerGetVersionFields()
        .SelectMany(f => f.Columns)
        .OrderBy(c => c.Field.MappingInfo.Offset);

    /// <inheritdoc/>
    public override void UpdateState()
    {
      base.UpdateState();

      var adapterIndex = 0;
      foreach (var field in Fields) {
        if (field.IsStructure || field.IsEntitySet) {
          field.AdapterIndex = adapterIndex++;
        }
      }

      affectedIndexes.UpdateState();
      indexes.UpdateState();
      columns.UpdateState();

      CreateTupleDescriptor();

      columns.UpdateState();
      fields.UpdateState();

      structureFieldMapping = BuildStructureFieldMapping();

      if (IsEntity) {
        if (HasVersionRoots) {
          versionFields = Array.Empty<FieldInfo>();
          versionColumns = Array.Empty<ColumnInfo>();
        }
        else {
          versionFields = InnerGetVersionFields().ToList();
        }
        HasVersionFields = versionFields.Count > 0;
        HasExplicitVersionFields = versionFields.Any(f => f.ManualVersion || f.AutoVersion);
      }

      if (IsInterface) {
        // Collect mapping information from the first implementor (if any)
        // We'll check that all implementors are mapped to the same database later.
        // MappingSchema is not important: it's copied for consistency.
        var firstImplementor = Implementors.FirstOrDefault();
        if (firstImplementor != null) {
          MappingDatabase = firstImplementor.MappingDatabase;
          MappingSchema = firstImplementor.MappingSchema;
        }
      }

      HasValidators = validators.Count > 0 || fields.Any(f => f.HasValidators);

      // Selecting master parts from paired associations & single associations
      var associations = model.Associations.Find(this)
        .Where(a => a.IsMaster)
        .ToList();

      typeIdField = Fields.FirstOrDefault(f => f.IsTypeId && f.IsSystem);

      BuildTuplePrototype();
      BuildVersionExtractor();

      if (associations.Count == 0) {
        removalSequence = Array.Empty<AssociationInfo>();
        return;
      }

      overridenAssociations = associations
        .Where(a =>
          (a.Ancestors.Count > 0 && ((a.OwnerType == this && a.Ancestors.All(an => an.OwnerType != this) || (a.TargetType == this && a.Ancestors.All(an => an.TargetType != this))))) ||
          (a.Reversed != null && (a.Reversed.Ancestors.Count > 0 && ((a.Reversed.OwnerType == this && a.Reversed.Ancestors.All(an => an.OwnerType != this) || (a.Reversed.TargetType == this && a.Reversed.Ancestors.All(an => an.TargetType != this)))))))
        .SelectMany(a => a.Ancestors.Concat(a.Reversed == null ? Enumerable.Empty<AssociationInfo>() : a.Reversed.Ancestors))
        .ToList();
      var ancestor = Ancestor;
      if (ancestor != null && ancestor.overridenAssociations != null)
        overridenAssociations.AddRange(ancestor.overridenAssociations);

      foreach (var ancestorAssociation in overridenAssociations)
        associations.Remove(ancestorAssociation);

      //
      //Commented action sequence bellow may add dublicates to "sequence".
      //Besides, it takes 6 times enumeration of "associations"
      //

      //var sequence = new List<AssociationInfo>(associations.Count);
      //sequence.AddRange(associations.Where(a => a.OnOwnerRemove == OnRemoveAction.Deny && a.OwnerType.UnderlyingType.IsAssignableFrom(UnderlyingType)));
      //sequence.AddRange(associations.Where(a => a.OnTargetRemove == OnRemoveAction.Deny && a.TargetType.UnderlyingType.IsAssignableFrom(UnderlyingType)));
      //sequence.AddRange(associations.Where(a => a.OnOwnerRemove == OnRemoveAction.Clear && a.OwnerType.UnderlyingType.IsAssignableFrom(UnderlyingType)));
      //sequence.AddRange(associations.Where(a => a.OnTargetRemove == OnRemoveAction.Clear && a.TargetType.UnderlyingType.IsAssignableFrom(UnderlyingType)));
      //sequence.AddRange(associations.Where(a => a.OnOwnerRemove == OnRemoveAction.Cascade && a.OwnerType.UnderlyingType.IsAssignableFrom(UnderlyingType)));
      //sequence.AddRange(associations.Where(a => a.OnTargetRemove == OnRemoveAction.Cascade && a.TargetType.UnderlyingType.IsAssignableFrom(UnderlyingType)));

      //
      // Code bellow adds the same associations, but without dublicates.
      // Also it takes only one enumeration of associations sequence.
      //
      var sequence = new List<AssociationInfo>(associations.Count);
      var b = associations.Where(
        a => (a.OnOwnerRemove == OnRemoveAction.Deny && a.OwnerType.UnderlyingType.IsAssignableFrom(UnderlyingType)) ||
          (a.OnTargetRemove == OnRemoveAction.Deny && a.TargetType.UnderlyingType.IsAssignableFrom(UnderlyingType)) ||
          (a.OnOwnerRemove == OnRemoveAction.Clear && a.OwnerType.UnderlyingType.IsAssignableFrom(UnderlyingType)) ||
          (a.OnTargetRemove == OnRemoveAction.Clear && a.TargetType.UnderlyingType.IsAssignableFrom(UnderlyingType)) ||
          (a.OnOwnerRemove == OnRemoveAction.Cascade && a.OwnerType.UnderlyingType.IsAssignableFrom(UnderlyingType)) ||
          (a.OnTargetRemove == OnRemoveAction.Cascade && a.TargetType.UnderlyingType.IsAssignableFrom(UnderlyingType)));
      sequence.AddRange(b);

      var sortedRemovalSequence = sequence.Where(a => a.Ancestors.Count > 0).ToList();
      if (sortedRemovalSequence.Count == 0) {
        removalSequence = sequence.AsReadOnly();
      }
      else {
        var sequenceSize = sequence.Count;
        if (sortedRemovalSequence.Capacity < sequenceSize) {
          sortedRemovalSequence.Capacity = sequenceSize;
        }
        sortedRemovalSequence.AddRange(sequence.Where(a => a.Ancestors.Count == 0));
        removalSequence = sortedRemovalSequence.AsReadOnly();
      }
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

      if (IsEntity || IsStructure)
        Accessors = new FieldAccessorProvider(this);

      if (!recursive)
        return;

      validators = Array.AsReadOnly(validators.ToArray());

      affectedIndexes.Lock(true);
      indexes.Lock(true);
      columns.Lock(true);
      fieldMap.Lock(true);
      fields.Lock(true);
    }

    #region Private / internal methods

    private KeyInfo GetKey() =>
      Hierarchy != null ? Hierarchy.Key
        : IsInterface ? Implementors.First().Hierarchy.Key
        : null;

    private bool GetIsLeaf() =>
      IsEntity && Descendants.Count == 0;

    private void CreateTupleDescriptor()
    {
      var orderedColumns = columns.OrderBy(c => c.Field.MappingInfo.Offset).ToList(columns.Count);
      columns.Clear();                    // To prevent event handler leak
      columns.AddRange(orderedColumns);
      TupleDescriptor = TupleDescriptor.Create(
        Columns.Select(c => c.ValueType).ToArray(Columns.Count));
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

      // Initializing defaults
      i = 0;
      foreach (var column in Columns) {
        if (column.DefaultValue != null) {
          try {
            tuple.SetValue(i, column.DefaultValue);
          }
          catch (Exception e) {
            OrmLog.Error(e, nameof(Strings.LogExErrorSettingDefaultValueXForColumnYInTypeZ),
              column.DefaultValue, column.Name, Name);
          }
        }
        i++;
      }

      // Aditional initialization for entities
      if (IsEntity) {
        // Setting type discriminator column
        if (Hierarchy.TypeDiscriminatorMap != null)
          tuple.SetValue(Hierarchy.TypeDiscriminatorMap.Field.MappingInfo.Offset, typeDiscriminatorValue);

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

    private void BuildVersionExtractor()
    {
      // Building version tuple extractor
      var versionColumns = GetVersionColumns().ToList();
      var versionColumnsCount = versionColumns?.Count ?? 0;
      if (versionColumns == null || versionColumnsCount == 0) {
        VersionExtractor = null;
        return;
      }
      var types = versionColumns.Select(c => c.ValueType).ToArray(versionColumnsCount);
      var map = versionColumns.Select(c => c.Field.MappingInfo.Offset).ToArray(versionColumnsCount);
      var versionTupleDescriptor = TupleDescriptor.Create(types);
      VersionExtractor = new MapTransform(true, versionTupleDescriptor, map);
    }

    private IDictionary<Pair<FieldInfo>, FieldInfo> BuildStructureFieldMapping()
    {
      var result = new Dictionary<Pair<FieldInfo>, FieldInfo>();
      var structureFields = Fields.Where(f => f.IsStructure && f.Parent == null);
      foreach (var structureField in structureFields) {
        var structureTypeInfo = Model.Types[structureField.ValueType];
        foreach (var pair in structureTypeInfo.Fields.Zip(structureField.Fields, (first, second) => (first, second)))
          result.Add(new Pair<FieldInfo>(structureField, pair.first), pair.second);
      }
      return new ReadOnlyDictionary<Pair<FieldInfo>, FieldInfo>(result);
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return Name;
    }

    internal void AddDescendant(TypeInfo descendant) =>
         (descendants ??= new()).Add(descendant);

    internal void AddInterface(TypeInfo iface) =>
      (interfaces ??= new()).Add(iface);

    internal void AddImplementor(TypeInfo implementor) =>
      (implementors ??= new()).Add(implementor);

    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="typeAttributes">The type attributes.</param>
    public TypeInfo(DomainModel model, TypeAttributes typeAttributes)
    {
      this.model = model;
      attributes = typeAttributes;
      columns = new ColumnInfoCollection(this, "Columns");
      fields = new FieldInfoCollection(this, "Fields");
      fieldMap = IsEntity ? new FieldMap() : FieldMap.Empty;
      indexes = new TypeIndexInfoCollection(this, "Indexes");
      affectedIndexes = new NodeCollection<IndexInfo>(this, "AffectedIndexes");
    }
  }
}
