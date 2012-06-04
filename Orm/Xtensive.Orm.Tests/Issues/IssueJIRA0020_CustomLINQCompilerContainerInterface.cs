// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2011.03.03

using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJIRA0020_CustomLINQCompilerContainerInterface_Model;

namespace Xtensive.Orm.Tests.Issues
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

      public string RegionName
      {
        get { return exprComp(this); }
      }

      private static readonly Expression<Func<Person, string>> expr = p => p.Region.Name;

      private static readonly Func<Person, string> exprComp = expr.Compile();

      /// <summary>
      /// The custom linq compiler container.
      /// </summary>
      [CompilerContainer(typeof (Expression))]
      public static class CustomLinqCompilerContainer
      {
        [Compiler(typeof (Person), "RegionName", TargetKind.PropertyGet)]
        public static Expression PersonRegionName(Expression assignmentExpression)
        {
          return expr.BindParameters(assignmentExpression);
        }

        [Compiler(typeof (ITest), "RegionName", TargetKind.PropertyGet)]
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
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (ITest).Assembly, typeof (ITest).Namespace);
      configuration.Sessions.Default.Options = SessionOptions.AutoActivation;
      return configuration;
    }

    [Test]
    public void VirtualFieldSelect()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var region = new Region {Name = "13123123121"};
        var person = new Person {Age = 1, Region = region};

        var persons = session.Query.All<Person>();

        var personsQuery = persons.Where(p => p.RegionName=="13123123121");
        var result = personsQuery.ToList();
        Assert.AreEqual(1, result.Count);

        var interfaces = persons as IQueryable<ITest>;
        if (interfaces!=null) {
          // Convariant upcast will work in .NET 4.0+
          var interfacesQuery = interfaces.Where(test => test.RegionName=="13123123121");
          var interfaceResult = interfacesQuery.ToList();
          Assert.AreEqual(1, interfaceResult.Count);
        }
      }
    }
  }
}