// Copyright (C) 2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Linq;
using NUnit.Framework;
using RealNonUniqueIndex = Xtensive.Orm.Tests.Model.IndexNullsDistinctTestModel.RealNonUniqueIndex;
using RealUniqueIndex = Xtensive.Orm.Tests.Model.IndexNullsDistinctTestModel.RealUniqueIndex;
using InheritedRealNonUniqueIndex = Xtensive.Orm.Tests.Model.IndexNullsDistinctTestModel.InheritedRealNonUniqueIndex;
using InheritedRealUniqueIndex = Xtensive.Orm.Tests.Model.IndexNullsDistinctTestModel.InheritedRealUniqueIndex;
using VirtualIndexes = Xtensive.Orm.Tests.Model.IndexNullsDistinctTestModel.VirtualIndexes;
using Xtensive.Orm.Building.Definitions;

namespace Xtensive.Orm.Tests.Model.IndexNullsDistinctTestModel
{
  namespace RealNonUniqueIndex
  {
    [HierarchyRoot]
    [Index($"{nameof(NullableValue)}:ASC", $"{nameof(NonNullableValue)}:ASC", Unique = false)]
    public class A : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field(Length = 50)]
      public string NullableValue { get; set; }

      [Field]
      public long NonNullableValue { get; set; }
    }
  }

  namespace RealUniqueIndex
  {
    [HierarchyRoot]
    [Index($"{nameof(NullableValue)}:ASC", $"{nameof(NonNullableValue)}:ASC", Unique = true)]
    public class A : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field(Length = 50)]
      public string NullableValue { get; set; }

      [Field]
      public long NonNullableValue { get; set; }
    }
  }

  namespace InheritedRealNonUniqueIndex
  {
    // copying fields, so copying indexes
    [HierarchyRoot(Orm.Model.InheritanceSchema.ConcreteTable)]
    [Index($"{nameof(NullableValue)}:ASC", $"{nameof(NonNullableValue)}:ASC", Unique = false)]
    public class A : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field(Length = 50)]
      public string NullableValue { get; set; }

      [Field]
      public long NonNullableValue { get; set; }
    }

    public class B : A
    {
      [Field(Length = 50)]
      public string NullableValueB { get; set; }

      [Field]
      public long NonNullableValueB { get; set; }
    }
  }

  namespace InheritedRealUniqueIndex
  {
    // copying fields, so copying indexes
    [HierarchyRoot(Orm.Model.InheritanceSchema.ConcreteTable)]
    [Index($"{nameof(NullableValue)}:ASC", $"{nameof(NonNullableValue)}:ASC", Unique = true)]
    public class A : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field(Length = 50)]
      public string NullableValue { get; set; }

      [Field]
      public long NonNullableValue { get; set; }
    }

    public class B : A
    {
      [Field(Length = 50)]
      public string NullableValueB { get; set; }

      [Field]
      public long NonNullableValueB { get; set; }
    }
  }

  namespace VirtualIndexes
  {
    public interface IA : IEntity
    {
      [Field]
      int Id { get; }

      [Field(Length = 50)]
      string NullableValueA { get; set; }

      [Field]
      long NonNullableValueA { get; set; }
    }

    [HierarchyRoot(Orm.Model.InheritanceSchema.ClassTable)]
    public class AClass : Entity, IA
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field(Length = 50)]
      public string NullableValueA { get; set; }

      [Field]
      public long NonNullableValueA { get; set; }
    }

    public class BClass : AClass
    {
      [Field(Length = 50)]
      public string NullableValueB { get; set; }

      [Field]
      public long NonNullableValueB { get; set; }
    }

    [HierarchyRoot(Orm.Model.InheritanceSchema.ConcreteTable)]
    public class AConcrete : Entity, IA
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field(Length = 50)]
      public string NullableValueA { get; set; }

      [Field]
      public long NonNullableValueA { get; set; }
    }
    public class BConcrete : AConcrete
    {
      [Field(Length = 50)]
      public string NullableValueB { get; set; }

      [Field]
      public long NonNullableValueB { get; set; }
    }

    [HierarchyRoot(Orm.Model.InheritanceSchema.SingleTable)]
    public class ASingle : Entity, IA
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field(Length = 50)]
      public string NullableValueA { get; set; }

      [Field]
      public long NonNullableValueA { get; set; }
    }

    public class BSingle : ASingle
    {
      [Field(Length = 50)]
      public string NullableValueB { get; set; }

      [Field]
      public long NonNullableValueB { get; set; }
    }
  }
}

