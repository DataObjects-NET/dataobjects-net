using System;
using System.Linq;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Linq.Materialization
{
  internal sealed class ItemMaterializationContext
  {
    public static MethodInfo IsMaterializedMethodInfo { get; private set; }
    public static MethodInfo GetEntityMethodInfo      { get; private set; }
    public static MethodInfo MaterializeMethodInfo    { get; private set; }

    private readonly Session session;
    private readonly Entity[] entities;
    private readonly MaterializationContext materializationContext;

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

      bool exactType;
      int typeId = RecordSetParser.ExtractTypeId(type, tuple, typeIdIndex, out exactType);
      if (typeId==TypeInfo.NoTypeId)
        return null;

      var materializationInfo = materializationContext.GetTypeMapping(entityIndex, typeId, entityColumns);
      Key key;
      if (materializationInfo.KeyIndexes.Length <= Key.MaxGenericKeyLength)
        key = Key.Create(session.Domain, materializationInfo.Type, tuple, materializationInfo.KeyIndexes, exactType, exactType);
      else {
        var keyTuple = materializationInfo.KeyTransform.Apply(TupleTransformType.TransformedTuple, tuple);
        key = Key.Create(session.Domain, materializationInfo.Type, keyTuple, null, exactType, exactType);
      }
      if (exactType) {
        var entityTuple = materializationInfo.Transform.Apply(TupleTransformType.Tuple, tuple);
        var entityState = session.UpdateEntityState(key, entityTuple);
        result = entityState.Entity;
      }
      else
        result = key.Resolve(session);
      entities[entityIndex] = result;
      return result;
    }
// ReSharper restore UnusedMember.Global


    // Constructors

    public ItemMaterializationContext(MaterializationContext materializationContext, Session session)
    {
      this.materializationContext = materializationContext;
      this.session = session;
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