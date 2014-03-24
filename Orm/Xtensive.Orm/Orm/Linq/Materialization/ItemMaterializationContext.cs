using System;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Model;
using Xtensive.Tuples.Transform;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Linq.Materialization
{
  internal sealed class ItemMaterializationContext
  {
    public static MethodInfo IsMaterializedMethodInfo { get; private set; }
    public static MethodInfo GetEntityMethodInfo      { get; private set; }
    public static MethodInfo MaterializeMethodInfo    { get; private set; }

    public static System.Reflection.FieldInfo SessionFieldInfo { get; private set; }

    public readonly Session Session;
    public readonly MaterializationContext MaterializationContext;

    private readonly TypeIdRegistry typeIdRegistry;
    private readonly Entity[] entities;

    // ReSharper disable UnusedMember.Global

    public bool IsMaterialized(int index)
    {
      return entities[index]!=null;
    }

    public Entity GetEntity(int index)
    {
      return entities[index];
    }

    public Entity Materialize(int entityIndex, int typeIdIndex, TypeInfo type, Pair<int>[] entityColumns, Tuple tuple)
    {
      var result = entities[entityIndex];
      if (result!=null)
        return result;

      TypeReferenceAccuracy accuracy;
      int typeId = RecordSetReader.ExtractTypeId(type, typeIdRegistry, tuple, typeIdIndex, out accuracy);
      if (typeId==TypeInfo.NoTypeId)
        return null;

      bool canCache = accuracy==TypeReferenceAccuracy.ExactType;
      var materializationInfo = MaterializationContext.GetTypeMapping(entityIndex, type, typeId, entityColumns);
      Key key;
      var keyIndexes = materializationInfo.KeyIndexes;
      if (!KeyFactory.IsValidKeyTuple(tuple, keyIndexes))
        return null;
      if (keyIndexes.Length <= WellKnown.MaxGenericKeyLength)
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

    // ReSharper restore UnusedMember.Global


    // Constructors

    public ItemMaterializationContext(MaterializationContext materializationContext, Session session)
    {
      MaterializationContext = materializationContext;
      Session = session;

      typeIdRegistry = session.StorageNode.TypeIdRegistry;
      entities = new Entity[materializationContext.EntitiesInRow];
    }

    static ItemMaterializationContext()
    {
      IsMaterializedMethodInfo = typeof (ItemMaterializationContext).GetMethod("IsMaterialized");
      GetEntityMethodInfo = typeof (ItemMaterializationContext).GetMethod("GetEntity");
      MaterializeMethodInfo = typeof (ItemMaterializationContext).GetMethod("Materialize");
      SessionFieldInfo = typeof (ItemMaterializationContext).GetField("Session");
    }
  }
}