namespace Xtensive.Orm.Tests.Model
{
  [TestFixture(Category = "Model")]
  public class IndexNullsDistinctTest : ModelBuildingTest
  {
    [Test]
    public void RealNonUniqueIndexTest1()
    {
      Require.AllFeaturesNotSupported(Providers.ProviderFeatures.UniqueNullTreatment);

      Action<Domain> domainValidator = static (domain) => {
        var model = domain.Model;
        var aClass = model.Types[typeof(RealNonUniqueIndex.A)];
        Assert.That(aClass.Indexes.Count, Is.EqualTo(4));
        var pk = aClass.Indexes.First(i => i.IsPrimary);
        Assert.That(pk.IsNullDistinct, Is.Null);
        Assert.That(aClass.Indexes.Any(i => i.IsSecondary && !i.IsVirtual), Is.True);
        Assert.That(aClass.Indexes.Where(i => i.IsSecondary).All(i => !i.IsUnique && i.IsNullDistinct is null), Is.True);
      };
      Action<DomainModelDef> definitionsValidator = static (modelDefs) => {
        var aClass = modelDefs.Types[typeof(RealNonUniqueIndex.A)];
        Assert.That(aClass.Indexes.Count, Is.EqualTo(2));
        var pk = aClass.Indexes.First(i => i.IsPrimary);
        Assert.That(pk.IsNullDistinct, Is.Null);
        var idx = aClass.Indexes.First(i => i.IsSecondary);
        Assert.That(idx.IsUnique, Is.False);
        Assert.That(idx.IsNullDistinct, Is.Null);
      };

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Skip;
      configuration.Types.Register(typeof(RealNonUniqueIndex.A));

      TestInvoker(configuration, domainValidator, definitionsValidator);

      configuration = configuration.Clone();
      configuration.UniqueNullTreatment = UniqueNullTreatment.Distinct;

      TestInvoker(configuration, domainValidator, definitionsValidator);

      configuration = configuration.Clone();
      configuration.UniqueNullTreatment = UniqueNullTreatment.NotDistinct;

      TestInvoker(configuration, domainValidator, definitionsValidator);
    }

    [Test]
    public void RealNonUniqueIndexTest2()
    {
      Require.AllFeaturesSupported(Providers.ProviderFeatures.UniqueNullTreatment);

      Action<Domain> domainValidator = static (domain) => {
        var model = domain.Model;
        var aClass = model.Types[typeof(RealNonUniqueIndex.A)];
        Assert.That(aClass.Indexes.Count, Is.EqualTo(4));
        var pk = aClass.Indexes.First(i => i.IsPrimary);
        Assert.That(pk.IsNullDistinct, Is.Null);
        Assert.That(aClass.Indexes.Any(i => i.IsSecondary && !i.IsVirtual), Is.True);
        Assert.That(aClass.Indexes.Where(i => i.IsSecondary).All(i => !i.IsUnique && i.IsNullDistinct is null), Is.True);
      };

      Action<DomainModelDef> definitionsValidator = (modelDefs) => {
        var aClass = modelDefs.Types[typeof(RealNonUniqueIndex.A)];
        Assert.That(aClass.Indexes.Count, Is.EqualTo(2));
        var pk = aClass.Indexes.First(i => i.IsPrimary);
        Assert.That(pk.IsNullDistinct, Is.Null);
        var idx = aClass.Indexes.First(i => i.IsSecondary);
        Assert.That(idx.IsUnique, Is.False);
        Assert.That(idx.IsNullDistinct, Is.Null);
      };

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Skip;
      configuration.Types.Register(typeof(RealNonUniqueIndex.A));

      TestInvoker(configuration, domainValidator, definitionsValidator);

      configuration = configuration.Clone();
      configuration.UniqueNullTreatment = UniqueNullTreatment.Distinct;

      TestInvoker(configuration, domainValidator, definitionsValidator);

      configuration = configuration.Clone();
      configuration.UniqueNullTreatment = UniqueNullTreatment.NotDistinct;

      TestInvoker(configuration, domainValidator, definitionsValidator);
    }

