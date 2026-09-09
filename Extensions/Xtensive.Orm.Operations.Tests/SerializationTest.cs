// Copyright (C) 2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests;
using Xtensive.Orm.Operations.Serialization.Json;
using Xtensive.Orm.Operations.Tests.SerializationTestModel;


namespace Xtensive.Orm.Operations.Tests.SerializationTestModel
{
  [HierarchyRoot]
  public class IntKeyDummy : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public DateTime Value { get; set; }

    [Field]
    public DummyReference Reference { get; set; }

    [Field]
    public EntitySet<DummyEsItem> Collection { get; private set; }

    [Field]
    public DummyStructure DummyStructure { get; set; }

    public IntKeyDummy(Session session)
      : base(session)
    {
    }
  }

  public class DummyStructure : Structure
  {
    [Field]
    public int Index { get; set; }

    [Field]
    public string Value { get; set; }

    public DummyStructure(Session session)
      : base(session)
    {
    }
  }

  [HierarchyRoot]
  public class DummyReference : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    public DummyReference(Session session)
      : base(session)
    {
    }
  }

  [HierarchyRoot]
  public class DummyEsItem : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    [Association(PairTo = nameof(IntKeyDummy.Collection))]
    public IntKeyDummy Dummy { get; set; }

    public DummyEsItem(Session session)
      : base(session)
    {
    }
  }

  [HierarchyRoot]
  [KeyGenerator(KeyGeneratorKind.None)]
  public class Complex2FieldKeyDummy : Entity
  {
    [Field, Key(0)]
    public int Id0 { get; private set; }

    [Field, Key(1)]
    public int Id1 { get; private set; }

    public Complex2FieldKeyDummy(Session session, int id0, int id1)
      : base(session, id0, id1)
    {
    }
  }

  [HierarchyRoot]
  [KeyGenerator(KeyGeneratorKind.None)]
  public class Complex3FieldKeyDummy : Entity
  {
    [Field, Key(0)]
    public int Id0 { get; private set; }

    [Field, Key(1)]
    public int Id1 { get; private set; }

    [Field, Key(2)]
    public int Id2 { get; private set; }

    public Complex3FieldKeyDummy(Session session, int id0, int id1, int id2)
      : base(session, id0, id1, id2)
    {
    }
  }

  [HierarchyRoot]
  [KeyGenerator(KeyGeneratorKind.None)]
  public class Complex5FieldKeyDummy : Entity
  {
    [Field, Key(0)]
    public int Id0 { get; private set; }

    [Field, Key(1)]
    public int Id1 { get; private set; }

    [Field, Key(2)]
    public int Id2 { get; private set; }

    [Field, Key(3)]
    public int Id3 { get; private set; }

    [Field, Key(4)]
    public int Id4 { get; private set; }

    public Complex5FieldKeyDummy(Session session, int id0, int id1, int id2, int id3, int id4)
      : base(session, id0, id1, id2, id3, id4)
    {
    }
  }
}

