// Copyright (C) 2008-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2008.08.27

using System;
using System.Text.Json;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Tests;
using Xtensive.Serialization.Json;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Tests.Core.Tuples
{
  [TestFixture]
  public class TupleSerializationTest
  {
    [Test]
    public void SerializationWithNullValuesTest()
    {
      var tuple = Tuple.Create(typeof (string), typeof (int));
      tuple.SetValue(0, null);
      tuple.SetValue(1, null);

      var clonedTuple = Cloner.CloneViaJsonSerialization(tuple, CreateJsonSettings());

      Assert.That(clonedTuple.Count, Is.EqualTo(tuple.Count));
      Assert.That(clonedTuple.Descriptor, Is.EqualTo(tuple.Descriptor));
      for (int i = 0, count = tuple.Count; i < count; i++) {
        var origValue = tuple.GetValue(i, out var origFState);
        var clonedValue = clonedTuple.GetValue(i, out var clonedFState);
        Assert.That(clonedValue, Is.EqualTo(origValue));
        Assert.That(clonedFState, Is.EqualTo(origFState));
      }
    }

    [Test]
    public void FastReadOnlyTupleTest()
    {
      var tuple = Tuple.Create<string, int, DateTime, double>("1", 2, new DateTime(2003, 4, 5, 6, 7, 8), 2.5);
      var fastTuple = new FastReadOnlyTuple(tuple);

      var jsonSerializerOptions = CreateJsonSettings();

      var clonedFastTuple = Cloner.CloneViaJsonSerialization(fastTuple, jsonSerializerOptions);

      Assert.That(clonedFastTuple.Count, Is.EqualTo(fastTuple.Count));
      Assert.That(clonedFastTuple.Descriptor, Is.EqualTo(fastTuple.Descriptor));
      for (int i = 0, count = fastTuple.Count; i < count; i++) {
        var origValue = fastTuple.GetValue(i, out var origFState);
        var clonedValue = clonedFastTuple.GetValue(i, out var clonedFState);
        Assert.That(clonedValue, Is.EqualTo(origValue));
        Assert.That(clonedFState, Is.EqualTo(origFState));
      }
    }

    [Test]
    public void PackedTupleTest()
    {
      var tuple = Tuple.Create<string, int, DateTime, double>("1", 2, new DateTime(2003, 4, 5, 6, 7, 8), 2.5);

      var jsonSerializerOptions = CreateJsonSettings();

      var clonedTuple = Cloner.CloneViaJsonSerialization(tuple, jsonSerializerOptions);

      Assert.That(clonedTuple.Count, Is.EqualTo(tuple.Count));
      Assert.That(clonedTuple.Descriptor, Is.EqualTo(tuple.Descriptor));
      for (int i = 0, count = tuple.Count; i < count; i++) {
        var origValue = tuple.GetValue(i, out var origFState);
        var clonedValue = clonedTuple.GetValue(i, out var clonedFState);
        Assert.That(clonedValue, Is.EqualTo(origValue));
        Assert.That(clonedFState, Is.EqualTo(origFState));
      }
    }

    [Test]
    public void DifferentialTupleTest()
    {
      var origin = (Tuple)Tuple.Create<string, int, DateTime, double>("1", 2, new DateTime(2003, 4, 5, 6, 7, 8), 2.5);
      var diffTuple = new DifferentialTuple(origin);

      var jsonSerializerOptions = CreateJsonSettings();

      var clonedDiffTuple = Cloner.CloneViaJsonSerialization(diffTuple, jsonSerializerOptions);

      Assert.That(clonedDiffTuple.Count, Is.EqualTo(origin.Count));
      Assert.That(clonedDiffTuple.Difference, Is.Null);
      Assert.That(clonedDiffTuple.Descriptor, Is.EqualTo(origin.Descriptor));

      origin = diffTuple.Origin;
      var clonedOrigin = clonedDiffTuple.Origin;
      Assert.That(clonedOrigin.Count, Is.EqualTo(origin.Count));
      Assert.That(clonedOrigin.Descriptor, Is.EqualTo(origin.Descriptor));

      for (int i = 0, count = origin.Count; i < count; i++) {
        var origValue = origin.GetValue(i, out var origFState);
        var clonedValue = clonedOrigin.GetValue(i, out var clonedFState);
        Assert.That(clonedValue, Is.EqualTo(origValue));
        Assert.That(clonedFState, Is.EqualTo(origFState));
      }


      for (int i = 0, count = origin.Count; i < count; i++) {
        var origValue = origin.GetValue(i, out var origFState);
        var clonedValue = diffTuple.GetValue(i, out var clonedFState);
        Assert.That(clonedValue, Is.EqualTo(origValue));
        Assert.That(clonedFState, Is.EqualTo(origFState));
      }

      diffTuple.SetValue(2, new DateTime(2010, 4, 5, 6, 7, 8));

      clonedDiffTuple = Cloner.CloneViaJsonSerialization(diffTuple, jsonSerializerOptions);

      Assert.That(clonedDiffTuple.Count, Is.EqualTo(origin.Count));
      Assert.That(clonedDiffTuple.Difference, Is.Not.Null);
      Assert.That(clonedDiffTuple.Descriptor, Is.EqualTo(origin.Descriptor));

      origin = diffTuple.Origin;
      clonedOrigin = clonedDiffTuple.Origin;
      Assert.That(clonedOrigin.Count, Is.EqualTo(origin.Count));
      Assert.That(clonedOrigin.Descriptor, Is.EqualTo(origin.Descriptor));

      for (int i = 0, count = origin.Count; i < count; i++) {
        var origValue = origin.GetValue(i, out var origFState);
        var clonedValue = clonedOrigin.GetValue(i, out var clonedFState);
        Assert.That(clonedValue, Is.EqualTo(origValue));
        Assert.That(clonedFState, Is.EqualTo(origFState));
      }

      for (int i = 0, count = origin.Count; i < count; i++) {
        var origValue = diffTuple.GetValue(i, out var origFState);
        var clonedValue = diffTuple.GetValue(i, out var clonedFState);
        Assert.That(clonedValue, Is.EqualTo(origValue));
        Assert.That(clonedFState, Is.EqualTo(origFState));
      }
      var diff = diffTuple.Difference;
      var clonedDiff = clonedDiffTuple.Difference;
      Assert.That(clonedDiff.Count, Is.EqualTo(diff.Count));
      Assert.That(clonedDiff.Descriptor, Is.EqualTo(diff.Descriptor));

      for (int i = 0, count = diff.Count; i < count; i++) {
        var origValue = diff.GetValue(i, out var origFState);
        var clonedValue = clonedDiff.GetValue(i, out var clonedFState);
        Assert.That(clonedValue, Is.EqualTo(origValue));
        Assert.That(clonedFState, Is.EqualTo(origFState));
      }
    }

    private JsonSerializerOptions CreateJsonSettings()
    {
      var jsonSerializerOptions = new JsonSerializerOptions() {
        WriteIndented = true,
      };
      _ = jsonSerializerOptions.RegisterModelConverters();
      return jsonSerializerOptions;
    }
  }
}