    [Test]
    public void RealUniqueIndexTest1()
    {
      Require.AllFeaturesNotSupported(Providers.ProviderFeatures.UniqueNullTreatment);

      bool? expectedDistinctNulls = null;

      Action<Domain> domainValidator = static (domain) => {
        var model = domain.Model;
        var aClass = model.Types[typeof(RealUniqueIndex.A)];
        Assert.That(aClass.Indexes.Count, Is.EqualTo(4));
        var pk = aClass.Indexes.First(i => i.IsPrimary);
        Assert.That(pk.IsNullDistinct, Is.Null);
        Assert.That(aClass.Indexes.Any(i => i.IsSecondary && !i.IsVirtual), Is.True);
        Assert.That(aClass.Indexes.Where(i => i.IsSecondary).All(i => i.IsNullDistinct is null), Is.True);
        Assert.That(aClass.Indexes.Any(i => i.IsSecondary && !i.IsVirtual && i.IsUnique), Is.True);
        Assert.That(aClass.Indexes.Where(i => i.IsSecondary && !i.IsVirtual && i.IsUnique).All(i => i.IsNullDistinct is null), Is.True);
      };
      Action<DomainModelDef> definitionsValidator = (modelDefs) => {
        var aClass = modelDefs.Types[typeof(RealUniqueIndex.A)];
        Assert.That(aClass.Indexes.Count, Is.EqualTo(2));
        var pk = aClass.Indexes.First(i => i.IsPrimary);
        Assert.That(pk.IsNullDistinct, Is.Null);
        var idx = aClass.Indexes.First(i => i.IsSecondary);
        Assert.That(idx.IsUnique, Is.True);
        Assert.That(idx.IsNullDistinct, Is.Null);
      };

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Skip;
      configuration.Types.Register(typeof(RealUniqueIndex.A));
      TestInvoker(configuration, domainValidator, definitionsValidator);

      configuration = configuration.Clone();
      configuration.UniqueNullTreatment = UniqueNullTreatment.Distinct;

      TestInvoker(configuration, domainValidator, definitionsValidator);

      configuration = configuration.Clone();
      configuration.UniqueNullTreatment = UniqueNullTreatment.NotDistinct;

      TestInvoker(configuration, domainValidator, definitionsValidator);
    }

