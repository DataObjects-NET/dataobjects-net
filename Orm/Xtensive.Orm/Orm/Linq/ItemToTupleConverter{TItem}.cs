// Copyright (C) 2009-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Gamzov
// Created:    2009.10.01

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Linq.Expressions;
using Xtensive.Orm.Model;
using Xtensive.Reflection;
using Xtensive.Tuples;
using FieldInfo = System.Reflection.FieldInfo;
using Tuple = Xtensive.Tuples.Tuple;
using TypeInfo = Xtensive.Orm.Model.TypeInfo;

namespace Xtensive.Orm.Linq
{
  internal sealed class ItemToTupleConverter<TItem> : ItemToTupleConverter
  {
    private struct TupleTypeCollection
    {
      private const int InitialTypesCollectorCapacity = 8;

      private List<Type> typesList;
      private Type[] singleType;
      private int count;

      public void Add(Type type)
      {
        if (count == 0) {
          singleType[0] = type;
          count++;
          return;
        }
        if (typesList is null) {
          typesList = new List<Type>(InitialTypesCollectorCapacity) { singleType[0], type };
          singleType = null;
          count++;
        }
        else {
          typesList.Add(type);
          count++;
        }
      }

      public void AddRange(IReadOnlyCollection<Type> newTypes)
      {
        var addedCount = newTypes.Count;
        if (addedCount == 0) {
          return;
        }

        if (addedCount == 1) {
          Add(newTypes.First());
          return;
        }

        if (typesList is null) {
          typesList = (count > 0)
            ? new List<Type>(InitialTypesCollectorCapacity) { singleType[0] }
            : new List<Type>(InitialTypesCollectorCapacity);
          typesList.AddRange(newTypes);
          singleType = null;
          count += newTypes.Count;
        }
        else {
          typesList.AddRange(newTypes);
          count += newTypes.Count;
        }
      }

      public Type[] ToArray()
      {
        return count == 0
          ? Array.Empty<Type>()
          : singleType ?? typesList.ToArray();
      }

      public TupleTypeCollection()
      {
        count = 0;
        singleType = new Type[1];
        typesList = null;
      }
    }

    private enum PersistableKind
    {
      Entity,
      Structure,
      PersistentInteface,
      RegularField,
      Unknown
    }

    private static readonly Type RefOfTType = typeof(Ref<>);
    private static readonly ParameterExpression ParamContext = Expression.Parameter(WellKnownOrmTypes.ParameterContext, "context");
    private static readonly MethodInfo SelectMethod = WellKnownMembers.Enumerable.Select.MakeGenericMethod(typeof(TItem), WellKnownOrmTypes.Tuple);

    private readonly Func<ParameterContext, IEnumerable<TItem>> enumerableFunc;
    private readonly DomainModel model;
    private readonly Type entityTypeStoredInKey;
    private readonly bool isKeyConverter;
    private readonly Func<TItem, Tuple> converter;

    public override Expression<Func<ParameterContext, IEnumerable<Tuple>>> GetEnumerable()
    {
      var call = Expression.Call(Expression.Constant(enumerableFunc.Target), enumerableFunc.Method, ParamContext);
      var select = Expression.Call(SelectMethod, call, Expression.Constant(converter));
      return FastExpression.Lambda<Func<ParameterContext, IEnumerable<Tuple>>>(select, ParamContext);
    }

