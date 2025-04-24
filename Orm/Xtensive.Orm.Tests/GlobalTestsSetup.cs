// Copyright (C) 2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using NUnit.Framework;

namespace Xtensive.Orm.Tests
{
  [SetUpFixture]
  public class GlobalTestsSetup
  {
    [OneTimeSetUp]
    public void GlobalSetup()
    {
      TestConfiguration.Instance.InitAppContextSwitches();
    }

    [OneTimeTearDown]
    public void GlobalTeardown()
    {
    }   
  }
}