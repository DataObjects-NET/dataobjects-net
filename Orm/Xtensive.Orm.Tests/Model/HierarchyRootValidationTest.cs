// Copyright (C) 2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.IoC;
using Xtensive.Orm.Building;
using Xtensive.Orm.Building.Builders;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Tuples;
using Models = Xtensive.Orm.Tests.Model.HierarchyRootValidationTestModel;

namespace Xtensive.Orm.Tests.Model
{
  [TestFixture]
  public sealed class HierarchyRootValidationTest
  {
    private const string CantBeOfMsg = "can't be of";
    private const string UnableToCreateKeyForMsg = "Unable to create key for";
    private const string UnsupportedTypeMsg = "Unsupported type";
    private const string KeyGeneratorCanServeHierarchyWithOneKeyFieldMsg = "generator can serve hierarchy with exactly one key field";

    [Test]
    public void ValidNonPersistentKeyTypesForDefaultGeneratorTest()
    {
      Domain domain = null;

      var configuration = CreateDomainConfiguration(new[] { typeof(Models.NonPersistentStorageSupporedTypesAsKeys.ByteKeyEntity) });
      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Assert.DoesNotThrow(() => {
          _ = new Models.NonPersistentStorageSupporedTypesAsKeys.ByteKeyEntity(session);
          session.SaveChanges();
        });
      }

      configuration = CreateDomainConfiguration(new[] { typeof(Models.NonPersistentStorageSupporedTypesAsKeys.Int16KeyEntity) });
      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Assert.DoesNotThrow(() => {
          _ = new Models.NonPersistentStorageSupporedTypesAsKeys.Int16KeyEntity(session);
          session.SaveChanges();
        });
      }

      configuration = CreateDomainConfiguration(new[] { typeof(Models.NonPersistentStorageSupporedTypesAsKeys.UInt16KeyEntity) });
      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Assert.DoesNotThrow(() => {
          _ = new Models.NonPersistentStorageSupporedTypesAsKeys.UInt16KeyEntity(session);
          session.SaveChanges();
        });
      }

      configuration = CreateDomainConfiguration(new[] { typeof(Models.NonPersistentStorageSupporedTypesAsKeys.Int32KeyEntity) });
      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Assert.DoesNotThrow(() => {
          _ = new Models.NonPersistentStorageSupporedTypesAsKeys.Int32KeyEntity(session);
          session.SaveChanges();
        });
      }

      configuration = CreateDomainConfiguration(new[] { typeof(Models.NonPersistentStorageSupporedTypesAsKeys.UInt32KeyEntity) });
      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Assert.DoesNotThrow(() => {
          _ = new Models.NonPersistentStorageSupporedTypesAsKeys.UInt32KeyEntity(session);
          session.SaveChanges();
        });
      }

      configuration = CreateDomainConfiguration(new[] { typeof(Models.NonPersistentStorageSupporedTypesAsKeys.Int64KeyEntity) });
      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Assert.DoesNotThrow(() => {
          _ = new Models.NonPersistentStorageSupporedTypesAsKeys.Int64KeyEntity(session);
          session.SaveChanges();
        });
      }

      configuration = CreateDomainConfiguration(new[] { typeof(Models.NonPersistentStorageSupporedTypesAsKeys.UInt64KeyEntity) });
      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Assert.DoesNotThrow(() => {
          _ = new Models.NonPersistentStorageSupporedTypesAsKeys.UInt64KeyEntity(session);
          session.SaveChanges();
        });
      }

      configuration = CreateDomainConfiguration(new[] { typeof(Models.NonPersistentStorageSupporedTypesAsKeys.LimitedStringKeyEntity) });
      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Assert.DoesNotThrow(() => {
          _ = new Models.NonPersistentStorageSupporedTypesAsKeys.LimitedStringKeyEntity(session);
          session.SaveChanges();
        });
      }

      configuration = CreateDomainConfiguration(new[] { typeof(Models.NonPersistentStorageSupporedTypesAsKeys.StringKeyEntity) });
      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Assert.DoesNotThrow(() => {
          _ = new Models.NonPersistentStorageSupporedTypesAsKeys.StringKeyEntity(session);
          session.SaveChanges();
        });
      }

      configuration = CreateDomainConfiguration(new[] { typeof(Models.NonPersistentStorageSupporedTypesAsKeys.GuidKeyEntity) });
      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Assert.DoesNotThrow(() => {
          _ = new Models.NonPersistentStorageSupporedTypesAsKeys.GuidKeyEntity(session);
          session.SaveChanges();
        });
      }
    }

    [Test]
    public void InvalidNonPersistentKeyTypesForDefaultGeneratorTest()
    {
      // NOTE!!!
      // To make user's life easier on writing model classes DO implicitly changes
      // key generator kind to KeyGeneratorKind.None for the hierarchies which key fields
      // don't match certain conditions.
      // Such "correction" makes Domain seem functional but exceptions can happen
      // while working with domain.

      // This benifits in short term but in long term creates another caveat for user to remember
      // along his project live, newcomers will probably not know this which can cause instability.

      Domain domain = null;
      var configuration = CreateDomainConfiguration(new[] { typeof(Models.NonPersistentStorageSupporedTypesAsKeys.FloatKeyEntity) });
      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      var entityType = domain.Model.Types[typeof(Models.NonPersistentStorageSupporedTypesAsKeys.FloatKeyEntity)];

      //generator kind is changed implicitly
      Assert.That(entityType.Hierarchy.Key.GeneratorKind, Is.EqualTo(KeyGeneratorKind.None));

      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var ex = Assert.Throws<InvalidOperationException>(() => {
          _ = new Models.NonPersistentStorageSupporedTypesAsKeys.FloatKeyEntity(session);
          session.SaveChanges();
        });
        Assert.That(ex.Message.Contains(UnableToCreateKeyForMsg));
      }

      configuration = CreateDomainConfiguration(new[] { typeof(Models.NonPersistentStorageSupporedTypesAsKeys.DoubleKeyEntity) });
      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      entityType = domain.Model.Types[typeof(Models.NonPersistentStorageSupporedTypesAsKeys.DoubleKeyEntity)];

      //generator kind is changed implicitly
      Assert.That(entityType.Hierarchy.Key.GeneratorKind, Is.EqualTo(KeyGeneratorKind.None));

      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var ex = Assert.Throws<InvalidOperationException>(() => {
          _ = new Models.NonPersistentStorageSupporedTypesAsKeys.DoubleKeyEntity(session);
          session.SaveChanges();
        });
        Assert.That(ex.Message.Contains(UnableToCreateKeyForMsg));
      }

      configuration = CreateDomainConfiguration(new[] { typeof(Models.NonPersistentStorageSupporedTypesAsKeys.DecimalKeyEntity) });
      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      entityType = domain.Model.Types[typeof(Models.NonPersistentStorageSupporedTypesAsKeys.DecimalKeyEntity)];

      //generator kind is changed implicitly
      Assert.That(entityType.Hierarchy.Key.GeneratorKind, Is.EqualTo(KeyGeneratorKind.None));

      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var ex = Assert.Throws<InvalidOperationException>(() => {
          _ = new Models.NonPersistentStorageSupporedTypesAsKeys.DecimalKeyEntity(session);
          session.SaveChanges();
        });
        Assert.That(ex.Message.Contains(UnableToCreateKeyForMsg));
      }

      configuration = CreateDomainConfiguration(new[] { typeof(Models.NonPersistentStorageSupporedTypesAsKeys.FractionalDecimalKeyEntity) });
      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      entityType = domain.Model.Types[typeof(Models.NonPersistentStorageSupporedTypesAsKeys.FractionalDecimalKeyEntity)];

      //generator kind is changed implicitly
      Assert.That(entityType.Hierarchy.Key.GeneratorKind, Is.EqualTo(KeyGeneratorKind.None));

      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var ex = Assert.Throws<InvalidOperationException>(() => {
          _ = new Models.NonPersistentStorageSupporedTypesAsKeys.FractionalDecimalKeyEntity(session);
          session.SaveChanges();
        });
        Assert.That(ex.Message.Contains(UnableToCreateKeyForMsg));
      }

      configuration = CreateDomainConfiguration(new[] { typeof(Models.NonPersistentStorageSupporedTypesAsKeys.TimeSpanKeyEntity) });
      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      entityType = domain.Model.Types[typeof(Models.NonPersistentStorageSupporedTypesAsKeys.TimeSpanKeyEntity)];

      //generator kind is changed implicitly
      Assert.That(entityType.Hierarchy.Key.GeneratorKind, Is.EqualTo(KeyGeneratorKind.None));

      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var ex = Assert.Throws<InvalidOperationException>(() => {
          _ = new Models.NonPersistentStorageSupporedTypesAsKeys.TimeSpanKeyEntity(session);
          session.SaveChanges();
        });
        Assert.That(ex.Message.Contains(UnableToCreateKeyForMsg));
      }

#if NET6_0_OR_GREATER
      configuration = CreateDomainConfiguration(new[] { typeof(Models.NonPersistentStorageSupporedTypesAsKeys.TimeOnlyKeyEntity) });
      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      entityType = domain.Model.Types[typeof(Models.NonPersistentStorageSupporedTypesAsKeys.TimeOnlyKeyEntity)];

      //generator kind is changed implicitly
      Assert.That(entityType.Hierarchy.Key.GeneratorKind, Is.EqualTo(KeyGeneratorKind.None));

      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var ex = Assert.Throws<InvalidOperationException>(() => {
          _ = new Models.NonPersistentStorageSupporedTypesAsKeys.TimeOnlyKeyEntity(session);
          session.SaveChanges();
        });
        Assert.That(ex.Message.Contains(UnableToCreateKeyForMsg));
      }

      configuration = CreateDomainConfiguration(new[] { typeof(Models.NonPersistentStorageSupporedTypesAsKeys.DateOnlyKeyEntity) });
      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      entityType = domain.Model.Types[typeof(Models.NonPersistentStorageSupporedTypesAsKeys.DateOnlyKeyEntity)];

      //generator kind is changed implicitly
      Assert.That(entityType.Hierarchy.Key.GeneratorKind, Is.EqualTo(KeyGeneratorKind.None));

      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var ex = Assert.Throws<InvalidOperationException>(() => {
          _ = new Models.NonPersistentStorageSupporedTypesAsKeys.DateOnlyKeyEntity(session);
          session.SaveChanges();
        });
        Assert.That(ex.Message.Contains(UnableToCreateKeyForMsg));
      }
