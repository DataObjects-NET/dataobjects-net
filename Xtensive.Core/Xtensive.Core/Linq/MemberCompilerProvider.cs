// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.09

using System;
using System.Linq;
using System.Reflection;
using Xtensive.Core.Collections;
using Xtensive.Core.Helpers;
using Xtensive.Core.Reflection;
using Xtensive.Core.Resources;
using Compilers = System.Collections.Generic.Dictionary<System.Reflection.MemberInfo,
  Xtensive.Core.Pair<System.Delegate, System.Reflection.MethodInfo>>;

namespace Xtensive.Core.Linq
{
  /// <summary>
  /// Default implementation of <see cref="IMemberCompilerProvider{T}"/>.
  /// This implementation is thread-safe.
  /// </summary>
  /// <typeparam name="T"><inheritdoc/></typeparam>
  public class MemberCompilerProvider<T> : LockableBase, IMemberCompilerProvider<T>
  {
    private const int MaxNumberOfCompilerParameters = 9;
    private volatile Compilers compilers = new Compilers();
    private readonly object syncRoot = new object();

    /// <inheritdoc/>
    public Func<T, T[], T> GetCompiler(MemberInfo source)
    {
      MethodInfo compiler;
      return GetCompiler(source, out compiler);
    }

    /// <inheritdoc/>
    public Func<T, T[], T> GetCompiler(MemberInfo source, out MethodInfo compiler)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");

      compiler = null;
      bool withMemberInfo = false;
      var realSource = source;

      var sourceProperty = realSource as PropertyInfo;
      if (sourceProperty != null) {
        source = realSource = sourceProperty.GetGetMethod();
        // GetGetMethod returns null in case of non public getter.
        if (source==null)
          return null;
      }

      var sourceMethod = realSource as MethodInfo;
      if (sourceMethod != null && sourceMethod.IsGenericMethod) {
        realSource = sourceMethod.GetGenericMethodDefinition();
        withMemberInfo = true;
      }

      var sourceType = realSource.ReflectedType;

      if (sourceType.IsGenericType) {
        sourceType = sourceType.GetGenericTypeDefinition();

        if (realSource is FieldInfo)
          realSource = sourceType.GetField(realSource.Name);
        else if (realSource is MethodInfo)
          realSource = FindBestMethod(sourceType.GetMethods(), (MethodInfo)realSource);
        else if (realSource is ConstructorInfo)
          realSource = FindBestMethod(sourceType.GetConstructors(), (ConstructorInfo)realSource);
        else
          realSource = null;
        withMemberInfo = true;
      }

      if (realSource==null)
        return null;

      Pair<Delegate, MethodInfo> result;

      if (!compilers.TryGetValue(realSource, out result))
        return null;

      compiler = result.Second;

      if (withMemberInfo) {
        var d = (Func<MemberInfo, T, T[], T>) result.First;
        return d.Bind(source);
      }

      return (Func<T, T[], T>) result.First;
    }

    /// <inheritdoc/>
    public Type ExpressionType { get { return typeof(T); } }

    /// <inheritdoc/>
    public void RegisterCompilers(Type typeWithCompilers)
    {
      RegisterCompilers(typeWithCompilers, ConflictHandlingMethod.ReportError);
    }

    /// <inheritdoc/>
    public void RegisterCompilers(Type typeWithCompilers, ConflictHandlingMethod conflictHandlingMethod)
    {
      ArgumentValidator.EnsureArgumentNotNull(typeWithCompilers, "typeWithCompilers");
      this.EnsureNotLocked();

      if (typeWithCompilers.IsGenericType)
        throw new InvalidOperationException(string.Format(
          Strings.ExTypeXShouldNotBeGeneric, typeWithCompilers.GetFullName(true)));

      var newCompilers = new Compilers();

      var allCompilers = typeWithCompilers.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
        .Where(mi => mi.IsDefined(typeof (CompilerAttribute), false) && !mi.IsGenericMethod);

      foreach (var compiler in allCompilers)
        RegisterCompiler(newCompilers, compiler);

      lock (syncRoot) {
        switch (conflictHandlingMethod) {
          case ConflictHandlingMethod.KeepOld:
            compilers = MergeCompilers(newCompilers, compilers);
            break;
          case ConflictHandlingMethod.Overwrite:
            compilers = MergeCompilers(compilers, newCompilers);
            break;
          case ConflictHandlingMethod.ReportError:
            var result = new Compilers(compilers);
              foreach (var pair in newCompilers) {
                if (result.ContainsKey(pair.Key))
                  throw new InvalidOperationException(string.Format(
                    Strings.ExCompilerForXIsAlreadyRegistered,
                    pair.Key.GetFullName(true)));
                result.Add(pair.Key, pair.Value);
              }
            compilers = result;
          break;
        }
      }
    }

