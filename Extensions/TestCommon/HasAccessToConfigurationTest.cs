using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Configuration;

namespace TestCommon
{
  [TestFixture]
  public abstract class HasConfigurationAccessTest
  {
    public System.Configuration.Configuration Configuration
    {
      get { return GetConfigurationForTestAssembly(); }
    }

    private System.Configuration.Configuration GetConfigurationForTestAssembly()
    {
      return GetType().Assembly.GetAssemblyConfiguration();
    }
  }
}
