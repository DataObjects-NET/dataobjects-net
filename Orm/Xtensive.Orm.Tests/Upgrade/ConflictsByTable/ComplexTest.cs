// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Modelling.Actions;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Modifiers = Xtensive.Orm.Tests.Upgrade.ConflictsByTable.ComplexTestModel.Modifiers;
using Orig = Xtensive.Orm.Tests.Upgrade.ConflictsByTable.ComplexTestModel.Original;
using Case1 = Xtensive.Orm.Tests.Upgrade.ConflictsByTable.ComplexTestModel.Case1;
using Case2 = Xtensive.Orm.Tests.Upgrade.ConflictsByTable.ComplexTestModel.Case2;
using Case3 = Xtensive.Orm.Tests.Upgrade.ConflictsByTable.ComplexTestModel.Case3;
using Case4 = Xtensive.Orm.Tests.Upgrade.ConflictsByTable.ComplexTestModel.Case4;
using Case5 = Xtensive.Orm.Tests.Upgrade.ConflictsByTable.ComplexTestModel.Case5;
using Case6 = Xtensive.Orm.Tests.Upgrade.ConflictsByTable.ComplexTestModel.Case6;
using Case7 = Xtensive.Orm.Tests.Upgrade.ConflictsByTable.ComplexTestModel.Case7;
using Case8 = Xtensive.Orm.Tests.Upgrade.ConflictsByTable.ComplexTestModel.Case8;

namespace Xtensive.Orm.Tests.Upgrade.ConflictsByTable
{
  public class ComplexTest
  {
    private class TypeIds
    {
      public int Root;
      public int Middle1;
      public int Middle2;
      public int Leaf1;
      public int Leaf2;
      public int Leaf3;
      public int Leaf4;
    }

    private TypeIds BuildInitialDomain(InheritanceSchema inheritanceSchema)
    {
      Require.ProviderIsNot(StorageProvider.Sqlite, "SQlite has some problems with table changes");

      var domainConfig = DomainConfigurationFactory.Create();
      RegisterTypes(domainConfig, inheritanceSchema, "Original");
      domainConfig.UpgradeMode = DomainUpgradeMode.Recreate;

      var typeIds = new TypeIds();
      using (var init = Domain.Build(domainConfig))
      using (var session = init.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var root = new Orig.Root(session, "Root");

        var middle1 = new Orig.Middle1(session, "Middle1");
        var leaf1 = new Orig.Leaf1(session, "Leaf1");
        var leaf2 = new Orig.Leaf2(session, "Leaf2");

        var middle2 = new Orig.Middle2(session, "Middle2");
        var leaf3 = new Orig.Leaf3(session, "Leaf3");
        var leaf4 = new Orig.Leaf4(session, "Leaf4");

        var rootRef = new Orig.RootRef(session);
        rootRef.RefRoot = root;
        _ = rootRef.RootItems.Add(root);
        _ = rootRef.RootItems.Add(middle1);
        _ = rootRef.RootItems.Add(leaf1);
        _ = rootRef.RootItems.Add(leaf2);
        _ = rootRef.RootItems.Add(middle2);
        _ = rootRef.RootItems.Add(leaf3);
        _ = rootRef.RootItems.Add(leaf4);

        var middle1Ref = new Orig.MiddleRef1(session);
        middle1Ref.RefMiddle = middle1;
        _ = middle1Ref.MiddleItems.Add(middle1);
        _ = middle1Ref.MiddleItems.Add(leaf1);
        _ = middle1Ref.MiddleItems.Add(leaf2);

        var leaf1Ref = new Orig.LeafRef1(session);
        leaf1Ref.RefLeaf = leaf1;
        _ = leaf1Ref.LeafItems.Add(leaf1);

        var leaf2Ref = new Orig.LeafRef2(session);
        leaf2Ref.RefLeaf = leaf2;
        _ = leaf2Ref.LeafItems.Add(leaf2);

        var middle2Ref = new Orig.MiddleRef2(session);
        middle2Ref.RefMiddle = middle2;
        _ = middle2Ref.MiddleItems.Add(middle2);
        _ = middle2Ref.MiddleItems.Add(leaf3);
        _ = middle2Ref.MiddleItems.Add(leaf4);

        var leaf3Ref = new Orig.LeafRef3(session);
        leaf3Ref.RefLeaf = leaf3;
        _ = leaf3Ref.LeafItems.Add(leaf3);

        var leaf4Ref = new Orig.LeafRef4(session);
        leaf4Ref.RefLeaf = leaf4;
        _ = leaf4Ref.LeafItems.Add(leaf4);

        typeIds.Root = root.TypeId;
        typeIds.Middle1 = middle1.TypeId;
        typeIds.Middle2 = middle2.TypeId;
        typeIds.Leaf1 = leaf1.TypeId;
        typeIds.Leaf2 = leaf2.TypeId;
        typeIds.Leaf3 = leaf3.TypeId;
        typeIds.Leaf4 = leaf4.TypeId;

        tx.Complete();
      }
      return typeIds;
    }

    private void RegisterTypes(DomainConfiguration domainConfig, InheritanceSchema inheritanceSchema, string @case)
    {
      var thisType = GetType();
      var ns = $"{thisType.FullName}Model.{@case}";
      domainConfig.Types.Register(thisType.Assembly, ns);
      switch (inheritanceSchema) {
        case InheritanceSchema.ClassTable:    domainConfig.Types.Register(typeof(Modifiers.MakeClassTable)); break;
        case InheritanceSchema.ConcreteTable: domainConfig.Types.Register(typeof(Modifiers.MakeConcreteTable)); break;
        default : throw new ArgumentOutOfRangeException();
      }
    }

