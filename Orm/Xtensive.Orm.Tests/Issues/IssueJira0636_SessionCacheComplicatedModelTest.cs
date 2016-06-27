// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Groznov
// Created:    2016.05.16

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Tests.Issues.IssueJira0636_ComplicatedModel.Interfaces;
using ClassTableModel = Xtensive.Orm.Tests.Issues.IssueJira0636_ComplicatedModel.ClassTableModel;
using ConcreteTableModel = Xtensive.Orm.Tests.Issues.IssueJira0636_ComplicatedModel.ConcreteTableModel;
using SingleTableModel = Xtensive.Orm.Tests.Issues.IssueJira0636_ComplicatedModel.SingleTableModel;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueJira0636_ComplicatedModel
  {
    namespace Interfaces
    {
      internal interface IBaseEntity
      {
        long Id { get; }
        string Field0 { get; set; }
      }

      internal interface IFirstSuccessor : IBaseEntity
      {
        string Field1 { get; set; }
      }

      internal interface ISecondSuccessor : IBaseEntity
      {
        string Field2 { get; set; }
      }

      internal interface ISecondSuccessorLeaf : ISecondSuccessor
      {
        int LeafCode { get; set; }
      }

      internal interface IThirdSuccessor : IBaseEntity
      {
        string Field3 { get; set; }
      }

      internal interface IThirdSuccessorLeaf1 : IThirdSuccessor
      {
        int Leaf1Code { get; set; }
      }

      internal interface IThirdSuccessorLeaf2 : IThirdSuccessor
      {
        int Leaf2Code { get; set; }
      }
    }

    namespace ClassTableModel
    {
      /// <summary>
      /// Base entity
      /// </summary>
      [HierarchyRoot(InheritanceSchema.ClassTable)]
      public class BaseEntity : Entity, IBaseEntity
      {
        [Key, Field]
        public long Id { get; private set; }

        [Field]
        public string Field0 { get; set; }
      }

      /// <summary>
      /// Successor without own successors
      /// </summary>
      public class FirstSuccessor : BaseEntity, IFirstSuccessor
      {
        [Field]
        public string Field1 { get; set; }
      }

      /// <summary>
      /// Successor with one own successor
      /// </summary>
      public class SecondSuccessor : BaseEntity, ISecondSuccessor
      {
        [Field]
        public string Field2 { get; set; }
      }

      public class SecondSuccessorLeaf : SecondSuccessor, ISecondSuccessorLeaf
      {
        [Field]
        public int LeafCode { get; set; }
      }

      /// <summary>
      /// Successor with two own successors
      /// </summary>
      public class ThirdSuccessor : BaseEntity, IThirdSuccessor
      {
        [Field]
        public string Field3 { get; set; }
      }

      public class ThirdSuccessorLeaf1 : ThirdSuccessor, IThirdSuccessorLeaf1
      {
        [Field]
        public int Leaf1Code { get; set; }
      }

      public class ThirdSuccessorLeaf2 : ThirdSuccessor, IThirdSuccessorLeaf2
      {
        [Field]
        public int Leaf2Code { get; set; }
      }
    }

    namespace ConcreteTableModel
    {
      /// <summary>
      /// Base entity
      /// </summary>
      [HierarchyRoot(InheritanceSchema.ConcreteTable)]
      public class BaseEntity : Entity, IBaseEntity
      {
        [Key, Field]
        public long Id { get; private set; }

        [Field]
        public string Field0 { get; set; }
      }

      /// <summary>
      /// Successor without own successors
      /// </summary>
      public class FirstSuccessor : BaseEntity, IFirstSuccessor
      {
        [Field]
        public string Field1 { get; set; }
      }

      /// <summary>
      /// Successor with one own successor
      /// </summary>
      public class SecondSuccessor : BaseEntity, ISecondSuccessor
      {
        [Field]
        public string Field2 { get; set; }
      }

      public class SecondSuccessorLeaf : SecondSuccessor, ISecondSuccessorLeaf
      {
        [Field]
        public int LeafCode { get; set; }
      }

      /// <summary>
      /// Successor with two own successors
      /// </summary>
      public class ThirdSuccessor : BaseEntity, IThirdSuccessor
      {
        [Field]
        public string Field3 { get; set; }
      }

      public class ThirdSuccessorLeaf1 : ThirdSuccessor, IThirdSuccessorLeaf1
      {
        [Field]
        public int Leaf1Code { get; set; }
      }

      public class ThirdSuccessorLeaf2 : ThirdSuccessor, IThirdSuccessorLeaf2
      {
        [Field]
        public int Leaf2Code { get; set; }
      }
    }

    namespace SingleTableModel
    {
      /// <summary>
      /// Base entity
      /// </summary>
      [HierarchyRoot(InheritanceSchema.SingleTable)]
      public class BaseEntity : Entity, IBaseEntity
      {
        [Key, Field]
        public long Id { get; private set; }

        [Field]
        public string Field0 { get; set; }
      }

      /// <summary>
      /// Successor without own successors
      /// </summary>
      public class FirstSuccessor : BaseEntity, IFirstSuccessor
      {
        [Field]
        public string Field1 { get; set; }
      }

      /// <summary>
      /// Successor with one own successor
      /// </summary>
      public class SecondSuccessor : BaseEntity, ISecondSuccessor
      {
        [Field]
        public string Field2 { get; set; }
      }

      public class SecondSuccessorLeaf : SecondSuccessor, ISecondSuccessorLeaf
      {
        [Field]
        public int LeafCode { get; set; }
      }

      /// <summary>
      /// Successor with two own successors
      /// </summary>
      public class ThirdSuccessor : BaseEntity, IThirdSuccessor
      {
        [Field]
        public string Field3 { get; set; }
      }

      public class ThirdSuccessorLeaf1 : ThirdSuccessor, IThirdSuccessorLeaf1
      {
        [Field]
        public int Leaf1Code { get; set; }
      }

      public class ThirdSuccessorLeaf2 : ThirdSuccessor, IThirdSuccessorLeaf2
      {
        [Field]
        public int Leaf2Code { get; set; }
      }
    }
  }

  [TestFixture]
  public class IssueJira0636_SessionCacheComplicatedModelTest
  {
    #region Consts

    private const string BaseEntityField0 = "BaseEntityField0";

    private const string FirstSuccessorField0 = "FirstSuccessorField0";
    private const string FirstSuccessorField1 = "FirstSuccessorField1";

    private const string SecondSuccessorField0 = "SecondSuccessorField0";
    private const string SecondSuccessorField2 = "SecondSuccessorField2";

    private const string SecondSuccessorLeafField0 = "SecondSuccessorLeafField0";
    private const string SecondSuccessorLeafField2 = "SecondSuccessorLeafField2";
    private const int SecondSuccessorLeafCode = 99;

    private const string ThirdSuccessorField0 = "ThirdSuccessorField0";
    private const string ThirdSuccessorField3 = "ThirdSuccessorField3";

    private const string ThirdSuccessorLeaf1Field0 = "ThirdSuccessorLeaf1Field0";
    private const string ThirdSuccessorLeaf1Field3 = "ThirdSuccessorLeaf1Field3";
    private const int ThirdSuccessorLeaf1Code = 98;

    private const string ThirdSuccessorLeaf2Field0 = "ThirdSuccessorLeaf2Field0";
    private const string ThirdSuccessorLeaf2Field3 = "ThirdSuccessorLeaf2Field3";
    private const int ThirdSuccessorLeaf2Code = 97;

    #endregion

    #region EntityIds

    private long baseEntityId = -1;
    private long firstSuccessorId = -1;
    private long secondSuccessorId = -1;
    private long thirdSuccessorId = -1;
    private long secondSuccessorLeafId = -1;
    private long thirdSuccessorLeaf1Id = -1;
    private long thirdSuccessorLeaf2Id = -1;

    #endregion

    [Test]
    public void ClassTableModelTest()
    {
      MainTest<
        ClassTableModel.BaseEntity,
        ClassTableModel.FirstSuccessor,
        ClassTableModel.SecondSuccessor,
        ClassTableModel.SecondSuccessorLeaf,
        ClassTableModel.ThirdSuccessor,
        ClassTableModel.ThirdSuccessorLeaf1,
        ClassTableModel.ThirdSuccessorLeaf2>();
    }

    [Test]
    public void ConcreteTableModelTest()
    {
      MainTest<
        ConcreteTableModel.BaseEntity,
        ConcreteTableModel.FirstSuccessor,
        ConcreteTableModel.SecondSuccessor,
        ConcreteTableModel.SecondSuccessorLeaf,
        ConcreteTableModel.ThirdSuccessor,
        ConcreteTableModel.ThirdSuccessorLeaf1,
        ConcreteTableModel.ThirdSuccessorLeaf2>();
    }

    [Test]
    public void SingleTableModelTest()
    {
      MainTest<
        SingleTableModel.BaseEntity,
        SingleTableModel.FirstSuccessor,
        SingleTableModel.SecondSuccessor,
        SingleTableModel.SecondSuccessorLeaf,
        SingleTableModel.ThirdSuccessor,
        SingleTableModel.ThirdSuccessorLeaf1,
        SingleTableModel.ThirdSuccessorLeaf2>();
    }

    #region Private methods

    private void MainTest<TBaseEntity, TFirstSuccessor, TSecondSuccessor, TSecondSuccessorLeaf, TThirdSuccessor, TThirdSuccessorLeaf1, TThirdSuccessorLeaf2>()
      where TBaseEntity : Entity, IBaseEntity, new()
      where TFirstSuccessor : TBaseEntity, IFirstSuccessor, new()
      where TSecondSuccessor : TBaseEntity, ISecondSuccessor, new()
      where TSecondSuccessorLeaf : TSecondSuccessor, ISecondSuccessorLeaf, new()
      where TThirdSuccessor : TBaseEntity, IThirdSuccessor, new()
      where TThirdSuccessorLeaf1 : TThirdSuccessor, IThirdSuccessorLeaf1, new()
      where TThirdSuccessorLeaf2 : TThirdSuccessor, IThirdSuccessorLeaf2, new()
    {
      var config = CreateConfig(DomainUpgradeMode.Recreate, typeof (TBaseEntity));

      using (var domain = Domain.Build(config))
        OpenSessionAndAction(domain, true, () => {
          var baseEntity = new TBaseEntity { Field0 = BaseEntityField0 };
          var firstSuccessor = new TFirstSuccessor { Field0 = FirstSuccessorField0, Field1 = FirstSuccessorField1 };
          var secondSuccessor = new TSecondSuccessor { Field0 = SecondSuccessorField0, Field2 = SecondSuccessorField2 };
          var secondSuccessorLeaf = new TSecondSuccessorLeaf { Field0 = SecondSuccessorLeafField0, Field2 = SecondSuccessorLeafField2, LeafCode = SecondSuccessorLeafCode };
          var thirdSuccessor = new TThirdSuccessor { Field0 = ThirdSuccessorField0, Field3 = ThirdSuccessorField3 };
          var thirdSuccessorLeaf1 = new TThirdSuccessorLeaf1 { Field0 = ThirdSuccessorLeaf1Field0, Field3 = ThirdSuccessorLeaf1Field3, Leaf1Code = ThirdSuccessorLeaf1Code };
          var thirdSuccessorLeaf2 = new TThirdSuccessorLeaf2 { Field0 = ThirdSuccessorLeaf2Field0, Field3 = ThirdSuccessorLeaf2Field3, Leaf2Code = ThirdSuccessorLeaf2Code };

          baseEntityId = baseEntity.Id;
          firstSuccessorId = firstSuccessor.Id;
          secondSuccessorId = secondSuccessor.Id;
          secondSuccessorLeafId = secondSuccessorLeaf.Id;
          thirdSuccessorId = thirdSuccessor.Id;
          thirdSuccessorLeaf1Id = thirdSuccessorLeaf1.Id;
          thirdSuccessorLeaf2Id = thirdSuccessorLeaf2.Id;
        });

      config = CreateConfig(DomainUpgradeMode.Validate, typeof (TBaseEntity));
      using (var domain = Domain.Build(config)) {
        var actions = new Action[] {
          CheckBaseEntity<TBaseEntity>, // 0
          CheckFirstSuccessor<TFirstSuccessor>, // 1
          CheckSecondSuccessor<TSecondSuccessor>, // 2
          CheckThirdSuccessor<TThirdSuccessor>, // 3
          CheckSecondSuccessorLeaf<TSecondSuccessorLeaf>, // 4
          CheckThirdSuccessorLeaf1<TThirdSuccessorLeaf1>, // 5
          CheckThirdSuccessorLeaf2<TThirdSuccessorLeaf2>, // 6
        };

        InvokeActions(domain, actions, 0, 1, 2, 3, 4, 5, 6); // GetEntityOrder: base entity, successors, leafs
        InvokeActions(domain, actions, 0, 4, 5, 6, 1, 2, 3); // GetEntityOrder: base entity, leafs, successors
        InvokeActions(domain, actions, 1, 2, 3, 0, 4, 5, 6); // GetEntityOrder: successors,  base entity, leafs
        InvokeActions(domain, actions, 1, 2, 3, 4, 5, 6, 0); // GetEntityOrder: successors, leafs, base entity
        InvokeActions(domain, actions, 4, 5, 6, 0, 1, 2, 3); // GetEntityOrder: leafs, base entity, successors
        InvokeActions(domain, actions, 4, 5, 6, 1, 2, 3, 0); // GetEntityOrder: leafs, successors, base entity

        InvokeActions(domain, actions, 6, 5, 3, 2, 4, 0, 1); // GetEntityOrder: thirdSuccessorLeafs, thirdSuccessor, secondSuccessor, secondSuccessorLeaf, baseEntity, firstSuccessor
        InvokeActions(domain, actions, 1, 0, 3, 2, 4, 5, 6); // GetEntityOrder: firstSuccessor, baseEntity, thirdSuccessor, secondSuccessor, leafs

        InvokeActions(domain, actions, 6, 4, 2, 5, 3, 1, 0);
        InvokeActions(domain, actions, 0, 6, 3, 5, 4, 2, 1);
        InvokeActions(domain, actions, 3, 6, 4, 5, 2, 1, 0);
        InvokeActions(domain, actions, 2, 3, 4, 5, 6, 0, 1);
        InvokeActions(domain, actions, 6, 5, 4, 3, 2, 1, 0);
      }
    }

    private void InvokeActions(Domain domain, Action[] actions, params int[] indexes)
    {
      OpenSessionAndAction(domain, false, () => {
        foreach (var index in indexes)
          actions[index].Invoke();
      });
    }

    private void CheckBaseEntity<TBaseEntity>()
      where TBaseEntity : Entity, IBaseEntity
    {
      var entity = GetEntity<TBaseEntity>(baseEntityId, false);
      Assert.AreEqual(entity.Field0, BaseEntityField0);

      entity = GetEntity<TBaseEntity>(firstSuccessorId, false);
      Assert.AreEqual(entity.Field0, FirstSuccessorField0);

      entity = GetEntity<TBaseEntity>(secondSuccessorId, false);
      Assert.AreEqual(entity.Field0, SecondSuccessorField0);

      entity = GetEntity<TBaseEntity>(secondSuccessorLeafId, false);
      Assert.AreEqual(entity.Field0, SecondSuccessorLeafField0);

      entity = GetEntity<TBaseEntity>(thirdSuccessorId, false);
      Assert.AreEqual(entity.Field0, ThirdSuccessorField0);

      entity = GetEntity<TBaseEntity>(thirdSuccessorLeaf1Id, false);
      Assert.AreEqual(entity.Field0, ThirdSuccessorLeaf1Field0);

      entity = GetEntity<TBaseEntity>(thirdSuccessorLeaf2Id, false);
      Assert.AreEqual(entity.Field0, ThirdSuccessorLeaf2Field0);
    }

    private void CheckFirstSuccessor<TFirstSuccessor>()
      where TFirstSuccessor : Entity, IFirstSuccessor
    {
      GetEntity<TFirstSuccessor>(baseEntityId, true);

      var entity = GetEntity<TFirstSuccessor>(firstSuccessorId, false);
      Assert.AreEqual(entity.Field0, FirstSuccessorField0);
      Assert.AreEqual(entity.Field1, FirstSuccessorField1);

      GetEntity<TFirstSuccessor>(secondSuccessorId, true);
      GetEntity<TFirstSuccessor>(secondSuccessorLeafId, true);
      GetEntity<TFirstSuccessor>(thirdSuccessorId, true);
      GetEntity<TFirstSuccessor>(thirdSuccessorLeaf1Id, true);
      GetEntity<TFirstSuccessor>(thirdSuccessorLeaf2Id, true);
    }

    private void CheckSecondSuccessor<TSecondSuccessor>()
      where TSecondSuccessor : Entity, ISecondSuccessor
    {
      GetEntity<TSecondSuccessor>(baseEntityId, true);
      GetEntity<TSecondSuccessor>(firstSuccessorId, true);

      var entity = GetEntity<TSecondSuccessor>(secondSuccessorId, false);
      Assert.AreEqual(entity.Field0, SecondSuccessorField0);
      Assert.AreEqual(entity.Field2, SecondSuccessorField2);

      entity = GetEntity<TSecondSuccessor>(secondSuccessorLeafId, false);
      Assert.AreEqual(entity.Field0, SecondSuccessorLeafField0);
      Assert.AreEqual(entity.Field2, SecondSuccessorLeafField2);

      GetEntity<TSecondSuccessor>(thirdSuccessorId, true);
      GetEntity<TSecondSuccessor>(thirdSuccessorLeaf1Id, true);
      GetEntity<TSecondSuccessor>(thirdSuccessorLeaf2Id, true);
    }

    private void CheckSecondSuccessorLeaf<TSecondSuccessorLeaf>()
      where TSecondSuccessorLeaf : Entity, ISecondSuccessorLeaf
    {
      GetEntity<TSecondSuccessorLeaf>(baseEntityId, true);
      GetEntity<TSecondSuccessorLeaf>(firstSuccessorId, true);
      GetEntity<TSecondSuccessorLeaf>(secondSuccessorId, true);

      var entity = GetEntity<TSecondSuccessorLeaf>(secondSuccessorLeafId, false);
      Assert.AreEqual(entity.Field0, SecondSuccessorLeafField0);
      Assert.AreEqual(entity.Field2, SecondSuccessorLeafField2);
      Assert.AreEqual(entity.LeafCode, SecondSuccessorLeafCode);

      GetEntity<TSecondSuccessorLeaf>(thirdSuccessorId, true);
      GetEntity<TSecondSuccessorLeaf>(thirdSuccessorLeaf1Id, true);
      GetEntity<TSecondSuccessorLeaf>(thirdSuccessorLeaf2Id, true);
    }

    private void CheckThirdSuccessor<TThirdSuccessor>()
      where TThirdSuccessor : Entity, IThirdSuccessor
    {
      GetEntity<TThirdSuccessor>(baseEntityId, true);
      GetEntity<TThirdSuccessor>(firstSuccessorId, true);
      GetEntity<TThirdSuccessor>(secondSuccessorId, true);
      GetEntity<TThirdSuccessor>(secondSuccessorLeafId, true);

      var entity = GetEntity<TThirdSuccessor>(thirdSuccessorId, false);
      Assert.AreEqual(entity.Field0, ThirdSuccessorField0);
      Assert.AreEqual(entity.Field3, ThirdSuccessorField3);

      entity = GetEntity<TThirdSuccessor>(thirdSuccessorLeaf1Id, false);
      Assert.AreEqual(entity.Field0, ThirdSuccessorLeaf1Field0);
      Assert.AreEqual(entity.Field3, ThirdSuccessorLeaf1Field3);

      entity = GetEntity<TThirdSuccessor>(thirdSuccessorLeaf2Id, false);
      Assert.AreEqual(entity.Field0, ThirdSuccessorLeaf2Field0);
      Assert.AreEqual(entity.Field3, ThirdSuccessorLeaf2Field3);
    }

    private void CheckThirdSuccessorLeaf1<TThirdSuccessorLeaf1>()
      where TThirdSuccessorLeaf1 : Entity, IThirdSuccessorLeaf1
    {
      GetEntity<TThirdSuccessorLeaf1>(baseEntityId, true);
      GetEntity<TThirdSuccessorLeaf1>(firstSuccessorId, true);
      GetEntity<TThirdSuccessorLeaf1>(secondSuccessorId, true);
      GetEntity<TThirdSuccessorLeaf1>(secondSuccessorLeafId, true);
      GetEntity<TThirdSuccessorLeaf1>(thirdSuccessorId, true);

      var entity = GetEntity<TThirdSuccessorLeaf1>(thirdSuccessorLeaf1Id, false);
      Assert.AreEqual(entity.Field0, ThirdSuccessorLeaf1Field0);
      Assert.AreEqual(entity.Field3, ThirdSuccessorLeaf1Field3);
      Assert.AreEqual(entity.Leaf1Code, ThirdSuccessorLeaf1Code);

      GetEntity<TThirdSuccessorLeaf1>(thirdSuccessorLeaf2Id, true);
    }

    private void CheckThirdSuccessorLeaf2<TThirdSuccessorLeaf2>()
      where TThirdSuccessorLeaf2 : Entity, IThirdSuccessorLeaf2
    {
      GetEntity<TThirdSuccessorLeaf2>(baseEntityId, true);
      GetEntity<TThirdSuccessorLeaf2>(firstSuccessorId, true);
      GetEntity<TThirdSuccessorLeaf2>(secondSuccessorId, true);
      GetEntity<TThirdSuccessorLeaf2>(secondSuccessorLeafId, true);
      GetEntity<TThirdSuccessorLeaf2>(thirdSuccessorId, true);
      GetEntity<TThirdSuccessorLeaf2>(thirdSuccessorLeaf1Id, true);

      var entity = GetEntity<TThirdSuccessorLeaf2>(thirdSuccessorLeaf2Id, false);
      Assert.AreEqual(entity.Field0, ThirdSuccessorLeaf2Field0);
      Assert.AreEqual(entity.Field3, ThirdSuccessorLeaf2Field3);
      Assert.AreEqual(entity.Leaf2Code, ThirdSuccessorLeaf2Code);
    }

    private DomainConfiguration CreateConfig(DomainUpgradeMode upgradeMode, Type type)
    {
      var config = DomainConfigurationFactory.Create();
      config.UpgradeMode = upgradeMode;
      config.Types.Register(type.Assembly, type.Namespace);
      return config;
    }

    private void OpenSessionAndAction(Domain domain, bool commitTransaction, Action action)
    {
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        TestLog.InfoRegion("Start openSessionAndAction");
        action();
        if (commitTransaction)
          tx.Complete();
        TestLog.Info("Finish openSessionAndAction");
      }
    }

    private T GetEntity<T>(long keyValue, bool expectedNull)
      where T : Entity
    {
      var entity = Query.SingleOrDefault<T>(keyValue);
      if (expectedNull)
        Assert.IsNull(entity);
      else
        Assert.IsNotNull(entity);
      return entity;
    }

    #endregion
  }
}
