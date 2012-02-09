// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
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
using Xtensive.Reflection;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Linq.Expressions;
using Xtensive.Orm.Model;

using FieldInfo=System.Reflection.FieldInfo;

namespace Xtensive.Orm.Linq
{
  [Serializable]
  internal sealed class ItemToTupleConverter<TItem> : ItemToTupleConverter
  {
    private readonly Func<IEnumerable<TItem>> enumerableFunc;
    private readonly DomainModel model;
    private Func<TItem, Tuple> converter;
    private readonly Expression sourceExpression;

    public override Expression<Func<IEnumerable<Tuple>>> GetEnumerable()
    {
      var call = Expression.Call(Expression.Constant(enumerableFunc.Target), enumerableFunc.Method);
      MethodInfo selectMethod = WellKnownMembers.Enumerable.Select.MakeGenericMethod(typeof (TItem), typeof (Tuple));
      var select = Expression.Call(selectMethod, call, Expression.Constant(converter));
      return Expression.Lambda<Func<IEnumerable<Tuple>>>(select);
    }


    /// <exception cref="InvalidOperationException"><c>InvalidOperationException</c>.</exception>
    private bool IsPersistableType(Type type)
    {
      if (type==typeof (Entity)
        || type.IsSubclassOf(typeof (Entity))
          || type==typeof (Structure)
            || type.IsSubclassOf(typeof (Structure))
        ) {
        if (!model.Types.Contains(type))
          throw new InvalidOperationException(String.Format(Strings.ExTypeNotFoundInModel, type.FullName));
        return true;
      }
      if (type.IsOfGenericType(typeof (Ref<>))) {
        var entityType = type.GetGenericType(typeof (Ref<>)).GetGenericArguments()[0];
        if (!model.Types.Contains(entityType))
          throw new InvalidOperationException(String.Format(Strings.ExTypeNotFoundInModel, type.FullName));
        return true;
      }
      return TypeIsStorageMappable(type);
    }

    private static bool TypeIsStorageMappable(Type type)
    {
      // TODO: AG: Take info from storage!
      return type.IsPrimitive || 
        type.IsEnum ||
        type==typeof (byte[]) || 
        type==typeof (decimal) || 
        type==typeof (string) || 
        type==typeof (DateTime) ||
        type==typeof(Guid) || 
        type==typeof (TimeSpan) || 
        (type.IsNullable() && TypeIsStorageMappable(type.GetGenericArguments()[0]));
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
      else
        throw new NotSupportedException();
    }

    private LocalCollectionExpression BuildLocalCollectionExpression(Type type, Xtensive.Collections.ISet<Type> processedTypes, ref int columnIndex, MemberInfo parentMember, ref IEnumerable<Type> types)
    {
      if (type.IsAssignableFrom(typeof (Key)))
        throw new InvalidOperationException(String.Format(Strings.ExUnableToStoreUntypedKeyToStorage, typeof (Ref<>).GetShortName()));
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
          IMappedExpression expression = BuildField(memberType, ref columnIndex, ref types);
          fields.Add(memberInfo, expression);
        }
        else {
          LocalCollectionExpression collectionExpression = BuildLocalCollectionExpression(memberType, new Set<Type>(processedTypes), ref columnIndex, memberInfo, ref types);
          fields.Add(memberInfo, collectionExpression);
        }
      }
      if (fields.Count==0)
        throw new InvalidOperationException(String.Format(Strings.ExTypeXDoesNotHasAnyPublicReadablePropertiesOrFieldsSoItCanTBePersistedToStorage, type.FullName));
      var result = new LocalCollectionExpression(type, parentMember, sourceExpression);
      result.Fields = fields;
      return result;
    }


    private IMappedExpression BuildField(Type type, ref int index, ref IEnumerable<Type> types)
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

      if (type.IsSubclassOf(typeof (Entity))) {
        TypeInfo typeInfo = model.Types[type];
        KeyInfo keyInfo = typeInfo.Key;
        TupleDescriptor keyTupleDescriptor = keyInfo.TupleDescriptor;
        EntityExpression entityExpression = EntityExpression.Create(typeInfo, index, true);
        entityExpression.IsNullable = true;
        index += keyTupleDescriptor.Count;
        types = types.Concat(keyTupleDescriptor);
        return entityExpression;
      }

      if (type.IsSubclassOf(typeof (Structure))) {
        TypeInfo typeInfo = model.Types[type];
        TupleDescriptor tupleDescriptor = typeInfo.TupleDescriptor;
        var tupleSegment = new Segment<int>(index, tupleDescriptor.Count);
        StructureExpression structureExpression = StructureExpression.CreateLocalCollectionStructure(typeInfo, tupleSegment);
        index += tupleDescriptor.Count;
        types = types.Concat(tupleDescriptor);
        return structureExpression;
      }

      if (TypeIsStorageMappable(type)) {
        ColumnExpression columnExpression = ColumnExpression.Create(type, index);
        types = types.AddOne(type);
        index++;
        return columnExpression;
      }

      throw new NotSupportedException();
    }

    private void BuildConverter()
    {
      Type itemType = typeof (TItem);
      int index = 0;
      ParameterExpression parameterExpression = Expression.Parameter(itemType, "item");
      IEnumerable<Type> types = EnumerableUtils<Type>.Empty;
      if (IsPersistableType(itemType)) {
        Expression = (Expression) BuildField(itemType, ref index, ref types);
        TupleDescriptor = TupleDescriptor.Create(types);
      }
      else {
        Xtensive.Collections.ISet<Type> processedTypes = new Set<Type>();
        LocalCollectionExpression itemExpression = BuildLocalCollectionExpression(itemType, processedTypes, ref index, null, ref types);
        TupleDescriptor = TupleDescriptor.Create(types);
        Expression = itemExpression;
      }
      Func<TItem, Tuple> converter = delegate(TItem item) {
        RegularTuple tuple = Tuple.Create(TupleDescriptor);
        if (ReferenceEquals(item, null))
          return tuple;
        int offset = 0;
        FillLocalCollectionField(item, tuple, Expression);
        return tuple;
      };
      this.converter = converter;
    }

    public ItemToTupleConverter(Func<IEnumerable<TItem>> enumerableFunc, DomainModel model, Expression sourceExpression)
    {
      this.model = model;
      this.enumerableFunc = enumerableFunc;
      this.sourceExpression = sourceExpression;
      BuildConverter();
    }
  }
}