    /// <exception cref="InvalidOperationException"><c>InvalidOperationException</c>.</exception>
    private PersistableKind GetPersistableKind(Type type)
    {
      if (type == WellKnownOrmTypes.Entity
        || type.IsSubclassOf(WellKnownOrmTypes.Entity)) {
        if (!model.Types.Contains(type))
          throw new InvalidOperationException(string.Format(Strings.ExTypeNotFoundInModel, type.FullName));
        return PersistableKind.Entity;
      }

      if (type == WellKnownOrmTypes.Structure || type.IsSubclassOf(WellKnownOrmTypes.Structure)) {
        if (!model.Types.Contains(type))
          throw new InvalidOperationException(string.Format(Strings.ExTypeNotFoundInModel, type.FullName));
        return PersistableKind.Structure;
      }

      if (type.IsInterface && type.IsAssignableTo(WellKnownOrmInterfaces.Entity)) {
        if (!model.Types.Contains(type))
          throw new InvalidOperationException(string.Format(Strings.ExTypeNotFoundInModel, type.FullName));
        return PersistableKind.PersistentInteface;
      }
      if (TypeIsStorageMappable(type)) {
        return PersistableKind.RegularField;
      }
      return PersistableKind.Unknown;
    }

    private static bool TypeIsStorageMappable(Type type)
    {
      // TODO: AG: Take info from storage!
      type = type.StripNullable();
      return type.IsPrimitive ||
        type.IsEnum ||
        type == WellKnownTypes.Guid ||
        type == WellKnownTypes.DateTime ||
        type == WellKnownTypes.String ||
        type == WellKnownTypes.TimeSpan ||
        type == WellKnownTypes.DateTimeOffset ||
        type == WellKnownTypes.DateOnly ||
        type == WellKnownTypes.TimeOnly ||
        type == WellKnownTypes.Decimal ||
        type == WellKnownTypes.ByteArray;
    }

    private static void FillLocalCollectionField(object item, Tuple tuple, Expression expression)
    {
      if (item is null)
        return;

      switch (expression) {
        case LocalCollectionExpression itemExpression:
          foreach (var field in itemExpression.Fields) {
            var value = field.Key is PropertyInfo propertyInfo
              ? propertyInfo.GetValue(item, BindingFlags.InvokeMethod, null, null, null)
              : ((FieldInfo) field.Key).GetValue(item);
            if (value is not null)
              FillLocalCollectionField(value, tuple, (Expression) field.Value);
          }
          break;
        case ColumnExpression columnExpression:
          tuple.SetValue(columnExpression.Mapping.Offset, item);
          break;
        case StructureExpression structureExpression:
          var structure = (Structure) item;
          var typeInfo = structureExpression.PersistentType;
          var tupleDescriptor = typeInfo.TupleDescriptor;
          var tupleSegment = new Segment<int>(0, tupleDescriptor.Count);
          var structureTuple = structure.Tuple.GetSegment(tupleSegment);
          structureTuple.CopyTo(tuple, 0, structureExpression.Mapping.Offset, structureTuple.Count);
          break;
        case EntityExpression entityExpression: {
          var entity = (Entity) item;
          var keyTuple = entity.Key.Value;
          keyTuple.CopyTo(tuple, 0, entityExpression.Key.Mapping.Offset, keyTuple.Count);
          break;
        }
        case KeyExpression keyExpression: {
          var key = (Key) item;
          var keyTuple = key.Value;
          keyTuple.CopyTo(tuple, 0, keyExpression.Mapping.Offset, keyTuple.Count);
          break;
        }
        default:
          throw new NotSupportedException();
      }
    }