    [Test]
    [TestCase(InheritanceSchema.ClassTable)]
    [TestCase(InheritanceSchema.ConcreteTable)]
    public void Case1PerformSafelyTest(InheritanceSchema inheritanceSchema)
    {
      var casePrefix = nameof(Case1);
      var ids = BuildInitialDomain(inheritanceSchema);

      var domainConfig = DomainConfigurationFactory.Create();
      RegisterTypes(domainConfig, inheritanceSchema, casePrefix);
      domainConfig.UpgradeMode = DomainUpgradeMode.PerformSafely;

      var exception = Assert.Throws<SchemaSynchronizationException>(() => Domain.Build(domainConfig));
      switch (inheritanceSchema) {
        case InheritanceSchema.ClassTable: {
          var unsafeActions = exception.ComparisonResult.UnsafeActions;
          Assert.That(unsafeActions.Count, Is.EqualTo(6));
          var dataActions = unsafeActions.OfType<DataAction>().ToList();
          Assert.That(dataActions.Count, Is.EqualTo(6));

          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Root") && a.DataHint.Identities[0].Target.Contains(ids.Leaf4.ToString()) ));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Middle2") && a.DataHint.Identities[0].Target.Contains(ids.Leaf4.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Leaf4") && a.DataHint.Identities[0].Target.Contains(ids.Leaf4.ToString())));

          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[1].Target.Contains(ids.Leaf4.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef2-MiddleItems-Middle2") && a.DataHint.Identities[1].Target.Contains(ids.Leaf4.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/LeafRef4-LeafItems-Leaf4") && a.DataHint.Identities[1].Target.Contains(ids.Leaf4.ToString())));
          break;
        }
        case InheritanceSchema.ConcreteTable: {
          var unsafeActions = exception.ComparisonResult.UnsafeActions;
          Assert.That(unsafeActions.Count, Is.EqualTo(4));
          var dataActions = unsafeActions.OfType<DataAction>().ToList();
          Assert.That(dataActions.Count, Is.EqualTo(4));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Leaf4") && a.DataHint.Identities.Count==0));

          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf4")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef2-MiddleItems-Middle2") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf4")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/LeafRef4-LeafItems-Leaf4") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf4")));
          break;
        }
      }
    }

    [Test]
    [TestCase(InheritanceSchema.ClassTable)]
    [TestCase(InheritanceSchema.ConcreteTable)]
    public void Case1PerformTest(InheritanceSchema inheritanceSchema)
    {
      var casePrefix = nameof(Case1);

      _ = BuildInitialDomain(inheritanceSchema);

      var domainConfig = DomainConfigurationFactory.Create();
      RegisterTypes(domainConfig, inheritanceSchema, casePrefix);
      domainConfig.UpgradeMode = DomainUpgradeMode.Perform;

      Domain domain = null;
      Assert.DoesNotThrow(() => domain = Domain.Build(domainConfig));
      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var rootItems = session.Query.All<Case1.Root>().ToList();
        Assert.That(rootItems.Count, Is.EqualTo(6));

        var middle1Items = session.Query.All<Case1.Middle1>().ToList();
        Assert.That(middle1Items.Count, Is.EqualTo(3));

        var leaf1Items = session.Query.All<Case1.Leaf1>().ToList();
        Assert.That(leaf1Items.Count, Is.EqualTo(1));

        var leaf2Items = session.Query.All<Case1.Leaf2>().ToList();
        Assert.That(leaf2Items.Count, Is.EqualTo(1));

        var middle2Items = session.Query.All<Case1.Middle2>().ToList();
        Assert.That(middle2Items.Count, Is.EqualTo(2));

        var leaf3Items = session.Query.All<Case1.Leaf3>().ToList();
        Assert.That(leaf3Items.Count, Is.EqualTo(1));

        var leaf4Items = session.Query.All<Case1.Leaf41>().ToList();
        Assert.That(leaf4Items.Count, Is.EqualTo(0));

        var rootRef = session.Query.All<Case1.RootRef>().FirstOrDefault();
        Assert.That(rootRef, Is.Not.Null);
        Assert.That(rootRef.RootItems.Count(), Is.EqualTo(6));

        var middle1Ref = session.Query.All<Case1.MiddleRef1>().FirstOrDefault();
        Assert.That(middle1Ref, Is.Not.Null);
        Assert.That(middle1Ref.MiddleItems.Count(), Is.EqualTo(3));

        var middle2Ref = session.Query.All<Case1.MiddleRef2>().FirstOrDefault();
        Assert.That(middle2Ref, Is.Not.Null);
        Assert.That(middle2Ref.MiddleItems.Count(), Is.EqualTo(2));

        var leaf1Ref = session.Query.All<Case1.LeafRef1>().FirstOrDefault();
        Assert.That(leaf1Ref, Is.Not.Null);
        Assert.That(leaf1Ref.LeafItems.Count(), Is.EqualTo(1));

        var leaf2Ref = session.Query.All<Case1.LeafRef2>().FirstOrDefault();
        Assert.That(leaf2Ref, Is.Not.Null);
        Assert.That(leaf2Ref.LeafItems.Count(), Is.EqualTo(1));

        var leaf3Ref = session.Query.All<Case1.LeafRef3>().FirstOrDefault();
        Assert.That(leaf3Ref, Is.Not.Null);
        Assert.That(leaf3Ref.LeafItems.Count(), Is.EqualTo(1));

        var leaf4Ref = session.Query.All<Case1.LeafRef4>().FirstOrDefault();
        Assert.That(leaf4Ref, Is.Not.Null);
        Assert.That(leaf4Ref.RefLeaf, Is.Null);
        Assert.That(leaf4Ref.LeafItems.Count(), Is.EqualTo(0));
      }
    }

    [Test]
    [TestCase(InheritanceSchema.ClassTable)]
    [TestCase(InheritanceSchema.ConcreteTable)]
    public void Case2PerformSafelyTest(InheritanceSchema inheritanceSchema)
    {
      var casePrefix = nameof(Case2);
      var ids = BuildInitialDomain(inheritanceSchema);

      var domainConfig = DomainConfigurationFactory.Create();
      RegisterTypes(domainConfig, inheritanceSchema, casePrefix);
      domainConfig.UpgradeMode = DomainUpgradeMode.PerformSafely;

      var exception = Assert.Throws<SchemaSynchronizationException>(() => Domain.Build(domainConfig));
      switch (inheritanceSchema) {
        case InheritanceSchema.ClassTable: {
          var unsafeActions = exception.ComparisonResult.UnsafeActions;
          Assert.That(unsafeActions.Count, Is.EqualTo(12));
          var dataActions = unsafeActions.OfType<DataAction>().ToList();
          Assert.That(dataActions.Count, Is.EqualTo(12));
          Assert.That(dataActions.Any( a => a.Path.Contains("Tables/Root") && a.DataHint.Identities[0].Target.Contains(ids.Leaf4.ToString())));
          Assert.That(dataActions.Any( a => a.Path.Contains("Tables/Root") && a.DataHint.Identities[0].Target.Contains(ids.Leaf3.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Middle2") && a.DataHint.Identities[0].Target.Contains(ids.Leaf4.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Middle2") && a.DataHint.Identities[0].Target.Contains(ids.Leaf3.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Leaf3") && a.DataHint.Identities[0].Target.Contains(ids.Leaf3.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Leaf4") && a.DataHint.Identities[0].Target.Contains(ids.Leaf4.ToString())));

          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[1].Target.Contains(ids.Leaf3.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[1].Target.Contains(ids.Leaf4.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef2-MiddleItems-Middle2") && a.DataHint.Identities[1].Target.Contains(ids.Leaf3.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef2-MiddleItems-Middle2") && a.DataHint.Identities[1].Target.Contains(ids.Leaf4.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/LeafRef3-LeafItems-Leaf3") && a.DataHint.Identities[1].Target.Contains(ids.Leaf3.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/LeafRef4-LeafItems-Leaf4") && a.DataHint.Identities[1].Target.Contains(ids.Leaf4.ToString())));
          break;
        }
        case InheritanceSchema.ConcreteTable: {
          var unsafeActions = exception.ComparisonResult.UnsafeActions;
          Assert.That(unsafeActions.Count, Is.EqualTo(8));
          var dataActions = unsafeActions.OfType<DataAction>().ToList();
          Assert.That(dataActions.Count, Is.EqualTo(8));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Leaf4") && a.DataHint.Identities.Count == 0));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Leaf3") && a.DataHint.Identities.Count == 0));

          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf3")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf4")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef2-MiddleItems-Middle2") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf3")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef2-MiddleItems-Middle2") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf4")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/LeafRef3-LeafItems-Leaf3") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf3")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/LeafRef4-LeafItems-Leaf4") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf4")));
          break;
        }
      }
    }

    [Test]
    [TestCase(InheritanceSchema.ClassTable)]
    [TestCase(InheritanceSchema.ConcreteTable)]
    public void Case2PerfromTest(InheritanceSchema inheritanceSchema)
    {
      var casePrefix = nameof(Case2);
      _ = BuildInitialDomain(inheritanceSchema);

      var domainConfig = DomainConfigurationFactory.Create();
      RegisterTypes(domainConfig, inheritanceSchema, casePrefix);
      domainConfig.UpgradeMode = DomainUpgradeMode.Perform;

      using (var upgrade = Domain.Build(domainConfig))
      using (var session = upgrade.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var rootItems = session.Query.All<Case2.Root>().ToList();
        Assert.That(rootItems.Count, Is.EqualTo(5));

        var middle1Items = session.Query.All<Case2.Middle1>().ToList();
        Assert.That(middle1Items.Count, Is.EqualTo(3));

        var leaf1Items = session.Query.All<Case2.Leaf1>().ToList();
        Assert.That(leaf1Items.Count, Is.EqualTo(1));

        var leaf2Items = session.Query.All<Case2.Leaf2>().ToList();
        Assert.That(leaf2Items.Count, Is.EqualTo(1));

        var middle2Items = session.Query.All<Case2.Middle2>().ToList();
        Assert.That(middle2Items.Count, Is.EqualTo(1));

        var leaf3Items = session.Query.All<Case2.Leaf31>().ToList();
        Assert.That(leaf3Items.Count, Is.EqualTo(0));

        var leaf4Items = session.Query.All<Case2.Leaf41>().ToList();
        Assert.That(leaf4Items.Count, Is.EqualTo(0));

        var rootRef = session.Query.All<Case2.RootRef>().FirstOrDefault();
        Assert.That(rootRef, Is.Not.Null);
        Assert.That(rootRef.RootItems.Count(), Is.EqualTo(5));

        var middle1Ref = session.Query.All<Case2.MiddleRef1>().FirstOrDefault();
        Assert.That(middle1Ref, Is.Not.Null);
        Assert.That(middle1Ref.MiddleItems.Count(), Is.EqualTo(3));

        var middle2Ref = session.Query.All<Case2.MiddleRef2>().FirstOrDefault();
        Assert.That(middle2Ref, Is.Not.Null);
        Assert.That(middle2Ref.MiddleItems.Count(), Is.EqualTo(1));

        var leaf1Ref = session.Query.All<Case2.LeafRef1>().FirstOrDefault();
        Assert.That(leaf1Ref, Is.Not.Null);
        Assert.That(leaf1Ref.LeafItems.Count(), Is.EqualTo(1));

        var leaf2Ref = session.Query.All<Case2.LeafRef2>().FirstOrDefault();
        Assert.That(leaf2Ref, Is.Not.Null);
        Assert.That(leaf2Ref.LeafItems.Count(), Is.EqualTo(1));

        var leaf3Ref = session.Query.All<Case2.LeafRef3>().FirstOrDefault();
        Assert.That(leaf3Ref, Is.Not.Null);
        Assert.That(leaf3Ref.RefLeaf, Is.Null);
        Assert.That(leaf3Ref.LeafItems.Count(), Is.EqualTo(0));

        var leaf4Ref = session.Query.All<Case2.LeafRef4>().FirstOrDefault();
        Assert.That(leaf4Ref, Is.Not.Null);
        Assert.That(leaf4Ref.RefLeaf, Is.Null);
        Assert.That(leaf4Ref.LeafItems.Count(), Is.EqualTo(0));
      }
    }

    [Test]
    [TestCase(InheritanceSchema.ClassTable)]
    [TestCase(InheritanceSchema.ConcreteTable)]
    public void Case3PerformSafelyTest(InheritanceSchema inheritanceSchema)
    {
      var casePrefix = nameof(Case3);
      var ids = BuildInitialDomain(inheritanceSchema);

      var domainConfig = DomainConfigurationFactory.Create();
      RegisterTypes(domainConfig, inheritanceSchema, casePrefix);
      domainConfig.UpgradeMode = DomainUpgradeMode.PerformSafely;

      var exception = Assert.Throws<SchemaSynchronizationException>(() => Domain.Build(domainConfig));
      switch (inheritanceSchema) {
        case InheritanceSchema.ClassTable: {
          var unsafeActions = exception.ComparisonResult.UnsafeActions;
          Assert.That(unsafeActions.Count, Is.EqualTo(18));
          var dataActions = unsafeActions.OfType<DataAction>().ToList();
          Assert.That(dataActions.Count, Is.EqualTo(18));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Root") && a.DataHint.Identities[0].Target.Contains(ids.Leaf4.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Root") && a.DataHint.Identities[0].Target.Contains(ids.Leaf3.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Root") && a.DataHint.Identities[0].Target.Contains(ids.Leaf2.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Middle1") && a.DataHint.Identities[0].Target.Contains(ids.Leaf2.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Middle2") && a.DataHint.Identities[0].Target.Contains(ids.Leaf4.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Middle2") && a.DataHint.Identities[0].Target.Contains(ids.Leaf3.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Leaf2") && a.DataHint.Identities[0].Target.Contains(ids.Leaf2.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Leaf3") && a.DataHint.Identities[0].Target.Contains(ids.Leaf3.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Leaf4") && a.DataHint.Identities[0].Target.Contains(ids.Leaf4.ToString())));

          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[1].Target.Contains(ids.Leaf2.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[1].Target.Contains(ids.Leaf3.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[1].Target.Contains(ids.Leaf4.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef1-MiddleItems-Middle1") && a.DataHint.Identities[1].Target.Contains(ids.Leaf2.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef2-MiddleItems-Middle2") && a.DataHint.Identities[1].Target.Contains(ids.Leaf3.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef2-MiddleItems-Middle2") && a.DataHint.Identities[1].Target.Contains(ids.Leaf4.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/LeafRef2-LeafItems-Leaf2") && a.DataHint.Identities[1].Target.Contains(ids.Leaf2.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/LeafRef3-LeafItems-Leaf3") && a.DataHint.Identities[1].Target.Contains(ids.Leaf3.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/LeafRef4-LeafItems-Leaf4") && a.DataHint.Identities[1].Target.Contains(ids.Leaf4.ToString())));
          break;
        }
        case InheritanceSchema.ConcreteTable: {
          var unsafeActions = exception.ComparisonResult.UnsafeActions;
          Assert.That(unsafeActions.Count, Is.EqualTo(12));
          var dataActions = unsafeActions.OfType<DataAction>().ToList();
          Assert.That(dataActions.Count, Is.EqualTo(12));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Leaf2") && a.DataHint.Identities.Count == 0));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Leaf3") && a.DataHint.Identities.Count == 0));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Leaf4") && a.DataHint.Identities.Count == 0));

          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf2")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf3")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf4")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef1-MiddleItems-Middle1") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf2")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef2-MiddleItems-Middle2") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf3")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef2-MiddleItems-Middle2") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf4")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/LeafRef2-LeafItems-Leaf2") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf2")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/LeafRef3-LeafItems-Leaf3") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf3")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/LeafRef4-LeafItems-Leaf4") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf4")));
          break;
        }
      }
    }

    [Test]
    [TestCase(InheritanceSchema.ClassTable)]
    [TestCase(InheritanceSchema.ConcreteTable)]
    public void Case3PerformTest(InheritanceSchema inheritanceSchema)
    {
      var casePrefix = nameof(Case3);
      _ = BuildInitialDomain(inheritanceSchema);

      var domainConfig = DomainConfigurationFactory.Create();
      RegisterTypes(domainConfig, inheritanceSchema, casePrefix);
      domainConfig.UpgradeMode = DomainUpgradeMode.Perform;

      using (var upgrade = Domain.Build(domainConfig))
      using (var session = upgrade.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var rootItems = session.Query.All<Case3.Root>().ToList();
        Assert.That(rootItems.Count, Is.EqualTo(4));

        var middle1Items = session.Query.All<Case3.Middle1>().ToList();
        Assert.That(middle1Items.Count, Is.EqualTo(2));

        var leaf1Items = session.Query.All<Case3.Leaf1>().ToList();
        Assert.That(leaf1Items.Count, Is.EqualTo(1));

        var leaf2Items = session.Query.All<Case3.Leaf21>().ToList();
        Assert.That(leaf2Items.Count, Is.EqualTo(0));

        var middle2Items = session.Query.All<Case3.Middle2>().ToList();
        Assert.That(middle2Items.Count, Is.EqualTo(1));

        var leaf3Items = session.Query.All<Case3.Leaf31>().ToList();
        Assert.That(leaf3Items.Count, Is.EqualTo(0));

        var leaf4Items = session.Query.All<Case3.Leaf41>().ToList();
        Assert.That(leaf4Items.Count, Is.EqualTo(0));

        var rootRef = session.Query.All<Case3.RootRef>().FirstOrDefault();
        Assert.That(rootRef, Is.Not.Null);
        Assert.That(rootRef.RootItems.Count(), Is.EqualTo(4));

        var middle1Ref = session.Query.All<Case3.MiddleRef1>().FirstOrDefault();
        Assert.That(middle1Ref, Is.Not.Null);
        Assert.That(middle1Ref.MiddleItems.Count(), Is.EqualTo(2));

        var middle2Ref = session.Query.All<Case3.MiddleRef2>().FirstOrDefault();
        Assert.That(middle2Ref, Is.Not.Null);
        Assert.That(middle2Ref.MiddleItems.Count(), Is.EqualTo(1));

        var leaf1Ref = session.Query.All<Case3.LeafRef1>().FirstOrDefault();
        Assert.That(leaf1Ref, Is.Not.Null);
        Assert.That(leaf1Ref.LeafItems.Count(), Is.EqualTo(1));

        var leaf2Ref = session.Query.All<Case3.LeafRef2>().FirstOrDefault();
        Assert.That(leaf2Ref, Is.Not.Null);
        Assert.That(leaf2Ref.RefLeaf, Is.Null);
        Assert.That(leaf2Ref.LeafItems.Count(), Is.EqualTo(0));

        var leaf3Ref = session.Query.All<Case3.LeafRef3>().FirstOrDefault();
        Assert.That(leaf3Ref, Is.Not.Null);
        Assert.That(leaf3Ref.RefLeaf, Is.Null);
        Assert.That(leaf3Ref.LeafItems.Count(), Is.EqualTo(0));

        var leaf4Ref = session.Query.All<Case3.LeafRef4>().FirstOrDefault();
        Assert.That(leaf4Ref, Is.Not.Null);
        Assert.That(leaf4Ref.RefLeaf, Is.Null);
        Assert.That(leaf4Ref.LeafItems.Count(), Is.EqualTo(0));
      }
    }

    [Test]
    [TestCase(InheritanceSchema.ClassTable)]
    [TestCase(InheritanceSchema.ConcreteTable)]
    public void Case4PerformSafelyTest(InheritanceSchema inheritanceSchema)
    {
      var casePrefix = nameof(Case4);
      var ids = BuildInitialDomain(inheritanceSchema);

      var domainConfig = DomainConfigurationFactory.Create();
      RegisterTypes(domainConfig, inheritanceSchema, casePrefix);
      domainConfig.UpgradeMode = DomainUpgradeMode.PerformSafely;

      var exception = Assert.Throws<SchemaSynchronizationException>(() => Domain.Build(domainConfig));
      switch (inheritanceSchema) {
        case InheritanceSchema.ClassTable: {
          var unsafeActions = exception.ComparisonResult.UnsafeActions;
          Assert.That(unsafeActions.Count, Is.EqualTo(24));
          var dataActions = unsafeActions.OfType<DataAction>().ToList();
          Assert.That(dataActions.Count, Is.EqualTo(24));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Root") && a.DataHint.Identities[0].Target.Contains(ids.Leaf4.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Root") && a.DataHint.Identities[0].Target.Contains(ids.Leaf3.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Root") && a.DataHint.Identities[0].Target.Contains(ids.Leaf2.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Root") && a.DataHint.Identities[0].Target.Contains(ids.Leaf1.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Middle1") && a.DataHint.Identities[0].Target.Contains(ids.Leaf1.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Middle1") && a.DataHint.Identities[0].Target.Contains(ids.Leaf2.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Middle2") && a.DataHint.Identities[0].Target.Contains(ids.Leaf3.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Middle2") && a.DataHint.Identities[0].Target.Contains(ids.Leaf4.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Leaf1") && a.DataHint.Identities[0].Target.Contains(ids.Leaf1.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Leaf2") && a.DataHint.Identities[0].Target.Contains(ids.Leaf2.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Leaf3") && a.DataHint.Identities[0].Target.Contains(ids.Leaf3.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Leaf4") && a.DataHint.Identities[0].Target.Contains(ids.Leaf4.ToString())));

          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[1].Target.Contains(ids.Leaf1.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[1].Target.Contains(ids.Leaf2.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[1].Target.Contains(ids.Leaf3.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[1].Target.Contains(ids.Leaf4.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef1-MiddleItems-Middle1") && a.DataHint.Identities[1].Target.Contains(ids.Leaf1.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef1-MiddleItems-Middle1") && a.DataHint.Identities[1].Target.Contains(ids.Leaf2.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef2-MiddleItems-Middle2") && a.DataHint.Identities[1].Target.Contains(ids.Leaf3.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef2-MiddleItems-Middle2") && a.DataHint.Identities[1].Target.Contains(ids.Leaf4.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/LeafRef1-LeafItems-Leaf1") && a.DataHint.Identities[1].Target.Contains(ids.Leaf1.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/LeafRef2-LeafItems-Leaf2") && a.DataHint.Identities[1].Target.Contains(ids.Leaf2.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/LeafRef3-LeafItems-Leaf3") && a.DataHint.Identities[1].Target.Contains(ids.Leaf3.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/LeafRef4-LeafItems-Leaf4") && a.DataHint.Identities[1].Target.Contains(ids.Leaf4.ToString())));
          break;
        }
        case InheritanceSchema.ConcreteTable: {
          var unsafeActions = exception.ComparisonResult.UnsafeActions;
          Assert.That(unsafeActions.Count, Is.EqualTo(16));
          var dataActions = unsafeActions.OfType<DataAction>().ToList();
          Assert.That(dataActions.Count, Is.EqualTo(16));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Leaf1") && a.DataHint.Identities.Count == 0));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Leaf2") && a.DataHint.Identities.Count == 0));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Leaf3") && a.DataHint.Identities.Count == 0));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Leaf4") && a.DataHint.Identities.Count == 0));

          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf1")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf2")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf3")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf4")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef1-MiddleItems-Middle1") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf1")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef1-MiddleItems-Middle1") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf2")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef2-MiddleItems-Middle2") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf3")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef2-MiddleItems-Middle2") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf4")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/LeafRef1-LeafItems-Leaf1") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf1")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/LeafRef2-LeafItems-Leaf2") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf2")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/LeafRef3-LeafItems-Leaf3") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf3")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/LeafRef4-LeafItems-Leaf4") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf4")));
          break;
        }
      }
    }

    [Test]
    [TestCase(InheritanceSchema.ClassTable)]
    [TestCase(InheritanceSchema.ConcreteTable)]
    public void Case4PerformTest(InheritanceSchema inheritanceSchema)
    {
      var casePrefix = nameof(Case4);
      _ = BuildInitialDomain(inheritanceSchema);

      var domainConfig = DomainConfigurationFactory.Create();
      RegisterTypes(domainConfig, inheritanceSchema, casePrefix);
      domainConfig.UpgradeMode = DomainUpgradeMode.Perform;

      using (var upgrade = Domain.Build(domainConfig))
      using (var session = upgrade.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var rootItems = session.Query.All<Case4.Root>().ToList();
        Assert.That(rootItems.Count, Is.EqualTo(3));

        var middle1Items = session.Query.All<Case4.Middle1>().ToList();
        Assert.That(middle1Items.Count, Is.EqualTo(1));

        var leaf1Items = session.Query.All<Case4.Leaf11>().ToList();
        Assert.That(leaf1Items.Count, Is.EqualTo(0));

        var leaf2Items = session.Query.All<Case4.Leaf21>().ToList();
        Assert.That(leaf2Items.Count, Is.EqualTo(0));

        var middle2Items = session.Query.All<Case4.Middle2>().ToList();
        Assert.That(middle2Items.Count, Is.EqualTo(1));

        var leaf3Items = session.Query.All<Case4.Leaf31>().ToList();
        Assert.That(leaf3Items.Count, Is.EqualTo(0));

        var leaf4Items = session.Query.All<Case4.Leaf41>().ToList();
        Assert.That(leaf4Items.Count, Is.EqualTo(0));

        var rootRef = session.Query.All<Case4.RootRef>().FirstOrDefault();
        Assert.That(rootRef, Is.Not.Null);
        Assert.That(rootRef.RootItems.Count(), Is.EqualTo(3));

        var middle1Ref = session.Query.All<Case4.MiddleRef1>().FirstOrDefault();
        Assert.That(middle1Ref, Is.Not.Null);
        Assert.That(middle1Ref.MiddleItems.Count(), Is.EqualTo(1));

        var middle2Ref = session.Query.All<Case4.MiddleRef2>().FirstOrDefault();
        Assert.That(middle2Ref, Is.Not.Null);
        Assert.That(middle2Ref.MiddleItems.Count(), Is.EqualTo(1));

        var leaf1Ref = session.Query.All<Case4.LeafRef1>().FirstOrDefault();
        Assert.That(leaf1Ref, Is.Not.Null);
        Assert.That(leaf1Ref.RefLeaf, Is.Null);
        Assert.That(leaf1Ref.LeafItems.Count(), Is.EqualTo(0));

        var leaf2Ref = session.Query.All<Case4.LeafRef2>().FirstOrDefault();
        Assert.That(leaf2Ref, Is.Not.Null);
        Assert.That(leaf2Ref.RefLeaf, Is.Null);
        Assert.That(leaf2Ref.LeafItems.Count(), Is.EqualTo(0));

        var leaf3Ref = session.Query.All<Case4.LeafRef3>().FirstOrDefault();
        Assert.That(leaf3Ref, Is.Not.Null);
        Assert.That(leaf3Ref.RefLeaf, Is.Null);
        Assert.That(leaf3Ref.LeafItems.Count(), Is.EqualTo(0));

        var leaf4Ref = session.Query.All<Case4.LeafRef4>().FirstOrDefault();
        Assert.That(leaf4Ref, Is.Not.Null);
        Assert.That(leaf4Ref.RefLeaf, Is.Null);
        Assert.That(leaf4Ref.LeafItems.Count(), Is.EqualTo(0));
      }
    }

    [Test]
    [TestCase(InheritanceSchema.ClassTable)]
    [TestCase(InheritanceSchema.ConcreteTable)]
    public void Case5PerformSafelyTest(InheritanceSchema inheritanceSchema)
    {
      var casePrefix = nameof(Case5);
      var ids = BuildInitialDomain(inheritanceSchema);

      var domainConfig = DomainConfigurationFactory.Create();
      RegisterTypes(domainConfig, inheritanceSchema, casePrefix);
      domainConfig.UpgradeMode = DomainUpgradeMode.PerformSafely;

      var exception = Assert.Throws<SchemaSynchronizationException>(() => Domain.Build(domainConfig));
      switch (inheritanceSchema) {
        case InheritanceSchema.ClassTable: {
          var unsafeActions = exception.ComparisonResult.UnsafeActions;
          Assert.That(unsafeActions.Count, Is.EqualTo(28));
          var dataActions = unsafeActions.OfType<DataAction>().ToList();
          Assert.That(dataActions.Count, Is.EqualTo(28));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Root") && a.DataHint.Identities[0].Target.Contains(ids.Middle2.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Root") && a.DataHint.Identities[0].Target.Contains(ids.Leaf4.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Root") && a.DataHint.Identities[0].Target.Contains(ids.Leaf3.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Root") && a.DataHint.Identities[0].Target.Contains(ids.Leaf2.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Root") && a.DataHint.Identities[0].Target.Contains(ids.Leaf1.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Middle1") && a.DataHint.Identities[0].Target.Contains(ids.Leaf1.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Middle1") && a.DataHint.Identities[0].Target.Contains(ids.Leaf2.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Middle2") && a.DataHint.Identities[0].Target.Contains(ids.Leaf3.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Middle2") && a.DataHint.Identities[0].Target.Contains(ids.Leaf4.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Middle2") && a.DataHint.Identities[0].Target.Contains(ids.Middle2.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Leaf1") && a.DataHint.Identities[0].Target.Contains(ids.Leaf1.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Leaf2") && a.DataHint.Identities[0].Target.Contains(ids.Leaf2.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Leaf3") && a.DataHint.Identities.Count == 0));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Leaf4") && a.DataHint.Identities.Count == 0));

          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[1].Target.Contains(ids.Leaf1.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[1].Target.Contains(ids.Leaf2.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[1].Target.Contains(ids.Middle2.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[1].Target.Contains(ids.Leaf3.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[1].Target.Contains(ids.Leaf4.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef1-MiddleItems-Middle1") && a.DataHint.Identities[1].Target.Contains(ids.Leaf1.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef1-MiddleItems-Middle1") && a.DataHint.Identities[1].Target.Contains(ids.Leaf2.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef2-MiddleItems-Middle2") && a.DataHint.Identities[1].Target.Contains(ids.Middle2.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef2-MiddleItems-Middle2") && a.DataHint.Identities[1].Target.Contains(ids.Leaf3.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef2-MiddleItems-Middle2") && a.DataHint.Identities[1].Target.Contains(ids.Leaf4.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/LeafRef1-LeafItems-Leaf1") && a.DataHint.Identities[1].Target.Contains(ids.Leaf1.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/LeafRef2-LeafItems-Leaf2") && a.DataHint.Identities[1].Target.Contains(ids.Leaf2.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/LeafRef3-LeafItems-Leaf3") && a.DataHint.Identities[1].Target.Contains(ids.Leaf3.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/LeafRef4-LeafItems-Leaf4") && a.DataHint.Identities[1].Target.Contains(ids.Leaf4.ToString())));
          break;
        }
        case InheritanceSchema.ConcreteTable: {
          var unsafeActions = exception.ComparisonResult.UnsafeActions;
          Assert.That(unsafeActions.Count, Is.EqualTo(19));
          var dataActions = unsafeActions.OfType<DataAction>().ToList();
          Assert.That(dataActions.Count, Is.EqualTo(19));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Middle2") && a.DataHint.Identities.Count == 0));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Leaf1") && a.DataHint.Identities.Count == 0));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Leaf2") && a.DataHint.Identities.Count == 0));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Leaf3") && a.DataHint.Identities.Count == 0));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Leaf4") && a.DataHint.Identities.Count == 0));

          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[0].Target.Contains("Tables/Middle2")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf1")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf2")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf3")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf4")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef1-MiddleItems-Middle1") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf1")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef1-MiddleItems-Middle1") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf2")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef2-MiddleItems-Middle2") && a.DataHint.Identities[0].Target.Contains("Tables/Middle2")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef2-MiddleItems-Middle2") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf3")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef2-MiddleItems-Middle2") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf4")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/LeafRef1-LeafItems-Leaf1") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf1")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/LeafRef2-LeafItems-Leaf2") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf2")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/LeafRef3-LeafItems-Leaf3") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf3")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/LeafRef4-LeafItems-Leaf4") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf4")));
          break;
        }
      }
    }

    [Test]
    [TestCase(InheritanceSchema.ClassTable)]
    [TestCase(InheritanceSchema.ConcreteTable)]
    public void Case5PerfromTest(InheritanceSchema inheritanceSchema)
    {
      var casePrefix = nameof(Case5);
      _ = BuildInitialDomain(inheritanceSchema);

      var domainConfig = DomainConfigurationFactory.Create();
      RegisterTypes(domainConfig, inheritanceSchema, casePrefix);
      domainConfig.UpgradeMode = DomainUpgradeMode.Perform;

      using (var upgrade = Domain.Build(domainConfig))
      using (var session = upgrade.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var rootItems = session.Query.All<Case5.Root>().ToList();
        Assert.That(rootItems.Count, Is.EqualTo(2));

        var middle1Items = session.Query.All<Case5.Middle1>().ToList();
        Assert.That(middle1Items.Count, Is.EqualTo(1));

        var leaf1Items = session.Query.All<Case5.Leaf11>().ToList();
        Assert.That(leaf1Items.Count, Is.EqualTo(0));

        var leaf2Items = session.Query.All<Case5.Leaf21>().ToList();
        Assert.That(leaf2Items.Count, Is.EqualTo(0));

        var middle2Items = session.Query.All<Case5.Middle21>().ToList();
        Assert.That(middle2Items.Count, Is.EqualTo(0));

        var leaf3Items = session.Query.All<Case5.Leaf31>().ToList();
        Assert.That(leaf3Items.Count, Is.EqualTo(0));

        var leaf4Items = session.Query.All<Case5.Leaf41>().ToList();
        Assert.That(leaf4Items.Count, Is.EqualTo(0));

        var rootRef = session.Query.All<Case5.RootRef>().FirstOrDefault();
        Assert.That(rootRef, Is.Not.Null);
        Assert.That(rootRef.RootItems.Count(), Is.EqualTo(2));

        var middle1Ref = session.Query.All<Case5.MiddleRef1>().FirstOrDefault();
        Assert.That(middle1Ref, Is.Not.Null);
        Assert.That(middle1Ref.MiddleItems.Count(), Is.EqualTo(1));

        var middle2Ref = session.Query.All<Case5.MiddleRef2>().FirstOrDefault();
        Assert.That(middle2Ref, Is.Not.Null);
        Assert.That(middle2Ref.RefMiddle, Is.Null);
        Assert.That(middle2Ref.MiddleItems.Count(), Is.EqualTo(0));

        var leaf1Ref = session.Query.All<Case5.LeafRef1>().FirstOrDefault();
        Assert.That(leaf1Ref, Is.Not.Null);
        Assert.That(leaf1Ref.RefLeaf, Is.Null);
        Assert.That(leaf1Ref.LeafItems.Count(), Is.EqualTo(0));

        var leaf2Ref = session.Query.All<Case5.LeafRef2>().FirstOrDefault();
        Assert.That(leaf2Ref, Is.Not.Null);
        Assert.That(leaf2Ref.RefLeaf, Is.Null);
        Assert.That(leaf2Ref.LeafItems.Count(), Is.EqualTo(0));

        var leaf3Ref = session.Query.All<Case5.LeafRef3>().FirstOrDefault();
        Assert.That(leaf3Ref, Is.Not.Null);
        Assert.That(leaf3Ref.RefLeaf, Is.Null);
        Assert.That(leaf3Ref.LeafItems.Count(), Is.EqualTo(0));

        var leaf4Ref = session.Query.All<Case5.LeafRef4>().FirstOrDefault();
        Assert.That(leaf4Ref, Is.Not.Null);
        Assert.That(leaf4Ref.RefLeaf, Is.Null);
        Assert.That(leaf4Ref.LeafItems.Count(), Is.EqualTo(0));
      }
    }

    [Test]
    [TestCase(InheritanceSchema.ClassTable)]
    [TestCase(InheritanceSchema.ConcreteTable)]
    public void Case6PerformSafelyTest(InheritanceSchema inheritanceSchema)
    {
      var casePrefix = nameof(Case6);
      var ids = BuildInitialDomain(inheritanceSchema);

      var domainConfig = DomainConfigurationFactory.Create();
      RegisterTypes(domainConfig, inheritanceSchema, casePrefix);
      domainConfig.UpgradeMode = DomainUpgradeMode.PerformSafely;

      var exception = Assert.Throws<SchemaSynchronizationException>(() => Domain.Build(domainConfig));
      switch (inheritanceSchema) {
        case InheritanceSchema.ClassTable: {
          var unsafeActions = exception.ComparisonResult.UnsafeActions;
          Assert.That(unsafeActions.Count, Is.EqualTo(32));
          var dataActions = unsafeActions.OfType<DataAction>().ToList();
          Assert.That(dataActions.Count, Is.EqualTo(32));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Root") && a.DataHint.Identities[0].Target.Contains(ids.Middle2.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Root") && a.DataHint.Identities[0].Target.Contains(ids.Middle1.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Root") && a.DataHint.Identities[0].Target.Contains(ids.Leaf4.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Root") && a.DataHint.Identities[0].Target.Contains(ids.Leaf3.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Root") && a.DataHint.Identities[0].Target.Contains(ids.Leaf2.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Root") && a.DataHint.Identities[0].Target.Contains(ids.Leaf1.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Middle1") && a.DataHint.Identities[0].Target.Contains(ids.Leaf1.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Middle1") && a.DataHint.Identities[0].Target.Contains(ids.Leaf2.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Middle1") && a.DataHint.Identities[0].Target.Contains(ids.Middle1.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Middle2") && a.DataHint.Identities[0].Target.Contains(ids.Leaf3.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Middle2") && a.DataHint.Identities[0].Target.Contains(ids.Leaf4.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Middle2") && a.DataHint.Identities[0].Target.Contains(ids.Middle2.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Leaf1") && a.DataHint.Identities.Count == 0));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Leaf2") && a.DataHint.Identities.Count == 0));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Leaf3") && a.DataHint.Identities.Count == 0));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Leaf4") && a.DataHint.Identities.Count == 0));

          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[1].Target.Contains(ids.Middle1.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[1].Target.Contains(ids.Leaf1.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[1].Target.Contains(ids.Leaf2.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[1].Target.Contains(ids.Middle2.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[1].Target.Contains(ids.Leaf3.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[1].Target.Contains(ids.Leaf4.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef1-MiddleItems-Middle1") && a.DataHint.Identities[1].Target.Contains(ids.Middle1.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef1-MiddleItems-Middle1") && a.DataHint.Identities[1].Target.Contains(ids.Leaf1.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef1-MiddleItems-Middle1") && a.DataHint.Identities[1].Target.Contains(ids.Leaf2.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef2-MiddleItems-Middle2") && a.DataHint.Identities[1].Target.Contains(ids.Middle2.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef2-MiddleItems-Middle2") && a.DataHint.Identities[1].Target.Contains(ids.Leaf3.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef2-MiddleItems-Middle2") && a.DataHint.Identities[1].Target.Contains(ids.Leaf4.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/LeafRef1-LeafItems-Leaf1") && a.DataHint.Identities[1].Target.Contains(ids.Leaf1.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/LeafRef2-LeafItems-Leaf2") && a.DataHint.Identities[1].Target.Contains(ids.Leaf2.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/LeafRef3-LeafItems-Leaf3") && a.DataHint.Identities[1].Target.Contains(ids.Leaf3.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/LeafRef4-LeafItems-Leaf4") && a.DataHint.Identities[1].Target.Contains(ids.Leaf4.ToString())));
          break;
        }
        case InheritanceSchema.ConcreteTable: {
          var unsafeActions = exception.ComparisonResult.UnsafeActions;
          Assert.That(unsafeActions.Count, Is.EqualTo(22));
          var dataActions = unsafeActions.OfType<DataAction>().ToList();
          Assert.That(dataActions.Count, Is.EqualTo(22));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Middle1") && a.DataHint.Identities.Count == 0));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Middle2") && a.DataHint.Identities.Count == 0));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Leaf1") && a.DataHint.Identities.Count == 0));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Leaf2") && a.DataHint.Identities.Count == 0));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Leaf3") && a.DataHint.Identities.Count == 0));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Leaf4") && a.DataHint.Identities.Count == 0));

          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[0].Target.Contains("Tables/Middle1")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[0].Target.Contains("Tables/Middle2")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf1")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf2")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf3")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf4")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef1-MiddleItems-Middle1") && a.DataHint.Identities[0].Target.Contains("Tables/Middle1")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef1-MiddleItems-Middle1") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf1")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef1-MiddleItems-Middle1") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf2")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef2-MiddleItems-Middle2") && a.DataHint.Identities[0].Target.Contains("Tables/Middle2")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef2-MiddleItems-Middle2") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf3")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef2-MiddleItems-Middle2") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf4")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/LeafRef1-LeafItems-Leaf1") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf1")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/LeafRef2-LeafItems-Leaf2") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf2")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/LeafRef3-LeafItems-Leaf3") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf3")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/LeafRef4-LeafItems-Leaf4") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf4")));
          break;
        }
      }
    }

    [Test]
    [TestCase(InheritanceSchema.ClassTable)]
    [TestCase(InheritanceSchema.ConcreteTable)]
    public void Case6PerformTest(InheritanceSchema inheritanceSchema)
    {
      var casePrefix = nameof(Case6);
      _ = BuildInitialDomain(inheritanceSchema);

      var domainConfig = DomainConfigurationFactory.Create();
      RegisterTypes(domainConfig, inheritanceSchema, casePrefix);
      domainConfig.UpgradeMode = DomainUpgradeMode.Perform;

      using (var upgrade = Domain.Build(domainConfig))
      using (var session = upgrade.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var rootItems = session.Query.All<Case6.Root>().ToList();
        Assert.That(rootItems.Count, Is.EqualTo(1));

        var middle1Items = session.Query.All<Case6.Middle11>().ToList();
        Assert.That(middle1Items.Count, Is.EqualTo(0));

        var leaf1Items = session.Query.All<Case6.Leaf11>().ToList();
        Assert.That(leaf1Items.Count, Is.EqualTo(0));

        var leaf2Items = session.Query.All<Case6.Leaf21>().ToList();
        Assert.That(leaf2Items.Count, Is.EqualTo(0));

        var middle2Items = session.Query.All<Case6.Middle21>().ToList();
        Assert.That(middle2Items.Count, Is.EqualTo(0));

        var leaf3Items = session.Query.All<Case6.Leaf31>().ToList();
        Assert.That(leaf3Items.Count, Is.EqualTo(0));

        var leaf4Items = session.Query.All<Case6.Leaf41>().ToList();
        Assert.That(leaf4Items.Count, Is.EqualTo(0));

        var rootRef = session.Query.All<Case6.RootRef>().FirstOrDefault();
        Assert.That(rootRef, Is.Not.Null);
        Assert.That(rootRef.RootItems.Count(), Is.EqualTo(1));

        var middle1Ref = session.Query.All<Case6.MiddleRef1>().FirstOrDefault();
        Assert.That(middle1Ref, Is.Not.Null);
        Assert.That(middle1Ref.RefMiddle, Is.Null);
        Assert.That(middle1Ref.MiddleItems.Count(), Is.EqualTo(0));

        var middle2Ref = session.Query.All<Case6.MiddleRef2>().FirstOrDefault();
        Assert.That(middle2Ref, Is.Not.Null);
        Assert.That(middle2Ref.RefMiddle, Is.Null);
        Assert.That(middle2Ref.MiddleItems.Count(), Is.EqualTo(0));

        var leaf1Ref = session.Query.All<Case6.LeafRef1>().FirstOrDefault();
        Assert.That(leaf1Ref, Is.Not.Null);
        Assert.That(leaf1Ref.RefLeaf, Is.Null);
        Assert.That(leaf1Ref.LeafItems.Count(), Is.EqualTo(0));

        var leaf2Ref = session.Query.All<Case6.LeafRef2>().FirstOrDefault();
        Assert.That(leaf2Ref, Is.Not.Null);
        Assert.That(leaf2Ref.RefLeaf, Is.Null);
        Assert.That(leaf2Ref.LeafItems.Count(), Is.EqualTo(0));

        var leaf3Ref = session.Query.All<Case6.LeafRef3>().FirstOrDefault();
        Assert.That(leaf3Ref, Is.Not.Null);
        Assert.That(leaf3Ref.RefLeaf, Is.Null);
        Assert.That(leaf3Ref.LeafItems.Count(), Is.EqualTo(0));

        var leaf4Ref = session.Query.All<Case6.LeafRef4>().FirstOrDefault();
        Assert.That(leaf4Ref, Is.Not.Null);
        Assert.That(leaf4Ref.RefLeaf, Is.Null);
        Assert.That(leaf4Ref.LeafItems.Count(), Is.EqualTo(0));
      }
    }

    [Test]
    [TestCase(InheritanceSchema.ClassTable)]
    [TestCase(InheritanceSchema.ConcreteTable)]
    public void Case7PerformSafelyTest(InheritanceSchema inheritanceSchema)
    {
      var casePrefix = nameof(Case7);
      var ids = BuildInitialDomain(inheritanceSchema);

      var domainConfig = DomainConfigurationFactory.Create();
      RegisterTypes(domainConfig, inheritanceSchema, casePrefix);
      domainConfig.UpgradeMode = DomainUpgradeMode.PerformSafely;

      var exception = Assert.Throws<SchemaSynchronizationException>(() => Domain.Build(domainConfig));
      switch (inheritanceSchema) {
        case InheritanceSchema.ClassTable: {
          var unsafeActions = exception.ComparisonResult.UnsafeActions;
          Assert.That(unsafeActions.Count, Is.EqualTo(16));
          var dataActions = unsafeActions.OfType<DataAction>().ToList();
          Assert.That(dataActions.Count, Is.EqualTo(16));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Root") && a.DataHint.Identities[0].Target.Contains(ids.Middle2.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Root") && a.DataHint.Identities[0].Target.Contains(ids.Leaf4.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Root") && a.DataHint.Identities[0].Target.Contains(ids.Leaf3.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Middle2") && a.DataHint.Identities[0].Target.Contains(ids.Leaf3.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Middle2") && a.DataHint.Identities[0].Target.Contains(ids.Leaf4.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Middle2") && a.DataHint.Identities[0].Target.Contains(ids.Middle2.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Leaf3") && a.DataHint.Identities.Count == 0));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Leaf4") && a.DataHint.Identities.Count == 0));

          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[1].Target.Contains(ids.Middle2.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[1].Target.Contains(ids.Leaf3.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[1].Target.Contains(ids.Leaf4.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef2-MiddleItems-Middle2") && a.DataHint.Identities[1].Target.Contains(ids.Middle2.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef2-MiddleItems-Middle2") && a.DataHint.Identities[1].Target.Contains(ids.Leaf3.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef2-MiddleItems-Middle2") && a.DataHint.Identities[1].Target.Contains(ids.Leaf4.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/LeafRef3-LeafItems-Leaf3") && a.DataHint.Identities[1].Target.Contains(ids.Leaf3.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/LeafRef4-LeafItems-Leaf4") && a.DataHint.Identities[1].Target.Contains(ids.Leaf4.ToString())));
          break;
        }
        case InheritanceSchema.ConcreteTable: {
          var unsafeActions = exception.ComparisonResult.UnsafeActions;
          Assert.That(unsafeActions.Count, Is.EqualTo(11));
          var dataActions = unsafeActions.OfType<DataAction>().ToList();
          Assert.That(dataActions.Count, Is.EqualTo(11));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Middle2") && a.DataHint.Identities.Count == 0));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Leaf3") && a.DataHint.Identities.Count == 0));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Leaf4") && a.DataHint.Identities.Count == 0));

          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[0].Target.Contains("Tables/Middle2")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf3")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf4")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef2-MiddleItems-Middle2") && a.DataHint.Identities[0].Target.Contains("Tables/Middle2")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef2-MiddleItems-Middle2") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf3")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef2-MiddleItems-Middle2") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf4")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/LeafRef3-LeafItems-Leaf3") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf3")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/LeafRef4-LeafItems-Leaf4") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf4")));
          break;
        }
      }
    }

    [Test]
    [TestCase(InheritanceSchema.ClassTable)]
    [TestCase(InheritanceSchema.ConcreteTable)]
    public void Case7PerformTest(InheritanceSchema inheritanceSchema)
    {
      var casePrefix = nameof(Case7);
      _ = BuildInitialDomain(inheritanceSchema);

      var domainConfig = DomainConfigurationFactory.Create();
      RegisterTypes(domainConfig, inheritanceSchema, casePrefix);
      domainConfig.UpgradeMode = DomainUpgradeMode.Perform;

      using (var upgrade = Domain.Build(domainConfig))
      using (var session = upgrade.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var rootItems = session.Query.All<Case7.Root>().ToList();
        Assert.That(rootItems.Count, Is.EqualTo(4));

        var middle1Items = session.Query.All<Case7.Middle1>().ToList();
        Assert.That(middle1Items.Count, Is.EqualTo(3));

        var leaf1Items = session.Query.All<Case7.Leaf1>().ToList();
        Assert.That(leaf1Items.Count, Is.EqualTo(1));

        var leaf2Items = session.Query.All<Case7.Leaf2>().ToList();
        Assert.That(leaf2Items.Count, Is.EqualTo(1));

        var middle2Items = session.Query.All<Case7.Middle21>().ToList();
        Assert.That(middle2Items.Count, Is.EqualTo(0));

        var leaf3Items = session.Query.All<Case7.Leaf3>().ToList();
        Assert.That(leaf3Items.Count, Is.EqualTo(0));

        var leaf4Items = session.Query.All<Case7.Leaf4>().ToList();
        Assert.That(leaf4Items.Count, Is.EqualTo(0));

        var rootRef = session.Query.All<Case7.RootRef>().FirstOrDefault();
        Assert.That(rootRef, Is.Not.Null);
        Assert.That(rootRef.RootItems.Count(), Is.EqualTo(4));

        var middle1Ref = session.Query.All<Case7.MiddleRef1>().FirstOrDefault();
        Assert.That(middle1Ref, Is.Not.Null);
        Assert.That(middle1Ref.MiddleItems.Count(), Is.EqualTo(3));

        var middle2Ref = session.Query.All<Case7.MiddleRef2>().FirstOrDefault();
        Assert.That(middle2Ref, Is.Not.Null);
        Assert.That(middle2Ref.RefMiddle, Is.Null);
        Assert.That(middle2Ref.MiddleItems.Count(), Is.EqualTo(0));

        var leaf1Ref = session.Query.All<Case7.LeafRef1>().FirstOrDefault();
        Assert.That(leaf1Ref, Is.Not.Null);
        Assert.That(leaf1Ref.LeafItems.Count(), Is.EqualTo(1));

        var leaf2Ref = session.Query.All<Case7.LeafRef2>().FirstOrDefault();
        Assert.That(leaf2Ref, Is.Not.Null);
        Assert.That(leaf2Ref.LeafItems.Count(), Is.EqualTo(1));

        var leaf3Ref = session.Query.All<Case7.LeafRef3>().FirstOrDefault();
        Assert.That(leaf3Ref, Is.Not.Null);
        Assert.That(leaf3Ref.RefLeaf, Is.Null);
        Assert.That(leaf3Ref.LeafItems.Count(), Is.EqualTo(0));

        var leaf4Ref = session.Query.All<Case7.LeafRef4>().FirstOrDefault();
        Assert.That(leaf4Ref, Is.Not.Null);
        Assert.That(leaf4Ref.RefLeaf, Is.Null);
        Assert.That(leaf4Ref.LeafItems.Count(), Is.EqualTo(0));
      }
    }

    [Test]
    [TestCase(InheritanceSchema.ClassTable)]
    [TestCase(InheritanceSchema.ConcreteTable)]
    public void Case8PerformSafelyTest(InheritanceSchema inheritanceSchema)
    {
      var casePrefix = nameof(Case8);

      var ids = BuildInitialDomain(inheritanceSchema);

      var domainConfig = DomainConfigurationFactory.Create();
      RegisterTypes(domainConfig, inheritanceSchema, casePrefix);
      domainConfig.UpgradeMode = DomainUpgradeMode.PerformSafely;

      var exception = Assert.Throws<SchemaSynchronizationException>(() => Domain.Build(domainConfig));
      switch (inheritanceSchema) {
        case InheritanceSchema.ClassTable: {
          var unsafeActions = exception.ComparisonResult.UnsafeActions;
          Assert.That(unsafeActions.Count, Is.EqualTo(16));
          var dataActions = unsafeActions.OfType<DataAction>().ToList();
          Assert.That(dataActions.Count, Is.EqualTo(16));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Root") && a.DataHint.Identities[0].Target.Contains(ids.Middle1.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Root") && a.DataHint.Identities[0].Target.Contains(ids.Leaf2.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Root") && a.DataHint.Identities[0].Target.Contains(ids.Leaf1.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Middle1") && a.DataHint.Identities[0].Target.Contains(ids.Leaf1.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Middle1") && a.DataHint.Identities[0].Target.Contains(ids.Leaf2.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Middle1") && a.DataHint.Identities[0].Target.Contains(ids.Middle1.ToString())));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Leaf1") && a.DataHint.Identities.Count == 0));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Leaf2") && a.DataHint.Identities.Count == 0));

          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[1].Target.Contains(ids.Middle1.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[1].Target.Contains(ids.Leaf1.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[1].Target.Contains(ids.Leaf2.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef1-MiddleItems-Middle1") && a.DataHint.Identities[1].Target.Contains(ids.Middle1.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef1-MiddleItems-Middle1") && a.DataHint.Identities[1].Target.Contains(ids.Leaf1.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef1-MiddleItems-Middle1") && a.DataHint.Identities[1].Target.Contains(ids.Leaf2.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/LeafRef1-LeafItems-Leaf1") && a.DataHint.Identities[1].Target.Contains(ids.Leaf1.ToString())));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/LeafRef2-LeafItems-Leaf2") && a.DataHint.Identities[1].Target.Contains(ids.Leaf2.ToString())));
          break;
        }
        case InheritanceSchema.ConcreteTable: {
          var unsafeActions = exception.ComparisonResult.UnsafeActions;
          Assert.That(unsafeActions.Count, Is.EqualTo(11));
          var dataActions = unsafeActions.OfType<DataAction>().ToList();
          Assert.That(dataActions.Count, Is.EqualTo(11));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Middle1") && a.DataHint.Identities.Count == 0));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Leaf1") && a.DataHint.Identities.Count == 0));
          Assert.That(dataActions.Any(a => a.Path.Contains("Tables/Leaf2") && a.DataHint.Identities.Count == 0));

          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[0].Target.Contains("Tables/Middle1")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf1")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/RootRef-RootItems-Root") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf2")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef1-MiddleItems-Middle1") && a.DataHint.Identities[0].Target.Contains("Tables/Middle1")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef1-MiddleItems-Middle1") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf1")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/MiddleRef1-MiddleItems-Middle1") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf2")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/LeafRef2-LeafItems-Leaf2") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf2")));
          Assert.That(dataActions.Any(
            a => a.Path.Contains("Tables/LeafRef1-LeafItems-Leaf1") && a.DataHint.Identities[0].Target.Contains("Tables/Leaf1")));
          break;
        }
      }
    }

    [Test]
    [TestCase(InheritanceSchema.ClassTable)]
    [TestCase(InheritanceSchema.ConcreteTable)]
    public void Case8PerformTest(InheritanceSchema inheritanceSchema)
    {
      var casePrefix = nameof(Case8);
      _ = BuildInitialDomain(inheritanceSchema);

      var domainConfig = DomainConfigurationFactory.Create();
      RegisterTypes(domainConfig, inheritanceSchema, casePrefix);
      domainConfig.UpgradeMode = DomainUpgradeMode.Perform;

      using (var upgrade = Domain.Build(domainConfig))
      using (var session = upgrade.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var rootItems = session.Query.All<Case8.Root>().ToList();
        Assert.That(rootItems.Count, Is.EqualTo(4));

        var middle1Items = session.Query.All<Case8.Middle11>().ToList();
        Assert.That(middle1Items.Count, Is.EqualTo(0));

        var leaf1Items = session.Query.All<Case8.Leaf1>().ToList();
        Assert.That(leaf1Items.Count, Is.EqualTo(0));

        var leaf2Items = session.Query.All<Case8.Leaf2>().ToList();
        Assert.That(leaf2Items.Count, Is.EqualTo(0));

        var middle2Items = session.Query.All<Case8.Middle2>().ToList();
        Assert.That(middle2Items.Count, Is.EqualTo(3));

        var leaf3Items = session.Query.All<Case8.Leaf3>().ToList();
        Assert.That(leaf3Items.Count, Is.EqualTo(1));

        var leaf4Items = session.Query.All<Case8.Leaf4>().ToList();
        Assert.That(leaf4Items.Count, Is.EqualTo(1));

        var rootRef = session.Query.All<Case8.RootRef>().FirstOrDefault();
        Assert.That(rootRef, Is.Not.Null);
        Assert.That(rootRef.RootItems.Count(), Is.EqualTo(4));

        var middle1Ref = session.Query.All<Case8.MiddleRef1>().FirstOrDefault();
        Assert.That(middle1Ref, Is.Not.Null);
        Assert.That(middle1Ref.RefMiddle, Is.Null);
        Assert.That(middle1Ref.MiddleItems.Count(), Is.EqualTo(0));

        var middle2Ref = session.Query.All<Case8.MiddleRef2>().FirstOrDefault();
        Assert.That(middle2Ref, Is.Not.Null);
        Assert.That(middle2Ref.MiddleItems.Count(), Is.EqualTo(3));

        var leaf1Ref = session.Query.All<Case8.LeafRef1>().FirstOrDefault();
        Assert.That(leaf1Ref, Is.Not.Null);
        Assert.That(leaf1Ref.RefLeaf, Is.Null);
        Assert.That(leaf1Ref.LeafItems.Count(), Is.EqualTo(0));

        var leaf2Ref = session.Query.All<Case8.LeafRef2>().FirstOrDefault();
        Assert.That(leaf2Ref, Is.Not.Null);
        Assert.That(leaf2Ref.RefLeaf, Is.Null);
        Assert.That(leaf2Ref.LeafItems.Count(), Is.EqualTo(0));

        var leaf3Ref = session.Query.All<Case8.LeafRef3>().FirstOrDefault();
        Assert.That(leaf3Ref, Is.Not.Null);
        Assert.That(leaf3Ref.LeafItems.Count(), Is.EqualTo(1));

        var leaf4Ref = session.Query.All<Case8.LeafRef4>().FirstOrDefault();
        Assert.That(leaf4Ref, Is.Not.Null);
        Assert.That(leaf4Ref.LeafItems.Count(), Is.EqualTo(1));
      }
    }
  }
}