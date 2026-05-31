// Copyright (C) 2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.


using System.Text.Json;
using Xtensive.Serialization.Json;
using NUnit.Framework;

namespace Xtensive.Orm.Tests.Core
{
  [TestFixture]
  public class VersionInfoTest
  {
    [Test]
    public void SerializationTest()
    {
      var jsonSerializerOptions = CreateJsonSettings();

      var versionTuple1 = Xtensive.Tuples.Tuple.Create<long>(1);
      var versionInfo1 = new VersionInfo(versionTuple1);
      var clonedVersionInfo1 = Cloner.CloneViaJsonSerialization(versionInfo1, jsonSerializerOptions);
      Assert.That(clonedVersionInfo1.IsVoid, Is.False);
      Assert.That(clonedVersionInfo1.Equals(versionInfo1), Is.True);

      var versionTuple2 = Xtensive.Tuples.Tuple.Create<long, long>(1, 2);
      var versionInfo2 = new VersionInfo(versionTuple2);
      var clonedVersionInfo2 = Cloner.CloneViaJsonSerialization(versionInfo2, jsonSerializerOptions);
      Assert.That(clonedVersionInfo2.IsVoid, Is.False);
      Assert.That(clonedVersionInfo2.Equals(versionInfo2), Is.True);

      var versionTuple6 = Xtensive.Tuples.Tuple.Create<long, long, long, long, long, long>(1, 2, 3, 4, 5, 6);
      var versionInfo6 = new VersionInfo(versionTuple6);
      var clonedVersionInfo6 = Cloner.CloneViaJsonSerialization(versionInfo6, jsonSerializerOptions);
      Assert.That(clonedVersionInfo6.IsVoid, Is.False);
      Assert.That(clonedVersionInfo6.Equals(versionInfo6), Is.True);
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
