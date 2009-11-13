// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.11.13

using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Core.Linq;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Linq.CustomExpressionCompilersModel;
using System.Linq;

namespace Xtensive.Storage.Tests.Linq.CustomExpressionCompilersModel
{
  [HierarchyRoot]
  public class Person : Entity
  {
    [Field]
    [Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public string Surname { get; set; }

    [Field]
    public DateTime BirthDay { get; set; }

    public string Fullname
    {
      get { return GetFullname(Name); }
    }

    public string GetFullname(string name)
    {
      return string.Format("{0} {1}", name, Surname); 
    }
  }
}

namespace Xtensive.Storage.Tests.Linq
{
  [CompilerContainer(typeof(Expression))]
  public static class CustomLinqCompilerContainer
  {
    [Compiler(typeof(Person), "Fullname", TargetKind.PropertyGet)]
    public static Expression Fullname(Expression _this)
    {
      throw new NotImplementedException();
    }

    [Compiler(typeof(Person), "GetFullname", TargetKind.Method)]
    public static Expression GetFullname(Expression _this, Expression name)
    {
      throw new NotImplementedException();
    }
  }

  public class CustomExpressionCompilers : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Person).Assembly, typeof (Person).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          Fill();
          var expected = Query<Person>.All.AsEnumerable().OrderBy(p=>p.Id).Select(p => p.Fullname).ToList();
          Assert.Greater(expected.Count, 0);
          var fullNames1 = Query<Person>.All.OrderBy(p=>p.Id).Select(p => p.Fullname).ToList();
          var fullNames2 = Query<Person>.All.OrderBy(p=>p.Id).Select(p => p.GetFullname(p.Name)).ToList();
          Assert.IsTrue(expected.SequenceEqual(fullNames1));
          Assert.IsTrue(expected.SequenceEqual(fullNames2));
          // Rollback
        }
      }
    }

    private void Fill()
    {
      new Person {Name = "Ivan", Surname = "Semenov"};
      new Person {Name = "John", Surname = "Smith"};
      new Person {Name = "Andrew", Surname = "Politkovsky"};
    }
  }
}