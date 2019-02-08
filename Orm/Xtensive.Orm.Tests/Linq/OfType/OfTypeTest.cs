// Copyright (C) 2019 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Kudelin
// Created:    2019.01.31

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Building;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Tests.Linq.OfType
{
  using Models;

  public class OfTypeTest : AutoBuildTest
  {
    [Test]
    public void Test01()
    {
      using (var sto = OpenSessionTransaction()) {
        var result = sto.Query.All<AClassTable>().OfType<IB>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();
        Assert.That(result[0], Is.EqualTo("100000String1"));
        Assert.That(result[1], Is.EqualTo("200000String2"));
        Assert.That(result[2], Is.EqualTo("300000String3"));
      }


      using (var sto = OpenSessionTransaction()) {
        var result = sto.Query.All<AConcreteTable>().OfType<IB>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();
        Assert.That(result[0], Is.EqualTo("100000String1"));
        Assert.That(result[1], Is.EqualTo("200000String2"));
        Assert.That(result[2], Is.EqualTo("300000String3"));
      }


      using (var sto = OpenSessionTransaction()) {
        var result = sto.Query.All<ASingleTable>().OfType<IB>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();
        Assert.That(result[0], Is.EqualTo("100000String1"));
        Assert.That(result[1], Is.EqualTo("200000String2"));
        Assert.That(result[2], Is.EqualTo("300000String3"));
      }
    }

    [Test]
    public void Test02()
    {
      using (var sto = OpenSessionTransaction()) {
        var result1 = sto.Query.All<B2ClassTable>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();

        var result2 = sto.Query.All<AClassTable>().OfType<B2ClassTable>()
          .OfType<IB>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();

        Assert.That(result1, Is.Not.Empty);
        Assert.That(result1.SequenceEqual(result2));
      }


      using (var sto = OpenSessionTransaction()) {
        var result1 = sto.Query.All<B2ConcreteTable>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();

        var result2 = sto.Query.All<AConcreteTable>().OfType<B2ConcreteTable>()
          .OfType<IB>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();

        Assert.That(result1, Is.Not.Empty);
        Assert.That(result1.SequenceEqual(result2));
      }


      using (var sto = OpenSessionTransaction()) {
        var result1 = sto.Query.All<B2SingleTable>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();

        var result2 = sto.Query.All<ASingleTable>().OfType<B2SingleTable>()
          .OfType<IB>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();

        Assert.That(result1, Is.Not.Empty);
        Assert.That(result1.SequenceEqual(result2));
      }
    }

    [Test]
    public void Test03()
    {
      using (var sto = OpenSessionTransaction()) {
        var result1 = sto.Query.All<B1ClassTable>().OfType<IB>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();
        Assert.That(result1.First(), Is.EqualTo("100000String1"));
        Assert.That(result1.Last(), Is.EqualTo("200000String2"));

        var result2 = sto.Query.All<B2ClassTable>().OfType<IB>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();
        Assert.That(result2.Single(), Is.EqualTo("300000String3"));
      }


      using (var sto = OpenSessionTransaction()) {
        var result1 = sto.Query.All<B1ConcreteTable>().OfType<IB>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();
        Assert.That(result1.First(), Is.EqualTo("100000String1"));
        Assert.That(result1.Last(), Is.EqualTo("200000String2"));

        var result2 = sto.Query.All<B2ConcreteTable>().OfType<IB>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();
        Assert.That(result2.Single(), Is.EqualTo("300000String3"));
      }


      using (var sto = OpenSessionTransaction()) {
        var result1 = sto.Query.All<B1SingleTable>().OfType<IB>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();
        Assert.That(result1.First(), Is.EqualTo("100000String1"));
        Assert.That(result1.Last(), Is.EqualTo("200000String2"));

        var result2 = sto.Query.All<B2SingleTable>().OfType<IB>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();
        Assert.That(result2.Single(), Is.EqualTo("300000String3"));
      }
    }

    [Test]
    public void Test04()
    {
      using (var sto = OpenSessionTransaction()) {
        var result1 = sto.Query.All<B1ClassTable>().OfType<AClassTable>()
          .Select(x => x.Field2).ToArray();
        Assert.That(result1.First(), Is.EqualTo(1000));
        Assert.That(result1.Last(), Is.EqualTo(2000));

        var result2 = sto.Query.All<B2ClassTable>().OfType<AClassTable>()
          .Select(x => x.Field2).ToArray();
        Assert.That(result2.Single(), Is.EqualTo(3000));
      }


      using (var sto = OpenSessionTransaction()) {
        var result1 = sto.Query.All<B1ConcreteTable>().OfType<AConcreteTable>()
          .Select(x => x.Field2).ToArray();
        Assert.That(result1.First(), Is.EqualTo(1000));
        Assert.That(result1.Last(), Is.EqualTo(2000));

        var result2 = sto.Query.All<B2ConcreteTable>().OfType<AConcreteTable>()
          .Select(x => x.Field2).ToArray();
        Assert.That(result2.Single(), Is.EqualTo(3000));
      }


      using (var sto = OpenSessionTransaction()) {
        var result1 = sto.Query.All<B1SingleTable>().OfType<ASingleTable>()
          .Select(x => x.Field2).ToArray();
        Assert.That(result1.First(), Is.EqualTo(1000));
        Assert.That(result1.Last(), Is.EqualTo(2000));

        var result2 = sto.Query.All<B2SingleTable>().OfType<ASingleTable>()
          .Select(x => x.Field2).ToArray();
        Assert.That(result2.Single(), Is.EqualTo(3000));
      }
    }

    [Test]
    public void Test05()
    {
      using (var sto = OpenSessionTransaction()) {
        var result = sto.Query.All<B1ClassTable>().OfType<B2ClassTable>()
          .Select(x => x.Field2).ToArray();
        Assert.That(result, Is.Empty);
      }

      using (var sto = OpenSessionTransaction()) {
        var result = sto.Query.All<B1ConcreteTable>().OfType<B2ConcreteTable>()
          .Select(x => x.Field2).ToArray();
        Assert.That(result, Is.Empty);
      }

      using (var sto = OpenSessionTransaction()) {
        var result = sto.Query.All<B1SingleTable>().OfType<B2SingleTable>()
          .Select(x => x.Field2).ToArray();
        Assert.That(result, Is.Empty);
      }
    }

    [Test]
    public void Test06()
    {
      using (var sto = OpenSessionTransaction()) {
        var result = sto.Query.All<B1ClassTable>().OfType<B3ClassTable>()
          .Select(x => x.Field3).ToArray();
        Assert.That(result, Is.Empty);
      }

      using (var sto = OpenSessionTransaction()) {
        var result = sto.Query.All<B1ConcreteTable>().OfType<B3ConcreteTable>()
          .Select(x => x.Field3).ToArray();
        Assert.That(result, Is.Empty);
      }

      using (var sto = OpenSessionTransaction()) {
        var result = sto.Query.All<B1SingleTable>().OfType<B3SingleTable>()
          .Select(x => x.Field3).ToArray();
        Assert.That(result, Is.Empty);
      }
    }

    [Test]
    public void Test07()
    {
      using (var sto = OpenSessionTransaction()) {
        var source = sto.Query.All<AClassTable>().OfType<IB>();

        var result1 = source.Select(x => x.Field3.Value1).ToArray();
        Assert.That(result1[0], Is.EqualTo(10000));
        Assert.That(result1[1], Is.EqualTo(20000));
        Assert.That(result1[2], Is.EqualTo(30000));
        Assert.That(result1.Length, Is.EqualTo(3));

        var result2 = source.OfType<B3ClassTable>().Select(x => x.Field3.Value1).ToArray();
        Assert.That(result2, Is.Empty);
      }

      using (var sto = OpenSessionTransaction()) {
        var source = sto.Query.All<AConcreteTable>().OfType<IB>();

        var result1 = source.Select(x => x.Field3.Value1).ToArray();
        Assert.That(result1[0], Is.EqualTo(10000));
        Assert.That(result1[1], Is.EqualTo(20000));
        Assert.That(result1[2], Is.EqualTo(30000));
        Assert.That(result1.Length, Is.EqualTo(3));

        var result2 = source.OfType<B3ConcreteTable>().Select(x => x.Field3.Value1).ToArray();
        Assert.That(result2, Is.Empty);
      }

      using (var sto = OpenSessionTransaction()) {
        var source = sto.Query.All<ASingleTable>().OfType<IB>();
        var result1 = source.Select(x => x.Field3.Value1).ToArray();
        Assert.That(result1[0], Is.EqualTo(10000));
        Assert.That(result1[1], Is.EqualTo(20000));
        Assert.That(result1[2], Is.EqualTo(30000));
        Assert.That(result1.Length, Is.EqualTo(3));

        var result2 = source.OfType<B3SingleTable>().Select(x => x.Field3.Value1).ToArray();
        Assert.That(result2, Is.Empty);
      }
    }

    [Test]
    public void Test08()
    {
      using (var sto = OpenSessionTransaction()) {
        var source = sto.Query.All<AClassTable>().OfType<IB>();
        var result1 = source.ToArray().OfType<IC1>().Select(x => x.Field5).ToArray();
        var result2 = source.OfType<IC1>().Select(x => x.Field5).ToArray();
        Assert.That(result1, Is.Not.Empty);
        Assert.That(result1.SequenceEqual(result2));
      }

      using (var sto = OpenSessionTransaction()) {
        var source = sto.Query.All<AConcreteTable>().OfType<IB>();
        var result1 = source.ToArray().OfType<IC1>().Select(x => x.Field5).ToArray();
        var result2 = source.OfType<IC1>().Select(x => x.Field5).ToArray();
        Assert.That(result1, Is.Not.Empty);
        Assert.That(result1.SequenceEqual(result2));
      }

      using (var sto = OpenSessionTransaction()) {
        var source = sto.Query.All<ASingleTable>().OfType<IB>();
        var result1 = source.ToArray().OfType<IC1>().Select(x => x.Field5).ToArray();
        var result2 = source.OfType<IC1>().Select(x => x.Field5).ToArray();
        Assert.That(result1, Is.Not.Empty);
        Assert.That(result1.SequenceEqual(result2));
      }
    }

    [Test]
    public void Test09()
    {
      using (var sto = OpenSessionTransaction()) {
        var source = sto.Query.All<AClassTable>().OfType<IB>();
        var result1 = source.ToArray().OfType<IBaseInterface>().Select(x => x.BaseField).ToArray();
        var result2 = source.OfType<IBaseInterface>().Select(x => x.BaseField).ToArray();
        Assert.That(result1, Is.Not.Empty);
        Assert.That(result1.SequenceEqual(result2));
      }

      using (var sto = OpenSessionTransaction()) {
        var source = sto.Query.All<AConcreteTable>().OfType<IB>();
        var result1 = source.ToArray().OfType<IBaseInterface>().Select(x => x.BaseField).ToArray();
        var result2 = source.OfType<IBaseInterface>().Select(x => x.BaseField).ToArray();
        Assert.That(result1, Is.Not.Empty);
        Assert.That(result1.SequenceEqual(result2));
      }

      using (var sto = OpenSessionTransaction()) {
        var source = sto.Query.All<ASingleTable>().OfType<IB>();
        var result1 = source.ToArray().OfType<IBaseInterface>().Select(x => x.BaseField).ToArray();
        var result2 = source.OfType<IBaseInterface>().Select(x => x.BaseField).ToArray();
        Assert.That(result1, Is.Not.Empty);
        Assert.That(result1.SequenceEqual(result2));
      }
    }

    [Test]
    public void Test10()
    {
      using (var sto = OpenSessionTransaction()) {
        var exception = Assert.Throws<QueryTranslationException>(
          () => sto.Query.All<AClassTable>().OfType<IB>()
            .OfType<BaseEntity>().Run());
        Assert.That(exception.InnerException, Is.AssignableFrom(typeof (NotSupportedException)));
      }

      using (var sto = OpenSessionTransaction()) {
        var exception = Assert.Throws<QueryTranslationException>(
          () => sto.Query.All<AConcreteTable>().OfType<IB>()
            .OfType<BaseEntity>().Run());
        Assert.That(exception.InnerException, Is.AssignableFrom(typeof (NotSupportedException)));
      }

      using (var sto = OpenSessionTransaction()) {
        var exception = Assert.Throws<QueryTranslationException>(
          () => sto.Query.All<ASingleTable>().OfType<IB>()
            .OfType<BaseEntity>().Run());
        Assert.That(exception.InnerException, Is.AssignableFrom(typeof (NotSupportedException)));
      }
    }

    [Test]
    public void Test11()
    {
      using (var sto = OpenSessionTransaction()) {
        var query = sto.Query.All<AClassTable>().OfType<IB>()
          .OfType<IBaseEntity>();

        var result1 = query.Select(x => x.Field1).ToArray();
        Assert.That(result1[0], Is.EqualTo(10));
        Assert.That(result1[1], Is.EqualTo(20));
        Assert.That(result1[2], Is.EqualTo(30));
        Assert.That(result1.Length, Is.EqualTo(3));

        var result2 = query.OfType<AClassTable>().Select(x => x.Field1).ToArray();
        Assert.That(result2[0], Is.EqualTo(10));
        Assert.That(result2[1], Is.EqualTo(20));
        Assert.That(result2[2], Is.EqualTo(30));
        Assert.That(result2.Length, Is.EqualTo(3));

        var result3 = query.OfType<AClassTable>().OfType<IBaseEntity>()
          .OfType<C1ClassTable>().Select(x => x.Field1).ToArray();
        Assert.That(result3[0], Is.EqualTo(10));
        Assert.That(result3[1], Is.EqualTo(20));
        Assert.That(result3.Length, Is.EqualTo(2));
      }

      using (var sto = OpenSessionTransaction()) {
        var query = sto.Query.All<AConcreteTable>().OfType<IB>()
          .OfType<IBaseEntity>();

        var result1 = query.Select(x => x.Field1).ToArray();
        Assert.That(result1[0], Is.EqualTo(10));
        Assert.That(result1[1], Is.EqualTo(20));
        Assert.That(result1[2], Is.EqualTo(30));
        Assert.That(result1.Length, Is.EqualTo(3));

        var result2 = query.OfType<AConcreteTable>().Select(x => x.Field1).ToArray();
        Assert.That(result2[0], Is.EqualTo(10));
        Assert.That(result2[1], Is.EqualTo(20));
        Assert.That(result2[2], Is.EqualTo(30));
        Assert.That(result2.Length, Is.EqualTo(3));

        var result3 = query.OfType<AConcreteTable>().OfType<IBaseEntity>()
          .OfType<C1ConcreteTable>().Select(x => x.Field1).ToArray();
        Assert.That(result3[0], Is.EqualTo(10));
        Assert.That(result3[1], Is.EqualTo(20));
        Assert.That(result3.Length, Is.EqualTo(2));
      }

      using (var sto = OpenSessionTransaction()) {
        var query = sto.Query.All<ASingleTable>().OfType<IB>()
          .OfType<IBaseEntity>();

        var result1 = query.Select(x => x.Field1).ToArray();
        Assert.That(result1[0], Is.EqualTo(10));
        Assert.That(result1[1], Is.EqualTo(20));
        Assert.That(result1[2], Is.EqualTo(30));
        Assert.That(result1.Length, Is.EqualTo(3));

        var result2 = query.OfType<ASingleTable>().Select(x => x.Field1).ToArray();
        Assert.That(result2[0], Is.EqualTo(10));
        Assert.That(result2[1], Is.EqualTo(20));
        Assert.That(result2[2], Is.EqualTo(30));
        Assert.That(result2.Length, Is.EqualTo(3));

        var result3 = query.OfType<ASingleTable>().OfType<IBaseEntity>()
          .OfType<C1SingleTable>().Select(x => x.Field1).ToArray();
        Assert.That(result3[0], Is.EqualTo(10));
        Assert.That(result3[1], Is.EqualTo(20));
        Assert.That(result3.Length, Is.EqualTo(2));
      }
    }

    [Test]
    public void Test12()
    {
      using (var sto = OpenSessionTransaction()) {

        var source = sto.Query.All<AClassTable>().OfType<IC1>()
          .SelectMany(x => x.Field6);

        var result1 = source.OfType<IC1>().Select(x => x.Field5).ToArray();
        Assert.That(result1.Single(), Is.EqualTo(3.5));

        var result2 = source.OfType<IB>().Select(x => x.Field3.Value1).ToArray();
        Assert.That(result2.Single(), Is.EqualTo(20000));

        var result3 = source.OfType<IBaseInterface>().Select(x => x.BaseField).ToArray();
        Assert.That(result3.Single(), Is.EqualTo(2));
      }

      using (var sto = OpenSessionTransaction()) {

        var source = sto.Query.All<AConcreteTable>().OfType<IC1>()
          .SelectMany(x => x.Field6);

        var result1 = source.OfType<IC1>().Select(x => x.Field5).ToArray();
        Assert.That(result1.Single(), Is.EqualTo(3.5));

        var result2 = source.OfType<IB>().Select(x => x.Field3.Value1).ToArray();
        Assert.That(result2.Single(), Is.EqualTo(20000));

        var result3 = source.OfType<IBaseInterface>().Select(x => x.BaseField).ToArray();
        Assert.That(result3.Single(), Is.EqualTo(2));
      }

      using (var sto = OpenSessionTransaction()) {

        var source = sto.Query.All<ASingleTable>().OfType<IC1>()
          .SelectMany(x => x.Field6);

        var result1 = source.OfType<IC1>().Select(x => x.Field5).ToArray();
        Assert.That(result1.Single(), Is.EqualTo(3.5));

        var result2 = source.OfType<IB>().Select(x => x.Field3.Value1).ToArray();
        Assert.That(result2.Single(), Is.EqualTo(20000));

        var result3 = source.OfType<IBaseInterface>().Select(x => x.BaseField).ToArray();
        Assert.That(result3.Single(), Is.EqualTo(2));
      }
    }

    [Test]
    public void Test13()
    {
      using (var sto = OpenSessionTransaction()) {
        var source = sto.Query.All<AClassTable>();

        var result1A = source.ToArray().OfType<IC1>().OfType<IBaseEntity>();
        var result1B = source.OfType<IC1>().OfType<IBaseEntity>();

        var result2A = source.ToArray().OfType<IB3ClassTable>().OfType<IBaseEntity>();
        var result2B = source.OfType<IB3ClassTable>().OfType<IBaseEntity>();

        var resultA = result1A.Union(result2A).Select(x => x.Field1).ToArray();
        var resultB = result1B.Union(result2B).Select(x => x.Field1).ToArray();

        Assert.That(resultA, Is.Not.Empty);
        Assert.That(resultA.SequenceEqual(resultB));
      }

      using (var sto = OpenSessionTransaction()) {
        var source = sto.Query.All<AConcreteTable>();

        var result1A = source.ToArray().OfType<IC1>().OfType<IBaseEntity>();
        var result1B = source.OfType<IC1>().OfType<IBaseEntity>();

        var result2A = source.ToArray().OfType<IB3ConcreteTable>().OfType<IBaseEntity>();
        var result2B = source.OfType<IB3ConcreteTable>().OfType<IBaseEntity>();

        var resultA = result1A.Union(result2A).Select(x => x.Field1).ToArray();
        var resultB = result1B.Union(result2B).Select(x => x.Field1).ToArray();

        Assert.That(resultA, Is.Not.Empty);
        Assert.That(resultA.SequenceEqual(resultB));
      }

      using (var sto = OpenSessionTransaction()) {
        var source = sto.Query.All<ASingleTable>();

        var result1A = source.ToArray().OfType<IC1>().OfType<IBaseEntity>();
        var result1B = source.OfType<IC1>().OfType<IBaseEntity>();

        var result2A = source.ToArray().OfType<IB3SingleTable>().OfType<IBaseEntity>();
        var result2B = source.OfType<IB3SingleTable>().OfType<IBaseEntity>();

        var resultA = result1A.Union(result2A).Select(x => x.Field1).ToArray();
        var resultB = result1B.Union(result2B).Select(x => x.Field1).ToArray();

        Assert.That(resultA, Is.Not.Empty);
        Assert.That(resultA.SequenceEqual(resultB));
      }
    }

    [Test]
    public void Test14()
    {
      using (var sto = OpenSessionTransaction()) {
        var source1A = sto.Query.All<AClassTable>().ToArray().OfType<IBaseEntity>();
        var source1B = sto.Query.All<AClassTable>().OfType<IBaseEntity>();

        var source2A = sto.Query.All<AConcreteTable>().ToArray().OfType<IBaseEntity>();
        var source2B = sto.Query.All<AConcreteTable>().OfType<IBaseEntity>();

        var result1A = source1A.Union(source2A).Select(x => x.Field1).ToArray();
        var result1B = source2B.Union(source1B).Select(x => x.Field1).ToArray();

        Assert.That(result1A, Is.Not.Empty);
        Assert.That(result1A.SequenceEqual(result1B));


        var result2A = source1B.Union(source2B).ToArray().OfType<B3ClassTable>().Select(x => x.Field4)
          .ToArray();
        var result2B = source1B.Union(source2B).OfType<B3ClassTable>().Select(x => x.Field4).ToArray();

        Assert.That(result2A, Is.Not.Empty);
        Assert.That(result2A.SequenceEqual(result2B));

        var result3 = sto.Query.All<AClassTable>().OfType<AConcreteTable>().ToArray();
        Assert.That(result3, Is.Empty);

        var result4 = sto.Query.All<AConcreteTable>().OfType<IB3ClassTable>().ToArray();
        Assert.That(result4, Is.Empty);
      }

      using (var sto = OpenSessionTransaction()) {
        var source1A = sto.Query.All<AConcreteTable>().ToArray().OfType<IBaseEntity>();
        var source1B = sto.Query.All<AConcreteTable>().OfType<IBaseEntity>();

        var source2A = sto.Query.All<ASingleTable>().ToArray().OfType<IBaseEntity>();
        var source2B = sto.Query.All<ASingleTable>().OfType<IBaseEntity>();

        var result1A = source1A.Union(source2A).Select(x => x.Field1).ToArray();
        var result1B = source2B.Union(source1B).Select(x => x.Field1).ToArray();

        Assert.That(result1A, Is.Not.Empty);
        Assert.That(result1A.SequenceEqual(result1B));


        var result2A = source1B.Union(source2B).ToArray().OfType<B3ConcreteTable>().Select(x => x.Field4)
          .ToArray();
        var result2B = source1B.Union(source2B).OfType<B3ConcreteTable>().Select(x => x.Field4).ToArray();

        Assert.That(result2A, Is.Not.Empty);
        Assert.That(result2A.SequenceEqual(result2B));

        var result3 = sto.Query.All<AConcreteTable>().OfType<ASingleTable>().ToArray();
        Assert.That(result3, Is.Empty);

        var result4 = sto.Query.All<ASingleTable>().OfType<IB3ConcreteTable>().ToArray();
        Assert.That(result4, Is.Empty);
      }

      using (var sto = OpenSessionTransaction()) {
        var source1A = sto.Query.All<ASingleTable>().ToArray().OfType<IBaseEntity>();
        var source1B = sto.Query.All<ASingleTable>().OfType<IBaseEntity>();

        var source2A = sto.Query.All<AClassTable>().ToArray().OfType<IBaseEntity>();
        var source2B = sto.Query.All<AClassTable>().OfType<IBaseEntity>();

        var result1A = source1A.Union(source2A).Select(x => x.Field1).ToArray();
        var result1B = source2B.Union(source1B).Select(x => x.Field1).ToArray();

        Assert.That(result1A, Is.Not.Empty);
        Assert.That(result1A.SequenceEqual(result1B));


        var result2A = source1B.Union(source2B).ToArray().OfType<B3SingleTable>().Select(x => x.Field4)
          .ToArray();
        var result2B = source1B.Union(source2B).OfType<B3SingleTable>().Select(x => x.Field4).ToArray();

        Assert.That(result2A, Is.Not.Empty);
        Assert.That(result2A.SequenceEqual(result2B));

        var result3 = sto.Query.All<ASingleTable>().OfType<AClassTable>().ToArray();
        Assert.That(result3, Is.Empty);

        var result4 = sto.Query.All<AClassTable>().OfType<IB3SingleTable>().ToArray();
        Assert.That(result4, Is.Empty);
      }
    }

    protected override void PopulateData()
    {
      using (var sto = OpenSessionTransaction()) {
        PopulateData<C1ClassTable, B2ClassTable, B3ClassTable>();
        PopulateData<C1ConcreteTable, B2ConcreteTable, B3ConcreteTable>();
        PopulateData<C1SingleTable, B2SingleTable, B3SingleTable>();
        sto.TransactionScope.Complete();
      }
    }

    private void PopulateData<C1, B2, B3>()
      where C1 : IC1, IB, IBaseEntity, IA, new()
      where B2 : IB, IBaseEntity, IA, new()
      where B3 : IB3, IBaseEntity, IA, new()
    {
      new C1() {
        BaseField = 1,
        Field1 = 10,
        Field2 = 1000,
        Field3 = new Structure1 {
          Value1 = 10000,
          Value2 = new Structure2 {
            Value1 = 100000,
            Value2 = DateTime.FromBinary(10)
          },
          Value3 = "StructureString1"
        },
        Field4 = "String1",
        Field5 = 2.5,
      }.Field6.Add(
        new C1 {
          BaseField = 2,
          Field1 = 20,
          Field2 = 2000,
          Field3 = new Structure1 {
            Value1 = 20000,
            Value2 = new Structure2 {
              Value1 = 200000,
              Value2 = DateTime.FromBinary(1000)
            },
            Value3 = "StructureString2"
          },
          Field4 = "String2",
          Field5 = 3.5,
        });
      new B2() {
        BaseField = 3,
        Field1 = 30,
        Field2 = 3000,
        Field3 = new Structure1 {
          Value1 = 30000,
          Value2 = new Structure2 {
            Value1 = 300000,
            Value2 = DateTime.FromBinary(100000)
          },
          Value3 = "StructureString3"
        },
        Field4 = "String3"
      };
      new B3() {
        BaseField = 4,
        Field1 = 40,
        Field2 = 4000,
        Field3 = new Structure1 {
          Value1 = 40000,
          Value2 = new Structure2 {
            Value1 = 400000,
            Value2 = DateTime.FromBinary(400000)
          },
          Value3 = "StructureString4"
        },
        Field4 = "String4"
      };
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (ASingleTable).Assembly, typeof (ASingleTable).Namespace);
      return config;
    }
  }
}