namespace Xtensive.Orm.Operations.Tests
{
  public class SerializationTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.RegisterCaching(typeof(IntKeyDummy).Assembly, typeof(IntKeyDummy).Namespace);
      configuration.Types.Register(typeof(OperationRegistry));
      configuration.Types.Register(typeof(OperationFactory));
      return configuration;
    }

    [Test]
    public void KeyGenerateOperationTest()
    {
      var jsonSerializerOptions = CreateJsonSettings();

      var key1 = Key.Create(Domain, typeof(IntKeyDummy), 111);
      var key2 = Key.Create(Domain, typeof(Complex2FieldKeyDummy), 111, 222);
      var key3 = Key.Create(Domain, typeof(Complex3FieldKeyDummy), 111, 222, 333);
      var key5 = Key.Create(Domain, typeof(Complex5FieldKeyDummy), 111, 222, 333, 444, 555);

      var operation = new KeyGenerateOperation(key1);
      var clonedOperation = Cloner.CloneViaJsonSerialization(operation, jsonSerializerOptions);
      Assert.That(clonedOperation.Key, Is.EqualTo(operation.Key));
      Assert.That(clonedOperation.Type, Is.EqualTo(operation.Type));
      Assert.That(clonedOperation.IdentifiedEntities, Is.Empty);
      Assert.That(clonedOperation.PrecedingOperations, Is.Empty);
      Assert.That(clonedOperation.FollowingOperations, Is.Empty);
      Assert.That(clonedOperation.UndoOperations, Is.Empty);

      operation = new KeyGenerateOperation(key2);
      clonedOperation = Cloner.CloneViaJsonSerialization(operation, jsonSerializerOptions);
      Assert.That(clonedOperation.Key, Is.EqualTo(operation.Key));
      Assert.That(clonedOperation.Type, Is.EqualTo(operation.Type));
      Assert.That(clonedOperation.IdentifiedEntities, Is.Empty);
      Assert.That(clonedOperation.PrecedingOperations, Is.Empty);
      Assert.That(clonedOperation.FollowingOperations, Is.Empty);
      Assert.That(clonedOperation.UndoOperations, Is.Empty);

      operation = new KeyGenerateOperation(key3);
      clonedOperation = Cloner.CloneViaJsonSerialization(operation, jsonSerializerOptions);
      Assert.That(clonedOperation.Key, Is.EqualTo(operation.Key));
      Assert.That(clonedOperation.Type, Is.EqualTo(operation.Type));
      Assert.That(clonedOperation.IdentifiedEntities, Is.Empty);
      Assert.That(clonedOperation.PrecedingOperations, Is.Empty);
      Assert.That(clonedOperation.FollowingOperations, Is.Empty);
      Assert.That(clonedOperation.UndoOperations, Is.Empty);

      operation = new KeyGenerateOperation(key5);
      clonedOperation = Cloner.CloneViaJsonSerialization(operation, jsonSerializerOptions);
      Assert.That(clonedOperation.Key, Is.EqualTo(operation.Key));
      Assert.That(clonedOperation.Type, Is.EqualTo(operation.Type));
      Assert.That(clonedOperation.IdentifiedEntities, Is.Empty);
      Assert.That(clonedOperation.PrecedingOperations, Is.Empty);
      Assert.That(clonedOperation.FollowingOperations, Is.Empty);
      Assert.That(clonedOperation.UndoOperations, Is.Empty);
    }

    [Test]
    public void EntityCreateOperationTest()
    {
      var jsonSerializerOptions = CreateJsonSettings();

      var key1 = Key.Create(Domain, typeof(IntKeyDummy), 111);
      var key2 = Key.Create(Domain, typeof(Complex2FieldKeyDummy), 111, 222);
      var key3 = Key.Create(Domain, typeof(Complex3FieldKeyDummy), 111, 222, 333);
      var key5 = Key.Create(Domain, typeof(Complex5FieldKeyDummy), 111, 222, 333, 444, 555);

      var operation = new EntityCreateOperation(key1);
      var clonedOperation = Cloner.CloneViaJsonSerialization(operation, jsonSerializerOptions);
      Assert.That(clonedOperation.Key, Is.EqualTo(operation.Key));
      Assert.That(clonedOperation.Type, Is.EqualTo(operation.Type));
      Assert.That(clonedOperation.IdentifiedEntities, Is.Empty);
      Assert.That(clonedOperation.PrecedingOperations, Is.Empty);
      Assert.That(clonedOperation.FollowingOperations, Is.Empty);
      Assert.That(clonedOperation.UndoOperations, Is.Empty);

      operation = new EntityCreateOperation(key2);
      clonedOperation = Cloner.CloneViaJsonSerialization(operation, jsonSerializerOptions);
      Assert.That(clonedOperation.Key, Is.EqualTo(operation.Key));
      Assert.That(clonedOperation.Type, Is.EqualTo(operation.Type));
      Assert.That(clonedOperation.IdentifiedEntities, Is.Empty);
      Assert.That(clonedOperation.PrecedingOperations, Is.Empty);
      Assert.That(clonedOperation.FollowingOperations, Is.Empty);
      Assert.That(clonedOperation.UndoOperations, Is.Empty);

      operation = new EntityCreateOperation(key3);
      clonedOperation = Cloner.CloneViaJsonSerialization(operation, jsonSerializerOptions);
      Assert.That(clonedOperation.Key, Is.EqualTo(operation.Key));
      Assert.That(clonedOperation.Type, Is.EqualTo(operation.Type));
      Assert.That(clonedOperation.IdentifiedEntities, Is.Empty);
      Assert.That(clonedOperation.PrecedingOperations, Is.Empty);
      Assert.That(clonedOperation.FollowingOperations, Is.Empty);
      Assert.That(clonedOperation.UndoOperations, Is.Empty);

      operation = new EntityCreateOperation(key5);
      clonedOperation = Cloner.CloneViaJsonSerialization(operation, jsonSerializerOptions);
      Assert.That(clonedOperation.Key, Is.EqualTo(operation.Key));
      Assert.That(clonedOperation.Type, Is.EqualTo(operation.Type));
      Assert.That(clonedOperation.IdentifiedEntities, Is.Empty);
      Assert.That(clonedOperation.PrecedingOperations, Is.Empty);
      Assert.That(clonedOperation.FollowingOperations, Is.Empty);
      Assert.That(clonedOperation.UndoOperations, Is.Empty);
    }

    [Test]
    public void EntityInitializeOperationTest()
    {
      var jsonSerializerOptions = CreateJsonSettings();

      var key1 = Key.Create(Domain, typeof(IntKeyDummy), 111);
      var key2 = Key.Create(Domain, typeof(Complex2FieldKeyDummy), 111, 222);
      var key3 = Key.Create(Domain, typeof(Complex3FieldKeyDummy), 111, 222, 333);
      var key5 = Key.Create(Domain, typeof(Complex5FieldKeyDummy), 111, 222, 333, 444, 555);

      var operation = new EntityInitializeOperation(key1);
      var clonedOperation = Cloner.CloneViaJsonSerialization(operation, jsonSerializerOptions);
      Assert.That(clonedOperation.Key, Is.EqualTo(operation.Key));
      Assert.That(clonedOperation.Type, Is.EqualTo(operation.Type));
      Assert.That(clonedOperation.IdentifiedEntities, Is.Empty);
      Assert.That(clonedOperation.PrecedingOperations, Is.Empty);
      Assert.That(clonedOperation.FollowingOperations, Is.Empty);
      Assert.That(clonedOperation.UndoOperations, Is.Empty);

      operation = new EntityInitializeOperation(key2);
      clonedOperation = Cloner.CloneViaJsonSerialization(operation, jsonSerializerOptions);
      Assert.That(clonedOperation.Key, Is.EqualTo(operation.Key));
      Assert.That(clonedOperation.Type, Is.EqualTo(operation.Type));
      Assert.That(clonedOperation.IdentifiedEntities, Is.Empty);
      Assert.That(clonedOperation.PrecedingOperations, Is.Empty);
      Assert.That(clonedOperation.FollowingOperations, Is.Empty);
      Assert.That(clonedOperation.UndoOperations, Is.Empty);

      operation = new EntityInitializeOperation(key3);
      clonedOperation = Cloner.CloneViaJsonSerialization(operation, jsonSerializerOptions);
      Assert.That(clonedOperation.Key, Is.EqualTo(operation.Key));
      Assert.That(clonedOperation.Type, Is.EqualTo(operation.Type));
      Assert.That(clonedOperation.IdentifiedEntities, Is.Empty);
      Assert.That(clonedOperation.PrecedingOperations, Is.Empty);
      Assert.That(clonedOperation.FollowingOperations, Is.Empty);
      Assert.That(clonedOperation.UndoOperations, Is.Empty);

      operation = new EntityInitializeOperation(key5);
      clonedOperation = Cloner.CloneViaJsonSerialization(operation, jsonSerializerOptions);
      Assert.That(clonedOperation.Key, Is.EqualTo(operation.Key));
      Assert.That(clonedOperation.Type, Is.EqualTo(operation.Type));
      Assert.That(clonedOperation.IdentifiedEntities, Is.Empty);
      Assert.That(clonedOperation.PrecedingOperations, Is.Empty);
      Assert.That(clonedOperation.FollowingOperations, Is.Empty);
      Assert.That(clonedOperation.UndoOperations, Is.Empty);
    }

    [Test]
    public void EntitesRemoveOperationTest()
    {
      var jsonSerializerOptions = CreateJsonSettings();

      var key1 = Key.Create(Domain, typeof(IntKeyDummy), 111);
      var key2 = Key.Create(Domain, typeof(Complex2FieldKeyDummy), 111, 222);
      var key3 = Key.Create(Domain, typeof(Complex3FieldKeyDummy), 111, 222, 333);
      var key5 = Key.Create(Domain, typeof(Complex5FieldKeyDummy), 111, 222, 333, 444, 555);

      var operation = new EntitiesRemoveOperation(key1);
      var clonedOperation = Cloner.CloneViaJsonSerialization(operation, jsonSerializerOptions);
      Assert.That(clonedOperation.Keys.SequenceEqual(operation.Keys), Is.True);
      Assert.That(clonedOperation.Type, Is.EqualTo(operation.Type));
      Assert.That(clonedOperation.IdentifiedEntities, Is.Empty);
      Assert.That(clonedOperation.PrecedingOperations, Is.Empty);
      Assert.That(clonedOperation.FollowingOperations, Is.Empty);
      Assert.That(clonedOperation.UndoOperations, Is.Empty);

      operation = new EntitiesRemoveOperation(key2);
      clonedOperation = Cloner.CloneViaJsonSerialization(operation, jsonSerializerOptions);
      Assert.That(clonedOperation.Keys.SequenceEqual(operation.Keys), Is.True);
      Assert.That(clonedOperation.Type, Is.EqualTo(operation.Type));
      Assert.That(clonedOperation.IdentifiedEntities, Is.Empty);
      Assert.That(clonedOperation.PrecedingOperations, Is.Empty);
      Assert.That(clonedOperation.FollowingOperations, Is.Empty);
      Assert.That(clonedOperation.UndoOperations, Is.Empty);

      operation = new EntitiesRemoveOperation(key3);
      clonedOperation = Cloner.CloneViaJsonSerialization(operation, jsonSerializerOptions);
      Assert.That(clonedOperation.Keys.SequenceEqual(operation.Keys), Is.True);
      Assert.That(clonedOperation.Type, Is.EqualTo(operation.Type));
      Assert.That(clonedOperation.IdentifiedEntities, Is.Empty);
      Assert.That(clonedOperation.PrecedingOperations, Is.Empty);
      Assert.That(clonedOperation.FollowingOperations, Is.Empty);
      Assert.That(clonedOperation.UndoOperations, Is.Empty);

      operation = new EntitiesRemoveOperation(key5);
      clonedOperation = Cloner.CloneViaJsonSerialization(operation, jsonSerializerOptions);
      Assert.That(clonedOperation.Keys.SequenceEqual(operation.Keys), Is.True);
      Assert.That(clonedOperation.Type, Is.EqualTo(operation.Type));
      Assert.That(clonedOperation.IdentifiedEntities, Is.Empty);
      Assert.That(clonedOperation.PrecedingOperations, Is.Empty);
      Assert.That(clonedOperation.FollowingOperations, Is.Empty);
      Assert.That(clonedOperation.UndoOperations, Is.Empty);

      operation = new EntitiesRemoveOperation(new List<Key> { key5, key3, key2, key1 }.AsReadOnly());
      clonedOperation = Cloner.CloneViaJsonSerialization(operation, jsonSerializerOptions);
      Assert.That(clonedOperation.Keys.SequenceEqual(operation.Keys), Is.True);
      Assert.That(clonedOperation.Type, Is.EqualTo(operation.Type));
      Assert.That(clonedOperation.IdentifiedEntities, Is.Empty);
      Assert.That(clonedOperation.PrecedingOperations, Is.Empty);
      Assert.That(clonedOperation.FollowingOperations, Is.Empty);
      Assert.That(clonedOperation.UndoOperations, Is.Empty);
    }

    [Test]
    public void EntityFieldSetOperationTest()
    {
      var jsonSerializerOptions = CreateJsonSettings();

      var dummyType = typeof(IntKeyDummy);
      var dummyTypeInfo = Domain.Model.Types[dummyType];

      var key = Key.Create(Domain, typeof(IntKeyDummy), 111);
      var primitiveField = dummyTypeInfo.Fields[nameof(IntKeyDummy.Value)];
      var referenceField = dummyTypeInfo.Fields[nameof(IntKeyDummy.Reference)];
      var structureField = dummyTypeInfo.Fields[nameof(IntKeyDummy.DummyStructure)];

      var primitiveFieldOperation = new EntityFieldSetOperation(key, primitiveField, new DateTime(2012, 08, 12));
      var cPrimitiveFieldOperation = Cloner.CloneViaJsonSerialization(primitiveFieldOperation, jsonSerializerOptions);
      Assert.That(cPrimitiveFieldOperation.Key, Is.EqualTo(primitiveFieldOperation.Key));
      Assert.That(cPrimitiveFieldOperation.Field, Is.EqualTo(primitiveFieldOperation.Field));
      Assert.That(cPrimitiveFieldOperation.IsStructure, Is.EqualTo(primitiveFieldOperation.IsStructure));
      Assert.That(cPrimitiveFieldOperation.IsReference, Is.EqualTo(primitiveFieldOperation.IsReference));
      Assert.That(cPrimitiveFieldOperation.ValueKey, Is.Null);
      Assert.That(cPrimitiveFieldOperation.Value, Is.Not.Null);
      Assert.That(cPrimitiveFieldOperation.Value, Is.EqualTo(primitiveFieldOperation.Value));
      Assert.That(cPrimitiveFieldOperation.NestedOperations, Is.Empty);
      Assert.That(cPrimitiveFieldOperation.Type, Is.EqualTo(primitiveFieldOperation.Type));
      Assert.That(cPrimitiveFieldOperation.IdentifiedEntities, Is.Empty);
      Assert.That(cPrimitiveFieldOperation.PrecedingOperations, Is.Empty);
      Assert.That(cPrimitiveFieldOperation.FollowingOperations, Is.Empty);
      Assert.That(cPrimitiveFieldOperation.UndoOperations, Is.Empty);


      var referenceKey = Key.Create(Domain, typeof(DummyReference), 321);
      var referenceFieldOperation = new EntityFieldSetOperation(key, referenceField, referenceKey);
      var cReferenceFieldOperation = Cloner.CloneViaJsonSerialization(referenceFieldOperation, jsonSerializerOptions);
      Assert.That(cReferenceFieldOperation.Key, Is.EqualTo(referenceFieldOperation.Key));
      Assert.That(cReferenceFieldOperation.Field, Is.EqualTo(referenceFieldOperation.Field));
      Assert.That(cReferenceFieldOperation.IsStructure, Is.EqualTo(referenceFieldOperation.IsStructure));
      Assert.That(cReferenceFieldOperation.IsReference, Is.EqualTo(referenceFieldOperation.IsReference));
      Assert.That(cReferenceFieldOperation.ValueKey, Is.Not.Null);
      Assert.That(cReferenceFieldOperation.ValueKey, Is.EqualTo(referenceFieldOperation.ValueKey));
      Assert.That(cReferenceFieldOperation.Value, Is.Null);
      Assert.That(cReferenceFieldOperation.NestedOperations, Is.Empty);
      Assert.That(cReferenceFieldOperation.Type, Is.EqualTo(referenceFieldOperation.Type));
      Assert.That(cReferenceFieldOperation.IdentifiedEntities, Is.Empty);
      Assert.That(cReferenceFieldOperation.PrecedingOperations, Is.Empty);
      Assert.That(cReferenceFieldOperation.FollowingOperations, Is.Empty);
      Assert.That(cReferenceFieldOperation.UndoOperations, Is.Empty);


      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        var dummyStructure = new DummyStructure(session) {Index = 2, Value = "abc" };

        var structureFieldSetOperation = new EntityFieldSetOperation(key, structureField, dummyStructure);
        var cStructureFieldSetOperation = Cloner.CloneViaJsonSerialization(structureFieldSetOperation, jsonSerializerOptions);

        Assert.That(cStructureFieldSetOperation.Key, Is.EqualTo(structureFieldSetOperation.Key));
        Assert.That(cStructureFieldSetOperation.Field, Is.EqualTo(structureFieldSetOperation.Field));
        Assert.That(cStructureFieldSetOperation.IsStructure, Is.EqualTo(structureFieldSetOperation.IsStructure));
        Assert.That(cStructureFieldSetOperation.IsReference, Is.EqualTo(structureFieldSetOperation.IsReference));
        Assert.That(cStructureFieldSetOperation.ValueKey, Is.Null);
        Assert.That(cStructureFieldSetOperation.Value, Is.Null);
        Assert.That(cStructureFieldSetOperation.NestedOperations, Is.Not.Empty);
        Assert.That(cStructureFieldSetOperation.NestedOperations.Count, Is.EqualTo(2));

        var nestedOp1 = (EntityFieldSetOperation)cStructureFieldSetOperation.NestedOperations[0];
        Assert.That(nestedOp1, Is.Not.Null);
        Assert.That(nestedOp1.Key, Is.EqualTo(structureFieldSetOperation.Key));
        Assert.That(nestedOp1.Field, Is.EqualTo(dummyTypeInfo.Fields[nameof(DummyStructure) + "." + nameof(DummyStructure.Index)]));
        Assert.That(nestedOp1.IsStructure, Is.False);
        Assert.That(nestedOp1.IsReference, Is.False);
        Assert.That(nestedOp1.ValueKey, Is.Null);
        Assert.That(nestedOp1.Value, Is.Not.Null);
        Assert.That(nestedOp1.Value, Is.EqualTo(2));
        Assert.That(nestedOp1.NestedOperations, Is.Empty);
        Assert.That(nestedOp1.IdentifiedEntities, Is.Empty);
        Assert.That(nestedOp1.PrecedingOperations, Is.Empty);
        Assert.That(nestedOp1.FollowingOperations, Is.Empty);
        Assert.That(nestedOp1.UndoOperations, Is.Empty);

        var nestedOp2 = (EntityFieldSetOperation) cStructureFieldSetOperation.NestedOperations[1];
        Assert.That(nestedOp2, Is.Not.Null);
        Assert.That(nestedOp2.Key, Is.EqualTo(structureFieldSetOperation.Key));
        Assert.That(nestedOp2.Field, Is.EqualTo(dummyTypeInfo.Fields[nameof(DummyStructure) + "." + nameof(DummyStructure.Value)]));
        Assert.That(nestedOp2.IsStructure, Is.False);
        Assert.That(nestedOp2.IsReference, Is.False);
        Assert.That(nestedOp2.ValueKey, Is.Null);
        Assert.That(nestedOp2.Value, Is.Not.Null);
        Assert.That(nestedOp2.Value, Is.EqualTo("abc"));
        Assert.That(nestedOp2.NestedOperations, Is.Empty);
        Assert.That(nestedOp2.IdentifiedEntities, Is.Empty);
        Assert.That(nestedOp2.PrecedingOperations, Is.Empty);
        Assert.That(nestedOp2.FollowingOperations, Is.Empty);
        Assert.That(nestedOp2.UndoOperations, Is.Empty);

        Assert.That(cStructureFieldSetOperation.Type, Is.EqualTo(structureFieldSetOperation.Type));
        Assert.That(cStructureFieldSetOperation.IdentifiedEntities, Is.Empty);
        Assert.That(cStructureFieldSetOperation.PrecedingOperations, Is.Empty);
        Assert.That(cStructureFieldSetOperation.FollowingOperations, Is.Empty);
        Assert.That(cStructureFieldSetOperation.UndoOperations, Is.Empty);
      }
    }

    [Test]
    public void EntitySetClearOperationTest()
    {
      var jsonSerializerOptions = CreateJsonSettings();

      var dummyType = typeof(IntKeyDummy);
      var dummyTypeInfo = Domain.Model.Types[dummyType];

      var key = Key.Create(Domain, typeof(IntKeyDummy), 111);
      var entitySetField = dummyTypeInfo.Fields[nameof(IntKeyDummy.Collection)];

      var operation = new EntitySetClearOperation(key, entitySetField);
      var clonedOperation = Cloner.CloneViaJsonSerialization(operation, jsonSerializerOptions);
      Assert.That(clonedOperation.Key, Is.EqualTo(operation.Key));
      Assert.That(clonedOperation.Field, Is.EqualTo(operation.Field));
      Assert.That(clonedOperation.Type, Is.EqualTo(operation.Type));
      Assert.That(clonedOperation.IdentifiedEntities, Is.Empty);
      Assert.That(clonedOperation.PrecedingOperations, Is.Empty);
      Assert.That(clonedOperation.FollowingOperations, Is.Empty);
      Assert.That(clonedOperation.UndoOperations, Is.Empty);
    }

    [Test]
    public void EntitySetItemAddOperationTest()
    {
      var jsonSerializerOptions = CreateJsonSettings();

      var dummyType = typeof(IntKeyDummy);
      var dummyTypeInfo = Domain.Model.Types[dummyType];

      var key = Key.Create(Domain, typeof(IntKeyDummy), 111);
      var entitySetField = dummyTypeInfo.Fields[nameof(IntKeyDummy.Collection)];
      var itemKey = Key.Create(Domain, typeof(DummyEsItem), 987);

      var operation = new EntitySetItemAddOperation(key, entitySetField, itemKey);
      var clonedOperation = Cloner.CloneViaJsonSerialization(operation, jsonSerializerOptions);
      Assert.That(clonedOperation.Key, Is.EqualTo(operation.Key));
      Assert.That(clonedOperation.Field, Is.EqualTo(operation.Field));
      Assert.That(clonedOperation.ItemKey, Is.EqualTo(operation.ItemKey));
      Assert.That(clonedOperation.Type, Is.EqualTo(operation.Type));
      Assert.That(clonedOperation.IdentifiedEntities, Is.Empty);
      Assert.That(clonedOperation.PrecedingOperations, Is.Empty);
      Assert.That(clonedOperation.FollowingOperations, Is.Empty);
      Assert.That(clonedOperation.UndoOperations, Is.Empty);
    }

    [Test]
    public void EntitySetItemRemoveOperationTest()
    {
      var jsonSerializerOptions = CreateJsonSettings();

      var dummyType = typeof(IntKeyDummy);
      var dummyTypeInfo = Domain.Model.Types[dummyType];

      var key = Key.Create(Domain, typeof(IntKeyDummy), 111);
      var entitySetField = dummyTypeInfo.Fields[nameof(IntKeyDummy.Collection)];
      var itemKey = Key.Create(Domain, typeof(DummyEsItem), 987);

      var operation = new EntitySetItemRemoveOperation(key, entitySetField, itemKey);
      var clonedOperation = Cloner.CloneViaJsonSerialization(operation, jsonSerializerOptions);
      Assert.That(clonedOperation.Key, Is.EqualTo(operation.Key));
      Assert.That(clonedOperation.Field, Is.EqualTo(operation.Field));
      Assert.That(clonedOperation.ItemKey, Is.EqualTo(operation.ItemKey));
      Assert.That(clonedOperation.Type, Is.EqualTo(operation.Type));
      Assert.That(clonedOperation.IdentifiedEntities, Is.Empty);
      Assert.That(clonedOperation.PrecedingOperations, Is.Empty);
      Assert.That(clonedOperation.FollowingOperations, Is.Empty);
      Assert.That(clonedOperation.UndoOperations, Is.Empty);
    }

    [Test]
    public void ValidateVersionOperationTest()
    {
      var jsonSerializerOptions = CreateJsonSettings();

      var key = Key.Create(Domain, typeof(IntKeyDummy), 111);
      var versionTuple = Xtensive.Tuples.Tuple.Create<long>(222);
      var versionInfo = new VersionInfo(versionTuple);

      var operation = new ValidateVersionOperation(key, versionInfo);

      var clonedOperation = Cloner.CloneViaJsonSerialization(operation, jsonSerializerOptions);
      Assert.That(clonedOperation.Key, Is.EqualTo(key));
      Assert.That(clonedOperation.Version, Is.EqualTo(versionInfo));
    }

    private JsonSerializerOptions CreateJsonSettings()
    {
      var jsonSerializerOptions = new JsonSerializerOptions() {
        WriteIndented = true,
      };
      _ = jsonSerializerOptions.AddOperationsConverters(Domain);
      return jsonSerializerOptions;
    }
  }
}
