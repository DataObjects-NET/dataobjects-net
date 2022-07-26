// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Modelling.Actions;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Upgrade;
using V1 = Xtensive.Orm.Tests.Upgrade.ConflictsByTable.ExactTableStructureNoGeneratorTestModel.Before;
using V2 = Xtensive.Orm.Tests.Upgrade.ConflictsByTable.ExactTableStructureNoGeneratorTestModel.After;
using TheTestHelper = Xtensive.Orm.Tests.Upgrade.ConflictsByTable.ExactTableStructureNoGeneratorTestModel.TestHelper;

namespace Xtensive.Orm.Tests.Upgrade.ConflictsByTable
{
  public abstract class TestBase
  {
    [Test]
    [TestCase(InheritanceSchema.SingleTable, true)]
    [TestCase(InheritanceSchema.ClassTable, true)]
    [TestCase(InheritanceSchema.ConcreteTable, true)]
    [TestCase(InheritanceSchema.SingleTable, false)]
    [TestCase(InheritanceSchema.ClassTable, false)]
    [TestCase(InheritanceSchema.ConcreteTable, false)]
    public async ValueTask PerformSafelyTest(InheritanceSchema inheritanceSchema, bool asyncBuild)
    {
      var initDomainTypes = GetTypes(inheritanceSchema, false);
      BuildAndPopulateDomain(initDomainTypes, DomainUpgradeMode.Recreate);

      var upgradeDomainTypes = GetTypes(inheritanceSchema, true);
      var ex = asyncBuild
        ? Assert.ThrowsAsync<SchemaSynchronizationException>(async () => { using (await BuildDomainAsync(DomainUpgradeMode.PerformSafely, upgradeDomainTypes)) { } })
        : Assert.Throws<SchemaSynchronizationException>(() => { using (BuildDomain(DomainUpgradeMode.PerformSafely, upgradeDomainTypes)) { } });

      CheckComparisonResult(inheritanceSchema, ex.ComparisonResult);

      initDomainTypes = GetTypes(inheritanceSchema, false);
      if (asyncBuild) {
        await using (var domain = await BuildDomainAsync(DomainUpgradeMode.Validate, initDomainTypes))
        await using (var session = await domain.OpenSessionAsync())
        await using (var tx = await session.OpenTransactionAsync()) {
          ValidateDataRemains(inheritanceSchema, session);
        }
      }
      else {
        using (var domain = BuildDomain(DomainUpgradeMode.Validate, initDomainTypes))
        using (var session = domain.OpenSession())
        using (var tx = session.OpenTransaction()) {
          ValidateDataRemains(inheritanceSchema, session);
        }
      }
    }

