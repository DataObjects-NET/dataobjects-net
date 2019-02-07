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

namespace Xtensive.Orm.Tests.Linq.OfTypeTranslation
{
  using Xtensive.Orm.Tests.Linq.OfTypeTranslation.Models;

  public class OfTypeTranslationTest : AutoBuildTest
  {
    [Test]
    public void Test01()
    {
      using (var sto = OpenSessionTransaction()) {
        var result = sto.Query.All<TestEntity1ClassTable>().OfType<ITestEntity2ClassTable>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();
        Assert.That(result[0], Is.EqualTo("100000String1"));
        Assert.That(result[1], Is.EqualTo("200000String2"));
        Assert.That(result[2], Is.EqualTo("300000String3"));
      }


      using (var sto = OpenSessionTransaction()) {
        var result = sto.Query.All<TestEntity1ConcreteTable>().OfType<ITestEntity2ConcreteTable>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();
        Assert.That(result[0], Is.EqualTo("100000String1"));
        Assert.That(result[1], Is.EqualTo("200000String2"));
        Assert.That(result[2], Is.EqualTo("300000String3"));
      }


      using (var sto = OpenSessionTransaction()) {
        var result = sto.Query.All<TestEntity1SingleTable>().OfType<ITestEntity2SingleTable>()
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
        var result1 = sto.Query.All<TestEntity2bClassTable>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();

        var result2 = sto.Query.All<TestEntity1ClassTable>().OfType<TestEntity2bClassTable>()
          .OfType<ITestEntity2ClassTable>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();

        Assert.That(result1, Is.Not.Empty);
        Assert.That(result1.SequenceEqual(result2));
      }


      using (var sto = OpenSessionTransaction()) {
        var result1 = sto.Query.All<TestEntity2bConcreteTable>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();

        var result2 = sto.Query.All<TestEntity1ConcreteTable>().OfType<TestEntity2bConcreteTable>()
          .OfType<ITestEntity2ConcreteTable>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();

        Assert.That(result1, Is.Not.Empty);
        Assert.That(result1.SequenceEqual(result2));
      }


      using (var sto = OpenSessionTransaction()) {
        var result1 = sto.Query.All<TestEntity2bSingleTable>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();

        var result2 = sto.Query.All<TestEntity1SingleTable>().OfType<TestEntity2bSingleTable>()
          .OfType<ITestEntity2SingleTable>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();

        Assert.That(result1, Is.Not.Empty);
        Assert.That(result1.SequenceEqual(result2));
      }
    }

    [Test]
    public void Test03()
    {
      using (var sto = OpenSessionTransaction()) {
        var result1 = sto.Query.All<TestEntity2aClassTable>().OfType<ITestEntity2ClassTable>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();
        Assert.That(result1.First(), Is.EqualTo("100000String1"));
        Assert.That(result1.Last(), Is.EqualTo("200000String2"));

        var result2 = sto.Query.All<TestEntity2bClassTable>().OfType<ITestEntity2ClassTable>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();
        Assert.That(result2.Single(), Is.EqualTo("300000String3"));
      }


      using (var sto = OpenSessionTransaction()) {
        var result1 = sto.Query.All<TestEntity2aConcreteTable>().OfType<ITestEntity2ConcreteTable>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();
        Assert.That(result1.First(), Is.EqualTo("100000String1"));
        Assert.That(result1.Last(), Is.EqualTo("200000String2"));

        var result2 = sto.Query.All<TestEntity2bConcreteTable>().OfType<ITestEntity2ConcreteTable>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();
        Assert.That(result2.Single(), Is.EqualTo("300000String3"));
      }


      using (var sto = OpenSessionTransaction()) {
        var result1 = sto.Query.All<TestEntity2aSingleTable>().OfType<ITestEntity2SingleTable>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();
        Assert.That(result1.First(), Is.EqualTo("100000String1"));
        Assert.That(result1.Last(), Is.EqualTo("200000String2"));

        var result2 = sto.Query.All<TestEntity2bSingleTable>().OfType<ITestEntity2SingleTable>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();
        Assert.That(result2.Single(), Is.EqualTo("300000String3"));
      }
    }

