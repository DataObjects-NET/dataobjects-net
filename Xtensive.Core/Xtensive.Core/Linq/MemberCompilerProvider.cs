// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.09

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Helpers;
using Xtensive.Core.Reflection;
using Xtensive.Core.Resources;
using CompilerDictionary = System.Collections.Generic.Dictionary<System.Reflection.MemberInfo,
  Xtensive.Core.Pair<System.Delegate, System.Reflection.MethodInfo>>;

namespace Xtensive.Core.Linq
{
  /// <summary>
  /// Default implementation of <see cref="IMemberCompilerProvider{T}"/>.
  /// This implementation is thread-safe.
  /// </summary>
  /// <typeparam name="T"><inheritdoc/></typeparam>
  public class MemberCompilerProvider<T> : IMemberCompilerProvider<T>
  {
    private volatile CompilerDictionary compilersByMethodBase = new CompilerDictionary();
    private readonly object syncRoot = new object();

    /// <inheritdoc/>
    public Func<T, T[], T> GetCompiler(MemberInfo memberInfo)
    {
      MethodInfo compiler;
      return GetCompiler(memberInfo, out compiler);
    }

    /// <inheritdoc/>
    public Func<T, T[], T> GetCompiler(MemberInfo memberInfo, out MethodInfo compilerMethodInfo)
    {
      ArgumentValidator.EnsureArgumentNotNull(memberInfo, "memberInfo");
      
      compilerMethodInfo = null;
      bool withMemberInfo = false;
      var realMemberInfo = memberInfo;
      var methodInfo = realMemberInfo as MethodInfo;

      if (methodInfo != null && methodInfo.IsGenericMethod) {
        realMemberInfo = methodInfo.GetGenericMethodDefinition();
        withMemberInfo = true;
      }

      var type = realMemberInfo.ReflectedType;

      if (type.IsGenericType) {
        type = type.GetGenericTypeDefinition();

        if (memberInfo is FieldInfo)
          realMemberInfo = type.GetField(realMemberInfo.Name);
        else if (memberInfo is MethodInfo)
          realMemberInfo = FindBestMethod(type.GetMethods(), (MethodInfo)realMemberInfo);
        else if (memberInfo is ConstructorInfo)
          realMemberInfo = FindBestMethod(type.GetConstructors(), (ConstructorInfo)realMemberInfo);
        else
          realMemberInfo = null;
        withMemberInfo = true;
      }

      if (realMemberInfo==null)
        return null;

      Pair<Delegate, MethodInfo> resultPair;

      if (!compilersByMethodBase.TryGetValue(realMemberInfo, out resultPair))
        return null;

      compilerMethodInfo = resultPair.Second;

      if (withMemberInfo) {
        var d = (Func<MemberInfo, T, T[], T>) resultPair.First;
        return (this_, arr) => d(memberInfo, this_, arr);
      }

      return (Func<T, T[], T>)resultPair.First;
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
        throw new InvalidOperationException(string.Format(
          Strings.ExTypeXShouldNotBeGeneric, type.GetFullName(true)));

      var dict = new CompilerDictionary();

      var compilers = type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
        .Where(mi => mi.IsDefined(typeof (CompilerAttribute), false) && !mi.IsGenericMethod);

      foreach (var compiler in compilers)
        RegisterCompiler(dict, compiler);

      lock (syncRoot) {
        switch (conflictHandlingMethod) {
          case ConflictHandlingMethod.KeepOld:
            compilersByMethodBase = MergeDicts(dict, compilersByMethodBase);
            break;
          case ConflictHandlingMethod.Overwrite:
            compilersByMethodBase = MergeDicts(compilersByMethodBase, dict);
            break;
          case ConflictHandlingMethod.ReportError:
            var result = new CompilerDictionary(compilersByMethodBase);
              foreach (var pair in dict) {
                if (result.ContainsKey(pair.Key))
                  throw new InvalidOperationException(string.Format(
                    Strings.ExCompilerForXIsAlreadyRegistered,
                    pair.Key.GetFullName(true)));
                result.Add(pair.Key, pair.Value);
              }
            compilersByMethodBase = result;
          break;
        }
      }
    }

    #region private methods

    private static CompilerDictionary MergeDicts(CompilerDictionary first, CompilerDictionary second)
    {
      var result = new CompilerDictionary(first);
      foreach (var pair in second)
        result[pair.Key] = pair.Value;
      return result;
    }

    private static MethodBase FindBestMethod(MethodBase[] allMethods, MethodBase methodBase)
    {
      var methods = allMethods.Where(
        m => m.Name == methodBase.Name
          && m.GetParameters().Length == methodBase.GetParameters().Length
          && m.IsStatic == methodBase.IsStatic);

      if (methods.Count() == 1)
        return methods.First();

      var paramTypes = methodBase.GetParameterTypes();

      Func<Type, Type, bool> oneParamMatch =
        (l, r) => l.IsGenericParameter ? r == l : (r.IsGenericParameter || l == r);

      Func<MethodBase, bool> allParamsMatch =
        m => paramTypes
          .ZipWith(m.GetParameterTypes(), (t1, t2) => new { t1, t2 })
          .All(a => oneParamMatch(a.t1, a.t2));

      methods = methods.Where(allParamsMatch);

      if (methods.Count() > 1)
        return null;

      return methods.FirstOrDefault();
    }

