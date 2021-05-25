// Copyright (C) 2012-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2012.05.16

using NUnit.Framework;

namespace Xtensive.Orm.Tracking.Tests
{
  [TestFixture]
  public class ServiceRegistrationTest : TrackingTestBase
  {
    [Test]
    public void ShouldReturnInstanceOfTrackingMonitor()
    {
      var tm = Domain.Services.Get<IDomainTrackingMonitor>();
      Assert.IsNotNull(tm);
    }

    [Test]
    public void ShouldReturnSameInstanceOfTrackingMonitor()
    {
      var tm1 = Domain.Services.Get<IDomainTrackingMonitor>();
      var tm2 = Domain.Services.Get<IDomainTrackingMonitor>();
      Assert.AreSame(tm1, tm2);
    }
  }
}