    [Test]
    public void RealUniqueIndexTest2()
    {
      Require.AllFeaturesSupported(Providers.ProviderFeatures.UniqueNullTreatment);

      bool? expectedDistinctNulls = null;

      Action<Domain> domainValidator = (domain) => {
        var model = domain.Model;
        var aClass = model.Types[typeof(RealUniqueIndex.A)];
        Assert.That(aClass.Indexes.Count, Is.EqualTo(4));
        var pk = aClass.Indexes.First(i => i.IsPrimary);
        Assert.That(pk.IsNullDistinct, Is.Null);
        Assert.That(aClass.Indexes.Any(i => i.IsSecondary && !i.IsVirtual), Is.True);
        Assert.That(aClass.Indexes.Any(i => i.IsSecondary && i.IsVirtual), Is.True);
        Assert.That(aClass.Indexes.Where(i => i.IsSecondary && i.IsVirtual).All(i => i.IsNullDistinct is null), Is.True);
        Assert.That(aClass.Indexes.Any(i => i.IsSecondary && !i.IsVirtual && i.IsUnique), Is.True);
        Assert.That(aClass.Indexes.Where(i => i.IsSecondary && !i.IsVirtual && i.IsUnique).All(i => i.IsNullDistinct == expectedDistinctNulls), Is.True);
      };
      Action<DomainModelDef> definitionsValidator = (modelDefs) => {
        var aClass = modelDefs.Types[typeof(RealUniqueIndex.A)];
        Assert.That(aClass.Indexes.Count, Is.EqualTo(2));
        var pk = aClass.Indexes.First(i => i.IsPrimary);
        Assert.That(pk.IsNullDistinct, Is.Null);
        var idx = aClass.Indexes.First(i => i.IsSecondary);
        Assert.That(idx.IsUnique, Is.True);
        Assert.That(idx.IsNullDistinct, Is.EqualTo(expectedDistinctNulls));
      };

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Skip;
      configuration.Types.Register(typeof(RealUniqueIndex.A));

      TestInvoker(configuration, domainValidator, definitionsValidator);

      configuration = configuration.Clone();
      configuration.UniqueNullTreatment = UniqueNullTreatment.Distinct;
      expectedDistinctNulls = true;

      TestInvoker(configuration, domainValidator, definitionsValidator);

      configuration = configuration.Clone();
      configuration.UniqueNullTreatment = UniqueNullTreatment.NotDistinct;
      expectedDistinctNulls = false;

      TestInvoker(configuration, domainValidator, definitionsValidator);
    }

    [Test]
    public void InheritedNonUniqueIndexTest1()
    {
      Require.AllFeaturesNotSupported(Providers.ProviderFeatures.UniqueNullTreatment);

      bool? expectedDistinctNulls = null;

      Action <Domain> domainValidator = (domain) => {
        var model = domain.Model;
        var aClass = model.Types[typeof(InheritedRealNonUniqueIndex.A)];
        Assert.That(aClass.Indexes.Count, Is.EqualTo(4));
        var pk = aClass.Indexes.First(i => i.IsPrimary);
        Assert.That(pk.IsNullDistinct, Is.Null);
        Assert.That(aClass.Indexes.Any(i => i.IsSecondary && !i.IsVirtual), Is.True);
        Assert.That(aClass.Indexes.Any(i => i.IsSecondary && i.IsVirtual), Is.True);
        Assert.That(aClass.Indexes.Where(i => i.IsSecondary && !i.IsVirtual && !i.IsUnique).All(i => i.IsNullDistinct is null), Is.True);

        var bClass = model.Types[typeof(InheritedRealNonUniqueIndex.B)];
        Assert.That(bClass.Indexes.Count, Is.EqualTo(4));
        Assert.That(bClass.Indexes.Any(i => i.IsSecondary && !i.IsVirtual), Is.True);
        Assert.That(bClass.Indexes.Any(i => i.IsSecondary && i.IsVirtual), Is.True);
        Assert.That(bClass.Indexes.Where(i => i.IsSecondary && !i.IsVirtual && !i.IsUnique).All(i => i.IsNullDistinct is null), Is.True);
      };
      Action<DomainModelDef> modelDefinitionsValidator = (modelDefs) => {
        var aClass = modelDefs.Types[typeof(InheritedRealNonUniqueIndex.A)];
        Assert.That(aClass.Indexes.Count, Is.EqualTo(2));
        var pk = aClass.Indexes.First(i => i.IsPrimary);
        Assert.That(pk.IsNullDistinct, Is.Null);
        var idx = aClass.Indexes.First(i => i.IsSecondary);
        Assert.That(idx.IsUnique, Is.False);
        Assert.That(idx.IsNullDistinct, Is.Null);

        var bClass = modelDefs.Types[typeof(InheritedRealNonUniqueIndex.B)];
        Assert.That(bClass.Indexes.Count, Is.EqualTo(1));
        idx = bClass.Indexes.First(i => i.IsSecondary);
        Assert.That(idx.IsUnique, Is.False);
        Assert.That(idx.IsNullDistinct, Is.Null);
      };

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Skip;
      configuration.Types.Register(typeof(InheritedRealNonUniqueIndex.A));
      configuration.Types.Register(typeof(InheritedRealNonUniqueIndex.B));

      TestInvoker(configuration, domainValidator, modelDefinitionsValidator);

      configuration = configuration.Clone();
      configuration.UniqueNullTreatment = UniqueNullTreatment.Distinct;
      TestInvoker(configuration, domainValidator, modelDefinitionsValidator);

      configuration = configuration.Clone();
      configuration.UniqueNullTreatment = UniqueNullTreatment.NotDistinct;
      TestInvoker(configuration, domainValidator, modelDefinitionsValidator);
    }

