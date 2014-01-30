// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
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
      object key = new Pair<object, AssociationInfo>(CachingRegion, association);
      Func<object, object> generator = p => BuildReferencingQuery(((Pair<object, AssociationInfo>)p).Second);
      var pair = (Pair<CompilableProvider, Parameter<Tuple>>)Session.Domain.Cache.GetValue(key, generator);
      var recordSet = pair.First;
      var parameter = pair.Second;
      var parameterContext = new ParameterContext();
      ExecutableProvider executableProvider;
      using (parameterContext.Activate()) {
        parameter.Value = target.Key.Value;
        executableProvider = Session.CompilationService.Compile(recordSet);
      }
      var queryTask = new QueryTask(executableProvider, Session.GetLifetimeToken(), parameterContext);
      Session.RegisterDelayedQuery(queryTask);

      return GetReferencesToInternal(association, target, recordSet.Header, queryTask);
    }

    private IEnumerable<ReferenceInfo> GetReferencesToInternal(AssociationInfo association, Entity target, RecordSetHeader header, QueryTask queryTask)
    {
      Session.ExecuteDelayedQueries(true);
      foreach (var entity in queryTask.ToEntities(header, Session, 0))
        yield return new ReferenceInfo(entity, target, association);
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

    private static Pair<CompilableProvider, Parameter<Tuple>> BuildReferencingQuery(AssociationInfo association)
    {
      var provider = (CompilableProvider)null;
      var parameter = new Parameter<Tuple>();
      switch (association.Multiplicity) {
        case Multiplicity.ZeroToOne:
        case Multiplicity.ManyToOne: {
          var index = association.OwnerType.Indexes.PrimaryIndex;
          var nonLazyColumnsSelector = index
            .Columns
            .Select((column, i)=>new {Column = column, Index = i})
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
            .Select((column, i)=>new {Column = column, Index = i})
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
            .Select((column, i)=>new {Column = column, Index = i})
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
      return new Pair<CompilableProvider, Parameter<Tuple>>(provider, parameter);
    }
  }
}