// Copyright (C) 2018 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2018.08.31

using System.Configuration;
using System.Reflection;

namespace Xtensive.Orm.Tests
{
  public static class AssemblyExtensions
  {
    public static System.Configuration.Configuration GetAssemblyConfiguration(this Assembly assembly)
    {
      return ConfigurationManager.OpenExeConfiguration(assembly.Location);
    }
  }
}