// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2011.03.03

using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Transactions;

using NUnit.Framework;

using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Orm.Configuration;

using System;

using Xtensive.Orm;
using Xtensive.Orm.Tests.Sandbox.Issues.IssueJIRA0020_CustomLINQCompilerContainerInterface_Model;

namespace Xtensive.Orm.Tests.Sandbox.Issues
{
  namespace IssueJIRA0020_CustomLINQCompilerContainerInterface_Model
  {
    

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

        [Compiler(typeof(ITest), "RegionName", TargetKind.PropertyGet)]
        public static Expression ITestRegionName(Expression assignmentExpression)
        {
          Expression<Func<ITest, string>> le = it => it is Person ? (it as Person).Region.Name : null;
          return le.BindParameters(assignmentExpression);
        }
      }
    }

    [Serializable]
    [HierarchyRoot]
    public class Region : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Name { get; set; }
    }

    public interface ITest : IEntity
    {
      string RegionName { get; }
    }
  }

  [Serializable]
  public class IssueJIRA0020_CustomLINQCompilerContainerInterface : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(ITest).Assembly, typeof(ITest).Namespace);
      config.Sessions.Default.Options = SessionOptions.AutoActivation;
      return config;
    }

    [Test]
//    [Transactional(ActivateSession = true)]
    public void VirtualFieldSelect1()
    {
    }

    /// <summary>
    /// Проверка виртуальных полей
    /// </summary>
    [Test]
    public void VirtualFieldSelect2()
    {
      using (var s = Domain.OpenSession())
      using (var t = s.OpenTransaction()) {
        var r = new Region { Name = "13123123121" };
        var p = new Person { Age = 1, Region = r };

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