    #region Private methods

    private static Compilers MergeCompilers(Compilers first, Compilers second)
    {
      var result = new Compilers(first);
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
          .Zip(m.GetParameterTypes(), (t1, t2) => new { t1, t2 })
          .All(a => oneParamMatch(a.t1, a.t2));

      methods = methods.Where(allParamsMatch);

      if (methods.Count() > 1)
        return null;

      return methods.FirstOrDefault();
    }

    private static Type[] ExtractTypesAndValidate(MethodInfo compiler, bool requireMemberInfo)
    {
      var parameters = compiler.GetParameters();
      int length = parameters.Length;

      if (length > MaxNumberOfCompilerParameters)
        throw new InvalidOperationException(string.Format(
          Strings.ExCompilerXHasTooManyParameters,
          compiler.GetFullName(true)));

      if (length == 0 && requireMemberInfo)
        throw new InvalidOperationException(string.Format(
          Strings.ExCompilerXShouldHaveMemberInfoParameter,
          compiler.GetFullName(true)));

      if (compiler.ReturnType != typeof(T))
        throw new InvalidOperationException(string.Format(
          Strings.ExCompilerXShouldReturnY,
          compiler.GetFullName(true),
          typeof(T).GetFullName(true)));

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
            compiler.GetFullName(true), param.Name,
            requiredType.GetFullName(true)));

        var attr = (TypeAttribute)param
          .GetCustomAttributes(typeof(TypeAttribute), false).FirstOrDefault();

        result[i] = attr == null ? null : attr.Value;
      }

      return result;
    }

    private static void RegisterCompiler(Compilers newCompilers, MethodInfo compiler)
    {
      var attribute = compiler.GetAttribute<CompilerAttribute>(AttributeSearchOptions.InheritNone);

      bool isBadTargetType = attribute.TargetType == null
        || (attribute.TargetType.IsGenericType && !attribute.TargetType.IsGenericTypeDefinition);

      if (isBadTargetType)
        throw new InvalidOperationException(string.Format(
          Strings.ExCompilerXHasBadTargetType,
          compiler.GetFullName(true)));

      bool isStatic = (attribute.TargetKind & TargetKind.Static) != 0;
      bool isCtor = (attribute.TargetKind & TargetKind.Constructor) != 0;
      bool isPropertySetter = (attribute.TargetKind & TargetKind.PropertySet) != 0;
      bool isPropertyGetter = (attribute.TargetKind & TargetKind.PropertyGet) != 0;
      bool isField = (attribute.TargetKind & TargetKind.Field) != 0;
      bool isGenericMethod = attribute.GenericParamsCount > 0;
      bool isGenericType = attribute.TargetType.IsGenericType;
      bool isGeneric = isGenericType || isGenericMethod;
      
      string memberName = attribute.TargetMember;

      if (memberName.IsNullOrEmpty())
        if (isPropertyGetter || isPropertySetter)
          memberName = WellKnown.IndexerPropertyName;
        else if (isCtor)
          memberName = WellKnown.CtorName;
        else
          throw new InvalidOperationException(string.Format(
            Strings.ExCompilerXHasBadTargetMember,
            compiler.GetFullName(true)));

      var parameterTypes = ExtractTypesAndValidate(compiler, isGeneric);
      var bindingFlags = BindingFlags.Public;

      if (isGeneric)
        parameterTypes = parameterTypes.Skip(1).ToArray();

      if (isCtor)
        bindingFlags |= BindingFlags.Instance;
      else
        if (!isStatic) {
          if (parameterTypes.Length==0)
            throw new InvalidOperationException(string.Format(
              Strings.ExCompilerXShouldHaveThisParameter,
              compiler.GetFullName(true)));

          parameterTypes = parameterTypes.Skip(1).ToArray();
          bindingFlags |= BindingFlags.Instance;
        }
        else
          bindingFlags |= BindingFlags.Static;

      if (isPropertyGetter) {
        bindingFlags |= BindingFlags.GetProperty;
        memberName = WellKnown.GetterPrefix + memberName;
      }

      if (isPropertySetter) {
        bindingFlags |= BindingFlags.SetProperty;
        memberName = WellKnown.SetterPrefix + memberName;
      }

      MemberInfo memberInfo = null;
      bool specialCase = false;

      // handle stupid cast operator that may be overloaded by return type
      if (memberName == WellKnown.Operator.Explicit || memberName == WellKnown.Operator.Implicit) {
        var returnTypeAttribute = compiler.ReturnTypeCustomAttributes
          .GetCustomAttributes(typeof (TypeAttribute), false)
          .Cast<TypeAttribute>()
          .FirstOrDefault();

        if (returnTypeAttribute != null && returnTypeAttribute.Value != null) {
          memberInfo = attribute.TargetType.GetMethods()
          .Where(mi => mi.Name == memberName
                    && mi.IsStatic
                    && mi.ReturnType == returnTypeAttribute.Value)
          .FirstOrDefault();
          specialCase = true;
        }
      }
      
      if (!specialCase) {
        if (isCtor)
          memberInfo = attribute.TargetType.GetConstructor(bindingFlags, parameterTypes);
        else if (isField)
          memberInfo = attribute.TargetType.GetField(memberName, bindingFlags);
        else {
          // method / property getter / property setter
          var genericArgumentNames = isGenericMethod ? new string[attribute.GenericParamsCount] : null;
          memberInfo = attribute.TargetType
            .GetMethod(memberName, bindingFlags, genericArgumentNames, parameterTypes);
        }
      }

      if (memberInfo == null)
        throw new InvalidOperationException(string.Format(
          Strings.ExTargetMemberIsNotFoundForCompilerX,
          compiler.GetFullName(true)));

      if (newCompilers.ContainsKey(memberInfo))
        throw new InvalidOperationException(string.Format(
          Strings.ExCompilerForXIsAlreadyRegistered,
          memberInfo.GetFullName(true)));

      Delegate result;

      if (isGeneric)
        result = isStatic || isCtor ? CreateCompilerForStaticGenericMember(compiler)
                                    : CreateCompilerForInstanceGenericMember(compiler);
      else
        result = isStatic || isCtor ? CreateCompilerForStaticMember(compiler)
                                    : CreateCompilerForInstanceMember(compiler);

      newCompilers[memberInfo] = new Pair<Delegate, MethodInfo>(result, compiler);
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
        case 5:
          var d5 = DelegateHelper.CreateDelegate<Func<T, T, T, T, T, T>>(null, t, s);
          return (this_, arr) => d5(this_, arr[0], arr[1], arr[2], arr[3]);
        case 6:
          var d6 = DelegateHelper.CreateDelegate<Func<T, T, T, T, T, T, T>>(null, t, s);
          return (this_, arr) => d6(this_, arr[0], arr[1], arr[2], arr[3], arr[4]);
        case 7:
          var d7 = DelegateHelper.CreateDelegate<Func<T, T, T, T, T, T, T, T>>(null, t, s);
          return (this_, arr) => d7(this_, arr[0], arr[1], arr[2], arr[3], arr[4], arr[5]);
        case 8:
          var d8 = DelegateHelper.CreateDelegate<Func<T, T, T, T, T, T, T, T, T>>(null, t, s);
          return (this_, arr) => d8(this_, arr[0], arr[1], arr[2], arr[3], arr[4], arr[5], arr[6]);
        case 9:
          var d9 = DelegateHelper.CreateDelegate<Func<T, T, T, T, T, T, T, T, T, T>>(null, t, s);
          return (this_, arr) => d9(this_, arr[0], arr[1], arr[2], arr[3], arr[4], arr[5], arr[6], arr[7]);
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
        case 5:
          var d5 = DelegateHelper.CreateDelegate<Func<MemberInfo, T, T, T, T, T>>(null, t, s);
          return (mi, this_, arr) => d5(mi, this_, arr[0], arr[1], arr[2]);
        case 6:
          var d6 = DelegateHelper.CreateDelegate<Func<MemberInfo, T, T, T, T, T, T>>(null, t, s);
          return (mi, this_, arr) => d6(mi, this_, arr[0], arr[1], arr[2], arr[3]);
        case 7:
          var d7 = DelegateHelper.CreateDelegate<Func<MemberInfo, T, T, T, T, T, T, T>>(null, t, s);
          return (mi, this_, arr) => d7(mi, this_, arr[0], arr[1], arr[2], arr[3], arr[4]);
        case 8:
          var d8 = DelegateHelper.CreateDelegate<Func<MemberInfo, T, T, T, T, T, T, T, T>>(null, t, s);
          return (mi, this_, arr) => d8(mi, this_, arr[0], arr[1], arr[2], arr[3], arr[4], arr[5]);
        case 9:
        var d9 = DelegateHelper.CreateDelegate <Func<MemberInfo, T, T, T, T, T, T, T, T, T>>(null, t, s);
        return (mi, this_, arr) => d9(mi, this_, arr[0], arr[1], arr[2], arr[3], arr[4], arr[5], arr[6]);
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
        case 5:
          var d5 = DelegateHelper.CreateDelegate<Func<T, T, T, T, T, T>>(null, t, s);
          return (_, arr) => d5(arr[0], arr[1], arr[2], arr[3], arr[4]);
        case 6:
          var d6 = DelegateHelper.CreateDelegate<Func<T, T, T, T, T, T, T>>(null, t, s);
          return (_, arr) => d6(arr[0], arr[1], arr[2], arr[3], arr[4], arr[5]);
        case 7:
          var d7 = DelegateHelper.CreateDelegate<Func<T, T, T, T, T, T, T, T>>(null, t, s);
          return (_, arr) => d7(arr[0], arr[1], arr[2], arr[3], arr[4], arr[5], arr[6]);
        case 8:
          var d8 = DelegateHelper.CreateDelegate<Func<T, T, T, T, T, T, T, T, T>>(null, t, s);
          return (_, arr) => d8(arr[0], arr[1], arr[2], arr[3], arr[4], arr[5], arr[6], arr[7]);
        case 9:
          var d9 = DelegateHelper.CreateDelegate<Func<T, T, T, T, T, T, T, T, T, T>>(null, t, s);
          return (_, arr) => d9(arr[0], arr[1], arr[2], arr[3], arr[4], arr[5], arr[6], arr[7], arr[8]);
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
        case 5:
          var d5 = DelegateHelper.CreateDelegate<Func<MemberInfo, T, T, T, T, T>>(null, t, s);
          return (mi, _, arr) => d5(mi, arr[0], arr[1], arr[2], arr[3]);
        case 6:
          var d6 = DelegateHelper.CreateDelegate<Func<MemberInfo, T, T, T, T, T, T>>(null, t, s);
          return (mi, _, arr) => d6(mi, arr[0], arr[1], arr[2], arr[3], arr[4]);
        case 7:
          var d7 = DelegateHelper.CreateDelegate<Func<MemberInfo, T, T, T, T, T, T, T>>(null, t, s);
          return (mi, _, arr) => d7(mi, arr[0], arr[1], arr[2], arr[3], arr[4], arr[5]);
        case 8:
          var d8 = DelegateHelper.CreateDelegate<Func<MemberInfo, T, T, T, T, T, T, T, T>>(null, t, s);
          return (mi, _, arr) => d8(mi, arr[0], arr[1], arr[2], arr[3], arr[4], arr[5], arr[6]);
        case 9:
          var d9 = DelegateHelper.CreateDelegate<Func<MemberInfo, T, T, T, T, T, T, T, T, T>>(null, t, s);
          return (mi, _, arr) => d9(mi, arr[0], arr[1], arr[2], arr[3], arr[4], arr[5], arr[6], arr[7]);
      }

      return null;
    }

    #endregion


    // Constructors

    internal MemberCompilerProvider()
    {
    }
  }
}
