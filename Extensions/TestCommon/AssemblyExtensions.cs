using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace TestCommon
{
  public static class AssemblyExtensions
  {
    public static Configuration GetAssemblyConfiguration(this Assembly assembly)
    {
      return ConfigurationManager.OpenExeConfiguration(assembly.Location);
    }
  }
}
