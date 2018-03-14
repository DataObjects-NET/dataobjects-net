// Copyright (C) 2018 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2018.03.02

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.VersioningConventionTestModel;

namespace Xtensive.Orm.Tests.Storage
{
  public abstract class EntityVersioningPolicyTest : AutoBuildTest
  {
    protected bool IsOptimisticPolicy
    {
      get { return Domain.Configuration.VersioningConvention.EntityVersioningPolicy==EntityVersioningPolicy.Optimistic; }
    }

    protected abstract void ApplyVersioningPolicy(DomainConfiguration configuration);

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof (TestStructure).Assembly, typeof (TestStructure).Namespace);
      ApplyVersioningPolicy(configuration);
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new SimpleTypesFieldsEntity() {
          BooleanField = true,
          ByteField = 5,
          SByteField = 6,
          Int16Field = 7,
          UInt16Field = 8,
          Int32Field = 9,
          UInt32Field = 10,
          Int64Field = 11,
          UInt64Field = 12,
          SingleField = 13.0f,
          DoubleField = 14.0,
          DecimalField = (decimal) 15.0,
          GuidField = Guid.NewGuid(),
          DateTimeField = DateTime.UtcNow,
          TimeSpanField = TimeSpan.FromDays(16),
          StringField = "ABC"
        };

