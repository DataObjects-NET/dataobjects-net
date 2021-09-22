// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Comparison;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Upgrade;
using OneToOneStructure = Xtensive.Orm.Tests.Upgrade.UpgradeToNewHierarchyThatInheritsStructureTestModel.OneToOneStructure;
using NewColumnWithinTable = Xtensive.Orm.Tests.Upgrade.UpgradeToNewHierarchyThatInheritsStructureTestModel.NewColumnWithinTable;
using RemoveColumnWithinTable = Xtensive.Orm.Tests.Upgrade.UpgradeToNewHierarchyThatInheritsStructureTestModel.RemoveColumnWithinTable;
using NewTable = Xtensive.Orm.Tests.Upgrade.UpgradeToNewHierarchyThatInheritsStructureTestModel.NewTable;
using RemoveTable = Xtensive.Orm.Tests.Upgrade.UpgradeToNewHierarchyThatInheritsStructureTestModel.RemoveTable;

namespace Xtensive.Orm.Tests.Upgrade
{
  [TestFixture]
  public class UpgradeToNewHierarchyThatInheritsStructureTest
  {
    private Domain BuildDomain(DomainUpgradeMode mode, Type[] types)
    {
      var configuration = BuildConfiguration(mode, types);
      return Domain.Build(configuration);
    }

    private Task<Domain> BuildDomainAsync(DomainUpgradeMode mode, Type[] types)
    {
      var configuration = BuildConfiguration(mode, types);
      return Domain.BuildAsync(configuration);
    }

    private DomainConfiguration BuildConfiguration(DomainUpgradeMode mode, Type[] types)
    {
      var configuration = DomainConfigurationFactory.Create();
      types.ForEach(t => configuration.Types.Register(t));
      configuration.UpgradeMode = mode;
      return configuration;
    }

    [Test]
    [TestCase(InheritanceSchema.ClassTable, false)]
    [TestCase(InheritanceSchema.ClassTable, true)]
    [TestCase(InheritanceSchema.ConcreteTable, false)]
    [TestCase(InheritanceSchema.ConcreteTable, true)]
    [TestCase(InheritanceSchema.SingleTable, false)]
    [TestCase(InheritanceSchema.SingleTable, true)]
    public void OneToOneStructurePerformSafelyTest(InheritanceSchema inheritanceSchema, bool asyncBuild)
    {
      var initTypes = GetOneToOneStructureTypes(inheritanceSchema, false);
      BuildAndPopulateInitialDomain(initTypes);

      var upgradeTypes = GetOneToOneStructureTypes(inheritanceSchema, true);
      var ex = (asyncBuild)
        ? Assert.ThrowsAsync<SchemaSynchronizationException>(async () => { await using (await BuildDomainAsync(DomainUpgradeMode.PerformSafely, upgradeTypes)) {}; })
        : Assert.Throws<SchemaSynchronizationException>(() => { using (BuildDomain(DomainUpgradeMode.PerformSafely, upgradeTypes)) {}; });
      Assert.That(ex.ComparisonResult, Is.Not.Null);
      Assert.That(ex.ComparisonResult.HasUnsafeActions, Is.True);
      Assert.That(ex.ComparisonResult.UnsafeActions.Count, Is.EqualTo(1));

      var dataAction = ex.ComparisonResult.UnsafeActions[0] as DataAction;
      Assert.That(dataAction, Is.Not.Null);
      Assert.That(dataAction.Path, Is.EqualTo($"Tables/NamedValue{inheritanceSchema.ToString()}"));
      Assert.That(((NodeDifference) dataAction.Difference).MovementInfo.HasFlag(MovementInfo.Removed | MovementInfo.Created), Is.True);
    }

    [Test]
    [TestCase(InheritanceSchema.ClassTable, false)]
    [TestCase(InheritanceSchema.ClassTable, true)]
    [TestCase(InheritanceSchema.ConcreteTable, false)]
    [TestCase(InheritanceSchema.ConcreteTable, true)]
    [TestCase(InheritanceSchema.SingleTable, false)]
    [TestCase(InheritanceSchema.SingleTable, true)]
    public void OneToOneStructurePerformSafelyWithHintsTest(InheritanceSchema inheritanceSchema, bool asyncBuild)
    {
      var initTypes = GetOneToOneStructureTypes(inheritanceSchema, false);
      BuildAndPopulateInitialDomain(initTypes);

      var upgradeTypes = GetOneToOneStructureTypes(inheritanceSchema, true, true);
      Domain domain = null;
      try {
        if (asyncBuild) {
          Assert.DoesNotThrowAsync(async () => domain = await BuildDomainAsync(DomainUpgradeMode.PerformSafely, upgradeTypes));
        }
        else {
          Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.PerformSafely, upgradeTypes));
        }
      }
      catch (SuccessException) {
        domain.DisposeSafely();
        throw;
      }

