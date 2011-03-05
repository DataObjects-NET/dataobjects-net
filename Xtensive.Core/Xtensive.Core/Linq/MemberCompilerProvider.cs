// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.09

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Reflection;
using Xtensive.Helpers;
using Xtensive.Resources;
using AttributeSearchOptions = Xtensive.Reflection.AttributeSearchOptions;
using DelegateHelper = Xtensive.Reflection.DelegateHelper;
using WellKnown = Xtensive.Reflection.WellKnown;

namespace Xtensive.Linq
{
  /// <summary>
  /// Default implementation of <see cref="IMemberCompilerProvider{T}"/>.
  /// </summary>
  /// <typeparam name="T"><inheritdoc/></typeparam>
  public partial class MemberCompilerProvider<T> : LockableBase, IMemberCompilerProvider<T>
  {
    private const int MaxNumberOfCompilerParameters = 9;
    private volatile MemberCompilerCollection compilers = new MemberCompilerCollection();
    
    /// <inheritdoc/>
    public Func<T, T[], T> GetCompiler(MemberInfo source)
    {
      MethodInfo compilerMethod;
      return GetCompiler(source, out compilerMethod);
    }

    /// <inheritdoc/>
    public Func<T, T[], T> GetCompiler(MemberInfo source, out MethodInfo compilerMethod)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");

      compilerMethod = null;
      bool includeMemberInfo = false;
      var actualSource = source;

      var sourceProperty = actualSource as PropertyInfo;
      if (sourceProperty!=null) {
        source = actualSource = sourceProperty.GetGetMethod();
        // GetGetMethod returns null in case of non public getter.
        if (source==null)
          return null;
      }

      var sourceMethod = actualSource as MethodInfo;
      if (sourceMethod!=null && sourceMethod.IsGenericMethod) {
        actualSource = sourceMethod.GetGenericMethodDefinition();
        includeMemberInfo = true;
      }

      var sourceType = actualSource.ReflectedType;

      if (sourceType.IsGenericType) {
        sourceType = sourceType.GetGenericTypeDefinition();

        if (actualSource is FieldInfo)
          actualSource = sourceType.GetField(actualSource.Name);
        else if (actualSource is MethodInfo)
          actualSource = FindBestMethod(sourceType.GetMethods(), (MethodInfo) actualSource);
        else if (actualSource is ConstructorInfo)
          actualSource = FindBestMethod(sourceType.GetConstructors(), (ConstructorInfo) actualSource);
        else
          actualSource = null;
        includeMemberInfo = true;
      }

      if (actualSource==null)
        return null;

      Pair<Delegate, MethodInfo> result;

      if (!compilers.TryGetValue(actualSource, out result))
        return null;

      compilerMethod = result.Second;

      if (includeMemberInfo) {
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

      var newCompilers = new MemberCompilerCollection();

      var allCompilers = typeWithCompilers
        .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
        .Where(method => method.IsDefined(typeof (CompilerAttribute), false) && !method.IsGenericMethod);

      foreach (var compiler in allCompilers)
        RegisterCompiler(newCompilers, compiler);
      
      switch (conflictHandlingMethod) {
      case ConflictHandlingMethod.KeepOld:
        compilers = MergeCompilers(newCompilers, compilers);
        break;
      case ConflictHandlingMethod.Overwrite:
        compilers = MergeCompilers(compilers, newCompilers);
        break;
      case ConflictHandlingMethod.ReportError:
        var result = new MemberCompilerCollection(compilers);
        foreach (var pair in newCompilers) {
          if (result.ContainsKey(pair.Key))
            throw new InvalidOperationException(string.Format(
              Strings.ExCompilerForXIsAlreadyRegistered, pair.Key.GetFullName(true)));
          result.Add(pair.Key, pair.Value);
        }
        compilers = result;
        break;
      }
    }

    #region Private methods

    private static MemberCompilerCollection MergeCompilers(MemberCompilerCollection first, MemberCompilerCollection second)
    {
      var result = new MemberCompilerCollection(first);
      foreach (var pair in second)
        result[pair.Key] = pair.Value;
      return result;
    }

    private static bool ParameterTypeMatches(Type originalParameterType, Type candidateParameterType)
    {
      return originalParameterType.IsGenericParameter
        ? candidateParameterType==originalParameterType
        : (candidateParameterType.IsGenericParameter || originalParameterType==candidateParameterType);
    }

    private static bool AllParameterTypesMatch(
      IEnumerable<Type> originalParameterTypes, IEnumerable<Type> candidateParameterTypes)
    {
      return originalParameterTypes
        .Zip(candidateParameterTypes)
        .All(pair => ParameterTypeMatches(pair.First, pair.Second));
    }