    [Test]
    public void InheritedNonUniqueIndexTest2()
    {
      Require.AllFeaturesSupported(Providers.ProviderFeatures.UniqueNullTreatment);

      Action<Domain> domainValidator = (domain) => {
        var model = domain.Model;
        var aClass = model.Types[typeof(InheritedRealNonUniqueIndex.A)];
        Assert.That(aClass.Indexes.Count, Is.EqualTo(4));
        var pk = aClass.Indexes.First(i => i.IsPrimary);
        Assert.That(pk.IsNullDistinct, Is.Null);
        Assert.That(aClass.Indexes.Any(i => i.IsSecondary && !i.IsVirtual), Is.True);
        Assert.That(aClass.Indexes.Any(i => i.IsSecondary && i.IsVirtual), Is.True);
        Assert.That(aClass.Indexes.Where(i => i.IsSecondary && !i.IsVirtual && !i.IsUnique).All(i => i.IsNullDistinct is null), Is.True);

        var bClass = model.Types[typeof(InheritedRealNonUniqueIndex.B)];
        Assert.That(bClass.Indexes.Count, Is.EqualTo(4));
        Assert.That(bClass.Indexes.Any(i => i.IsSecondary && !i.IsVirtual), Is.True);
        Assert.That(bClass.Indexes.Any(i => i.IsSecondary && i.IsVirtual), Is.True);
        Assert.That(bClass.Indexes.Where(i => i.IsSecondary && !i.IsVirtual && !i.IsUnique).All(i => i.IsNullDistinct is null), Is.True);
      };

      Action<DomainModelDef> definitionsValidator = static (modelDefs) => {
        var aClass = modelDefs.Types[typeof(InheritedRealNonUniqueIndex.A)];
        Assert.That(aClass.Indexes.Count, Is.EqualTo(2));
        var pk = aClass.Indexes.First(i => i.IsPrimary);
        Assert.That(pk.IsNullDistinct, Is.Null);
        var idx = aClass.Indexes.First(i => i.IsSecondary);
        Assert.That(idx.IsUnique, Is.False);
        Assert.That(idx.IsNullDistinct, Is.Null);

        var bClass = modelDefs.Types[typeof(InheritedRealNonUniqueIndex.B)];
        Assert.That(bClass.Indexes.Count, Is.EqualTo(1));
        idx = bClass.Indexes.First(i => i.IsSecondary);
        Assert.That(idx.IsUnique, Is.False);
        Assert.That(idx.IsNullDistinct, Is.Null);
      };


      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Skip;
      configuration.Types.Register(typeof(InheritedRealNonUniqueIndex.A));
      configuration.Types.Register(typeof(InheritedRealNonUniqueIndex.B));

      TestInvoker(configuration, domainValidator, definitionsValidator);

      configuration = configuration.Clone();
      configuration.UniqueNullTreatment = UniqueNullTreatment.Distinct;
      TestInvoker(configuration, domainValidator, definitionsValidator);

      configuration = configuration.Clone();
      configuration.UniqueNullTreatment = UniqueNullTreatment.NotDistinct;
      TestInvoker(configuration, domainValidator, definitionsValidator);
    }