      TestDomainContentAndDispose(domain, true);
    }

    [Test]
    [TestCase(InheritanceSchema.ClassTable, false)]
    [TestCase(InheritanceSchema.ClassTable, true)]
    [TestCase(InheritanceSchema.ConcreteTable, false)]
    [TestCase(InheritanceSchema.ConcreteTable, true)]
    [TestCase(InheritanceSchema.SingleTable, false)]
    [TestCase(InheritanceSchema.SingleTable, true)]
    public void OneToOneStructurePerformTest(InheritanceSchema inheritanceSchema, bool asyncBuild)
    {
      var initTypes = GetOneToOneStructureTypes(inheritanceSchema, false);
      BuildAndPopulateInitialDomain(initTypes);

      var upgradeTypes = GetOneToOneStructureTypes(inheritanceSchema, true);
      Domain domain = null;
      try {
        if (asyncBuild) {
          Assert.DoesNotThrowAsync(async () => domain = await BuildDomainAsync(DomainUpgradeMode.Perform, upgradeTypes));
        }
        else {
          Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.Perform, upgradeTypes));
        }
      }
      catch (SuccessException) {
        domain.DisposeSafely();
        throw;
      }

      TestDomainContentAndDispose(domain, false);
    }

    [Test]
    [TestCase(InheritanceSchema.ClassTable, false)]
    [TestCase(InheritanceSchema.ClassTable, true)]
    [TestCase(InheritanceSchema.ConcreteTable, false)]
    [TestCase(InheritanceSchema.ConcreteTable, true)]
    [TestCase(InheritanceSchema.SingleTable, false)]
    [TestCase(InheritanceSchema.SingleTable, true)]
    public void OneToOneStructurePerformWithHintsTest(InheritanceSchema inheritanceSchema, bool asyncBuild)
    {
      var initTypes = GetOneToOneStructureTypes(inheritanceSchema, false);
      BuildAndPopulateInitialDomain(initTypes);

      var upgradeTypes = GetOneToOneStructureTypes(inheritanceSchema, true, true);
      Domain domain = null;
      try {
        if (asyncBuild) {
          Assert.DoesNotThrowAsync(async () => domain = await BuildDomainAsync(DomainUpgradeMode.Perform, upgradeTypes));
        }
        else {
          Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.Perform, upgradeTypes));
        }
      }
      catch (SuccessException) {
        domain.DisposeSafely();
        throw;
      }

      TestDomainContentAndDispose(domain, true);
    }

    [Test]
    [TestCase(InheritanceSchema.ClassTable, false)]
    [TestCase(InheritanceSchema.ClassTable, true)]
    [TestCase(InheritanceSchema.ConcreteTable, false)]
    [TestCase(InheritanceSchema.ConcreteTable, true)]
    [TestCase(InheritanceSchema.SingleTable, false)]
    [TestCase(InheritanceSchema.SingleTable, true)]
    public void OneToOneStructureValidateTest(InheritanceSchema inheritanceSchema, bool asyncBuild)
    {
      var initTypes = GetOneToOneStructureTypes(inheritanceSchema, false);
      BuildAndPopulateInitialDomain(initTypes);

      var upgradeTypes = GetOneToOneStructureTypes(inheritanceSchema, true);
      var ex = (asyncBuild)
        ? Assert.ThrowsAsync<SchemaSynchronizationException>(async () => { using (await BuildDomainAsync(DomainUpgradeMode.Validate, upgradeTypes)) {}; })
        : Assert.Throws<SchemaSynchronizationException>(() => { using (BuildDomain(DomainUpgradeMode.Validate, upgradeTypes)) {}; });
      Assert.That(ex.ComparisonResult, Is.Null);
      Assert.That(ex.Message, Is.EqualTo("Extracted and target schemas are equal but there are changes in type identifiers set."));
    }

    [Test]
    [TestCase(InheritanceSchema.ClassTable, false)]
    [TestCase(InheritanceSchema.ClassTable, true)]
    [TestCase(InheritanceSchema.ConcreteTable, false)]
    [TestCase(InheritanceSchema.ConcreteTable, true)]
    [TestCase(InheritanceSchema.SingleTable, false)]
    [TestCase(InheritanceSchema.SingleTable, true)]
    public void NewColumnWithinTablePerformSafelyTest(InheritanceSchema inheritanceSchema, bool asyncBuild)
    {
      var initTypes = GetNewColumnWithinTableTypes(inheritanceSchema, false);
      BuildAndPopulateInitialDomain(initTypes);

      var upgradeTypes = GetNewColumnWithinTableTypes(inheritanceSchema, true);
      var ex = (asyncBuild)
        ? Assert.ThrowsAsync<SchemaSynchronizationException>(async () => { using (await BuildDomainAsync(DomainUpgradeMode.PerformSafely, upgradeTypes)) {}; })
        : Assert.Throws<SchemaSynchronizationException>(() => { using (BuildDomain(DomainUpgradeMode.PerformSafely, upgradeTypes)) {}; });
      Assert.That(ex.ComparisonResult, Is.Not.Null);
      var allActions = ex.ComparisonResult.UpgradeActions.Flatten().ToList();
      Assert.That(allActions.Count(), Is.EqualTo(5));
      Assert.That(ex.ComparisonResult.HasUnsafeActions, Is.True);
      Assert.That(ex.ComparisonResult.UnsafeActions.Count, Is.EqualTo(1));

      var dataAction = ex.ComparisonResult.UnsafeActions[0] as DataAction;
      Assert.That(dataAction, Is.Not.Null);
      Assert.That(dataAction.Path, Is.EqualTo($"Tables/NamedValue{inheritanceSchema.ToString()}"));
      Assert.That(((NodeDifference) dataAction.Difference).MovementInfo.HasFlag(MovementInfo.Removed | MovementInfo.Created), Is.True);
    }

    [Test]
    [TestCase(InheritanceSchema.ClassTable, false)]
    [TestCase(InheritanceSchema.ClassTable, true)]
    [TestCase(InheritanceSchema.ConcreteTable, false)]
    [TestCase(InheritanceSchema.ConcreteTable, true)]
    [TestCase(InheritanceSchema.SingleTable, false)]
    [TestCase(InheritanceSchema.SingleTable, true)]
    public void NewColumnWithinTablePerformTest(InheritanceSchema inheritanceSchema, bool asyncBuild)
    {
      var initTypes = GetNewColumnWithinTableTypes(inheritanceSchema, false);
      BuildAndPopulateInitialDomain(initTypes);

      var upgradeTypes = GetNewColumnWithinTableTypes(inheritanceSchema, true);
      Domain domain = null;
      try {
        if (asyncBuild) {
          Assert.DoesNotThrowAsync(async () => domain = await BuildDomainAsync(DomainUpgradeMode.Perform, upgradeTypes));
        }
        else {
          Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.Perform, upgradeTypes));
        }
      }
      catch (SuccessException) {
        domain.DisposeSafely();
        throw;
      }

      TestDomainContentAndDispose(domain, false);
    }

    [Test]
    [TestCase(InheritanceSchema.ClassTable, false)]
    [TestCase(InheritanceSchema.ClassTable, true)]
    [TestCase(InheritanceSchema.ConcreteTable, false)]
    [TestCase(InheritanceSchema.ConcreteTable, true)]
    [TestCase(InheritanceSchema.SingleTable, false)]
    [TestCase(InheritanceSchema.SingleTable, true)]
    public void RemoveColumnWithinTablePerformSafelyTest(InheritanceSchema inheritanceSchema, bool asyncBuild)
    {
      var initTypes = GetRemoveColumnWithinTableTypes(inheritanceSchema, false);
      BuildAndPopulateInitialDomain(initTypes);

      var upgradeTypes = GetRemoveColumnWithinTableTypes(inheritanceSchema, true);
      var ex = (asyncBuild)
        ? Assert.ThrowsAsync<SchemaSynchronizationException>(async () => { using (await BuildDomainAsync(DomainUpgradeMode.PerformSafely, upgradeTypes)) {}; })
        : Assert.Throws<SchemaSynchronizationException>(() => { using (BuildDomain(DomainUpgradeMode.PerformSafely, upgradeTypes)) {}; });
      Assert.That(ex.ComparisonResult, Is.Not.Null);
      var allActions = ex.ComparisonResult.UpgradeActions.Flatten().ToList();
      Assert.That(allActions.Count(), Is.EqualTo(2));
      Assert.That(ex.ComparisonResult.HasUnsafeActions, Is.True);
      Assert.That(ex.ComparisonResult.UnsafeActions.Count, Is.EqualTo(2));
      Assert.That(ex.ComparisonResult.UnsafeActions.OfType<DataAction>().Count(), Is.EqualTo(1));

      var dataAction = ex.ComparisonResult.UnsafeActions.OfType<DataAction>().First();
      Assert.That(dataAction, Is.Not.Null);
      Assert.That(dataAction.Path, Is.EqualTo($"Tables/NamedValue{inheritanceSchema.ToString()}"));
      Assert.That(((NodeDifference) dataAction.Difference).MovementInfo.HasFlag(MovementInfo.Removed | MovementInfo.Created), Is.True);
    }

    [Test]
    [TestCase(InheritanceSchema.ClassTable, false)]
    [TestCase(InheritanceSchema.ClassTable, true)]
    [TestCase(InheritanceSchema.ConcreteTable, false)]
    [TestCase(InheritanceSchema.ConcreteTable, true)]
    [TestCase(InheritanceSchema.SingleTable, false)]
    [TestCase(InheritanceSchema.SingleTable, true)]
    public void RemoveColumnWithinTablePerformTest(InheritanceSchema inheritanceSchema, bool asyncBuild)
    {
      var initTypes = GetRemoveColumnWithinTableTypes(inheritanceSchema, false);
      BuildAndPopulateInitialDomain(initTypes);

      var upgradeTypes = GetRemoveColumnWithinTableTypes(inheritanceSchema, true);
      Domain domain = null;
      try {
        if (asyncBuild) {
          Assert.DoesNotThrowAsync(async () => domain = await BuildDomainAsync(DomainUpgradeMode.Perform, upgradeTypes));
        }
        else {
          Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.Perform, upgradeTypes));
        }
      }
      catch (SuccessException) {
        domain.DisposeSafely();
        throw;
      }

      TestDomainContentAndDispose(domain, false);
    }

    [Test]
    [TestCase(InheritanceSchema.ClassTable, false)]
    [TestCase(InheritanceSchema.ClassTable, true)]
    [TestCase(InheritanceSchema.ConcreteTable, false)]
    [TestCase(InheritanceSchema.ConcreteTable, true)]
    [TestCase(InheritanceSchema.SingleTable, false)]
    [TestCase(InheritanceSchema.SingleTable, true)]
    public void NewTablePerformSafelyTest(InheritanceSchema inheritanceSchema, bool asyncBuild)
    {
      var initTypes = GetNewTableTypes(inheritanceSchema, false);
      BuildAndPopulateInitialDomain(initTypes);

      var upgradeTypes = GetNewTableTypes(inheritanceSchema, true);
      var ex = (asyncBuild)
        ? Assert.ThrowsAsync<SchemaSynchronizationException>(async () => { using (await BuildDomainAsync(DomainUpgradeMode.PerformSafely, upgradeTypes)) {}; })
        : Assert.Throws<SchemaSynchronizationException>(() => { using (BuildDomain(DomainUpgradeMode.PerformSafely, upgradeTypes)) {}; });
      Assert.That(ex.ComparisonResult, Is.Not.Null);
      var allActions = ex.ComparisonResult.UpgradeActions.Flatten().ToList();
      var expectedActionsCount = StorageProviderInfo.Instance.CheckProviderIsNot(StorageProvider.SqlServer)
        ? 15
        : 17;
      Assert.That(allActions.Count(), Is.EqualTo(expectedActionsCount));
      Assert.That(ex.ComparisonResult.HasUnsafeActions, Is.True);
      Assert.That(ex.ComparisonResult.UnsafeActions.Count, Is.EqualTo(1));
      Assert.That(ex.ComparisonResult.UnsafeActions.OfType<DataAction>().Count(), Is.EqualTo(1));

      var dataAction = ex.ComparisonResult.UnsafeActions.OfType<DataAction>().First();
      Assert.That(dataAction, Is.Not.Null);
      Assert.That(dataAction.Path, Is.EqualTo($"Tables/NamedValue{inheritanceSchema.ToString()}"));
      Assert.That(((NodeDifference) dataAction.Difference).MovementInfo.HasFlag(MovementInfo.Removed | MovementInfo.Created), Is.True);
    }

    [Test]
    [TestCase(InheritanceSchema.ClassTable, false)]
    [TestCase(InheritanceSchema.ClassTable, true)]
    [TestCase(InheritanceSchema.ConcreteTable, false)]
    [TestCase(InheritanceSchema.ConcreteTable, true)]
    [TestCase(InheritanceSchema.SingleTable, false)]
    [TestCase(InheritanceSchema.SingleTable, true)]
    public void NewTablePerformTest(InheritanceSchema inheritanceSchema, bool asyncBuild)
    {
      var initTypes = GetNewTableTypes(inheritanceSchema, false);
      BuildAndPopulateInitialDomain(initTypes);

      var upgradeTypes = GetNewTableTypes(inheritanceSchema, true);
      Domain domain = null;
      try {
        if (asyncBuild) {
          Assert.DoesNotThrowAsync(async () => domain = await BuildDomainAsync(DomainUpgradeMode.Perform, upgradeTypes));
        }
        else {
          Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.Perform, upgradeTypes));
        }
      }
      catch (SuccessException) {
        domain.DisposeSafely();
        throw;
      }

      TestDomainContentAndDispose(domain, false);
    }

    [Test]
    [TestCase(InheritanceSchema.ClassTable, false)]
    [TestCase(InheritanceSchema.ClassTable, true)]
    [TestCase(InheritanceSchema.ConcreteTable, false)]
    [TestCase(InheritanceSchema.ConcreteTable, true)]
    [TestCase(InheritanceSchema.SingleTable, false)]
    [TestCase(InheritanceSchema.SingleTable, true)]
    public void RemoveTablePerformSafelyTest(InheritanceSchema inheritanceSchema, bool asyncBuild)
    {
      var initTypes = GetRemoveTableTypes(inheritanceSchema, false);
      BuildAndPopulateInitialDomain(initTypes);

      var upgradeTypes = GetRemoveTableTypes(inheritanceSchema, true);
      var ex = (asyncBuild)
        ? Assert.ThrowsAsync<SchemaSynchronizationException>(async () => { using (await BuildDomainAsync(DomainUpgradeMode.PerformSafely, upgradeTypes)) {}; })
        : Assert.Throws<SchemaSynchronizationException>(() => { using (BuildDomain(DomainUpgradeMode.PerformSafely, upgradeTypes)) {}; });
      Assert.That(ex.ComparisonResult, Is.Not.Null);
      var allActions = ex.ComparisonResult.UpgradeActions.Flatten().ToList();
      Assert.That(allActions.Count(), Is.EqualTo(5)); // (2 data deletions + 2 types deletion) + table reuse data deletion
      Assert.That(ex.ComparisonResult.HasUnsafeActions, Is.True);
      Assert.That(ex.ComparisonResult.UnsafeActions.Count, Is.EqualTo(3));
      Assert.That(ex.ComparisonResult.UnsafeActions.OfType<DataAction>().Count(), Is.EqualTo(1));

      var dataAction = ex.ComparisonResult.UnsafeActions.OfType<DataAction>().First();
      Assert.That(dataAction, Is.Not.Null);
      Assert.That(dataAction.Path, Is.EqualTo($"Tables/NamedValue{inheritanceSchema.ToString()}"));
      Assert.That(((NodeDifference) dataAction.Difference).MovementInfo.HasFlag(MovementInfo.Removed | MovementInfo.Created), Is.True);
    }

    [Test]
    [TestCase(InheritanceSchema.ClassTable, false)]
    [TestCase(InheritanceSchema.ClassTable, true)]
    [TestCase(InheritanceSchema.ConcreteTable, false)]
    [TestCase(InheritanceSchema.ConcreteTable, true)]
    [TestCase(InheritanceSchema.SingleTable, false)]
    [TestCase(InheritanceSchema.SingleTable, true)]
    public void RemoveTablePerformTest(InheritanceSchema inheritanceSchema, bool asyncBuild)
    {
      var initTypes = GetRemoveTableTypes(inheritanceSchema, false);
      BuildAndPopulateInitialDomain(initTypes);

      var upgradeTypes = GetRemoveTableTypes(inheritanceSchema, true);
      Domain domain = null;
      try {
        if (asyncBuild) {
          Assert.DoesNotThrowAsync(async () => domain = await BuildDomainAsync(DomainUpgradeMode.Perform, upgradeTypes));
        }
        else {
          Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.Perform, upgradeTypes));
        }
      }
      catch (SuccessException) {
        domain.DisposeSafely();
        throw;
      }

      TestDomainContentAndDispose(domain, false);
    }

    //[Test]
    //public void ConnectorTypeTest()
    //{
    //  var types = new Type[] { typeof(ConnectorType.Before.Author), typeof(ConnectorType.Before.Book) };
    //  using (var domain = BuildDomain(DomainUpgradeMode.Recreate, types)) {
    //    using (var session = domain.OpenSession())
    //    using (var tx = session.OpenTransaction()) {
    //      new ConnectorType.Before.Book();
    //      new ConnectorType.Before.Book();
    //      new ConnectorType.Before.Book();
    //      new ConnectorType.Before.Book();
    //      new ConnectorType.Before.Book();
    //      tx.Complete();
    //    }
    //  }


    //  types = new Type[] { typeof(ConnectorType.After.Author), typeof(ConnectorType.After.Book), typeof(ConnectorType.After.CustomUpgradeHandler) };
    //  using (BuildDomain(DomainUpgradeMode.PerformSafely, types)) {

    //  }
    //}


    private void BuildAndPopulateInitialDomain(Type[] types)
    {
      using (var initialDomain = BuildDomain(DomainUpgradeMode.Recreate, types))
      using (var session = initialDomain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var theRoot = initialDomain.Model.Hierarchies.Single(h => h.Root.Name.Contains("NamedValue")).Root;
        _ = CreateEntity(theRoot, session, "name");
        t.Complete();
      }
    }

    private void TestDomainContentAndDispose(Domain domain, bool expectedResult)
    {
      using (domain)
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var theRoot = domain.Model.Hierarchies.Single(h => h.Root.Name.Contains("NamedValue")).Root;
        var result = CheckExistance(theRoot, session);
        Assert.That(result, Is.EqualTo(expectedResult));
      }
    }

    private Type[] GetOneToOneStructureTypes(InheritanceSchema inheritanceSchema, bool isAfter, bool? withHints = null)
    {
      switch (inheritanceSchema) {
        case InheritanceSchema.ClassTable: {
          if (isAfter) {
            if (withHints.HasValue && withHints.Value) {
              return new[] {
                typeof(OneToOneStructure.After.NamedValueClassTable),
                typeof(OneToOneStructure.After.MasterNamedValueClassTable),
                typeof(OneToOneStructure.After.ClassTableUpgradeHandler)
              };
            }
            else {
              return new[] {
                typeof(OneToOneStructure.After.NamedValueClassTable),
                typeof(OneToOneStructure.After.MasterNamedValueClassTable),
              };
            }
          }
          else {
            return new[] {
              typeof(OneToOneStructure.Before.NamedValueClassTable)
            };
          }
        }
        case InheritanceSchema.ConcreteTable: {
          if (isAfter) {
            if (withHints.HasValue && withHints.Value) {
              return new[] {
                typeof(OneToOneStructure.After.NamedValueConcreteTable),
                typeof(OneToOneStructure.After.MasterNamedValueConcreteTable),
                typeof(OneToOneStructure.After.ConcreteTableUpgradeHandler)
              };
            }
            else {
              return new[] {
                typeof(OneToOneStructure.After.NamedValueConcreteTable),
                typeof(OneToOneStructure.After.MasterNamedValueConcreteTable),
              };
            }
          }
          else {
            return new[] {
              typeof(OneToOneStructure.Before.NamedValueConcreteTable)
            };
          }
        }
        case InheritanceSchema.SingleTable: {
          if (isAfter) {
            if (withHints.HasValue && withHints.Value) {
              return new[] {
                typeof(OneToOneStructure.After.NamedValueSingleTable),
                typeof(OneToOneStructure.After.MasterNamedValueSingleTable),
                typeof(OneToOneStructure.After.SingleTableUpgradeHandler)
              };
            }
            else {
              return new[] {
                typeof(OneToOneStructure.After.NamedValueSingleTable),
                typeof(OneToOneStructure.After.MasterNamedValueSingleTable),
              };
            }
          }
          else {
            return new[] {
              typeof(OneToOneStructure.Before.NamedValueSingleTable)
            };
          }
        }
        default:
          throw new ArgumentOutOfRangeException(nameof(inheritanceSchema));
      }
    }

    private Type[] GetNewColumnWithinTableTypes(InheritanceSchema inheritanceSchema, bool isAfter)
    {
      switch (inheritanceSchema) {
        case InheritanceSchema.ClassTable: {
          if (isAfter) {
            return new[] {
              typeof (NewColumnWithinTable.After.NamedValueClassTable),
              typeof (NewColumnWithinTable.After.MasterNamedValueClassTable),
            };
          }
          else {
            return new[] {
              typeof(NewColumnWithinTable.Before.NamedValueClassTable)
            };
          }
        }
        case InheritanceSchema.ConcreteTable: {
          if (isAfter) {
            return new[] {
              typeof(NewColumnWithinTable.After.NamedValueConcreteTable),
              typeof(NewColumnWithinTable.After.MasterNamedValueConcreteTable),
            };
          }
          else {
            return new[] {
              typeof(NewColumnWithinTable.Before.NamedValueConcreteTable)
            };
          }
        }
        case InheritanceSchema.SingleTable: {
          if (isAfter) {
            return new[] {
              typeof(NewColumnWithinTable.After.NamedValueSingleTable),
              typeof(NewColumnWithinTable.After.MasterNamedValueSingleTable),
            };
          }
          else {
            return new[] {
              typeof(NewColumnWithinTable.Before.NamedValueSingleTable)
            };
          }
        }
        default:
          throw new ArgumentOutOfRangeException(nameof(inheritanceSchema));
      }
    }

    private Type[] GetRemoveColumnWithinTableTypes(InheritanceSchema inheritanceSchema, bool isAfter)
    {
      switch (inheritanceSchema) {
        case InheritanceSchema.ClassTable: {
          if (isAfter) {
            return new[] {
              typeof(RemoveColumnWithinTable.After.NamedValueClassTable),
              typeof(RemoveColumnWithinTable.After.MasterNamedValueClassTable),
            };
          }
          else {
            return new[] {
              typeof(RemoveColumnWithinTable.Before.NamedValueClassTable)
            };
          }
        }
        case InheritanceSchema.ConcreteTable: {
          if (isAfter) {
            return new[] {
              typeof(RemoveColumnWithinTable.After.NamedValueConcreteTable),
              typeof(RemoveColumnWithinTable.After.MasterNamedValueConcreteTable),
            };
          }
          else {
            return new[] {
              typeof(RemoveColumnWithinTable.Before.NamedValueConcreteTable)
            };
          }
        }
        case InheritanceSchema.SingleTable: {
          if (isAfter) {
            return new[] {
              typeof(RemoveColumnWithinTable.After.NamedValueSingleTable),
              typeof(RemoveColumnWithinTable.After.MasterNamedValueSingleTable),
            };
          }
          else {
            return new[] {
              typeof(RemoveColumnWithinTable.Before.NamedValueSingleTable)
            };
          }
        }
        default:
          throw new ArgumentOutOfRangeException(nameof(inheritanceSchema));
      }
    }

    private Type[] GetNewTableTypes(InheritanceSchema inheritanceSchema, bool isAfter)
    {
      switch (inheritanceSchema) {
        case InheritanceSchema.ClassTable: {
          if (isAfter) {
            return new[] {
              typeof(NewTable.After.AFirst),
              typeof(NewTable.After.NamedValueClassTable),
              typeof(NewTable.After.MasterNamedValueClassTable),
              typeof(NewTable.After.VeryLast)
            };
          }
          else {
            return new[] {
              typeof(NewTable.Before.NamedValueClassTable)
            };
          }
        }
        case InheritanceSchema.ConcreteTable: {
          if (isAfter) {
            return new[] {
              typeof(NewTable.After.AFirst),
              typeof(NewTable.After.NamedValueConcreteTable),
              typeof(NewTable.After.MasterNamedValueConcreteTable),
              typeof(NewTable.After.VeryLast)
            };
          }
          else {
            return new[] {
              typeof(NewTable.Before.NamedValueConcreteTable)
            };
          }
        }
        case InheritanceSchema.SingleTable: {
          if (isAfter) {
            return new[] {
              typeof(NewTable.After.AFirst),
              typeof(NewTable.After.NamedValueSingleTable),
              typeof(NewTable.After.MasterNamedValueSingleTable),
              typeof(NewTable.After.VeryLast)
            };
          }
          else {
            return new[] {
              typeof(NewTable.Before.NamedValueSingleTable)
            };
          }
        }
        default:
          throw new ArgumentOutOfRangeException(nameof(inheritanceSchema));
      }
    }

    private Type[] GetRemoveTableTypes(InheritanceSchema inheritanceSchema, bool isAfter)
    {
      switch (inheritanceSchema) {
        case InheritanceSchema.ClassTable: {
          if (isAfter) {
            return new[] {
              typeof (RemoveTable.After.NamedValueClassTable),
              typeof (RemoveTable.After.MasterNamedValueClassTable),
            };
          }
          else {
            return new[] {
              typeof(RemoveTable.Before.AFirst),
              typeof(RemoveTable.Before.NamedValueClassTable),
              typeof(RemoveTable.Before.VeryLast),
            };
          }
        }
        case InheritanceSchema.ConcreteTable: {
          if (isAfter) {
            return new[] {
              typeof(RemoveTable.After.NamedValueConcreteTable),
              typeof(RemoveTable.After.MasterNamedValueConcreteTable),
            };
          }
          else {
            return new[] {
              typeof(RemoveTable.Before.AFirst),
              typeof(RemoveTable.Before.NamedValueConcreteTable),
              typeof(RemoveTable.Before.VeryLast),
            };
          }
        }
        case InheritanceSchema.SingleTable: {
          if (isAfter) {
            return new[] {
              typeof(RemoveTable.After.NamedValueSingleTable),
              typeof(RemoveTable.After.MasterNamedValueSingleTable),
            };
          }
          else {
            return new[] {
              typeof(RemoveTable.Before.AFirst),
              typeof(RemoveTable.Before.NamedValueSingleTable),
              typeof(RemoveTable.Before.VeryLast),
            };
          }
        }
        default:
          throw new ArgumentOutOfRangeException(nameof(inheritanceSchema));
      }
    }

    private Entity CreateEntity(TypeInfo rootType, Session session, string keyValue)
    {
      var ctor = rootType.UnderlyingType.GetConstructor(new[] { typeof(Session), typeof(string), typeof(string) });
      var entity = (Entity) ctor.Invoke(new object[] { session, keyValue, (string) null });
      return entity;
    }

    private bool CheckExistance(TypeInfo rootType, Session session)
    {
      var checkMethod = rootType.UnderlyingType.GetMethod("CheckAnyExists", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
      return (bool)checkMethod.Invoke(null, new[] { session });
    }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.UpgradeToNewHierarchyThatInheritsStructureTestModel
{
  namespace OneToOneStructure
  {
    namespace Before
    {
      [HierarchyRoot(InheritanceSchema.ClassTable)]
      [KeyGenerator(KeyGeneratorKind.None)]
      public class NamedValueClassTable : Entity
      {
        [Key, Field(Length = 12, Nullable = false)]
        public string Name { get; private set; }

        [Field(Length = int.MaxValue)]
        public string Value { get; set; }

        public NamedValueClassTable(Session session, string name, string value = null)
          : base(session, name)
        {
          Value = value;
        }
      }

      [HierarchyRoot(InheritanceSchema.ConcreteTable)]
      [KeyGenerator(KeyGeneratorKind.None)]
      public class NamedValueConcreteTable : Entity
      {
        [Key, Field(Length = 12, Nullable = false)]
        public string Name { get; private set; }

        [Field(Length = int.MaxValue)]
        public string Value { get; set; }

        public NamedValueConcreteTable(Session session, string name, string value = null)
          : base(session, name)
        {
          Value = value;
        }
      }

      [HierarchyRoot(InheritanceSchema.SingleTable)]
      [KeyGenerator(KeyGeneratorKind.None)]
      public class NamedValueSingleTable : Entity
      {
        [Key, Field(Length = 12, Nullable = false)]
        public string Name { get; private set; }

        [Field(Length = int.MaxValue)]
        public string Value { get; set; }

        public NamedValueSingleTable(Session session, string name, string value = null)
          : base(session, name)
        {
          Value = value;
        }
      }
    }

    namespace After
    {
      // The table structure is the same but hierarchy root is different.
      // Since there is no hints that resolve this hierarchy to the old one this hierarchy is treated as a completely new hierarchy
      // so data in NamedValue table no longer fits the model.
      //In this case, DO have to delete data but deleting the data is not good in PerfromSafely mode (acceptable for the Perform mode though)
      [HierarchyRoot(InheritanceSchema.ClassTable)]
      [KeyGenerator(KeyGeneratorKind.None)]
      [TableMapping("NamedValueClassTable")]
      [Index("Name", Name = "PK_NamedValueClassTable", Clustered = true, Unique = true)]
      public class MasterNamedValueClassTable : NamedValueClassTable
      {
        public static bool CheckAnyExists(Session session)
          => session.Query.All<MasterNamedValueClassTable>().Any();

        public MasterNamedValueClassTable(Session session, string name, string value = null)
          : base(session, name, value)
        {
        }
      }

      public abstract class NamedValueClassTable : Entity
      {
        [Key, Field(Length = 12, Nullable = false)]
        public string Name { get; private set; }

        [Field(Length = int.MaxValue)]
        public string Value { get; set; }

        public NamedValueClassTable(Session session, string name, string value = null)
          : base(session, name)
        {
          Value = value;
        }
      }

      [HierarchyRoot(InheritanceSchema.ConcreteTable)]
      [KeyGenerator(KeyGeneratorKind.None)]
      [TableMapping("NamedValueConcreteTable")]
      [Index("Name", Name = "PK_NamedValueConcreteTable", Clustered = true, Unique = true)]
      public class MasterNamedValueConcreteTable : NamedValueConcreteTable
      {
        public static bool CheckAnyExists(Session session)
          => session.Query.All<MasterNamedValueConcreteTable>().Any();

        public MasterNamedValueConcreteTable(Session session, string name, string value = null)
          : base(session, name, value)
        {
        }
      }

      public abstract class NamedValueConcreteTable : Entity
      {
        [Key, Field(Length = 12, Nullable = false)]
        public string Name { get; private set; }

        [Field(Length = int.MaxValue)]
        public string Value { get; set; }

        public NamedValueConcreteTable(Session session, string name, string value = null)
          : base(session, name)
        {
          Value = value;
        }
      }

      [HierarchyRoot(InheritanceSchema.SingleTable)]
      [KeyGenerator(KeyGeneratorKind.None)]
      [TableMapping("NamedValueSingleTable")]
      [Index("Name", Name = "PK_NamedValueSingleTable", Clustered = true, Unique = true)]
      public class MasterNamedValueSingleTable : NamedValueSingleTable
      {
        public static bool CheckAnyExists(Session session)
          => session.Query.All<MasterNamedValueSingleTable>().Any();

        public MasterNamedValueSingleTable(Session session, string name, string value = null)
          : base(session, name, value)
        {
        }
      }

      public abstract class NamedValueSingleTable : Entity
      {
        [Key, Field(Length = 12, Nullable = false)]
        public string Name { get; private set; }

        [Field(Length = int.MaxValue)]
        public string Value { get; set; }

        public NamedValueSingleTable(Session session, string name, string value = null)
          : base(session, name)
        {
          Value = value;
        }
      }

      public class ClassTableUpgradeHandler : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion) => true;

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          base.AddUpgradeHints(hints);
          var currentType = typeof(MasterNamedValueClassTable);

          _ = hints.Add(
            new RenameTypeHint(currentType.Namespace.Replace("After", "Before") + ".NamedValueClassTable", currentType));
        }
      }

      public class ConcreteTableUpgradeHandler : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion) => true;

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          base.AddUpgradeHints(hints);
          var currentType = typeof(MasterNamedValueConcreteTable);

          _ = hints.Add(
            new RenameTypeHint(currentType.Namespace.Replace("After", "Before") + ".NamedValueConcreteTable", currentType));
        }
      }

      public class SingleTableUpgradeHandler : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion) => true;

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          base.AddUpgradeHints(hints);
          var currentType = typeof(MasterNamedValueSingleTable);

          _ = hints.Add(
            new RenameTypeHint(currentType.Namespace.Replace("After", "Before") + ".NamedValueSingleTable", currentType));
        }
      }
    }
  }

  namespace NewColumnWithinTable
  {
    namespace Before
    {
      [HierarchyRoot(InheritanceSchema.ClassTable)]
      [KeyGenerator(KeyGeneratorKind.None)]
      public class NamedValueClassTable : Entity
      {
        [Key, Field(Length = 12, Nullable = false)]
        public string Name { get; private set; }

        [Field(Length = int.MaxValue)]
        public string Value { get; set; }

        public NamedValueClassTable(Session session, string name, string value = null)
          : base(session, name)
        {
          Value = value;
        }
      }

      [HierarchyRoot(InheritanceSchema.ConcreteTable)]
      [KeyGenerator(KeyGeneratorKind.None)]
      public class NamedValueConcreteTable : Entity
      {
        [Key, Field(Length = 12, Nullable = false)]
        public string Name { get; private set; }

        [Field(Length = int.MaxValue)]
        public string Value { get; set; }

        public NamedValueConcreteTable(Session session, string name, string value = null)
          : base(session, name)
        {
          Value = value;
        }
      }

      [HierarchyRoot(InheritanceSchema.SingleTable)]
      [KeyGenerator(KeyGeneratorKind.None)]
      public class NamedValueSingleTable : Entity
      {
        [Key, Field(Length = 12, Nullable = false)]
        public string Name { get; private set; }

        [Field(Length = int.MaxValue)]
        public string Value { get; set; }

        public NamedValueSingleTable(Session session, string name, string value = null)
          : base(session, name)
        {
          Value = value;
        }
      }
    }

    namespace After
    {
      [HierarchyRoot(InheritanceSchema.ClassTable)]
      [KeyGenerator(KeyGeneratorKind.None)]
      [TableMapping("NamedValueClassTable")]
      [Index("Name", Name = "PK_NamedValueClassTable", Clustered = true, Unique = true)]
      public class MasterNamedValueClassTable : NamedValueClassTable
      {
        [Field(Length = 12)]
        public string NewDeclaredField { get; set; }

        public static bool CheckAnyExists(Session session)
          => session.Query.All<MasterNamedValueClassTable>().Any();

        public MasterNamedValueClassTable(Session session, string name, string value = null)
          : base(session, name, value)
        {
        }
      }

      public abstract class NamedValueClassTable : Entity
      {
        [Key, Field(Length = 12, Nullable = false)]
        public string Name { get; private set; }

        [Field(Length = int.MaxValue)]
        public string Value { get; set; }

        [Field(Length = 12)]
        public string NewInheritedField { get; set; }

        public NamedValueClassTable(Session session, string name, string value = null)
          : base(session, name)
        {
          Value = value;
        }
      }

      [HierarchyRoot(InheritanceSchema.ConcreteTable)]
      [KeyGenerator(KeyGeneratorKind.None)]
      [TableMapping("NamedValueConcreteTable")]
      [Index("Name", Name = "PK_NamedValueConcreteTable", Clustered = true, Unique = true)]
      public class MasterNamedValueConcreteTable : NamedValueConcreteTable
      {
        [Field(Length = 12)]
        public string NewDeclaredField { get; set; }

        public static bool CheckAnyExists(Session session)
          => session.Query.All<MasterNamedValueConcreteTable>().Any();

        public MasterNamedValueConcreteTable(Session session, string name, string value = null)
          : base(session, name, value)
        {
        }
      }

      public abstract class NamedValueConcreteTable : Entity
      {
        [Key, Field(Length = 12, Nullable = false)]
        public string Name { get; private set; }

        [Field(Length = int.MaxValue)]
        public string Value { get; set; }

        [Field(Length = 12)]
        public string NewInheritedField { get; set; }

        public NamedValueConcreteTable(Session session, string name, string value = null)
          : base(session, name)
        {
          Value = value;
        }
      }

      [HierarchyRoot(InheritanceSchema.SingleTable)]
      [KeyGenerator(KeyGeneratorKind.None)]
      [TableMapping("NamedValueSingleTable")]
      [Index("Name", Name = "PK_NamedValueSingleTable", Clustered = true, Unique = true)]
      public class MasterNamedValueSingleTable : NamedValueSingleTable
      {
        [Field(Length = 12)]
        public string NewDeclaredField { get; set; }

        public static bool CheckAnyExists(Session session)
          => session.Query.All<MasterNamedValueSingleTable>().Any();

        public MasterNamedValueSingleTable(Session session, string name, string value = null)
          : base(session, name, value)
        {
        }
      }

      public abstract class NamedValueSingleTable : Entity
      {
        [Key, Field(Length = 12, Nullable = false)]
        public string Name { get; private set; }

        [Field(Length = int.MaxValue)]
        public string Value { get; set; }

        [Field(Length = 12)]
        public string NewInheritedField { get; set; }

        public NamedValueSingleTable(Session session, string name, string value = null)
          : base(session, name)
        {
          Value = value;
        }
      }
    }
  }

  namespace RemoveColumnWithinTable
  {
    namespace Before
    {
      [HierarchyRoot(InheritanceSchema.ClassTable)]
      [KeyGenerator(KeyGeneratorKind.None)]
      public class NamedValueClassTable : Entity
      {
        [Key, Field(Length = 12, Nullable = false)]
        public string Name { get; private set; }

        [Field(Length = int.MaxValue)]
        public string Value { get; set; }

        [Field(Length = 12)]
        public string FieldThatNoLongerExists { get; set; }

        public NamedValueClassTable(Session session, string name, string value = null)
          : base(session, name)
        {
          Value = value;
        }
      }

      [HierarchyRoot(InheritanceSchema.ConcreteTable)]
      [KeyGenerator(KeyGeneratorKind.None)]
      public class NamedValueConcreteTable : Entity
      {
        [Key, Field(Length = 12, Nullable = false)]
        public string Name { get; private set; }

        [Field(Length = int.MaxValue)]
        public string Value { get; set; }

        [Field(Length = 12)]
        public string FieldThatNoLongerExists { get; set; }

        public NamedValueConcreteTable(Session session, string name, string value = null)
          : base(session, name)
        {
          Value = value;
        }
      }

      [HierarchyRoot(InheritanceSchema.SingleTable)]
      [KeyGenerator(KeyGeneratorKind.None)]
      public class NamedValueSingleTable : Entity
      {
        [Key, Field(Length = 12, Nullable = false)]
        public string Name { get; private set; }

        [Field(Length = int.MaxValue)]
        public string Value { get; set; }

        [Field(Length = 12)]
        public string FieldThatNoLongerExists { get; set; }

        public NamedValueSingleTable(Session session, string name, string value = null)
          : base(session, name)
        {
          Value = value;
        }
      }
    }

    namespace After
    {
      [HierarchyRoot(InheritanceSchema.ClassTable)]
      [KeyGenerator(KeyGeneratorKind.None)]
      [TableMapping("NamedValueClassTable")]
      [Index("Name", Name = "PK_NamedValueClassTable", Clustered = true, Unique = true)]
      public class MasterNamedValueClassTable : NamedValueClassTable
      {
        public static bool CheckAnyExists(Session session)
          => session.Query.All<MasterNamedValueClassTable>().Any();

        public MasterNamedValueClassTable(Session session, string name, string value = null)
          : base(session, name, value)
        {
        }
      }

      public abstract class NamedValueClassTable : Entity
      {
        [Key, Field(Length = 12, Nullable = false)]
        public string Name { get; private set; }

        [Field(Length = int.MaxValue)]
        public string Value { get; set; }

        public NamedValueClassTable(Session session, string name, string value = null)
          : base(session, name)
        {
          Value = value;
        }
      }

      [HierarchyRoot(InheritanceSchema.ConcreteTable)]
      [KeyGenerator(KeyGeneratorKind.None)]
      [TableMapping("NamedValueConcreteTable")]
      [Index("Name", Name = "PK_NamedValueConcreteTable", Clustered = true, Unique = true)]
      public class MasterNamedValueConcreteTable : NamedValueConcreteTable
      {
        public static bool CheckAnyExists(Session session)
          => session.Query.All<MasterNamedValueConcreteTable>().Any();

        public MasterNamedValueConcreteTable(Session session, string name, string value = null)
          : base(session, name, value)
        {
        }
      }

      public abstract class NamedValueConcreteTable : Entity
      {
        [Key, Field(Length = 12, Nullable = false)]
        public string Name { get; private set; }

        [Field(Length = int.MaxValue)]
        public string Value { get; set; }

        public NamedValueConcreteTable(Session session, string name, string value = null)
          : base(session, name)
        {
          Value = value;
        }
      }

      [HierarchyRoot(InheritanceSchema.SingleTable)]
      [KeyGenerator(KeyGeneratorKind.None)]
      [TableMapping("NamedValueSingleTable")]
      [Index("Name", Name = "PK_NamedValueSingleTable", Clustered = true, Unique = true)]
      public class MasterNamedValueSingleTable : NamedValueSingleTable
      {
        public static bool CheckAnyExists(Session session)
          => session.Query.All<MasterNamedValueSingleTable>().Any();

        public MasterNamedValueSingleTable(Session session, string name, string value = null)
          : base(session, name, value)
        {
        }
      }

      public abstract class NamedValueSingleTable : Entity
      {
        [Key, Field(Length = 12, Nullable = false)]
        public string Name { get; private set; }

        [Field(Length = int.MaxValue)]
        public string Value { get; set; }

        public NamedValueSingleTable(Session session, string name, string value = null)
          : base(session, name)
        {
          Value = value;
        }
      }
    }
  }

  namespace NewTable
  {
    namespace Before
    {
      [HierarchyRoot(InheritanceSchema.ClassTable)]
      [KeyGenerator(KeyGeneratorKind.None)]
      public class NamedValueClassTable : Entity
      {
        [Key, Field(Length = 12, Nullable = false)]
        public string Name { get; private set; }

        [Field(Length = int.MaxValue)]
        public string Value { get; set; }

        public NamedValueClassTable(Session session, string name, string value = null)
          : base(session, name)
        {
          Value = value;
        }
      }

      [HierarchyRoot(InheritanceSchema.ConcreteTable)]
      [KeyGenerator(KeyGeneratorKind.None)]
      public class NamedValueConcreteTable : Entity
      {
        [Key, Field(Length = 12, Nullable = false)]
        public string Name { get; private set; }

        [Field(Length = int.MaxValue)]
        public string Value { get; set; }

        public NamedValueConcreteTable(Session session, string name, string value = null)
          : base(session, name)
        {
          Value = value;
        }
      }

      [HierarchyRoot(InheritanceSchema.SingleTable)]
      [KeyGenerator(KeyGeneratorKind.None)]
      public class NamedValueSingleTable : Entity
      {
        [Key, Field(Length = 12, Nullable = false)]
        public string Name { get; private set; }

        [Field(Length = int.MaxValue)]
        public string Value { get; set; }

        public NamedValueSingleTable(Session session, string name, string value = null)
          : base(session, name)
        {
          Value = value;
        }
      }
    }

    namespace After
    {
      [HierarchyRoot(InheritanceSchema.ClassTable)]
      [KeyGenerator(KeyGeneratorKind.None)]
      [TableMapping("NamedValueClassTable")]
      [Index("Name", Name = "PK_NamedValueClassTable", Clustered = true, Unique = true)]
      public class MasterNamedValueClassTable : NamedValueClassTable
      {
        public static bool CheckAnyExists(Session session)
          => session.Query.All<MasterNamedValueClassTable>().Any();

        public MasterNamedValueClassTable(Session session, string name, string value = null)
          : base(session, name, value)
        {
        }
      }

      public abstract class NamedValueClassTable : Entity
      {
        [Key, Field(Length = 12, Nullable = false)]
        public string Name { get; private set; }

        [Field(Length = int.MaxValue)]
        public string Value { get; set; }

        public NamedValueClassTable(Session session, string name, string value = null)
          : base(session, name)
        {
          Value = value;
        }
      }

      [HierarchyRoot(InheritanceSchema.ConcreteTable)]
      [KeyGenerator(KeyGeneratorKind.None)]
      [TableMapping("NamedValueConcreteTable")]
      [Index("Name", Name = "PK_NamedValueConcreteTable", Clustered = true, Unique = true)]
      public class MasterNamedValueConcreteTable : NamedValueConcreteTable
      {
        public static bool CheckAnyExists(Session session)
          => session.Query.All<MasterNamedValueConcreteTable>().Any();

        public MasterNamedValueConcreteTable(Session session, string name, string value = null)
          : base(session, name, value)
        {
        }
      }

      public abstract class NamedValueConcreteTable : Entity
      {
        [Key, Field(Length = 12, Nullable = false)]
        public string Name { get; private set; }

        [Field(Length = int.MaxValue)]
        public string Value { get; set; }

        public NamedValueConcreteTable(Session session, string name, string value = null)
          : base(session, name)
        {
          Value = value;
        }
      }

      [HierarchyRoot(InheritanceSchema.SingleTable)]
      [KeyGenerator(KeyGeneratorKind.None)]
      [TableMapping("NamedValueSingleTable")]
      [Index("Name", Name = "PK_NamedValueSingleTable", Clustered = true, Unique = true)]
      public class MasterNamedValueSingleTable : NamedValueSingleTable
      {
        public static bool CheckAnyExists(Session session)
          => session.Query.All<MasterNamedValueSingleTable>().Any();

        public MasterNamedValueSingleTable(Session session, string name, string value = null)
          : base(session, name, value)
        {
        }
      }

      public abstract class NamedValueSingleTable : Entity
      {
        [Key, Field(Length = 12, Nullable = false)]
        public string Name { get; private set; }

        [Field(Length = int.MaxValue)]
        public string Value { get; set; }

        public NamedValueSingleTable(Session session, string name, string value = null)
          : base(session, name)
        {
          Value = value;
        }
      }

      [HierarchyRoot]
      [KeyGenerator(KeyGeneratorKind.None)]
      public class AFirst : Entity
      {
        [Field, Key]
        public Guid Id { get; set; }

        public AFirst(Session session)
          : base(session, Guid.NewGuid())
        {
        }
      }

      [HierarchyRoot]
      [KeyGenerator(KeyGeneratorKind.None)]
      public class VeryLast : Entity
      {
        [Field, Key]
        public Guid Id { get; set; }

        public VeryLast(Session session)
          : base(session, Guid.NewGuid())
        {
        }
      }
    }
  }

  namespace RemoveTable
  {
    namespace Before
    {
      [HierarchyRoot(InheritanceSchema.ClassTable)]
      [KeyGenerator(KeyGeneratorKind.None)]
      public class NamedValueClassTable : Entity
      {
        [Key, Field(Length = 12, Nullable = false)]
        public string Name { get; private set; }

        [Field(Length = int.MaxValue)]
        public string Value { get; set; }

        public NamedValueClassTable(Session session, string name, string value = null)
          : base(session, name)
        {
          Value = value;
        }
      }

      [HierarchyRoot(InheritanceSchema.ConcreteTable)]
      [KeyGenerator(KeyGeneratorKind.None)]
      public class NamedValueConcreteTable : Entity
      {
        [Key, Field(Length = 12, Nullable = false)]
        public string Name { get; private set; }

        [Field(Length = int.MaxValue)]
        public string Value { get; set; }

        public NamedValueConcreteTable(Session session, string name, string value = null)
          : base(session, name)
        {
          Value = value;
        }
      }

      [HierarchyRoot(InheritanceSchema.SingleTable)]
      [KeyGenerator(KeyGeneratorKind.None)]
      public class NamedValueSingleTable : Entity
      {
        [Key, Field(Length = 12, Nullable = false)]
        public string Name { get; private set; }

        [Field(Length = int.MaxValue)]
        public string Value { get; set; }

        public NamedValueSingleTable(Session session, string name, string value = null)
          : base(session, name)
        {
          Value = value;
        }
      }

      [HierarchyRoot]
      [KeyGenerator(KeyGeneratorKind.None)]
      public class AFirst : Entity
      {
        [Field, Key]
        public Guid Id { get; set; }

        public AFirst(Session session)
          : base(session, Guid.NewGuid())
        {
        }
      }

      [HierarchyRoot]
      [KeyGenerator(KeyGeneratorKind.None)]
      public class VeryLast : Entity
      {
        [Field, Key]
        public Guid Id { get; set; }

        public VeryLast(Session session)
          : base(session, Guid.NewGuid())
        {
        }
      }
    }

    namespace After
    {
      [HierarchyRoot(InheritanceSchema.ClassTable)]
      [KeyGenerator(KeyGeneratorKind.None)]
      [TableMapping("NamedValueClassTable")]
      [Index("Name", Name = "PK_NamedValueClassTable", Clustered = true, Unique = true)]
      public class MasterNamedValueClassTable : NamedValueClassTable
      {
        public static bool CheckAnyExists(Session session)
          => session.Query.All<MasterNamedValueClassTable>().Any();

        public MasterNamedValueClassTable(Session session, string name, string value = null)
          : base(session, name, value)
        {
        }
      }

      public abstract class NamedValueClassTable : Entity
      {
        [Key, Field(Length = 12, Nullable = false)]
        public string Name { get; private set; }

        [Field(Length = int.MaxValue)]
        public string Value { get; set; }

        public NamedValueClassTable(Session session, string name, string value = null)
          : base(session, name)
        {
          Value = value;
        }
      }

      [HierarchyRoot(InheritanceSchema.ConcreteTable)]
      [KeyGenerator(KeyGeneratorKind.None)]
      [TableMapping("NamedValueConcreteTable")]
      [Index("Name", Name = "PK_NamedValueConcreteTable", Clustered = true, Unique = true)]
      public class MasterNamedValueConcreteTable : NamedValueConcreteTable
      {
        public static bool CheckAnyExists(Session session)
          => session.Query.All<MasterNamedValueConcreteTable>().Any();

        public MasterNamedValueConcreteTable(Session session, string name, string value = null)
          : base(session, name, value)
        {
        }
      }

      public abstract class NamedValueConcreteTable : Entity
      {
        [Key, Field(Length = 12, Nullable = false)]
        public string Name { get; private set; }

        [Field(Length = int.MaxValue)]
        public string Value { get; set; }

        public NamedValueConcreteTable(Session session, string name, string value = null)
          : base(session, name)
        {
          Value = value;
        }
      }

      [HierarchyRoot(InheritanceSchema.SingleTable)]
      [KeyGenerator(KeyGeneratorKind.None)]
      [TableMapping("NamedValueSingleTable")]
      [Index("Name", Name = "PK_NamedValueSingleTable", Clustered = true, Unique = true)]
      public class MasterNamedValueSingleTable : NamedValueSingleTable
      {
        public static bool CheckAnyExists(Session session)
          => session.Query.All<MasterNamedValueSingleTable>().Any();

        public MasterNamedValueSingleTable(Session session, string name, string value = null)
          : base(session, name, value)
        {
        }
      }

      public abstract class NamedValueSingleTable : Entity
      {
        [Key, Field(Length = 12, Nullable = false)]
        public string Name { get; private set; }

        [Field(Length = int.MaxValue)]
        public string Value { get; set; }

        public NamedValueSingleTable(Session session, string name, string value = null)
          : base(session, name)
        {
          Value = value;
        }
      }
    }
  }
}