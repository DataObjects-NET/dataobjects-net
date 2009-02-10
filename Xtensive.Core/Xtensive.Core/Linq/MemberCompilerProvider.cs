// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.09

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xtensive.Core.Helpers;
using Xtensive.Core.Reflection;

namespace Xtensive.Core.Linq
{
  /// <summary>
  /// Default implementation of <see cref="IMemberCompilerProvider{T}"/>.
  /// </summary>
  /// <typeparam name="T"><inheritdoc/></typeparam>
  public class MemberCompilerProvider<T> : IMemberCompilerProvider<T>
  {
    private volatile Dictionary<MethodInfo, Delegate> delegatesByMethodInfo
      = new Dictionary<MethodInfo, Delegate>();
    private readonly object syncRoot = new object();

    private Type[] ExtractParamTypesAndValidate(MethodInfo methodInfo, bool requireMethodInfo)
    {
      int length = methodInfo.GetParameters().Length;

      if (length > 4 || length==0 && requireMethodInfo)
        throw new InvalidOperationException();

      if (methodInfo.ReturnType != typeof(T))
        throw new InvalidOperationException();

      var parameters = methodInfo.GetParameters();
      var result = new Type[length];

      for (int i = 0; i < length; i++) {
        var param = parameters[i];
        var requiredType = typeof (T);

        if (requireMethodInfo && i == 0)
          requiredType = typeof (MethodInfo);

        if (param.ParameterType != requiredType)
          throw new InvalidOperationException();

        var attr = (ParamTypeAttribute)param
          .GetCustomAttributes(typeof(ParamTypeAttribute), false).FirstOrDefault();

        result[i] = attr==null ? null : attr.ParamType;

        if (result[i]!=null && result[i].ContainsGenericParameters)
          throw new InvalidOperationException();
      }

      return result;
    }

    private Dictionary<MethodInfo, Delegate> MergeDicts(Dictionary<MethodInfo, Delegate> first,
      Dictionary<MethodInfo, Delegate> second)
    {
      var result = new Dictionary<MethodInfo, Delegate>(first);
      foreach (var pair in second)
        result[pair.Key] = pair.Value;
      return result;
    }

    private Func<T[],T> CreateCompilerDelegate(MethodInfo compiler)
    {
      var t = compiler.ReflectedType;
      string s = compiler.Name;

      switch (compiler.GetParameters().Length) {
      case 0:
        var d0 = DelegateHelper.CreateDelegate<Func<T>>(null, t, s);
        return arr => d0();
      case 1:
        var d1 = DelegateHelper.CreateDelegate<Func<T, T>>(null, t, s);
        return arr => d1(arr[0]);
      case 2:
        var d2 = DelegateHelper.CreateDelegate<Func<T, T, T>>(null, t, s);
        return arr => d2(arr[0], arr[1]);
      case 3:
        var d3 = DelegateHelper.CreateDelegate<Func<T, T, T, T>>(null, t, s);
        return arr => d3(arr[0], arr[1], arr[2]);
      case 4:
        var d4 = DelegateHelper.CreateDelegate<Func<T, T, T, T, T>>(null, t, s);
        return arr => d4(arr[0], arr[1], arr[2], arr[3]);
      }

      return null;
    }

    private Func<MethodInfo, T[], T> CreateCompilerDelegateG(MethodInfo compiler)
    {
      var t = compiler.ReflectedType;
      string s = compiler.Name;

      switch (compiler.GetParameters().Length) {
      case 1:
        var d1 = DelegateHelper.CreateDelegate<Func<MethodInfo, T>>(null, t, s);
        return (mi, arr) => d1(mi);
      case 2:
        var d2 = DelegateHelper.CreateDelegate<Func<MethodInfo, T, T>>(null, t, s);
        return (mi, arr) => d2(mi, arr[0]);
      case 3:
        var d3 = DelegateHelper.CreateDelegate <Func<MethodInfo, T, T, T>>(null, t, s);
        return (mi, arr) => d3(mi, arr[0], arr[1]);
      case 4:
        var d4 = DelegateHelper.CreateDelegate<Func<MethodInfo, T, T, T, T>>(null, t, s);
        return (mi, arr) => d4(mi, arr[0], arr[1], arr[2]);
      }

      return null;
    }