    [Test]
    public void InheritedUniqueIndexTest1()
    {
      Require.AllFeaturesNotSupported(Providers.ProviderFeatures.UniqueNullTreatment);

      bool? expectedDistinctNulls = null;

      Action<Domain> domainValidator = (domain) => {
        var model = domain.Model;
        var aClass = model.Types[typeof(InheritedRealUniqueIndex.A)];
        Assert.That(aClass.Indexes.Count, Is.EqualTo(4));
        var pk = aClass.Indexes.First(i => i.IsPrimary);
        Assert.That(pk.IsNullDistinct, Is.Null);
        Assert.That(aClass.Indexes.Any(i => i.IsSecondary && !i.IsVirtual), Is.True);
        Assert.That(aClass.Indexes.Any(i => i.IsSecondary && i.IsVirtual), Is.True);
        Assert.That(aClass.Indexes.Where(i => i.IsSecondary && i.IsVirtual).All(i => i.IsNullDistinct is null), Is.True);
        Assert.That(aClass.Indexes.Any(i => i.IsSecondary && !i.IsVirtual && i.IsUnique), Is.True);
        Assert.That(aClass.Indexes.Where(i => i.IsSecondary && !i.IsVirtual && i.IsUnique).All(i => i.IsNullDistinct is null), Is.True);

        var bClass = model.Types[typeof(InheritedRealUniqueIndex.B)];
        Assert.That(bClass.Indexes.Count, Is.EqualTo(4));
        Assert.That(bClass.Indexes.Any(i => i.IsSecondary && !i.IsVirtual), Is.True);
        Assert.That(bClass.Indexes.Any(i => i.IsSecondary && i.IsVirtual), Is.True);
        Assert.That(bClass.Indexes.Where(i => i.IsSecondary && i.IsVirtual).All(i => i.IsNullDistinct is null), Is.True);
        Assert.That(bClass.Indexes.Any(i => i.IsSecondary && !i.IsVirtual && i.IsUnique), Is.True);
        Assert.That(bClass.Indexes.Where(i => i.IsSecondary && !i.IsVirtual && i.IsUnique).All(i => i.IsNullDistinct is null), Is.True);
      };
      Action<DomainModelDef> modelDefinitionsValidator = (modelDefs) => {
        var aClass = modelDefs.Types[typeof(InheritedRealUniqueIndex.A)];
        Assert.That(aClass.Indexes.Count, Is.EqualTo(2));
        var pk = aClass.Indexes.First(i => i.IsPrimary);
        Assert.That(pk.IsNullDistinct, Is.Null);
        var idx = aClass.Indexes.First(i => i.IsSecondary);
        Assert.That(idx.IsUnique, Is.True);
        Assert.That(idx.IsNullDistinct, Is.Null);

        var bClass = modelDefs.Types[typeof(InheritedRealUniqueIndex.B)];
        Assert.That(bClass.Indexes.Count, Is.EqualTo(1));
        idx = bClass.Indexes.First(i => i.IsSecondary);
        Assert.That(idx.IsUnique, Is.True);
        Assert.That(idx.IsNullDistinct, Is.Null);
      };

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Skip;
      configuration.Types.Register(typeof(InheritedRealUniqueIndex.A));
      configuration.Types.Register(typeof(InheritedRealUniqueIndex.B));

      TestInvoker(configuration, domainValidator, modelDefinitionsValidator);

      configuration = configuration.Clone();
      configuration.UniqueNullTreatment = UniqueNullTreatment.Distinct;

      configuration = configuration.Clone();
      configuration.UniqueNullTreatment = UniqueNullTreatment.NotDistinct;
    }

