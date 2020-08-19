// Copyright (C) 2017-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Julian Mamokin
// Created:    2017.03.09

using NUnit.Framework;
using Xtensive.Orm.Upgrade;

namespace Xtensive.Orm.Tests.Upgrade
{
  public class DomainUpgradeModeExtensionsTest
  {
    [Test]
    public void SkipModeTest() =>
      Assert.That(DomainUpgradeMode.Skip.GetSqlWorkerTask() == SqlWorkerTask.ExtractMetadataTypes, Is.True);

    [Test]
    public void LegacySkip() =>
      Assert.That(DomainUpgradeMode.LegacySkip.GetSqlWorkerTask() == SqlWorkerTask.ExtractSchema, Is.True);

    [Test]
    public void LegacyValidate() =>
      Assert.That(DomainUpgradeMode.LegacySkip.GetSqlWorkerTask() == SqlWorkerTask.ExtractSchema, Is.True);

    [Test]
    public void ValidateTest()
    {
      var expectedResult = SqlWorkerTask.ExtractSchema | SqlWorkerTask.ExtractMetadata;
      Assert.That(DomainUpgradeMode.Validate.GetSqlWorkerTask()==expectedResult, Is.True);
    }

    [Test]
    public void PerformTest()
    {
      var expectedResult = SqlWorkerTask.ExtractSchema | SqlWorkerTask.ExtractMetadata;
      Assert.That(DomainUpgradeMode.Perform.GetSqlWorkerTask()==expectedResult, Is.True);
    }

    [Test]
    public void PerformSafelyTest()
    {
      var expectedResult = SqlWorkerTask.ExtractSchema | SqlWorkerTask.ExtractMetadata;
      Assert.That(DomainUpgradeMode.PerformSafely.GetSqlWorkerTask()==expectedResult, Is.True);
    }

    [Test]
    public void RecreateTest()
    {
      var expectedResult = SqlWorkerTask.ExtractSchema | SqlWorkerTask.DropSchema;
      Assert.That(DomainUpgradeMode.Recreate.GetSqlWorkerTask()==expectedResult, Is.True);
    }
  }
}
