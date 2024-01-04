// Copyright (C) 2009-2022 Xtensive LLC.
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
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Orm.Internals;
using Xtensive.Reflection;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Linq.Expressions;
using Xtensive.Orm.Model;

using FieldInfo=System.Reflection.FieldInfo;
using TypeInfo = Xtensive.Orm.Model.TypeInfo;

namespace Xtensive.Orm.Linq
{
  [Serializable]
  internal sealed class ItemToTupleConverter<TItem> : ItemToTupleConverter
  {
    private class TupleTypeCollection: IReadOnlyCollection<Type>
    {
      private IEnumerable<Type> types;
      private int count;

      public int Count => count;

      public IEnumerator<Type> GetEnumerator() => (types ?? Enumerable.Empty<Type>()).GetEnumerator();

      IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

      public void Add(Type type)
      {
        count++;
        types = types==null ? EnumerableUtils.One(type) : types.Concat(EnumerableUtils.One(type));
      }

      public void AddRange(IReadOnlyCollection<Type> newTypes)
      {
        count += newTypes.Count;
        types = types == null ? newTypes : types.Concat(newTypes);
      }
    }

    private static readonly ParameterExpression ParamContext = Expression.Parameter(WellKnownOrmTypes.ParameterContext, "context");
    private static readonly MethodInfo SelectMethod = WellKnownMembers.Enumerable.Select.MakeGenericMethod(typeof(TItem), WellKnownOrmTypes.Tuple);

    private readonly Func<ParameterContext, IEnumerable<TItem>> enumerableFunc;
    private readonly DomainModel model;
    private readonly Type entityTypestoredInKey;
    private readonly bool isKeyConverter;

    private Func<TItem, Tuple> converter;
    

    public override Expression<Func<ParameterContext, IEnumerable<Tuple>>> GetEnumerable()
    {
      var call = Expression.Call(Expression.Constant(enumerableFunc.Target), enumerableFunc.Method, ParamContext);
      var select = Expression.Call(SelectMethod, call, Expression.Constant(converter));
      return FastExpression.Lambda<Func<ParameterContext, IEnumerable<Tuple>>>(select, ParamContext);
    }


    /// <exception cref="InvalidOperationException"><c>InvalidOperationException</c>.</exception>
    private bool IsPersistableType(Type type)
    {
      if (type==WellKnownOrmTypes.Entity
        || type.IsSubclassOf(WellKnownOrmTypes.Entity)
          || type==WellKnownOrmTypes.Structure
            || type.IsSubclassOf(WellKnownOrmTypes.Structure)) {
        if (!model.Types.Contains(type))
          throw new InvalidOperationException(string.Format(Strings.ExTypeNotFoundInModel, type.FullName));
        return true;
      }
      if (type.IsOfGenericType(RefOfTType)) {
        var entityType = type.GetGenericType(RefOfTType).GetGenericArguments()[0];
        if (!model.Types.Contains(entityType))
          throw new InvalidOperationException(string.Format(Strings.ExTypeNotFoundInModel, type.FullName));
        return true;
      }
      return TypeIsStorageMappable(type);
    }

    private static bool TypeIsStorageMappable(Type type)
    {
      // TODO: AG: Take info from storage!
      type = type.StripNullable();
      return type.IsPrimitive ||
        type.IsEnum ||
        type == WellKnownTypes.ByteArray ||
        type == WellKnownTypes.Decimal ||
        type == WellKnownTypes.String ||
        type == WellKnownTypes.DateTime ||
        type == WellKnownTypes.DateTimeOffset ||
        type == WellKnownTypes.DateOnly ||
        type == WellKnownTypes.TimeOnly ||
        type == WellKnownTypes.Guid ||
        type == WellKnownTypes.TimeSpan;
    }


    private static void FillLocalCollectionField(object item, Tuple tuple, Expression expression)
    {
      if (item==null)
        return;
      // LocalCollectionExpression
      switch (expression) {
        case LocalCollectionExpression itemExpression:
          foreach (var field in itemExpression.Fields) {
            object value;
            if (field.Key is PropertyInfo propertyInfo) {
              value = propertyInfo.GetValue(item, BindingFlags.InvokeMethod, null, null, null);
            }
            else {
              value = ((FieldInfo) field.Key).GetValue(item);
            }
            if (value != null)
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
        }
        break;
        case KeyExpression keyExpression: {
          var key = (Key) item;
          var keyTuple = key.Value;
          keyTuple.CopyTo(tuple, 0, keyExpression.Mapping.Offset, keyTuple.Count);
        }
        break;
        default:
          throw new NotSupportedException();
      }
    }

