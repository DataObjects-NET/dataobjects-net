// Copyright (C) 2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Tuples;
using NUnit.Framework;
using Tuple = Xtensive.Tuples.Tuple;
using System.Linq;

namespace Xtensive.Orm.Tests.Core.Tuples
{
  [TestFixture]
  public class TupleFormatAndParseTest
  {
    // The test contains cases of types which can be in tuple in real usage,
    // basically all the type which have map in any of storage.
    // No exotic cases.

    [Test]
    public void BoolValueTest()
    {
      var tuple = Xtensive.Tuples.Tuple.Create<bool, bool, bool, bool, bool, bool>(true, true, false, true, false, false);

      TestTuple(tuple);
    }

    [Test]
    public void CharValueTest()
    {
      var tuple = Xtensive.Tuples.Tuple.Create<char, char, char>('s', 'q', 'd');
      TestTuple(tuple);
    }

    [Test]
    public void NumbericValuesTest()
    {
      var tuple = Tuple.Create<byte, byte, byte, sbyte, sbyte, sbyte>(byte.MinValue, byte.MaxValue, 254, sbyte.MinValue, sbyte.MaxValue, 127);
      TestTuple(tuple);

      tuple = Tuple.Create<short, short, short, ushort, ushort, ushort>(short.MinValue, short.MaxValue, 23859, ushort.MinValue, ushort.MaxValue, 58994);
      TestTuple(tuple);

      tuple = Tuple.Create<int, int, int, uint, uint, uint>(int.MinValue, int.MaxValue, 578621485, uint.MinValue, uint.MaxValue, 545564624);
      TestTuple(tuple);

      tuple = Tuple.Create<long, long, long, ulong, ulong, ulong>(
        long.MinValue, long.MaxValue, 5468546548746545465, ulong.MinValue, ulong.MaxValue, 8868768654687865456);
      TestTuple(tuple);

      tuple = Tuple.Create<float, float, float, float, float, float>(
        float.MinValue, float.MaxValue, float.E, float.NaN, float.Pi, float.Tau);
      TestTuple(tuple);

      tuple = Tuple.Create<float, float, float>(
        2f, 3.000057f, 4.5678911f);
      TestTuple(tuple);

      tuple = Tuple.Create<double, double, double, double, double, double>(
        double.MinValue, double.MaxValue, double.E, double.Epsilon, double.NaN, double.Pi);
      TestTuple(tuple);

      tuple = Tuple.Create<double, double, double>(double.Tau, 1, 2.487987484654987);
      TestTuple(tuple);

      tuple = Tuple.Create<decimal, decimal, decimal, decimal, decimal, decimal>(
        decimal.MinValue, decimal.MaxValue, decimal.One, decimal.Zero, decimal.One, decimal.MinusOne);
      TestTuple(tuple);

      tuple = Tuple.Create<decimal, decimal, decimal, decimal, decimal>(
        1m,  0m, -1m, 20746875411587982147878187855m, 0.18775548545212454615315465325m);
      TestTuple(tuple);
    }

    [Test]
    public void StringValuesTest()
    {
      var tuple = Tuple.Create<string, string, string>("", "bdc", string.Empty);
      TestTuple(tuple);
    }

    [Test]
    public void TimeSpanValueTest()
    {
      var tuple = Tuple.Create<TimeSpan, TimeSpan, TimeSpan, TimeSpan, TimeSpan>(
        TimeSpan.MinValue, TimeSpan.MaxValue, TimeSpan.Zero,
        TimeSpan.FromMicroseconds(6060123456),
        TimeSpan.FromTicks(70701234567));
      TestTuple(tuple);
    }

    [Test]
    public void DateTimeValueTest()
    {
      var tuple = Tuple.Create<DateTime, DateTime, DateTime, DateTime, DateTime, DateTime>(
        DateTime.MinValue,
        DateTime.MaxValue,
        DateTime.Now,
        DateTime.UtcNow,
        new DateTime(2018, 7, 15, 12, 13, 14, 152, 168),
        new DateTime(1_000_000_000 + 1234567)
        );
      TestTuple(tuple);
    }

    [Test]
    public void DateOnlyValueTest()
    {
      var tuple = Tuple.Create<DateOnly, DateOnly, DateOnly>(
        DateOnly.MinValue,
        DateOnly.MaxValue,
        DateOnly.FromDayNumber(6584));
      TestTuple(tuple);
    }

    [Test]
    public void TimeOnlyValueTest()
    {
      var tuple = Tuple.Create<TimeOnly, TimeOnly, TimeOnly, TimeOnly>(
        TimeOnly.MinValue,
        TimeOnly.MaxValue,
        new TimeOnly(12,13,14,152,168),
        new TimeOnly(12132_3456789));
      TestTuple(tuple);
    }

    [Test]
    public void DateTimeOffsetValueTest()
    {
      var tuple = Tuple.Create<DateTimeOffset, DateTimeOffset, DateTimeOffset, DateTimeOffset, DateTimeOffset, DateTimeOffset>(
        DateTimeOffset.MinValue,
        DateTimeOffset.MaxValue,
        DateTimeOffset.Now,
        DateTimeOffset.UtcNow,
        new DateTimeOffset(new DateTime(2018, 7, 15, 12, 13, 14, 152, 168), TimeSpan.FromHours(3)),
        new DateTimeOffset(new DateTime(1_000_000_000_000 + 1234567), TimeSpan.FromHours(2)));

      TestTuple(tuple);

    }

    [Test]
    public void GuidValueTest()
    {
      var tuple = Tuple.Create<Guid, Guid>(Guid.Empty, Guid.NewGuid());
      TestTuple(tuple);
    }

    [Test]
    public void ByteArrayValueTest()
    {
      var tuple = Tuple.Create<byte[], byte[]>(new byte[0], new byte[] { 2, 18, 35, 75, 92, 120, 150, 254 } );

      var descriptor = tuple.Descriptor;
      var stringTuple = tuple.Format();

      var parsedTuple = Tuple.Parse(descriptor, stringTuple);
      Assert.That(parsedTuple.Count, Is.EqualTo(tuple.Count));
      for (var i = 0; i < parsedTuple.Count; i++) {
        var original = (byte[]) tuple.GetValue(i, out var _);
        var parsed = (byte[]) parsedTuple.GetValue(i, out var _);

        Assert.That(original, Is.Not.Null);
        Assert.That(parsed, Is.Not.Null);
        Assert.That(parsed.SequenceEqual(parsed), Is.True);
      }
    }

    private static void TestTuple(RegularTuple tuple)
    {
      var descriptor = tuple.Descriptor;
      var stringTuple = tuple.Format();

      var parsedTuple = Tuple.Parse(descriptor, stringTuple);
      Assert.That(parsedTuple.Equals(tuple), Is.True);
    }
  }
}