    private static Type[] ExtractParamTypesAndValidate(MethodInfo methodInfo, bool requireMemberInfo)
    {
      int length = methodInfo.GetParameters().Length;

      if (length > 4)
        throw new InvalidOperationException(string.Format(
          Strings.ExCompilerXHasTooManyParameters,
          methodInfo.GetFullName(true)));

      if (length == 0 && requireMemberInfo)
        throw new InvalidOperationException(string.Format(
          Strings.ExCompilerXShouldHaveMemberInfoParameter,
          methodInfo.GetFullName(true)));

      if (methodInfo.ReturnType != typeof(T))
        throw new InvalidOperationException(string.Format(
          Strings.ExCompilerXShouldReturnY,
          methodInfo.GetFullName(true),
          typeof(T).GetFullName(true)));

      var parameters = methodInfo.GetParameters();
      var result = new Type[length];

      for (int i = 0; i < length; i++)
      {
        var param = parameters[i];
        var requiredType = typeof(T);

        if (requireMemberInfo && i == 0)
          requiredType = typeof(MemberInfo);

        if (param.ParameterType != requiredType)
          throw new InvalidOperationException(string.Format(
            Strings.ExCompilerXShouldHaveParameterYOfTypeZ,
            methodInfo.GetFullName(true), param.Name,
            requiredType.GetFullName(true)));

        var attr = (TypeAttribute)param
          .GetCustomAttributes(typeof(TypeAttribute), false).FirstOrDefault();

        result[i] = attr == null ? null : attr.Value;
      }

      return result;
    }

    private static void RegisterCompiler(CompilerDictionary dict, MethodInfo compiler)
    {
      var attr = compiler.GetAttribute<CompilerAttribute>(AttributeSearchOptions.InheritNone);

      bool isBadTargetType = attr.TargetType == null
        || (attr.TargetType.IsGenericType && !attr.TargetType.IsGenericTypeDefinition);

      if (isBadTargetType)
        throw new InvalidOperationException(string.Format(
          Strings.ExCompilerXHasBadTargetType,
          compiler.GetFullName(true)));

      bool isStatic = (attr.TargetKind & TargetKind.Static) != 0;
      bool isCtor = (attr.TargetKind & TargetKind.Constructor) != 0;
      bool isPropertySetter = (attr.TargetKind & TargetKind.PropertySet) != 0;
      bool isPropertyGetter = (attr.TargetKind & TargetKind.PropertyGet) != 0;
      bool isField = (attr.TargetKind & TargetKind.Field) != 0;
      bool isGenericMethod = attr.GenericParamsCount > 0;
      bool isGenericType = attr.TargetType.IsGenericType;
      bool isGeneric = isGenericType || isGenericMethod;

      string memberName = attr.TargetMember;

      if (memberName.IsNullOrEmpty())
        if (isPropertyGetter || isPropertySetter)
          memberName = WellKnown.IndexerPropertyName;
        else if (isCtor)
          memberName = WellKnown.CtorName;
        else
          throw new InvalidOperationException(string.Format(
            Strings.ExCompilerXHasBadTargetMember,
            compiler.GetFullName(true)));

      var paramTypes = ExtractParamTypesAndValidate(compiler, isGeneric);
      var bindFlags = BindingFlags.Public;

      if (isGeneric)
        paramTypes = paramTypes.Skip(1).ToArray();

      if (isCtor)
        bindFlags |= BindingFlags.Instance;
      else
        if (!isStatic) {
          if (paramTypes.Length==0)
            throw new InvalidOperationException(string.Format(
              Strings.ExCompilerXShouldHaveThisParameter,
              compiler.GetFullName(true)));

          paramTypes = paramTypes.Skip(1).ToArray();
          bindFlags |= BindingFlags.Instance;
        }
        else
          bindFlags |= BindingFlags.Static;

      if (isPropertyGetter) {
        bindFlags |= BindingFlags.GetProperty;
        memberName = WellKnown.GetterPrefix + memberName;
      }

      if (isPropertySetter) {
        bindFlags |= BindingFlags.SetProperty;
        memberName = WellKnown.SetterPrefix + memberName;
      }

      MemberInfo memberInfo;
      if (isCtor)
        memberInfo = attr.TargetType.GetConstructor(bindFlags, paramTypes);
      else if (isField)
        memberInfo = attr.TargetType.GetField(memberName, bindFlags);
      else {
        // method / property getter / property setter
        var genericArgNames = isGenericMethod ? new string[attr.GenericParamsCount] : null;
        memberInfo = attr.TargetType.GetMethod(memberName, bindFlags, genericArgNames, paramTypes);
      }

      if (memberInfo == null)
        throw new InvalidOperationException(string.Format(
          Strings.ExTargetMemberIsNotFoundForCompilerX,
          compiler.GetFullName(true)));

      if (dict.ContainsKey(memberInfo))
        throw new InvalidOperationException(string.Format(
          Strings.ExCompilerForXIsAlreadyRegistered,
          memberInfo.GetFullName(true)));

      Delegate result;

      if (isGeneric)
        result = isStatic ? CreateCompilerForStaticGenericMember(compiler)
                          : CreateCompilerForInstanceGenericMember(compiler);
      else
        result = isStatic ? CreateCompilerForStaticMember(compiler)
                          : CreateCompilerForInstanceMember(compiler);

      dict[memberInfo] = new Pair<Delegate, MethodInfo>(result, compiler);
    }

