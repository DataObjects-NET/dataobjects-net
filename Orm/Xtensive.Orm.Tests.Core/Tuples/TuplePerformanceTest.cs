using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Orm.Tests;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Tests.Core.Tuples
{
  [TestFixture]
  public class TuplePerformanceTest
  {
    private static readonly Type[] allFieldTypes =
      new Type[] {
        typeof (bool),
        typeof (byte),
        typeof (sbyte),
        typeof (char),
        typeof (short),
        typeof (int),
        typeof (long),
        typeof (float),
        typeof (double),
        typeof (Guid),
        typeof (DateTime),
        typeof (KeyValuePair<int, int>),
        typeof (string),
      };

    private static readonly Type[] shortFieldTypes =
      new Type[] {
        typeof (int),
      };

    private static readonly Type[] typicalFieldTypes =
      new Type[] {
        typeof (long),
        typeof (int),
        typeof (string),
      };

    [Test]
    public void BasicTest()
    {
      var t = Tuple.Create(TupleDescriptor.Create(new [] {
        typeof(string), typeof(int), typeof(string), 
        typeof(TimeSpan), typeof(string), typeof(string)
      }));
      t.SetValue(0, string.Empty);
      t.SetValue(2, "n\\a");
      t.SetValue(3, new TimeSpan());
      t.SetValue(4, null);
      t.SetValue(5, "null");

      var s = t.Format();
      var tt = Xtensive.Tuples.Tuple.Parse(t.Descriptor, s);

      Assert.AreEqual(t, tt);
    }


    [Test]
    [Explicit, Category("Performance")]
    public void GeneratorTest()
    {
      const int fieldCount = 10;
      const int iterationCount = 100;
      int iteration = 0;
      Random random = RandomManager.CreateRandom(SeedVariatorType.CallingMethod);
      using (new Measurement("Random Tuple generation", iterationCount)) {
        while (iteration++ < iterationCount) {
          var fieldTypes = new List<Type>();
          for (int i = 0; i < fieldCount; i++)
            fieldTypes.Add(allFieldTypes[random.Next(allFieldTypes.Length)]);
          var d = TupleDescriptor.Create(fieldTypes.ToArray());
          var tuple = Tuple.Create(d);
        }
      }
    }

    [Test]
    [Explicit, Category("Profile")]
    public void ProfileIntFieldAccessTest()
    {
      const int iterationCount = 10000000;
      TupleDescriptor descriptor = TupleDescriptor.Create(shortFieldTypes);
      Tuple tuple = Tuple.Create(descriptor);
      using (new Measurement("Tuple.SetValue", iterationCount))
        for (int i = 0; i < iterationCount; i++)
          tuple.SetValue(0, (object) i);
      using (new Measurement("Tuple.GetValue(_,_)", iterationCount))
        for (int i = 0; i < iterationCount; i++) {
          TupleFieldState state;
          tuple.GetValue(0, out state);
        }
      using (new Measurement("Tuple.SetValue<T>", iterationCount))
        for (int i = 0; i < iterationCount; i++)
          tuple.SetValue(0, i);
      using (new Measurement("Tuple.SetValue<T?>", iterationCount))
        for (int i = 0; i < iterationCount; i++)
          tuple.SetValue<int?>(0, i);
      using (new Measurement("Tuple.GetValue<T>", iterationCount))
        for (int i = 0; i < iterationCount; i++)
          tuple.GetValue<int>(0);
      using (new Measurement("Tuple.GetValue<T>(_,_)", iterationCount))
        for (int i = 0; i < iterationCount; i++) {
          TupleFieldState state;
          tuple.GetValue<int>(0, out state);
        }
      using (new Measurement("Tuple.GetValue<T?>(_,_)", iterationCount))
        for (int i = 0; i < iterationCount; i++) {
          TupleFieldState state;
          tuple.GetValue<int?>(0, out state);
        }
    }

    [Test]
    [Explicit, Category("Profile")]
    public void ProfileDecimalFieldAccessTest()
    {
      const int iterationCount = 10000000;
      TupleDescriptor descriptor = TupleDescriptor.Create<decimal>();
      Tuple tuple = Tuple.Create(descriptor);
      using (new Measurement("Tuple.SetValue", iterationCount))
        for (int i = 0; i < iterationCount; i++)
          tuple.SetValue(0, (object) (decimal) i);
      using (new Measurement("Tuple.GetValue(_,_)", iterationCount))
        for (int i = 0; i < iterationCount; i++) {
          TupleFieldState state;
          tuple.GetValue(0, out state);
        }
      using (new Measurement("Tuple.SetValue<T>", iterationCount))
        for (int i = 0; i < iterationCount; i++)
          tuple.SetValue(0, (decimal) i);
      using (new Measurement("Tuple.SetValue<T?>", iterationCount))
        for (int i = 0; i < iterationCount; i++)
          tuple.SetValue<decimal?>(0, i);
      using (new Measurement("Tuple.GetValue<T>", iterationCount))
        for (int i = 0; i < iterationCount; i++)
          tuple.GetValue<decimal>(0);
      using (new Measurement("Tuple.GetValue<T>(_,_)", iterationCount))
        for (int i = 0; i < iterationCount; i++) {
          TupleFieldState state;
          tuple.GetValue<decimal>(0, out state);
        }
      using (new Measurement("Tuple.GetValue<T?>(_,_)", iterationCount))
        for (int i = 0; i < iterationCount; i++) {
          TupleFieldState state;
          tuple.GetValue<decimal?>(0, out state);
        }
    }

    [Test]
    [Explicit, Category("Profile")]
    public void ProfileGuidFieldAccessTest()
    {
      const int iterationCount = 10000000;
      TupleDescriptor descriptor = TupleDescriptor.Create<Guid>();
      Tuple tuple = Tuple.Create(descriptor);
      using (new Measurement("Tuple.SetValue", iterationCount))
        for (int i = 0; i < iterationCount; i++)
          tuple.SetValue(0, Guid.Empty);
      using (new Measurement("Tuple.GetValue(_,_)", iterationCount))
        for (int i = 0; i < iterationCount; i++) {
          TupleFieldState state;
          tuple.GetValue(0, out state);
        }
      using (new Measurement("Tuple.SetValue<T>", iterationCount))
        for (int i = 0; i < iterationCount; i++)
          tuple.SetValue(0, Guid.Empty);
      using (new Measurement("Tuple.SetValue<T?>", iterationCount))
        for (int i = 0; i < iterationCount; i++)
          tuple.SetValue<Guid?>(0, Guid.Empty);
      using (new Measurement("Tuple.GetValue<T>", iterationCount))
        for (int i = 0; i < iterationCount; i++)
          tuple.GetValue<Guid>(0);
      using (new Measurement("Tuple.GetValue<T>(_,_)", iterationCount))
        for (int i = 0; i < iterationCount; i++) {
          TupleFieldState state;
          tuple.GetValue<Guid>(0, out state);
        }
      using (new Measurement("Tuple.GetValue<T?>(_,_)", iterationCount))
        for (int i = 0; i < iterationCount; i++) {
          TupleFieldState state;
          tuple.GetValue<Guid?>(0, out state);
        }
    }

    [Test]
    [Explicit, Category("Performance")]
    public void FieldAccessTest()
    {
      const int iterationCount = 10000000;
      TupleDescriptor descriptor = TupleDescriptor.Create(shortFieldTypes);
      Tuple tuple = Tuple.Create(descriptor);
      for (int i = 0; i < iterationCount; i++)
        tuple.SetValue(0, (object) i);
      for (int i = 0; i < iterationCount; i++) {
        TupleFieldState state;
        tuple.GetValue(0, out state);
      }

      for (int i = 0; i < iterationCount; i++)
        tuple.SetValue(0, i);
      for (int i = 0; i < iterationCount; i++)
        tuple.GetValue<int>(0);
      for (int i = 0; i < iterationCount; i++) {
        TupleFieldState state;
        tuple.GetValue<int>(0, out state);
      }
      for (int i = 0; i < iterationCount; i++) {
        TupleFieldState state;
        tuple.GetValue<int?>(0, out state);
      }

      using (new Measurement("Tuple.SetValue?", iterationCount))
        for (int i = 0; i < iterationCount; i++)
          tuple.SetValue(0, null);
      using (new Measurement("Tuple.SetValue", iterationCount))
        for (int i = 0; i < iterationCount; i++)
          tuple.SetValue(0, (object) i);
      using (new Measurement("Tuple.GetValue", iterationCount))
        for (int i = 0; i < iterationCount; i++)
          tuple.GetValue(0);
      using (new Measurement("Tuple.GetValue(_,_)", iterationCount))
        for (int i = 0; i < iterationCount; i++) {
          TupleFieldState state;
          tuple.GetValue(0, out state);
        }
      using (new Measurement("Tuple.GetValueOrDefault", iterationCount))
        for (int i = 0; i < iterationCount; i++)
          tuple.GetValueOrDefault(0);

      using (new Measurement("Tuple.SetValue<T>", iterationCount))
        for (int i = 0; i < iterationCount; i++)
          tuple.SetValue(0, i);
      using (new Measurement("Tuple.GetValue<T>", iterationCount))
        for (int i = 0; i < iterationCount; i++)
          tuple.GetValue<int>(0);
      using (new Measurement("Tuple.GetValue<T>(_,_)", iterationCount))
        for (int i = 0; i < iterationCount; i++) {
          TupleFieldState state;
          tuple.GetValue<int>(0, out state);
        }
      using (new Measurement("Tuple.GetValueOrDefault<T>", iterationCount))
        for (int i = 0; i < iterationCount; i++)
          tuple.GetValueOrDefault<int>(0);

      using (new Measurement("Tuple.SetValue<T?>", iterationCount))
        for (int i = 0; i < iterationCount; i++)
          tuple.SetValue(0, new int?(i));
      using (new Measurement("Tuple.GetValue<T?>", iterationCount))
        for (int i = 0; i < iterationCount; i++)
          tuple.GetValue<int?>(0);
      using (new Measurement("Tuple.GetValue<T?>(_,_)", iterationCount))
        for (int i = 0; i < iterationCount; i++) {
          TupleFieldState state;
          tuple.GetValue<int?>(0, out state);
        }
      using (new Measurement("Tuple.GetValueOrDefault<T?>", iterationCount))
        for (int i = 0; i < iterationCount; i++)
          tuple.GetValueOrDefault<int?>(0);
    }

    [Test]
    [Explicit, Category("Performance")]
    public void CreateExistingTupleTest()
    {
      const int iterationCount = 10000000;
      TupleDescriptor descriptor = TupleDescriptor.Create(typicalFieldTypes);
      Xtensive.Tuples.Tuple tuple = Xtensive.Tuples.Tuple.Create(descriptor);
      using (new Measurement("CreateNew", iterationCount))
        for (int i = 0; i < iterationCount; i++)
          tuple.CreateNew();
      using (new Measurement("TupleDescriptor.CreateTuple", iterationCount))
        for (int i = 0; i < iterationCount; i++)
          Xtensive.Tuples.Tuple.Create(descriptor);
    }

    [Test]
    [Explicit, Category("Performance")]
    public void CopyToTest()
    {
      const int iterationCount = 10000000;
      Random random = RandomManager.CreateRandom(SeedVariatorType.CallingMethod);
      Xtensive.Tuples.Tuple tuple = Xtensive.Tuples.Tuple.Create(10, 20, 234.456f, 2345.34534d, "aaaaaaaaaaa", DateTime.Now);
      var hashCode = tuple.GetHashCode();
      var copy = tuple.CreateNew();

      // Warmup
      for (int i = 0; i < iterationCount / 10; i++)
        tuple.CopyTo(copy);

      // Actual run
      TestHelper.CollectGarbage(true);
      // TestLog.Info("Get ready...");
      using (new Measurement("Copying tuples", MeasurementOptions.Log, iterationCount)) {
        for (int i = 0; i < iterationCount; i++)
          tuple.CopyTo(copy);
      }
    }

    [Test]
    [Explicit, Category("Performance")]
    public void EqualsTest()
    {
      const int iterationCount = 10000000;
      Random random = RandomManager.CreateRandom(SeedVariatorType.CallingMethod);
      var tuple = Xtensive.Tuples.Tuple.Create(10, 20, 234.456f, 2345.34534d, "aaaaaaaaaaa", DateTime.Now);
      var clone = tuple.Clone();
      Assert.AreEqual(tuple.GetHashCode(), clone.GetHashCode());
      var equals = AdvancedComparer<Xtensive.Tuples.Tuple>.System.Equals;

      // Warmup
      for (int i = 0; i < iterationCount / 10; i++) {
        tuple.Equals(clone);
        equals(tuple, clone);
      }

      // Actual run
      TestHelper.CollectGarbage(true);
      // TestLog.Info("Get ready...");
      using (new Measurement("Comparing using AdvancedComparer tuples", MeasurementOptions.Log, iterationCount))
        for (int i = 0; i < iterationCount; i++)
          equals(tuple, clone);
      using (new Measurement("Comparing using Equals tuples", MeasurementOptions.Log, iterationCount))
        for (int i = 0; i < iterationCount; i++)
          tuple.Equals(clone);
    }

    [Test]
    [Explicit, Category("Performance")]
    public void MemoryUsageTest()
    {
      const int iterationCount = 1000000;
      var descriptor = TupleDescriptor.Create(typicalFieldTypes);
      var tuple = Xtensive.Tuples.Tuple.Create(descriptor);
      var dummyTuple = new DummyTuple(descriptor);
      var tuplesList = new List<Xtensive.Tuples.Tuple>(iterationCount);

      int iteration = 0;
      using (new Measurement("DummyTuple memory usage", iterationCount))
        while (iteration++ <= iterationCount)
          tuplesList.Add(dummyTuple.CreateNew());
      tuplesList.Clear();
      TestHelper.CollectGarbage(true);
      iteration = 0;
      using (new Measurement("Tuple memory usage", iterationCount))
        while (iteration++ <= iterationCount)
          tuplesList.Add(tuple.CreateNew());
    }

    [Test]
    [Explicit, Category("Performance")]
    public void FormatTest()
    {
      const int iterationCount = 1000000;
      var output = new List<string>(iterationCount);

      var descriptor = TupleDescriptor.Create(typicalFieldTypes);
      var tuple = Tuple.Create(descriptor);
      tuple.SetValue(0, 123L);
      tuple.SetValue(2, "hello");

      using (new Measurement("Format", iterationCount))
        for (int i = 0; i < iterationCount; i++) {
          var formatted = tuple.Format();
          output.Add(formatted);
        }
    }

    [Test]
    [Explicit, Category("Performance")]
    public void ParseTest()
    {
      const int iterationCount = 1000000;
      var output = new List<Tuple>(iterationCount);

      var descriptor = TupleDescriptor.Create(typicalFieldTypes);
      var tuple = Tuple.Create(descriptor);
      tuple.SetValue(0, 123L);
      tuple.SetValue(2, "hello");
      var source = tuple.Format();

      using (new Measurement("Parse", iterationCount))
        for (int i = 0; i < iterationCount; i++) {
          var parsed = descriptor.Parse(source);
          output.Add(parsed);
        }
    }

    [Test]
    [Explicit, Category("Performance")]
    public void GeneratorAdvancedTest()
    {
      var types = new[] {
        typeof (int),
        typeof (long),
        typeof (float),
        typeof (double),
        typeof (string),
        typeof (DateTime),
        typeof (TimeSpan),
        typeof (byte[])
      };

      var sizeRandomizer = new Random();
      var typeRandomizer = new Random();

      const int maxSize = 30;
      const int runCount = 10000;

      using (new Measurement("Create tuple with random descriptor", runCount))
        for (int i = 0; i < runCount; i++) {
          var count = sizeRandomizer.Next(maxSize + 1);
          var tupleTypes = Enumerable.Repeat(0, count)
            .Select(_ => types[typeRandomizer.Next(types.Length)])
            .ToArray(count);
          var descriptor = TupleDescriptor.Create(tupleTypes);
          var tuple = Tuple.Create(descriptor);
        }
    }
  }
}