    [Test]
    public void Test04()
    {
      using (var sto = OpenSessionTransaction()) {
        var result1 = sto.Query.All<TestEntity2aClassTable>().OfType<TestEntity1ClassTable>()
          .Select(x => x.Field2).ToArray();
        Assert.That(result1.First(), Is.EqualTo(1000));
        Assert.That(result1.Last(), Is.EqualTo(2000));

        var result2 = sto.Query.All<TestEntity2bClassTable>().OfType<TestEntity1ClassTable>()
          .Select(x => x.Field2).ToArray();
        Assert.That(result2.Single(), Is.EqualTo(3000));
      }


      using (var sto = OpenSessionTransaction()) {
        var result1 = sto.Query.All<TestEntity2aConcreteTable>().OfType<TestEntity1ConcreteTable>()
          .Select(x => x.Field2).ToArray();
        Assert.That(result1.First(), Is.EqualTo(1000));
        Assert.That(result1.Last(), Is.EqualTo(2000));

        var result2 = sto.Query.All<TestEntity2bConcreteTable>().OfType<TestEntity1ConcreteTable>()
          .Select(x => x.Field2).ToArray();
        Assert.That(result2.Single(), Is.EqualTo(3000));
      }


      using (var sto = OpenSessionTransaction()) {
        var result1 = sto.Query.All<TestEntity2aSingleTable>().OfType<TestEntity1SingleTable>()
          .Select(x => x.Field2).ToArray();
        Assert.That(result1.First(), Is.EqualTo(1000));
        Assert.That(result1.Last(), Is.EqualTo(2000));

        var result2 = sto.Query.All<TestEntity2bSingleTable>().OfType<TestEntity1SingleTable>()
          .Select(x => x.Field2).ToArray();
        Assert.That(result2.Single(), Is.EqualTo(3000));
      }
    }

    [Test]
    public void Test05()
    {
      using (var sto = OpenSessionTransaction()) {
        var result = sto.Query.All<TestEntity2aClassTable>().OfType<TestEntity2bClassTable>()
          .Select(x => x.Field2).ToArray();
        Assert.That(result, Is.Empty);
      }

      using (var sto = OpenSessionTransaction()) {
        var result = sto.Query.All<TestEntity2aConcreteTable>().OfType<TestEntity2bConcreteTable>()
          .Select(x => x.Field2).ToArray();
        Assert.That(result, Is.Empty);
      }

      using (var sto = OpenSessionTransaction()) {
        var result = sto.Query.All<TestEntity2aSingleTable>().OfType<TestEntity2bSingleTable>()
          .Select(x => x.Field2).ToArray();
        Assert.That(result, Is.Empty);
      }
    }

    [Test]
    public void Test06()
    {
      using (var sto = OpenSessionTransaction()) {
        var result = sto.Query.All<TestEntity2aClassTable>().OfType<ITestEntity2cClassTable>()
          .Select(x => x.Field3).ToArray();
        Assert.That(result, Is.Empty);
      }

      using (var sto = OpenSessionTransaction()) {
        var result = sto.Query.All<TestEntity2aConcreteTable>().OfType<ITestEntity2cConcreteTable>()
          .Select(x => x.Field3).ToArray();
        Assert.That(result, Is.Empty);
      }

      using (var sto = OpenSessionTransaction()) {
        var result = sto.Query.All<TestEntity2aSingleTable>().OfType<ITestEntity2cSingleTable>()
          .Select(x => x.Field3).ToArray();
        Assert.That(result, Is.Empty);
      }
    }

