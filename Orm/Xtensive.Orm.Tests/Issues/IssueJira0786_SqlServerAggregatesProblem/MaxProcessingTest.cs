// Copyright (C) 2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2020.03.26

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Tests.Issues.IssueJira0786_SqlServerAggregatesProblem
{
  public sealed class MaxProcessingTest : AggregatesProblemTestBase
  {
    [Test]
    public void ByteFieldTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.ByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.ByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void SByteFieldTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.SByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.SByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void ShortFieldTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.ShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.ShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void UShortFieldTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.UShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.UShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void IntFieldTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.ByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.ByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void UIntFieldTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.UIntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.UIntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void LongFieldTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.ByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.ByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void FloatFieldTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.FloatValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.FloatValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void DoubleFieldTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.DoubleValue1);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.DoubleValue1);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void DecimalFieldTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.DecimalValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.DecimalValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableByteFieldTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableSByteFieldTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableSByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableSByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableShortFieldTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableUShortFieldTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableUShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableUShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableIntFieldTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableUIntFieldTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableUIntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableUIntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableLongFieldTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableFloatFieldTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableFloatValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableFloatValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableDoubleFieldTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableDoubleValue1);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableDoubleValue1);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableDecimalFieldTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableDecimalValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableDecimalValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void ByteFieldExpressionTest01()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.ByteValue + i.ByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.ByteValue + i.ByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void ByteFieldExpressionTest02()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (short) i.ByteValue + i.ByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (short) i.ByteValue + i.ByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void ByteFieldExpressionTest03()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (short) i.ByteValue + (short) i.ByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (short) i.ByteValue + (short) i.ByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void ByteFieldExpressionTest04()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (int) i.ByteValue + i.ByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (int) i.ByteValue + i.ByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void ByteFieldExpressionTest05()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (int) i.ByteValue + (int) i.ByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (int) i.ByteValue + (int) i.ByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void ByteFieldExpressionTest06()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (long) i.ByteValue + i.ByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (long) i.ByteValue + i.ByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void ByteFieldExpressionTest07()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (long) i.ByteValue + (long) i.ByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (long) i.ByteValue + (long) i.ByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void ByteFieldExpressionTest08()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (float) i.ByteValue + i.ByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (float) i.ByteValue + i.ByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void ByteFieldExpressionTest09()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (float) i.ByteValue + (float) i.ByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (float) i.ByteValue + (float) i.ByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void ByteFieldExpressionTest10()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (double) i.ByteValue + i.ByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (double) i.ByteValue + i.ByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void ByteFieldExpressionTest11()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (double) i.ByteValue + (double) i.ByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (double) i.ByteValue + (double) i.ByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void ByteFieldExpressionTest12()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (decimal) i.ByteValue + i.ByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (decimal) i.ByteValue + i.ByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void ByteFieldExpressionTest13()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (decimal) i.ByteValue + (decimal) i.ByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (decimal) i.ByteValue + (decimal) i.ByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void ByteFieldExpressionTest14()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.ByteValue + i.IntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.ByteValue + i.IntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void ByteFieldExpressionTest15()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.ByteValue + i.LongValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.ByteValue + i.LongValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void ByteFieldExpressionTest16()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.ByteValue + i.FloatValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.ByteValue + i.FloatValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void ByteFieldExpressionTest17()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.ByteValue + i.DoubleValue1);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.ByteValue + i.DoubleValue1);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void ByteFieldExpressionTest18()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.ByteValue + i.DecimalValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.ByteValue + i.DecimalValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void SByteFieldExpressionTest01()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.SByteValue + i.SByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.SByteValue + i.SByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void SByteFieldExpressionTest02()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (short) i.SByteValue + i.SByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (short) i.SByteValue + i.SByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void SByteFieldExpressionTest03()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (short) i.SByteValue + (short) i.SByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (short) i.SByteValue + (short) i.SByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void SByteFieldExpressionTest04()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (int) i.SByteValue + i.SByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (int) i.SByteValue + i.SByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void SByteFieldExpressionTest05()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (int) i.SByteValue + (int) i.SByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (int) i.SByteValue + (int) i.SByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void SByteFieldExpressionTest06()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (long) i.SByteValue + i.SByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (long) i.SByteValue + i.SByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void SByteFieldExpressionTest07()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (long) i.SByteValue + (long) i.SByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (long) i.SByteValue + (long) i.SByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void SByteFieldExpressionTest08()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (float) i.SByteValue + i.SByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (float) i.SByteValue + i.SByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void SByteFieldExpressionTest09()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (float) i.SByteValue + (float) i.SByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (float) i.SByteValue + (float) i.SByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void SByteFieldExpressionTest10()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (double) i.SByteValue + i.SByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (double) i.SByteValue + i.SByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void SByteFieldExpressionTest11()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (double) i.SByteValue + (double) i.SByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (double) i.SByteValue + (double) i.SByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void SByteFieldExpressionTest12()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (decimal) i.SByteValue + i.SByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (decimal) i.SByteValue + i.SByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void SByteFieldExpressionTest13()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (decimal) i.SByteValue + (decimal) i.SByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (decimal) i.SByteValue + (decimal) i.SByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void SByteFieldExpressionTest14()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.SByteValue + i.ByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.SByteValue + i.ByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void SByteFieldExpressionTest15()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.SByteValue + i.IntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.SByteValue + i.IntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void SByteFieldExpressionTest16()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.SByteValue + i.LongValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.SByteValue + i.LongValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void SByteFieldExpressionTest17()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.SByteValue + i.FloatValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.SByteValue + i.FloatValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void SByteFieldExpressionTest18()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.SByteValue + i.DoubleValue1);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.SByteValue + i.DoubleValue1);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void SByteFieldExpressionTest19()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.SByteValue + i.DecimalValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.SByteValue + i.DecimalValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void ShortFieldExpressionTest01()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.ShortValue + i.ShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.ShortValue + i.ShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void ShortFieldExpressionTest02()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (int) i.ShortValue + i.ShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (int) i.ShortValue + i.ShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void ShortFieldExpressionTest03()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (int) i.ShortValue + (int) i.ShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (int) i.ShortValue + (int) i.ShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void ShortFieldExpressionTest04()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (long) i.ShortValue + i.ShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (long) i.ShortValue + i.ShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void ShortFieldExpressionTest05()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (long) i.ShortValue + (long) i.ShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (long) i.ShortValue + (long) i.ShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void ShortFieldExpressionTest06()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (float) i.ShortValue + i.ShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (float) i.ShortValue + i.ShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void ShortFieldExpressionTest07()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (float) i.ShortValue + (float) i.ShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (float) i.ShortValue + (float) i.ShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void ShortFieldExpressionTest08()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (double) i.ShortValue + i.ShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (double) i.ShortValue + i.ShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void ShortFieldExpressionTest09()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (double) i.ShortValue + (double) i.ShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (double) i.ShortValue + (double) i.ShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void ShortFieldExpressionTest10()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (decimal) i.ShortValue + i.ShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (decimal) i.ShortValue + i.ShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void ShortFieldExpressionTest11()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (decimal) i.ShortValue + (decimal) i.ShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (decimal) i.ShortValue + (decimal) i.ShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void ShortFieldExpressionTest12()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.ShortValue + i.ByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.ShortValue + i.ByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void ShortFieldExpressionTest13()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.ShortValue + i.IntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.ShortValue + i.IntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void ShortFieldExpressionTest14()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.ShortValue + i.LongValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.ShortValue + i.LongValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void ShortFieldExpressionTest15()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.ShortValue + i.FloatValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.ShortValue + i.FloatValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void ShortFieldExpressionTest16()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.ShortValue + i.DoubleValue1);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.ShortValue + i.DoubleValue1);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void ShortFieldExpressionTest17()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.ShortValue + i.DecimalValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.ShortValue + i.DecimalValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void UShortFieldExpressionTest01()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.ShortValue + i.ShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.ShortValue + i.ShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void UShortFieldExpressionTest02()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (int) i.UShortValue + i.UShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (int) i.UShortValue + i.UShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void UShortFieldExpressionTest03()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (int) i.UShortValue + (int) i.UShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (int) i.UShortValue + (int) i.UShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void UShortFieldExpressionTest04()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (long) i.UShortValue + i.UShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (long) i.UShortValue + i.UShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void UShortFieldExpressionTest05()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (long) i.UShortValue + (long) i.UShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (long) i.UShortValue + (long) i.UShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void UShortFieldExpressionTest06()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (float) i.UShortValue + i.UShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (float) i.UShortValue + i.UShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void UShortFieldExpressionTest07()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (float) i.UShortValue + (float) i.UShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (float) i.UShortValue + (float) i.UShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void UShortFieldExpressionTest08()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (double) i.UShortValue + i.UShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (double) i.UShortValue + i.UShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void UShortFieldExpressionTest09()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (double) i.UShortValue + (double) i.UShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (double) i.UShortValue + (double) i.UShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void UShortFieldExpressionTest10()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (decimal) i.UShortValue + i.UShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (decimal) i.UShortValue + i.UShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void UShortFieldExpressionTest11()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (decimal) i.UShortValue + (decimal) i.UShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (decimal) i.UShortValue + (decimal) i.UShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void UShortFieldExpressionTest12()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.UShortValue + i.ByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.UShortValue + i.ByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void UShortFieldExpressionTest13()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.UShortValue + i.IntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.UShortValue + i.IntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void UShortFieldExpressionTest14()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.UShortValue + i.LongValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.UShortValue + i.LongValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void UShortFieldExpressionTest15()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.UShortValue + i.FloatValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.UShortValue + i.FloatValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void UShortFieldExpressionTest16()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.UShortValue + i.DoubleValue1);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.UShortValue + i.DoubleValue1);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void UShortFieldExpressionTest17()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.UShortValue + i.DecimalValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.UShortValue + i.DecimalValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void IntFieldExpressionTest01()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.IntValue * i.IntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.IntValue * i.IntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void IntFieldExpressionTest02()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (long) i.IntValue * i.IntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (long) i.IntValue * i.IntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void IntFieldExpressionTest03()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (long) i.IntValue * (long) i.IntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (long) i.IntValue * (long) i.IntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void IntFieldExpressionTest04()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (float) i.IntValue * i.IntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (float) i.IntValue * i.IntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void IntFieldExpressionTest05()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (float) i.IntValue * (float) i.IntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (float) i.IntValue * (float) i.IntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void IntFieldExpressionTest06()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (double) i.IntValue * i.IntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (double) i.IntValue * i.IntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void IntFieldExpressionTest07()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (double) i.IntValue * (double) i.IntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (double) i.IntValue * (double) i.IntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void IntFieldExpressionTest08()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (decimal) i.IntValue * i.IntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (decimal) i.IntValue * i.IntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void IntFieldExpressionTest09()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (decimal) i.IntValue * (decimal) i.IntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (decimal) i.IntValue * (decimal) i.IntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void IntFieldExpressionTest10()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.IntValue * i.ByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.IntValue * i.ByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void IntFieldExpressionTest11()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.IntValue * i.LongValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.IntValue * i.LongValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void IntFieldExpressionTest12()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.IntValue * i.FloatValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.IntValue * i.FloatValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void IntFieldExpressionTest13()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.IntValue * i.DoubleValue1);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.IntValue * i.DoubleValue1);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void IntFieldExpressionTest14()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.IntValue * i.DecimalValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.IntValue * i.DecimalValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void UIntFieldExpressionTest01()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.UIntValue * i.UIntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.UIntValue * i.UIntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void UIntFieldExpressionTest02()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (long) i.UIntValue * i.UIntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (long) i.UIntValue * i.UIntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void UIntFieldExpressionTest03()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (long) i.UIntValue * (long) i.UIntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (long) i.UIntValue * (long) i.UIntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void UIntFieldExpressionTest04()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (float) i.UIntValue * i.UIntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (float) i.UIntValue * i.UIntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void UIntFieldExpressionTest05()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (float) i.UIntValue * (float) i.UIntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (float) i.UIntValue * (float) i.UIntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void UIntFieldExpressionTest06()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (double) i.UIntValue * i.UIntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (double) i.UIntValue * i.UIntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void UIntFieldExpressionTest07()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (double) i.UIntValue * (double) i.UIntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (double) i.UIntValue * (double) i.UIntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void UIntFieldExpressionTest08()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (decimal) i.UIntValue * i.UIntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (decimal) i.UIntValue * i.UIntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void UIntFieldExpressionTest09()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (decimal) i.UIntValue * (decimal) i.UIntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (decimal) i.UIntValue * (decimal) i.UIntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void UIntFieldExpressionTest10()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.UIntValue + i.UIntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.UIntValue + i.UIntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void UIntFieldExpressionTest11()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.UIntValue * i.LongValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.UIntValue * i.LongValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void UIntFieldExpressionTest12()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.UIntValue * i.FloatValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.UIntValue * i.FloatValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void UIntFieldExpressionTest13()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.UIntValue * i.DoubleValue1);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.UIntValue * i.DoubleValue1);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void UIntFieldExpressionTest14()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.UIntValue * i.DecimalValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.UIntValue * i.DecimalValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void LongFieldExpressionTest01()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.LongValue * i.LongValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.LongValue * i.LongValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void LongFieldExpressionTest02()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (float) i.LongValue * i.LongValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (float) i.LongValue * i.LongValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void LongFieldExpressionTest03()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (float) i.LongValue * (float) i.LongValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (float) i.LongValue * (float) i.LongValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void LongFieldExpressionTest04()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (double) i.LongValue * i.LongValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (double) i.LongValue * i.LongValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void LongFieldExpressionTest05()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (double) i.LongValue * (double) i.LongValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (double) i.LongValue * (double) i.LongValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void LongFieldExpressionTest06()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (decimal) i.LongValue * i.LongValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (decimal) i.LongValue * i.LongValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void LongFieldExpressionTest07()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (decimal) i.LongValue * (decimal) i.LongValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (decimal) i.LongValue * (decimal) i.LongValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void LongFieldExpressionTest08()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.LongValue * i.FloatValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.LongValue * i.FloatValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void LongFieldExpressionTest09()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.LongValue * i.DoubleValue1);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.LongValue * i.DoubleValue1);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void LongFieldExpressionTest10()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.LongValue * i.DecimalValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.LongValue * i.DecimalValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void ULongFieldExpressionTest02()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (float) i.ULongValue * i.ULongValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (float) i.ULongValue * i.ULongValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void ULongFieldExpressionTest03()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (float) i.ULongValue * (float) i.ULongValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (float) i.ULongValue * (float) i.ULongValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void ULongFieldExpressionTest04()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (double) i.ULongValue * i.ULongValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (double) i.ULongValue * i.ULongValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void ULongFieldExpressionTest05()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (double) i.ULongValue * (double) i.ULongValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (double) i.ULongValue * (double) i.ULongValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void ULongFieldExpressionTest06()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (decimal) i.ULongValue * i.ULongValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (decimal) i.ULongValue * i.ULongValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void ULongFieldExpressionTest07()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (decimal) i.ULongValue * (decimal) i.ULongValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (decimal) i.ULongValue * (decimal) i.ULongValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void ULongFieldExpressionTest08()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.ULongValue * i.FloatValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.ULongValue * i.FloatValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void ULongFieldExpressionTest09()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.ULongValue * i.DoubleValue1);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.ULongValue * i.DoubleValue1);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void ULongFieldExpressionTest10()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.ULongValue * i.DecimalValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.ULongValue * i.DecimalValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void FloatFieldExpressionTest01()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.FloatValue * i.FloatValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.FloatValue * i.FloatValue);
        Assert.That(Math.Abs(queryableResult - enumerableResult), Is.LessThan(FloatValueAccuracy));
      }
    }

    [Test]
    public void FloatFieldExpressionTest02()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (double) i.FloatValue * i.FloatValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (double) i.FloatValue * i.FloatValue);
        Assert.That(Math.Abs(queryableResult - enumerableResult), Is.LessThan(FloatValueAccuracy));
      }
    }

    [Test]
    public void FloatFieldExpressionTest03()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (double) i.FloatValue * (double) i.FloatValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (double) i.FloatValue * (double) i.FloatValue);
        Assert.That(Math.Abs(queryableResult - enumerableResult), Is.LessThan(FloatValueAccuracy));
      }
    }

    [Test]
    public void FloatFieldExpressionTest04()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (decimal) i.FloatValue * (decimal) i.FloatValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (decimal) i.FloatValue * (decimal) i.FloatValue);
        Assert.That(Math.Abs(queryableResult - enumerableResult), Is.LessThan(FloatValueAccuracy));
      }
    }

    [Test]
    public void FloatFieldExpressionTest05()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.FloatValue * i.DoubleValue1);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.FloatValue * i.DoubleValue1);
        Assert.That(Math.Abs(queryableResult - enumerableResult), Is.LessThan(FloatValueAccuracy));
      }
    }

    [Test]
    public void FloatFieldExpressionTest06()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (decimal) i.FloatValue * i.DecimalValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (decimal) i.FloatValue * i.DecimalValue);
        Assert.That(Math.Abs(queryableResult - enumerableResult), Is.LessThan(FloatValueAccuracy));
      }
    }

    [Test]
    public void DoubleFieldExpressionTest01()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.DoubleValue1 * i.DoubleValue1);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.DoubleValue1 * i.DoubleValue1);
        Assert.That(Math.Abs(queryableResult - enumerableResult), Is.LessThan(DoubleValueAccuracy));
      }
    }

    [Test]
    public void DoubleFieldExpressionTest02()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (decimal) i.DoubleValue1 * i.DecimalValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (decimal) i.DoubleValue1 * i.DecimalValue);
        Assert.That(Math.Abs(queryableResult - enumerableResult), Is.LessThan(DoubleValueAccuracy));
      }
    }

    [Test]
    public void DecimalFieldExpressionTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.DecimalValue * i.DecimalValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.DecimalValue * i.DecimalValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableByteFieldExpressionTest01()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableByteValue + i.NullableByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableByteValue + i.NullableByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableByteFieldExpressionTest02()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (short?) i.NullableByteValue + i.NullableByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (short?) i.NullableByteValue + i.NullableByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableByteFieldExpressionTest03()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (short?) i.NullableByteValue + (short?) i.NullableByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (short?) i.NullableByteValue + (short?) i.NullableByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableByteFieldExpressionTest04()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (int?) i.NullableByteValue + i.NullableByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (int?) i.NullableByteValue + i.NullableByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableByteFieldExpressionTest05()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (int?) i.NullableByteValue + (int?) i.NullableByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (int?) i.NullableByteValue + (int?) i.NullableByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableByteFieldExpressionTest06()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (long?) i.NullableByteValue + i.NullableByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (long?) i.NullableByteValue + i.NullableByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableByteFieldExpressionTest07()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (long?) i.NullableByteValue + (long?) i.NullableByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (long?) i.NullableByteValue + (long?) i.NullableByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableByteFieldExpressionTest08()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (float?) i.NullableByteValue + i.NullableByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (float) i.ByteValue + i.NullableByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableByteFieldExpressionTest09()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (float?) i.NullableByteValue + (float?) i.NullableByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (float?) i.NullableByteValue + (float?) i.NullableByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableByteFieldExpressionTest10()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (double?) i.NullableByteValue + i.NullableByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (double?) i.NullableByteValue + i.NullableByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableByteFieldExpressionTest11()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (double) i.ByteValue + (double) i.ByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (double) i.ByteValue + (double) i.ByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableByteFieldExpressionTest12()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (decimal?) i.NullableByteValue + i.NullableByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (decimal?) i.NullableByteValue + i.NullableByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableByteFieldExpressionTest13()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (decimal?) i.NullableByteValue + (decimal?) i.NullableByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (decimal?) i.NullableByteValue + (decimal?) i.NullableByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableByteFieldExpressionTest14()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableByteValue + i.NullableIntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableByteValue + i.NullableIntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableByteFieldExpressionTest15()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableByteValue + i.NullableLongValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableByteValue + i.NullableLongValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableByteFieldExpressionTest16()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableByteValue + i.NullableFloatValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableByteValue + i.NullableFloatValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableByteFieldExpressionTest17()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableByteValue + i.NullableDoubleValue1);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableByteValue + i.NullableDoubleValue1);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableByteFieldExpressionTest18()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableByteValue + i.NullableDecimalValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableByteValue + i.NullableDecimalValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableSByteFieldExpressionTest01()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableSByteValue + i.NullableSByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableSByteValue + i.NullableSByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableSByteFieldExpressionTest02()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (short?) i.NullableSByteValue + i.NullableSByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (short?) i.NullableSByteValue + i.NullableSByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableSByteFieldExpressionTest03()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (short?) i.NullableSByteValue + (short?) i.NullableSByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (short?) i.NullableSByteValue + (short?) i.NullableSByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableSByteFieldExpressionTest04()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (int?) i.NullableSByteValue + i.NullableSByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (int?) i.NullableSByteValue + i.NullableSByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableSByteFieldExpressionTest05()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (int?) i.NullableSByteValue + (int?) i.NullableSByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (int?) i.NullableSByteValue + (int?) i.NullableSByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableSByteFieldExpressionTest06()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (long?) i.NullableSByteValue + i.NullableSByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (long?) i.NullableSByteValue + i.NullableSByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableSByteFieldExpressionTest07()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (long?) i.NullableSByteValue + (long?) i.NullableSByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (long?) i.NullableSByteValue + (long?) i.NullableSByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableSByteFieldExpressionTest08()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (float?) i.NullableSByteValue + i.NullableSByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (float?) i.NullableSByteValue + i.NullableSByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableSByteFieldExpressionTest09()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (float?) i.NullableSByteValue + (float?) i.NullableSByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (float?) i.NullableSByteValue + (float?) i.NullableSByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableSByteFieldExpressionTest10()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (double?) i.NullableSByteValue + i.NullableSByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (double?) i.NullableSByteValue + i.NullableSByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableSByteFieldExpressionTest11()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (double?) i.NullableSByteValue + (double?) i.NullableSByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (double?) i.NullableSByteValue + (double?) i.NullableSByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableSByteFieldExpressionTest12()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (decimal?) i.NullableSByteValue + i.NullableSByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (decimal?) i.NullableSByteValue + i.NullableSByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableSByteFieldExpressionTest13()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (decimal?) i.NullableSByteValue + (decimal?) i.NullableSByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (decimal?) i.NullableSByteValue + (decimal?) i.NullableSByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableSByteFieldExpressionTest14()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableSByteValue + i.NullableByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableSByteValue + i.NullableByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableSByteFieldExpressionTest15()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableSByteValue + i.NullableIntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableSByteValue + i.NullableIntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableSByteFieldExpressionTest16()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableSByteValue + i.NullableLongValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableSByteValue + i.NullableLongValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableSByteFieldExpressionTest17()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableSByteValue + i.NullableFloatValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableSByteValue + i.NullableFloatValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableSByteFieldExpressionTest18()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableSByteValue + i.NullableDoubleValue1);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableSByteValue + i.NullableDoubleValue1);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableSByteFieldExpressionTest19()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableSByteValue + i.NullableDecimalValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableSByteValue + i.NullableDecimalValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableShortFieldExpressionTest01()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableShortValue + i.NullableShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableShortValue + i.NullableShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableShortFieldExpressionTest02()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (int?) i.NullableShortValue + i.NullableShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (int?) i.NullableShortValue + i.NullableShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableShortFieldExpressionTest03()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (int?) i.NullableShortValue + (int?) i.NullableShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (int?) i.NullableShortValue + (int?) i.NullableShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableShortFieldExpressionTest04()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (long?) i.NullableShortValue + i.NullableShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (long?) i.NullableShortValue + i.NullableShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableShortFieldExpressionTest05()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (long?) i.NullableShortValue + (long?) i.NullableShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (long?) i.NullableShortValue + (long?) i.NullableShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableShortFieldExpressionTest06()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (float?) i.NullableShortValue + i.NullableShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (float?) i.NullableShortValue + i.NullableShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableShortFieldExpressionTest07()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (float?) i.NullableShortValue + (float?) i.NullableShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (float) i.ShortValue + (float?) i.NullableShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableShortFieldExpressionTest08()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (double?) i.NullableShortValue + i.NullableShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (double?) i.NullableShortValue + i.NullableShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableShortFieldExpressionTest09()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (double?) i.NullableShortValue + (double?) i.NullableShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (double?) i.NullableShortValue + (double?) i.NullableShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableShortFieldExpressionTest10()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (decimal?) i.NullableShortValue + i.NullableShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (decimal?) i.NullableShortValue + i.NullableShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableShortFieldExpressionTest11()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (decimal?) i.NullableShortValue + (decimal?) i.NullableShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (decimal?) i.NullableShortValue + (decimal?) i.NullableShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableShortFieldExpressionTest12()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableShortValue + i.NullableByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableShortValue + i.NullableByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableShortFieldExpressionTest13()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableShortValue + i.NullableIntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableShortValue + i.NullableIntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableShortFieldExpressionTest14()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableShortValue + i.NullableLongValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableShortValue + i.NullableLongValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableShortFieldExpressionTest15()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableShortValue + i.NullableFloatValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableShortValue + i.NullableFloatValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableShortFieldExpressionTest16()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableShortValue + i.NullableDoubleValue1);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableShortValue + i.NullableDoubleValue1);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableShortFieldExpressionTest17()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableShortValue + i.NullableDecimalValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableShortValue + i.NullableDecimalValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableUShortFieldExpressionTest01()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableUShortValue + i.NullableUShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableUShortValue + i.NullableUShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableUShortFieldExpressionTest02()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (int?) i.NullableUShortValue + i.NullableUShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (int?) i.NullableUShortValue + i.NullableUShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableUShortFieldExpressionTest03()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (int?) i.NullableUShortValue + (int?) i.NullableUShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (int?) i.NullableUShortValue + (int?) i.NullableUShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableUShortFieldExpressionTest04()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (long?) i.NullableUShortValue + i.NullableUShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (long?) i.NullableUShortValue + i.NullableUShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableUShortFieldExpressionTest05()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (long?) i.NullableUShortValue + (long?) i.NullableUShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (long?) i.NullableUShortValue + (long?) i.NullableUShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableUShortFieldExpressionTest06()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (float?) i.NullableUShortValue + i.NullableUShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (float?) i.NullableUShortValue + i.NullableUShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableUShortFieldExpressionTest07()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (float?) i.NullableUShortValue + (float?) i.NullableUShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (float?) i.NullableUShortValue + (float?) i.NullableUShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableUShortFieldExpressionTest08()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (double?) i.NullableUShortValue + i.NullableUShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (double?) i.NullableUShortValue + i.NullableUShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableUShortFieldExpressionTest09()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (double?) i.NullableUShortValue + (double?) i.NullableUShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (double?) i.NullableUShortValue + (double?) i.NullableUShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableUShortFieldExpressionTest10()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (decimal?) i.NullableUShortValue + i.NullableUShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (decimal?) i.NullableUShortValue + i.NullableUShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableUShortFieldExpressionTest11()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (decimal?) i.NullableUShortValue + (decimal?) i.NullableUShortValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (decimal?) i.NullableUShortValue + (decimal?) i.NullableUShortValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableUShortFieldExpressionTest12()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableUShortValue + i.NullableByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableUShortValue + i.NullableByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableUShortFieldExpressionTest13()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableUShortValue + i.NullableIntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableUShortValue + i.NullableIntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableUShortFieldExpressionTest14()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableUShortValue + i.NullableLongValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableUShortValue + i.NullableLongValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableUShortFieldExpressionTest15()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableUShortValue + i.NullableFloatValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableUShortValue + i.NullableFloatValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableUShortFieldExpressionTest16()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableUShortValue + i.NullableDoubleValue1);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableUShortValue + i.NullableDoubleValue1);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableUShortFieldExpressionTest17()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableUShortValue + i.NullableDecimalValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableUShortValue + i.NullableDecimalValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableIntFieldExpressionTest01()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableIntValue * i.NullableIntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableIntValue * i.NullableIntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableIntFieldExpressionTest02()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (long?) i.NullableIntValue * i.NullableIntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (long?) i.NullableIntValue * i.NullableIntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableIntFieldExpressionTest03()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (long?) i.NullableIntValue * (long?) i.NullableIntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (long?) i.NullableIntValue * (long?) i.NullableIntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableIntFieldExpressionTest04()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (float?) i.NullableIntValue * i.NullableIntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (float?) i.NullableIntValue * i.NullableIntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableIntFieldExpressionTest05()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (float?) i.NullableIntValue * (float?) i.NullableIntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (float?) i.NullableIntValue * (float?) i.NullableIntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableIntFieldExpressionTest06()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (double?) i.NullableIntValue * i.NullableIntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (double?) i.NullableIntValue * i.NullableIntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableIntFieldExpressionTest07()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (double?) i.NullableIntValue * (double?) i.NullableIntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (double?) i.NullableIntValue * (double?) i.NullableIntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableIntFieldExpressionTest08()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (decimal?) i.NullableIntValue * i.NullableIntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (decimal?) i.NullableIntValue * i.NullableIntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableIntFieldExpressionTest09()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (decimal?) i.NullableIntValue * (decimal?) i.NullableIntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (decimal?) i.NullableIntValue * (decimal?) i.NullableIntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableIntFieldExpressionTest10()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableIntValue * i.NullableByteValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableIntValue * i.NullableByteValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableIntFieldExpressionTest11()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableIntValue * i.NullableLongValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableIntValue * i.NullableLongValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableIntFieldExpressionTest12()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableIntValue * i.NullableFloatValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableIntValue * i.NullableFloatValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableIntFieldExpressionTest13()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableIntValue * i.NullableDoubleValue1);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableIntValue * i.NullableDoubleValue1);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableIntFieldExpressionTest14()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableIntValue * i.NullableDecimalValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableIntValue * i.NullableDecimalValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableUIntFieldExpressionTest01()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableUIntValue * i.NullableUIntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableUIntValue * i.NullableUIntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableUIntFieldExpressionTest02()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (long?) i.NullableUIntValue * i.NullableUIntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (long?) i.NullableUIntValue * i.NullableUIntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableUIntFieldExpressionTest03()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (long?) i.NullableUIntValue * (long?) i.NullableUIntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (long?) i.NullableUIntValue * (long?) i.NullableUIntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableUIntFieldExpressionTest04()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (float?) i.NullableUIntValue * i.NullableUIntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (float?) i.NullableUIntValue * i.NullableUIntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableUIntFieldExpressionTest05()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (float?) i.NullableUIntValue * (float?) i.NullableUIntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (float?) i.NullableUIntValue * (float?) i.NullableUIntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableUIntFieldExpressionTest06()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (double?) i.NullableUIntValue * i.NullableUIntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (double?) i.NullableUIntValue * i.NullableUIntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableUIntFieldExpressionTest07()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (double?) i.NullableUIntValue * (double?) i.NullableUIntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (double?) i.NullableUIntValue * (double?) i.NullableUIntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableUIntFieldExpressionTest08()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (decimal?) i.NullableUIntValue * i.NullableUIntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (decimal?) i.NullableUIntValue * i.NullableUIntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableUIntFieldExpressionTest09()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (decimal?) i.NullableUIntValue * (decimal?) i.NullableUIntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (decimal?) i.NullableUIntValue * (decimal?) i.NullableUIntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableUIntFieldExpressionTest10()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableUIntValue + i.NullableUIntValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableUIntValue + i.NullableUIntValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableUIntFieldExpressionTest11()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableUIntValue * i.NullableLongValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableUIntValue * i.NullableLongValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableUIntFieldExpressionTest12()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableUIntValue * i.NullableFloatValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableUIntValue * i.NullableFloatValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableUIntFieldExpressionTest13()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableUIntValue * i.NullableDoubleValue1);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableUIntValue * i.NullableDoubleValue1);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableUIntFieldExpressionTest14()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableUIntValue * i.NullableDecimalValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableUIntValue * i.NullableDecimalValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableLongFieldExpressionTest01()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableLongValue * i.NullableLongValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableLongValue * i.NullableLongValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableLongFieldExpressionTest02()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (float?) i.NullableLongValue * i.NullableLongValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (float?) i.NullableLongValue * i.NullableLongValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableLongFieldExpressionTest03()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (float?) i.NullableLongValue * (float?) i.NullableLongValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (float?) i.NullableLongValue * (float?) i.NullableLongValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableLongFieldExpressionTest04()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (double?) i.NullableLongValue * i.NullableLongValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (double?) i.NullableLongValue * i.NullableLongValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableLongFieldExpressionTest05()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (double?) i.NullableLongValue * (double?) i.NullableLongValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (double?) i.NullableLongValue * (double?) i.NullableLongValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableLongFieldExpressionTest06()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (decimal?) i.NullableLongValue * i.NullableLongValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (decimal?) i.NullableLongValue * i.NullableLongValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableLongFieldExpressionTest07()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (decimal?) i.NullableLongValue * (decimal?) i.NullableLongValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (decimal?) i.NullableLongValue * (decimal?) i.NullableLongValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableLongFieldExpressionTest08()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableLongValue * i.NullableFloatValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableLongValue * i.NullableFloatValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableLongFieldExpressionTest09()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableLongValue * i.NullableDoubleValue1);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableLongValue * i.NullableDoubleValue1);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableLongFieldExpressionTest10()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableLongValue * i.NullableDecimalValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableLongValue * i.NullableDecimalValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableULongFieldExpressionTest02()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (float?) i.NullableULongValue * i.NullableULongValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (float?) i.NullableULongValue * i.NullableULongValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableULongFieldExpressionTest03()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (float?) i.NullableULongValue * (float?) i.NullableULongValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (float?) i.NullableULongValue * (float?) i.NullableULongValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableULongFieldExpressionTest04()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (double?) i.NullableULongValue * i.NullableULongValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (double?) i.NullableULongValue * i.NullableULongValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableULongFieldExpressionTest05()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (double?) i.NullableULongValue * (double?) i.NullableULongValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (double?) i.NullableULongValue * (double?) i.NullableULongValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableULongFieldExpressionTest06()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (decimal?) i.NullableULongValue * i.NullableULongValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (decimal?) i.NullableULongValue * i.NullableULongValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableULongFieldExpressionTest07()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (decimal?) i.NullableULongValue * (decimal?) i.NullableULongValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (decimal?) i.NullableULongValue * (decimal?) i.NullableULongValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableULongFieldExpressionTest08()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableULongValue * i.NullableFloatValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableULongValue * i.NullableFloatValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableULongFieldExpressionTest09()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableULongValue * i.NullableDoubleValue1);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableULongValue * i.NullableDoubleValue1);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableULongFieldExpressionTest10()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableULongValue * i.NullableDecimalValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableULongValue * i.NullableDecimalValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableFloatFieldExpressionTest01()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableFloatValue * i.NullableFloatValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableFloatValue * i.NullableFloatValue);
        Assert.That(Math.Abs(queryableResult.Value - enumerableResult.Value), Is.LessThan(FloatValueAccuracy));
      }
    }

    [Test]
    public void NullableFloatFieldExpressionTest02()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (double?) i.NullableFloatValue * i.NullableFloatValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (double?) i.NullableFloatValue * i.NullableFloatValue);
        Assert.That(Math.Abs(queryableResult.Value - enumerableResult.Value), Is.LessThan(FloatValueAccuracy));
      }
    }

    [Test]
    public void NullableFloatFieldExpressionTest03()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (double?) i.NullableFloatValue * (double?) i.NullableFloatValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (double?) i.NullableFloatValue * (double?) i.NullableFloatValue);
        Assert.That(Math.Abs(queryableResult.Value - enumerableResult.Value), Is.LessThan(FloatValueAccuracy));
      }
    }

    [Test]
    public void NullableFloatFieldExpressionTest04()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (decimal?) i.NullableFloatValue * (decimal?) i.NullableFloatValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (decimal?) i.NullableFloatValue * (decimal?) i.NullableFloatValue);
        Assert.That(Math.Abs(queryableResult.Value - enumerableResult.Value), Is.LessThan(FloatValueAccuracy));
      }
    }

    [Test]
    public void NullableFloatFieldExpressionTest05()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableFloatValue * i.NullableDoubleValue1);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableFloatValue * i.NullableDoubleValue1);
        Assert.That(Math.Abs(queryableResult.Value - enumerableResult.Value), Is.LessThan(FloatValueAccuracy));
      }
    }

    [Test]
    public void NullableFloatFieldExpressionTest06()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (decimal?) i.NullableFloatValue * i.NullableDecimalValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (decimal?) i.NullableFloatValue * i.NullableDecimalValue);
        Assert.That(Math.Abs(queryableResult.Value - enumerableResult.Value), Is.LessThan(FloatValueAccuracy));
      }
    }

    [Test]
    public void NullableDoubleFieldExpressionTest01()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableDoubleValue1 * i.NullableDoubleValue1);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableDoubleValue1 * i.NullableDoubleValue1);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Mute]
    [Test]
    public void NullableDoubleFieldExpressionTest02()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => (decimal?) i.NullableDoubleValue1 * i.NullableDecimalValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => (decimal?) i.NullableDoubleValue1 * i.NullableDecimalValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }

    [Test]
    public void NullableDecimalFieldExpressionTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var queryableResult = session.Query.All<TestEntity>().Max(i => i.NullableDecimalValue * i.NullableDecimalValue);
        var enumerableResult = session.Query.All<TestEntity>().AsEnumerable().Max(i => i.NullableDecimalValue * i.NullableDecimalValue);
        Assert.That(queryableResult, Is.EqualTo(enumerableResult));
      }
    }
  }
}