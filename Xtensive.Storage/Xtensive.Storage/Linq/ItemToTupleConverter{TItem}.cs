// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.10.01

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Reflection;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Linq.Expressions;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;
using FieldInfo=System.Reflection.FieldInfo;

namespace Xtensive.Storage.Linq
{
  [Serializable]
  internal sealed class ItemToTupleConverter<TItem> : ItemToTupleConverter
  {
    private readonly IEnumerable<Tuple> enumerable;
    private readonly DomainModel model;

    public ItemToTupleConverter(IEnumerable<TItem> values, DomainModel model)
    {
      this.model = model;
      Type itemType = typeof (TItem);
      int index = 0;
      ParameterExpression parameterExpression = Expression.Parameter(itemType, "item");
      IEnumerable<Type> types = EnumerableUtils<Type>.Empty;
      if (IsPersistableType(itemType)) {
        Expression = (Expression) BuildField(itemType, ref index, ref types);
        TupleDescriptor = TupleDescriptor.Create(types);
      }
      else {
        ISet<Type> processedTypes = new Set<Type>();
        LocalCollectionExpression itemExpression = BuildLocalCollectionExpression(itemType, processedTypes, ref index, null, ref types);
        TupleDescriptor = TupleDescriptor.Create(types);
        Expression = itemExpression;
      }
      Func<TItem, int, Tuple> converter = delegate(TItem item, int number) {
        RegularTuple tuple = Tuple.Create(TupleDescriptor);
        if (ReferenceEquals(item, null))
          return tuple;
        int offset = 0;
        FillLocalCollectionField(item, tuple, Expression);
        return tuple;
      };
      enumerable = values.Select(converter);
    }

    public override IEnumerator<Tuple> GetEnumerator()
    {
      return enumerable.GetEnumerator();
    }

    private bool IsPersistableType(Type type)
    {
      if (type==typeof (Entity)
        || type.IsSubclassOf(typeof (Entity))
          || type==typeof (Structure)
            || type.IsSubclassOf(typeof (Structure))) {
        if (!model.Types.Contains(type))
          throw new InvalidOperationException(String.Format(Strings.ExTypeNotFoundInModel, type.FullName));
        return true;
      }
      return TypeIsStorageMappable(type);
    }