    private static Func<T, T[], T> CreateCompilerForInstanceMember(MethodInfo compiler)
    {
      var t = compiler.ReflectedType;
      string s = compiler.Name;

      switch (compiler.GetParameters().Length) {
        case 1:
          var d1 = DelegateHelper.CreateDelegate<Func<T, T>>(null, t, s);
          return (this_, arr) => d1(this_);
        case 2:
          var d2 = DelegateHelper.CreateDelegate<Func<T, T, T>>(null, t, s);
          return (this_, arr) => d2(this_, arr[0]);
        case 3:
          var d3 = DelegateHelper.CreateDelegate<Func<T, T, T, T>>(null, t, s);
          return (this_, arr) => d3(this_, arr[0], arr[1]);
        case 4:
          var d4 = DelegateHelper.CreateDelegate<Func<T, T, T, T, T>>(null, t, s);
          return (this_, arr) => d4(this_, arr[0], arr[1], arr[2]);
      }

      return null;
    }

    private static Func<MemberInfo, T, T[], T> CreateCompilerForInstanceGenericMember(MethodInfo compiler)
    {
      var t = compiler.ReflectedType;
      string s = compiler.Name;

      switch (compiler.GetParameters().Length) {
        case 2:
          var d2 = DelegateHelper.CreateDelegate<Func<MemberInfo, T, T>>(null, t, s);
          return (mi, this_, arr) => d2(mi, this_);
        case 3:
          var d3 = DelegateHelper.CreateDelegate<Func<MemberInfo, T, T, T>>(null, t, s);
          return (mi, this_, arr) => d3(mi, this_, arr[0]);
        case 4:
          var d4 = DelegateHelper.CreateDelegate<Func<MemberInfo, T, T, T, T>>(null, t, s);
          return (mi, this_, arr) => d4(mi, this_, arr[0], arr[1]);
      }

      return null;
    }

    private static Func<T, T[], T> CreateCompilerForStaticMember(MethodInfo compiler)
    {
      var t = compiler.ReflectedType;
      string s = compiler.Name;

      switch (compiler.GetParameters().Length) {
        case 0:
          var d0 = DelegateHelper.CreateDelegate<Func<T>>(null, t, s);
          return (_, arr) => d0();
        case 1:
          var d1 = DelegateHelper.CreateDelegate<Func<T, T>>(null, t, s);
          return (_, arr) => d1(arr[0]);
        case 2:
          var d2 = DelegateHelper.CreateDelegate<Func<T, T, T>>(null, t, s);
          return (_, arr) => d2(arr[0], arr[1]);
        case 3:
          var d3 = DelegateHelper.CreateDelegate<Func<T, T, T, T>>(null, t, s);
          return (_, arr) => d3(arr[0], arr[1], arr[2]);
        case 4:
          var d4 = DelegateHelper.CreateDelegate<Func<T, T, T, T, T>>(null, t, s);
          return (_, arr) => d4(arr[0], arr[1], arr[2], arr[3]);
      }

      return null;
    }

    private static Func<MemberInfo, T, T[], T> CreateCompilerForStaticGenericMember(MethodInfo compiler)
    {
      var t = compiler.ReflectedType;
      string s = compiler.Name;

      switch (compiler.GetParameters().Length) {
        case 1:
          var d1 = DelegateHelper.CreateDelegate<Func<MemberInfo, T>>(null, t, s);
          return (mi, _, arr) => d1(mi);
        case 2:
          var d2 = DelegateHelper.CreateDelegate<Func<MemberInfo, T, T>>(null, t, s);
          return (mi, _, arr) => d2(mi, arr[0]);
        case 3:
          var d3 = DelegateHelper.CreateDelegate<Func<MemberInfo, T, T, T>>(null, t, s);
          return (mi, _,arr) => d3(mi, arr[0], arr[1]);
        case 4:
          var d4 = DelegateHelper.CreateDelegate<Func<MemberInfo, T, T, T, T>>(null, t, s);
          return (mi, _, arr) => d4(mi, arr[0], arr[1], arr[2]);
      }

      return null;
    }
  }

  #endregion
}
