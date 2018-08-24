// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.07.03

using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Xtensive.Orm.Tests.Storage.Modules
{
  [TestFixture, Category("Upgrade")]
  [Explicit("Requires specific file path.")]
  public sealed class ModulesRegistrationTest
  {
    [OneTimeSetUp]
    public void TestFixtureSetUp()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
    }
    
    [Test]
    public void CombinedTest()
    {
      using (var domain0 = Domain.Build(BuildConfiguration0())) {
        using (var session = domain0.OpenSession())
        using (var tx = session.OpenTransaction()){
          Activator.CreateInstance(domain0.Configuration.Types.Single(t => t.Name=="Simple0"));
          tx.Complete();
        }
      }
      using (var domain1 = Domain.Build(BuildConfiguration1())) {
      }
      Type handler2Type = GetTypeFromAssembly("ModuleAssembly2", "Modules.Model.UpgradeHandler2");
      var handler2Count = (int)handler2Type.GetField("Count").GetValue(null);
      Assert.AreEqual(3, handler2Count);
      var handler2ModuleCount = (int)handler2Type.GetField("ModuleCount").GetValue(null);
      Assert.AreEqual(3, handler2ModuleCount);
      var module2Type = GetTypeFromAssembly("ModuleAssembly2", "Modules.Model.Module2");
      var module2ModuleCount = (int)module2Type.GetField("ModuleCount").GetValue(null);
      Assert.AreEqual(3, module2ModuleCount);
    }

    private static Type GetTypeFromAssembly(string assemblyName, string typeFullName)
    {
      return AppDomain.CurrentDomain.GetAssemblies()
        .Single(a => a.GetName().Name== assemblyName)
        .GetType(typeFullName);
    }

    private Xtensive.Orm.Configuration.DomainConfiguration BuildConfiguration0()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(CompileAssembly(0), "Modules.Model");
      return config;
    }

    private Xtensive.Orm.Configuration.DomainConfiguration BuildConfiguration1()
    {
      var config = DomainConfigurationFactory.Create();
      config.UpgradeMode = DomainUpgradeMode.Perform;
      config.Types.Register(CompileAssembly(1, 0), "Modules.Model");
      config.Types.Register(CompileAssembly(2, 0, 1), "Modules.Model");
      return config;
    }

    private Assembly CompileAssembly(int version, params int[] references)
    {
      var codeProvider = CodeDomProvider.CreateProvider("CSharp");
      var compilerParameters = new CompilerParameters();
      compilerParameters.OutputAssembly = GetModelAssemblyName(version);
      compilerParameters.GenerateInMemory = false;
      compilerParameters.IncludeDebugInformation = true;
      compilerParameters.CompilerOptions = "/define:_TEST_COMPILATION;DEBUG";
      foreach (var reference in references)
        compilerParameters.ReferencedAssemblies.Add(GetModelAssemblyName(reference));
      compilerParameters.ReferencedAssemblies.Add("System.dll");
      compilerParameters.ReferencedAssemblies
        .Add(@"C:\Program Files\Reference Assemblies\Microsoft\Framework\v3.5\System.Core.dll");
      compilerParameters.ReferencedAssemblies.Add(@"C:\Program Files\PostSharp 1.0\PostSharp.Laos.dll");
      compilerParameters.ReferencedAssemblies.Add(@"C:\Program Files\PostSharp 1.0\PostSharp.Public.dll");
      compilerParameters.ReferencedAssemblies.Add("..\\..\\..\\Lib\\Xtensive.Core.dll");
      compilerParameters.ReferencedAssemblies.Add("..\\..\\..\\Lib\\Xtensive.Core.Aspects.dll");
      compilerParameters.ReferencedAssemblies.Add("..\\..\\..\\Lib\\Xtensive.Indexing.dll");
      compilerParameters.ReferencedAssemblies.Add("..\\..\\..\\Lib\\Xtensive.Integrity.dll");
      compilerParameters.ReferencedAssemblies.Add("..\\..\\..\\Lib\\Xtensive.Storage.dll");
      compilerParameters.ReferencedAssemblies.Add("..\\..\\..\\Lib\\Xtensive.Storage.Model.dll");
      var result = codeProvider.CompileAssemblyFromFile(compilerParameters,
        String.Format("..\\..\\Storage\\Modules\\Assembly{0}.cs", version));
      Assert.AreEqual(0, result.Errors.Count);
      return Assembly.LoadFile(Directory.GetCurrentDirectory() + "\\" + result.PathToAssembly);
    }

    private static string GetModelAssemblyName(int version)
    {
      return String.Format("ModuleAssembly{0}.dll", version);
    }
  }
}