    [Test]
    public void InheritedUniqueIndexTest2()
    {
      Require.AllFeaturesSupported(Providers.ProviderFeatures.UniqueNullTreatment);

      bool? expectedDistinctNulls = null;

      Action<Domain> domainValidator = (domain) => {
        var model = domain.Model;
        var aClass = model.Types[typeof(InheritedRealUniqueIndex.A)];
        Assert.That(aClass.Indexes.Count, Is.EqualTo(4));
        var pk = aClass.Indexes.First(i => i.IsPrimary);
        Assert.That(pk.IsNullDistinct, Is.Null);
        Assert.That(aClass.Indexes.Any(i => i.IsSecondary && !i.IsVirtual), Is.True);
        Assert.That(aClass.Indexes.Any(i => i.IsSecondary && i.IsVirtual), Is.True);
        Assert.That(aClass.Indexes.Where(i => i.IsSecondary && i.IsVirtual).All(i => i.IsNullDistinct is null), Is.True);
        Assert.That(aClass.Indexes.Any(i => i.IsSecondary && !i.IsVirtual && i.IsUnique), Is.True);
        Assert.That(aClass.Indexes.Where(i => i.IsSecondary && !i.IsVirtual && i.IsUnique).All(i => i.IsNullDistinct == expectedDistinctNulls), Is.True);

        var bClass = model.Types[typeof(InheritedRealUniqueIndex.B)];
        Assert.That(bClass.Indexes.Count, Is.EqualTo(4));
        Assert.That(bClass.Indexes.Any(i => i.IsSecondary && !i.IsVirtual), Is.True);
        Assert.That(bClass.Indexes.Any(i => i.IsSecondary && i.IsVirtual), Is.True);
        Assert.That(bClass.Indexes.Where(i => i.IsSecondary && i.IsVirtual).All(i => i.IsNullDistinct is null), Is.True);
        Assert.That(bClass.Indexes.Any(i => i.IsSecondary && !i.IsVirtual && i.IsUnique), Is.True);
        Assert.That(bClass.Indexes.Where(i => i.IsSecondary && !i.IsVirtual && i.IsUnique).All(i => i.IsNullDistinct == expectedDistinctNulls), Is.True);
      };
      Action<DomainModelDef> definitionsValidator = (modelDefs) => {
        var aClass = modelDefs.Types[typeof(InheritedRealUniqueIndex.A)];
        Assert.That(aClass.Indexes.Count, Is.EqualTo(2));
        var pk = aClass.Indexes.First(i => i.IsPrimary);
        Assert.That(pk.IsNullDistinct, Is.Null);
        var idx = aClass.Indexes.First(i => i.IsSecondary);
        Assert.That(idx.IsUnique, Is.True);
        Assert.That(idx.IsNullDistinct, Is.EqualTo(expectedDistinctNulls));

        var bClass = modelDefs.Types[typeof(InheritedRealUniqueIndex.B)];
        Assert.That(bClass.Indexes.Count, Is.EqualTo(1));
        idx = bClass.Indexes.First(i => i.IsSecondary);
        Assert.That(idx.IsUnique, Is.True);
        Assert.That(idx.IsNullDistinct, Is.EqualTo(expectedDistinctNulls));
      };

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Skip;
      configuration.Types.Register(typeof(InheritedRealUniqueIndex.A));
      configuration.Types.Register(typeof(InheritedRealUniqueIndex.B));

      TestInvoker(configuration, domainValidator, definitionsValidator);

      configuration = configuration.Clone();
      configuration.UniqueNullTreatment = UniqueNullTreatment.Distinct;
      expectedDistinctNulls = true;

      TestInvoker(configuration, domainValidator, definitionsValidator);

      configuration = configuration.Clone();
      configuration.UniqueNullTreatment = UniqueNullTreatment.NotDistinct;
      expectedDistinctNulls = false;

      TestInvoker(configuration, domainValidator, definitionsValidator);
    }


