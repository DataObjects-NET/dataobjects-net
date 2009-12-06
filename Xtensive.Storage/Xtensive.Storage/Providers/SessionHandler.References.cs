// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.19

using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Linq;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Providers
{
  partial class SessionHandler
  {
    /// <summary>
    /// Gets the references to specified entity.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="association">The association.</param>
    /// <returns>References.</returns>
    public virtual IEnumerable<ReferenceInfo> GetReferencesTo(Entity target, AssociationInfo association)
    {
      var result = new List<ReferenceInfo>();
      using (new ParameterContext().Activate()) {
        var pair = (Pair<RecordSet, Parameter<Tuple>>)Session.Domain.GetCachedItem(
          new Pair<object, AssociationInfo>(CachingRegion, association),
          p => BuildReferencingQuery(((Pair<object, AssociationInfo>)p).Second));
        var recordSet = pair.First;
        var parameter = pair.Second;
        parameter.Value = target.Key.Value;
        foreach (var item in recordSet.ToEntities(0))
          result.Add(new ReferenceInfo(item, target, association));
      }
      return result;

    }

    /// <summary>
    /// Gets the references from specified entity.
    /// </summary>
    /// <param name="owner">The owner.</param>
    /// <param name="association">The association.</param>
    /// <returns>References.</returns>
    public virtual IEnumerable<ReferenceInfo> GetReferencesFrom(Entity owner, AssociationInfo association)
    {
      switch (association.Multiplicity) {
        case Multiplicity.ZeroToOne:
        case Multiplicity.OneToOne:
        case Multiplicity.ManyToOne:
          var target = owner.GetFieldValue<IEntity>(association.OwnerField);
          if (target != null)
            yield return new ReferenceInfo(owner, (Entity) target, association);
          break;
        case Multiplicity.ZeroToMany:
        case Multiplicity.OneToMany:
        case Multiplicity.ManyToMany:
          var targets = owner.GetFieldValue<EntitySetBase>(association.OwnerField);
          foreach (var item in targets.Entities)
            yield return new ReferenceInfo(owner, (Entity) item, association);
          break;
      }
    }

    private static Pair<RecordSet,Parameter<Tuple>> BuildReferencingQuery(AssociationInfo association)
    {
      var recordSet = (RecordSet)null;
      var parameter = new Parameter<Tuple>();
      switch (association.Multiplicity) {
        case Multiplicity.ZeroToOne:
        case Multiplicity.ManyToOne: {
          var index = association.OwnerType.Indexes.PrimaryIndex;
          recordSet = index.ToRecordSet()
            .Filter(QueryHelper.BuildFilterLambda(
              association.OwnerField.MappingInfo.Offset,
              association.OwnerField.Columns.Select(c => c.ValueType).ToList(),
              parameter));
          break;
        }
        case Multiplicity.OneToOne:
        case Multiplicity.OneToMany: {
          var index = association.OwnerType.Indexes.PrimaryIndex;
          var targetIndex = association.TargetType.Indexes.PrimaryIndex;
          recordSet = targetIndex.ToRecordSet()
            .Filter(QueryHelper.BuildFilterLambda(0,
              association.TargetType.KeyProviderInfo.TupleDescriptor,
              parameter))
            .Alias("a")
            .Join(
              index.ToRecordSet(), 
              JoinAlgorithm.Loop, 
              association.Reversed.OwnerField.MappingInfo
                .GetItems()
                .Select((l,r) => new Pair<int>(l,r))
                .ToArray())
            .Select(Enumerable.Range(targetIndex.Columns.Count, index.Columns.Count).ToArray());
          break;
        }
        case Multiplicity.ZeroToMany:
        case Multiplicity.ManyToMany: {
          var referencedType = association.IsMaster
            ? association.OwnerType
            : association.TargetType;
          var referencingType = association.IsMaster
            ? association.TargetType
            : association.OwnerType;
          var index = referencedType.Indexes.PrimaryIndex;
          var targetIndex = association.AuxiliaryType.Indexes.PrimaryIndex;
          var referencingField = association.IsMaster
            ? association.AuxiliaryType.Fields[WellKnown.SlaveFieldName]
            : association.AuxiliaryType.Fields[WellKnown.MasterFieldName];
          var referencedField = association.IsMaster
            ? association.AuxiliaryType.Fields[WellKnown.MasterFieldName]
            : association.AuxiliaryType.Fields[WellKnown.SlaveFieldName];
          recordSet = targetIndex.ToRecordSet()
            .Filter(QueryHelper.BuildFilterLambda(
              referencingField.MappingInfo.Offset,
              referencingType.KeyProviderInfo.TupleDescriptor,
              parameter))
            .Alias("a")
            .Join(
              index.ToRecordSet(),
              JoinAlgorithm.Loop,
              referencedField.MappingInfo
                .GetItems()
                .Select((l, r) => new Pair<int>(l, r))
                .ToArray())
            .Select(Enumerable.Range(targetIndex.Columns.Count, index.Columns.Count).ToArray());
          break;
        }
      }
      return new Pair<RecordSet, Parameter<Tuple>>(recordSet, parameter);
    }
  }
}