    private LocalCollectionExpression BuildLocalCollectionExpression(Type type,
      HashSet<Type> processedTypes, ref int columnIndex, MemberInfo parentMember, ref TupleTypeCollection types, Expression sourceExpression)
    {
      if (type.IsAssignableFrom(WellKnownOrmTypes.Key))
        throw new InvalidOperationException(string.Format(Strings.ExUnableToStoreUntypedKeyToStorage, RefOfTType.GetShortName()));
      if (!processedTypes.Add(type))
        throw new InvalidOperationException(string.Format(Strings.ExUnableToPersistTypeXBecauseOfLoopReference, type.FullName));

      var members = type
        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
        .Where(static propertyInfo => propertyInfo.CanRead)
        .Cast<MemberInfo>()
        .Concat(type.GetFields(BindingFlags.Instance | BindingFlags.Public));

      var fields = new Dictionary<MemberInfo, IMappedExpression>();
      foreach (var memberInfo in members) {
        var memberType = memberInfo is PropertyInfo pInfo
          ? pInfo.PropertyType
          : ((FieldInfo) memberInfo).FieldType;

        var typeKind = GetPersistableKind(memberType);
        if (typeKind != PersistableKind.Unknown) {
          var expression = BuildField(memberType, ref columnIndex, ref types, typeKind);
          fields.Add(memberInfo, expression);
        }
        else {
          var collectionExpression = BuildLocalCollectionExpression(memberType, new HashSet<Type>(processedTypes), ref columnIndex, memberInfo, ref types, sourceExpression);
          fields.Add(memberInfo, collectionExpression);
        }
      }
      if (fields.Count == 0)
        throw new InvalidOperationException(string.Format(Strings.ExTypeXDoesNotHasAnyPublicReadablePropertiesOrFieldsSoItCanTBePersistedToStorage, type.FullName));

      return new LocalCollectionExpression(type, parentMember, sourceExpression) { Fields = fields };
    }

    private IMappedExpression BuildField(Type type, ref int index, ref TupleTypeCollection types, PersistableKind typeKind)
    {
      if (typeKind is PersistableKind.RegularField) {
        var columnExpression = ColumnExpression.Create(type, index);
        types.Add(type);
        index++;
        return columnExpression;
      }

      var typeInfo = model.Types[type];
      if (typeKind is PersistableKind.Entity or PersistableKind.PersistentInteface) {
        var keyInfo = typeInfo.Key;
        var keyTupleDescriptor = keyInfo.TupleDescriptor;
        IMappedExpression expression;
        if (isKeyConverter)
          expression = KeyExpression.Create(typeInfo, index);
        else {
          var entityExpression = EntityExpression.Create(typeInfo, index, true);
          entityExpression.IsNullable = true;
          expression = entityExpression;
        }
        index += keyTupleDescriptor.Count;
        types.AddRange(keyTupleDescriptor);
        return expression;
      }

      if (typeKind is PersistableKind.Structure) {
        var tupleDescriptor = typeInfo.TupleDescriptor;
        var tupleSegment = new Segment<int>(index, tupleDescriptor.Count);
        var structureExpression = StructureExpression.CreateLocalCollectionStructure(typeInfo, tupleSegment);
        index += tupleDescriptor.Count;
        types.AddRange(tupleDescriptor);
        return structureExpression;
      }

      throw new NotSupportedException();
    }

    private Func<TItem, Tuple> BuildConverter(Expression sourceExpression, Type itemType)
    {
      var index = 0;
      var tupleTypes = new TupleTypeCollection();
      var typeKind = GetPersistableKind(itemType);
      Expression = typeKind != PersistableKind.Unknown
        ? (Expression) BuildField(itemType, ref index, ref tupleTypes, typeKind)
        : BuildLocalCollectionExpression(itemType, new HashSet<Type>(), ref index, null, ref tupleTypes, sourceExpression);
      TupleDescriptor = TupleDescriptor.Create(tupleTypes.ToArray());

      return delegate (TItem item) {
        var tuple = Tuple.Create(TupleDescriptor);
        if (item is null) {
          return tuple;
        }
        FillLocalCollectionField(item, tuple, Expression);
        return tuple;
      };
    }

    public ItemToTupleConverter(
      Func<ParameterContext, IEnumerable<TItem>> enumerableFunc,
      DomainModel model,
      Expression sourceExpression,
      Type storedEntityType)
    {
      this.model = model;
      this.enumerableFunc = enumerableFunc;
      entityTypeStoredInKey = storedEntityType;
      var itemType = typeof(TItem);
      isKeyConverter = itemType.IsAssignableFrom(WellKnownOrmTypes.Key);
      converter = BuildConverter(sourceExpression,
        isKeyConverter ? entityTypeStoredInKey : itemType);
    }
  }
}
