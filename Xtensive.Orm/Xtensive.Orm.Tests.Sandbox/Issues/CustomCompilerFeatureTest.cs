// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2011.05.29

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Transactions;

using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Model;
using Xtensive.Reflection;
using Xtensive.Linq;
using Xtensive.Orm.Configuration;

using System;

using Xtensive.Orm;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Tests.Sandbox.Issues.CustomCompilerFeatureTest_Model;

namespace Xtensive.Orm.Tests.Sandbox.Issues
{
  namespace CustomCompilerFeatureTest_Model
  {
    public interface ITest : IEntity
    {
      string RegionName { get; }
    }

    /// <summary>
    /// The custom linq compiler container.
    /// </summary>
    [CompilerContainer(typeof(Expression))]
    public static class CustomLinqCompilerContainer
    {
      [Compiler(typeof(ITest), "RegionName", TargetKind.PropertyGet)]
      public static Expression ITestRegionName(Expression _this)
      {
        throw new NotImplementedException();
      }
    }

    [Serializable]
    [HierarchyRoot]
    public class Person : Entity, ITest
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public int Age { get; set; }

      [Field]
      public Region Region { get; set; }

      public string RegionName { get { return exprComp(this); } }

      private static readonly Expression<Func<Person, string>> expr = p => p.Region.Name;

      private static readonly Func<Person, string> exprComp = expr.Compile();

      public Person(Session session)
        : base(session)
      {}

      /// <summary>
      /// The custom linq compiler container.
      /// </summary>
      [CompilerContainer(typeof(Expression))]
      public static class CustomLinqCompilerContainer
      {
        [Compiler(typeof(Person), "RegionName", TargetKind.PropertyGet)]
        public static Expression PersonRegionName(Expression assignmentExpression)
        {
          return expr.BindParameters(assignmentExpression);
        }
      }
    }

    [Serializable]
    [HierarchyRoot]
    public class Company : Entity, ITest
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Address Address { get; set; }

      public string RegionName { get { return exprComp(this); } }

      private static readonly Expression<Func<Company, string>> expr = p => p.Address.Region.Name;

      private static readonly Func<Company, string> exprComp = expr.Compile();

      /// <summary>
      /// The custom linq compiler container.
      /// </summary>
      [CompilerContainer(typeof(Expression))]
      public static class CustomLinqCompilerContainer
      {
        [Compiler(typeof(Company), "RegionName", TargetKind.PropertyGet)]
        public static Expression PersonRegionName(Expression assignmentExpression)
        {
          return expr.BindParameters(assignmentExpression);
        }
      }
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
      public Region(Session session)
        : base(session)
      {
      }

      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Name { get; set; }
    }
  }

  [Serializable]
  public class CustomCompilerFeatureTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(ITest).Assembly, typeof(ITest).Namespace);
      config.Sessions.Default.Options = SessionOptions.ServerProfile;
      return config;
    }

    /// <summary>
    /// Проверка виртуальных полей
    /// </summary>
    [Test]
    public void VirtualFieldSelect2()
    {
      using (var s = Domain.OpenSession())
      using (var t = s.OpenTransaction())
      {
        var r = new Region(s) { Name = "13123123121" };
        var p = new Person(s) { Age = 1, Region = r };

        var queryable = s.Query.All<Person>();

        var qwe = from person in queryable
                  where person.RegionName == "13123123121"
                  select person;
        var result = qwe.ToList();
        Assert.AreEqual(1, result.Count);

        var q = queryable as IQueryable<ITest>;
        var qq = from test in q
                 where test.RegionName == "13123123121"
                 select test;
        var interfaceResult = qq.ToList();
        Assert.AreEqual(1, interfaceResult.Count);
      }
    }
  }
}