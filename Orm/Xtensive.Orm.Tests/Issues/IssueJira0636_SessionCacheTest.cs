// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Groznov
// Created:    2016.05.16

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Tests.Issues.IssueJira0636_Model.ComplicatedModel.Interfaces;
using IssueModel = Xtensive.Orm.Tests.Issues.IssueJira0636_Model.IssueModel;
using ClassTableModel = Xtensive.Orm.Tests.Issues.IssueJira0636_Model.ComplicatedModel.ClassTableModel;
using ConcreteTableModel = Xtensive.Orm.Tests.Issues.IssueJira0636_Model.ComplicatedModel.ConcreteTableModel;
using SingleTableModel = Xtensive.Orm.Tests.Issues.IssueJira0636_Model.ComplicatedModel.SingleTableModel;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueJira0636_Model
  {
    // Issue model (user case)
    namespace IssueModel
    {
      [HierarchyRoot]
      public class BaseEntity : Entity
      {
        [Key, Field]
        public long Id { get; private set; }

        public BaseEntity()
        {
        }

        public BaseEntity(Session session, long id)
          : base(session, id)
        {
        }
      }

      public class FirstSuccessor : BaseEntity
      {
        [Field]
        public string StringField1 { get; set; }

        public FirstSuccessor()
        {
        }

        public FirstSuccessor(Session session, long id)
          : base(session, id)
        {
        }
      }

      public class SecondSuccessor : BaseEntity
      {
        [Field]
        public string StringField2 { get; set; }

        public SecondSuccessor()
        {
        }

        public SecondSuccessor(Session session, long id)
          : base(session, id)
        {
        }
      }
    }

    // Complicated model for possible apperance of this bug
    namespace ComplicatedModel
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
  }

  public class IssueJira0636_SessionCacheTest : AutoBuildTest
  {
    private struct IdsStruct
    {
      public InheritanceSchema InheritanceSchema { get; private set; }

      public long BaseEntityId { get; private set; }
      public long FirstSuccessorId { get; private set; }
      public long SecondSuccessorId { get; private set; }
      public long SecondSuccessorLeafId { get; private set; }
      public long ThirdSuccessorId { get; private set; }
      public long ThirdSuccessorLeaf1Id { get; private set; }
      public long ThirdSuccessorLeaf2Id { get; private set; }

      public IdsStruct(InheritanceSchema inheritanceSchema, long baseEntityId, long firstSuccessorId, long secondSuccessorId, long secondSuccessorLeafId,
        long thirdSuccessorId, long thirdSuccessorLeaf1Id, long thirdSuccessorLeaf2Id)
        : this()
      {
        InheritanceSchema = inheritanceSchema;
        BaseEntityId = baseEntityId;
        FirstSuccessorId = firstSuccessorId;
        SecondSuccessorId = secondSuccessorId;
        SecondSuccessorLeafId = secondSuccessorLeafId;
        ThirdSuccessorId = thirdSuccessorId;
        ThirdSuccessorLeaf1Id = thirdSuccessorLeaf1Id;
        ThirdSuccessorLeaf2Id = thirdSuccessorLeaf2Id;
      }
    }

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
    private const long WrongId = int.MaxValue - 1;

    #endregion

    private long baseEntityId;
    private long firstSuccessorId;
    private long secondSuccessorId;
    private readonly List<IdsStruct> idsByInheritanceSchema = new List<IdsStruct>();

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (IssueModel.BaseEntity).Assembly, "Xtensive.Orm.Tests.Issues.IssueJira0636_Model");
      configuration.NamingConvention = new NamingConvention { NamespacePolicy = NamespacePolicy.AsIs };
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        #region Issue model entities creation

        baseEntityId = new IssueModel.BaseEntity().Id;
        firstSuccessorId = new IssueModel.FirstSuccessor().Id;
        secondSuccessorId = new IssueModel.SecondSuccessor().Id;

        #endregion

        #region Complicated model entities creation

        #region ClassTableModel entities creation

        IBaseEntity baseEntity = new ClassTableModel.BaseEntity { Field0 = BaseEntityField0 };
        IFirstSuccessor firstSuccessor = new ClassTableModel.FirstSuccessor { Field0 = FirstSuccessorField0, Field1 = FirstSuccessorField1 };
        ISecondSuccessor secondSuccessor = new ClassTableModel.SecondSuccessor { Field0 = SecondSuccessorField0, Field2 = SecondSuccessorField2 };
        ISecondSuccessorLeaf secondSuccessorLeaf = new ClassTableModel.SecondSuccessorLeaf { Field0 = SecondSuccessorLeafField0, Field2 = SecondSuccessorLeafField2, LeafCode = SecondSuccessorLeafCode };
        IThirdSuccessor thirdSuccessor = new ClassTableModel.ThirdSuccessor { Field0 = ThirdSuccessorField0, Field3 = ThirdSuccessorField3 };
        IThirdSuccessorLeaf1 thirdSuccessorLeaf1 = new ClassTableModel.ThirdSuccessorLeaf1 { Field0 = ThirdSuccessorLeaf1Field0, Field3 = ThirdSuccessorLeaf1Field3, Leaf1Code = ThirdSuccessorLeaf1Code };
        IThirdSuccessorLeaf2 thirdSuccessorLeaf2 = new ClassTableModel.ThirdSuccessorLeaf2 { Field0 = ThirdSuccessorLeaf2Field0, Field3 = ThirdSuccessorLeaf2Field3, Leaf2Code = ThirdSuccessorLeaf2Code };

        var classTableStruct = new IdsStruct(InheritanceSchema.ClassTable,
          baseEntity.Id, firstSuccessor.Id, secondSuccessor.Id, secondSuccessorLeaf.Id,
          thirdSuccessor.Id, thirdSuccessorLeaf1.Id, thirdSuccessorLeaf2.Id);
        idsByInheritanceSchema.Add(classTableStruct);

        #endregion

        #region ConcreteTableModel entities creation

        baseEntity = new ConcreteTableModel.BaseEntity { Field0 = BaseEntityField0 };
        firstSuccessor = new ConcreteTableModel.FirstSuccessor { Field0 = FirstSuccessorField0, Field1 = FirstSuccessorField1 };
        secondSuccessor = new ConcreteTableModel.SecondSuccessor { Field0 = SecondSuccessorField0, Field2 = SecondSuccessorField2 };
        secondSuccessorLeaf = new ConcreteTableModel.SecondSuccessorLeaf { Field0 = SecondSuccessorLeafField0, Field2 = SecondSuccessorLeafField2, LeafCode = SecondSuccessorLeafCode };
        thirdSuccessor = new ConcreteTableModel.ThirdSuccessor { Field0 = ThirdSuccessorField0, Field3 = ThirdSuccessorField3 };
        thirdSuccessorLeaf1 = new ConcreteTableModel.ThirdSuccessorLeaf1 { Field0 = ThirdSuccessorLeaf1Field0, Field3 = ThirdSuccessorLeaf1Field3, Leaf1Code = ThirdSuccessorLeaf1Code };
        thirdSuccessorLeaf2 = new ConcreteTableModel.ThirdSuccessorLeaf2 { Field0 = ThirdSuccessorLeaf2Field0, Field3 = ThirdSuccessorLeaf2Field3, Leaf2Code = ThirdSuccessorLeaf2Code };

        var concreteTableStruct = new IdsStruct(InheritanceSchema.ConcreteTable,
          baseEntity.Id, firstSuccessor.Id, secondSuccessor.Id, secondSuccessorLeaf.Id,
          thirdSuccessor.Id, thirdSuccessorLeaf1.Id, thirdSuccessorLeaf2.Id);
        idsByInheritanceSchema.Add(concreteTableStruct);

        #endregion

        #region SingleTableModel entities creation

        baseEntity = new SingleTableModel.BaseEntity { Field0 = BaseEntityField0 };
        firstSuccessor = new SingleTableModel.FirstSuccessor { Field0 = FirstSuccessorField0, Field1 = FirstSuccessorField1 };
        secondSuccessor = new SingleTableModel.SecondSuccessor { Field0 = SecondSuccessorField0, Field2 = SecondSuccessorField2 };
        secondSuccessorLeaf = new SingleTableModel.SecondSuccessorLeaf { Field0 = SecondSuccessorLeafField0, Field2 = SecondSuccessorLeafField2, LeafCode = SecondSuccessorLeafCode };
        thirdSuccessor = new SingleTableModel.ThirdSuccessor { Field0 = ThirdSuccessorField0, Field3 = ThirdSuccessorField3 };
        thirdSuccessorLeaf1 = new SingleTableModel.ThirdSuccessorLeaf1 { Field0 = ThirdSuccessorLeaf1Field0, Field3 = ThirdSuccessorLeaf1Field3, Leaf1Code = ThirdSuccessorLeaf1Code };
        thirdSuccessorLeaf2 = new SingleTableModel.ThirdSuccessorLeaf2 { Field0 = ThirdSuccessorLeaf2Field0, Field3 = ThirdSuccessorLeaf2Field3, Leaf2Code = ThirdSuccessorLeaf2Code };

        var singleTableStruct = new IdsStruct(InheritanceSchema.SingleTable,
          baseEntity.Id, firstSuccessor.Id, secondSuccessor.Id, secondSuccessorLeaf.Id,
          thirdSuccessor.Id, thirdSuccessorLeaf1.Id, thirdSuccessorLeaf2.Id);
        idsByInheritanceSchema.Add(singleTableStruct);

        #endregion

        #endregion

        transaction.Complete();
      }
    }

    #region Issue model tests (including user case

    [Test]
    public void SelectBaseEntityFirstTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        GetEntity<IssueModel.BaseEntity>(WrongId, true);
        GetEntity<IssueModel.BaseEntity>(baseEntityId, false);
        GetEntity<IssueModel.BaseEntity>(firstSuccessorId, false);
        GetEntity<IssueModel.BaseEntity>(secondSuccessorId, false);

        GetEntity<IssueModel.FirstSuccessor>(WrongId, true);
        GetEntity<IssueModel.FirstSuccessor>(baseEntityId, true);
        GetEntity<IssueModel.FirstSuccessor>(firstSuccessorId, false);
        GetEntity<IssueModel.FirstSuccessor>(secondSuccessorId, true);

        GetEntity<IssueModel.SecondSuccessor>(WrongId, true);
        GetEntity<IssueModel.SecondSuccessor>(baseEntityId, true);
        GetEntity<IssueModel.SecondSuccessor>(firstSuccessorId, true);
        GetEntity<IssueModel.SecondSuccessor>(secondSuccessorId, false);
      }
    }

    [Test]
    public void SelectBaseEntityInTheMiddleTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        GetEntity<IssueModel.FirstSuccessor>(WrongId, true);
        GetEntity<IssueModel.FirstSuccessor>(baseEntityId, true);
        GetEntity<IssueModel.FirstSuccessor>(firstSuccessorId, false);
        GetEntity<IssueModel.FirstSuccessor>(secondSuccessorId, true);

        GetEntity<IssueModel.BaseEntity>(WrongId, true);
        GetEntity<IssueModel.BaseEntity>(baseEntityId, false);
        GetEntity<IssueModel.BaseEntity>(firstSuccessorId, false);
        GetEntity<IssueModel.BaseEntity>(secondSuccessorId, false);

        GetEntity<IssueModel.SecondSuccessor>(WrongId, true);
        GetEntity<IssueModel.SecondSuccessor>(baseEntityId, true);
        GetEntity<IssueModel.SecondSuccessor>(firstSuccessorId, true);
        GetEntity<IssueModel.SecondSuccessor>(secondSuccessorId, false);
      }
    }

    [Test]
    public void SelectBaseEntityLastTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        GetEntity<IssueModel.FirstSuccessor>(WrongId, true);
        GetEntity<IssueModel.FirstSuccessor>(baseEntityId, true);
        GetEntity<IssueModel.FirstSuccessor>(firstSuccessorId, false);
        GetEntity<IssueModel.FirstSuccessor>(secondSuccessorId, true);

        GetEntity<IssueModel.SecondSuccessor>(WrongId, true);
        GetEntity<IssueModel.SecondSuccessor>(baseEntityId, true);
        GetEntity<IssueModel.SecondSuccessor>(firstSuccessorId, true);
        GetEntity<IssueModel.SecondSuccessor>(secondSuccessorId, false);

        GetEntity<IssueModel.BaseEntity>(WrongId, true);
        GetEntity<IssueModel.BaseEntity>(baseEntityId, false);
        GetEntity<IssueModel.BaseEntity>(firstSuccessorId, false);
        GetEntity<IssueModel.BaseEntity>(secondSuccessorId, false);
      }
    }

    [Test]
    public void SelectBeforeCreateTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var id = 5001L;
        var entity = Query.SingleOrDefault<IssueModel.BaseEntity>(id);
        Assert.IsNull(entity);
        new IssueModel.BaseEntity(session, id);
        entity = Query.SingleOrDefault<IssueModel.BaseEntity>(id);
        Assert.IsNotNull(entity);
        entity.Remove();
        entity = Query.SingleOrDefault<IssueModel.BaseEntity>(id);
        Assert.IsNull(entity);

        ++id;
        entity = Query.SingleOrDefault<IssueModel.FirstSuccessor>(id);
        Assert.IsNull(entity);
        new IssueModel.FirstSuccessor(session, id);
        entity = Query.SingleOrDefault<IssueModel.FirstSuccessor>(id);
        Assert.IsNotNull(entity);
        entity.Remove();
        entity = Query.SingleOrDefault<IssueModel.FirstSuccessor>(id);
        Assert.IsNull(entity);

        ++id;
        entity = Query.SingleOrDefault<IssueModel.SecondSuccessor>(id);
        Assert.IsNull(entity);
        new IssueModel.SecondSuccessor(session, id);
        entity = Query.SingleOrDefault<IssueModel.FirstSuccessor>(id);
        Assert.IsNull(entity);
        entity = Query.SingleOrDefault<IssueModel.BaseEntity>(id);
        Assert.IsNotNull(entity);
        entity = Query.SingleOrDefault<IssueModel.SecondSuccessor>(id);
        Assert.IsNotNull(entity);
        entity.Remove();
        entity = Query.SingleOrDefault<IssueModel.FirstSuccessor>(id);
        Assert.IsNull(entity);
        entity = Query.SingleOrDefault<IssueModel.BaseEntity>(id);
        Assert.IsNull(entity);
        entity = Query.SingleOrDefault<IssueModel.SecondSuccessor>(id);
        Assert.IsNull(entity);

        ++id;
        entity = Query.SingleOrDefault<IssueModel.FirstSuccessor>(id);
        Assert.IsNull(entity);
        new IssueModel.BaseEntity(session, id);
        entity = Query.SingleOrDefault<IssueModel.BaseEntity>(id);
        Assert.IsNotNull(entity);
        entity = Query.SingleOrDefault<IssueModel.FirstSuccessor>(id);
        Assert.IsNull(entity);
        entity = Query.SingleOrDefault<IssueModel.SecondSuccessor>(id);
        Assert.IsNull(entity);
        entity = Query.Single<IssueModel.BaseEntity>(id);
        entity.Remove();
        entity = Query.SingleOrDefault<IssueModel.BaseEntity>(id);
        Assert.IsNull(entity);
        entity = Query.SingleOrDefault<IssueModel.FirstSuccessor>(id);
        Assert.IsNull(entity);
        entity = Query.SingleOrDefault<IssueModel.SecondSuccessor>(id);
        Assert.IsNull(entity);

        ++id;
        entity = Query.SingleOrDefault<IssueModel.FirstSuccessor>(id);
        Assert.IsNull(entity);
        entity = Query.SingleOrDefault<IssueModel.SecondSuccessor>(id);
        Assert.IsNull(entity);
        new IssueModel.BaseEntity(session, id);
        entity = Query.SingleOrDefault<IssueModel.FirstSuccessor>(id);
        Assert.IsNull(entity);
        entity = Query.SingleOrDefault<IssueModel.SecondSuccessor>(id);
        Assert.IsNull(entity);
        entity = Query.SingleOrDefault<IssueModel.BaseEntity>(id);
        Assert.IsNotNull(entity);
        entity = Query.SingleOrDefault<IssueModel.FirstSuccessor>(id);
        Assert.IsNull(entity);
        entity = Query.Single<IssueModel.BaseEntity>(id);
        entity.Remove();
        entity = Query.SingleOrDefault<IssueModel.FirstSuccessor>(id);
        Assert.IsNull(entity);
        entity = Query.SingleOrDefault<IssueModel.SecondSuccessor>(id);
        Assert.IsNull(entity);
        entity = Query.SingleOrDefault<IssueModel.BaseEntity>(id);
        Assert.IsNull(entity);
      }
    }

    #endregion

    #region Complicated model tests

    [Test]
    public void Test00()
    {
      var actions = new Action[] {
        CheckBaseEntity,
        CheckFirstSuccessor,
        CheckSecondSuccessor,
        CheckSecondSuccessorLeaf,
        CheckThirdSuccessor,
        CheckThirdSuccessorLeaf1,
        CheckThirdSuccessorLeaf2
      };

      InvokeActions(actions);
    }

    [Test]
    public void Test01()
    {
      var actions = new Action[] {
        CheckBaseEntity,
        CheckFirstSuccessor,
        CheckSecondSuccessor,
        CheckThirdSuccessor,
        CheckSecondSuccessorLeaf,
        CheckThirdSuccessorLeaf1,
        CheckThirdSuccessorLeaf2
      };

      InvokeActions(actions);
    }

    [Test]
    public void Test02()
    {
      var actions = new Action[] {
        CheckBaseEntity,
        CheckSecondSuccessorLeaf,
        CheckThirdSuccessorLeaf1,
        CheckThirdSuccessorLeaf2,
        CheckFirstSuccessor,
        CheckSecondSuccessor,
        CheckThirdSuccessor
      };

      InvokeActions(actions);
    }

    [Test]
    public void Test03()
    {
      var actions = new Action[] {
        CheckFirstSuccessor,
        CheckSecondSuccessor,
        CheckThirdSuccessor,
        CheckBaseEntity,
        CheckSecondSuccessorLeaf,
        CheckThirdSuccessorLeaf1,
        CheckThirdSuccessorLeaf2
      };

      InvokeActions(actions);
    }

    [Test]
    public void Test04()
    {
      var actions = new Action[] {
        CheckFirstSuccessor,
        CheckSecondSuccessor,
        CheckThirdSuccessor,
        CheckSecondSuccessorLeaf,
        CheckThirdSuccessorLeaf1,
        CheckThirdSuccessorLeaf2,
        CheckBaseEntity
      };

      InvokeActions(actions);
    }

    [Test]
    public void Test05()
    {
      var actions = new Action[] {
        CheckSecondSuccessorLeaf,
        CheckThirdSuccessorLeaf1,
        CheckThirdSuccessorLeaf2,
        CheckBaseEntity,
        CheckFirstSuccessor,
        CheckSecondSuccessor,
        CheckThirdSuccessor
      };

      InvokeActions(actions);
    }

    [Test]
    public void Test06()
    {
      var actions = new Action[] {
        CheckSecondSuccessorLeaf,
        CheckThirdSuccessorLeaf1,
        CheckThirdSuccessorLeaf2,
        CheckFirstSuccessor,
        CheckSecondSuccessor,
        CheckThirdSuccessor,
        CheckBaseEntity
      };

      InvokeActions(actions);
    }

    [Test]
    public void Test07()
    {
      var actions = new Action[] {
        CheckThirdSuccessorLeaf1,
        CheckThirdSuccessorLeaf2,
        CheckThirdSuccessor,
        CheckSecondSuccessor,
        CheckSecondSuccessorLeaf,
        CheckBaseEntity,
        CheckFirstSuccessor
      };

      InvokeActions(actions);
    }

    [Test]
    public void Test08()
    {
      var actions = new Action[] {
        CheckFirstSuccessor,
        CheckBaseEntity,
        CheckThirdSuccessor,
        CheckSecondSuccessor,
        CheckSecondSuccessorLeaf,
        CheckThirdSuccessorLeaf1,
        CheckThirdSuccessorLeaf2
      };

      InvokeActions(actions);
    }

    [Test]
    public void Test09()
    {
      var actions = new Action[] {
        CheckThirdSuccessorLeaf2,
        CheckSecondSuccessorLeaf,
        CheckSecondSuccessor,
        CheckThirdSuccessorLeaf1,
        CheckThirdSuccessor,
        CheckFirstSuccessor,
        CheckBaseEntity
      };

      InvokeActions(actions);
    }

    [Test]
    public void Test10()
    {
      var actions = new Action[] {
        CheckBaseEntity,
        CheckThirdSuccessorLeaf2,
        CheckThirdSuccessor,
        CheckThirdSuccessorLeaf1,
        CheckSecondSuccessorLeaf,
        CheckSecondSuccessor,
        CheckFirstSuccessor
      };

      InvokeActions(actions);
    }

    [Test]
    public void Test11()
    {
      var actions = new Action[] {
        CheckThirdSuccessor,
        CheckThirdSuccessorLeaf2,
        CheckSecondSuccessorLeaf,
        CheckThirdSuccessorLeaf1,
        CheckSecondSuccessor,
        CheckFirstSuccessor,
        CheckBaseEntity
      };

      InvokeActions(actions);
    }

    [Test]
    public void Test12()
    {
      var actions = new Action[] {
        CheckSecondSuccessor,
        CheckThirdSuccessor,
        CheckSecondSuccessorLeaf,
        CheckThirdSuccessorLeaf1,
        CheckThirdSuccessorLeaf2,
        CheckBaseEntity,
        CheckFirstSuccessor
      };

      InvokeActions(actions);
    }

    [Test]
    public void Test13()
    {
      var actions = new Action[] {
        CheckThirdSuccessorLeaf2,
        CheckThirdSuccessorLeaf1,
        CheckSecondSuccessorLeaf,
        CheckThirdSuccessor,
        CheckSecondSuccessor,
        CheckFirstSuccessor,
        CheckBaseEntity
      };

      InvokeActions(actions);
    }

    #endregion

    #region Helper methods

    private void CheckBaseEntity()
    {
      foreach (var idsStruct in idsByInheritanceSchema) {
        Func<long, bool, IBaseEntity> getEntity;
        switch (idsStruct.InheritanceSchema) {
        case InheritanceSchema.ClassTable:
          getEntity = GetEntity<ClassTableModel.BaseEntity>;
          break;
        case InheritanceSchema.ConcreteTable:
          getEntity = GetEntity<ConcreteTableModel.BaseEntity>;
          break;
        case InheritanceSchema.SingleTable:
          getEntity = GetEntity<SingleTableModel.BaseEntity>;
          break;
        default:
          throw new NotSupportedException();
        }

        var entity = getEntity(idsStruct.BaseEntityId, false);
        Assert.AreEqual(entity.Field0, BaseEntityField0);

        entity = getEntity(idsStruct.FirstSuccessorId, false);
        Assert.AreEqual(entity.Field0, FirstSuccessorField0);

        entity = getEntity(idsStruct.SecondSuccessorId, false);
        Assert.AreEqual(entity.Field0, SecondSuccessorField0);

        entity = getEntity(idsStruct.SecondSuccessorLeafId, false);
        Assert.AreEqual(entity.Field0, SecondSuccessorLeafField0);

        entity = getEntity(idsStruct.ThirdSuccessorId, false);
        Assert.AreEqual(entity.Field0, ThirdSuccessorField0);

        entity = getEntity(idsStruct.ThirdSuccessorLeaf1Id, false);
        Assert.AreEqual(entity.Field0, ThirdSuccessorLeaf1Field0);

        entity = getEntity(idsStruct.ThirdSuccessorLeaf2Id, false);
        Assert.AreEqual(entity.Field0, ThirdSuccessorLeaf2Field0);
      }
    }

    private void CheckFirstSuccessor()
    {
      foreach (var idsStruct in idsByInheritanceSchema) {
        Func<long, bool, IFirstSuccessor> getEntity;
        switch (idsStruct.InheritanceSchema) {
        case InheritanceSchema.ClassTable:
          getEntity = GetEntity<ClassTableModel.FirstSuccessor>;
          break;
        case InheritanceSchema.ConcreteTable:
          getEntity = GetEntity<ConcreteTableModel.FirstSuccessor>;
          break;
        case InheritanceSchema.SingleTable:
          getEntity = GetEntity<SingleTableModel.FirstSuccessor>;
          break;
        default:
          throw new NotSupportedException();
        }

        getEntity(idsStruct.BaseEntityId, true);

        var entity = getEntity(idsStruct.FirstSuccessorId, false);
        Assert.AreEqual(entity.Field0, FirstSuccessorField0);
        Assert.AreEqual(entity.Field1, FirstSuccessorField1);

        getEntity(idsStruct.SecondSuccessorId, true);
        getEntity(idsStruct.SecondSuccessorLeafId, true);
        getEntity(idsStruct.ThirdSuccessorId, true);
        getEntity(idsStruct.ThirdSuccessorLeaf1Id, true);
        getEntity(idsStruct.ThirdSuccessorLeaf2Id, true);
      }
    }

    private void CheckSecondSuccessor()
    {
      foreach (var idsStruct in idsByInheritanceSchema) {
        Func<long, bool, ISecondSuccessor> getEntity;
        switch (idsStruct.InheritanceSchema) {
        case InheritanceSchema.ClassTable:
          getEntity = GetEntity<ClassTableModel.SecondSuccessor>;
          break;
        case InheritanceSchema.ConcreteTable:
          getEntity = GetEntity<ConcreteTableModel.SecondSuccessor>;
          break;
        case InheritanceSchema.SingleTable:
          getEntity = GetEntity<SingleTableModel.SecondSuccessor>;
          break;
        default:
          throw new NotSupportedException();
        }

        getEntity(idsStruct.BaseEntityId, true);
        getEntity(idsStruct.FirstSuccessorId, true);

        var entity = getEntity(idsStruct.SecondSuccessorId, false);
        Assert.AreEqual(entity.Field0, SecondSuccessorField0);
        Assert.AreEqual(entity.Field2, SecondSuccessorField2);

        entity = getEntity(idsStruct.SecondSuccessorLeafId, false);
        Assert.AreEqual(entity.Field0, SecondSuccessorLeafField0);
        Assert.AreEqual(entity.Field2, SecondSuccessorLeafField2);

        getEntity(idsStruct.ThirdSuccessorId, true);
        getEntity(idsStruct.ThirdSuccessorLeaf1Id, true);
        getEntity(idsStruct.ThirdSuccessorLeaf2Id, true);
      }
    }

    private void CheckSecondSuccessorLeaf()
    {
      foreach (var idsStruct in idsByInheritanceSchema) {
        Func<long, bool, ISecondSuccessorLeaf> getEntity;
        switch (idsStruct.InheritanceSchema) {
        case InheritanceSchema.ClassTable:
          getEntity = GetEntity<ClassTableModel.SecondSuccessorLeaf>;
          break;
        case InheritanceSchema.ConcreteTable:
          getEntity = GetEntity<ConcreteTableModel.SecondSuccessorLeaf>;
          break;
        case InheritanceSchema.SingleTable:
          getEntity = GetEntity<SingleTableModel.SecondSuccessorLeaf>;
          break;
        default:
          throw new NotSupportedException();
        }

        getEntity(idsStruct.BaseEntityId, true);
        getEntity(idsStruct.FirstSuccessorId, true);
        getEntity(idsStruct.SecondSuccessorId, true);

        var entity = getEntity(idsStruct.SecondSuccessorLeafId, false);
        Assert.AreEqual(entity.Field0, SecondSuccessorLeafField0);
        Assert.AreEqual(entity.Field2, SecondSuccessorLeafField2);
        Assert.AreEqual(entity.LeafCode, SecondSuccessorLeafCode);

        getEntity(idsStruct.ThirdSuccessorId, true);
        getEntity(idsStruct.ThirdSuccessorLeaf1Id, true);
        getEntity(idsStruct.ThirdSuccessorLeaf2Id, true);
      }
    }

    private void CheckThirdSuccessor()
    {
      foreach (var idsStruct in idsByInheritanceSchema) {
        Func<long, bool, IThirdSuccessor> getEntity;
        switch (idsStruct.InheritanceSchema) {
        case InheritanceSchema.ClassTable:
          getEntity = GetEntity<ClassTableModel.ThirdSuccessor>;
          break;
        case InheritanceSchema.ConcreteTable:
          getEntity = GetEntity<ConcreteTableModel.ThirdSuccessor>;
          break;
        case InheritanceSchema.SingleTable:
          getEntity = GetEntity<SingleTableModel.ThirdSuccessor>;
          break;
        default:
          throw new NotSupportedException();
        }

        getEntity(idsStruct.BaseEntityId, true);
        getEntity(idsStruct.FirstSuccessorId, true);
        getEntity(idsStruct.SecondSuccessorId, true);
        getEntity(idsStruct.SecondSuccessorLeafId, true);

        var entity = getEntity(idsStruct.ThirdSuccessorId, false);
        Assert.AreEqual(entity.Field0, ThirdSuccessorField0);
        Assert.AreEqual(entity.Field3, ThirdSuccessorField3);

        entity = getEntity(idsStruct.ThirdSuccessorLeaf1Id, false);
        Assert.AreEqual(entity.Field0, ThirdSuccessorLeaf1Field0);
        Assert.AreEqual(entity.Field3, ThirdSuccessorLeaf1Field3);

        entity = getEntity(idsStruct.ThirdSuccessorLeaf2Id, false);
        Assert.AreEqual(entity.Field0, ThirdSuccessorLeaf2Field0);
        Assert.AreEqual(entity.Field3, ThirdSuccessorLeaf2Field3);
      }
    }

    private void CheckThirdSuccessorLeaf1()
    {
      foreach (var idsStruct in idsByInheritanceSchema) {
        Func<long, bool, IThirdSuccessorLeaf1> getEntity;
        switch (idsStruct.InheritanceSchema) {
        case InheritanceSchema.ClassTable:
          getEntity = GetEntity<ClassTableModel.ThirdSuccessorLeaf1>;
          break;
        case InheritanceSchema.ConcreteTable:
          getEntity = GetEntity<ConcreteTableModel.ThirdSuccessorLeaf1>;
          break;
        case InheritanceSchema.SingleTable:
          getEntity = GetEntity<SingleTableModel.ThirdSuccessorLeaf1>;
          break;
        default:
          throw new NotSupportedException();
        }

        getEntity(idsStruct.BaseEntityId, true);
        getEntity(idsStruct.FirstSuccessorId, true);
        getEntity(idsStruct.SecondSuccessorId, true);
        getEntity(idsStruct.SecondSuccessorLeafId, true);
        getEntity(idsStruct.ThirdSuccessorId, true);

        var entity = getEntity(idsStruct.ThirdSuccessorLeaf1Id, false);
        Assert.AreEqual(entity.Field0, ThirdSuccessorLeaf1Field0);
        Assert.AreEqual(entity.Field3, ThirdSuccessorLeaf1Field3);
        Assert.AreEqual(entity.Leaf1Code, ThirdSuccessorLeaf1Code);

        getEntity(idsStruct.ThirdSuccessorLeaf2Id, true);
      }
    }

    private void CheckThirdSuccessorLeaf2()
    {
      foreach (var idsStruct in idsByInheritanceSchema) {
        Func<long, bool, IThirdSuccessorLeaf2> getEntity;
        switch (idsStruct.InheritanceSchema) {
        case InheritanceSchema.ClassTable:
          getEntity = GetEntity<ClassTableModel.ThirdSuccessorLeaf2>;
          break;
        case InheritanceSchema.ConcreteTable:
          getEntity = GetEntity<ConcreteTableModel.ThirdSuccessorLeaf2>;
          break;
        case InheritanceSchema.SingleTable:
          getEntity = GetEntity<SingleTableModel.ThirdSuccessorLeaf2>;
          break;
        default:
          throw new NotSupportedException();
        }
        getEntity(idsStruct.BaseEntityId, true);
        getEntity(idsStruct.FirstSuccessorId, true);
        getEntity(idsStruct.SecondSuccessorId, true);
        getEntity(idsStruct.SecondSuccessorLeafId, true);
        getEntity(idsStruct.ThirdSuccessorId, true);
        getEntity(idsStruct.ThirdSuccessorLeaf1Id, true);

        var entity = getEntity(idsStruct.ThirdSuccessorLeaf2Id, false);
        Assert.AreEqual(entity.Field0, ThirdSuccessorLeaf2Field0);
        Assert.AreEqual(entity.Field3, ThirdSuccessorLeaf2Field3);
        Assert.AreEqual(entity.Leaf2Code, ThirdSuccessorLeaf2Code);
      }
    }

    private void InvokeActions(Action[] actions)
    {
      OpenSessionAndAction(() => {
        foreach (var action in actions)
          action.Invoke();
      });
    }

    private void OpenSessionAndAction(Action action)
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        TestLog.InfoRegion("Start openSessionAndAction");
        action();
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
