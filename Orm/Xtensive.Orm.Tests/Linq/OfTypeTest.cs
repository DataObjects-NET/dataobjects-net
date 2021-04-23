// Copyright (C) 2019-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Kudelin
// Created:    2019.01.31

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Tests.Linq.OfTypeTestModel;

#region Model

namespace Xtensive.Orm.Tests.Linq.OfTypeTestModel
{
  public interface IBaseEntity : IEntity
  {
    [Field, Key]
    int Id { get; }

    [Field]
    long Field1 { get; set; }
  }

  public interface IBaseInterface : IEntity
  {
    [Field]
    ulong BaseField { get; set; }
  }

  public interface IA
  {
    long Field2 { get; set; }
  }

  public interface IB1 : IBaseInterface
  {
    [Field]
    Structure1 Field3 { get; set; }

    [Field(Length = 50)]
    string Field4 { get; set; }
  }

  public interface IC : IBaseInterface
  {
    [Field]
    double Field5 { get; set; }

    [Field]
    EntitySet<IB1> Field6 { get; set; }
  }

  public interface IB2 : IBaseInterface
  {
    [Field]
    Structure1 Field3 { get; set; }

    [Field(Length = 50)]
    string Field4 { get; set; }
  }

  public class BaseEntity : Entity, IBaseEntity
  {
    public int Id { get; private set; }

    public long Field1 { get; set; }
  }

  public class Structure1 : Structure
  {
    [Field]
    public int Value1 { get; set; }

    [Field]
    public Structure2 Value2 { get; set; }

    [Field(Length = 50)]
    public string Value3 { get; set; }
  }

  public class Structure2 : Structure
  {
    [Field]
    public int Value1 { get; set; }

    [Field]
    public DateTime Value2 { get; set; }
  }

  public interface IB3ClassTable : IB2
  {
  }

  [HierarchyRoot(InheritanceSchema.ClassTable)]
  public class AClassTable : BaseEntity, IA
  {
    [Field]
    public long Field2 { get; set; }
  }

  public class B1ClassTable : AClassTable, IB1
  {
    public Structure1 Field3 { get; set; }

    public ulong BaseField { get; set; }

    public string Field4 { get; set; }
  }

  public class C1ClassTable : B1ClassTable, IC
  {
    public double Field5 { get; set; }

    public EntitySet<IB1> Field6 { get; set; }
  }

  public class B2ClassTable : AClassTable, IB1
  {
    [Field]
    public EntitySet<B2ClassTable> Field5 { get; set; }

    public ulong BaseField { get; set; }

    public string Field4 { get; set; }

    public Structure1 Field3 { get; set; }
  }

  public class B3ClassTable : AClassTable, IB3ClassTable
  {
    public ulong BaseField { get; set; }

    public Structure1 Field3 { get; set; }

    public string Field4 { get; set; }
  }


  public interface IB3SingleTable : IB2
  {
  }

  [HierarchyRoot(InheritanceSchema.SingleTable)]
  public class ASingleTable : BaseEntity, IA
  {
    [Field]
    public long Field2 { get; set; }
  }

  public class B1SingleTable : ASingleTable, IB1
  {
    public Structure1 Field3 { get; set; }

    public ulong BaseField { get; set; }

    public string Field4 { get; set; }
  }

  public class C1SingleTable : B1SingleTable, IC
  {
    public double Field5 { get; set; }

    public EntitySet<IB1> Field6 { get; set; }
  }

  public class B2SingleTable : ASingleTable, IB1
  {
    [Field]
    public EntitySet<B2SingleTable> Field5 { get; set; }

    public ulong BaseField { get; set; }

    public string Field4 { get; set; }

    public Structure1 Field3 { get; set; }
  }

  public class B3SingleTable : ASingleTable, IB3SingleTable
  {
    public ulong BaseField { get; set; }

    public Structure1 Field3 { get; set; }

    public string Field4 { get; set; }
  }


  public interface IB3ConcreteTable : IB2
  {
  }

  [HierarchyRoot(InheritanceSchema.ConcreteTable)]
  public class AConcreteTable : BaseEntity, IA
  {
    [Field]
    public long Field2 { get; set; }
  }

