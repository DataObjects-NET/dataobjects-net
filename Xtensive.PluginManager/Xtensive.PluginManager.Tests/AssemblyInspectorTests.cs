// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.06.15

using System;
using System.IO;
using System.Runtime.Serialization;
using NUnit.Framework;

namespace Xtensive.PluginManager.Tests
{
  [TestFixture]
  public class AssemblyInspectorTests
  {
    private AttributeActivator<SerializableAttribute> activator = new AttributeActivator<SerializableAttribute>();

    [Test]
    public void Test1()
    {
      int amount = AppDomain.CurrentDomain.GetAssemblies().Length;
      string[] files = Directory.GetFiles(@"C:\Debug", "*.dll", SearchOption.AllDirectories);

      AssemblyInspector ai = new AssemblyInspector();
      ai.TypeFound += new EventHandler<TypeFoundEventArgs>(ai_TargetFound);

      Console.WriteLine("TypeFilter delegate search");
      for (int i = 0, count = files.Length; i < count; i++) {
        ai.FindTypes(files[i], delegate(Type type, object filterCriteria) { return type.Name.Contains("Collection"); },
                     null);
      }

      Console.WriteLine("\nType-base search");
      for (int i = 0, count = files.Length; i < count; i++) {
        ai.FindTypes(files[i], typeof (Enum));
      }

      Console.WriteLine("\nInterface-based + Attribute search");
      for (int i = 0, count = files.Length; i < count; i++) {
        ai.FindTypes(files[i], typeof (IDeserializationCallback), typeof (SerializableAttribute));
      }

      if (amount < AppDomain.CurrentDomain.GetAssemblies().Length)
        throw new ApplicationException();
    }

    private void ai_TargetFound(object sender, TypeFoundEventArgs e)
    {
      Console.Out.WriteLine(
        string.Format("AssemblyName: {0}, Type: {1}", e.TypeInfo.AssemblyName.Name, e.TypeInfo.TypeName));
      if (e.TypeInfo.GetAttributes() != null) {
        Attribute a = activator.CreateInstance(e.TypeInfo.GetAttributes()[0]);
      }
    }
  }
}