#endif

      configuration = CreateDomainConfiguration(new[] { typeof(Models.NonPersistentStorageSupporedTypesAsKeys.DateTimeKeyEntity) });
      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      entityType = domain.Model.Types[typeof(Models.NonPersistentStorageSupporedTypesAsKeys.DateTimeKeyEntity)];

      //generator kind is changed implicitly
      Assert.That(entityType.Hierarchy.Key.GeneratorKind, Is.EqualTo(KeyGeneratorKind.None));

      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var ex = Assert.Throws<InvalidOperationException>(() => {
          _ = new Models.NonPersistentStorageSupporedTypesAsKeys.DateTimeKeyEntity(session);
          session.SaveChanges();
        });
        Assert.That(ex.Message.Contains(UnableToCreateKeyForMsg));
      }

      if (StorageProviderInfo.Instance.CheckProviderIs(StorageProvider.SqlServer)) {
        configuration = CreateDomainConfiguration(new[] { typeof(Models.NonPersistentStorageSupporedTypesAsKeys.DateTimeOffsetKeyEntity) });
        Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
        Assert.That(domain, Is.Not.Null);
        entityType = domain.Model.Types[typeof(Models.NonPersistentStorageSupporedTypesAsKeys.DateTimeOffsetKeyEntity)];

        //generator kind is changed implicitly
        Assert.That(entityType.Hierarchy.Key.GeneratorKind, Is.EqualTo(KeyGeneratorKind.None));

        using (domain)
        using (var session = domain.OpenSession())
        using (var tx = session.OpenTransaction()) {
          var ex = Assert.Throws<InvalidOperationException>(() => {
            _ = new Models.NonPersistentStorageSupporedTypesAsKeys.DateTimeOffsetKeyEntity(session);
            session.SaveChanges();
          });
          Assert.That(ex.Message.Contains(UnableToCreateKeyForMsg));
        }
      }

      configuration = CreateDomainConfiguration(new[] { typeof(Models.NonPersistentStorageSupporedTypesAsKeys.CharKeyEntity) });
      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      entityType = domain.Model.Types[typeof(Models.NonPersistentStorageSupporedTypesAsKeys.CharKeyEntity)];

      //generator kind is changed implicitly
      Assert.That(entityType.Hierarchy.Key.GeneratorKind, Is.EqualTo(KeyGeneratorKind.None));

      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var ex = Assert.Throws<InvalidOperationException>(() => {
          _ = new Models.NonPersistentStorageSupporedTypesAsKeys.CharKeyEntity(session);
          session.SaveChanges();
        });
        Assert.That(ex.Message.Contains(UnableToCreateKeyForMsg));
      }
    }

    [Test]
    public void NonPersistentComplexKeyTest()
    {
      // NOTE!!!
      // To make user's life easier on writing model classes DO implicitly changes
      // key generator kind to KeyGeneratorKind.None for the hierarchies which key fields
      // don't match certain conditions.
      // Such "correction" makes Domain seem functional but exceptions can happen
      // while working with domain.

      // This benifits in short term but in long term creates another caveat for user to remember
      // along his project live, newcomers will probably not know this which can cause instability.

      Domain domain = null;
      var configuration = CreateDomainConfiguration(new[] { typeof(Models.NonPersistentComplexKey.ByteKeyEntity) });
      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      var entityType = domain.Model.Types[typeof(Models.NonPersistentComplexKey.ByteKeyEntity)];

      //generator kind is changed implicitly
      Assert.That(entityType.Hierarchy.Key.GeneratorKind, Is.EqualTo(KeyGeneratorKind.None));

      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var ex = Assert.Throws<InvalidOperationException>(() => {
          _ = new Models.NonPersistentComplexKey.ByteKeyEntity(session);
          session.SaveChanges();
        });
        Assert.That(ex.Message.Contains(UnableToCreateKeyForMsg));
      }
    }

    [Test]
    public void PersistentInterfaceKeyTest()
    {
      // NOTE!!!
      // To make user's life easier on writing model classes DO implicitly changes
      // key generator kind to KeyGeneratorKind.None for the hierarchies which key fields
      // don't match certain conditions.
      // Such "correction" makes Domain seem functional but exceptions can happen
      // while working with domain.

      // This benifits in short term but in long term creates another caveat for user to remember
      // along his project live, newcomers will probably not know this which can cause instability.

      Domain domain = null;
      var configuration = CreateDomainConfiguration(
        new[] {
          typeof(Models.PersistentInterfaceAsKey.DefaultGeneratorSingleKeyEntity),
          typeof(Models.PersistentInterfaceAsKey.ISomeEntity),
          typeof(Models.PersistentInterfaceAsKey.SomeEntity)
        });

      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      var entityType = domain.Model.Types[typeof(Models.PersistentInterfaceAsKey.DefaultGeneratorSingleKeyEntity)];

      //generator kind is changed implicitly
      Assert.That(entityType.Hierarchy.Key.GeneratorKind, Is.EqualTo(KeyGeneratorKind.None));

      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var ex = Assert.Throws<InvalidOperationException>(() => {
          _ = new Models.PersistentInterfaceAsKey.DefaultGeneratorSingleKeyEntity(session);
          session.SaveChanges();
        });
        Assert.That(ex.Message.Contains(UnableToCreateKeyForMsg));
      }

      configuration = CreateDomainConfiguration(
        new[] {
          typeof(Models.PersistentInterfaceAsKey.DefaultGeneratorComplexKeyEntity),
          typeof(Models.PersistentInterfaceAsKey.ISomeEntity),
          typeof(Models.PersistentInterfaceAsKey.SomeEntity)
        });

      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      entityType = domain.Model.Types[typeof(Models.PersistentInterfaceAsKey.DefaultGeneratorComplexKeyEntity)];

      //generator kind is changed implicitly
      Assert.That(entityType.Hierarchy.Key.GeneratorKind, Is.EqualTo(KeyGeneratorKind.None));

      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var ex = Assert.Throws<InvalidOperationException>(() => {
          _ = new Models.PersistentInterfaceAsKey.DefaultGeneratorComplexKeyEntity(session);
          session.SaveChanges();
        });
        Assert.That(ex.Message.Contains(UnableToCreateKeyForMsg));
      }

      configuration = CreateDomainConfiguration(
        new[] {
          typeof(Models.PersistentInterfaceAsKey.NamedDefaultGeneratorSingleKeyEntity),
          typeof(Models.PersistentInterfaceAsKey.ISomeEntity),
          typeof(Models.PersistentInterfaceAsKey.SomeEntity)
        });

      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      entityType = domain.Model.Types[typeof(Models.PersistentInterfaceAsKey.NamedDefaultGeneratorSingleKeyEntity)];

      //generator kind is changed implicitly
      Assert.That(entityType.Hierarchy.Key.GeneratorKind, Is.EqualTo(KeyGeneratorKind.None));

      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var ex = Assert.Throws<InvalidOperationException>(() => {
          _ = new Models.PersistentInterfaceAsKey.NamedDefaultGeneratorSingleKeyEntity(session);
          session.SaveChanges();
        });
        Assert.That(ex.Message.Contains(UnableToCreateKeyForMsg));
      }

      configuration = CreateDomainConfiguration(
        new[] {
          typeof(Models.PersistentInterfaceAsKey.NamedDefaultGeneratorComplexKeyEntity),
          typeof(Models.PersistentInterfaceAsKey.ISomeEntity),
          typeof(Models.PersistentInterfaceAsKey.SomeEntity)
        });

      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      entityType = domain.Model.Types[typeof(Models.PersistentInterfaceAsKey.NamedDefaultGeneratorComplexKeyEntity)];

      //generator kind is changed implicitly
      Assert.That(entityType.Hierarchy.Key.GeneratorKind, Is.EqualTo(KeyGeneratorKind.None));

      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var ex = Assert.Throws<InvalidOperationException>(() => {
          _ = new Models.PersistentInterfaceAsKey.NamedDefaultGeneratorComplexKeyEntity(session);
          session.SaveChanges();
        });
        Assert.That(ex.Message.Contains(UnableToCreateKeyForMsg));
      }

      configuration = CreateDomainConfiguration(
        new[] {
          typeof(Models.PersistentInterfaceAsKey.CustomGeneratorSingleKeyEntity),
          typeof(Models.PersistentInterfaceAsKey.ISomeEntity),
          typeof(Models.PersistentInterfaceAsKey.SomeEntity),
          typeof(Models.PersistentInterfaceAsKey.SingleKeyGenerator)
        });

      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      entityType = domain.Model.Types[typeof(Models.PersistentInterfaceAsKey.CustomGeneratorSingleKeyEntity)];

      //generator kind is changed implicitly
      Assert.That(entityType.Hierarchy.Key.GeneratorKind, Is.EqualTo(KeyGeneratorKind.None));

      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var ex = Assert.Throws<InvalidOperationException>(() => {
          _ = new Models.PersistentInterfaceAsKey.CustomGeneratorSingleKeyEntity(session);
          session.SaveChanges();
        });
        Assert.That(ex.Message.Contains(UnableToCreateKeyForMsg));
      }

      configuration = CreateDomainConfiguration(
        new[] {
          typeof(Models.PersistentInterfaceAsKey.CustomGeneratorComplexKeyEntity),
          typeof(Models.PersistentInterfaceAsKey.ISomeEntity),
          typeof(Models.PersistentInterfaceAsKey.SomeEntity),
          typeof(Models.PersistentInterfaceAsKey.ComplexKeyGenerator),
        });

      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      entityType = domain.Model.Types[typeof(Models.PersistentInterfaceAsKey.CustomGeneratorComplexKeyEntity)];

      //generator kind is changed implicitly
      Assert.That(entityType.Hierarchy.Key.GeneratorKind, Is.EqualTo(KeyGeneratorKind.None));

      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var ex = Assert.Throws<InvalidOperationException>(() => {
          _ = new Models.PersistentInterfaceAsKey.CustomGeneratorComplexKeyEntity(session);
          session.SaveChanges();
        });
        Assert.That(ex.Message.Contains(UnableToCreateKeyForMsg));
      }

      configuration = CreateDomainConfiguration(
        new[] {
          typeof(Models.PersistentInterfaceAsKey.NoGeneratorSingleKeyEntity),
          typeof(Models.PersistentInterfaceAsKey.ISomeEntity),
          typeof(Models.PersistentInterfaceAsKey.SomeEntity)
        });
      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      entityType = domain.Model.Types[typeof(Models.PersistentInterfaceAsKey.NoGeneratorSingleKeyEntity)];

      Assert.That(entityType.Hierarchy.Key.GeneratorKind, Is.EqualTo(KeyGeneratorKind.None));

      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var ex = Assert.Throws<InvalidOperationException>(() => {
          var keyEntity = new Models.PersistentInterfaceAsKey.SomeEntity(session);
          _ = new Models.PersistentInterfaceAsKey.NoGeneratorSingleKeyEntity(session, keyEntity); // suppose to be this way
          _ = new Models.PersistentInterfaceAsKey.NoGeneratorSingleKeyEntity(session); // tries to trigger generator
          session.SaveChanges();
        });
        Assert.That(ex.Message.Contains(UnableToCreateKeyForMsg));
      }

      configuration = CreateDomainConfiguration(
        new[] {
          typeof(Models.PersistentInterfaceAsKey.NoGeneratorComplexKeyEntity),
          typeof(Models.PersistentInterfaceAsKey.ISomeEntity),
          typeof(Models.PersistentInterfaceAsKey.SomeEntity)
        });

      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      entityType = domain.Model.Types[typeof(Models.PersistentInterfaceAsKey.NoGeneratorComplexKeyEntity)];

      Assert.That(entityType.Hierarchy.Key.GeneratorKind, Is.EqualTo(KeyGeneratorKind.None));

      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var ex = Assert.Throws<InvalidOperationException>(() => {
          var keyEntity = new Models.PersistentInterfaceAsKey.SomeEntity(session);
          _ = new Models.PersistentInterfaceAsKey.NoGeneratorComplexKeyEntity(session, 123, keyEntity); // suppose to be this way
          _ = new Models.PersistentInterfaceAsKey.NoGeneratorComplexKeyEntity(session); // tries to trigger generator
          session.SaveChanges();
        });
        Assert.That(ex.Message.Contains(UnableToCreateKeyForMsg));
      }
    }

    [Test]
    public void EntityAsKeyTest()
    {
      // NOTE!!!
      // To make user's life easier on writing model classes DO implicitly changes
      // key generator kind to KeyGeneratorKind.None for the hierarchies which key fields
      // don't match certain conditions.
      // Such "correction" makes Domain seem functional but exceptions can happen
      // while working with domain.

      // This benifits in short term but in long term creates another caveat for user to remember
      // along his project live, newcomers will probably not know this which can cause instability.

      Domain domain = null;
      var configuration = CreateDomainConfiguration(
        new[] {
          typeof(Models.EntityAsKey.DefaultGeneratorSingleKeyEntity),
          typeof(Models.EntityAsKey.SomeEntity)
        });

      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      var entityType = domain.Model.Types[typeof(Models.EntityAsKey.DefaultGeneratorSingleKeyEntity)];

      //generator kind is changed implicitly
      Assert.That(entityType.Hierarchy.Key.GeneratorKind, Is.EqualTo(KeyGeneratorKind.None));

      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var ex = Assert.Throws<InvalidOperationException>(() => {
          _ = new Models.EntityAsKey.DefaultGeneratorSingleKeyEntity(session);
          session.SaveChanges();
        });
        Assert.That(ex.Message.Contains(UnableToCreateKeyForMsg));
      }

      configuration = CreateDomainConfiguration(
        new[] {
          typeof(Models.EntityAsKey.DefaultGeneratorComplexKeyEntity),
          typeof(Models.EntityAsKey.SomeEntity)
        });

      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      entityType = domain.Model.Types[typeof(Models.EntityAsKey.DefaultGeneratorComplexKeyEntity)];

      //generator kind is changed implicitly
      Assert.That(entityType.Hierarchy.Key.GeneratorKind, Is.EqualTo(KeyGeneratorKind.None));

      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var ex = Assert.Throws<InvalidOperationException>(() => {
          _ = new Models.EntityAsKey.DefaultGeneratorComplexKeyEntity(session);
          session.SaveChanges();
        });
        Assert.That(ex.Message.Contains(UnableToCreateKeyForMsg));
      }

      configuration = CreateDomainConfiguration(
        new[] {
          typeof(Models.EntityAsKey.NamedDefaultGeneratorSingleKeyEntity),
          typeof(Models.EntityAsKey.SomeEntity)
        });

      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      entityType = domain.Model.Types[typeof(Models.EntityAsKey.NamedDefaultGeneratorSingleKeyEntity)];

      //generator kind is changed implicitly
      Assert.That(entityType.Hierarchy.Key.GeneratorKind, Is.EqualTo(KeyGeneratorKind.None));

      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var ex = Assert.Throws<InvalidOperationException>(() => {
          _ = new Models.EntityAsKey.NamedDefaultGeneratorSingleKeyEntity(session);
          session.SaveChanges();
        });
        Assert.That(ex.Message.Contains(UnableToCreateKeyForMsg));
      }

      configuration = CreateDomainConfiguration(
        new[] {
          typeof(Models.EntityAsKey.NamedDefaultGeneratorComplexKeyEntity),
          typeof(Models.EntityAsKey.SomeEntity)
        });

      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      entityType = domain.Model.Types[typeof(Models.EntityAsKey.NamedDefaultGeneratorComplexKeyEntity)];

      //generator kind is changed implicitly
      Assert.That(entityType.Hierarchy.Key.GeneratorKind, Is.EqualTo(KeyGeneratorKind.None));

      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var ex = Assert.Throws<InvalidOperationException>(() => {
          _ = new Models.EntityAsKey.NamedDefaultGeneratorComplexKeyEntity(session);
          session.SaveChanges();
        });
        Assert.That(ex.Message.Contains(UnableToCreateKeyForMsg));
      }

      configuration = CreateDomainConfiguration(
        new[] {
          typeof(Models.EntityAsKey.CustomGeneratorSingleKeyEntity),
          typeof(Models.EntityAsKey.SomeEntity),
          typeof(Models.EntityAsKey.SingleKeyGenerator)
        });

      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      entityType = domain.Model.Types[typeof(Models.EntityAsKey.CustomGeneratorSingleKeyEntity)];

      //generator kind is changed implicitly
      Assert.That(entityType.Hierarchy.Key.GeneratorKind, Is.EqualTo(KeyGeneratorKind.None));

      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var ex = Assert.Throws<InvalidOperationException>(() => {
          _ = new Models.EntityAsKey.CustomGeneratorSingleKeyEntity(session);
          session.SaveChanges();
        });
        Assert.That(ex.Message.Contains(UnableToCreateKeyForMsg));
      }

      configuration = CreateDomainConfiguration(
        new[] {
          typeof(Models.EntityAsKey.CustomGeneratorComplexKeyEntity),
          typeof(Models.EntityAsKey.SomeEntity),
          typeof(Models.EntityAsKey.ComplexKeyGenerator),

        });

      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      entityType = domain.Model.Types[typeof(Models.EntityAsKey.CustomGeneratorComplexKeyEntity)];

      //generator kind is changed implicitly
      Assert.That(entityType.Hierarchy.Key.GeneratorKind, Is.EqualTo(KeyGeneratorKind.None));

      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var ex = Assert.Throws<InvalidOperationException>(() => {
          _ = new Models.EntityAsKey.CustomGeneratorComplexKeyEntity(session);
          session.SaveChanges();
        });
        Assert.That(ex.Message.Contains(UnableToCreateKeyForMsg));
      }

      configuration = CreateDomainConfiguration(
        new[] {
          typeof(Models.EntityAsKey.NoGeneratorSingleKeyEntity),
          typeof(Models.EntityAsKey.SomeEntity)
        });
      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      entityType = domain.Model.Types[typeof(Models.EntityAsKey.NoGeneratorSingleKeyEntity)];

      Assert.That(entityType.Hierarchy.Key.GeneratorKind, Is.EqualTo(KeyGeneratorKind.None));

      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var ex = Assert.Throws<InvalidOperationException>(() => {
          var keyEntity = new Models.EntityAsKey.SomeEntity(session);
          _ = new Models.EntityAsKey.NoGeneratorSingleKeyEntity(session, keyEntity); // suppose to be this way
          _ = new Models.EntityAsKey.NoGeneratorSingleKeyEntity(session); // tries to trigger generator
          session.SaveChanges();
        });
        Assert.That(ex.Message.Contains(UnableToCreateKeyForMsg));
      }

      configuration = CreateDomainConfiguration(
        new[] {
          typeof(Models.EntityAsKey.NoGeneratorComplexKeyEntity),
          typeof(Models.EntityAsKey.SomeEntity)
        });

      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      entityType = domain.Model.Types[typeof(Models.EntityAsKey.NoGeneratorComplexKeyEntity)];

      Assert.That(entityType.Hierarchy.Key.GeneratorKind, Is.EqualTo(KeyGeneratorKind.None));

      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var ex = Assert.Throws<InvalidOperationException>(() => {
          var keyEntity = new Models.EntityAsKey.SomeEntity(session);
          _ = new Models.EntityAsKey.NoGeneratorComplexKeyEntity(session, 123, keyEntity); // suppose to be this way
          _ = new Models.EntityAsKey.NoGeneratorComplexKeyEntity(session); // tries to trigger generator
          session.SaveChanges();
        });
        Assert.That(ex.Message.Contains(UnableToCreateKeyForMsg));
      }
    }

    [Test]
    public void ByteArrayKeyTest()
    {
      TestWithExceptionExpected<DomainBuilderException>(new[] { typeof(Models.NonPersistentStorageSupporedTypesAsKeys.ByteArrayKeyEntity) },
        (ex) => ex.Message.Contains(CantBeOfMsg));
    }

    [Test]
    public void IntArrayKeyTest()
    {
      TestWithExceptionExpected<DomainBuilderException>(new[] { typeof(Models.NonPersistentStorageSupporedTypesAsKeys.IntArrayKeyEntity) },
        (ex) => ex.Message.Contains(CantBeOfMsg));
    }

    [Test]
    public void ObjectKeyTest()
    {
      TestWithExceptionExpected<DomainBuilderException>(new[] { typeof(Models.NonPersistentStorageSupporedTypesAsKeys.ObjectKeyEntity) },
        (ex) => ex.Message.Contains("Unsupported type"));
    }

    [Test]
    public void LazyLoadKeyTest()
    {
      const string CannotBeLazyLoadMsg = "cannot be LazyLoad";

      TestWithExceptionExpected<DomainBuilderException>(new[] { typeof(Models.LazyLoadKey.DefaultGeneratorSingleKeyEntity) },
        (ex) => Assert.That(ex.Message.Contains(CannotBeLazyLoadMsg)));

      TestWithExceptionExpected<DomainBuilderException>(new[] { typeof(Models.LazyLoadKey.DefaultGeneratorComplexKeyEntity) },
        (ex) => Assert.That(ex.Message.Contains(CannotBeLazyLoadMsg)));

      TestWithExceptionExpected<DomainBuilderException>(new[] { typeof(Models.LazyLoadKey.NamedDefaultGeneratorSingleKeyEntity) },
        (ex) => Assert.That(ex.Message.Contains(CannotBeLazyLoadMsg)));

      TestWithExceptionExpected<DomainBuilderException>(new[] { typeof(Models.LazyLoadKey.NamedDefaultGeneratorComplexKeyEntity) },
        (ex) => Assert.That(ex.Message.Contains(CannotBeLazyLoadMsg)));

      TestWithExceptionExpected<DomainBuilderException>(
        new[] { typeof(Models.LazyLoadKey.CustomGeneratorSingleKeyEntity), typeof(Models.LazyLoadKey.SingleKeyGenerator) },
        (ex) => Assert.That(ex.Message.Contains(CannotBeLazyLoadMsg)));

      TestWithExceptionExpected<DomainBuilderException>(
        new[] { typeof(Models.LazyLoadKey.CustomGeneratorComplexKeyEntity), typeof(Models.LazyLoadKey.ComplexKeyGenerator) },
        (ex) => Assert.That(ex.Message.Contains(CannotBeLazyLoadMsg)));

      TestWithExceptionExpected<DomainBuilderException>(new[] { typeof(Models.LazyLoadKey.NoGeneratorSingleKeyEntity) },
        (ex) => Assert.That(ex.Message.Contains(CannotBeLazyLoadMsg)));

      TestWithExceptionExpected<DomainBuilderException>(new[] { typeof(Models.LazyLoadKey.NoGeneratorComplexKeyEntity) },
        (ex) => Assert.That(ex.Message.Contains(CannotBeLazyLoadMsg)));
    }

    [Test]
    public void EntitySetKeyTest()
    {
      TestWithExceptionExpected<DomainBuilderException>(new[] { typeof(Models.EntitySetAsKey.DefaultGeneratorSingleKeyEntity) },
        (ex) => Assert.That(ex.Message.Contains(CantBeOfMsg)));

      TestWithExceptionExpected<DomainBuilderException>(new[] { typeof(Models.EntitySetAsKey.DefaultGeneratorComplexKeyEntity) },
        (ex) => Assert.That(ex.Message.Contains(CantBeOfMsg)));

      TestWithExceptionExpected<DomainBuilderException>(new[] { typeof(Models.EntitySetAsKey.NamedDefaultGeneratorSingleKeyEntity) },
        (ex) => Assert.That(ex.Message.Contains(CantBeOfMsg)));

      TestWithExceptionExpected<DomainBuilderException>(new[] { typeof(Models.EntitySetAsKey.NamedDefaultGeneratorComplexKeyEntity) },
        (ex) => Assert.That(ex.Message.Contains(CantBeOfMsg)));

      TestWithExceptionExpected<DomainBuilderException>(
        new[] { typeof(Models.EntitySetAsKey.CustomGeneratorSingleKeyEntity), typeof(Models.EntitySetAsKey.SingleKeyGenerator) },
        (ex) => Assert.That(ex.Message.Contains(CantBeOfMsg)));

      TestWithExceptionExpected<DomainBuilderException>(
        new[] { typeof(Models.EntitySetAsKey.CustomGeneratorComplexKeyEntity), typeof(Models.EntitySetAsKey.ComplexKeyGenerator) },
        (ex) => Assert.That(ex.Message.Contains(CantBeOfMsg)));

      TestWithExceptionExpected<DomainBuilderException>(new[] { typeof(Models.EntitySetAsKey.NoGeneratorSingleKeyEntity) },
        (ex) => Assert.That(ex.Message.Contains(CantBeOfMsg)));

      TestWithExceptionExpected<DomainBuilderException>(new[] { typeof(Models.EntitySetAsKey.NoGeneratorComplexKeyEntity) },
        (ex) =>  Assert.That(ex.Message.Contains(CantBeOfMsg)));
    }

    [Test]
    public void IEntityKeyTest()
    {
      TestWithExceptionExpected<DomainBuilderException>(new[] { typeof(Models.IEntityAsKey.DefaultGeneratorSingleKeyEntity) },
        (ex) => Assert.That(ex.Message.Contains(UnsupportedTypeMsg)));

      TestWithExceptionExpected<DomainBuilderException>(new[] { typeof(Models.IEntityAsKey.DefaultGeneratorComplexKeyEntity) },
        (ex) => Assert.That(ex.Message.Contains(UnsupportedTypeMsg)));

      TestWithExceptionExpected<DomainBuilderException>(new[] { typeof(Models.IEntityAsKey.NamedDefaultGeneratorSingleKeyEntity) },
        (ex) => Assert.That(ex.Message.Contains(UnsupportedTypeMsg)));

      TestWithExceptionExpected<DomainBuilderException>(new[] { typeof(Models.IEntityAsKey.NamedDefaultGeneratorComplexKeyEntity) },
        (ex) => Assert.That(ex.Message.Contains(UnsupportedTypeMsg)));

      TestWithExceptionExpected<DomainBuilderException>(
        new[] { typeof(Models.IEntityAsKey.CustomGeneratorSingleKeyEntity), typeof(Models.IEntityAsKey.SingleKeyGenerator) },
        (ex) => Assert.That(ex.Message.Contains(UnsupportedTypeMsg)));

      TestWithExceptionExpected<DomainBuilderException>(
        new[] { typeof(Models.IEntityAsKey.CustomGeneratorComplexKeyEntity), typeof(Models.IEntityAsKey.ComplexKeyGenerator) },
        (ex) => Assert.That(ex.Message.Contains(UnsupportedTypeMsg)));

      TestWithExceptionExpected<DomainBuilderException>(new[] { typeof(Models.IEntityAsKey.NoGeneratorSingleKeyEntity) },
        (ex) => Assert.That(ex.Message.Contains(UnsupportedTypeMsg)));

      TestWithExceptionExpected<DomainBuilderException>(new[] { typeof(Models.IEntityAsKey.NoGeneratorComplexKeyEntity) },
        (ex) => Assert.That(ex.Message.Contains(UnsupportedTypeMsg)));
    }

    [Test]
    public void StructureKeyTest()
    {
      TestWithExceptionExpected<DomainBuilderException>(
        new[] {
          typeof(Models.StructureAsKey.DefaultGeneratorSingleKeyEntity),
          typeof(Models.StructureAsKey.Point)
        },
        (ex) => Assert.That(ex.Message.Contains(CantBeOfMsg)));

      TestWithExceptionExpected<DomainBuilderException>(
        new[] {
          typeof(Models.StructureAsKey.DefaultGeneratorComplexKeyEntity),
          typeof(Models.StructureAsKey.Point)
        },
        (ex) => Assert.That(ex.Message.Contains(CantBeOfMsg)));

      TestWithExceptionExpected<DomainBuilderException>(
        new[] {
          typeof(Models.StructureAsKey.NamedDefaultGeneratorSingleKeyEntity),
          typeof(Models.StructureAsKey.Point)
        },
        (ex) => Assert.That(ex.Message.Contains(CantBeOfMsg)));

      TestWithExceptionExpected<DomainBuilderException>(
        new[] {
          typeof(Models.StructureAsKey.NamedDefaultGeneratorComplexKeyEntity),
          typeof(Models.StructureAsKey.Point)
        },
        (ex) => Assert.That(ex.Message.Contains(CantBeOfMsg)));

      TestWithExceptionExpected<DomainBuilderException>(
        new[] {
          typeof(Models.StructureAsKey.CustomGeneratorSingleKeyEntity),
          typeof(Models.StructureAsKey.SingleKeyGenerator),
          typeof(Models.StructureAsKey.Point)
        },
        (ex) => Assert.That(ex.Message.Contains(CantBeOfMsg)));

      TestWithExceptionExpected<DomainBuilderException>(
        new[] {
          typeof(Models.StructureAsKey.CustomGeneratorComplexKeyEntity),
          typeof(Models.StructureAsKey.ComplexKeyGenerator),
          typeof(Models.StructureAsKey.Point)
        },
        (ex) => Assert.That(ex.Message.Contains(CantBeOfMsg)));

      TestWithExceptionExpected<DomainBuilderException>(
        new[] {
          typeof(Models.StructureAsKey.NoGeneratorSingleKeyEntity),
          typeof(Models.StructureAsKey.Point)
        },
        (ex) => Assert.That(ex.Message.Contains(CantBeOfMsg)));

      TestWithExceptionExpected<DomainBuilderException>(
        new[] {
          typeof(Models.StructureAsKey.NoGeneratorComplexKeyEntity),
          typeof(Models.StructureAsKey.Point)
        },
        (ex) => Assert.That(ex.Message.Contains(CantBeOfMsg)));
    }

    [Test]
    public void NoKeyFieldsHierarchyTest()
    {
      TestWithExceptionExpected<DomainBuilderException>(typeof(Models.NoKeyFieldsHierarchy.NoKeyFieldsEntity),
        (ex) => Assert.That(ex.Message.Contains("doesn't contain any key fields")));
    }

    [Test]
    public void KeyNotInRootTest()
    {
      TestWithExceptionExpected<DomainBuilderException>(
        new[] {
          typeof(Models.KeyNotInRoot.DefaultGeneratorSingleKeyEntityRoot),
          typeof(Models.KeyNotInRoot.DefaultGeneratorSingleKeyEntityLeaf)
        },
        (ex) => Assert.That(ex.Message.Contains("doesn't contain any key fields")));

      TestWithExceptionExpected<DomainBuilderException>(
        new[] {
          typeof(Models.KeyNotInRoot.NamedDefaultGeneratorSingleKeyEntityRoot),
          typeof(Models.KeyNotInRoot.NamedDefaultGeneratorSingleKeyEntityLeaf)
        },
        (ex) => Assert.That(ex.Message.Contains("doesn't contain any key fields")));

      TestWithExceptionExpected<DomainBuilderException>(
        new[] {
          typeof(Models.KeyNotInRoot.CustomGeneratorSingleKeyEntityRoot),
          typeof(Models.KeyNotInRoot.CustomGeneratorSingleKeyEntityLeaf),
          typeof(Models.KeyNotInRoot.SingleKeyGenerator)
        },
        (ex) => Assert.That(ex.Message.Contains("doesn't contain any key fields")));

      TestWithExceptionExpected<DomainBuilderException>(
        new[] {
          typeof(Models.KeyNotInRoot.NoGeneratorSingleKeyEntityRoot),
          typeof(Models.KeyNotInRoot.NoGeneratorSingleKeyEntityLeaf),
        },
        (ex) => Assert.That(ex.Message.Contains("doesn't contain any key fields")));
    }

    [Test]
    public void TwoKeysTest()
    {
      // NOTE!!!
      // To make user's life easier on writing model classes DO implicitly changes
      // key generator kind to KeyGeneratorKind.None for the hierarchies which key fields
      // don't match certain conditions.
      // Such "correction" makes Domain seem functional but exceptions can happen
      // while working with domain.

      // This benifits in short term but in long term creates another caveat for user to remember
      // along his project live, newcomers will probably not know this which can cause instability.

      Domain domain = null;
      var configuration = CreateDomainConfiguration(new[] { typeof(Models.TwoKeys.DefaultGeneratorEntity) });

      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      var entityType = domain.Model.Types[typeof(Models.TwoKeys.DefaultGeneratorEntity)];

      //generator kind is changed implicitly
      Assert.That(entityType.Hierarchy.Key.GeneratorKind, Is.EqualTo(KeyGeneratorKind.None));

      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var ex = Assert.Throws<InvalidOperationException>(() => {
          _ = new Models.TwoKeys.DefaultGeneratorEntity(session);
          session.SaveChanges();
        });
        Assert.That(ex.Message.Contains(UnableToCreateKeyForMsg));
      }

      configuration = CreateDomainConfiguration(new[] { typeof(Models.TwoKeys.NamedGeneratorEntity) });

      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      entityType = domain.Model.Types[typeof(Models.TwoKeys.NamedGeneratorEntity)];

      //generator kind is changed implicitly
      Assert.That(entityType.Hierarchy.Key.GeneratorKind, Is.EqualTo(KeyGeneratorKind.None));

      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var ex = Assert.Throws<InvalidOperationException>(() => {
          _ = new Models.TwoKeys.NamedGeneratorEntity(session);
          session.SaveChanges();
        });
        Assert.That(ex.Message.Contains(UnableToCreateKeyForMsg));
      }

      configuration = CreateDomainConfiguration(
        new[] {
          typeof(Models.TwoKeys.CustomGeneratorEntity),
          typeof(Models.TwoKeys.TwoKeyFieldsGen)
        });

      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      entityType = domain.Model.Types[typeof(Models.TwoKeys.CustomGeneratorEntity)];

      //generator kind is changed implicitly
      Assert.That(entityType.Hierarchy.Key.GeneratorKind, Is.EqualTo(KeyGeneratorKind.Custom));

      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Assert.DoesNotThrow(() => {
          _ = new Models.TwoKeys.CustomGeneratorEntity(session);
          session.SaveChanges();
        });
      }

      configuration = CreateDomainConfiguration(
        new[] {
          typeof(Models.TwoKeys.NoGeneratorEntity)
        });

      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      entityType = domain.Model.Types[typeof(Models.TwoKeys.NoGeneratorEntity)];

      Assert.That(entityType.Hierarchy.Key.GeneratorKind, Is.EqualTo(KeyGeneratorKind.None));

      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Assert.DoesNotThrow(() => {
          _ = new Models.TwoKeys.NoGeneratorEntity(session, 1, 10);
          session.SaveChanges();
        });
      }
    }

    [Test]
    public void SingleKeyWithTypeId()
    {
      Domain domain = null;
      var configuration = CreateDomainConfiguration(new[] { typeof(Models.SingleKeyWithTypeId.DefaultGeneratorEntity) });

      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      var entityType = domain.Model.Types[typeof(Models.SingleKeyWithTypeId.DefaultGeneratorEntity)];

      Assert.That(entityType.Hierarchy.Key.GeneratorKind, Is.EqualTo(KeyGeneratorKind.Default));

      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Assert.DoesNotThrow(() => {
          _ = new Models.SingleKeyWithTypeId.DefaultGeneratorEntity(session);
          session.SaveChanges();
        });
      }

      configuration = CreateDomainConfiguration(new[] { typeof(Models.SingleKeyWithTypeId.NamedGeneratorEntity) });

      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      entityType = domain.Model.Types[typeof(Models.SingleKeyWithTypeId.NamedGeneratorEntity)];

      Assert.That(entityType.Hierarchy.Key.GeneratorKind, Is.EqualTo(KeyGeneratorKind.Default));

      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Assert.DoesNotThrow(() => {
          _ = new Models.SingleKeyWithTypeId.NamedGeneratorEntity(session);
          session.SaveChanges();
        });
      }

      configuration = CreateDomainConfiguration(
        new[] {
          typeof(Models.SingleKeyWithTypeId.CustomGeneratorEntity),
          typeof(Models.SingleKeyWithTypeId.SingleKeyGenerator)
        });

      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      entityType = domain.Model.Types[typeof(Models.SingleKeyWithTypeId.CustomGeneratorEntity)];

      Assert.That(entityType.Hierarchy.Key.GeneratorKind, Is.EqualTo(KeyGeneratorKind.Custom));

      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Assert.DoesNotThrow(() => {
          _ = new Models.SingleKeyWithTypeId.CustomGeneratorEntity(session);
          session.SaveChanges();
        });
      }

      configuration = CreateDomainConfiguration(
        new[] { typeof(Models.SingleKeyWithTypeId.NoGeneratorEntity) });

      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      entityType = domain.Model.Types[typeof(Models.SingleKeyWithTypeId.NoGeneratorEntity)];

      Assert.That(entityType.Hierarchy.Key.GeneratorKind, Is.EqualTo(KeyGeneratorKind.None));

      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Assert.DoesNotThrow(() => {
          _ = new Models.SingleKeyWithTypeId.NoGeneratorEntity(session, 10);
          session.SaveChanges();
        });
      }
    }

    [Test]
    public void ThreeKeysTest()
    {
      TestWithExceptionExpected<DomainBuilderException>(new[] { typeof(Models.ThreeKeys.DefaultGeneratorEntity) },
        ex => ex.Message.Contains(KeyGeneratorCanServeHierarchyWithOneKeyFieldMsg));

      TestWithExceptionExpected<DomainBuilderException>(new[] { typeof(Models.ThreeKeys.NamedGeneratorEntity) },
        ex => ex.Message.Contains(KeyGeneratorCanServeHierarchyWithOneKeyFieldMsg));

      Domain domain = null;

      var configuration = CreateDomainConfiguration(
        new[] {
          typeof(Models.ThreeKeys.CustomGeneratorEntity),
          typeof(Models.ThreeKeys.TwoKeyFieldsGen)
        });

      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      var entityType = domain.Model.Types[typeof(Models.ThreeKeys.CustomGeneratorEntity)];

      //generator kind is changed implicitly
      Assert.That(entityType.Hierarchy.Key.GeneratorKind, Is.EqualTo(KeyGeneratorKind.Custom));

      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Assert.DoesNotThrow(() => {
          _ = new Models.ThreeKeys.CustomGeneratorEntity(session);
          session.SaveChanges();
        });
      }

      configuration = CreateDomainConfiguration(new[] { typeof(Models.ThreeKeys.NoGeneratorEntity) });

      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      entityType = domain.Model.Types[typeof(Models.ThreeKeys.NoGeneratorEntity)];

      Assert.That(entityType.Hierarchy.Key.GeneratorKind, Is.EqualTo(KeyGeneratorKind.None));

      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Assert.DoesNotThrow(() => {
          _ = new Models.ThreeKeys.NoGeneratorEntity(session, 1, 10, 100);
          session.SaveChanges();
        });
      }
    }

    [Test]
    public void IModuleModifications()
    {
      // IModule manually adds existing TypeId column to key columns

      Domain domain = null;
      var configuration = CreateDomainConfiguration(new[] {
        typeof(Models.IModuleModifications.ManualIncludeTypeIdEntity),
        typeof(Models.IModuleModifications.ModelDefModifier)
      });

      _ = Assert.Throws<DomainBuilderException>(() => domain = Domain.Build(configuration));

      // IModule changes HierarhyRootDef.IncludeTypeId
      configuration = CreateDomainConfiguration(new[] {
        typeof(Models.IModuleModifications.IncludeTypeIdEntity),
        typeof(Models.IModuleModifications.ModelDefModifier)
      });

      Assert.DoesNotThrow(() => domain = Domain.Build(configuration));
      Assert.That(domain, Is.Not.Null);
      var entityType = domain.Model.Types[typeof(Models.IModuleModifications.IncludeTypeIdEntity)];

      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Assert.DoesNotThrow(() => {
          _ = new Models.IModuleModifications.IncludeTypeIdEntity(session);
          session.SaveChanges();
        });
      }
    }

    private void TestWithExceptionExpected<TException>(Type baseTypeFromNs, Action<TException> exceptionValidator)
      where TException : Exception
    {
      var exceptionInstance = Assert.Throws<TException>(() => BuildDomainStructureOnly(CreateDomainConfiguration(baseTypeFromNs)));
      exceptionValidator?.Invoke(exceptionInstance);
    }

    private void TestWithExceptionExpected<TException>(Type[] typesToRegister, Action<TException> exceptionValidator)
      where TException : Exception
    {
      var exceptionInstance = Assert.Throws<TException>(() => BuildDomainStructureOnly(CreateDomainConfiguration(typesToRegister)));
      exceptionValidator?.Invoke(exceptionInstance);
    }

    private DomainConfiguration CreateDomainConfiguration(Type baseTypeFromNs)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.RegisterCaching(baseTypeFromNs.Assembly, baseTypeFromNs.Namespace);
      return configuration;
    }

    private DomainConfiguration CreateDomainConfiguration(Type[] typesToRegister)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      foreach(var t in typesToRegister) {
        configuration.Types.Register(t);
      }
      return configuration;
    }

    private Domain BuildDomainStructureOnly(DomainConfiguration domainConfiguration)
    {
      var upgradeContext = new Orm.Upgrade.UpgradeContext(domainConfiguration);

      using (upgradeContext.Activate())
      using (upgradeContext.Services) {
        var configuration = CreateDomainBuilderConfiguration(upgradeContext);
        return DomainBuilder.Run(configuration);
      }
    }

    private DomainBuilderConfiguration CreateDomainBuilderConfiguration(Orm.Upgrade.UpgradeContext upgradeContext)
    {
      var services = upgradeContext.Services;

      BuildServices(services, upgradeContext.Configuration);

      var configuration = new DomainBuilderConfiguration {
        DomainConfiguration = upgradeContext.Configuration,
        Stage = Orm.Upgrade.UpgradeStage.Final,
        Services = services,
        ModelFilter = new Orm.Upgrade.StageModelFilter(services.UpgradeHandlers, Orm.Upgrade.UpgradeStage.Final),
        UpgradeContextCookie = new object(),
        RecycledDefinitions = new List<RecycledDefinition>(),
        DefaultSchemaInfo = new Sql.Info.DefaultSchemaInfo("dummyDb", "dummySchema")
      };

      configuration.Lock();
      return configuration;
    }

    private void BuildServices(Orm.Upgrade.UpgradeServiceAccessor services, DomainConfiguration configuration)
    {
      services.Configuration = configuration;
      services.IndexFilterCompiler = new Providers.PartialIndexFilterCompiler();

      var descriptor = ProviderDescriptor.Get(configuration.ConnectionInfo.Provider);
      var driverFactory = (Sql.SqlDriverFactory) Activator.CreateInstance(descriptor.DriverFactory);
      var handlerFactory = (Providers.HandlerFactory) Activator.CreateInstance(descriptor.HandlerFactory);
      var driver = Providers.StorageDriver.Create(driverFactory, configuration);
      services.HandlerFactory = handlerFactory;
      services.StorageDriver = driver;
      services.NameBuilder = new Providers.NameBuilder(configuration, driver.ProviderInfo);


      var dummyDefaultSchemaInfo = new Sql.Info.DefaultSchemaInfo("dummyDb", "dummySchema");
      services.MappingResolver = new Providers.SimpleMappingResolver(dummyDefaultSchemaInfo);
      BuildExternalServices(services, configuration);
      services.Lock();
    }

    private void BuildExternalServices(Orm.Upgrade.UpgradeServiceAccessor serviceAccessor, DomainConfiguration configuration)
    {
      var standardRegistrations = new[] {
        new ServiceRegistration(typeof (DomainConfiguration), configuration),
        //new ServiceRegistration(typeof (Orm.Upgrade.UpgradeContext), context)
      };

      var handlers = configuration.Types.UpgradeHandlers
        .Select(type => new ServiceRegistration(typeof(Orm.Upgrade.IUpgradeHandler), type, false));

      var registrations = standardRegistrations.Concat(handlers);
      var serviceContainer = new ServiceContainer(registrations);
      serviceAccessor.RegisterResource(serviceContainer);

      BuildModules(serviceAccessor);
      BuildUpgradeHandlers(serviceAccessor, serviceContainer);
    }

    private static void BuildModules(Orm.Upgrade.UpgradeServiceAccessor serviceAccessor)
    {
      serviceAccessor.Modules = new List<IModule>().AsReadOnly();
    }

    private static void BuildUpgradeHandlers(Orm.Upgrade.UpgradeServiceAccessor serviceAccessor, IServiceContainer serviceContainer)
    {
      // Getting user handlers
      var userHandlers =
        from handler in serviceContainer.GetAll<Orm.Upgrade.IUpgradeHandler>()
        let assembly = handler.Assembly ?? handler.GetType().Assembly
        where handler.IsEnabled
        group handler by assembly;

      // Adding user handlers
      var handlers = new Dictionary<Assembly, Orm.Upgrade.IUpgradeHandler>();
      foreach (var group in userHandlers) {
        var candidates = group.ToList();
        if (candidates.Count > 1) {
          throw new DomainBuilderException(
            string.Format(Strings.ExMoreThanOneEnabledXIsProvidedForAssemblyY, typeof(Orm.Upgrade.IUpgradeHandler).Name, @group.Key));
        }
        handlers.Add(group.Key, candidates[0]);
      }

      // Adding default handlers
      var assembliesWithUserHandlers = handlers.Select(pair => pair.Key);
      var assembliesWithoutUserHandler =
        serviceAccessor.Configuration.Types.PersistentTypes
          .Select(type => type.Assembly)
          .Distinct()
          .Except(assembliesWithUserHandlers);

      foreach (var assembly in assembliesWithoutUserHandler) {
        var handler = new Orm.Upgrade.UpgradeHandler(assembly);
        handlers.Add(assembly, handler);
      }

      // Building a list of handlers sorted by dependencies of their assemblies
      var dependencies = handlers.Keys.ToDictionary(
        assembly => assembly,
        assembly => assembly.GetReferencedAssemblies().Select(assemblyName => assemblyName.ToString()).ToHashSet());
      var sortedHandlers = handlers
        .SortTopologically((a0, a1) => dependencies[a1.Key].Contains(a0.Key.GetName().ToString()))
        .Select(pair => pair.Value);

      // Storing the result
      serviceAccessor.UpgradeHandlers = new ReadOnlyDictionary<Assembly, Orm.Upgrade.IUpgradeHandler>(handlers);
      serviceAccessor.OrderedUpgradeHandlers = sortedHandlers.ToList().AsReadOnly();
    }
  }
}