  public class B1ConcreteTable : AConcreteTable, IB1
  {
    public Structure1 Field3 { get; set; }

    public ulong BaseField { get; set; }

    public string Field4 { get; set; }
  }

  public class C1ConcreteTable : B1ConcreteTable, IC
  {
    public double Field5 { get; set; }

    public EntitySet<IB1> Field6 { get; set; }
  }

  public class B2ConcreteTable : AConcreteTable, IB1
  {
    [Field]
    public EntitySet<B2ConcreteTable> Field5 { get; set; }

    public ulong BaseField { get; set; }

    public string Field4 { get; set; }

    public Structure1 Field3 { get; set; }
  }

  public class B3ConcreteTable : AConcreteTable, IB3ConcreteTable
  {
    public ulong BaseField { get; set; }

    public Structure1 Field3 { get; set; }

    public string Field4 { get; set; }
  }
}

#endregion

namespace Xtensive.Orm.Tests.Linq
{
  public class OfTypeTest : AutoBuildTest
  {
    [Test]
    public void Test01()
    {
      var expectedStrings = new[] { "100000String1", "200000String2", "300000String3" };

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result = session.Query.All<AClassTable>().OfType<IB1>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();
        foreach (var expectedString in expectedStrings) {
          Assert.That(result.Contains(expectedString), Is.True);
        }
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result = session.Query.All<AConcreteTable>().OfType<IB1>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();
        foreach (var expectedString in expectedStrings) {
          Assert.That(result.Contains(expectedString), Is.True);
        }
      }


      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result = session.Query.All<ASingleTable>().OfType<IB1>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();
        foreach (var expectedString in expectedStrings) {
          Assert.That(result.Contains(expectedString), Is.True);
        }
      }
    }

    [Test]
    public void Test02()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result1 = session.Query.All<B2ClassTable>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();

        var result2 = session.Query.All<AClassTable>().OfType<B2ClassTable>()
          .OfType<IB1>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();

        Assert.That(result1, Is.Not.Empty);
        Assert.That(result1.Length, Is.EqualTo(result2.Length));
        Assert.That(result1.Intersect(result2).Count(), Is.EqualTo(result1.Length));
      }


      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result1 = session.Query.All<B2ConcreteTable>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();

        var result2 = session.Query.All<AConcreteTable>().OfType<B2ConcreteTable>()
          .OfType<IB1>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();

        Assert.That(result1, Is.Not.Empty);
        Assert.That(result1.Length, Is.EqualTo(result2.Length));
        Assert.That(result1.Intersect(result2).Count(), Is.EqualTo(result1.Length));
      }


      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result1 = session.Query.All<B2SingleTable>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();

        var result2 = session.Query.All<ASingleTable>().OfType<B2SingleTable>()
          .OfType<IB1>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();

        Assert.That(result1, Is.Not.Empty);
        Assert.That(result1.Length, Is.EqualTo(result2.Length));
        Assert.That(result1.Intersect(result2).Count(), Is.EqualTo(result1.Length));
      }
    }

    [Test]
    public void Test03()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result1 = session.Query.All<B1ClassTable>().OfType<IB1>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();
        Assert.That(result1.Length, Is.EqualTo(2));
        new[] { "100000String1", "200000String2" }
          .ForEach(s => Assert.That(result1.Contains(s), Is.True));

        var result2 = session.Query.All<B2ClassTable>().OfType<IB1>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();
        Assert.That(result2.Single(), Is.EqualTo("300000String3"));
      }


      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result1 = session.Query.All<B1ConcreteTable>().OfType<IB1>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();
        Assert.That(result1.Length, Is.EqualTo(2));
        new[] { "100000String1", "200000String2" }
          .ForEach(s => Assert.That(result1.Contains(s), Is.True));

        var result2 = session.Query.All<B2ConcreteTable>().OfType<IB1>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();
        Assert.That(result2.Single(), Is.EqualTo("300000String3"));
      }


      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result1 = session.Query.All<B1SingleTable>().OfType<IB1>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();
        Assert.That(result1.Length, Is.EqualTo(2));
        new[] { "100000String1", "200000String2" }
          .ForEach(s => Assert.That(result1.Contains(s), Is.True));

        var result2 = session.Query.All<B2SingleTable>().OfType<IB1>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();
        Assert.That(result2.Single(), Is.EqualTo("300000String3"));
      }
    }

    [Test]
    public void Test04()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result1 = session.Query.All<B1ClassTable>().OfType<AClassTable>()
          .Select(x => x.Field2).ToArray();
        Assert.That(result1.Length, Is.EqualTo(2));
        new[] { 1000, 2000 }
          .ForEach(i => Assert.That(result1.Contains(i), Is.True));
        Assert.That(result1.First(), Is.EqualTo(1000));
        Assert.That(result1.Last(), Is.EqualTo(2000));

        var result2 = session.Query.All<B2ClassTable>().OfType<AClassTable>()
          .Select(x => x.Field2).ToArray();
        Assert.That(result2.Single(), Is.EqualTo(3000));
      }


      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result1 = session.Query.All<B1ConcreteTable>().OfType<AConcreteTable>()
          .Select(x => x.Field2).ToArray();
        Assert.That(result1.Length, Is.EqualTo(2));
        new[] { 1000, 2000 }
          .ForEach(i => Assert.That(result1.Contains(i), Is.True));

        var result2 = session.Query.All<B2ConcreteTable>().OfType<AConcreteTable>()
          .Select(x => x.Field2).ToArray();
        Assert.That(result2.Single(), Is.EqualTo(3000));
      }


      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result1 = session.Query.All<B1SingleTable>().OfType<ASingleTable>()
          .Select(x => x.Field2).ToArray();
        Assert.That(result1.Length, Is.EqualTo(2));
        new[] { 1000, 2000 }
          .ForEach(i => Assert.That(result1.Contains(i), Is.True));

        var result2 = session.Query.All<B2SingleTable>().OfType<ASingleTable>()
          .Select(x => x.Field2).ToArray();
        Assert.That(result2.Single(), Is.EqualTo(3000));
      }
    }

    [Test]
    public void Test05()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result = session.Query.All<B1ClassTable>().OfType<B2ClassTable>()
          .Select(x => x.Field2).ToArray();
        Assert.That(result, Is.Empty);
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result = session.Query.All<B1ConcreteTable>().OfType<B2ConcreteTable>()
          .Select(x => x.Field2).ToArray();
        Assert.That(result, Is.Empty);
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result = session.Query.All<B1SingleTable>().OfType<B2SingleTable>()
          .Select(x => x.Field2).ToArray();
        Assert.That(result, Is.Empty);
      }
    }

    [Test]
    public void Test06()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result = session.Query.All<B1ClassTable>().OfType<B3ClassTable>()
          .Select(x => x.Field3).ToArray();
        Assert.That(result, Is.Empty);
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result = session.Query.All<B1ConcreteTable>().OfType<B3ConcreteTable>()
          .Select(x => x.Field3).ToArray();
        Assert.That(result, Is.Empty);
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result = session.Query.All<B1SingleTable>().OfType<B3SingleTable>()
          .Select(x => x.Field3).ToArray();
        Assert.That(result, Is.Empty);
      }
    }

    [Test]
    public void Test07()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var source = session.Query.All<AClassTable>().OfType<IB1>();

        var result1 = source.Select(x => x.Field3.Value1).ToArray();
        Assert.That(result1.Length, Is.EqualTo(3));
        new[] { 10000, 20000, 30000 }
          .ForEach(i => Assert.That(result1.Contains(i)));

        var result2 = source.OfType<B3ClassTable>().Select(x => x.Field3.Value1).ToArray();
        Assert.That(result2, Is.Empty);
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var source = session.Query.All<AConcreteTable>().OfType<IB1>();

        var result1 = source.Select(x => x.Field3.Value1).ToArray();
        Assert.That(result1.Length, Is.EqualTo(3));
        new[] { 10000, 20000, 30000 }
          .ForEach(i => Assert.That(result1.Contains(i)));

        var result2 = source.OfType<B3ConcreteTable>().Select(x => x.Field3.Value1).ToArray();
        Assert.That(result2, Is.Empty);
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var source = session.Query.All<ASingleTable>().OfType<IB1>();
        var result1 = source.Select(x => x.Field3.Value1).ToArray();
        Assert.That(result1.Length, Is.EqualTo(3));
        new[] { 10000, 20000, 30000 }
          .ForEach(i => Assert.That(result1.Contains(i)));

        var result2 = source.OfType<B3SingleTable>().Select(x => x.Field3.Value1).ToArray();
        Assert.That(result2, Is.Empty);
      }
    }

    [Test]
    public void Test08()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var source = session.Query.All<AClassTable>().OfType<IB1>();
        var result1 = source.ToArray().OfType<IC>().Select(x => x.Field5).ToArray();
        var result2 = source.OfType<IC>().Select(x => x.Field5).ToArray();
        Assert.That(result1, Is.Not.Empty);
        Assert.That(result1.Length, Is.EqualTo(result2.Length));
        Assert.That(result1.Intersect(result2).Count(), Is.EqualTo(result1.Length));
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var source = session.Query.All<AConcreteTable>().OfType<IB1>();
        var result1 = source.ToArray().OfType<IC>().Select(x => x.Field5).ToArray();
        var result2 = source.OfType<IC>().Select(x => x.Field5).ToArray();
        Assert.That(result1, Is.Not.Empty);
        Assert.That(result1.Length, Is.EqualTo(result2.Length));
        Assert.That(result1.Intersect(result2).Count(), Is.EqualTo(result1.Length));
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var source = session.Query.All<ASingleTable>().OfType<IB1>();
        var result1 = source.ToArray().OfType<IC>().Select(x => x.Field5).ToArray();
        var result2 = source.OfType<IC>().Select(x => x.Field5).ToArray();
        Assert.That(result1, Is.Not.Empty);
        Assert.That(result1.Length, Is.EqualTo(result2.Length));
        Assert.That(result1.Intersect(result2).Count(), Is.EqualTo(result1.Length));
      }
    }

    [Test]
    public void Test09()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var source = session.Query.All<AClassTable>().OfType<IB1>();
        var result1 = source.ToArray().OfType<IBaseInterface>().Select(x => x.BaseField).ToArray();
        var result2 = source.OfType<IBaseInterface>().Select(x => x.BaseField).ToArray();
        Assert.That(result1, Is.Not.Empty);
        Assert.That(result1.Length, Is.EqualTo(result2.Length));
        Assert.That(result1.Intersect(result2).Count(), Is.EqualTo(result1.Length));
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var source = session.Query.All<AConcreteTable>().OfType<IB1>();
        var result1 = source.ToArray().OfType<IBaseInterface>().Select(x => x.BaseField).ToArray();
        var result2 = source.OfType<IBaseInterface>().Select(x => x.BaseField).ToArray();
        Assert.That(result1, Is.Not.Empty);
        Assert.That(result1.Length, Is.EqualTo(result2.Length));
        Assert.That(result1.Intersect(result2).Count(), Is.EqualTo(result1.Length));
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var source = session.Query.All<ASingleTable>().OfType<IB1>();
        var result1 = source.ToArray().OfType<IBaseInterface>().Select(x => x.BaseField).ToArray();
        var result2 = source.OfType<IBaseInterface>().Select(x => x.BaseField).ToArray();
        Assert.That(result1, Is.Not.Empty);
        Assert.That(result1.Length, Is.EqualTo(result2.Length));
        Assert.That(result1.Intersect(result2).Count(), Is.EqualTo(result1.Length));
      }
    }

    [Test]
    public void Test10()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var exception = Assert.Throws<QueryTranslationException>(
          () => session.Query.All<AClassTable>().OfType<IB1>()
            .OfType<BaseEntity>().Run());
        Assert.That(exception.InnerException, Is.AssignableFrom(typeof(NotSupportedException)));
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var exception = Assert.Throws<QueryTranslationException>(
          () => session.Query.All<AConcreteTable>().OfType<IB1>()
            .OfType<BaseEntity>().Run());
        Assert.That(exception.InnerException, Is.AssignableFrom(typeof(NotSupportedException)));
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var exception = Assert.Throws<QueryTranslationException>(
          () => session.Query.All<ASingleTable>().OfType<IB1>()
            .OfType<BaseEntity>().Run());
        Assert.That(exception.InnerException, Is.AssignableFrom(typeof(NotSupportedException)));
      }
    }

    [Test]
    public void Test11()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<AClassTable>().OfType<IB1>()
          .OfType<IBaseEntity>();

        var result1 = query.Select(x => x.Field1).ToArray();
        Assert.That(result1.Length, Is.EqualTo(3));
        new[] { 10, 20, 30 }
          .ForEach(i => Assert.That(result1.Contains(i)));

        var result2 = query.OfType<AClassTable>().Select(x => x.Field1).ToArray();
        Assert.That(result2.Length, Is.EqualTo(3));
        new[] { 10, 20, 30 }
          .ForEach(i => Assert.That(result2.Contains(i)));

        var result3 = query.OfType<AClassTable>().OfType<IBaseEntity>()
          .OfType<C1ClassTable>().Select(x => x.Field1).ToArray();
        Assert.That(result3.Length, Is.EqualTo(2));
        new[] { 10, 20 }
          .ForEach(i => Assert.That(result3.Contains(i)));
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<AConcreteTable>().OfType<IB1>()
          .OfType<IBaseEntity>();

        var result1 = query.Select(x => x.Field1).ToArray();
        Assert.That(result1.Length, Is.EqualTo(3));
        new[] { 10, 20, 30 }
          .ForEach(i => Assert.That(result1.Contains(i)));

        var result2 = query.OfType<AConcreteTable>().Select(x => x.Field1).ToArray();
        Assert.That(result2.Length, Is.EqualTo(3));
        new[] { 10, 20, 30 }
          .ForEach(i => Assert.That(result2.Contains(i)));

        var result3 = query.OfType<AConcreteTable>().OfType<IBaseEntity>()
          .OfType<C1ConcreteTable>().Select(x => x.Field1).ToArray();
        Assert.That(result3.Length, Is.EqualTo(2));
        new[] { 10, 20 }
          .ForEach(i => Assert.That(result3.Contains(i)));
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<ASingleTable>().OfType<IB1>()
          .OfType<IBaseEntity>();

        var result1 = query.Select(x => x.Field1).ToArray();
        Assert.That(result1.Length, Is.EqualTo(3));
        new[] { 10, 20, 30 }
          .ForEach(i => Assert.That(result1.Contains(i)));

        var result2 = query.OfType<ASingleTable>().Select(x => x.Field1).ToArray();
        Assert.That(result2.Length, Is.EqualTo(3));
        new[] { 10, 20, 30 }
          .ForEach(i => Assert.That(result2.Contains(i)));

        var result3 = query.OfType<ASingleTable>().OfType<IBaseEntity>()
          .OfType<C1SingleTable>().Select(x => x.Field1).ToArray();
        Assert.That(result3.Length, Is.EqualTo(2));
        new[] { 10, 20 }
          .ForEach(i => Assert.That(result3.Contains(i)));
      }
    }

    [Test]
    public void Test12()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        var source = session.Query.All<AClassTable>().OfType<IC>()
          .SelectMany(x => x.Field6);

        var result1 = source.OfType<IC>().Select(x => x.Field5).ToArray();
        Assert.That(result1.Single(), Is.EqualTo(3.5));

        var result2 = source.OfType<IB1>().Select(x => x.Field3.Value1).ToArray();
        Assert.That(result2.Single(), Is.EqualTo(20000));

        var result3 = source.OfType<IBaseInterface>().Select(x => x.BaseField).ToArray();
        Assert.That(result3.Single(), Is.EqualTo(2));
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        var source = session.Query.All<AConcreteTable>().OfType<IC>()
          .SelectMany(x => x.Field6);

        var result1 = source.OfType<IC>().Select(x => x.Field5).ToArray();
        Assert.That(result1.Single(), Is.EqualTo(3.5));

        var result2 = source.OfType<IB1>().Select(x => x.Field3.Value1).ToArray();
        Assert.That(result2.Single(), Is.EqualTo(20000));

        var result3 = source.OfType<IBaseInterface>().Select(x => x.BaseField).ToArray();
        Assert.That(result3.Single(), Is.EqualTo(2));
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        var source = session.Query.All<ASingleTable>().OfType<IC>()
          .SelectMany(x => x.Field6);

        var result1 = source.OfType<IC>().Select(x => x.Field5).ToArray();
        Assert.That(result1.Single(), Is.EqualTo(3.5));

        var result2 = source.OfType<IB1>().Select(x => x.Field3.Value1).ToArray();
        Assert.That(result2.Single(), Is.EqualTo(20000));

        var result3 = source.OfType<IBaseInterface>().Select(x => x.BaseField).ToArray();
        Assert.That(result3.Single(), Is.EqualTo(2));
      }
    }

    [Test]
    public void Test13()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var source = session.Query.All<AClassTable>();

        var result1A = source.ToArray().OfType<IC>().OfType<IBaseEntity>();
        var result1B = source.OfType<IC>().OfType<IBaseEntity>();

        var result2A = source.ToArray().OfType<IB3ClassTable>().OfType<IBaseEntity>();
        var result2B = source.OfType<IB3ClassTable>().OfType<IBaseEntity>();

        var resultA = result1A.Union(result2A).Select(x => x.Field1).ToArray();
        var resultB = result1B.Union(result2B).Select(x => x.Field1).ToArray();

        Assert.That(resultA, Is.Not.Empty);
        Assert.That(resultA.Length, Is.EqualTo(resultB.Length));
        Assert.That(resultA.Intersect(resultB).Count(), Is.EqualTo(resultA.Length));
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var source = session.Query.All<AConcreteTable>();

        var result1A = source.ToArray().OfType<IC>().OfType<IBaseEntity>();
        var result1B = source.OfType<IC>().OfType<IBaseEntity>();

        var result2A = source.ToArray().OfType<IB3ConcreteTable>().OfType<IBaseEntity>();
        var result2B = source.OfType<IB3ConcreteTable>().OfType<IBaseEntity>();

        var resultA = result1A.Union(result2A).Select(x => x.Field1).ToArray();
        var resultB = result1B.Union(result2B).Select(x => x.Field1).ToArray();

        Assert.That(resultA, Is.Not.Empty);
        Assert.That(resultA.Length, Is.EqualTo(resultB.Length));
        Assert.That(resultA.Intersect(resultB).Count(), Is.EqualTo(resultA.Length));
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var source = session.Query.All<ASingleTable>();

        var result1A = source.ToArray().OfType<IC>().OfType<IBaseEntity>();
        var result1B = source.OfType<IC>().OfType<IBaseEntity>();

        var result2A = source.ToArray().OfType<IB3SingleTable>().OfType<IBaseEntity>();
        var result2B = source.OfType<IB3SingleTable>().OfType<IBaseEntity>();

        var resultA = result1A.Union(result2A).Select(x => x.Field1).ToArray();
        var resultB = result1B.Union(result2B).Select(x => x.Field1).ToArray();

        Assert.That(resultA, Is.Not.Empty);
        Assert.That(resultA.Length, Is.EqualTo(resultB.Length));
        Assert.That(resultA.Intersect(resultB).Count(), Is.EqualTo(resultA.Length));
      }
    }

    [Test]
    public void Test14()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var source1A = session.Query.All<AClassTable>().ToArray().OfType<IBaseEntity>();
        var source1B = session.Query.All<AClassTable>().OfType<IBaseEntity>();

        var source2A = session.Query.All<AConcreteTable>().ToArray().OfType<IBaseEntity>();
        var source2B = session.Query.All<AConcreteTable>().OfType<IBaseEntity>();

        var result1A = source1A.Union(source2A).Select(x => x.Field1).ToArray();
        var result1B = source2B.Union(source1B).Select(x => x.Field1).ToArray();

        Assert.That(result1A, Is.Not.Empty);
        Assert.That(result1B.Length, Is.EqualTo(result1A.Length));
        foreach (var item in result1B) {
          Assert.That(result1A.Contains(item));
        }


        var result2A = source1B.Union(source2B).ToArray().OfType<B3ClassTable>().Select(x => x.Field4)
          .ToArray();
        var result2B = source1B.Union(source2B).OfType<B3ClassTable>().Select(x => x.Field4).ToArray();

        Assert.That(result2A, Is.Not.Empty);
        Assert.That(result2B.Length, Is.EqualTo(result2A.Length));
        foreach (var item in result2B) {
          Assert.That(result2A.Contains(item));
        }

        var result3 = session.Query.All<AClassTable>().OfType<AConcreteTable>().ToArray();
        Assert.That(result3, Is.Empty);

        var result4 = session.Query.All<AConcreteTable>().OfType<IB3ClassTable>().ToArray();
        Assert.That(result4, Is.Empty);
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var source1A = session.Query.All<AConcreteTable>().ToArray().OfType<IBaseEntity>();
        var source1B = session.Query.All<AConcreteTable>().OfType<IBaseEntity>();

        var source2A = session.Query.All<ASingleTable>().ToArray().OfType<IBaseEntity>();
        var source2B = session.Query.All<ASingleTable>().OfType<IBaseEntity>();

        var result1A = source1A.Union(source2A).Select(x => x.Field1).ToArray();
        var result1B = source2B.Union(source1B).Select(x => x.Field1).ToArray();

        Assert.That(result1A, Is.Not.Empty);
        Assert.That(result1B.Length, Is.EqualTo(result1A.Length));
        foreach (var item in result1B) {
          Assert.That(result1A.Contains(item));
        }


        var result2A = source1B.Union(source2B).ToArray().OfType<B3ConcreteTable>().Select(x => x.Field4)
          .ToArray();
        var result2B = source1B.Union(source2B).OfType<B3ConcreteTable>().Select(x => x.Field4).ToArray();

        Assert.That(result2A, Is.Not.Empty);
        Assert.That(result2B.Length, Is.EqualTo(result2A.Length));
        foreach (var item in result2B) {
          Assert.That(result2A.Contains(item));
        }

        var result3 = session.Query.All<AConcreteTable>().OfType<ASingleTable>().ToArray();
        Assert.That(result3, Is.Empty);

        var result4 = session.Query.All<ASingleTable>().OfType<IB3ConcreteTable>().ToArray();
        Assert.That(result4, Is.Empty);
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var source1A = session.Query.All<ASingleTable>().ToArray().OfType<IBaseEntity>();
        var source1B = session.Query.All<ASingleTable>().OfType<IBaseEntity>();

        var source2A = session.Query.All<AClassTable>().ToArray().OfType<IBaseEntity>();
        var source2B = session.Query.All<AClassTable>().OfType<IBaseEntity>();

        var result1A = source1A.Union(source2A).Select(x => x.Field1).ToArray();
        var result1B = source2B.Union(source1B).Select(x => x.Field1).ToArray();

        Assert.That(result1A, Is.Not.Empty);
        Assert.That(result1B.Length, Is.EqualTo(result1A.Length));
        foreach (var item in result1B) {
          Assert.That(result1A.Contains(item));
        }

        var result2A = source1B.Union(source2B).ToArray().OfType<B3SingleTable>().Select(x => x.Field4)
          .ToArray();
        var result2B = source1B.Union(source2B).OfType<B3SingleTable>().Select(x => x.Field4).ToArray();

        Assert.That(result2A, Is.Not.Empty);
        Assert.That(result2B.Length, Is.EqualTo(result2A.Length));
        foreach (var item in result2B) {
          Assert.That(result2A.Contains(item));
        }

        var result3 = session.Query.All<ASingleTable>().OfType<AClassTable>().ToArray();
        Assert.That(result3, Is.Empty);

        var result4 = session.Query.All<AClassTable>().OfType<IB3SingleTable>().ToArray();
        Assert.That(result4, Is.Empty);
      }
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        PopulateData<C1ClassTable, B2ClassTable, B3ClassTable>();
        PopulateData<C1ConcreteTable, B2ConcreteTable, B3ConcreteTable>();
        PopulateData<C1SingleTable, B2SingleTable, B3SingleTable>();
        tx.Complete();
      }
    }

    private void PopulateData<TC1, TB2, TB3>()
      where TC1 : IC, IB1, IBaseEntity, IA, new()
      where TB2 : IB1, IBaseEntity, IA, new()
      where TB3 : IB2, IBaseEntity, IA, new()
    {
      _ = new TC1() {
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
        new TC1 {
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
      _ = new TB2() {
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
      _ = new TB3() {
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
      config.Types.Register(typeof(ASingleTable).Assembly, typeof(ASingleTable).Namespace);
      return config;
    }
  }
}