// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.10.22

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Disconnected
{
  internal sealed class AssociationCache
  {
    private readonly DisconnectedState disconnectedState;
    // Triplet<Association, MasterField, SlaveField>
    private readonly Dictionary<TypeInfo, Triplet<AssociationInfo, FieldInfo, FieldInfo>> auxTypeDescriptions = 
      new Dictionary<TypeInfo, Triplet<AssociationInfo, FieldInfo, FieldInfo>>();
    private readonly Dictionary<TypeInfo, List<Pair<FieldInfo>>> entitySets = 
      new Dictionary<TypeInfo, List<Pair<FieldInfo>>>();
    private readonly Dictionary<TypeInfo, List<FieldInfo>> entitySetFields = 
      new Dictionary<TypeInfo, List<FieldInfo>>();
    private readonly Dictionary<TypeInfo, List<FieldInfo>> referencingFields = 
      new Dictionary<TypeInfo, List<FieldInfo>>();

    private static FieldInfo GetTypeField(TypeInfo typeInfo, FieldInfo candidate)
    {
      FieldInfo typeField;
      return typeInfo.FieldMap.TryGetValue(candidate, out typeField) 
               ? typeField 
               : candidate;
    }


    /// <summary>
    /// Gets the references from state.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="tuple">The tuple.</param>
    /// <returns><see cref="ReferenceDescriptor"/> instances.</returns>
    public IEnumerable<ReferenceDescriptor> GetReferencesFrom(Key key, Tuple tuple)
    {
      var type = key.TypeReference.Type;
      var baseType = type.UnderlyingType.BaseType;
      var isAuxEntity = baseType.IsGenericType && baseType.GetGenericTypeDefinition()==typeof (EntitySetItem<,>);
      if (isAuxEntity) {
        var typeDesc = GetEntitySetArgs(type);
        if (typeDesc.First.Multiplicity==Multiplicity.ZeroToMany) {
          var ownerKey = GetKeyFieldValue(typeDesc.Second, tuple);
          var itemKey = GetKeyFieldValue(typeDesc.Third, tuple);
          if (ownerKey!=null && itemKey!=null)
            yield return new ReferenceDescriptor(itemKey, typeDesc.First.OwnerField, ownerKey);
        }
      }
      else {
        foreach (var field in GetReferencingFields(type)) {
          var ownerKey = GetKeyFieldValue(field, tuple);
          if (ownerKey!=null)
            yield return new ReferenceDescriptor(ownerKey, field, key);
        }
      }
    }

    /// <summary>
    /// Gets entity set items.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="tuple">The tuple.</param>
    /// <returns><see cref="EntitySetItemDesc"/> instances.</returns>
    public IEnumerable<EntitySetItemDesc> GetEntitySetItems(Key key, Tuple tuple)
    {
      var type = key.TypeReference.Type;
      var baseType = type.UnderlyingType.BaseType;
      var isAuxEntity = baseType.IsGenericType && baseType.GetGenericTypeDefinition()==typeof (EntitySetItem<,>);
      if (isAuxEntity) {
        var args = GetEntitySetArgs(type);
        var ownerKey = GetKeyFieldValue(args.Second, tuple);
        var itemKey = GetKeyFieldValue(args.Third, tuple);
        if (ownerKey!=null && itemKey!=null) {
          if (args.First.Multiplicity==Multiplicity.ManyToMany)
            yield return new EntitySetItemDesc(itemKey, args.First.OwnerField, ownerKey);
          yield return new EntitySetItemDesc(ownerKey, args.First.Master.OwnerField, itemKey);
        }
      }
      else {
        foreach (var pair in GetEntitySets(type)) {
          var ownerKey = GetKeyFieldValue(pair.First, tuple);
          if (ownerKey!=null)
            yield return new EntitySetItemDesc(ownerKey, pair.Second, key);
        }
      }
    }

    /// <summary>
    /// Gets the entity sets for many to one associations.
    /// </summary>
    /// <param name="typeInfo">The type info.</param>
    /// <returns>Set of pairs (First = <see cref="EntitySet{TItem}"/> field, Second = Paired field).</returns>
    public IEnumerable<Pair<FieldInfo>> GetEntitySets(TypeInfo typeInfo)
    {
      List<Pair<FieldInfo>> result;
      if (!entitySets.TryGetValue(typeInfo, out result)) {
        result = typeInfo.GetOwnerAssociations()
          .Where(association => association.Multiplicity==Multiplicity.ManyToOne)
          .Select(association => new Pair<FieldInfo>(
            GetTypeField(typeInfo, association.OwnerField), 
            GetTypeField(typeInfo, association.Reversed.OwnerField)))
          .ToList();
        entitySets.Add(typeInfo, result);
      }
      return result;
    }

    /// <summary>
    /// Gets all entity set fields.
    /// </summary>
    /// <param name="typeInfo">The type info.</param>
    /// <returns><see cref="FieldInfo"/> instances for all entity set fields.</returns>
    public IEnumerable<FieldInfo> GetEntitySetFields(TypeInfo typeInfo)
    {
      List<FieldInfo> result;
      if (!entitySetFields.TryGetValue(typeInfo, out result)) {
        result = typeInfo.Fields.Where(field => field.IsEntitySet).ToList();
        entitySetFields.Add(typeInfo, result);
      }
      return result;
    }

    /// <summary>
    /// Gets the referencing fields for zero to one and many to one references.
    /// </summary>
    /// <param name="typeInfo">The type info.</param>
    /// <returns><see cref="FieldInfo"/> instances.</returns>
    public IEnumerable<FieldInfo> GetReferencingFields(TypeInfo typeInfo)
    {
      List<FieldInfo> result;
      if (!referencingFields.TryGetValue(typeInfo, out result)) {
        result = typeInfo.GetOwnerAssociations()
          .Where(association => association.Multiplicity==Multiplicity.ZeroToOne
            || association.Multiplicity==Multiplicity.ManyToOne)
          .Select(association => GetTypeField(typeInfo, association.OwnerField))
          .ToList();
        referencingFields.Add(typeInfo, result);
      }
      return result;
    }

    /// <summary>
    /// Gets the value of FK field.
    /// </summary>
    /// <param name="field">The field.</param>
    /// <param name="tuple">The tuple.</param>
    /// <returns>Key value.</returns>
    public Key GetKeyFieldValue(FieldInfo field, Tuple tuple)
    {
      if (!field.IsEntity)
        throw new InvalidOperationException();
      var session = disconnectedState.Session;
      var types = session.Domain.Model.Types;
      var type = types[field.ValueType];
      if (tuple.ContainsEmptyValues(field.MappingInfo))
        return null;
      int typeIdFieldIndex = -1;
      if (type.Hierarchy != null)
        typeIdFieldIndex = type.Hierarchy.Key.TypeIdColumnIndex;
      bool exactType = typeIdFieldIndex >= 0;
      var keyValue = field.ExtractValue(tuple);
      if (exactType) {
        int typeId = keyValue.GetValueOrDefault<int>(typeIdFieldIndex);
        if (typeId!=TypeInfo.NoTypeId) // != default(int) != 0
          type = types[typeId];
        else
          // This may happen if referense is null
          exactType = false;
      }
      var key = Key.Create(session.Domain, session.StorageNodeId, type, exactType ? TypeReferenceAccuracy.ExactType : TypeReferenceAccuracy.BaseType, keyValue);
      return key;
    }

    private Triplet<AssociationInfo, FieldInfo, FieldInfo> GetEntitySetArgs(TypeInfo typeInfo)
    {
      Triplet<AssociationInfo, FieldInfo, FieldInfo> result;
      if (!auxTypeDescriptions.TryGetValue(typeInfo, out result)) {
          var association = typeInfo.Model.Associations.First(a => a.AuxiliaryType==typeInfo);
          var masterField = typeInfo.Fields[WellKnown.MasterFieldName];
          var slaveField = typeInfo.Fields[WellKnown.SlaveFieldName];
          result = new Triplet<AssociationInfo, FieldInfo, FieldInfo>(association, masterField, slaveField);
          auxTypeDescriptions.Add(typeInfo, result);
      }
      return result;
    }


    // Constructors

    public AssociationCache(DisconnectedState disconnectedState)
    {
      this.disconnectedState = disconnectedState;
    }
  }
}