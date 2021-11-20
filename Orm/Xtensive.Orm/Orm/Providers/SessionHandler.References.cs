// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.08.19

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Model;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Rse.Providers;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Providers
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
      if (association.IsPaired)
        return FindReferences(target, association, true);
      var (recordSet, parameter) = Session.StorageNode.InternalAssociationCache.GetOrAdd(association, BuildReferencingQuery);
      var parameterContext = new ParameterContext();
      parameterContext.SetValue(parameter, target.Key.Value);
      ExecutableProvider executableProvider = Session.Compile(recordSet);

      var queryTask = new QueryTask(executableProvider, Session.GetLifetimeToken(), parameterContext);
      Session.RegisterInternalDelayedQuery(queryTask);

      return GetReferencesToInternal(association, target, recordSet.Header, queryTask);
    }

    /// <summary>
    /// Gets the references from specified entity.
    /// </summary>
    /// <param name="owner">The owner.</param>
    /// <param name="association">The association.</param>
    /// <returns>References.</returns>
    public virtual IEnumerable<ReferenceInfo> GetReferencesFrom(Entity owner, AssociationInfo association)
    {
      return FindReferences(owner, association, false);
    }

    private IEnumerable<ReferenceInfo> GetReferencesToInternal(AssociationInfo association, Entity target, RecordSetHeader header, QueryTask queryTask)
    {
      Session.ExecuteInternalDelayedQueries(true);

      var referenceToTarget = queryTask.ToEntities(header, Session, 0).Where(e=>!e.IsRemoved);
      var removedReferences = Session.NonPairedReferencesRegistry.GetRemovedReferencesTo(target.State, association).Select(es=>es.Entity);
      var addedReferences = Session.NonPairedReferencesRegistry.GetAddedReferenceTo(target.State, association).Select(es => es.Entity).Where(e=>!e.IsRemoved);
      var exceptRemovedReferences = referenceToTarget.Except(removedReferences);
      var withNewReferences = exceptRemovedReferences.Concat(addedReferences);
      foreach (var entity in withNewReferences)
        yield return new ReferenceInfo(entity, target, association);
    }

    private static IEnumerable<ReferenceInfo> FindReferences(Entity owner, AssociationInfo association, bool reversed)
    {
      Func<Entity, Entity, AssociationInfo, ReferenceInfo> referenceCtor = (o, t, a) => new ReferenceInfo(o, t, a);
      if (reversed) {
        association = association.Reversed;
        referenceCtor = (o, t, a) => new ReferenceInfo(t, o, a);
      }
      switch (association.Multiplicity) {
        case Multiplicity.ZeroToOne:
        case Multiplicity.OneToOne:
        case Multiplicity.ManyToOne:
          var target = (Entity) owner.GetFieldValue(association.OwnerField);
          if (target != null && association.TargetType.UnderlyingType.IsAssignableFrom(target.TypeInfo.UnderlyingType))
            yield return referenceCtor(owner, target, association);
          break;
        case Multiplicity.ZeroToMany:
        case Multiplicity.OneToMany:
        case Multiplicity.ManyToMany:
          var targets = (EntitySetBase) owner.GetFieldValue(association.OwnerField);
          foreach (var item in targets.Entities)
            if (association.TargetType.UnderlyingType.IsAssignableFrom(item.TypeInfo.UnderlyingType))
              yield return referenceCtor(owner, (Entity)item, association);
          break;
      }
    }

    private static (CompilableProvider, Parameter<Tuple>) BuildReferencingQuery(AssociationInfo association)
    {
      var provider = (CompilableProvider)null;
      var parameter = new Parameter<Tuple>("pTuple");
      switch (association.Multiplicity) {
        case Multiplicity.ZeroToOne:
        case Multiplicity.ManyToOne: {
          var index = association.OwnerType.Indexes.PrimaryIndex;
          var nonLazyColumnsSelector = index
            .Columns
            .Select((column, i) => (Column: column, Index: i))
            .Where(a=>!a.Column.IsLazyLoad)
            .Select(a=>a.Index)
            .ToArray();
          provider = index.GetQuery()
            .Filter(QueryHelper.BuildFilterLambda(
              association.OwnerField.MappingInfo.Offset,
              association.OwnerField.Columns.Select(c => c.ValueType).ToList(),
              parameter))
            .Select(nonLazyColumnsSelector);
          break;
        }
        case Multiplicity.OneToOne:
        case Multiplicity.OneToMany: {
          var index = association.OwnerType.Indexes.PrimaryIndex;
          var targetIndex = association.TargetType.Indexes.PrimaryIndex;
          var nonLazyColumnsSelector = index
            .Columns
            .Select((column, i) => (Column: column, Index: i))
            .Where(a=>!a.Column.IsLazyLoad)
            .Select(a=>targetIndex.Columns.Count + a.Index)
            .ToArray();
          provider = targetIndex.GetQuery()
            .Filter(QueryHelper.BuildFilterLambda(0,
              association.TargetType.Key.TupleDescriptor,
              parameter))
            .Alias("a")
            .Join(
              index.GetQuery(), 
              association.Reversed.OwnerField.MappingInfo
                .GetItems()
                .Select((l,r) => new Pair<int>(l,r))
                .ToArray())
            .Select(nonLazyColumnsSelector);
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
          var nonLazyColumnsSelector = index
            .Columns
            .Select((column, i) => (Column: column, Index: i))
            .Where(a=>!a.Column.IsLazyLoad)
            .Select(a=>targetIndex.Columns.Count + a.Index)
            .ToArray();
          var referencingField = association.IsMaster
            ? association.AuxiliaryType.Fields[WellKnown.SlaveFieldName]
            : association.AuxiliaryType.Fields[WellKnown.MasterFieldName];
          var referencedField = association.IsMaster
            ? association.AuxiliaryType.Fields[WellKnown.MasterFieldName]
            : association.AuxiliaryType.Fields[WellKnown.SlaveFieldName];
          provider = targetIndex.GetQuery()
            .Filter(QueryHelper.BuildFilterLambda(
              referencingField.MappingInfo.Offset,
              referencingType.Key.TupleDescriptor,
              parameter))
            .Alias("a")
            .Join(
              index.GetQuery(),
              referencedField.MappingInfo
                .GetItems()
                .Select((l, r) => new Pair<int>(l, r))
                .ToArray())
            .Select(nonLazyColumnsSelector);
          break;
        }
      }
      return (provider, parameter);
    }
  }
}
