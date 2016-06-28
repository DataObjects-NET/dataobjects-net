// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.04.27

using System;
using NUnit.Framework;
using Xtensive.Orm.Model;
using userCase = Xtensive.Orm.Tests.Issues.IssueJira0613_DataCleanUpInPerformSafelyModel.UserCase;
using upgradeSource = Xtensive.Orm.Tests.Issues.IssueJira0613_DataCleanUpInPerformSafelyModel.Sources;
using upgradeTargets = Xtensive.Orm.Tests.Issues.IssueJira0613_DataCleanUpInPerformSafelyModel.Targets;


namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class IssueJira0613_DataCleanUpInPerformSafely
  {
    [Test]
    public void MainTest()
    {
      var sourceType = typeof (userCase.Source.A1);
      var targetType = typeof (userCase.Target.A1);

      Action populateAction = () => {
        for (int i = 0; i < 10; i++) {
          new userCase.Source.A1 {Date = DateTime.Now, Text = "A1 " + i};
        }
        for (int i = 0; i < 10; i++) {
          new userCase.Source.A2 {Length = i, Text = "A2 " + i};
        }
      };

      Action<Session> validateAction = (session) => {
        var count = session.Query.All<userCase.Source.A1>().Count();
        Assert.That(count, Is.EqualTo(10));

        count = session.Query.All<userCase.Source.A2>().Count();
        Assert.That(count, Is.EqualTo(10));
      };

      RunTest(populateAction, validateAction, sourceType, targetType, true);
      RunTest(populateAction, validateAction, sourceType, targetType, false);
    }

    // Movement handling algorithm is based on source type's hierarchy
    // We need to test only three cases
    // 1) type is moved from ConcreteTable hierarchy
    // 2) type is moved from ClassTable hierarcy
    // 3) type is moved from SingleTable hierarchy
    // Target types' hierarchies are not so important so we are free to choose
    // whatever hierarchy we like. The esiest case is to get target hierarchy of same type

    [Test]
    public void ConcreteTableToConcreteTableMovement()
    {
      var sourceType = typeof (upgradeSource.ConcreteToConcrete.BaseType1);
      var targetType = typeof (upgradeTargets.ConcreteToConcrete.BaseType1);

      Action populateAction = () => {
        for (int i = 0; i < 10; i++) {
          new upgradeSource.ConcreteToConcrete.A1 {
            Date = DateTime.Now.Date,
            Text = (100 + i).ToString()
          };
        }
        for (int i = 0; i < 10; i++) {
          new upgradeSource.ConcreteToConcrete.A2 {
            Date = DateTime.Now.AddDays(10).Date,
            Text = (200 + i).ToString()
          };
        }
      };

      Action<Session> validateAction = (session) => {
        var count = session.Query.All<upgradeSource.ConcreteToConcrete.A1>().Count();
        Assert.That(count, Is.EqualTo(10));

        count = session.Query.All<upgradeSource.ConcreteToConcrete.A2>().Count();
        Assert.That(count, Is.EqualTo(10));
      };

      RunTest(populateAction, validateAction, sourceType, targetType, true);
      RunTest(populateAction, validateAction, sourceType, targetType, false);
    }

    [Test]
    public void ClassTableToClassTable()
    {
      var sourceType = typeof(upgradeSource.ClassToClass.BaseType1);
      var targetType = typeof(upgradeTargets.ClassToClass.BaseType1);

      Action populateAction = () => {
        for (int i = 0; i < 10; i++) {
          new upgradeSource.ClassToClass.A1 {
            Text = (100 + i).ToString(),
            Value = i
          };
        }
        for (int i = 0; i < 10; i++) {
          new upgradeSource.ClassToClass.A2 {
            Date = DateTime.Now.Date,
            Text = (100 + i).ToString()
          };
        }
      };

      Action<Session> validateAction = (session) =>
      {
        var count = session.Query.All<upgradeSource.ClassToClass.A1>().Count();
        Assert.That(count, Is.EqualTo(10));

        count = session.Query.All<upgradeSource.ClassToClass.A2>().Count();
        Assert.That(count, Is.EqualTo(10));
      };

      RunTest(populateAction, validateAction, sourceType, targetType, true);
      RunTest(populateAction, validateAction, sourceType, targetType, false);
    }

    [Test]
    public void SingleTableToSingleTable()
    {
      var sourceType = typeof(upgradeSource.SingleToSingle.BaseType1);
      var targetType = typeof(upgradeTargets.SingleToSingle.BaseType1);

      Action populateAction = () => {
        for (int i = 0; i < 10; i++) {
          new upgradeSource.SingleToSingle.A1 {
            Text = (100 + i).ToString(),
          };
        }
        for (int i = 0; i < 10; i++) {
          new upgradeSource.SingleToSingle.A2 {

            Text = (100 + i).ToString()
          };
        }
      };

      Action<Session> validateAction = (session) =>
      {
        var count = session.Query.All<upgradeSource.SingleToSingle.A1>().Count();
        Assert.That(count, Is.EqualTo(10));

        count = session.Query.All<upgradeSource.SingleToSingle.A2>().Count();
        Assert.That(count, Is.EqualTo(10));
      };

      RunTest(populateAction, validateAction, sourceType, targetType, true);
      // There is no difference in structure of database before and after uprade in this case
      // Needn't run test one more time.
      //RunTest(populateAction, validateAction, sourceType, targetType, false);
    }

    private void RunTest(Action populateAction, Action<Session> validateAction, Type initialType, Type upgradedType, bool isSafelyMode)
    {
      var inintialConfiguration = DomainConfigurationFactory.Create();
      inintialConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
      inintialConfiguration.Types.Register(initialType.Assembly, initialType.Namespace);
      using (var initialDomain = Domain.Build(inintialConfiguration))
      using (var session = initialDomain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        populateAction.Invoke();
        transaction.Complete();
      }

      var upgradingConfiguration = DomainConfigurationFactory.Create();
      upgradingConfiguration.UpgradeMode = (isSafelyMode) ? DomainUpgradeMode.PerformSafely : DomainUpgradeMode.Perform;
      upgradingConfiguration.Types.Register(upgradedType.Assembly, upgradedType.Namespace);
      if (isSafelyMode) {
        Assert.Throws<SchemaSynchronizationException>(() => Domain.Build(upgradingConfiguration));
      }
      else
        Assert.DoesNotThrow(() => Domain.Build(upgradingConfiguration));

      var validationConfiguration = inintialConfiguration.Clone();
      validationConfiguration.UpgradeMode = DomainUpgradeMode.Validate;
      if (isSafelyMode) {
        using (var performedDomain = Domain.Build(validationConfiguration))
        using (var session = performedDomain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          validateAction.Invoke(session);
        }
      }
      else
        // In this kind of tests we have only primary keys changed.
        // To be clear, only name is changed.
        // But MySQL has no named PKs so we have no difference in mysql
        if (validationConfiguration.ConnectionInfo.Provider!=WellKnown.Provider.MySql)
          Assert.Throws<SchemaSynchronizationException>(() => Domain.Build(validationConfiguration));
        else
          Assert.DoesNotThrow(()=>Domain.Build(validationConfiguration));
    }
  }
}

