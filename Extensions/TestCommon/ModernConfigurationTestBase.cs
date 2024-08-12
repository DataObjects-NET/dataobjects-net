using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace TestCommon
{
  public abstract class ModernConfigurationTestBase
  {
    protected enum ConfigTypes
    {
      Json,
      Xml,
    }

    protected IConfiguration configuration;

    protected abstract ConfigTypes ConfigFormat { get; }

    protected abstract void AddConfigurationFile(IConfigurationBuilder configurationBuilder);

    [OneTimeSetUp]
    public virtual void BeforeAllTestsSetUp()
    {
      var configurationBuilder = new ConfigurationBuilder();
      configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());
      AddConfigurationFile(configurationBuilder);
      configuration = (ConfigurationRoot) configurationBuilder.Build();
    }

    protected void IgnoreIfXml()
    {
      if (ConfigFormat == ConfigTypes.Xml)
        throw new IgnoreException("Not valid for Xml format");
    }
    protected void IgnoreIfJson()
    {
      if (ConfigFormat == ConfigTypes.Json)
        throw new IgnoreException("Not valid for JSON format");
    }

    protected IConfigurationSection GetAndCheckConfigurationSection(string sectionName)
    {
      var section = configuration.GetSection(sectionName);
      Assert.That(section, Is.Not.Null);
      return section;
    }
  }
}
