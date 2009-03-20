// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.03.20

using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Tests.MsSql
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
      using (Domain.OpenSession()) {
        using (Transaction.Open()) {
          var result =
            from o in Query<MyEntity>.All
            select o.Flag;
          var list = result.ToList();
        }
      }
    }

    [Test]
    public void SelectCalculatedTest()
    {
      using (Domain.OpenSession()) {
        using (Transaction.Open()) {
          var result =
            from o in Query<MyEntity>.All
            select new { value = o.Name.StartsWith("Yes") };
          var list = result.ToList();
        }
      }
    }

    [Test]
    public void WhereColumnTest()
    {
      using (Domain.OpenSession()) {
        using (Transaction.Open()) {
          var result =
            from o in Query<MyEntity>.All
            where o.Flag
            select o;
          var list = result.ToList();
        }
      }
    }

    [Test]
    public void WhereNotColumnTest()
    {
      using (Domain.OpenSession()) {
        using (Transaction.Open()) {
          var result =
            from o in Query<MyEntity>.All
            where !o.Flag
            select o;
          var list = result.ToList();
        }
      }
    }

    [Test]
    public void WhereColumnAndColumnTest()
    {
      using (Domain.OpenSession()) {
        using (Transaction.Open()) {
          var result =
            from o in Query<MyEntity>.All
            where o.Flag && o.HasStupidName
            select o;
          var list = result.ToList();
        }
      }
    }

    [Test]
    public void WhereColumnOrColumnTest()
    {
      using (Domain.OpenSession()) {
        using (Transaction.Open()) {
          var result =
            from o in Query<MyEntity>.All
            where o.Flag || o.HasStupidName
            select o;
          var list = result.ToList();
        }
      }
    }

    [Test]
    public void WhereColumnEqualsExpressionTest()
    {
      using (Domain.OpenSession()) {
        using (Transaction.Open()) {
          var result =
            from o in Query<MyEntity>.All
            where o.Flag == (o.Id==5)
            select o;
          var list = result.ToList();
        }
      }
    }

    [Test]
    public void WhereColumnNotEqualsExpressionTest()
    {
      using (Domain.OpenSession()) {
        using (Transaction.Open()) {
          var result =
            from o in Query<MyEntity>.All
            where o.Flag != o.Name.StartsWith("No")
            select o;
          var list = result.ToList();
        }
      }
    }

    [Test]
    public void WhereColumnOrExpressionTest()
    {
      using (Domain.OpenSession()) {
        using (Transaction.Open()) {
          var result =
            from o in Query<MyEntity>.All
            where o.Flag || (o.Id > 6)
            select o;
          var list = result.ToList();
        }
      }
    }

    [Test]
    public void WhereColumnAndExpressionTest() {
      using (Domain.OpenSession()) {
        using (Transaction.Open()) {
          var result =
            from o in Query<MyEntity>.All
            where o.Flag && o.Name.StartsWith("Yes")
            select o;
          var list = result.ToList();
        }
      }
    }

    [Test]
    public void WhereExpressionEqualsExpressionTest()
    {
      using (Domain.OpenSession()) {
        using (Transaction.Open()) {
          var result =
            from o in Query<MyEntity>.All
            where o.Name.StartsWith("Yes")==(o.Id==5)
            select o;
          var list = result.ToList();
        }
      }      
    }

    [Test]
    public void WhereExpressionNotEqualsExpressionTest()
    {
      using (Domain.OpenSession()) {
        using (Transaction.Open()) {
          var result =
            from o in Query<MyEntity>.All
            where o.Name.StartsWith("Yes") != (o.Id < 5)
            select o;
          var list = result.ToList();
        }
      }
    }

    [Test]
    public void ComplexTest()
    {
      using (Domain.OpenSession()) {
        using (Transaction.Open()) {
          var result =
            from o in Query<MyEntity>.All
            where !o.Flag && o.Name.StartsWith("No") || o.HasStupidName
            select new {value = o.Id % 10 < 5, flag = o.Flag, notflag = !o.Flag};
          var list = result.ToList();
        }
      }
    }
  }
}
