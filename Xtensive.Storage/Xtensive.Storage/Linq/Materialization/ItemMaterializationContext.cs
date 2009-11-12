using System;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Linq.Materialization
{
  internal sealed class ItemMaterializationContext
  {
    public static MethodInfo IsMaterializedMethodInfo { get; private set; }
    public static MethodInfo GetEntityMethodInfo      { get; private set; }
    public static MethodInfo MaterializeMethodInfo    { get; private set; }

    public readonly Session Session;
    public readonly MaterializationContext MaterializationContext;
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

    /// <exception cref="InvalidOperationException">Something went wrong.</exception>
    public Entity Materialize(int entityIndex, int typeIdIndex, TypeInfo type, Pair<int>[] entityColumns, Tuple tuple)
    {
      var result = entities[entityIndex];
      if (result!=null)
        return result;

      TypeReferenceAccuracy accuracy;
      int typeId = RecordSetReader.ExtractTypeId(type, tuple, typeIdIndex, out accuracy);
      if (typeId==TypeInfo.NoTypeId)
        return null;

      bool canCache = accuracy==TypeReferenceAccuracy.ExactType;
      var materializationInfo = MaterializationContext.GetTypeMapping(entityIndex, type, typeId, entityColumns);
      Key key;
      if (materializationInfo.KeyIndexes.Length <= WellKnown.MaxGenericKeyLength)
        key = KeyFactory.Create(Domain.Demand(), materializationInfo.Type, tuple, accuracy, canCache, materializationInfo.KeyIndexes);
      else {
        var keyTuple = materializationInfo.KeyTransform.Apply(TupleTransformType.TransformedTuple, tuple);
        key = KeyFactory.Create(Domain.Demand(), materializationInfo.Type, keyTuple, accuracy, canCache, null);
      }
      if (accuracy==TypeReferenceAccuracy.ExactType) {
        var entityTuple = materializationInfo.Transform.Apply(TupleTransformType.Tuple, tuple);
        var entityState = Session.Handler.RegisterEntityState(key, entityTuple);
        result = entityState.Entity;
      }
      else {
        result = Query.SingleOrDefault(Session, key);
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
      entities = new Entity[materializationContext.EntitiesInRow];
    }

    static ItemMaterializationContext()
    {
      IsMaterializedMethodInfo = typeof (ItemMaterializationContext).GetMethod("IsMaterialized");
      GetEntityMethodInfo = typeof (ItemMaterializationContext).GetMethod("GetEntity");
      MaterializeMethodInfo = typeof (ItemMaterializationContext).GetMethod("Materialize");
    }
  }
}