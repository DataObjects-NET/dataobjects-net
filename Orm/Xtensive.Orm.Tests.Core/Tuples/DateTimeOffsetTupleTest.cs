// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using Xtensive.Tuples.Packed;
using Xtensive.Orm.Tests;

namespace Xtensive.Tuples
{
  [TestFixture]
  public sealed class DateTimeOffsetTupleTest
  {
    [Test]
    public void OnlyDateTimeOffsetFieldsTest()
    {
      var minLength = 30;
      var maxLength = 200;
      var iterationCount = 1000;
      var rnd = RandomManager.CreateRandom();
      var generator = InstanceGeneratorProvider.Default.GetInstanceGenerator<DateTimeOffset>();
      for (var i = 0; i < iterationCount; i++) {
        var count = rnd.Next(maxLength - minLength);

        var fields = new Type[count];
        var data = new DateTimeOffset[count];
        for (var j = 0; j < count; j++) {
          data[j] = generator.GetInstance(rnd);
          fields[j] = typeof(DateTimeOffset);
        }

        // Testing writes (untyped)
        var tuple1 = Tuple.Create(fields);
        Assert.That(tuple1, Is.InstanceOf<PackedTuple>());
        for (var j = 0; j < count; j++)
          tuple1.SetValue(j, (object) data[j]);
        // Testing reads (untyped)
        for (var j = 0; j < count; j++)
          Assert.AreEqual(data[j], tuple1.GetValue(j));

        // Testing writes (untyped)
        var tuple2 = Tuple.Create(fields);
        Assert.That(tuple2, Is.InstanceOf<PackedTuple>());
        for (var j = 0; j < count; j++)
          tuple2.SetValue(j, data[j]);
        for (var j = 0; j < count; j++)
          Assert.AreEqual(data[j], tuple2.GetValue<DateTimeOffset>(j));
      }
    }
  }
}