    [Test]
    public void Test07()
    {
      using (var sto = OpenSessionTransaction()) {
        var source = sto.Query.All<TestEntity1ClassTable>().OfType<ITestEntity2ClassTable>();

        var result1 = source.Select(x => x.Field3.Value1).ToArray();
        Assert.That(result1[0], Is.EqualTo(10000));
        Assert.That(result1[1], Is.EqualTo(20000));
        Assert.That(result1[2], Is.EqualTo(30000));
        Assert.That(result1.Length, Is.EqualTo(3));

        var result2 = source.OfType<ITestEntity2cClassTable>().Select(x => x.Field3.Value1).ToArray();
        Assert.That(result2, Is.Empty);
      }

      using (var sto = OpenSessionTransaction()) {
        var source = sto.Query.All<TestEntity1ConcreteTable>().OfType<ITestEntity2ConcreteTable>();

        var result1 = source.Select(x => x.Field3.Value1).ToArray();
        Assert.That(result1[0], Is.EqualTo(10000));
        Assert.That(result1[1], Is.EqualTo(20000));
        Assert.That(result1[2], Is.EqualTo(30000));
        Assert.That(result1.Length, Is.EqualTo(3));

        var result2 = source.OfType<ITestEntity2cConcreteTable>().Select(x => x.Field3.Value1).ToArray();
        Assert.That(result2, Is.Empty);
      }

      using (var sto = OpenSessionTransaction()) {
        var source = sto.Query.All<TestEntity1SingleTable>().OfType<ITestEntity2SingleTable>();

        var result1 = source.Select(x => x.Field3.Value1).ToArray();
        Assert.That(result1[0], Is.EqualTo(10000));
        Assert.That(result1[1], Is.EqualTo(20000));
        Assert.That(result1[2], Is.EqualTo(30000));
        Assert.That(result1.Length, Is.EqualTo(3));

        var result2 = source.OfType<ITestEntity2cSingleTable>().Select(x => x.Field3.Value1).ToArray();
        Assert.That(result2, Is.Empty);
      }
    }

    [Test]
    public void Test08()
    {
      using (var sto = OpenSessionTransaction()) {
        var source = sto.Query.All<TestEntity1ClassTable>().OfType<ITestEntity2ClassTable>();
        var result1 = source.ToArray().OfType<ITestEntity3aClassTable>().Select(x => x.Field5).ToArray();
        var result2 = source.OfType<ITestEntity3aClassTable>().Select(x => x.Field5).ToArray();
        Assert.That(result1, Is.Not.Empty);
        Assert.That(result1.SequenceEqual(result2));
      }

      using (var sto = OpenSessionTransaction()) {
        var source = sto.Query.All<TestEntity1ConcreteTable>().OfType<ITestEntity2ConcreteTable>();
        var result1 = source.ToArray().OfType<ITestEntity3aConcreteTable>().Select(x => x.Field5).ToArray();
        var result2 = source.OfType<ITestEntity3aConcreteTable>().Select(x => x.Field5).ToArray();
        Assert.That(result1, Is.Not.Empty);
        Assert.That(result1.SequenceEqual(result2));
      }

      using (var sto = OpenSessionTransaction()) {
        var source = sto.Query.All<TestEntity1SingleTable>().OfType<ITestEntity2SingleTable>();
        var result1 = source.ToArray().OfType<ITestEntity3aSingleTable>().Select(x => x.Field5).ToArray();
        var result2 = source.OfType<ITestEntity3aSingleTable>().Select(x => x.Field5).ToArray();
        Assert.That(result1, Is.Not.Empty);
        Assert.That(result1.SequenceEqual(result2));
      }
    }