    private LocalCollectionExpression BuildLocalCollectionExpression(Type type,
      HashSet<Type> processedTypes, ref int columnIndex, MemberInfo parentMember, TupleTypeCollection types, Expression sourceExpression)
    {
      if (type.IsAssignableFrom(WellKnownOrmTypes.Key))
        throw new InvalidOperationException(string.Format(Strings.ExUnableToStoreUntypedKeyToStorage, RefOfTType.GetShortName()));
      if (!processedTypes.Add(type))
        throw new InvalidOperationException(string.Format(Strings.ExUnableToPersistTypeXBecauseOfLoopReference, type.FullName));


      var members = type
        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
        .Where(propertyInfo => propertyInfo.CanRead)
        .Cast<MemberInfo>()
        .Concat(type.GetFields(BindingFlags.Instance | BindingFlags.Public));
      var fields = new Dictionary<MemberInfo, IMappedExpression>();
      foreach (var memberInfo in members) {
        var propertyInfo = memberInfo as PropertyInfo;
        var memberType = propertyInfo==null
          ? ((FieldInfo) memberInfo).FieldType
          : propertyInfo.PropertyType;
        if (IsPersistableType(memberType)) {
          var expression = BuildField(memberType, ref columnIndex, types);
          fields.Add(memberInfo, expression);
        }
        else {
          var collectionExpression = BuildLocalCollectionExpression(memberType, new HashSet<Type>(processedTypes), ref columnIndex, memberInfo, types, sourceExpression);
          fields.Add(memberInfo, collectionExpression);
        }
      }
      if (fields.Count==0)
        throw new InvalidOperationException(string.Format(Strings.ExTypeXDoesNotHasAnyPublicReadablePropertiesOrFieldsSoItCanTBePersistedToStorage, type.FullName));

      return new LocalCollectionExpression(type, parentMember, sourceExpression) { Fields = fields };
    }


    private IMappedExpression BuildField(Type type, ref int index, TupleTypeCollection types)
    {
//      if (type.IsOfGenericType(typeof (Ref<>))) {
//        var entityType = type.GetGenericType(typeof (Ref<>)).GetGenericArguments()[0];
//        TypeInfo typeInfo = model.Types[entityType];
//        KeyInfo keyProviderInfo = typeInfo.KeyInfo;
//        TupleDescriptor keyTupleDescriptor = keyProviderInfo.TupleDescriptor;
//        KeyExpression entityExpression = KeyExpression.Create(typeInfo, index);
//        index += keyTupleDescriptor.Count;
//        types = types.Concat(keyTupleDescriptor);
//        return Expression.Convert(entityExpression, type);
//      }

      if (type.IsSubclassOf(WellKnownOrmTypes.Entity)) {
        var typeInfo = model.Types[type];
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

      if (type.IsSubclassOf(WellKnownOrmTypes.Structure)) {
        var typeInfo = model.Types[type];
        var tupleDescriptor = typeInfo.TupleDescriptor;
        var tupleSegment = new Segment<int>(index, tupleDescriptor.Count);
        var structureExpression = StructureExpression.CreateLocalCollectionStructure(typeInfo, tupleSegment);
        index += tupleDescriptor.Count;
        types.AddRange(tupleDescriptor);
        return structureExpression;
      }

      if (TypeIsStorageMappable(type)) {
        ColumnExpression columnExpression = ColumnExpression.Create(type, index);
        types.Add(type);
        index++;
        return columnExpression;
      }

      throw new NotSupportedException();
    }

    private void BuildConverter(Expression sourceExpression)
    {
      var itemType = isKeyConverter ? entityTypestoredInKey : typeof (TItem);
      var index = 0;
      var types = new TupleTypeCollection();
      if (IsPersistableType(itemType)) {
        Expression = (Expression) BuildField(itemType, ref index, types);
        TupleDescriptor = TupleDescriptor.Create(types.ToArray(types.Count));
      }
      else {
        var processedTypes = new HashSet<Type>();
        var itemExpression = BuildLocalCollectionExpression(itemType, processedTypes, ref index, null, types, sourceExpression);
        TupleDescriptor = TupleDescriptor.Create(types.ToArray(types.Count));
        Expression = itemExpression;
      }

      converter = delegate(TItem item) {
        var tuple = Tuple.Create(TupleDescriptor);
        if (ReferenceEquals(item, null)) {
          return tuple;
        }
        FillLocalCollectionField(item, tuple, Expression);
        return tuple;
      };
    }

    public ItemToTupleConverter(Func<ParameterContext, IEnumerable<TItem>> enumerableFunc, DomainModel model, Expression sourceExpression, Type storedEntityType)
    {
      this.model = model;
      this.enumerableFunc = enumerableFunc;
      entityTypestoredInKey = storedEntityType;
      isKeyConverter = typeof(TItem).IsAssignableFrom(WellKnownOrmTypes.Key);
      BuildConverter(sourceExpression);
    }
  }
}