    private static MethodBase FindBestMethod(MethodBase[] allCandidates, MethodBase originalMethod)
    {
      var originalParameterTypes = originalMethod.GetParameterTypes();

      var candidates = allCandidates
        .Where(candidate => candidate.Name==originalMethod.Name
          && candidate.GetParameters().Length==originalParameterTypes.Length
          && candidate.IsStatic==originalMethod.IsStatic)
        .ToArray();

      if (candidates.Length==0)
        return null;
      if (candidates.Length==1)
        return candidates[0];

      candidates = candidates
        .Where(candidate =>
          AllParameterTypesMatch(originalParameterTypes, candidate.GetParameterTypes()))
        .ToArray();

      if (candidates.Length!=1)
        return null;

      return candidates[0];
    }

    private static Type[] ExtractTypesAndValidate(MethodInfo compiler, bool requireMemberInfo)
    {
      var parameters = compiler.GetParameters();
      int length = parameters.Length;

      if (length > DelegateHelper.MaxNumberOfGenericDelegateParameters)
        throw new InvalidOperationException(string.Format(
          Strings.ExCompilerXHasTooManyParameters, compiler.GetFullName(true)));

      if (length == 0 && requireMemberInfo)
        throw new InvalidOperationException(string.Format(
          Strings.ExCompilerXShouldHaveMemberInfoParameter, compiler.GetFullName(true)));

      if (compiler.ReturnType != typeof(T))
        throw new InvalidOperationException(string.Format(
          Strings.ExCompilerXShouldReturnY, compiler.GetFullName(true), typeof(T).GetFullName(true)));

      var result = new Type[length];

      for (int i = 0; i < length; i++) {
        var parameter = parameters[i];
        var requiredType = typeof(T);

        if (requireMemberInfo && i == 0)
          requiredType = typeof(MemberInfo);

        if (parameter.ParameterType != requiredType)
          throw new InvalidOperationException(string.Format(
            Strings.ExCompilerXShouldHaveParameterYOfTypeZ,
            compiler.GetFullName(true), parameter.Name, requiredType.GetFullName(true)));

        var attribute = (TypeAttribute) parameter
          .GetCustomAttributes(typeof (TypeAttribute), false).FirstOrDefault();

        result[i] = attribute==null ? null : attribute.Value;
      }

      return result;
    }

    private static void RegisterCompiler(MemberCompilerCollection newCompilers, MethodInfo compiler)
    {
      var attribute = compiler.GetAttribute<CompilerAttribute>(AttributeSearchOptions.InheritNone);

      var targetType = Type.GetType(attribute.TargetTypeAssemblyQualifiedName, false);

      bool isBadTargetType = targetType == null
        || (targetType.IsGenericType && !targetType.IsGenericTypeDefinition);

      if (isBadTargetType)
        throw new InvalidOperationException(string.Format(
          Strings.ExCompilerXHasBadTargetType,
          compiler.GetFullName(true)));

      bool isStatic = (attribute.TargetKind & TargetKind.Static) != 0;
      bool isCtor = (attribute.TargetKind & TargetKind.Constructor) != 0;
      bool isPropertySetter = (attribute.TargetKind & TargetKind.PropertySet) != 0;
      bool isPropertyGetter = (attribute.TargetKind & TargetKind.PropertyGet) != 0;
      bool isField = (attribute.TargetKind & TargetKind.Field) != 0;
      bool isGenericMethod = attribute.NumberOfGenericArguments > 0;
      bool isGenericType = targetType.IsGenericType;
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
          memberInfo = targetType.GetMethods()
          .Where(mi => mi.Name == memberName
                    && mi.IsStatic
                    && mi.ReturnType == returnTypeAttribute.Value)
          .FirstOrDefault();
          specialCase = true;
        }
      }
      
      if (!specialCase) {
        if (isCtor)
          memberInfo = targetType.GetConstructor(bindingFlags, parameterTypes);
        else if (isField)
          memberInfo = targetType.GetField(memberName, bindingFlags);
        else {
          // method / property getter / property setter
          var genericArgumentNames = isGenericMethod ? new string[attribute.NumberOfGenericArguments] : null;
          memberInfo = targetType
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
        result = isStatic || isCtor
          ? CreateInvokerForStaticGenericCompiler(compiler)
          : CreateInvokerForInstanceGenericCompiler(compiler);
      else
        result = isStatic || isCtor
          ? CreateInvokerForStaticCompiler(compiler)
          : CreateInvokerForInstanceCompiler(compiler);

      newCompilers[memberInfo] = new Pair<Delegate, MethodInfo>(result, compiler);
    }

    #endregion


    // Constructors

    internal MemberCompilerProvider()
    {
    }
  }
}