    [Test]
    public void Test09()
    {
      using (var sto = OpenSessionTransaction()) {
        var source = sto.Query.All<TestEntity1ClassTable>().OfType<ITestEntity2ClassTable>();
        var result1 = source.ToArray().OfType<ITestEntityBaseClassTable>().Select(x => x.BaseField).ToArray();
        var result2 = source.OfType<ITestEntityBaseClassTable>().Select(x => x.BaseField).ToArray();
        Assert.That(result1, Is.Not.Empty);
        Assert.That(result1.SequenceEqual(result2));
      }

      using (var sto = OpenSessionTransaction()) {
        var source = sto.Query.All<TestEntity1ConcreteTable>().OfType<ITestEntity2ConcreteTable>();
        var result1 = source.ToArray().OfType<ITestEntityBaseConcreteTable>().Select(x => x.BaseField).ToArray();
        var result2 = source.OfType<ITestEntityBaseConcreteTable>().Select(x => x.BaseField).ToArray();
        Assert.That(result1, Is.Not.Empty);
        Assert.That(result1.SequenceEqual(result2));
      }

      using (var sto = OpenSessionTransaction()) {
        var source = sto.Query.All<TestEntity1SingleTable>().OfType<ITestEntity2SingleTable>();
        var result1 = source.ToArray().OfType<ITestEntityBaseSingleTable>().Select(x => x.BaseField).ToArray();
        var result2 = source.OfType<ITestEntityBaseSingleTable>().Select(x => x.BaseField).ToArray();
        Assert.That(result1, Is.Not.Empty);
        Assert.That(result1.SequenceEqual(result2));
      }
    }

    [Test]
    public void Test10()
    {
      using (var sto = OpenSessionTransaction()) {
        var exception = Assert.Throws<QueryTranslationException>(
          () => sto.Query.All<TestEntity1ClassTable>().OfType<ITestEntity2ClassTable>()
            .OfType<BaseEntityClassTable>().Run());
        Assert.That(exception.InnerException, Is.AssignableFrom(typeof (NotSupportedException)));
      }

      using (var sto = OpenSessionTransaction()) {
        var exception = Assert.Throws<QueryTranslationException>(
          () => sto.Query.All<TestEntity1ConcreteTable>().OfType<ITestEntity2ConcreteTable>()
            .OfType<BaseEntityConcreteTable>().Run());
        Assert.That(exception.InnerException, Is.AssignableFrom(typeof (NotSupportedException)));
      }

      using (var sto = OpenSessionTransaction()) {
        var exception = Assert.Throws<QueryTranslationException>(
          () => sto.Query.All<TestEntity1SingleTable>().OfType<ITestEntity2SingleTable>()
            .OfType<BaseEntitySingleTable>().Run());
        Assert.That(exception.InnerException, Is.AssignableFrom(typeof (NotSupportedException)));
      }
    }

