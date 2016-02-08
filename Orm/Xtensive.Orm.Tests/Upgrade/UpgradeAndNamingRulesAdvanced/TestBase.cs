// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.02.03

using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0168_RemoveQueryableExtensionFails_Model;
using Xtensive.Reflection;
using sources = Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.SourceModels;
using targets = Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced.TargetModels;

namespace Xtensive.Orm.Tests.Upgrade.UpgradeAndNamingRulesAdvanced
{
  [TestFixture]
  public abstract class TestBase
  {
    public readonly object guard = new object();
    private NamingConvention namingConvention;
    
    [Test]
    public void AddNewFieldTest()
    {
      var initialConfiguration = BuildInitialDomainConfiguration(GetType().Assembly, typeof (sources.AddNewFieldModel.SingleTableHierarchyBase).Namespace);
      using (var domain = BuildDomain(initialConfiguration))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new sources.AddNewFieldModel.ClassTableHierarchyBase {BaseClassField = Guid.NewGuid().ToString()};
        new sources.AddNewFieldModel.SingleTableHierarchyBase {BaseClassField = Guid.NewGuid().ToString()};
        new sources.AddNewFieldModel.ConcreteTableHierarchyBase {BaseClassField = Guid.NewGuid().ToString()};

        new sources.AddNewFieldModel.ClassTableDescendant {BaseClassField = Guid.NewGuid().ToString(), DescendantClassField = Guid.NewGuid().ToString()};
        new sources.AddNewFieldModel.SingleTableDescendant {BaseClassField = Guid.NewGuid().ToString(), DescendantClassField = Guid.NewGuid().ToString()};
        new sources.AddNewFieldModel.ConcreteTableDescendant {BaseClassField = Guid.NewGuid().ToString(), DescendantClassField = Guid.NewGuid().ToString()};

        transaction.Complete();
      }

      var finalConfiguration = BuildFinalDomainConfiguration(GetType().Assembly, typeof (targets.AddNewFieldModel.SingleTableHierarchyBase).Namespace, false);
      using (var domain = BuildDomain(finalConfiguration))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query1 = session.Query.All<targets.AddNewFieldModel.SingleTableHierarchyBase>().ToList();
        Assert.That(query1.Count, Is.EqualTo(2));
        foreach (var singleTableHierarchyBase in query1) {
          Assert.That(singleTableHierarchyBase.BaseClassField.IsNullOrEmpty(), Is.False);
        }

        var query2 = session.Query.All<targets.AddNewFieldModel.SingleTableDescendant>().ToList();
        Assert.That(query2.Count, Is.EqualTo(1));
        foreach (var singleTableDescendant in query2) {
          Assert.That(singleTableDescendant.BaseClassField.IsNullOrEmpty(), Is.False);
          Assert.That(singleTableDescendant.DescendantClassField.IsNullOrEmpty(), Is.False);
          Assert.That(singleTableDescendant.DescendantValueField.HasValue, Is.False);
        }

        var query3 = session.Query.All<targets.AddNewFieldModel.ClassTableHierarchyBase>().ToList();
        Assert.That(query3.Count, Is.EqualTo(2));
        foreach (var singleTableHierarchyBase in query3) {
          Assert.That(singleTableHierarchyBase.BaseClassField.IsNullOrEmpty(), Is.False);
        }

        var query4 = session.Query.All<targets.AddNewFieldModel.ClassTableDescendant>().ToList();
        Assert.That(query4.Count, Is.EqualTo(1));
        foreach (var singleTableDescendant in query4) {
          Assert.That(singleTableDescendant.BaseClassField.IsNullOrEmpty(), Is.False);
          Assert.That(singleTableDescendant.DescendantClassField.IsNullOrEmpty(), Is.False);
          Assert.That(singleTableDescendant.DescendantValueField.HasValue, Is.False);
        }

        var query5 = session.Query.All<targets.AddNewFieldModel.ConcreteTableHierarchyBase>().ToList();
        Assert.That(query5.Count, Is.EqualTo(2));
        foreach (var singleTableHierarchyBase in query5) {
          Assert.That(singleTableHierarchyBase.BaseClassField.IsNullOrEmpty(), Is.False);
        }

