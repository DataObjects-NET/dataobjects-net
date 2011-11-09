// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.03.20

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples;
using Tuple = Xtensive.Core.Tuples.Tuple;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Tests.Storage.Providers.Sql.BooleanHandlingTestModel;

namespace Xtensive.Storage.Tests.Storage.Providers.Sql.BooleanHandlingTestModel
{
  [Serializable]
  [HierarchyRoot]
  public class MyEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public bool? Flag { get; set; }

    [Field]
    public bool HasStupidName { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Storage.Providers.Sql
{
  [TestFixture]
  public class BooleanHandlingTest : AutoBuildTest
  {
    protected override void CheckRequirements()
    {
      Require.AllFeaturesNotSupported(ProviderFeatures.FullFeaturedBooleanExpressions);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (MyEntity).Assembly, typeof (MyEntity).Namespace);
      return configuration;
    }

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      using (Session.Open(Domain)) {
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
    public void SelectParameterTest()
    {
      var value = true;
      TestQuery(() =>
        from o in Query.All<MyEntity>()
        select value
        );
    }

    [Test]
    public void SelectFieldTest()
    {
      TestQuery(() =>
        from o in Query.All<MyEntity>()
        select o.Flag
        );
    }

    [Test]
    public void SelectNotFieldTest()
    {
      TestQuery(() =>
        from o in Query.All<MyEntity>()
        select !o.Flag
        );
    }

    [Test]
    public void SelectCalculatedTest()
    {
      TestQuery(() =>
        from o in Query.All<MyEntity>()
        select new { value = o.Name.StartsWith("Yes") }
        );
    }

    [Test]
    public void WhereColumnTest()
    {
      TestQuery(() =>
        from o in Query.All<MyEntity>()
        where o.Flag.Value
        select o
        );
    }

    [Test]
    public void WhereNotColumnTest()
    {
      TestQuery(() => 
        from o in Query.All<MyEntity>()
        where !o.Flag.Value
        select o
        );
    }

    [Test]
    public void WhereColumnAndColumnTest()
    {
      TestQuery(() =>
        from o in Query.All<MyEntity>()
        where o.Flag.Value && o.HasStupidName
        select o
        );
    }

    [Test]
    public void WhereColumnOrColumnTest()
    {
      TestQuery(() =>
        from o in Query.All<MyEntity>()
        where o.Flag.Value || o.HasStupidName
        select o
        );
    }

    [Test]
    public void WhereColumnEqualsColumnTest()
    {
      TestQuery(() =>
        from o in Query.All<MyEntity>()
        where o.Flag==o.HasStupidName
        select o
        );
    }

    [Test]
    public void WhereColumnNotEqualsColumnTest()
    {
      TestQuery(() =>
        from o in Query.All<MyEntity>()
        where o.Flag!=o.HasStupidName
        select o
        );
    }

    [Test]
    public void WhereColumnOrExpressionTest()
    {
      TestQuery(() =>
        from o in Query.All<MyEntity>()
        where o.Flag.Value || (o.Id > 6)
        select o
        );
    }

    [Test]
    public void WhereColumnAndExpressionTest()
    {
      TestQuery(() =>
        from o in Query.All<MyEntity>()
        where o.Flag.Value && o.Name.StartsWith("Yes")
        select o
        );
    }

    [Test]
    public void WhereColumnEqualsExpressionTest()
    {
      TestQuery(() =>
        from o in Query.All<MyEntity>()
        where o.Flag == (o.Id == 5)
        select o
        );
    }

    [Test]
    public void WhereColumnNotEqualsExpressionTest()
    {
      TestQuery(() =>
        from o in Query.All<MyEntity>()
        where o.Flag != o.Name.StartsWith("No")
        select o
        );
    }

    [Test]
    public void WhereExpressionEqualsExpressionTest()
    {
      TestQuery(() =>
        from o in Query.All<MyEntity>()
        where o.Name.StartsWith("Yes")==(o.Id==5)
        select o
        );
    }

    [Test]
    public void WhereExpressionNotEqualsExpressionTest()
    {
      TestQuery(() =>
        from o in Query.All<MyEntity>()
        where o.Name.StartsWith("Yes")!=(o.Id < 5)
        select o
        );
    }

    [Test]
    public void WhereParameterTest()
    {
      var parameter = true;
      TestQuery(() =>
        from o in Query.All<MyEntity>()
        where parameter
        select o
        );
    }

    [Test]
    public void WhereNotParameterTest()
    {
      var parameter = true;
      TestQuery(() =>
        from o in Query.All<MyEntity>()
        where !parameter
        select o
        );
    }

    [Test]
    public void WhereParameterOrColumnTest()
    {
      var parameter = true;
      TestQuery(() =>
        from o in Query.All<MyEntity>()
        where parameter || o.Flag.Value
        select o
        );
    }

    [Test]
    public void WhereParameterAndColumnTest()
    {
      var parameter = true;
      TestQuery(() =>
        from o in Query.All<MyEntity>()
        where parameter && o.HasStupidName
        select o
        );
    }

    [Test]
    public void WhereParameterEqualsColumnTest()
    {
      var parameter = true;
      TestQuery(() =>
        from o in Query.All<MyEntity>()
        where parameter==o.Flag
        select o
        );
    }

    [Test]
    public void WhereParameterNotEqualsColumnTest()
    {
      var parameter = true;
      TestQuery(() =>
        from o in Query.All<MyEntity>()
        where parameter!=o.Flag
        select o
        );
    }

    [Test]
    public void WhereParameterOrExpressionTest()
    {
      var parameter = true;
      TestQuery(() =>
        from o in Query.All<MyEntity>()
        where parameter || o.Name.EndsWith("Yes")
        select o
        );
    }

    [Test]
    public void WhereParameterAndExpressionTest()
    {
      var parameter = true;
      TestQuery(() =>
        from o in Query.All<MyEntity>()
        where parameter && o.Name.EndsWith("No")
        select o
        );
    }

    [Test]
    public void WhereParameterEqualsExpressionTest()
    {
      var parameter = true;
      TestQuery(() =>
        from o in Query.All<MyEntity>()
        where parameter==o.Name.EndsWith("Yes")
        select o
        );
    }

    [Test]
    public void WhereParameterNotEqualsExpressionTest()
    {
      var parameter = true;
      TestQuery(() =>
        from o in Query.All<MyEntity>()
        where parameter!=o.Name.StartsWith("No")
        select o
        );
    }

    [Test]
    public void IifTest()
    {
      TestQuery(() =>
        from it in Query.All<MyEntity>()
        where
          (it.Name==null ? null : (bool?) it.Name.StartsWith("Y"))==null
            ? false
            : (it.Name==null ? null : (bool?) (it.Name.StartsWith("Y"))).Value
        select it
        );
    }

    [Test]
    public void CoalesceTest()
    {
      TestQuery(() =>
        from it in Query.All<MyEntity>()
        select it.Flag ?? it.HasStupidName
        );
    }

    [Test]
    public void ComplexTest()
    {
      var parameter1 = true;
      var parameter2 = false;
      TestQuery(() =>
        from o in Query.All<MyEntity>()
        where (!o.Flag.Value && o.Name.StartsWith("No") || parameter1) && (o.HasStupidName || !parameter2)
        select new {value = o.Id % 10 < 5, flag = o.Flag, notflag = !o.Flag}
        );
    }

    [Test]
    public void ConfusingParameterTest()
    {
      var parameter = new Parameter<Tuple>();
      using (new ParameterContext().Activate()) {
        parameter.Value = Tuple.Create(false);
        TestQuery(() =>
          from o in Query.All<MyEntity>()
          where o.HasStupidName==parameter.Value.GetValueOrDefault<bool>(0)
          select o
          );
      }
    }

    [Test]
    public void CastToObjectAndBackTest()
    {
      TestQuery(() => Query.All<MyEntity>().Select(e => (object)e.Flag).Where(f => (bool)f));
    }

    private void TestQuery<T>(Func<IQueryable<T>> query)
    {
      using (Session.Open(Domain)) {
        using (Transaction.Open()) {
          query.Invoke().ToList();
        }
      }
    }
  }
}