namespace Xtensive.Orm.Tests.Issues.IssueJira0613_DataCleanUpInPerformSafelyModel
{
  namespace UserCase
  {
    namespace Source
    {
      public abstract class BaseType : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Text { get; set; }
      }

      [HierarchyRoot]
      public class A1 : BaseType
      {
        [Field]
        public DateTime Date { get; set; }
      }

      [HierarchyRoot]
      public class A2 : BaseType
      {
        [Field]
        public int Length { get; set; }
      }
    }

    namespace Target
    {
      [HierarchyRoot(InheritanceSchema.ConcreteTable)]
      public abstract class BaseType : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Text { get; set; }
      }

      public class A1 : BaseType
      {
        [Field]
        public DateTime Date { get; set; }
      }

      public class A2 : BaseType
      {
        [Field]
        public int Length { get; set; }
      }
    }
  }

  namespace Sources
  {
    namespace ConcreteToConcrete
    {
      [HierarchyRoot(InheritanceSchema.ConcreteTable)]
      public abstract class BaseType1 : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field]
        public string Text { get; set; }
      }

      public class A1 : BaseType1
      {
        [Field]
        public DateTime Date { get; set; }
      }

      [HierarchyRoot(InheritanceSchema.ConcreteTable)]
      public abstract class BaseType2: Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field]
        public string Text { get; set; }
      }

      public class A2 : BaseType2
      {
        [Field]
        public DateTime Date { get; set; }
      }
    }

    namespace ClassToClass
    {
      [HierarchyRoot]
      public class BaseType1 : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        public string Text { get; set; }
      }

      public class A1 : BaseType1
      {
        public double Value { get; set; }
      }

      public class A2 : BaseType1
      {
        public DateTime Date { get; set; }
      }

      [HierarchyRoot(InheritanceSchema.ClassTable)]
      public class BaseType2 : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        public string Text { get; set; }
      }

      public class A3 : BaseType2
      {
        public double Value { get; set; }
      }

      public class A4 : BaseType2
      {
        public DateTime Date { get; set; }
      }
    }

    namespace SingleToSingle
    {
      [HierarchyRoot(InheritanceSchema.SingleTable)]
      public abstract class BaseType1 : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field]
        public string Text { get; set; }
      }

      public class A1 : BaseType1
      {
      }

      public class A2 : BaseType1
      {
      }

      [HierarchyRoot(InheritanceSchema.SingleTable)]
      public abstract class BaseType2 : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field]
        public string Text { get; set; }
      }

      public class A3 : BaseType2
      {
      }

      public class A4 : BaseType2
      {
      }
    }
  }

  namespace Targets
  {
    namespace ConcreteToConcrete
    {
      [HierarchyRoot(InheritanceSchema.ConcreteTable)]
      public abstract class BaseType1 : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field]
        public string Text { get; set; }
      }

      public class A2 : BaseType1
      {
        [Field]
        public DateTime Date { get; set; }
      }

      [HierarchyRoot(InheritanceSchema.ConcreteTable)]
      public abstract class BaseType2 : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field]
        public string Text { get; set; }
      }

      public class A1 : BaseType2
      {
        [Field]
        public DateTime Date { get; set; }
      }
    }

    namespace ClassToClass
    {
      [HierarchyRoot]
      public class BaseType1 : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        public string Text { get; set; }
      }

      public class A3 : BaseType1
      {
        public double Value { get; set; }
      }

      public class A4 : BaseType1
      {
        public DateTime Date { get; set; }
      }

      [HierarchyRoot(InheritanceSchema.ClassTable)]
      public class BaseType2 : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        public string Text { get; set; }
      }

      public class A1 : BaseType2
      {
        public double Value { get; set; }
      }

      public class A2 : BaseType2
      {
        public DateTime Date { get; set; }
      }
    }

    namespace SingleToSingle
    {
      [HierarchyRoot(InheritanceSchema.SingleTable)]
      public abstract class BaseType1 : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field]
        public string Text { get; set; }
      }

      public class A3 : BaseType1
      {
      }

      public class A4 : BaseType1
      {
      }

      [HierarchyRoot(InheritanceSchema.SingleTable)]
      public abstract class BaseType2 : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field]
        public string Text { get; set; }
      }

      public class A1 : BaseType2
      {
      }

      public class A2 : BaseType2
      {
      }
    }
  }
}
