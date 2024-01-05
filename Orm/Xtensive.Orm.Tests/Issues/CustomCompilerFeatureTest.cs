﻿// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2011.05.29

using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.CustomCompilerFeatureTest_HierarchyModel;
using Xtensive.Orm.Tests.Issues.CustomCompilerFeatureTest_InterfaceModel;

namespace Xtensive.Orm.Tests.Issues
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
      {
      }
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
      {
      }
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
      {
      }
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
      {
      }
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

      public Enterprise(Session session)
        : base(session)
      {
      }
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
      {
      }
    }
  }

  [Serializable]
  public class CustomCompilerFeatureTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (IHasVirtualFields).Assembly, typeof (IHasVirtualFields).Namespace);
      configuration.Types.Register(typeof (HasVirtualFields).Assembly, typeof (HasVirtualFields).Namespace);
      configuration.Sessions.Default.Options = SessionOptions.ServerProfile;
      return configuration;
    }

    [Test]
    public void HierarchyVirtualFieldTest()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var area = new Area(session) {Name = "13123123121"};
        var customer = new Customer(session) {Age = 1, Area = area};

        var customers = session.Query.All<Customer>();
        var customersQuery = customers.Where(c => c.RegionName=="13123123121");
        Assert.AreEqual(1, customersQuery.ToList().Count);

        var virtualEntities = customers as IQueryable<HasVirtualFields>;
        if (virtualEntities!=null) {
          // Covariant upcast will work in .NET 4.0+
          var virtualEntitiesQuery = virtualEntities.Where(item => item.RegionName=="13123123121");
          Assert.AreEqual(1, virtualEntitiesQuery.ToList().Count);
        }

        var allCustomers = session.Query.All<Customer>()
          .OrderBy(c => c.RegionName)
          .ToList();

        var allVirtualEntities = session.Query.All<HasVirtualFields>()
          .OrderBy(c => c.RegionName)
          .ToList();
      }
    }

    [Test]
    public void InterfaceVirtualFieldTest()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var region = new Region(session) {Name = "13123123121"};
        var person = new Person(session) {Age = 1, Region = region};

        var persons = session.Query.All<Person>();
        var personsQuery = persons.Where(p => p.RegionName=="13123123121");
        Assert.AreEqual(1, personsQuery.ToList().Count);

        var virtualEntities = persons as IQueryable<IHasVirtualFields>;
        if (virtualEntities!=null) {
          // Covariant upcast will work in .NET 4.0+
          var virtualEntitiesQuery = virtualEntities.Where(root => root.RegionName=="13123123121");
          Assert.AreEqual(1, virtualEntitiesQuery.ToList().Count);
        }

        var allPersons = session.Query.All<Person>()
          .OrderBy(c => c.RegionName)
          .ToList();

        var allVirtualEntities = session.Query.All<IHasVirtualFields>()
          .OrderBy(c => c.RegionName)
          .ToList();
      }
    }
  }
}