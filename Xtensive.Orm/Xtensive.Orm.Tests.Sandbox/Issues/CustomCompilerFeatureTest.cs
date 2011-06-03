// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2011.05.29

using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Sandbox.Issues.CustomCompilerFeatureTest_HierarchyModel;
using Xtensive.Orm.Tests.Sandbox.Issues.CustomCompilerFeatureTest_InterfaceModel;

namespace Xtensive.Orm.Tests.Sandbox.Issues
{
  namespace CustomCompilerFeatureTest_InterfaceModel
  {
    public interface IHasVirtualFields : IEntity
    {
      string RegionName { get; }
    }

    [CompilerContainer(typeof (Expression))]
    public static class CustomLinqCompilerContainer
    {
      [Compiler(typeof (IHasVirtualFields), "RegionName", TargetKind.PropertyGet)]
      public static Expression ITestRegionName(Expression _this)
      {
        return null;
      }
    }

    [Serializable]
    [HierarchyRoot]
    public class Person : Entity, IHasVirtualFields
    {
      private static readonly Expression<Func<Person, string>> expr = p => p.Region.Name;

      private static readonly Func<Person, string> exprComp = expr.Compile();

      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public int Age { get; set; }

      [Field]
      public Region Region { get; set; }

      #region Nested type: CustomLinqCompilerContainer

      [CompilerContainer(typeof (Expression))]
      public static class CustomLinqCompilerContainer
      {
        [Compiler(typeof (Person), "RegionName", TargetKind.PropertyGet)]
        public static Expression PersonRegionName(Expression assignmentExpression)
        {
          return expr.BindParameters(assignmentExpression);
        }
      }

      #endregion

      #region IHasVirtualFields Members

      public string RegionName
      {
        get { return exprComp(this); }
      }

      #endregion

      public Person(Session session)
        : base(session)
      {}
    }

    [Serializable]
    [HierarchyRoot]
    public class Company : Entity, IHasVirtualFields
    {
      private static readonly Expression<Func<Company, string>> expr = p => p.Address.Region.Name;

      private static readonly Func<Company, string> exprComp = expr.Compile();

      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Address Address { get; set; }

      #region Nested type: CustomLinqCompilerContainer

      [CompilerContainer(typeof (Expression))]
      public static class CustomLinqCompilerContainer
      {
        [Compiler(typeof (Company), "RegionName", TargetKind.PropertyGet)]
        public static Expression PersonRegionName(Expression assignmentExpression)
        {
          return expr.BindParameters(assignmentExpression);
        }
      }

      #endregion

      #region IHasVirtualFields Members

      public string RegionName
      {
        get { return exprComp(this); }
      }

      #endregion
    }

    [Serializable]
    [HierarchyRoot]
    public class Address : Structure
    {
      [Field]
      public Region Region { get; set; }
    }

    [Serializable]
    [HierarchyRoot]
    public class Region : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Name { get; set; }

      public Region(Session session)
        : base(session)
      {}
    }
  }

  namespace CustomCompilerFeatureTest_HierarchyModel
  {
    [HierarchyRoot]
    public abstract class HasVirtualFields : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      public abstract string RegionName { get; }

      protected HasVirtualFields(Session session)
        : base(session)
      {}
    }

    [CompilerContainer(typeof (Expression))]
    public static class CustomLinqCompilerContainer
    {
      [Compiler(typeof (HasVirtualFields), "RegionName", TargetKind.PropertyGet)]
      public static Expression RootRegionName(Expression _this)
      {
        return null;
      }
    }

    public class Customer : HasVirtualFields
    {
      private static readonly Expression<Func<Customer, string>> expr = p => p.Area.Name;

      private static readonly Func<Customer, string> exprComp = expr.Compile();

      [Field]
      public int Age { get; set; }

      [Field]
      public Area Area { get; set; }

      public override string RegionName
      {
        get { return exprComp(this); }
      }

      #region Nested type: CustomLinqCompilerContainer

      [CompilerContainer(typeof (Expression))]
      public static class CustomLinqCompilerContainer
      {
        [Compiler(typeof (Customer), "RegionName", TargetKind.PropertyGet)]
        public static Expression CustomerRegionName(Expression _this)
        {
          return expr.BindParameters(_this);
        }
      }

      #endregion

      public Customer(Session session)
        : base(session)
      {}
    }

    public class Enterprise : HasVirtualFields
    {
      private static readonly Expression<Func<Enterprise, string>> expr = p => p.BusinessAddress.Area.Name;

      private static readonly Func<Enterprise, string> exprComp = expr.Compile();

      [Field]
      public BusinessAddress BusinessAddress { get; set; }

      public override string RegionName
      {
        get { return exprComp(this); }
      }

      #region Nested type: CustomLinqCompilerContainer

      [CompilerContainer(typeof (Expression))]
      public static class CustomLinqCompilerContainer
      {
        [Compiler(typeof (Enterprise), "RegionName", TargetKind.PropertyGet)]
        public static Expression EnterpriseRegionName(Expression _this)
        {
          return expr.BindParameters(_this);
        }
      }

      #endregion

      public Enterprise(Session session) : base(session)
      {}
    }

    [Serializable]
    [HierarchyRoot]
    public class BusinessAddress : Structure
    {
      [Field]
      public Area Area { get; set; }
    }

    [Serializable]
    [HierarchyRoot]
    public class Area : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Name { get; set; }

      public Area(Session session)
        : base(session)
      {}
    }
  }

  [Serializable]
  public class CustomCompilerFeatureTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(typeof (IHasVirtualFields).Assembly, typeof (IHasVirtualFields).Namespace);
      config.Types.Register(typeof (HasVirtualFields).Assembly, typeof (HasVirtualFields).Namespace);
      config.Sessions.Default.Options = SessionOptions.ServerProfile;
      return config;
    }

    [Test]
    public void HierarchyVirtualFieldTest()
    {
      using (Session s = Domain.OpenSession())
      using (TransactionScope t = s.OpenTransaction()) {
        var r = new Area(s) {Name = "13123123121"};
        var p = new Customer(s) {Age = 1, Area = r};

        var queryable = s.Query.All<Customer>();

        var qwe = from customer in queryable
                                   where customer.RegionName == "13123123121"
                                   select customer;
        Assert.AreEqual(1, qwe.ToList().Count);

        var q = queryable as IQueryable<HasVirtualFields>;
        var qq = from root in q
                                          where root.RegionName == "13123123121"
                                          select root;
        Assert.AreEqual(1, qq.ToList().Count);
      }
    }

    [Test]
    public void InterfaceVirtualFieldTest()
    {
      using (Session s = Domain.OpenSession())
      using (TransactionScope t = s.OpenTransaction()) {
        var r = new Region(s) {Name = "13123123121"};
        var p = new Person(s) {Age = 1, Region = r};

        var queryable = s.Query.All<Person>();

        var qwe = from person in queryable
                                 where person.RegionName == "13123123121"
                                 select person;
        Assert.AreEqual(1, qwe.ToList().Count);

        var q = queryable as IQueryable<IHasVirtualFields>;
        var qq = from root in q
                                           where root.RegionName == "13123123121"
                                           select root;
        Assert.AreEqual(1, qq.ToList().Count);
      }
    }
  }
}