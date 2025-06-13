// Copyright (C) 2018 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2018.08.31

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Xtensive.Orm.Tests
{
  public static class AssemblyExtensions
  {
    private static readonly Type ObjectType = typeof(object);
    private static readonly string MainTestAsseblyNsPrefix = "Xtensive.Orm.Tests."; // keep the dot at the end 
    private static readonly byte[] ThisAssemblyPkt = typeof(AssemblyExtensions).Assembly.GetName().GetPublicKeyToken();
    
    private static readonly ConcurrentDictionary<Assembly, Type[]> TypesPerAssembly = new();
    private static readonly ConcurrentDictionary<char, int> XtensiveOrmTestsNsAlphabeticIndex = new();

    public static System.Configuration.Configuration GetAssemblyConfiguration(this Assembly assembly)
    {
      return ConfigurationManager.OpenExeConfiguration(assembly.Location);
    }

    public static IReadOnlyList<Type> GetTypesFromNamespaceCaching(this Assembly assembly, string @namespace)
    {
      if (string.IsNullOrWhiteSpace(@namespace))
        throw new ArgumentException("Namespace cannot be null, empty or contains only white spaces");

      // these two dummy mentions to not forget to sync filtration algorithm here and in the classes,
      // in particular BaseType property, if the property changed then this algorighm should be changed as well
      _ = nameof(Xtensive.IoC.ServiceTypeRegistrationProcessor.BaseType);
      _ = nameof(Xtensive.Orm.Configuration.DomainTypeRegistrationHandler.BaseType);

      var assemblyNameInfo = assembly.GetName();
      var isMainTestAssembly = assemblyNameInfo.Name == "Xtensive.Orm.Tests" && !ThisAssemblyPkt.Except(assemblyNameInfo.GetPublicKeyToken()).Any();

      var assemblyTypes = TypesPerAssembly.GetOrAdd(assembly, static (a, isMain) => {
        var allTypes = a.GetTypes();
        var list = new List<Type>(allTypes.Length);
        var currentIndex = 0;
        foreach (var t in allTypes) {
          // we ignore compiler generated types because usuallty they are
          // at the end of sorted types
          if (t.IsSubclassOf(ObjectType) && t.GetCustomAttribute<CompilerGeneratedAttribute>() == null) {
            list.Add(t);
            if (isMain) {
              if (t.Namespace != null && t.Namespace.StartsWith(MainTestAsseblyNsPrefix, StringComparison.Ordinal)) {
                var firstLetter = t.Namespace[MainTestAsseblyNsPrefix.Length];
                // main test library has 5000+ types, to not enumerate them every type from the beginning
                // we try to have parts by first letter
                _ = XtensiveOrmTestsNsAlphabeticIndex.TryAdd(firstLetter, currentIndex);
              }
            }
            currentIndex++;
          }
        }
        return list.ToArray();
      }, isMainTestAssembly);

      var range = FindRange(assemblyTypes, @namespace, isMainTestAssembly);
      return new ArraySegment<Type>(assemblyTypes, range.first, range.last - range.first + 1);
      

      //type.IsSubclassOf(BaseType) && (ns.IsNullOrEmpty() || (type.FullName.IndexOf(ns + ".", StringComparison.InvariantCulture) >= 0));
    }

    private static (int first, int last) FindRange(Type[] types, string ns, bool isMainAssembly)
    {
      const int windowSize = 10;

      var searchStart = (isMainAssembly)
        ? (ns.StartsWith(MainTestAsseblyNsPrefix))
          ? XtensiveOrmTestsNsAlphabeticIndex[ns[MainTestAsseblyNsPrefix.Length]]
          : 0 //types from root namespace
        : 0;

      // we rely on the fact that types are sorted by full name, that means types of same namespace are go one by one
      // which gives us to optimize search - we find first type that has desired namespace, then we try to find last one
      // and then we return the part of original array as result
      var firstHit = -1;
      var lastHit = -1;

      for (int headIndex = searchStart, count = types.Length ; headIndex < count; headIndex++) {
        var head = types[headIndex];
        if (head.FullName.IndexOf(ns + ".", StringComparison.InvariantCulture) >= 0) {
          firstHit = headIndex;
          break;
        }
      }

      var isOutOfRange = false;
      lastHit = firstHit;
      do {
        lastHit = lastHit + windowSize;
        if (lastHit > types.Length - 1) {
          lastHit = types.Length - 1;
        }
        var tail = types[lastHit];
        if (tail.FullName.IndexOf(ns + ".", StringComparison.InvariantCulture) < 0)
          isOutOfRange = true;
        if (lastHit < firstHit)
          throw new Exception("There is something strage in the neighborhood! :-)");
      }
      while (!isOutOfRange);

      for (int tailIndex = lastHit; tailIndex >= firstHit; tailIndex--) {
        var tail = types[tailIndex];
        lastHit = tailIndex;
        if (tail.FullName.IndexOf(ns + ".", StringComparison.InvariantCulture) >= 0) {
          break;
        }
      }

      return (firstHit, lastHit);
    }
  }

  public static class TypeRegistryExtensions
  {
    public static void RegisterCaching(this Xtensive.Orm.Configuration.DomainTypeRegistry _this, Assembly assembly, string @namespace)
    {
      foreach (var t in assembly.GetTypesFromNamespaceCaching(@namespace)) {
        _this.Register(t);
      }
    }
  }
}