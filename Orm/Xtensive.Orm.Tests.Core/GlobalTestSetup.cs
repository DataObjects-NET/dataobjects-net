// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using NUnit.Framework;
using Xtensive.Orm.Logging;

namespace Xtensive.Orm.Tests.Core
{
  [SetUpFixture]
  public class GlobalTestSetup
  {
    [OneTimeSetUp]
    public void RunBeforeAnyTests()
    {
      var configuraion = GetType().GetConfigurationForAssembly();
      LogManager.Default.Initialize(configuraion);
    }
  }
}