    [Test]
    public void Test11()
    {
      using (var sto = OpenSessionTransaction()) {
        var query = sto.Query.All<TestEntity1ClassTable>().OfType<ITestEntity2ClassTable>()
          .OfType<IBaseEntityClassTable>();

        var result1 = query.Select(x => x.Field1).ToArray();
        Assert.That(result1[0], Is.EqualTo(10));
        Assert.That(result1[1], Is.EqualTo(20));
        Assert.That(result1[2], Is.EqualTo(30));
        Assert.That(result1.Length, Is.EqualTo(3));

        var result2 = query.OfType<TestEntity1ClassTable>().Select(x => x.Field1).ToArray();
        Assert.That(result2[0], Is.EqualTo(10));
        Assert.That(result2[1], Is.EqualTo(20));
        Assert.That(result2[2], Is.EqualTo(30));
        Assert.That(result2.Length, Is.EqualTo(3));

        var result3 = query.OfType<TestEntity1ClassTable>().OfType<IBaseEntityClassTable>()
          .OfType<TestEntity3aClassTable>().Select(x => x.Field1).ToArray();
        Assert.That(result3[0], Is.EqualTo(10));
        Assert.That(result3[1], Is.EqualTo(20));
        Assert.That(result3.Length, Is.EqualTo(2));
      }

      using (var sto = OpenSessionTransaction()) {
        var query = sto.Query.All<TestEntity1ConcreteTable>().OfType<ITestEntity2ConcreteTable>()
          .OfType<IBaseEntityConcreteTable>();

        var result1 = query.Select(x => x.Field1).ToArray();
        Assert.That(result1[0], Is.EqualTo(10));
        Assert.That(result1[1], Is.EqualTo(20));
        Assert.That(result1[2], Is.EqualTo(30));
        Assert.That(result1.Length, Is.EqualTo(3));

        var result2 = query.OfType<TestEntity1ConcreteTable>().Select(x => x.Field1).ToArray();
        Assert.That(result2[0], Is.EqualTo(10));
        Assert.That(result2[1], Is.EqualTo(20));
        Assert.That(result2[2], Is.EqualTo(30));
        Assert.That(result2.Length, Is.EqualTo(3));

        var result3 = query.OfType<TestEntity1ConcreteTable>().OfType<IBaseEntityConcreteTable>()
          .OfType<TestEntity3aConcreteTable>().Select(x => x.Field1).ToArray();
        Assert.That(result3[0], Is.EqualTo(10));
        Assert.That(result3[1], Is.EqualTo(20));
        Assert.That(result3.Length, Is.EqualTo(2));
      }

      using (var sto = OpenSessionTransaction()) {
        var query = sto.Query.All<TestEntity1SingleTable>().OfType<ITestEntity2SingleTable>()
          .OfType<IBaseEntitySingleTable>();

        var result1 = query.Select(x => x.Field1).ToArray();
        Assert.That(result1[0], Is.EqualTo(10));
        Assert.That(result1[1], Is.EqualTo(20));
        Assert.That(result1[2], Is.EqualTo(30));
        Assert.That(result1.Length, Is.EqualTo(3));

        var result2 = query.OfType<TestEntity1SingleTable>().Select(x => x.Field1).ToArray();
        Assert.That(result2[0], Is.EqualTo(10));
        Assert.That(result2[1], Is.EqualTo(20));
        Assert.That(result2[2], Is.EqualTo(30));
        Assert.That(result2.Length, Is.EqualTo(3));

        var result3 = query.OfType<TestEntity1SingleTable>().OfType<IBaseEntitySingleTable>()
          .OfType<TestEntity3aSingleTable>().Select(x => x.Field1).ToArray();
        Assert.That(result3[0], Is.EqualTo(10));
        Assert.That(result3[1], Is.EqualTo(20));
        Assert.That(result3.Length, Is.EqualTo(2));
      }
    }

    [Test]
    public void Test12()
    {
      using (var sto = OpenSessionTransaction()) {

        var source = sto.Query.All<TestEntity1ClassTable>().OfType<ITestEntity3aClassTable>()
          .SelectMany(x => x.Field6);

        var result1 = source.OfType<ITestEntity3aClassTable>().Select(x => x.Field5).ToArray();
        Assert.That(result1.Single(), Is.EqualTo(3.5));

        var result2 = source.OfType<ITestEntity2ClassTable>().Select(x => x.Field3.Value1).ToArray();
        Assert.That(result2.Single(), Is.EqualTo(20000));

        var result3 = source.OfType<ITestEntityBaseClassTable>().Select(x => x.BaseField).ToArray();
        Assert.That(result3.Single(), Is.EqualTo(2));
      }

      using (var sto = OpenSessionTransaction()) {

        var source = sto.Query.All<TestEntity1ConcreteTable>().OfType<ITestEntity3aConcreteTable>()
          .SelectMany(x => x.Field6);

        var result1 = source.OfType<ITestEntity3aConcreteTable>().Select(x => x.Field5).ToArray();
        Assert.That(result1.Single(), Is.EqualTo(3.5));

        var result2 = source.OfType<ITestEntity2ConcreteTable>().Select(x => x.Field3.Value1).ToArray();
        Assert.That(result2.Single(), Is.EqualTo(20000));

        var result3 = source.OfType<ITestEntityBaseConcreteTable>().Select(x => x.BaseField).ToArray();
        Assert.That(result3.Single(), Is.EqualTo(2));
      }

      using (var sto = OpenSessionTransaction()) {

        var source = sto.Query.All<TestEntity1SingleTable>().OfType<ITestEntity3aSingleTable>()
          .SelectMany(x => x.Field6);

        var result1 = source.OfType<ITestEntity3aSingleTable>().Select(x => x.Field5).ToArray();
        Assert.That(result1.Single(), Is.EqualTo(3.5));

        var result2 = source.OfType<ITestEntity2SingleTable>().Select(x => x.Field3.Value1).ToArray();
        Assert.That(result2.Single(), Is.EqualTo(20000));

        var result3 = source.OfType<ITestEntityBaseSingleTable>().Select(x => x.BaseField).ToArray();
        Assert.That(result3.Single(), Is.EqualTo(2));
      }
    }

