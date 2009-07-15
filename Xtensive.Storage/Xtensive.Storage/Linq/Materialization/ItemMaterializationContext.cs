using System;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Model;
using System.Linq;

namespace Xtensive.Storage.Linq.Materialization
{
  internal sealed class ItemMaterializationContext
  {
    public static MethodInfo IsMaterializedMethodInfo{ get; private set;}
    public static MethodInfo GetEntityMethodInfo{ get; private set;}
    public static MethodInfo MaterializeMethodInfo{ get; private set;}

    private readonly Entity[] entities;
    private readonly Session session;
    private readonly MaterializationContext parentContext;

// ReSharper disable UnusedMember.Global
    public bool IsMaterialized(int index)
    {
      return entities[index]!=null;
    }

    public Entity GetEntity(int index)
    {
      return entities[index];
    }

    public Entity Materialize(int entityIndex, int typeIdIndex, TypeInfo typeInfo, Pair<int>[] columns, Tuple tuple)
    {
      var result = entities[entityIndex];
      if (result!=null)
        return result;


      int typeId;
      if (typeIdIndex >= 0) {
        if (tuple.HasValue(typeIdIndex))
          typeId = tuple.GetValueOrDefault<int>(typeIdIndex);
        else if (tuple.IsAvailable(typeIdIndex))
          return null;
        else
          return null;
        // throw new InvalidOperationException(string.Format("Unable to materialize entity. There is no TypeId field."));
      }
      else
        typeId = TypeInfo.NoTypeId;

      var exactType = typeId != TypeInfo.NoTypeId;
      if (!exactType) {
        typeId = typeInfo.TypeId;
        exactType = typeInfo.GetDescendants().Count()==0;
      }
      
      var materializationData = parentContext.GetEntityMaterializationData(entityIndex, typeId, columns);
      var entityTuple = materializationData.Transform.Apply(TupleTransformType.Tuple, tuple);
      Key key;
      if (!Key.TryCreateGenericKey(materializationData.EntityType,
        materializationData.KeyFields, entityTuple, exactType, false, out key)) {
        var entityKeyTuple = materializationData.KeyTransform.Apply(TupleTransformType.Tuple, entityTuple);
        key = Key.Create(materializationData.EntityType, entityKeyTuple, exactType);
      }
      if (exactType) {
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
      parentContext = materializationContext;
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