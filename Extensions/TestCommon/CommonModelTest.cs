// Copyright (C) 2019-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
using NUnit.Framework;
using TestCommon.Model;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests;

namespace TestCommon
{
  [TestFixture]
  public abstract class CommonModelTest : AutoBuildTest
  {
    private bool justBuilt = true;

    [SetUp]
    public virtual void SetUp()
    {
      if (justBuilt) {
        justBuilt = false;
      }
      else {
        RebuildDomain();
      }
    }


    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Bar).Assembly);
      return configuration;
    }
  }
}