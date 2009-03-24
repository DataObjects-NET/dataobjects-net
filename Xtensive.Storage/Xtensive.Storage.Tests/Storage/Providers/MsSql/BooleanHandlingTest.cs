// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.03.20

using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Tests.Storage.Providers.MsSql
{
  [HierarchyRoot(typeof(KeyGenerator), "Id")]
  public class MyEntity : Entity
  {
    [Field]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public bool Flag { get; set; }

    [Field]
    public bool HasStupidName { get; set; }
  }

  [TestFixture]
  public class BooleanHandlingTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = DomainConfigurationFactory.Create("mssql2005");
      configuration.Types.Register(Assembly.GetExecutingAssembly(), typeof(MyEntity).Namespace);
      return configuration;
    }

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          new MyEntity {Name = "Yes", Flag = true, HasStupidName = false};
          new MyEntity {Name = "YesYes", Flag = true, HasStupidName = true};
          new MyEntity {Name = "No", Flag = false, HasStupidName = false};
          new MyEntity {Name = "NoNo", Flag = false, HasStupidName = true};
          t.Complete();
        }
      }
    }

    [Test]
    public void SelectFieldTest()
    {
      TestQuery(() =>
        from o in Query<MyEntity>.All
        select o.Flag
        );
    }

    [Test]
    public void SelectNotFieldTest()
    {
      TestQuery(() =>
        from o in Query<MyEntity>.All
        select !o.Flag
        );
    }

    [Test]
    public void SelectCalculatedTest()
    {
      TestQuery(() =>
        from o in Query<MyEntity>.All
        select new { value = o.Name.StartsWith("Yes") }
        );
    }

    [Test]
    public void WhereColumnTest()
    {
      TestQuery(() =>
        from o in Query<MyEntity>.All
        where o.Flag
        select o
        );
    }

    [Test]
    public void WhereNotColumnTest()
    {
      TestQuery(() => 
        from o in Query<MyEntity>.All
        where !o.Flag
        select o
        );
    }

    [Test]
    public void WhereColumnAndColumnTest()
    {
      TestQuery(() =>
        from o in Query<MyEntity>.All
        where o.Flag && o.HasStupidName
        select o
        );
    }

    [Test]
    public void WhereColumnOrColumnTest()
    {
      TestQuery(() =>
        from o in Query<MyEntity>.All
        where o.Flag || o.HasStupidName
        select o
        );
    }

    [Test]
    public void WhereColumnEqualsColumnTest()
    {
      TestQuery(() =>
        from o in Query<MyEntity>.All
        where o.Flag==o.HasStupidName
        select o
        );
    }

    [Test]
    public void WhereColumnNotEqualsColumnTest()
    {
      TestQuery(() =>
        from o in Query<MyEntity>.All
        where o.Flag!=o.HasStupidName
        select o
        );
    }

    [Test]
    public void WhereColumnOrExpressionTest()
    {
      TestQuery(() =>
        from o in Query<MyEntity>.All
        where o.Flag || (o.Id > 6)
        select o
        );
    }

    [Test]
    public void WhereColumnAndExpressionTest()
    {
      TestQuery(() =>
        from o in Query<MyEntity>.All
        where o.Flag && o.Name.StartsWith("Yes")
        select o
        );
    }

    [Test]
    public void WhereColumnEqualsExpressionTest()
    {
      TestQuery(() =>
        from o in Query<MyEntity>.All
        where o.Flag == (o.Id == 5)
        select o
        );
    }

    [Test]
    public void WhereColumnNotEqualsExpressionTest()
    {
      TestQuery(() =>
        from o in Query<MyEntity>.All
        where o.Flag != o.Name.StartsWith("No")
        select o
        );
    }

    [Test]
    public void WhereExpressionEqualsExpressionTest()
    {
      TestQuery(() =>
        from o in Query<MyEntity>.All
        where o.Name.StartsWith("Yes")==(o.Id==5)
        select o
        );
    }

    [Test]
    public void WhereExpressionNotEqualsExpressionTest()
    {
      TestQuery(() =>
        from o in Query<MyEntity>.All
        where o.Name.StartsWith("Yes")!=(o.Id < 5)
        select o
        );
    }

    [Test]
    public void WhereParameterTest()
    {
      var parameter = true;
      TestQuery(() =>
        from o in Query<MyEntity>.All
        where parameter
        select o
        );
    }

    [Test]
    public void WhereNotParameterTest()
    {
      var parameter = true;
      TestQuery(() =>
        from o in Query<MyEntity>.All
        where !parameter
        select o
        );
    }

    [Test]
    public void WhereParameterOrColumnTest()
    {
      var parameter = true;
      TestQuery(() =>
        from o in Query<MyEntity>.All
        where parameter || o.Flag
        select o
        );
    }

    [Test]
    public void WhereParameterAndColumnTest()
    {
      var parameter = true;
      TestQuery(() =>
        from o in Query<MyEntity>.All
        where parameter && o.HasStupidName
        select o
        );
    }

    [Test]
    public void WhereParameterEqualsColumnTest()
    {
      var parameter = true;
      TestQuery(() =>
        from o in Query<MyEntity>.All
        where parameter==o.Flag
        select o
        );
    }

    [Test]
    public void WhereParameterNotEqualsColumnTest()
    {
      var parameter = true;
      TestQuery(() =>
        from o in Query<MyEntity>.All
        where parameter!=o.Flag
        select o
        );
    }

    [Test]
    public void WhereParameterOrExpressionTest()
    {
      var parameter = true;
      TestQuery(() =>
        from o in Query<MyEntity>.All
        where parameter || o.Name.EndsWith("Yes")
        select o
        );
    }

    [Test]
    public void WhereParameterAndExpressionTest()
    {
      var parameter = true;
      TestQuery(() =>
        from o in Query<MyEntity>.All
        where parameter && o.Name.EndsWith("No")
        select o
        );
    }

    [Test]
    public void WhereParameterEqualsExpressionTest()
    {
      var parameter = true;
      TestQuery(() =>
        from o in Query<MyEntity>.All
        where parameter==o.Name.EndsWith("Yes")
        select o
        );
    }

    [Test]
    public void WhereParameterNotEqualsExpressionTest()
    {
      var parameter = true;
      TestQuery(() =>
        from o in Query<MyEntity>.All
        where parameter!=o.Name.StartsWith("No")
        select o
        );
    }

    [Test]
    public void ComplexTest()
    {
      var parameter1 = true;
      var parameter2 = false;
      TestQuery(() =>
        from o in Query<MyEntity>.All
        where (!o.Flag && o.Name.StartsWith("No") || parameter1) && (o.HasStupidName || !parameter2)
        select new {value = o.Id % 10 < 5, flag = o.Flag, notflag = !o.Flag}
        );
    }

    private void TestQuery<T>(Func<IQueryable<T>> query)
    {
      using (Domain.OpenSession()) {
        using (Transaction.Open()) {
          var list = query.Invoke().ToList();
        }
      }
    }
  }
}