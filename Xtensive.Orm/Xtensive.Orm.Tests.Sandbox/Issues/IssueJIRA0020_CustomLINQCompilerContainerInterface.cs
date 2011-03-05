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

      public string RegionName { get { return exprComp(Region); } }

      private static readonly Expression<Func<Region, string>> expr = r => r.Name;

      private static readonly Func<Region, string> exprComp = expr.Compile();

      /// <summary>
      /// The custom linq compiler container.
      /// </summary>
      [CompilerContainer(typeof(Expression))]
      public static class CustomLinqCompilerContainer
      {
        /// <summary>
        /// The current.
        /// </summary>
        /// <param name="assignmentExpression">
        /// The assignment expression.
        /// </param>
        /// <returns>
        /// </returns>
        [Compiler(typeof(Person), "RegionName", TargetKind.PropertyGet)]
        public static Expression Current(Expression assignmentExpression)
        {
          return expr.BindParameters(assignmentExpression);
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
  public class IssueJIRA0020_CustomLINQCompilerContainerInterface
  {
    private Domain domain;

    [TestFixtureSetUp]
    public virtual void Setup()
    {
      var cfg = new DomainConfiguration("sqlserver", "Data Source=.; Initial Catalog=DO40-Tests; Integrated Security=True;Connection Timeout=300")
      {
        UpgradeMode = DomainUpgradeMode.Recreate,
        ValidationMode = ValidationMode.OnDemand,
        NamingConvention = new NamingConvention { NamingRules = NamingRules.UnderscoreDots }
      };

      cfg.Sessions.Add(new SessionConfiguration("Default")
      {
        BatchSize = 25,
        DefaultIsolationLevel = IsolationLevel.ReadCommitted,
        CacheSize = 1000,
        Options = SessionOptions.Default | SessionOptions.AutoTransactionOpenMode | SessionOptions.AutoActivation
      });

      cfg.Types.Register(Assembly.GetExecutingAssembly());

      domain = Domain.Build(cfg);
    }

    /// <summary>
    /// Проверка виртуальных полей
    /// </summary>
    [Test]
    [Transactional(ActivateSession = true)]
    public void VirtualFieldSelect1()
    {
    }

    /// <summary>
    /// Проверка виртуальных полей
    /// </summary>
    [Test]
    public void VirtualFieldSelect2()
    {
      using (var s = domain.OpenSession())
      {
        var r = new Region { Name = "13123123121" };
        var p = new Person { Age = 1, Region = r };

        var queryable = Query.All<Person>();

        var qwe = from person in queryable
                  where person.RegionName != null
                  select person;
        Assert.DoesNotThrow(() => qwe.ToArray());

        var q = queryable as IQueryable<ITest>;
        var qq = from test in q
                 where test.RegionName != null
                 select test;
        Assert.DoesNotThrow(() => qq.ToArray());
      }
    }
  }
}