namespace Xtensive.Orm.Tests.Model.HierarchyRootValidationTestModel
{
  namespace NonPersistentStorageSupporedTypesAsKeys
  {
    [HierarchyRoot]
    public class ByteKeyEntity : Entity
    {
      [Field]
      [Key]
      public byte Id { get; private set; }

      [Field]
      public int Value { get; set; }

      public ByteKeyEntity(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class Int16KeyEntity : Entity
    {
      [Field]
      [Key]
      public short Id { get; private set; }

      [Field]
      public int Value { get; set; }

      public Int16KeyEntity(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class UInt16KeyEntity : Entity
    {
      [Field]
      [Key]
      public ushort Id { get; private set; }

      [Field]
      public int Value { get; set; }

      public UInt16KeyEntity(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class Int32KeyEntity : Entity
    {
      [Field]
      [Key]
      public int Id { get; private set; }

      [Field]
      public int Value { get; set; }

      public Int32KeyEntity(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class UInt32KeyEntity : Entity
    {
      [Field]
      [Key]
      public uint Id { get; private set; }

      [Field]
      public int Value { get; set; }

      public UInt32KeyEntity(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class Int64KeyEntity : Entity
    {
      [Field]
      [Key]
      public long Id { get; private set; }

      [Field]
      public int Value { get; set; }

      public Int64KeyEntity(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class UInt64KeyEntity : Entity
    {
      [Field]
      [Key]
      public ulong Id { get; private set; }

      [Field]
      public int Value { get; set; }

      public UInt64KeyEntity(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class FloatKeyEntity : Entity
    {
      [Field]
      [Key]
      public float Id { get; private set; }

      [Field]
      public int Value { get; set; }

      public FloatKeyEntity(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class DoubleKeyEntity : Entity
    {
      [Field]
      [Key]
      public double Id { get; private set; }

      [Field]
      public int Value { get; set; }

      public DoubleKeyEntity(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class DecimalKeyEntity : Entity
    {
      [Field(Precision = 12, Scale = 0)]
      [Key]
      public decimal Id { get; private set; }

      [Field]
      public int Value { get; set; }

      public DecimalKeyEntity(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class FractionalDecimalKeyEntity : Entity
    {
      [Field(Precision = 12, Scale = 3)]
      [Key]
      public decimal Id { get; private set; }

      [Field]
      public int Value { get; set; }

      public FractionalDecimalKeyEntity(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class TimeSpanKeyEntity : Entity
    {
      [Field]
      [Key]
      public TimeSpan Id { get; private set; }

      [Field]
      public int Value { get; set; }

      public TimeSpanKeyEntity(Session session)
        : base(session)
      {
      }
    }

#if NET6_0_OR_GREATER

    [HierarchyRoot]
    public class TimeOnlyKeyEntity : Entity
    {
      [Field]
      [Key]
      public TimeOnly Id { get; private set; }

      [Field]
      public int Value { get; set; }

      public TimeOnlyKeyEntity(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class DateOnlyKeyEntity : Entity
    {
      [Field]
      [Key]
      public DateOnly Id { get; private set; }

      [Field]
      public int Value { get; set; }

      public DateOnlyKeyEntity(Session session)
        : base(session)
      {
      }
    }

#endif

    [HierarchyRoot]
    public class DateTimeKeyEntity : Entity
    {
      [Field]
      [Key]
      public DateTime Id { get; private set; }

      [Field]
      public int Value { get; set; }

      public DateTimeKeyEntity(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class DateTimeOffsetKeyEntity : Entity
    {
      [Field]
      [Key]
      public DateTimeOffset Id { get; private set; }

      [Field]
      public int Value { get; set; }

      public DateTimeOffsetKeyEntity(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class CharKeyEntity : Entity
    {
      [Field]
      [Key]
      public char Id { get; private set; }

      [Field]
      public int Value { get; set; }

      public CharKeyEntity(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class LimitedStringKeyEntity : Entity
    {
      [Field(Length = 255)]
      [Key]
      public string Id { get; private set; }

      [Field]
      public int Value { get; set; }

      public LimitedStringKeyEntity(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class StringKeyEntity : Entity
    {
      [Field]
      [Key]
      public string Id { get; private set; }

      [Field]
      public int Value { get; set; }

      public StringKeyEntity(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class GuidKeyEntity : Entity
    {
      [Field]
      [Key]
      public Guid Id { get; private set; }

      [Field]
      public int Value { get; set; }

      public GuidKeyEntity(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class ByteArrayKeyEntity : Entity
    {
      [Field]
      [Key]
      public byte[] Id { get; private set; }

      [Field]
      public int Value { get; set; }

      public ByteArrayKeyEntity(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class IntArrayKeyEntity : Entity
    {
      [Field]
      [Key]
      public int[] Id { get; private set; }

      [Field]
      public int Value { get; set; }

      public IntArrayKeyEntity(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class ObjectKeyEntity : Entity
    {
      [Field]
      [Key]
      public object Id { get; private set; }

      [Field]
      public int Value { get; set; }

      public ObjectKeyEntity(Session session)
        : base(session)
      {
      }
    }
  }

  namespace NonPersistentComplexKey
  {
    [HierarchyRoot]
    public class ByteKeyEntity : Entity
    {
      [Field]
      [Key(0)]
      public byte Id1 { get; private set; }

      [Field]
      [Key(1)]
      public byte Id2 { get; private set; }

      [Field]
      public int Value { get; set; }

      public ByteKeyEntity(Session session)
        : base(session)
      {
      }
    }
  }

  namespace LazyLoadKey
  {
    [Service(typeof(KeyGenerator), Name = nameof(SingleKeyGenerator))]
    public sealed class SingleKeyGenerator : KeyGenerator
    {
      private int counter1;

      public override void Initialize(Domain ownerDomain, TupleDescriptor keyTupleDescriptor)
      {
      }

      public override Tuples.Tuple GenerateKey(KeyInfo keyInfo, Session session)
      {
        return Tuples.Tuple.Create(counter1++);
      }
    }

    [Service(typeof(KeyGenerator), Name = nameof(ComplexKeyGenerator))]
    public sealed class ComplexKeyGenerator : KeyGenerator
    {
      private int counter1;

      public override void Initialize(Domain ownerDomain, TupleDescriptor keyTupleDescriptor)
      {
      }

      public override Tuples.Tuple GenerateKey(KeyInfo keyInfo, Session session)
      {
        var now = DateTime.UtcNow;
        return Tuples.Tuple.Create(counter1++, now.Minute);
      }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.Default)] // equals no attribute at all, added for clearance
    public class DefaultGeneratorSingleKeyEntity : Entity
    {
      [Field(LazyLoad = true)]
      [Key]
      public int Id { get; private set; }

      [Field]
      public string Name { get; set; }
    }

    [HierarchyRoot]
    [KeyGenerator(Name = nameof(NamedDefaultGeneratorSingleKeyEntity))] // pretty much default generator
    public class NamedDefaultGeneratorSingleKeyEntity : Entity
    {
      [Field(LazyLoad = true)]
      [Key]
      public int Id { get; private set; }

      [Field]
      public string Name { get; set; }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.Custom, Name = nameof(SingleKeyGenerator))]
    public class CustomGeneratorSingleKeyEntity : Entity
    {
      [Field(LazyLoad = true)]
      [Key]
      public int Id { get; private set; }

      [Field]
      public string Name { get; set; }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.None)]
    public class NoGeneratorSingleKeyEntity : Entity
    {
      [Field(LazyLoad = true)]
      [Key]
      public int Id { get; private set; }

      [Field]
      public string Name { get; set; }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.Default)] // equals no attribute at all, added for clearance
    public class DefaultGeneratorComplexKeyEntity : Entity
    {
      [Field]
      [Key(0)]
      public int Id1 { get; private set; }

      [Field(LazyLoad = true)]
      [Key(1)]
      public int Id2 { get; private set; }

      [Field]
      public string Name { get; set; }
    }

    [HierarchyRoot]
    [KeyGenerator(Name = nameof(NamedDefaultGeneratorSingleKeyEntity))] // pretty much default generator
    public class NamedDefaultGeneratorComplexKeyEntity : Entity
    {
      [Field]
      [Key(0)]
      public int Id1 { get; private set; }

      [Field(LazyLoad = true)]
      [Key(1)]
      public int Id2 { get; private set; }

      [Field]
      public string Name { get; set; }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.Custom, Name = nameof(ComplexKeyGenerator))]
    public class CustomGeneratorComplexKeyEntity : Entity
    {
      [Field]
      [Key(0)]
      public int Id1 { get; private set; }

      [Field(LazyLoad = true)]
      [Key(1)]
      public int Id2 { get; private set; }

      [Field]
      public string Name { get; set; }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.None)]
    public class NoGeneratorComplexKeyEntity : Entity
    {
      [Field]
      [Key(0)]
      public int Id1 { get; private set; }

      [Field(LazyLoad = true)]
      [Key(1)]
      public int Id2 { get; private set; }

      [Field]
      public string Name { get; set; }
    }
  }

  namespace EntityAsKey
  {
    [Service(typeof(KeyGenerator), Name = nameof(SingleKeyGenerator))]
    public sealed class SingleKeyGenerator : KeyGenerator
    {
      private int counter1;

      public override void Initialize(Domain ownerDomain, TupleDescriptor keyTupleDescriptor)
      {
      }

      public override Tuples.Tuple GenerateKey(KeyInfo keyInfo, Session session)
      {
        return Tuples.Tuple.Create(counter1++);
      }
    }

    [Service(typeof(KeyGenerator), Name = nameof(ComplexKeyGenerator))]
    public sealed class ComplexKeyGenerator : KeyGenerator
    {
      private int counter1;

      public override void Initialize(Domain ownerDomain, TupleDescriptor keyTupleDescriptor)
      {
      }

      public override Tuples.Tuple GenerateKey(KeyInfo keyInfo, Session session)
      {
        var now = DateTime.UtcNow;
        return Tuples.Tuple.Create(counter1++, now.Minute);
      }
    }

    [HierarchyRoot]
    public class SomeEntity : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      public SomeEntity(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.Default)] // equals no attribute at all, added for clearance
    public class DefaultGeneratorSingleKeyEntity : Entity
    {
      [Field]
      [Key]
      public SomeEntity Id { get; private set; }

      [Field]
      public string Name { get; set; }

      public DefaultGeneratorSingleKeyEntity(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    [KeyGenerator(Name = nameof(NamedDefaultGeneratorSingleKeyEntity))] // pretty much default generator
    public class NamedDefaultGeneratorSingleKeyEntity : Entity
    {
      [Field]
      [Key]
      public SomeEntity Id { get; private set; }

      [Field]
      public string Name { get; set; }

      public NamedDefaultGeneratorSingleKeyEntity(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.Custom, Name = nameof(SingleKeyGenerator))]
    public class CustomGeneratorSingleKeyEntity : Entity
    {
      [Field]
      [Key]
      public SomeEntity Id { get; private set; }

      [Field]
      public string Name { get; set; }

      public CustomGeneratorSingleKeyEntity(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.None)]
    public class NoGeneratorSingleKeyEntity : Entity
    {
      [Field]
      [Key]
      public SomeEntity Id { get; private set; }

      [Field]
      public string Name { get; set; }

      public NoGeneratorSingleKeyEntity(Session session)
        : base(session)
      {
      }

      public NoGeneratorSingleKeyEntity(Session session, SomeEntity id)
        : base(session, id)
      {
      }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.Default)] // equals no attribute at all, added for clearance
    public class DefaultGeneratorComplexKeyEntity : Entity
    {
      [Field]
      [Key(0)]
      public int Id1 { get; private set; }

      [Field]
      [Key(1)]
      public SomeEntity Id2 { get; private set; }

      [Field]
      public string Name { get; set; }

      public DefaultGeneratorComplexKeyEntity(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    [KeyGenerator(Name = nameof(NamedDefaultGeneratorSingleKeyEntity))] // pretty much default generator
    public class NamedDefaultGeneratorComplexKeyEntity : Entity
    {
      [Field]
      [Key(0)]
      public int Id1 { get; private set; }

      [Field]
      [Key(1)]
      public SomeEntity Id2 { get; private set; }

      [Field]
      public string Name { get; set; }

      public NamedDefaultGeneratorComplexKeyEntity(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.Custom, Name = nameof(ComplexKeyGenerator))]
    public class CustomGeneratorComplexKeyEntity : Entity
    {
      [Field]
      [Key(0)]
      public int Id1 { get; private set; }

      [Field]
      [Key(1)]
      public SomeEntity Id2 { get; private set; }

      [Field]
      public string Name { get; set; }

      public CustomGeneratorComplexKeyEntity(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.None)]
    public class NoGeneratorComplexKeyEntity : Entity
    {
      [Field]
      [Key(0)]
      public int Id1 { get; private set; }

      [Field]
      [Key(1)]
      public SomeEntity Id2 { get; private set; }

      [Field]
      public string Name { get; set; }

      public NoGeneratorComplexKeyEntity(Session session)
        : base(session)
      {
      }

      public NoGeneratorComplexKeyEntity(Session session, int id1, SomeEntity id2)
        : base(session, id1, id2)
      {
      }
    }
  }

  namespace PersistentInterfaceAsKey
  {
    [Service(typeof(KeyGenerator), Name = nameof(SingleKeyGenerator))]
    public sealed class SingleKeyGenerator : KeyGenerator
    {
      private int counter1;

      public override void Initialize(Domain ownerDomain, TupleDescriptor keyTupleDescriptor)
      {
      }

      public override Tuples.Tuple GenerateKey(KeyInfo keyInfo, Session session)
      {
        return Tuples.Tuple.Create(counter1++);
      }
    }

    [Service(typeof(KeyGenerator), Name = nameof(ComplexKeyGenerator))]
    public sealed class ComplexKeyGenerator : KeyGenerator
    {
      private int counter1;

      public override void Initialize(Domain ownerDomain, TupleDescriptor keyTupleDescriptor)
      {
      }

      public override Tuples.Tuple GenerateKey(KeyInfo keyInfo, Session session)
      {
        var now = DateTime.UtcNow;
        return Tuples.Tuple.Create(counter1++, now.Minute);
      }
    }

    public interface ISomeEntity : IEntity
    {
      [Field]
      int Id { get; }

      [Field]
      string Name { get; set; }
    }

    [HierarchyRoot]
    public class SomeEntity : Entity, ISomeEntity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Name { get; set; }

      public SomeEntity(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.Default)] // equals no attribute at all, added for clearance
    public class DefaultGeneratorSingleKeyEntity : Entity
    {
      [Field]
      [Key]
      public ISomeEntity Id { get; private set; }

      [Field]
      public string Name { get; set; }

      public DefaultGeneratorSingleKeyEntity(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    [KeyGenerator(Name = nameof(NamedDefaultGeneratorSingleKeyEntity))] // pretty much default generator
    public class NamedDefaultGeneratorSingleKeyEntity : Entity
    {
      [Field]
      [Key]
      public ISomeEntity Id { get; private set; }

      [Field]
      public string Name { get; set; }

      public NamedDefaultGeneratorSingleKeyEntity(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.Custom, Name = nameof(SingleKeyGenerator))]
    public class CustomGeneratorSingleKeyEntity : Entity
    {
      [Field]
      [Key]
      public ISomeEntity Id { get; private set; }

      [Field]
      public string Name { get; set; }

      public CustomGeneratorSingleKeyEntity(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.None)]
    public class NoGeneratorSingleKeyEntity : Entity
    {
      [Field]
      [Key]
      public ISomeEntity Id { get; private set; }

      [Field]
      public string Name { get; set; }

      public NoGeneratorSingleKeyEntity(Session session)
        : base(session)
      {
      }

      public NoGeneratorSingleKeyEntity(Session session, ISomeEntity id)
        : base(session, id)
      {
      }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.Default)] // equals no attribute at all, added for clearance
    public class DefaultGeneratorComplexKeyEntity : Entity
    {
      [Field]
      [Key(0)]
      public int Id1 { get; private set; }

      [Field]
      [Key(1)]
      public ISomeEntity Id2 { get; private set; }

      [Field]
      public string Name { get; set; }

      public DefaultGeneratorComplexKeyEntity(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    [KeyGenerator(Name = nameof(NamedDefaultGeneratorSingleKeyEntity))] // pretty much default generator
    public class NamedDefaultGeneratorComplexKeyEntity : Entity
    {
      [Field]
      [Key(0)]
      public int Id1 { get; private set; }

      [Field]
      [Key(1)]
      public ISomeEntity Id2 { get; private set; }

      [Field]
      public string Name { get; set; }

      public NamedDefaultGeneratorComplexKeyEntity(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.Custom, Name = nameof(ComplexKeyGenerator))]
    public class CustomGeneratorComplexKeyEntity : Entity
    {
      [Field]
      [Key(0)]
      public int Id1 { get; private set; }

      [Field]
      [Key(1)]
      public ISomeEntity Id2 { get; private set; }

      [Field]
      public string Name { get; set; }

      public CustomGeneratorComplexKeyEntity(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.None)]
    public class NoGeneratorComplexKeyEntity : Entity
    {
      [Field]
      [Key(0)]
      public int Id1 { get; private set; }

      [Field]
      [Key(1)]
      public ISomeEntity Id2 { get; private set; }

      [Field]
      public string Name { get; set; }

      public NoGeneratorComplexKeyEntity(Session session)
        : base(session)
      {
      }

      public NoGeneratorComplexKeyEntity(Session session, int id1, ISomeEntity id2)
        : base(session, id1, id2)
      {
      }
    }
  }

  namespace EntitySetAsKey
  {
    [Service(typeof(KeyGenerator), Name = nameof(SingleKeyGenerator))]
    public sealed class SingleKeyGenerator : KeyGenerator
    {
      private int counter1;

      public override void Initialize(Domain ownerDomain, TupleDescriptor keyTupleDescriptor)
      {
      }

      public override Tuples.Tuple GenerateKey(KeyInfo keyInfo, Session session)
      {
        return Tuples.Tuple.Create(counter1++);
      }
    }

    [Service(typeof(KeyGenerator), Name = nameof(ComplexKeyGenerator))]
    public sealed class ComplexKeyGenerator : KeyGenerator
    {
      private int counter1;

      public override void Initialize(Domain ownerDomain, TupleDescriptor keyTupleDescriptor)
      {
      }

      public override Tuples.Tuple GenerateKey(KeyInfo keyInfo, Session session)
      {
        var now = DateTime.UtcNow;
        return Tuples.Tuple.Create(counter1++, now.Minute);
      }
    }

    [HierarchyRoot]
    public class SomeEntity : Entity
    {
      [Field, Key]
      public int Id { get; private set; }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.Default)] // equals no attribute at all, added for clearance
    public class DefaultGeneratorSingleKeyEntity : Entity
    {
      [Field]
      [Key]
      public EntitySet<SomeEntity> Id { get; private set; }

      [Field]
      public string Name { get; set; }
    }

    [HierarchyRoot]
    [KeyGenerator(Name = nameof(NamedDefaultGeneratorSingleKeyEntity))] // pretty much default generator
    public class NamedDefaultGeneratorSingleKeyEntity : Entity
    {
      [Field]
      [Key]
      public EntitySet<SomeEntity> Id { get; private set; }

      [Field]
      public string Name { get; set; }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.Custom, Name = nameof(SingleKeyGenerator))]
    public class CustomGeneratorSingleKeyEntity : Entity
    {
      [Field]
      [Key]
      public EntitySet<SomeEntity> Id { get; private set; }

      [Field]
      public string Name { get; set; }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.None)]
    public class NoGeneratorSingleKeyEntity : Entity
    {
      [Field]
      [Key]
      public EntitySet<SomeEntity> Id { get; private set; }

      [Field]
      public string Name { get; set; }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.Default)] // equals no attribute at all, added for clearance
    public class DefaultGeneratorComplexKeyEntity : Entity
    {
      [Field]
      [Key(0)]
      public int Id1 { get; private set; }

      [Field]
      [Key(1)]
      public EntitySet<SomeEntity> Id2 { get; private set; }

      [Field]
      public string Name { get; set; }
    }

    [HierarchyRoot]
    [KeyGenerator(Name = nameof(NamedDefaultGeneratorSingleKeyEntity))] // pretty much default generator
    public class NamedDefaultGeneratorComplexKeyEntity : Entity
    {
      [Field]
      [Key(0)]
      public int Id1 { get; private set; }

      [Field]
      [Key(1)]
      public EntitySet<SomeEntity> Id2 { get; private set; }

      [Field]
      public string Name { get; set; }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.Custom, Name = nameof(ComplexKeyGenerator))]
    public class CustomGeneratorComplexKeyEntity : Entity
    {
      [Field]
      [Key(0)]
      public int Id1 { get; private set; }

      [Field]
      [Key(1)]
      public EntitySet<SomeEntity> Id2 { get; private set; }

      [Field]
      public string Name { get; set; }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.None)]
    public class NoGeneratorComplexKeyEntity : Entity
    {
      [Field]
      [Key(0)]
      public int Id1 { get; private set; }

      [Field]
      [Key(1)]
      public EntitySet<SomeEntity> Id2 { get; private set; }

      [Field]
      public string Name { get; set; }
    }
  }

  namespace IEntityAsKey
  {
    [Service(typeof(KeyGenerator), Name = nameof(SingleKeyGenerator))]
    public sealed class SingleKeyGenerator : KeyGenerator
    {
      private int counter1;

      public override void Initialize(Domain ownerDomain, TupleDescriptor keyTupleDescriptor)
      {
      }

      public override Tuples.Tuple GenerateKey(KeyInfo keyInfo, Session session)
      {
        return Tuples.Tuple.Create(counter1++);
      }
    }

    [Service(typeof(KeyGenerator), Name = nameof(ComplexKeyGenerator))]
    public sealed class ComplexKeyGenerator : KeyGenerator
    {
      private int counter1;

      public override void Initialize(Domain ownerDomain, TupleDescriptor keyTupleDescriptor)
      {
      }

      public override Tuples.Tuple GenerateKey(KeyInfo keyInfo, Session session)
      {
        var now = DateTime.UtcNow;
        return Tuples.Tuple.Create(counter1++, now.Minute);
      }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.Default)] // equals no attribute at all, added for clearance
    public class DefaultGeneratorSingleKeyEntity : Entity
    {
      [Field]
      [Key]
      public IEntity Id { get; private set; }

      [Field]
      public string Name { get; set; }
    }

    [HierarchyRoot]
    [KeyGenerator(Name = nameof(NamedDefaultGeneratorSingleKeyEntity))] // pretty much default generator
    public class NamedDefaultGeneratorSingleKeyEntity : Entity
    {
      [Field]
      [Key]
      public IEntity Id { get; private set; }

      [Field]
      public string Name { get; set; }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.Custom, Name = nameof(SingleKeyGenerator))]
    public class CustomGeneratorSingleKeyEntity : Entity
    {
      [Field]
      [Key]
      public IEntity Id { get; private set; }

      [Field]
      public string Name { get; set; }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.None)]
    public class NoGeneratorSingleKeyEntity : Entity
    {
      [Field]
      [Key]
      public IEntity Id { get; private set; }

      [Field]
      public string Name { get; set; }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.Default)] // equals no attribute at all, added for clearance
    public class DefaultGeneratorComplexKeyEntity : Entity
    {
      [Field]
      [Key(0)]
      public int Id1 { get; private set; }

      [Field]
      [Key(1)]
      public IEntity Id2 { get; private set; }

      [Field]
      public string Name { get; set; }
    }

    [HierarchyRoot]
    [KeyGenerator(Name = nameof(NamedDefaultGeneratorSingleKeyEntity))] // pretty much default generator
    public class NamedDefaultGeneratorComplexKeyEntity : Entity
    {
      [Field]
      [Key(0)]
      public int Id1 { get; private set; }

      [Field]
      [Key(1)]
      public IEntity Id2 { get; private set; }

      [Field]
      public string Name { get; set; }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.Custom, Name = nameof(ComplexKeyGenerator))]
    public class CustomGeneratorComplexKeyEntity : Entity
    {
      [Field]
      [Key(0)]
      public int Id1 { get; private set; }

      [Field]
      [Key(1)]
      public IEntity Id2 { get; private set; }

      [Field]
      public string Name { get; set; }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.None)]
    public class NoGeneratorComplexKeyEntity : Entity
    {
      [Field]
      [Key(0)]
      public int Id1 { get; private set; }

      [Field]
      [Key(1)]
      public IEntity Id2 { get; private set; }

      [Field]
      public string Name { get; set; }
    }
  }

  namespace StructureAsKey
  {
    [Service(typeof(KeyGenerator), Name = nameof(SingleKeyGenerator))]
    public sealed class SingleKeyGenerator : KeyGenerator
    {
      private int counter1;

      public override void Initialize(Domain ownerDomain, TupleDescriptor keyTupleDescriptor)
      {
      }

      public override Tuples.Tuple GenerateKey(KeyInfo keyInfo, Session session)
      {
        return Tuples.Tuple.Create(counter1++);
      }
    }

    [Service(typeof(KeyGenerator), Name = nameof(ComplexKeyGenerator))]
    public sealed class ComplexKeyGenerator : KeyGenerator
    {
      private int counter1;

      public override void Initialize(Domain ownerDomain, TupleDescriptor keyTupleDescriptor)
      {
      }

      public override Tuples.Tuple GenerateKey(KeyInfo keyInfo, Session session)
      {
        var now = DateTime.UtcNow;
        return Tuples.Tuple.Create(counter1++, now.Minute);
      }
    }

    public class Point : Structure
    {
      [Field]
      public int X { get; set; }

      [Field]
      public int Y { get; set; }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.Default)] // equals no attribute at all, added for clearance
    public class DefaultGeneratorSingleKeyEntity : Entity
    {
      [Field]
      [Key]
      public Point Id { get; private set; }

      [Field]
      public string Name { get; set; }
    }

    [HierarchyRoot]
    [KeyGenerator(Name = nameof(NamedDefaultGeneratorSingleKeyEntity))] // pretty much default generator
    public class NamedDefaultGeneratorSingleKeyEntity : Entity
    {
      [Field]
      [Key]
      public Point Id { get; private set; }

      [Field]
      public string Name { get; set; }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.Custom, Name = nameof(SingleKeyGenerator))]
    public class CustomGeneratorSingleKeyEntity : Entity
    {
      [Field]
      [Key]
      public Point Id { get; private set; }

      [Field]
      public string Name { get; set; }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.None)]
    public class NoGeneratorSingleKeyEntity : Entity
    {
      [Field]
      [Key]
      public Point Id { get; private set; }

      [Field]
      public string Name { get; set; }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.Default)] // equals no attribute at all, added for clearance
    public class DefaultGeneratorComplexKeyEntity : Entity
    {
      [Field]
      [Key(0)]
      public int Id1 { get; private set; }

      [Field]
      [Key(1)]
      public Point Id2 { get; private set; }

      [Field]
      public string Name { get; set; }
    }

    [HierarchyRoot]
    [KeyGenerator(Name = nameof(NamedDefaultGeneratorSingleKeyEntity))] // pretty much default generator
    public class NamedDefaultGeneratorComplexKeyEntity : Entity
    {
      [Field]
      [Key(0)]
      public int Id1 { get; private set; }

      [Field]
      [Key(1)]
      public Point Id2 { get; private set; }

      [Field]
      public string Name { get; set; }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.Custom, Name = nameof(ComplexKeyGenerator))]
    public class CustomGeneratorComplexKeyEntity : Entity
    {
      [Field]
      [Key(0)]
      public int Id1 { get; private set; }

      [Field]
      [Key(1)]
      public Point Id2 { get; private set; }

      [Field]
      public string Name { get; set; }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.None)]
    public class NoGeneratorComplexKeyEntity : Entity
    {
      [Field]
      [Key(0)]
      public int Id1 { get; private set; }

      [Field]
      [Key(1)]
      public Point Id2 { get; private set; }

      [Field]
      public string Name { get; set; }
    }
  }

  namespace NoKeyFieldsHierarchy
  {
    [HierarchyRoot]
    public class NoKeyFieldsEntity : Entity
    {
      [Field]
      //No Key Attribute!
      public int Id { get; private set; }

      [Field]
      public string Name { get; set; }
    }
  }

  namespace KeyNotInRoot
  {
    [Service(typeof(KeyGenerator), Name = nameof(SingleKeyGenerator))]
    public sealed class SingleKeyGenerator : KeyGenerator
    {
      private int counter1;

      public override void Initialize(Domain ownerDomain, TupleDescriptor keyTupleDescriptor)
      {
      }

      public override Tuples.Tuple GenerateKey(KeyInfo keyInfo, Session session)
      {
        return Tuples.Tuple.Create(counter1++);
      }
    }

    [Service(typeof(KeyGenerator), Name = nameof(ComplexKeyGenerator))]
    public sealed class ComplexKeyGenerator : KeyGenerator
    {
      private int counter1;

      public override void Initialize(Domain ownerDomain, TupleDescriptor keyTupleDescriptor)
      {
      }

      public override Tuples.Tuple GenerateKey(KeyInfo keyInfo, Session session)
      {
        var now = DateTime.UtcNow;
        return Tuples.Tuple.Create(counter1++, now.Minute);
      }
    }


    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.Default)] // equals no attribute at all, added for clearance
    public class DefaultGeneratorSingleKeyEntityRoot : Entity
    {
      [Field]
      public string Name { get; set; }
    }

    public class DefaultGeneratorSingleKeyEntityLeaf : DefaultGeneratorSingleKeyEntityRoot
    {
      [Field]
      [Key]
      public int Id { get; private set; }

      [Field]
      public string Name2 { get; set; }
    }

    [HierarchyRoot]
    [KeyGenerator(Name = nameof(NamedDefaultGeneratorSingleKeyEntityRoot))] // pretty much default generator
    public class NamedDefaultGeneratorSingleKeyEntityRoot : Entity
    {
      [Field]
      public string Name { get; set; }
    }

    public class NamedDefaultGeneratorSingleKeyEntityLeaf : NamedDefaultGeneratorSingleKeyEntityRoot
    {
      [Field]
      [Key]
      public int Id { get; private set; }

      [Field]
      public string Name2 { get; set; }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.Custom, Name = nameof(SingleKeyGenerator))]
    public class CustomGeneratorSingleKeyEntityRoot : Entity
    {
      [Field]
      public string Name { get; set; }
    }

    public class CustomGeneratorSingleKeyEntityLeaf : CustomGeneratorSingleKeyEntityRoot
    {
      [Field]
      [Key]
      public int Id { get; private set; }

      [Field]
      public string Name2 { get; set; }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.None)]
    public class NoGeneratorSingleKeyEntityRoot : Entity
    {
      [Field]
      public string Name { get; set; }
    }

    public class NoGeneratorSingleKeyEntityLeaf : NoGeneratorSingleKeyEntityRoot
    {
      [Field]
      [Key]
      public int Id { get; private set; }

      [Field]
      public string Name2 { get; set; }
    }
  }

  namespace TwoKeys
  {
    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.Default)] // equals no attribute at all, added for clearance
    public class DefaultGeneratorEntity : Entity
    {
      [Field]
      [Key(0)]
      public int Id1 { get; private set; }

      [Field]
      [Key(1)]
      public int Id2 { get; private set; }

      [Field]
      public string Name { get; set; }

      public DefaultGeneratorEntity(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    [KeyGenerator(Name = nameof(NamedGeneratorEntity))] // still defaut key generator capabilities but dedicated instance
    public class NamedGeneratorEntity : Entity
    {
      [Field]
      [Key(0)]
      public int Id1 { get; private set; }

      [Field]
      [Key(1)]
      public int Id2 { get; private set; }

      [Field]
      public string Name { get; set; }

      public NamedGeneratorEntity(Session session)
        : base(session)
      {
      }
    }

    [Service(typeof(KeyGenerator), Name = nameof(TwoKeyFieldsGen))]
    public sealed class TwoKeyFieldsGen : KeyGenerator
    {
      private int counter1;

      public override void Initialize(Domain ownerDomain, TupleDescriptor keyTupleDescriptor)
      {
      }

      public override Tuples.Tuple GenerateKey(KeyInfo keyInfo, Session session)
      {
        return Tuples.Tuple.Create(counter1++, DateTime.UtcNow.Minute);
      }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.Custom, Name = nameof(TwoKeyFieldsGen))]
    public class CustomGeneratorEntity : Entity
    {
      [Field]
      [Key(0)]
      public int Id1 { get; private set; }

      [Field]
      [Key(1)]
      public int Id2 { get; private set; }

      [Field]
      public string Name { get; set; }

      public CustomGeneratorEntity(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.None)]
    public class NoGeneratorEntity : Entity
    {
      [Field]
      [Key(0)]
      public int Id1 { get; private set; }

      [Field]
      [Key(1)]
      public int Id2 { get; private set; }

      [Field]
      public string Name { get; set; }

      public NoGeneratorEntity(Session session, int id1, int id2)
        : base(session, id1, id2)
      {
      }
    }
  }

  namespace SingleKeyWithTypeId
  {
    [HierarchyRoot(IncludeTypeId = true)] // this setting doesn't cause addtional keyField on HierarchyDef level
    [KeyGenerator(KeyGeneratorKind.Default)] // equals no attribute at all, added for clearance
    public class DefaultGeneratorEntity : Entity
    {
      [Field]
      [Key]
      public int Id { get; private set; }

      [Field]
      public string Name { get; set; }

      public DefaultGeneratorEntity(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot(IncludeTypeId = true)] // this setting doesn't cause addtional keyField on HierarchyDef level
    [KeyGenerator(Name = nameof(NamedGeneratorEntity))] // pretty much default generator
    public class NamedGeneratorEntity : Entity
    {
      [Field]
      [Key]
      public int Id { get; private set; }

      [Field]
      public string Name { get; set; }

      public NamedGeneratorEntity(Session session)
        : base(session)
      {
      }
    }

    [Service(typeof(KeyGenerator), Name = nameof(SingleKeyGenerator))]
    public sealed class SingleKeyGenerator : KeyGenerator
    {
      private int counter1;

      public override void Initialize(Domain ownerDomain, TupleDescriptor keyTupleDescriptor)
      {
      }

      public override Tuples.Tuple GenerateKey(KeyInfo keyInfo, Session session)
      {
        if (keyInfo.TypeIdColumnIndex >= 0) {
          return Tuples.Tuple.Create(counter1++, -1);// -1 will be replaced with actual TypeId value
        }
        return Tuples.Tuple.Create(counter1++);
      }
    }

    [HierarchyRoot(IncludeTypeId = true)] // this setting doesn't cause addtional keyField on HierarchyDef level
    [KeyGenerator(KeyGeneratorKind.Custom, Name = nameof(SingleKeyGenerator))]
    public class CustomGeneratorEntity : Entity
    {
      [Field]
      [Key]
      public int Id { get; private set; }

      [Field]
      public string Name { get; set; }

      public CustomGeneratorEntity(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot(IncludeTypeId = true)] // this setting doesn't cause addtional keyField on HierarchyDef level
    [KeyGenerator(KeyGeneratorKind.None)]
    public class NoGeneratorEntity : Entity
    {
      [Field]
      [Key]
      public int Id { get; private set; }

      [Field]
      public string Name { get; set; }

      public NoGeneratorEntity(Session session, int id)
        : base(session, id)
      {
      }
    }
  }

  namespace ThreeKeys
  {
    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.Default)] // equals no attribute at all, added for clearance
    public class DefaultGeneratorEntity : Entity
    {
      [Field]
      [Key(0)]
      public int Id1 { get; private set; }

      [Field]
      [Key(1)]
      public int Id2 { get; private set; }

      [Field]
      [Key(2)]
      public int Id3 { get; private set; }

      [Field]
      public string Name { get; set; }

      public DefaultGeneratorEntity(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    [KeyGenerator(Name = nameof(NamedGeneratorEntity))] // still defaut key generator capabilities but dedicated instance
    public class NamedGeneratorEntity : Entity
    {
      [Field]
      [Key(0)]
      public int Id1 { get; private set; }

      [Field]
      [Key(1)]
      public int Id2 { get; private set; }

      [Field]
      [Key(2)]
      public int Id3 { get; private set; }

      [Field]
      public string Name { get; set; }

      public NamedGeneratorEntity(Session session)
        : base(session)
      {
      }
    }

    [Service(typeof(KeyGenerator), Name = nameof(TwoKeyFieldsGen))]
    public sealed class TwoKeyFieldsGen : KeyGenerator
    {
      private int counter1;

      public override void Initialize(Domain ownerDomain, TupleDescriptor keyTupleDescriptor)
      {
      }

      public override Tuples.Tuple GenerateKey(KeyInfo keyInfo, Session session)
      {
        var utcNow = DateTime.UtcNow;
        return Tuples.Tuple.Create(counter1++, utcNow.Minute, utcNow.Second);
      }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.Custom, Name = nameof(TwoKeyFieldsGen))]
    public class CustomGeneratorEntity : Entity
    {
      [Field]
      [Key(0)]
      public int Id1 { get; private set; }

      [Field]
      [Key(1)]
      public int Id2 { get; private set; }

      [Field]
      [Key(2)]
      public int Id3 { get; private set; }

      [Field]
      public string Name { get; set; }

      public CustomGeneratorEntity(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.None)]
    public class NoGeneratorEntity : Entity
    {
      [Field]
      [Key(0)]
      public int Id1 { get; private set; }

      [Field]
      [Key(1)]
      public int Id2 { get; private set; }

      [Field]
      [Key(2)]
      public int Id3 { get; private set; }

      [Field]
      public string Name { get; set; }

      public NoGeneratorEntity(Session session, int id1, int id2, int id3)
        : base(session, id1, id2, id3)
      {
      }
    }
  }

  namespace IModuleModifications
  {
    public class ModelDefModifier : IModule2
    {
      public void OnAutoGenericsBuilt(BuildingContext context, ICollection<Type> autoGenerics)
      {

      }
      public void OnBuilt(Domain domain)
      {

      }
      public void OnDefinitionsBuilt(BuildingContext context, DomainModelDef model)
      {
        var manualIncludeTypeIdHierarchy = model.Hierarchies.TryGetValue(typeof(ManualIncludeTypeIdEntity));
        if (manualIncludeTypeIdHierarchy is not null) {
          var rootType = manualIncludeTypeIdHierarchy.Root;
          manualIncludeTypeIdHierarchy.KeyFields.Add(new KeyField(WellKnown.TypeIdFieldName));
        }

        var includeTypeIdHierarchy = model.Hierarchies.TryGetValue(typeof(IncludeTypeIdEntity));
        if (includeTypeIdHierarchy is not null) {
          includeTypeIdHierarchy.IncludeTypeId = true;
        }
      }
    }

    [HierarchyRoot]
    public class ManualIncludeTypeIdEntity : Entity
    {
      [Field]
      [Key]
      public int Id { get; private set; }

      [Field]
      public string Name { get; set; }

      public ManualIncludeTypeIdEntity(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class IncludeTypeIdEntity : Entity
    {
      [Field]
      [Key]
      public int Id { get; private set; }

      [Field]
      public string Name { get; set; }

      public IncludeTypeIdEntity(Session session)
        : base(session)
      {
      }
    }
  }
}