    [Test]
    public void Test13()
    {
      using (var sto = OpenSessionTransaction()) {
        var source = sto.Query.All<TestEntity1ClassTable>();

        var result1A = source.ToArray().OfType<ITestEntity3aClassTable>().OfType<IBaseEntityClassTable>();
        var result1B = source.OfType<ITestEntity3aClassTable>().OfType<IBaseEntityClassTable>();

        var result2A = source.ToArray().OfType<ITestEntity2cClassTable>().OfType<IBaseEntityClassTable>();
        var result2B = source.OfType<ITestEntity2cClassTable>().OfType<IBaseEntityClassTable>();

        var resultA = result1A.Union(result2A).Select(x => x.Field1).ToArray();
        var resultB = result1B.Union(result2B).Select(x => x.Field1).ToArray();

        Assert.That(resultA, Is.Not.Empty);
        Assert.That(resultA.SequenceEqual(resultB));
      }

      using (var sto = OpenSessionTransaction()) {
        var source = sto.Query.All<TestEntity1ConcreteTable>();

        var result1A = source.ToArray().OfType<ITestEntity3aConcreteTable>().OfType<IBaseEntityConcreteTable>();
        var result1B = source.OfType<ITestEntity3aConcreteTable>().OfType<IBaseEntityConcreteTable>();

        var result2A = source.ToArray().OfType<ITestEntity2cConcreteTable>().OfType<IBaseEntityConcreteTable>();
        var result2B = source.OfType<ITestEntity2cConcreteTable>().OfType<IBaseEntityConcreteTable>();

        var resultA = result1A.Union(result2A).Select(x => x.Field1).ToArray();
        var resultB = result1B.Union(result2B).Select(x => x.Field1).ToArray();

        Assert.That(resultA, Is.Not.Empty);
        Assert.That(resultA.SequenceEqual(resultB));
      }

      using (var sto = OpenSessionTransaction()) {
        var source = sto.Query.All<TestEntity1SingleTable>();

        var result1A = source.ToArray().OfType<ITestEntity3aSingleTable>().OfType<IBaseEntitySingleTable>();
        var result1B = source.OfType<ITestEntity3aSingleTable>().OfType<IBaseEntitySingleTable>();

        var result2A = source.ToArray().OfType<ITestEntity2cSingleTable>().OfType<IBaseEntitySingleTable>();
        var result2B = source.OfType<ITestEntity2cSingleTable>().OfType<IBaseEntitySingleTable>();

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
        var source1A = sto.Query.All<TestEntity1ClassTable>().ToArray().OfType<IBaseEntityClassTable>();
        var source1B = sto.Query.All<TestEntity1ClassTable>().OfType<IBaseEntityClassTable>();

        var source2A = sto.Query.All<TestEntityHierarchy2ClassTable>().ToArray().OfType<IBaseEntityClassTable>();
        var source2B = sto.Query.All<TestEntityHierarchy2ClassTable>().OfType<IBaseEntityClassTable>();

        var result1A = source1A.Union(source2A).Select(x => x.Field1).ToArray();
        var result1B = source2B.Union(source1B).Select(x => x.Field1).ToArray();

        Assert.That(result1A, Is.Not.Empty);
        Assert.That(result1A.SequenceEqual(result1B));


        var result2A = source1B.Union(source2B).ToArray().OfType<ITestEntity2cClassTable>().Select(x => x.Field4)
          .ToArray();
        var result2B = source1B.Union(source2B).OfType<ITestEntity2cClassTable>().Select(x => x.Field4).ToArray();

        Assert.That(result2A, Is.Not.Empty);
        Assert.That(result2A.SequenceEqual(result2B));

        var result3 = sto.Query.All<TestEntity1ClassTable>().OfType<TestEntityHierarchy2ClassTable>().ToArray();
        Assert.That(result3, Is.Empty);

        var result4 = sto.Query.All<TestEntityHierarchy2ClassTable>().OfType<ITestEntity2cClassTable>().ToArray();
        Assert.That(result4, Is.Empty);
      }

      using (var sto = OpenSessionTransaction()) {
        var source1A = sto.Query.All<TestEntity1ConcreteTable>().ToArray().OfType<IBaseEntityConcreteTable>();
        var source1B = sto.Query.All<TestEntity1ConcreteTable>().OfType<IBaseEntityConcreteTable>();

        var source2A = sto.Query.All<TestEntityHierarchy2ConcreteTable>().ToArray().OfType<IBaseEntityConcreteTable>();
        var source2B = sto.Query.All<TestEntityHierarchy2ConcreteTable>().OfType<IBaseEntityConcreteTable>();

        var result1A = source1A.Union(source2A).Select(x => x.Field1).ToArray();
        var result1B = source2B.Union(source1B).Select(x => x.Field1).ToArray();

        Assert.That(result1A, Is.Not.Empty);
        Assert.That(result1A.SequenceEqual(result1B));


        var result2A = source1B.Union(source2B).ToArray().OfType<ITestEntity2cConcreteTable>().Select(x => x.Field4)
          .ToArray();
        var result2B = source1B.Union(source2B).OfType<ITestEntity2cConcreteTable>().Select(x => x.Field4).ToArray();

        Assert.That(result2A, Is.Not.Empty);
        Assert.That(result2A.SequenceEqual(result2B));

        var result3 = sto.Query.All<TestEntity1ConcreteTable>().OfType<TestEntityHierarchy2ConcreteTable>().ToArray();
        Assert.That(result3, Is.Empty);

        var result4 = sto.Query.All<TestEntityHierarchy2ConcreteTable>().OfType<ITestEntity2cConcreteTable>().ToArray();
        Assert.That(result4, Is.Empty);
      }

      using (var sto = OpenSessionTransaction()) {
        var source1A = sto.Query.All<TestEntity1SingleTable>().ToArray().OfType<IBaseEntitySingleTable>();
        var source1B = sto.Query.All<TestEntity1SingleTable>().OfType<IBaseEntitySingleTable>();

        var source2A = sto.Query.All<TestEntityHierarchy2SingleTable>().ToArray().OfType<IBaseEntitySingleTable>();
        var source2B = sto.Query.All<TestEntityHierarchy2SingleTable>().OfType<IBaseEntitySingleTable>();

        var result1A = source1A.Union(source2A).Select(x => x.Field1).ToArray();
        var result1B = source2B.Union(source1B).Select(x => x.Field1).ToArray();

        Assert.That(result1A, Is.Not.Empty);
        Assert.That(result1A.SequenceEqual(result1B));


        var result2A = source1B.Union(source2B).ToArray().OfType<ITestEntity2cSingleTable>().Select(x => x.Field4)
          .ToArray();
        var result2B = source1B.Union(source2B).OfType<ITestEntity2cSingleTable>().Select(x => x.Field4).ToArray();

        Assert.That(result2A, Is.Not.Empty);
        Assert.That(result2A.SequenceEqual(result2B));

        var result3 = sto.Query.All<TestEntity1SingleTable>().OfType<TestEntityHierarchy2SingleTable>().ToArray();
        Assert.That(result3, Is.Empty);

        var result4 = sto.Query.All<TestEntityHierarchy2SingleTable>().OfType<ITestEntity2cSingleTable>().ToArray();
        Assert.That(result4, Is.Empty);
      }
    }

    protected override void PopulateData()
    {
      using (var sto = OpenSessionTransaction()) {
        ClassTableData.PopulateData();
        ConcreteTableData.PopulateData();
        SingleTableData.PopulateData();
        sto.TransactionScope.Complete();
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (TestEntity1SingleTable).Assembly, typeof (TestEntity1SingleTable).Namespace);
      return config;
    }
  }
}