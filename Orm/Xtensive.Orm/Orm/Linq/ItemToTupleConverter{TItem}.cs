// Copyright (C) 2009-2021 Xtensive LLC.
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

    private static readonly ParameterExpression paramContext = Expression.Parameter(WellKnownOrmTypes.ParameterContext, "context");
    private static readonly MethodInfo selectMethod = WellKnownMembers.Enumerable.Select.MakeGenericMethod(typeof(TItem), WellKnownOrmTypes.Tuple);

    private readonly Func<ParameterContext, IEnumerable<TItem>> enumerableFunc;
    private readonly DomainModel model;
    private Func<TItem, Tuple> converter;
    private readonly Expression sourceExpression;
    private readonly Type entityTypestoredInKey;
    private readonly bool isKeyConverter;

    public override Expression<Func<ParameterContext, IEnumerable<Tuple>>> GetEnumerable()
    {
      var call = Expression.Call(Expression.Constant(enumerableFunc.Target), enumerableFunc.Method, paramContext);
      var select = Expression.Call(selectMethod, call, Expression.Constant(converter));
      return FastExpression.Lambda<Func<ParameterContext, IEnumerable<Tuple>>>(select, paramContext);
    }


    /// <exception cref="InvalidOperationException"><c>InvalidOperationException</c>.</exception>
    private bool IsPersistableType(Type type)
    {
      if (type==WellKnownOrmTypes.Entity
        || type.IsSubclassOf(WellKnownOrmTypes.Entity)
          || type==WellKnownOrmTypes.Structure
            || type.IsSubclassOf(WellKnownOrmTypes.Structure)
        ) {
        if (!model.Types.Contains(type))
          throw new InvalidOperationException(String.Format(Strings.ExTypeNotFoundInModel, type.FullName));
        return true;
      }
      if (type.IsOfGenericType(RefOfTType)) {
        var entityType = type.GetGenericType(RefOfTType).GetGenericArguments()[0];
        if (!model.Types.Contains(entityType))
          throw new InvalidOperationException(String.Format(Strings.ExTypeNotFoundInModel, type.FullName));
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
        type==WellKnownTypes.ByteArray ||
        type==WellKnownTypes.Decimal ||
        type==WellKnownTypes.String ||
        type==WellKnownTypes.DateTime ||
        type==WellKnownTypes.DateTimeOffset ||
        type==WellKnownTypes.Guid ||
        type==WellKnownTypes.TimeSpan;
    }


    private void FillLocalCollectionField(object item, Tuple tuple, Expression expression)
    {
      if (item==null)
        return;
      // LocalCollectionExpression
      if (expression is LocalCollectionExpression) {
        var itemExpression = (LocalCollectionExpression) expression;
        foreach (var field in itemExpression.Fields) {
          var propertyInfo = field.Key as PropertyInfo;
          object value = propertyInfo==null
            ? ((FieldInfo) field.Key).GetValue(item)
            : propertyInfo.GetValue(item, BindingFlags.InvokeMethod, null, null, null);
          if (value!=null)
            FillLocalCollectionField(value, tuple, (Expression) field.Value);
        }
      }
      else if (expression is ColumnExpression) {
        var columnExpression = (ColumnExpression) expression;
        tuple.SetValue(columnExpression.Mapping.Offset, item);
      }
      else if (expression is StructureExpression) {
        var structureExpression = (StructureExpression) expression;
        var structure = (Structure) item;
        var typeInfo = structureExpression.PersistentType;
        var tupleDescriptor = typeInfo.TupleDescriptor;
        var tupleSegment = new Segment<int>(0, tupleDescriptor.Count);
        var structureTuple = structure.Tuple.GetSegment(tupleSegment);
        structureTuple.CopyTo(tuple, 0, structureExpression.Mapping.Offset, structureTuple.Count);
      }
      else if (expression is EntityExpression) {
        var entityExpression = (EntityExpression) expression;
        var entity = (Entity) item;
        var keyTuple = entity.Key.Value;
        keyTuple.CopyTo(tuple, 0, entityExpression.Key.Mapping.Offset, keyTuple.Count);
      }
      else if (expression is KeyExpression) {
        var keyExpression = (KeyExpression) expression;
        var key = (Key) item;
        var keyTuple = key.Value;
        keyTuple.CopyTo(tuple, 0, keyExpression.Mapping.Offset, keyTuple.Count);
      }
      else
        throw new NotSupportedException();
    }

    private LocalCollectionExpression BuildLocalCollectionExpression(Type type, HashSet<Type> processedTypes, ref int columnIndex, MemberInfo parentMember, TupleTypeCollection types)
    {
      if (type.IsAssignableFrom(WellKnownOrmTypes.Key))
        throw new InvalidOperationException(String.Format(Strings.ExUnableToStoreUntypedKeyToStorage, RefOfTType.GetShortName()));
      if (!processedTypes.Add(type))
        throw new InvalidOperationException(String.Format(Strings.ExUnableToPersistTypeXBecauseOfLoopReference, type.FullName));


      IEnumerable<MemberInfo> members = type
        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
        .Where(propertyInfo => propertyInfo.CanRead)
        .Cast<MemberInfo>()
        .Concat(type.GetFields(BindingFlags.Instance | BindingFlags.Public));
      var fields = new Dictionary<MemberInfo, IMappedExpression>();
      foreach (MemberInfo memberInfo in members) {
        var propertyInfo = memberInfo as PropertyInfo;
        Type memberType = propertyInfo==null
          ? ((FieldInfo) memberInfo).FieldType
          : propertyInfo.PropertyType;
        if (IsPersistableType(memberType)) {
          IMappedExpression expression = BuildField(memberType, ref columnIndex, types);
          fields.Add(memberInfo, expression);
        }
        else {
          LocalCollectionExpression collectionExpression = BuildLocalCollectionExpression(memberType, new HashSet<Type>(processedTypes), ref columnIndex, memberInfo, types);
          fields.Add(memberInfo, collectionExpression);
        }
      }
      if (fields.Count==0)
        throw new InvalidOperationException(String.Format(Strings.ExTypeXDoesNotHasAnyPublicReadablePropertiesOrFieldsSoItCanTBePersistedToStorage, type.FullName));
      var result = new LocalCollectionExpression(type, parentMember, sourceExpression);
      result.Fields = fields;
      return result;
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
        TypeInfo typeInfo = model.Types[type];
        KeyInfo keyInfo = typeInfo.Key;
        TupleDescriptor keyTupleDescriptor = keyInfo.TupleDescriptor;
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
        TypeInfo typeInfo = model.Types[type];
        TupleDescriptor tupleDescriptor = typeInfo.TupleDescriptor;
        var tupleSegment = new Segment<int>(index, tupleDescriptor.Count);
        StructureExpression structureExpression = StructureExpression.CreateLocalCollectionStructure(typeInfo, tupleSegment);
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

    private void BuildConverter()
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
        var itemExpression = BuildLocalCollectionExpression(itemType, processedTypes, ref index, null, types);
        TupleDescriptor = TupleDescriptor.Create(types.ToArray(types.Count));
        Expression = itemExpression;
      }

      converter = delegate(TItem item) {
        Tuple tuple = Tuple.Create(TupleDescriptor);
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
      this.sourceExpression = sourceExpression;
      this.entityTypestoredInKey = storedEntityType;
      isKeyConverter = typeof (TItem).IsAssignableFrom(WellKnownOrmTypes.Key);
      BuildConverter();
    }
  }
}
