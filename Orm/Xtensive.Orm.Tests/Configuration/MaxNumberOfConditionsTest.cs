// Copyright (C) 2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using NUnit.Framework;

namespace Xtensive.Orm.Tests.Configuration
{
  [TestFixture]
  public class MaxNumberOfConditionsTest
  {
    [Test]
    public void NegativeNumberTest()
    {
      var config = DomainConfigurationFactory.Create();
      config.MaxNumberOfConditions = -1;
      _ = Assert.Throws<InvalidOperationException>(() => config.Lock());

      config = DomainConfigurationFactory.Create();
      config.MaxNumberOfConditions = -100;
      _ = Assert.Throws<InvalidOperationException>(() => config.Lock());
    }

    [Test]
    public void MaxNumberIsZeroTest()
    {
      var config = DomainConfigurationFactory.Create();
      config.MaxNumberOfConditions = 0;
      _ = Assert.Throws<InvalidOperationException>(() => config.Lock());
    }

    [Test]
    public void MaxNumberIsOneTest()
    {
      var config = DomainConfigurationFactory.Create();
      config.MaxNumberOfConditions = 1;
      _ = Assert.Throws<InvalidOperationException>(() => config.Lock());
    }

    [Test]
    public void EdgeValuesTest()
    {
      var config = DomainConfigurationFactory.Create();
      config.MaxNumberOfConditions = 2;
      Assert.DoesNotThrow(() => config.Lock());

      config = DomainConfigurationFactory.Create();
      config.MaxNumberOfConditions = 999;
      Assert.DoesNotThrow(() => config.Lock());
    }

    [Test]
    public void ExceededLimit()
    {
      var config = DomainConfigurationFactory.Create();
      config.MaxNumberOfConditions = 1000;
      _ = Assert.Throws<InvalidOperationException>(() => config.Lock());
    }
  }
}