        var query6 = session.Query.All<targets.AddNewFieldModel.ConcreteTableDescendant>().ToList();
        Assert.That(query6.Count, Is.EqualTo(1));
        foreach (var singleTableDescendant in query6) {
          Assert.That(singleTableDescendant.BaseClassField.IsNullOrEmpty(), Is.False);
          Assert.That(singleTableDescendant.DescendantClassField.IsNullOrEmpty(), Is.False);
          Assert.That(singleTableDescendant.DescendantValueField.HasValue, Is.False);
        }
      }
    }

    [Test]
    public void AddNewTypeTest()
    {
      var initialConfiguration = BuildInitialDomainConfiguration(GetType().Assembly, typeof(sources.AddNewTypeModel.SingleTableHierarchyBase).Namespace);
      using (var domain = BuildDomain(initialConfiguration))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new sources.AddNewTypeModel.ClassTableHierarchyBase {SomeKindOfText = Guid.NewGuid().ToString()};
        new sources.AddNewTypeModel.SingleTableHierarchyBase { SomeKindOfText = Guid.NewGuid().ToString() };
        new sources.AddNewTypeModel.ConcreteTableHierarchyBase { SomeKindOfText = Guid.NewGuid().ToString() };

        new sources.AddNewTypeModel.ClassTableDescendant { SomeKindOfText = Guid.NewGuid().ToString(), Comment = Guid.NewGuid().ToString()};
        new sources.AddNewTypeModel.SingleTableDescendant { SomeKindOfText = Guid.NewGuid().ToString(), Comment = Guid.NewGuid().ToString() };
        new sources.AddNewTypeModel.ConcreteTableDescendant { SomeKindOfText = Guid.NewGuid().ToString(), Comment = Guid.NewGuid().ToString() };

        transaction.Complete();
      }

      var finalConfiguration = BuildFinalDomainConfiguration(GetType().Assembly, typeof(targets.AddNewTypeModel.SingleTableHierarchyBase).Namespace, false);
      using (var domain = BuildDomain(finalConfiguration))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query1 = session.Query.All<targets.AddNewTypeModel.SingleTableHierarchyBase>().ToList();
        Assert.That(query1.Count, Is.EqualTo(2));
        foreach (var singleTableHierarchyBase in query1) {
          Assert.That(singleTableHierarchyBase.SomeKindOfText.IsNullOrEmpty(), Is.False);
        }

        var query2 = session.Query.All<targets.AddNewTypeModel.SingleTableDescendant>().ToList();
        Assert.That(query2.Count, Is.EqualTo(1));
        foreach (var singleTableDescendant in query2) {
          Assert.That(singleTableDescendant.SomeKindOfText.IsNullOrEmpty(), Is.False);
          Assert.That(singleTableDescendant.Comment.IsNullOrEmpty(), Is.False);
        }

        var query3 = session.Query.All<targets.AddNewTypeModel.AnotherSingleTableDescentant>().ToList();
        Assert.That(query3.Count, Is.EqualTo(0));

        var query4 = session.Query.All<targets.AddNewTypeModel.ClassTableHierarchyBase>().ToList();
        Assert.That(query4.Count, Is.EqualTo(2));
        foreach (var singleTableHierarchyBase in query4) {
          Assert.That(singleTableHierarchyBase.SomeKindOfText.IsNullOrEmpty(), Is.False);
        }

        var query5 = session.Query.All<targets.AddNewTypeModel.ClassTableDescendant>().ToList();
        Assert.That(query5.Count, Is.EqualTo(1));
        foreach (var singleTableDescendant in query5) {
          Assert.That(singleTableDescendant.SomeKindOfText.IsNullOrEmpty(), Is.False);
          Assert.That(singleTableDescendant.Comment.IsNullOrEmpty(), Is.False);
        }

        var query6 = session.Query.All<targets.AddNewTypeModel.AnotherClassTableDescendant>().ToList();
        Assert.That(query6.Count, Is.EqualTo(0));

        var query7 = session.Query.All<targets.AddNewTypeModel.ConcreteTableHierarchyBase>().ToList();
        Assert.That(query7.Count, Is.EqualTo(2));
        foreach (var singleTableHierarchyBase in query7) {
          Assert.That(singleTableHierarchyBase.SomeKindOfText.IsNullOrEmpty(), Is.False);
        }

        var query8 = session.Query.All<targets.AddNewTypeModel.ConcreteTableDescendant>().ToList();
        Assert.That(query8.Count, Is.EqualTo(1));
        foreach (var singleTableDescendant in query8) {
          Assert.That(singleTableDescendant.SomeKindOfText.IsNullOrEmpty(), Is.False);
          Assert.That(singleTableDescendant.Comment.IsNullOrEmpty(), Is.False);
        }

        var query9 = session.Query.All<targets.AddNewTypeModel.AnotherConcreteTableDescendant>().ToList();
        Assert.That(query9.Count, Is.EqualTo(0));
      }
    }

    [Test]
    public void RemoveFieldWithHintTest()
    {
      var initialConfiguration = BuildInitialDomainConfiguration(GetType().Assembly, typeof (sources.RemoveFieldModel.SingleTableHierarchyBase).Namespace);

      using (var domain = Domain.Build(initialConfiguration))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new sources.RemoveFieldModel.ClassTableHierarchyBase {
          ClassTableHBField = Guid.NewGuid().ToString(),
          ClassTableRemovableField = Guid.NewGuid().ToString()
        };
        new sources.RemoveFieldModel.ClassTableDescendant {
          ClassTableHBField = Guid.NewGuid().ToString(),
          ClassTableRemovableField = Guid.NewGuid().ToString(),
          SomeDescendantField = Guid.NewGuid().ToString(),
          SomeDescendantRemovableField = Guid.NewGuid().ToString()};
        new sources.RemoveFieldModel.ConcreteTableHierarchyBase {
          ConcreteTableHBField = Guid.NewGuid().ToString(),
          ConcreteTableRemovableField = Guid.NewGuid().ToString()
        };
        new sources.RemoveFieldModel.ConcreteTableDescendant {
          ConcreteTableHBField = Guid.NewGuid().ToString(),
          ConcreteTableRemovableField = Guid.NewGuid().ToString(),
          SomeDescendantField = Guid.NewGuid().ToString(),
          SomeDescendantRemovableField = Guid.NewGuid().ToString()
        };
        new sources.RemoveFieldModel.SingleTableHierarchyBase {
          SingleTableHBField = Guid.NewGuid().ToString(),
          SingleTableRemovableField = Guid.NewGuid().ToString()
        };
        new sources.RemoveFieldModel.SingleTableDescendant {
          SingleTableHBField = Guid.NewGuid().ToString(),
          SingleTableRemovableField = Guid.NewGuid().ToString(),
          SomeDescendantField = Guid.NewGuid().ToString(),
          SomeDescendantRemovableField = Guid.NewGuid().ToString()
        };
        transaction.Complete();
      }

      var finalConfiguration = BuildFinalDomainConfiguration(GetType().Assembly, typeof (targets.RemoveFieldModel.SingleTableHierarchyBase).Namespace, true);

      using (var domain = Domain.Build(finalConfiguration))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query1 = session.Query.All<targets.RemoveFieldModel.SingleTableHierarchyBase>().ToArray();
        Assert.That(query1.Length, Is.EqualTo(2));

        var query2 = session.Query.All<targets.RemoveFieldModel.SingleTableDescendant>().ToArray();
        Assert.That(query2.Length, Is.EqualTo(1));

        var query3 = session.Query.All<targets.RemoveFieldModel.ConcreteTableHierarchyBase>().ToArray();
        Assert.That(query3.Length, Is.EqualTo(2));

        var query4 = session.Query.All<targets.RemoveFieldModel.ConcreteTableDescendant>().ToArray();
        Assert.That(query4.Length, Is.EqualTo(1));

        var query5 = session.Query.All<targets.RemoveFieldModel.ClassTableHierarchyBase>().ToArray();
        Assert.That(query5.Length, Is.EqualTo(2));

        var query6 = session.Query.All<targets.RemoveFieldModel.ClassTableDescendant>().ToArray();
        Assert.That(query6.Length, Is.EqualTo(1));
      }
    }

    [Test]
    public void RemoveFieldWithoutHintTest()
    {
      var initialConfiguration = BuildInitialDomainConfiguration(GetType().Assembly, typeof(sources.RemoveFieldModel.SingleTableHierarchyBase).Namespace);

      using (var domain = Domain.Build(initialConfiguration))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new sources.RemoveFieldModel.ClassTableHierarchyBase {
          ClassTableHBField = Guid.NewGuid().ToString(),
          ClassTableRemovableField = Guid.NewGuid().ToString()
        };
        new sources.RemoveFieldModel.ClassTableDescendant {
          ClassTableHBField = Guid.NewGuid().ToString(),
          ClassTableRemovableField = Guid.NewGuid().ToString(),
          SomeDescendantField = Guid.NewGuid().ToString(),
          SomeDescendantRemovableField = Guid.NewGuid().ToString()
        };
        new sources.RemoveFieldModel.ConcreteTableHierarchyBase {
          ConcreteTableHBField = Guid.NewGuid().ToString(),
          ConcreteTableRemovableField = Guid.NewGuid().ToString()
        };
        new sources.RemoveFieldModel.ConcreteTableDescendant {
          ConcreteTableHBField = Guid.NewGuid().ToString(),
          ConcreteTableRemovableField = Guid.NewGuid().ToString(),
          SomeDescendantField = Guid.NewGuid().ToString(),
          SomeDescendantRemovableField = Guid.NewGuid().ToString()
        };
        new sources.RemoveFieldModel.SingleTableHierarchyBase {
          SingleTableHBField = Guid.NewGuid().ToString(),
          SingleTableRemovableField = Guid.NewGuid().ToString()
        };
        new sources.RemoveFieldModel.SingleTableDescendant {
          SingleTableHBField = Guid.NewGuid().ToString(),
          SingleTableRemovableField = Guid.NewGuid().ToString(),
          SomeDescendantField = Guid.NewGuid().ToString(),
          SomeDescendantRemovableField = Guid.NewGuid().ToString()
        };
        transaction.Complete();
      }

      var finalConfiguration = BuildFinalDomainConfiguration(GetType().Assembly, typeof(targets.RemoveFieldModel.SingleTableHierarchyBase).Namespace, false);
      Assert.Throws<SchemaSynchronizationException>(() => BuildDomain(finalConfiguration));
    }

    [Test]
    public void RemoveTypeWithHintTest()
    {
      var initialConfiguration = BuildInitialDomainConfiguration(GetType().Assembly, typeof (sources.RemoveTypeModel.SingleTableHierarchyBase).Namespace);

      using (var domain = Domain.Build(initialConfiguration))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new sources.RemoveTypeModel.SingleTableHierarchyBase {
          SinlgeTableHBField = Guid.NewGuid().ToString()
        };
        new sources.RemoveTypeModel.SingleTableDescendant1 {
          SinlgeTableHBField = Guid.NewGuid().ToString(),
          SingleTableD1Field = Guid.NewGuid().ToString()
        };
        new sources.RemoveTypeModel.SingleTableDescendant11 {
          SinlgeTableHBField = Guid.NewGuid().ToString(),
          SingleTableD1Field = Guid.NewGuid().ToString(),
          SingleTableD11Field = Guid.NewGuid().ToString()
        };
        new sources.RemoveTypeModel.SingleTableDescendant12Removed {
          SinlgeTableHBField = Guid.NewGuid().ToString(),
          SingleTableD1Field = Guid.NewGuid().ToString(),
          SingleTableD12Field = Guid.NewGuid().ToString()
        };
        new sources.RemoveTypeModel.SingleTableDescendant2 {
          SinlgeTableHBField = Guid.NewGuid().ToString(),
          SingleTableD2Field = Guid.NewGuid().ToString()
        };
        new sources.RemoveTypeModel.SingleTableDescendant21 {
          SinlgeTableHBField = Guid.NewGuid().ToString(),
          SingleTableD2Field = Guid.NewGuid().ToString(),
          SingleTableD21Field = Guid.NewGuid().ToString()
        };
        new sources.RemoveTypeModel.SingleTableDescendant22Removed {
          SinlgeTableHBField = Guid.NewGuid().ToString(),
          SingleTableD2Field = Guid.NewGuid().ToString(),
          SingleTableD22Field = Guid.NewGuid().ToString()
        };

        new sources.RemoveTypeModel.ClassTableHierarchyBase {
          ClassTableHBField = Guid.NewGuid().ToString(),
        };
        new sources.RemoveTypeModel.ClassTableDescendant1 {
          ClassTableHBField = Guid.NewGuid().ToString(),
          ClassTableD1Field = Guid.NewGuid().ToString(),
        };
        new sources.RemoveTypeModel.ClassTableDescendant11 {
          ClassTableHBField = Guid.NewGuid().ToString(),
          ClassTableD1Field = Guid.NewGuid().ToString(),
          ClassTableD11Field = Guid.NewGuid().ToString()
        };
        new sources.RemoveTypeModel.ClassTableDescendant12Removed {
          ClassTableHBField = Guid.NewGuid().ToString(),
          ClassTableD1Field = Guid.NewGuid().ToString(),
          ClassTableD12Field = Guid.NewGuid().ToString()
        };
        new sources.RemoveTypeModel.ClassTableDescendant2 {
          ClassTableHBField = Guid.NewGuid().ToString(),
          ClassTableD2Field = Guid.NewGuid().ToString(),
        };
        new sources.RemoveTypeModel.ClassTableDescendant21() {
          ClassTableHBField = Guid.NewGuid().ToString(),
          ClassTableD2Field = Guid.NewGuid().ToString(),
          ClassTableD21Field = Guid.NewGuid().ToString()
        };
        new sources.RemoveTypeModel.ClassTableDescendant22Removed {
          ClassTableHBField = Guid.NewGuid().ToString(),
          ClassTableD2Field = Guid.NewGuid().ToString(),
          ClassTableD22Field = Guid.NewGuid().ToString()
        };

        new sources.RemoveTypeModel.ConcreteTableHierarchyBase {
          ConcreteTableHBField = Guid.NewGuid().ToString(),
        };
        new sources.RemoveTypeModel.ConcreteTableDescendant1 {
          ConcreteTableHBField = Guid.NewGuid().ToString(),
          ConcreteTableD1Field = Guid.NewGuid().ToString()
        };
        new sources.RemoveTypeModel.ConcreteTableDescendant11 {
          ConcreteTableHBField = Guid.NewGuid().ToString(),
          ConcreteTableD1Field = Guid.NewGuid().ToString(),
          ConcreteTableD11Field = Guid.NewGuid().ToString()
        };
        new sources.RemoveTypeModel.ConcreteTableDescendant12Removed {
          ConcreteTableHBField = Guid.NewGuid().ToString(),
          ConcreteTableD1Field = Guid.NewGuid().ToString(),
          ConcreteTableD12Field = Guid.NewGuid().ToString()
        };
        new sources.RemoveTypeModel.ConcreteTableDescendant2 {
          ConcreteTableHBField = Guid.NewGuid().ToString(),
          ConcreteTableD2Field = Guid.NewGuid().ToString()
        };
        new sources.RemoveTypeModel.ConcreteTableDescendant21 {
          ConcreteTableHBField = Guid.NewGuid().ToString(),
          ConcreteTableD2Field = Guid.NewGuid().ToString(),
          ConcreteTableD21Field = Guid.NewGuid().ToString()
        };
        new sources.RemoveTypeModel.ConcreteTableDescendant22Removed {
          ConcreteTableHBField = Guid.NewGuid().ToString(),
          ConcreteTableD2Field = Guid.NewGuid().ToString(),
          ConcreteTableD22Field = Guid.NewGuid().ToString()
        };
        transaction.Complete();
      }

      var finalConfiguration = BuildFinalDomainConfiguration(GetType().Assembly, typeof (targets.RemoveTypeModel.SingleTableHierarchyBase).Namespace, true);

      using (var domain = Domain.Build(finalConfiguration))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query1 = session.Query.All<targets.RemoveTypeModel.SingleTableHierarchyBase>().ToArray();
        Assert.That(query1.Length, Is.EqualTo(5));

        var query2 = session.Query.All<targets.RemoveTypeModel.SingleTableDescendant1>().ToArray();
        Assert.That(query2.Length, Is.EqualTo(2));

        var query3 = session.Query.All<targets.RemoveTypeModel.SingleTableDescendant2>().ToArray();
        Assert.That(query3.Length, Is.EqualTo(2));

        var query4 = session.Query.All<targets.RemoveTypeModel.SingleTableDescendant11>().ToArray();
        Assert.That(query4.Length, Is.EqualTo(1));

        var query5 = session.Query.All<targets.RemoveTypeModel.SingleTableDescendant21>().ToArray();
        Assert.That(query5.Length, Is.EqualTo(1));

        var query6 = session.Query.All<targets.RemoveTypeModel.ClassTableHierarchyBase>().ToArray();
        Assert.That(query6.Length, Is.EqualTo(5));

        var query7 = session.Query.All<targets.RemoveTypeModel.ClassTableDescendant1>().ToArray();
        Assert.That(query7.Length, Is.EqualTo(2));

        var query8 = session.Query.All<targets.RemoveTypeModel.ClassTableDescendant2>().ToArray();
        Assert.That(query8.Length, Is.EqualTo(2));

        var query9 = session.Query.All<targets.RemoveTypeModel.ClassTableDescendant11>().ToArray();
        Assert.That(query9.Length, Is.EqualTo(1));

        var query10 = session.Query.All<targets.RemoveTypeModel.ClassTableDescendant21>().ToArray();
        Assert.That(query10.Length, Is.EqualTo(1));

        var query11 = session.Query.All<targets.RemoveTypeModel.ConcreteTableHierarchyBase>().ToArray();
        Assert.That(query11.Length, Is.EqualTo(5));

        var query12 = session.Query.All<targets.RemoveTypeModel.ConcreteTableDescendant1>().ToArray();
        Assert.That(query12.Length, Is.EqualTo(2));

        var query13 = session.Query.All<targets.RemoveTypeModel.ConcreteTableDescendant2>().ToArray();
        Assert.That(query13.Length, Is.EqualTo(2));

        var query14 = session.Query.All<targets.RemoveTypeModel.ConcreteTableDescendant11>().ToArray();
        Assert.That(query14.Length, Is.EqualTo(1));

        var query15 = session.Query.All<targets.RemoveTypeModel.ConcreteTableDescendant21>().ToArray();
        Assert.That(query15.Length, Is.EqualTo(1));
      }
    }

    [Test]
    public void RemoveTypeWithoutHintTest()
    {
      var initialConfiguration = BuildInitialDomainConfiguration(GetType().Assembly, typeof(sources.RemoveTypeModel.SingleTableHierarchyBase).Namespace);

      using (var domain = Domain.Build(initialConfiguration))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new sources.RemoveTypeModel.SingleTableHierarchyBase {
          SinlgeTableHBField = Guid.NewGuid().ToString()
        };
        new sources.RemoveTypeModel.SingleTableDescendant1 {
          SinlgeTableHBField = Guid.NewGuid().ToString(),
          SingleTableD1Field = Guid.NewGuid().ToString()
        };
        new sources.RemoveTypeModel.SingleTableDescendant11 {
          SinlgeTableHBField = Guid.NewGuid().ToString(),
          SingleTableD1Field = Guid.NewGuid().ToString(),
          SingleTableD11Field = Guid.NewGuid().ToString()
        };
        new sources.RemoveTypeModel.SingleTableDescendant12Removed {
          SinlgeTableHBField = Guid.NewGuid().ToString(),
          SingleTableD1Field = Guid.NewGuid().ToString(),
          SingleTableD12Field = Guid.NewGuid().ToString()
        };
        new sources.RemoveTypeModel.SingleTableDescendant2 {
          SinlgeTableHBField = Guid.NewGuid().ToString(),
          SingleTableD2Field = Guid.NewGuid().ToString()
        };
        new sources.RemoveTypeModel.SingleTableDescendant21 {
          SinlgeTableHBField = Guid.NewGuid().ToString(),
          SingleTableD2Field = Guid.NewGuid().ToString(),
          SingleTableD21Field = Guid.NewGuid().ToString()
        };
        new sources.RemoveTypeModel.SingleTableDescendant22Removed {
          SinlgeTableHBField = Guid.NewGuid().ToString(),
          SingleTableD2Field = Guid.NewGuid().ToString(),
          SingleTableD22Field = Guid.NewGuid().ToString()
        };

        new sources.RemoveTypeModel.ClassTableHierarchyBase {
          ClassTableHBField = Guid.NewGuid().ToString(),
        };
        new sources.RemoveTypeModel.ClassTableDescendant1 {
          ClassTableHBField = Guid.NewGuid().ToString(),
          ClassTableD1Field = Guid.NewGuid().ToString(),
        };
        new sources.RemoveTypeModel.ClassTableDescendant11 {
          ClassTableHBField = Guid.NewGuid().ToString(),
          ClassTableD1Field = Guid.NewGuid().ToString(),
          ClassTableD11Field = Guid.NewGuid().ToString()
        };
        new sources.RemoveTypeModel.ClassTableDescendant12Removed {
          ClassTableHBField = Guid.NewGuid().ToString(),
          ClassTableD1Field = Guid.NewGuid().ToString(),
          ClassTableD12Field = Guid.NewGuid().ToString()
        };
        new sources.RemoveTypeModel.ClassTableDescendant2 {
          ClassTableHBField = Guid.NewGuid().ToString(),
          ClassTableD2Field = Guid.NewGuid().ToString(),
        };
        new sources.RemoveTypeModel.ClassTableDescendant21 {
          ClassTableHBField = Guid.NewGuid().ToString(),
          ClassTableD2Field = Guid.NewGuid().ToString(),
          ClassTableD21Field = Guid.NewGuid().ToString()
        };
        new sources.RemoveTypeModel.ClassTableDescendant22Removed {
          ClassTableHBField = Guid.NewGuid().ToString(),
          ClassTableD2Field = Guid.NewGuid().ToString(),
          ClassTableD22Field = Guid.NewGuid().ToString()
        };

        new sources.RemoveTypeModel.ConcreteTableHierarchyBase {
          ConcreteTableHBField = Guid.NewGuid().ToString(),
        };
        new sources.RemoveTypeModel.ConcreteTableDescendant1 {
          ConcreteTableHBField = Guid.NewGuid().ToString(),
          ConcreteTableD1Field = Guid.NewGuid().ToString()
        };
        new sources.RemoveTypeModel.ConcreteTableDescendant11 {
          ConcreteTableHBField = Guid.NewGuid().ToString(),
          ConcreteTableD1Field = Guid.NewGuid().ToString(),
          ConcreteTableD11Field = Guid.NewGuid().ToString()
        };
        new sources.RemoveTypeModel.ConcreteTableDescendant12Removed {
          ConcreteTableHBField = Guid.NewGuid().ToString(),
          ConcreteTableD1Field = Guid.NewGuid().ToString(),
          ConcreteTableD12Field = Guid.NewGuid().ToString()
        };
        new sources.RemoveTypeModel.ConcreteTableDescendant2 {
          ConcreteTableHBField = Guid.NewGuid().ToString(),
          ConcreteTableD2Field = Guid.NewGuid().ToString()
        };
        new sources.RemoveTypeModel.ConcreteTableDescendant21 {
          ConcreteTableHBField = Guid.NewGuid().ToString(),
          ConcreteTableD2Field = Guid.NewGuid().ToString(),
          ConcreteTableD21Field = Guid.NewGuid().ToString()
        };
        new sources.RemoveTypeModel.ConcreteTableDescendant22Removed {
          ConcreteTableHBField = Guid.NewGuid().ToString(),
          ConcreteTableD2Field = Guid.NewGuid().ToString(),
          ConcreteTableD22Field = Guid.NewGuid().ToString()
        };
        transaction.Complete();
      }

      var finalConfiguration = BuildFinalDomainConfiguration(GetType().Assembly, typeof(targets.RemoveTypeModel.SingleTableHierarchyBase).Namespace, false);

      Assert.Throws<SchemaSynchronizationException>(() => BuildDomain(finalConfiguration));
    }

    [Test]
    public void RenameFieldWithHintTest()
    {
      var initialConfiguration = BuildInitialDomainConfiguration(GetType().Assembly, typeof (sources.RenameFieldModel.SingleTableHierarchyBase).Namespace);

      var intValues = Enumerable.Range(10, 10).ToArray();

      using (var domain = BuildDomain(initialConfiguration))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new sources.RenameFieldModel.SingleTableHierarchyBase {
          FieldWithWrongName = intValues[0],
          SingleTableHBField = Guid.NewGuid().ToString()
        };
        new sources.RenameFieldModel.SingleTableDescendant {
          FieldWithWrongName = intValues[1],
          AnotherFieldWithWrongName = (double)intValues[2],
          SingleTableHBField = Guid.NewGuid().ToString()
        };
        new sources.RenameFieldModel.ClassTableHierarchyBase {
          FieldWithWrongName = intValues[3],
          ClassTableHBField = Guid.NewGuid().ToString()
        };
        new sources.RenameFieldModel.ClassTableDescendant {
          FieldWithWrongName = intValues[4],
          AnotherFieldWithWrongName = (double)intValues[5],
          ClassTableHBField = Guid.NewGuid().ToString()
        };
        new sources.RenameFieldModel.ConcreteTableHierarchyBase {
          FieldWithWrongName = intValues[6],
          ConcreteTableHBField = Guid.NewGuid().ToString()
        };
        new sources.RenameFieldModel.ConcreteTableDescendant {
          FieldWithWrongName = intValues[7],
          AnotherFieldWithWrongName = (double)intValues[8],
          ConcreteTableHBField = Guid.NewGuid().ToString()
        };
        transaction.Complete();
      }

      var finalConfiguration = BuildFinalDomainConfiguration(GetType().Assembly, typeof (targets.RenameFieldModel.SingleTableHierarchyBase).Namespace, true);

      using (var domain = BuildDomain(finalConfiguration))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query1 = session.Query.All<targets.RenameFieldModel.SingleTableHierarchyBase>().OrderBy(el => el.Id).ToArray();
        Assert.That(query1.Length, Is.EqualTo(2));
        Assert.That(query1[0].FieldWithRightName, Is.EqualTo(intValues[0]));
        Assert.That(query1[0].SingleTableHBField.IsNullOrEmpty(), Is.False);
        Assert.That(query1[1] is targets.RenameFieldModel.SingleTableDescendant, Is.True);
        var singleTableDescendant = query1[1] as targets.RenameFieldModel.SingleTableDescendant;
        Assert.That(singleTableDescendant.FieldWithRightName, Is.EqualTo(intValues[1]));
        Assert.That(singleTableDescendant.AnotherFieldWithRightName, Is.EqualTo((double)intValues[2]));
        Assert.That(singleTableDescendant.SingleTableHBField.IsNullOrEmpty(), Is.False);

        var query2 = session.Query.All<targets.RenameFieldModel.ClassTableHierarchyBase>().OrderBy(el => el.Id).ToArray();
        Assert.That(query2.Length, Is.EqualTo(2));
        Assert.That(query2[0].FieldWithRightName, Is.EqualTo(intValues[3]));
        Assert.That(query2[0].ClassTableHBField.IsNullOrEmpty(), Is.False);
        Assert.That(query2[1] is targets.RenameFieldModel.ClassTableDescendant, Is.True);
        var classTableDescendant = query2[1] as targets.RenameFieldModel.ClassTableDescendant;
        Assert.That(classTableDescendant.FieldWithRightName, Is.EqualTo(intValues[4]));
        Assert.That(classTableDescendant.AnotherFieldWithRightName, Is.EqualTo((double)intValues[5]));
        Assert.That(classTableDescendant.ClassTableHBField.IsNullOrEmpty(), Is.False);

        var query3 = session.Query.All<targets.RenameFieldModel.ConcreteTableHierarchyBase>().OrderBy(el => el.Id).ToArray();
        Assert.That(query3.Length, Is.EqualTo(2));
        Assert.That(query3[0].FieldWithRightName, Is.EqualTo(intValues[6]));
        Assert.That(query3[0].ConcreteTableHBField.IsNullOrEmpty(), Is.False);
        Assert.That(query3[1] is targets.RenameFieldModel.ConcreteTableDescendant, Is.True);
        var concreteTableDescendant = query3[1] as targets.RenameFieldModel.ConcreteTableDescendant;
        Assert.That(concreteTableDescendant.FieldWithRightName, Is.EqualTo(intValues[7]));
        Assert.That(concreteTableDescendant.AnotherFieldWithRightName, Is.EqualTo((double)intValues[8]));
        Assert.That(concreteTableDescendant.ConcreteTableHBField.IsNullOrEmpty(), Is.False);
      }
    }

    [Test]
    public void RenameFieldWithoutHingTest()
    {
      var initialConfiguration = BuildInitialDomainConfiguration(GetType().Assembly, typeof (sources.RenameFieldModel.SingleTableHierarchyBase).Namespace);

      var intValues = Enumerable.Range(10, 10).ToArray();

      using (var domain = BuildDomain(initialConfiguration))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new sources.RenameFieldModel.SingleTableHierarchyBase {
          FieldWithWrongName = intValues[0],
          SingleTableHBField = Guid.NewGuid().ToString()
        };
        new sources.RenameFieldModel.SingleTableDescendant {
          FieldWithWrongName = intValues[1],
          AnotherFieldWithWrongName = (double)intValues[2],
          SingleTableHBField = Guid.NewGuid().ToString()
        };
        new sources.RenameFieldModel.ClassTableHierarchyBase {
          FieldWithWrongName = intValues[3],
          ClassTableHBField = Guid.NewGuid().ToString()
        };
        new sources.RenameFieldModel.ClassTableDescendant {
          FieldWithWrongName = intValues[4],
          AnotherFieldWithWrongName = (double)intValues[5],
          ClassTableHBField = Guid.NewGuid().ToString()
        };
        new sources.RenameFieldModel.ConcreteTableHierarchyBase {
          FieldWithWrongName = intValues[6],
          ConcreteTableHBField = Guid.NewGuid().ToString()
        };
        new sources.RenameFieldModel.ConcreteTableDescendant {
          FieldWithWrongName = intValues[7],
          AnotherFieldWithWrongName = (double)intValues[8],
          ConcreteTableHBField = Guid.NewGuid().ToString()
        };
        transaction.Complete();
      }

      var finalConfiguration = BuildFinalDomainConfiguration(GetType().Assembly, typeof (targets.RenameFieldModel.SingleTableHierarchyBase).Namespace, false);

      Assert.Throws<SchemaSynchronizationException>(() => BuildDomain(finalConfiguration));

      finalConfiguration = BuildFinalDomainConfiguration(GetType().Assembly, typeof(targets.RenameFieldModel.SingleTableHierarchyBase).Namespace, true);

      using (var domain = BuildDomain(finalConfiguration))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query1 = session.Query.All<targets.RenameFieldModel.SingleTableHierarchyBase>().OrderBy(el => el.Id).ToArray();
        Assert.That(query1.Length, Is.EqualTo(2));
        Assert.That(query1[0].FieldWithRightName, Is.EqualTo(intValues[0]));
        Assert.That(query1[0].SingleTableHBField.IsNullOrEmpty(), Is.False);
        Assert.That(query1[1] is targets.RenameFieldModel.SingleTableDescendant, Is.True);
        var singleTableDescendant = query1[1] as targets.RenameFieldModel.SingleTableDescendant;
        Assert.That(singleTableDescendant.FieldWithRightName, Is.EqualTo(intValues[1]));
        Assert.That(singleTableDescendant.AnotherFieldWithRightName, Is.EqualTo((double)intValues[2]));
        Assert.That(singleTableDescendant.SingleTableHBField.IsNullOrEmpty(), Is.False);

        var query2 = session.Query.All<targets.RenameFieldModel.ClassTableHierarchyBase>().OrderBy(el => el.Id).ToArray();
        Assert.That(query2.Length, Is.EqualTo(2));
        Assert.That(query2[0].FieldWithRightName, Is.EqualTo(intValues[3]));
        Assert.That(query2[0].ClassTableHBField.IsNullOrEmpty(), Is.False);
        Assert.That(query2[1] is targets.RenameFieldModel.ClassTableDescendant, Is.True);
        var classTableDescendant = query2[1] as targets.RenameFieldModel.ClassTableDescendant;
        Assert.That(classTableDescendant.FieldWithRightName, Is.EqualTo(intValues[4]));
        Assert.That(classTableDescendant.AnotherFieldWithRightName, Is.EqualTo((double)intValues[5]));
        Assert.That(classTableDescendant.ClassTableHBField.IsNullOrEmpty(), Is.False);

        var query3 = session.Query.All<targets.RenameFieldModel.ConcreteTableHierarchyBase>().OrderBy(el => el.Id).ToArray();
        Assert.That(query3.Length, Is.EqualTo(2));
        Assert.That(query3[0].FieldWithRightName, Is.EqualTo(intValues[6]));
        Assert.That(query3[0].ConcreteTableHBField.IsNullOrEmpty(), Is.False);
        Assert.That(query3[1] is targets.RenameFieldModel.ConcreteTableDescendant, Is.True);
        var concreteTableDescendant = query3[1] as targets.RenameFieldModel.ConcreteTableDescendant;
        Assert.That(concreteTableDescendant.FieldWithRightName, Is.EqualTo(intValues[7]));
        Assert.That(concreteTableDescendant.AnotherFieldWithRightName, Is.EqualTo((double)intValues[8]));
        Assert.That(concreteTableDescendant.ConcreteTableHBField.IsNullOrEmpty(), Is.False);
      }
    }

    [Test]
    public void RenameTypeWithHintTest()
    {
      var initialConfiguration = BuildInitialDomainConfiguration(GetType().Assembly, typeof (sources.RenameTypeModel.SingleTableHierarchyBase).Namespace);

      using (var domain = BuildDomain(initialConfiguration))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new sources.RenameTypeModel.SingleTableHierarchyBase();
        new sources.RenameTypeModel.SingleTableDescendant1 {SomeStringField = Guid.NewGuid().ToString()};

        new sources.RenameTypeModel.ClassTableHierarchyBase();
        new sources.RenameTypeModel.ClassTableDescendant1 {SomeStringField = Guid.NewGuid().ToString()};

        new sources.RenameTypeModel.ConcreteTableHierarchyBase();
        new sources.RenameTypeModel.ConcreteTableDescendant1 {SomeStringField = Guid.NewGuid().ToString()};
        transaction.Complete();
      }

      var finalConfiguration = BuildFinalDomainConfiguration(GetType().Assembly, typeof(targets.RenameTypeModel.SingleTableHierarchyBase).Namespace, true);

      using (var domain = BuildDomain(finalConfiguration))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query1 = session.Query.All<targets.RenameTypeModel.SingleTableHierarchyBase>().ToArray();
        Assert.That(query1.Length, Is.EqualTo(2));
        var query2 = session.Query.All<targets.RenameTypeModel.SingleTableDescendant>().ToArray();
        Assert.That(query2.Length, Is.EqualTo(1));
        Assert.That(query2[0].SomeStringField.IsNullOrEmpty(), Is.False);

        var query3 = session.Query.All<targets.RenameTypeModel.ConcreteTableHierarchyBase>().ToArray();
        Assert.That(query3.Length, Is.EqualTo(2));
        var query4 = session.Query.All<targets.RenameTypeModel.ConcreteTableDescendant>().ToArray();
        Assert.That(query4.Length, Is.EqualTo(1));
        Assert.That(query4[0].SomeStringField.IsNullOrEmpty(), Is.False);

        var query5 = session.Query.All<targets.RenameTypeModel.ClassTableHierarchyBase>().ToArray();
        Assert.That(query5.Length, Is.EqualTo(2));
        var query6 = session.Query.All<targets.RenameTypeModel.ClassTableDescendant>().ToArray();
        Assert.That(query6.Length, Is.EqualTo(1));
        Assert.That(query6[0].SomeStringField.IsNullOrEmpty(), Is.False);
      }
    }

    [Test]
    public void RenameTypeWithoutHintTest()
    {
      var initialConfiguration = BuildInitialDomainConfiguration(GetType().Assembly, typeof(sources.RenameTypeModel.SingleTableHierarchyBase).Namespace);

      using (var domain = BuildDomain(initialConfiguration))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new sources.RenameTypeModel.SingleTableHierarchyBase();
        new sources.RenameTypeModel.SingleTableDescendant1 { SomeStringField = Guid.NewGuid().ToString() };

        new sources.RenameTypeModel.ClassTableHierarchyBase();
        new sources.RenameTypeModel.ClassTableDescendant1 { SomeStringField = Guid.NewGuid().ToString() };

        new sources.RenameTypeModel.ConcreteTableHierarchyBase();
        new sources.RenameTypeModel.ConcreteTableDescendant1 { SomeStringField = Guid.NewGuid().ToString() };
        transaction.Complete();
      }

      var finalConfiguration = BuildFinalDomainConfiguration(GetType().Assembly, typeof(targets.RenameTypeModel.SingleTableHierarchyBase).Namespace, false);
      Assert.Throws<SchemaSynchronizationException>(() => BuildDomain(finalConfiguration));

      finalConfiguration = BuildFinalDomainConfiguration(GetType().Assembly, typeof(targets.RenameTypeModel.SingleTableHierarchyBase).Namespace, true);

      using (var domain = BuildDomain(finalConfiguration))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query1 = session.Query.All<targets.RenameTypeModel.SingleTableHierarchyBase>().ToArray();
        Assert.That(query1.Length, Is.EqualTo(2));
        var query2 = session.Query.All<targets.RenameTypeModel.SingleTableDescendant>().ToArray();
        Assert.That(query2.Length, Is.EqualTo(1));
        Assert.That(query2[0].SomeStringField.IsNullOrEmpty(), Is.False);

        var query3 = session.Query.All<targets.RenameTypeModel.ConcreteTableHierarchyBase>().ToArray();
        Assert.That(query3.Length, Is.EqualTo(2));
        var query4 = session.Query.All<targets.RenameTypeModel.ConcreteTableDescendant>().ToArray();
        Assert.That(query4.Length, Is.EqualTo(1));
        Assert.That(query4[0].SomeStringField.IsNullOrEmpty(), Is.False);

        var query5 = session.Query.All<targets.RenameTypeModel.ClassTableHierarchyBase>().ToArray();
        Assert.That(query5.Length, Is.EqualTo(2));
        var query6 = session.Query.All<targets.RenameTypeModel.ClassTableDescendant>().ToArray();
        Assert.That(query6.Length, Is.EqualTo(1));
        Assert.That(query6[0].SomeStringField.IsNullOrEmpty(), Is.False);
      }
    }

    [Test]
    public void MoveFieldToLastDestendantTest()
    {
      var initialConfiguration = BuildInitialDomainConfiguration(GetType().Assembly, typeof (sources.MoveFieldModel.SingleTableHierarchyBase).Namespace);

      using (var domain = BuildDomain(initialConfiguration))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new sources.MoveFieldModel.SingleTableHierarchyBase {
          SingleTableHBField = Guid.NewGuid().ToString(),
          MovableField = "SingleTableHierarchyBaseMovableFieldValue"
        };
        new sources.MoveFieldModel.SingleTableDescendant1() {
          SingleTableHBField = Guid.NewGuid().ToString(),
          MovableField = "SingleTableDescendant1MovableFieldValue",
          Comment = Guid.NewGuid().ToString()
        };
        new sources.MoveFieldModel.SingleTableDescendant2() {
          SingleTableHBField = Guid.NewGuid().ToString(),
          MovableField = "SingleTableDescendant2MovableFieldValue",
          Comment = Guid.NewGuid().ToString()
        };

        new sources.MoveFieldModel.ClassTableHierarchyBase {
          ClassTableHBField = Guid.NewGuid().ToString(),
          MovableField = "SingleTableHierarchyBaseMovableFieldValue"
        };
        new sources.MoveFieldModel.ClassTableDescendant1 {
          ClassTableHBField = Guid.NewGuid().ToString(),
          MovableField = "SingleTableDescendant1MovableFieldValue",
          Comment = Guid.NewGuid().ToString()
        };
        new sources.MoveFieldModel.ClassTableDescendant2 {
          ClassTableHBField = Guid.NewGuid().ToString(),
          MovableField = "SingleTableDescendant2MovableFieldValue",
          Comment = Guid.NewGuid().ToString()
        };

        new sources.MoveFieldModel.ConcreteTableHierarchyBase {
          ConcreteTableHBField = Guid.NewGuid().ToString(),
          MovableField = "SingleTableHierarchyBaseMovableFieldValue"
        };
        new sources.MoveFieldModel.ConcreteTableDescendant1 {
          ConcreteTableHBField = Guid.NewGuid().ToString(),
          MovableField = "SingleTableDescendant1MovableFieldValue",
          Comment = Guid.NewGuid().ToString()
        };
        new sources.MoveFieldModel.ConcreteTableDescendant2 {
          ConcreteTableHBField = Guid.NewGuid().ToString(),
          MovableField = "SingleTableDescendant2MovableFieldValue",
          Comment = Guid.NewGuid().ToString()
        };
        transaction.Complete();
      }

      var finalConfiguration = BuildFinalDomainConfiguration(GetType().Assembly, typeof (targets.MoveFieldToLastDescendantModel.SingleTableHierarchyBase).Namespace, true);

      using (var domain = Domain.Build(finalConfiguration))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query1 = session.Query.All<targets.MoveFieldToLastDescendantModel.SingleTableDescendant2>().ToArray();
        Assert.That(query1.Length, Is.EqualTo(1));
        Assert.That(query1[0].MovableField.IsNullOrEmpty(), Is.False);

        var query2 = session.Query.All<targets.MoveFieldToLastDescendantModel.ConcreteTableDescendant2>().ToArray();
        Assert.That(query2.Length, Is.EqualTo(1));
        Assert.That(query2[0].MovableField.IsNullOrEmpty(), Is.False);

        var query3 = session.Query.All<targets.MoveFieldToLastDescendantModel.ClassTableDescendant2>().ToArray();
        Assert.That(query3.Length, Is.EqualTo(1));
        Assert.That(query3[0].MovableField.IsNullOrEmpty(), Is.False);
      }
    }

    [Test]
    public void MoveFieldToDirectDescendantTest()
    {
      var initialConfiguration = BuildInitialDomainConfiguration(GetType().Assembly, typeof (sources.MoveFieldModel.SingleTableHierarchyBase).Namespace);
      
      using (var domain = BuildDomain(initialConfiguration))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new sources.MoveFieldModel.SingleTableHierarchyBase {
          SingleTableHBField = Guid.NewGuid().ToString(),
          MovableField = "SingleTableHierarchyBaseMovableFieldValue"
        };
        new sources.MoveFieldModel.SingleTableDescendant1 {
          SingleTableHBField = Guid.NewGuid().ToString(),
          MovableField = "SingleTableDescendant1MovableFieldValue",
          Comment = Guid.NewGuid().ToString()
        };
        new sources.MoveFieldModel.SingleTableDescendant2 {
          SingleTableHBField = Guid.NewGuid().ToString(),
          MovableField = "SingleTableDescendant2MovableFieldValue",
          Comment = Guid.NewGuid().ToString()
        };

        new sources.MoveFieldModel.ClassTableHierarchyBase {
          ClassTableHBField = Guid.NewGuid().ToString(),
          MovableField = "SingleTableHierarchyBaseMovableFieldValue"
        };
        new sources.MoveFieldModel.ClassTableDescendant1 {
          ClassTableHBField = Guid.NewGuid().ToString(),
          MovableField = "SingleTableDescendant1MovableFieldValue",
          Comment = Guid.NewGuid().ToString()
        };
        new sources.MoveFieldModel.ClassTableDescendant2 {
          ClassTableHBField = Guid.NewGuid().ToString(),
          MovableField = "SingleTableDescendant2MovableFieldValue",
          Comment = Guid.NewGuid().ToString()
        };

        new sources.MoveFieldModel.ConcreteTableHierarchyBase {
          ConcreteTableHBField = Guid.NewGuid().ToString(),
          MovableField = "SingleTableHierarchyBaseMovableFieldValue"
        };
        new sources.MoveFieldModel.ConcreteTableDescendant1 {
          ConcreteTableHBField = Guid.NewGuid().ToString(),
          MovableField = "SingleTableDescendant1MovableFieldValue",
          Comment = Guid.NewGuid().ToString()
        };
        new sources.MoveFieldModel.ConcreteTableDescendant2 {
          ConcreteTableHBField = Guid.NewGuid().ToString(),
          MovableField = "SingleTableDescendant2MovableFieldValue",
          Comment = Guid.NewGuid().ToString()
        };
        transaction.Complete();
      }

      var finalConfiguration = BuildFinalDomainConfiguration(GetType().Assembly, typeof (targets.MoveFieldToFirstDescendantModel.SingleTableHierarchyBase).Namespace, true);

      using (var domain = BuildDomain(finalConfiguration))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query1 = session.Query.All<targets.MoveFieldToFirstDescendantModel.SingleTableDescendant1>().ToArray();
        Assert.That(query1.Length, Is.EqualTo(2));
        Assert.That(query1[0].MovableField.IsNullOrEmpty(), Is.False);
        Assert.That(query1[1].MovableField.IsNullOrEmpty(), Is.False);

        var query2 = session.Query.All<targets.MoveFieldToFirstDescendantModel.ConcreteTableDescendant1>().ToArray();
        Assert.That(query2.Length, Is.EqualTo(2));
        Assert.That(query2[0].MovableField.IsNullOrEmpty(), Is.False);
        Assert.That(query2[1].MovableField.IsNullOrEmpty(), Is.False);

        var query3 = session.Query.All<targets.MoveFieldToFirstDescendantModel.ClassTableDescendant1>().ToArray();
        Assert.That(query3.Length, Is.EqualTo(2));
        Assert.That(query3[0].MovableField.IsNullOrEmpty(), Is.False);
        Assert.That(query3[1].MovableField.IsNullOrEmpty(), Is.False);
      }
    }

    protected abstract NamingConvention CreateNamingConvention();

    protected virtual DomainConfiguration BuildInitialDomainConfiguration(Assembly assembly, string @namespace)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.NamingConvention = GetNamingConvention();
      configuration.Types.Register(assembly, @namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    protected virtual DomainConfiguration BuildFinalDomainConfiguration(Assembly assembly, string @namespace, bool withUpgrader)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.NamingConvention = GetNamingConvention();
      foreach (var type in assembly.GetTypes().Where(t => t.Namespace==@namespace))
        configuration.Types.Register(type);

      if (withUpgrader) {
        var type = Type.GetType(@namespace + ".UpgradeHandlers.CustomUpgradeHandler");
        configuration.Types.Register(type);
      }
      configuration.UpgradeMode = DomainUpgradeMode.PerformSafely;
      return configuration;
    }

    protected NamingConvention GetNamingConvention()
    {
      lock (guard) {
        if (namingConvention!=null)
          return namingConvention;
        namingConvention = CreateNamingConvention();
        return namingConvention;
      }
    }

    protected Domain BuildDomain(DomainConfiguration configuration)
    {
      try {
        return Domain.Build(configuration);
      }
      catch (Exception e) {
        TestLog.Error(GetType().GetFullName());
        TestLog.Error(e);
        throw;
      }
    }
  }
}