        new ReferenceFieldsEntity {ReferencedEntityField = new ReferencedEntity()};
        new StructureFieldsEntity {TestStructureField = new TestStructure() {X = 17, Y = 18}};
        transaction.Complete();
      }
    }

    [Test]
    public void ChangeStructFieldTest()
    {
      using (var session = Domain.OpenSession()) {
        RunStructTest<bool>(session, IsOptimisticPolicy);
        RunStructTest<byte>(session, IsOptimisticPolicy);
        RunStructTest<sbyte>(session, IsOptimisticPolicy);
        RunStructTest<short>(session, IsOptimisticPolicy);
        RunStructTest<ushort>(session, IsOptimisticPolicy);
        RunStructTest<int>(session, IsOptimisticPolicy);
        RunStructTest<uint>(session, IsOptimisticPolicy);
        RunStructTest<long>(session, IsOptimisticPolicy);
        RunStructTest<ulong>(session, IsOptimisticPolicy);
        RunStructTest<DateTime>(session, IsOptimisticPolicy);
        RunStructTest<TimeSpan>(session, IsOptimisticPolicy);
        RunStructTest<Guid>(session, IsOptimisticPolicy);
        RunStructTest<float>(session, IsOptimisticPolicy);
        RunStructTest<double>(session, IsOptimisticPolicy);
        RunStructTest<decimal>(session, IsOptimisticPolicy);
      }
    }

    [Test]
    public void ChangeStringTest()
    {
      using (var session = Domain.OpenSession()) {
        RunClassTest<string>(session, IsOptimisticPolicy);
      }
    }

    [Test]
    public void ChangeEntityFieldTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<ReferenceFieldsEntity>().First();
          var currentValue = entity.ReferencedEntityField;
          var currentVersion = entity.Version;

          entity.ReferencedEntityField = currentValue;
          var expectedVersion = (IsOptimisticPolicy) ? currentVersion : currentVersion + 1;
          Assert.That(entity.Version, Is.EqualTo(expectedVersion));
        }

        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<ReferenceFieldsEntity>().First();
          var currentValue = entity.ReferencedEntityField;
          var currentVersion = entity.Version;

          entity.ReferencedEntityField = new ReferencedEntity();
          Assert.That(entity.Version, Is.EqualTo(currentVersion + 1));
        }
      }
    }

    [Test]
    public void ChangeStructureFieldTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<StructureFieldsEntity>().First();
          var currentValue = entity.TestStructureField;
          var currentVersion = entity.Version;

          var a = ReferenceEquals(entity.TestStructureField, currentValue);
          entity.TestStructureField = currentValue;
          var expectedVersion = (IsOptimisticPolicy) ? currentVersion : currentVersion + 1;
          Assert.That(entity.Version, Is.EqualTo(expectedVersion));
        }

        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<StructureFieldsEntity>().First();
          var currentValue = entity.TestStructureField;
          var currentVersion = entity.Version;

          entity.TestStructureField = new TestStructure() {
            X = currentValue.X.ChangeStructValue(),
            Y = currentValue.Y.ChangeStructValue()
          };
          Assert.That(entity.Version, Is.EqualTo(currentVersion + 1));
        }
      }
    }

    [Test]
    public void ChangeFieldOfStructureTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<StructureFieldsEntity>().First();
          var currentValue = entity.TestStructureField.X;
          var currentVersion = entity.Version;

          entity.TestStructureField.X = currentValue;
          var expectedVersion = (IsOptimisticPolicy) ? currentVersion : currentVersion + 1;
          Assert.That(entity.Version, Is.EqualTo(expectedVersion));
        }

        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<StructureFieldsEntity>().First();
          var currentValue = entity.TestStructureField.X;
          var currentVersion = entity.Version;

          entity.TestStructureField.X = currentValue.ChangeStructValue();
          Assert.That(entity.Version, Is.EqualTo(currentVersion + 1));
        }
      }
    }

    private void RunStructTest<T>(Session session, bool isVersionChangeAfterwords)
      where T : struct
    {
      var fieldNameTemplate = "{0}Field";
      var typename = typeof(T).Name;
      var fiedlName = string.Format(fieldNameTemplate, typename);
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<SimpleTypesFieldsEntity>().First();
        var currentValue = (T) entity[fiedlName];
        var currentVersion = entity.Version;

        entity[fiedlName] = currentValue;
        var exprectedVersion = (isVersionChangeAfterwords) ? currentVersion : currentVersion + 1;
        Assert.That(entity.Version, Is.EqualTo(exprectedVersion));
      }

      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<SimpleTypesFieldsEntity>().First();
        var currentValue = (T) entity[fiedlName];
        var currentVersion = entity.Version;

        entity[fiedlName] = currentValue.ChangeStructValue();
        Assert.That(entity.Version, Is.EqualTo(currentVersion + 1));
      }
    }

    private void RunClassTest<T>(Session session, bool isVersionChangeAfterwords)
      where T : class
    {
      var fieldNameTemplate = "{0}Field";
      var typeName = typeof(T).Name;
      var fieldName = string.Format(fieldNameTemplate, typeName);

      using (var transacion = session.OpenTransaction()) {
        var entity = session.Query.All<SimpleTypesFieldsEntity>().First();
        var currentValue = (T) entity[fieldName];
        var currentVersion = entity.Version;

        entity[fieldName] = currentValue;
        var exprectedVersion = (isVersionChangeAfterwords) ? currentVersion : currentVersion + 1;
        Assert.That(entity.Version, Is.EqualTo(exprectedVersion));
      }

      using (var transacion = session.OpenTransaction()) {
        var entity = session.Query.All<SimpleTypesFieldsEntity>().First();
        var currentValue = (T) entity[fieldName];
        var currentVersion = entity.Version;

        entity[fieldName] = currentValue.ChangeClassValue();
        Assert.That(entity.Version, Is.EqualTo(currentVersion + 1));
      }
    }
  }

  public sealed class OptimistictVersioningPolicy : EntityVersioningPolicyTest
  {
    protected override void ApplyVersioningPolicy(DomainConfiguration configuration)
    {
      configuration.VersioningConvention.EntityVersioningPolicy = EntityVersioningPolicy.Optimistic;
    }
  }

  public sealed class PessimisticVersioningPolicy : EntityVersioningPolicyTest
  {
    protected override void ApplyVersioningPolicy(DomainConfiguration configuration)
    {
      configuration.VersioningConvention.EntityVersioningPolicy = EntityVersioningPolicy.Pessimistic;
    }
  }
}