    [Test]
    public void VirtualIndexesTest()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Skip;
      configuration.Types.Register(typeof(VirtualIndexes.AClass));
      configuration.Types.Register(typeof(VirtualIndexes.BClass));
      configuration.Types.Register(typeof(VirtualIndexes.AConcrete));
      configuration.Types.Register(typeof(VirtualIndexes.BConcrete));
      configuration.Types.Register(typeof(VirtualIndexes.ASingle));
      configuration.Types.Register(typeof(VirtualIndexes.BSingle));

      TestInvoker(configuration,
        static (domain) => {
          var model = domain.Model;
          var aClass = model.Types[typeof(VirtualIndexes.AClass)];
          Assert.That(aClass.Indexes.Count, Is.EqualTo(1));
          var pk = aClass.Indexes[0];
          Assert.That(pk.IsNullDistinct, Is.Null);

          var bClass = model.Types[typeof(VirtualIndexes.BClass)];
          Assert.That(bClass.Indexes.Count, Is.EqualTo(2));
          Assert.That(bClass.Indexes.Any(i => i.IsVirtual), Is.True);
          Assert.That(bClass.Indexes.All(i => i.IsNullDistinct is null), Is.True);

          var aConcrete = model.Types[typeof(VirtualIndexes.AConcrete)];
          Assert.That(aConcrete.Indexes.Count, Is.EqualTo(2));
          Assert.That(aConcrete.Indexes.Any(i => i.IsVirtual), Is.True);
          Assert.That(aConcrete.Indexes.All(i => i.IsNullDistinct is null), Is.True);

          var bConcrete = model.Types[typeof(VirtualIndexes.BConcrete)];
          Assert.That(bConcrete.Indexes.Count, Is.EqualTo(2));
          Assert.That(bConcrete.Indexes.Any(i => i.IsVirtual), Is.True);
          Assert.That(bConcrete.Indexes.All(i => i.IsNullDistinct is null), Is.True);

          var aSingle = model.Types[typeof(VirtualIndexes.ASingle)];
          Assert.That(aSingle.Indexes.Count, Is.EqualTo(1));
          Assert.That(aSingle.Indexes.Any(i => i.IsVirtual), Is.False);
          pk = aSingle.Indexes[0];
          Assert.That(pk.IsNullDistinct, Is.Null);

          var bSingle = model.Types[typeof(VirtualIndexes.BSingle)];
          Assert.That(bSingle.Indexes.Count, Is.EqualTo(1));
          Assert.That(bSingle.Indexes.Any(i => i.IsVirtual), Is.True);
          Assert.That(bSingle.Indexes.All(i => i.IsNullDistinct is null), Is.True);
        },
        static (modelDefs) => {
          var aClass = modelDefs.Types[typeof(VirtualIndexes.AClass)];
          Assert.That(aClass.Indexes.Count, Is.EqualTo(1));
          var pk = aClass.Indexes[0];
          Assert.That(pk.IsNullDistinct, Is.Null);

          var bClass = modelDefs.Types[typeof(VirtualIndexes.BClass)];
          Assert.That(bClass.Indexes.Count, Is.EqualTo(0));

          var aConcrete = modelDefs.Types[typeof(VirtualIndexes.AConcrete)];
          Assert.That(aConcrete.Indexes.Count, Is.EqualTo(1));
          pk = aConcrete.Indexes[0];
          Assert.That(pk.IsNullDistinct, Is.Null);

          var bConcrete = modelDefs.Types[typeof(VirtualIndexes.BConcrete)];
          Assert.That(bConcrete.Indexes.Count, Is.EqualTo(0));

          var aSingle = modelDefs.Types[typeof(VirtualIndexes.ASingle)];
          Assert.That(aSingle.Indexes.Count, Is.EqualTo(1));
          pk = aSingle.Indexes[0];
          Assert.That(pk.IsNullDistinct, Is.Null);

          var bSingle = modelDefs.Types[typeof(VirtualIndexes.BSingle)];
          Assert.That(bSingle.Indexes.Count, Is.EqualTo(0));
        });
    }
  }
}
