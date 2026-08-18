// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Tests.Model
{
  [TestFixture, Category("Model")]
  public abstract class DomainBuildabilityTest
  {
    [Test]
    public void DomainBuildTest()
    {
      var configuration = BuildConfiguration();
      Assert.DoesNotThrow(() => Domain.Build(configuration).Dispose());
    }

    [Test]
    public void DomainBuildAsyncTest()
    {
      var configuration = BuildConfiguration();
      Assert.DoesNotThrowAsync(async () => (await Domain.BuildAsync(configuration)).Dispose());
    }

    protected abstract DomainConfiguration BuildConfiguration();
  }
}