    private static bool TypeIsStorageMappable(Type type)
    {
      // TODO: AG: Take info from storage!
      return type.IsPrimitive
        || type==typeof (byte[])
          || type==typeof (decimal)
            || type==typeof (string)
              || type==typeof (DateTime)
                || type==typeof (TimeSpan)
                  || (type.IsNullable() && TypeIsStorageMappable(type.GetGenericArguments()[0]));
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
            FillLocalCollectionField(value, tuple, (Expression)field.Value);
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
        structureTuple.CopyTo(tuple, structureExpression.Mapping.Offset);
      }
      else if (expression is EntityExpression) {
        var entityExpression = (EntityExpression) expression;
        var entity = (Entity) item;
        var keyTuple = entity.Key.Value;
        keyTuple.CopyTo(tuple, entityExpression.Key.Mapping.Offset);
      }
      else
        throw new NotSupportedException();
    }

//
//
//    private ProjectionExpression VisitLocalCollectionSequence<TItem>(Expression expression)
//    {
//      var source = (IEnumerable<TItem>) context.Evaluator.Evaluate(expression).Value;
//      var itemType = typeof (TItem);
//      var type = itemType.IsNullable() ? itemType.GetGenericArguments()[0] : itemType;
//
//      if (type==typeof (Entity) || type.IsSubclassOf(typeof (Entity))) {
//        var typeInfo = context.Model.Types[type];
//        var keyInfo = typeInfo.KeyInfo;
//        var keyTupleDescriptor = keyInfo.TupleDescriptor;
//        var columns = keyInfo.Columns.Select((columnInfo, i) => new SystemColumn(context.GetNextColumnAlias(), i, columnInfo.ValueType)).Cast<Column>();
//        var rsHeader = new RecordSetHeader(keyTupleDescriptor, columns);
//        var rawProvider = new RawProvider(rsHeader, source.Select(e => ReferenceEquals(e, null) ? Tuple.Create(keyTupleDescriptor) : ((Entity) (object) e).Key.Value));
//        var recordset = new StoreProvider(rawProvider).Result;
//        var entityExpression = EntityExpression.Create(typeInfo, 0, true);
//        var itemProjector = new ItemProjectorExpression(entityExpression, recordset, context);
//        if (state.JoinLocalCollectionEntity)
//          EnsureEntityFieldsAreJoined(entityExpression, itemProjector);
//        return new ProjectionExpression(itemType, itemProjector, new Dictionary<Parameter<Tuple>, Tuple>());
//      }
//
//      if (type==typeof (Structure) || type.IsSubclassOf(typeof (Structure))) {
//        var typeInfo = context.Model.Types[type];
//        var tupleDescriptor = typeInfo.TupleDescriptor;
//        var columns = tupleDescriptor.Select((columnType, i) => new SystemColumn(context.GetNextColumnAlias(), i, columnType)).Cast<Column>();
//        var rsHeader = new RecordSetHeader(tupleDescriptor, columns);
//        var tupleSegment = new Segment<int>(0, tupleDescriptor.Count);
//        var rawProvider = new RawProvider(rsHeader, source.Select(structure => ReferenceEquals(structure, null) ? typeInfo.TuplePrototype : ((Structure) (object) structure).Tuple.GetSegment(tupleSegment)));
//        var recordset = new StoreProvider(rawProvider).Result;
//        var structureExpression = StructureExpression.CreateLocalCollectionStructure(typeInfo, tupleSegment);
//        var itemProjector = new ItemProjectorExpression(structureExpression, recordset, context);
//        return new ProjectionExpression(itemType, itemProjector, new Dictionary<Parameter<Tuple>, Tuple>());
//      }
//
//      if (TypeIsStorageMappable(type)) {
//        var rsHeader = new RecordSetHeader(TupleDescriptor.Create(new[] {type}), new[] {new SystemColumn(context.GetNextColumnAlias(), 0, type)});
//        var rawProvider = new RawProvider(rsHeader, source.Select(t => (Tuple) Tuple.Create(t)));
//        var recordset = new StoreProvider(rawProvider).Result;
//        var column = ColumnExpression.Create(itemType, 0);
//        var itemProjector = new ItemProjectorExpression(column, recordset, context);
//        return new ProjectionExpression(itemType, itemProjector, new Dictionary<Parameter<Tuple>, Tuple>());
//      }
//      else {
//        ISet<Type> processedTypes = new SetSlim<Type>();
//        LocalCollectionExpression itemExpression = BuildLocalCollectionExpression(itemType, processedTypes, 0, null);
//
//        var tupleDescriptor = TupleDescriptor.Create(itemExpression.Columns.Select(columnExpression => columnExpression.Type));
//
//        Func<TItem, int, Tuple> converter = delegate(TItem item, int number) {
//          var tuple = Tuple.Create(tupleDescriptor);
//          if (ReferenceEquals(item, null))
//            return tuple;
//          FillLocalCollectionField(item, tuple, itemExpression);
//          return tuple;
//        };
//
//        var rsHeader = new RecordSetHeader(tupleDescriptor, tupleDescriptor.Select(x => new SystemColumn(context.GetNextColumnAlias(), 0, x)).Cast<Column>());
//        var rawProvider = new RawProvider(rsHeader, source.Select(converter));
//        var recordset = new StoreProvider(rawProvider).Result;
//        var itemProjector = new ItemProjectorExpression(itemExpression, recordset, context);
//        return new ProjectionExpression(itemType, itemProjector, new Dictionary<Parameter<Tuple>, Tuple>());
//      }
//    }
//
//
//
//        if (IsPersistableType(itemType)) {
//          throw new NotImplementedException();
//        }
//        else {
//          var localCollectionExpression = BuildLocalCollectionExpression(itemType, new Set<Type>(), 0, null);
//          if (localCollectionExpression.Fields.Count==0)
//            throw new InvalidOperationException(String.Format(Strings.ExTypeXDoesNotHasAnyPublicReadablePropertiesOrFieldsSoItCanTBePersistedToStorage, itemType.FullName));
//        var tupleDescriptor = TupleDescriptor.Create(localCollectionExpression.Columns.Select(columnExpression => columnExpression.Type));
//
//        Func<TItem, int, Tuple> converter = delegate(TItem item, int number) {
//          var tuple = Tuple.Create(tupleDescriptor);
//          if (ReferenceEquals(item, null))
//            return tuple;
//          FillLocalCollectionField(item, tuple, itemExpression);
//          return tuple;
//        };
//
    private LocalCollectionExpression BuildLocalCollectionExpression(Type type, ISet<Type> processedTypes, ref int columnIndex, MemberInfo parentMember, ref IEnumerable<Type> types)
    {
      if (!processedTypes.Add(type))
        throw new InvalidOperationException(String.Format("Unable to persist type '{0}' to storage because of loop reference.", type.FullName));


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
      var result = new LocalCollectionExpression(type, parentMember);
      result.Fields = fields;
      return result;
    }


    private IMappedExpression BuildField(Type type, ref int index, ref IEnumerable<Type> types)
    {
      if (type.IsSubclassOf(typeof (Entity))) {
        TypeInfo typeInfo = model.Types[type];
        KeyInfo keyInfo = typeInfo.KeyInfo;
        TupleDescriptor keyTupleDescriptor = keyInfo.TupleDescriptor;
        EntityExpression entityExpression = EntityExpression.Create(typeInfo, index, true);
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
  }
}