    [Test]
    [TestCase(InheritanceSchema.SingleTable, true)]
    [TestCase(InheritanceSchema.ClassTable, true)]
    [TestCase(InheritanceSchema.ConcreteTable, true)]
    [TestCase(InheritanceSchema.SingleTable, false)]
    [TestCase(InheritanceSchema.ClassTable, false)]
    [TestCase(InheritanceSchema.ConcreteTable, false)]
    public void PerformSafelyWithHintsTest(InheritanceSchema inheritanceSchema, bool asyncBuild)
    {
      var initDomainTypes = GetTypes(inheritanceSchema, false);
      BuildAndPopulateDomain(initDomainTypes, DomainUpgradeMode.Recreate);

      var upgradeDomainTypes = GetTypes(inheritanceSchema, true, true);
      Domain domain = null;
      try {
        if (asyncBuild) {
          Assert.DoesNotThrowAsync(async () => domain = await BuildDomainAsync(DomainUpgradeMode.PerformSafely, upgradeDomainTypes));
        }
        else {
          Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.PerformSafely, upgradeDomainTypes));
        }
      }
      catch (SuccessException) {
        domain.DisposeSafely();
        throw;
      }
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        ValidateDataExists(inheritanceSchema, session);
      }
    }

    [Test]
    [TestCase(InheritanceSchema.SingleTable, true)]
    [TestCase(InheritanceSchema.ClassTable, true)]
    [TestCase(InheritanceSchema.ConcreteTable, true)]
    [TestCase(InheritanceSchema.SingleTable, false)]
    [TestCase(InheritanceSchema.ClassTable, false)]
    [TestCase(InheritanceSchema.ConcreteTable, false)]
    public void PerformTest(InheritanceSchema inheritanceSchema, bool asyncBuild)
    {
      var initDomainTypes = GetTypes(inheritanceSchema, false);
      BuildAndPopulateDomain(initDomainTypes, DomainUpgradeMode.Recreate);

      var upgradeDomainTypes = GetTypes(inheritanceSchema, true);

      Domain domain = null;
      try {
        if (asyncBuild) {
          Assert.DoesNotThrowAsync(async () => domain = await BuildDomainAsync(DomainUpgradeMode.Perform, upgradeDomainTypes));
        }
        else {
          Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.Perform, upgradeDomainTypes));
        }
      }
      catch (SuccessException) {
        domain.DisposeSafely();
        throw;
      }
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        ValidateDataDisapeared(inheritanceSchema, session);
      }
    }

    [Test]
    [TestCase(InheritanceSchema.SingleTable, true)]
    [TestCase(InheritanceSchema.ClassTable, true)]
    [TestCase(InheritanceSchema.ConcreteTable, true)]
    [TestCase(InheritanceSchema.SingleTable, false)]
    [TestCase(InheritanceSchema.ClassTable, false)]
    [TestCase(InheritanceSchema.ConcreteTable, false)]
    public void PerformWithHintsTest(InheritanceSchema inheritanceSchema, bool asyncBuild)
    {
      var initDomainTypes = GetTypes(inheritanceSchema, false);
      BuildAndPopulateDomain(initDomainTypes, DomainUpgradeMode.Recreate);

      var upgradeDomainTypes = GetTypes(inheritanceSchema, true, true);

      Domain domain = null;
      try {
        if (asyncBuild) {
          Assert.DoesNotThrowAsync(async () => domain = await BuildDomainAsync(DomainUpgradeMode.Perform, upgradeDomainTypes));
        }
        else {
          Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.Perform, upgradeDomainTypes));
        }
      }
      catch (SuccessException) {
        domain.DisposeSafely();
        throw;
      }
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        ValidateDataExists(inheritanceSchema, session);
      }
    }


    protected abstract void CheckSingleTableComparisonResult(SchemaComparisonResult comparisonResult);
    protected abstract void CheckConcreteTableComparisonResult(SchemaComparisonResult comparisonResult);
    protected abstract void CheckClassTableComparisonResult(SchemaComparisonResult comparisonResult);

    protected abstract void ValidateSingleTableDataRemains(Session session);
    protected abstract void ValidateSingleTableDataExists(Session session);
    protected abstract void ValidateSingleTableDataDisapeared(Session session);

    protected abstract void ValidateConcreteTableDataRemains(Session session);
    protected abstract void ValidateConcreteTableDataExists(Session session);
    protected abstract void ValidateConcreteTableDataDisapeared(Session session);

    protected abstract void ValidateClassTableDataRemains(Session session);
    protected abstract void ValidateClassTableDataExists(Session session);
    protected abstract void ValidateClassTableDataDisapeared(Session session);

    protected abstract void PopulateSingleTableDomain(Session session);
    protected abstract void PopulateConcreteTableDomain(Session session);
    protected abstract void PopulateClassTableDomain(Session session);

    protected abstract Type[] GetTypes(InheritanceSchema inheritanceSchema, bool isAfter, bool? withHints = null);

    private void CheckComparisonResult(InheritanceSchema inheritanceSchema, SchemaComparisonResult comparisonResult)
    {
      switch (inheritanceSchema) {
        case InheritanceSchema.ClassTable:
          CheckClassTableComparisonResult(comparisonResult);
          break;
        case InheritanceSchema.ConcreteTable:
          CheckConcreteTableComparisonResult(comparisonResult);
          break;
        case InheritanceSchema.SingleTable:
          CheckSingleTableComparisonResult(comparisonResult);
          break;
      }
    }

    private void ValidateDataExists(InheritanceSchema inheritanceSchema, Session session)
    {
      switch (inheritanceSchema) {
        case InheritanceSchema.ClassTable:
          ValidateClassTableDataExists(session);
          break;
        case InheritanceSchema.ConcreteTable:
          ValidateConcreteTableDataExists(session);
          break;
        case InheritanceSchema.SingleTable:
          ValidateSingleTableDataExists(session);
          break;
      }
    }

    private void ValidateDataRemains(InheritanceSchema inheritanceSchema, Session session)
    {
      switch (inheritanceSchema) {
        case InheritanceSchema.ClassTable:
          ValidateClassTableDataRemains(session);
          break;
        case InheritanceSchema.ConcreteTable:
          ValidateConcreteTableDataRemains(session);
          break;
        case InheritanceSchema.SingleTable:
          ValidateSingleTableDataRemains(session);
          break;
      }
    }

    private void ValidateDataDisapeared(InheritanceSchema inheritanceSchema, Session session)
    {
      switch (inheritanceSchema) {
        case InheritanceSchema.ClassTable:
          ValidateClassTableDataDisapeared(session);
          break;
        case InheritanceSchema.ConcreteTable:
          ValidateConcreteTableDataDisapeared(session);
          break;
        case InheritanceSchema.SingleTable:
          ValidateSingleTableDataDisapeared(session);
          break;
      }
    }

    private void BuildAndPopulateDomain(Type[] types, DomainUpgradeMode upgradeMode)
    {
      using (var initialDomain = BuildDomain(upgradeMode, types))
      using (var session = initialDomain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var schema = initialDomain.Model.Hierarchies.First(h => !h.Root.IsSystem).InheritanceSchema;
        switch (schema) {
          case InheritanceSchema.SingleTable:
            PopulateSingleTableDomain(session);
            break;
          case InheritanceSchema.ClassTable:
            PopulateClassTableDomain(session);
            break;
          case InheritanceSchema.ConcreteTable:
            PopulateConcreteTableDomain(session);
            break;
        }

        t.Complete();
      }
    }

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
  }
}
