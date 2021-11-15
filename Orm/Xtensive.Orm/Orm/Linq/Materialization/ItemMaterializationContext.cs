// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System.Reflection;
using Xtensive.Core;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Model;
using Xtensive.Tuples.Transform;
using Tuple = Xtensive.Tuples.Tuple;
using TypeInfo = Xtensive.Orm.Model.TypeInfo;

namespace Xtensive.Orm.Linq.Materialization
{
  internal sealed class ItemMaterializationContext
  {
    public static readonly MethodInfo IsMaterializedMethodInfo = WellKnownOrmTypes.ItemMaterializationContext.GetMethod(nameof(IsMaterialized));
    public static readonly MethodInfo GetEntityMethodInfo = WellKnownOrmTypes.ItemMaterializationContext.GetMethod(nameof(GetEntity));
    public static readonly MethodInfo MaterializeMethodInfo = WellKnownOrmTypes.ItemMaterializationContext.GetMethod(nameof(Materialize));
    public static readonly System.Reflection.FieldInfo SessionFieldInfo = WellKnownOrmTypes.ItemMaterializationContext.GetField(nameof(Session));

    public readonly Session Session;
    public readonly MaterializationContext MaterializationContext;

    private readonly TypeIdRegistry typeIdRegistry;
    private readonly Entity[] entities;

    public ParameterContext ParameterContext { get; }

    public bool IsMaterialized(int index) => entities[index] != null;

    public Entity GetEntity(int index) => entities[index];

    public Entity Materialize(int entityIndex, int typeIdIndex, TypeInfo type, Pair<int>[] entityColumns, Tuple tuple)
    {
      var result = entities[entityIndex];
      if (result!=null)
        return result;

      var typeId = EntityDataReader.ExtractTypeId(type, typeIdRegistry, tuple, typeIdIndex, out var accuracy);
      if (typeId==TypeInfo.NoTypeId)
        return null;

      var canCache = accuracy==TypeReferenceAccuracy.ExactType;
      var materializationInfo = MaterializationContext.GetTypeMapping(entityIndex, type, typeId, entityColumns);
      Key key;
      var keyIndexes = materializationInfo.KeyIndexes;
      if (!KeyFactory.IsValidKeyTuple(tuple, keyIndexes))
        return null;
      if (keyIndexes.Count <= WellKnown.MaxGenericKeyLength)
        key = KeyFactory.Materialize(Session.Domain, Session.StorageNodeId, materializationInfo.Type, tuple, accuracy, canCache, keyIndexes);
      else {
        var keyTuple = materializationInfo.KeyTransform.Apply(TupleTransformType.TransformedTuple, tuple);
        key = KeyFactory.Materialize(Session.Domain, Session.StorageNodeId, materializationInfo.Type, keyTuple, accuracy, canCache, null);
      }
      if (accuracy==TypeReferenceAccuracy.ExactType) {
        var entityTuple = materializationInfo.Transform.Apply(TupleTransformType.Tuple, tuple);
        var entityState = Session.Handler.UpdateState(key, entityTuple);
        result = entityState.Entity;
      }
      else {
        result = Session.Query.SingleOrDefault(key);
      }
      entities[entityIndex] = result;
      return result;
    }


    // Constructors

    public ItemMaterializationContext(MaterializationContext materializationContext, ParameterContext parameterContext)
    {
      ParameterContext = parameterContext;
      MaterializationContext = materializationContext;
      Session = materializationContext.Session;

      typeIdRegistry = Session.StorageNode.TypeIdRegistry;
      entities = new Entity[materializationContext.EntitiesInRow];
    }
  }
}