    /// <inheritdoc/>
    public Func<T[], T> GetCompiler(MethodInfo methodInfo)
    {
      ArgumentValidator.EnsureArgumentNotNull(methodInfo, "methodInfo");

      bool withMethodInfo = false;

      var mi = methodInfo;
      
      if (mi.IsGenericMethod) {
        mi = mi.GetGenericMethodDefinition();
        withMethodInfo = true;
      }

      var type = mi.ReflectedType;

      if (type.IsGenericType) {
        type = type.GetGenericTypeDefinition();
        var maybeMethod = type.GetMethods()
          .Where(m => m.Name==mi.Name && m.GetParameters().Length==mi.GetParameters().Length)
          .FirstOrDefault();
        if (maybeMethod == null)
          return null;
        mi = maybeMethod;
        withMethodInfo = true;
      }

      Delegate d;
      if (!delegatesByMethodInfo.TryGetValue(mi, out d))
        return null;

      if (withMethodInfo) {
        var d1 = d as Func<MethodInfo, T[], T>;
        if (d1 == null)
          return null;
        return arr => d1(methodInfo, arr);
      }

      return d as Func<T[], T>;
    }

    /// <inheritdoc/>
    public void RegisterCompilers(Type type)
    {
      RegisterCompilers(type, ConflictHandlingMethod.ReportError);
    }

    /// <inheritdoc/>
    public void RegisterCompilers(Type type, ConflictHandlingMethod conflictHandlingMethod)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      if (type.IsGenericType)
        throw new ArgumentException();

      var dict = new Dictionary<MethodInfo, Delegate>();

      var compilers = type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
        .Where(mi => mi.IsDefined(typeof (CompilerAttribute), false) && !mi.IsGenericMethod);

      foreach (var compiler in compilers) {
        var attr = compiler.GetAttribute<CompilerAttribute>(AttributeSearchOptions.InheritNone);
        
        if (attr.TargetType==null || (attr.TargetType.IsGenericType && !attr.TargetType.IsGenericTypeDefinition))
          throw new InvalidOperationException();

        bool isStatic = (attr.TargetKind & TargetKind.Static) != 0;
        bool isGenericMethod = attr.GenericParamsCount > 0;
        bool isGenericType = attr.TargetType.IsGenericType;
        
        string methodName = attr.TargetMethod.IsNullOrEmpty() ? compiler.Name : attr.TargetMethod;

        var paramTypes = ExtractParamTypesAndValidate(compiler, isGenericMethod || isGenericType);
        var bindFlags = BindingFlags.Public;

        if (isGenericType || isGenericMethod)
          paramTypes = paramTypes.Skip(1).ToArray();

        if (!isStatic) {
          if (paramTypes.Length==0 || paramTypes[0]!=attr.TargetType)
            throw new InvalidOperationException();
          paramTypes = paramTypes.Skip(1).ToArray();
          bindFlags |= BindingFlags.Instance;
        }
        else
          bindFlags |= BindingFlags.Static;
        
        if ((attr.TargetKind & TargetKind.PropertyGet)!=0)
          bindFlags |= BindingFlags.GetProperty;

        if ((attr.TargetKind & TargetKind.PropertySet)!=0)
          bindFlags |= BindingFlags.SetProperty;

        var genericArgNames = isGenericMethod ? new string[attr.GenericParamsCount] : null;
        var methodInfo = attr.TargetType.GetMethod(methodName, bindFlags, genericArgNames, paramTypes);

        if (dict.ContainsKey(methodInfo))
          throw new InvalidOperationException();

        if (isGenericMethod || isGenericType)
          dict[methodInfo] = CreateCompilerDelegateG(compiler);
        else
          dict[methodInfo] = CreateCompilerDelegate(compiler);
       }
      
      lock (syncRoot) {
        switch (conflictHandlingMethod) {
        case ConflictHandlingMethod.KeepOld:
          delegatesByMethodInfo = MergeDicts(dict, delegatesByMethodInfo);
          break;
        case ConflictHandlingMethod.Overwrite:
          delegatesByMethodInfo = MergeDicts(delegatesByMethodInfo, dict);
          break;
        case ConflictHandlingMethod.ReportError:
          var result = new Dictionary<MethodInfo, Delegate>(delegatesByMethodInfo);
            foreach (var pair in dict) {
              if (result.ContainsKey(pair.Key))
                throw new InvalidOperationException();
              result.Add(pair.Key, pair.Value);
            }
          delegatesByMethodInfo = result;
          break;
        }
      }
    }
  }
}
