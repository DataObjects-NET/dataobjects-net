// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.10

using System.Configuration;
using System.Reflection;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.IoC;

namespace Xtensive.Core.Tests
{
  /// <summary>
  /// Log for this namespace.
  /// </summary>
  public sealed class Log: LogTemplate<Log>
  {
    // Copy-paste this code!
    public static readonly string Name;
    
    static Log()
    {
      var section = (UnityConfigurationSection) ConfigurationManager.GetSection("Unity");
      var result = new UnityContainer();
      section.Containers.Default.Configure(result);
      ServiceLocator.SetLocatorProvider(() => new Microsoft.Practices.Unity.ServiceLocatorAdapter.UnityServiceLocator(result));

      string className = MethodInfo.GetCurrentMethod().DeclaringType.FullName;
      Name = className.Substring(0, className.LastIndexOf('.'));
    }
  }
}