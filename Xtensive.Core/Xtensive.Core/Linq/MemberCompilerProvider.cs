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
using Xtensive.Core.Helpers;
using Xtensive.Core.Reflection;
using Xtensive.Core.Resources;

namespace Xtensive.Core.Linq
{
  /// <summary>
  /// Default implementation of <see cref="IMemberCompilerProvider{T}"/>.
  /// </summary>
  /// <typeparam name="T"><inheritdoc/></typeparam>
  public partial class MemberCompilerProvider<T> : LockableBase, IMemberCompilerProvider<T>
  {
    private MemberCompilerCollection compilers = new MemberCompilerCollection();

    /// <inheritdoc/>
    public Type ExpressionType { get { return typeof(T); } }

    /// <inheritdoc/>
    public Delegate GetUntypedCompiler(MemberInfo target)
    {
      ArgumentValidator.EnsureArgumentNotNull(target, "target");

      var actualTarget = GetCanonicalMember(target);
      if (actualTarget == null)
        return null;
      var registration = compilers.Get(actualTarget);
      if (registration == null)
        return null;
      return registration.CompilerInvoker;
    }

    /// <inheritdoc/>
    public Func<T, T[], T> GetCompiler(MemberInfo target)
    {
      var compiler = (Func<MemberInfo, T, T[], T>) GetUntypedCompiler(target);
      return compiler.Bind(target);
    }

    /// <inheritdoc/>
    public void RegisterCompilers(Type compilerContainer)
    {
      RegisterCompilers(compilerContainer, ConflictHandlingMethod.Default);
    }

    /// <inheritdoc/>
    public void RegisterCompilers(Type compilerContainer, ConflictHandlingMethod conflictHandlingMethod)
    {
      ArgumentValidator.EnsureArgumentNotNull(compilerContainer, "compilerContainer");
      this.EnsureNotLocked();

      if (compilerContainer.IsGenericType)
        throw new InvalidOperationException(string.Format(
          Strings.ExTypeXShouldNotBeGeneric, compilerContainer.GetFullName(true)));

      var compilersToRegister = new MemberCompilerCollection();

      var compilerMethods = compilerContainer
        .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
        .Where(method => method.IsDefined(typeof (CompilerAttribute), false) && !method.IsGenericMethod);

      foreach (var compiler in compilerMethods)
        compilersToRegister.Add(ProcessCompiler(compiler));
      
      UpdateRegistry(compilersToRegister, conflictHandlingMethod);
    }

    /// <inheritdoc/>
    public void RegisterCompilers(IEnumerable<KeyValuePair<MemberInfo, Func<MemberInfo, T, T[], T>>> compilerDefinitions)
    {
      RegisterCompilers(compilerDefinitions, ConflictHandlingMethod.Default);
    }

    /// <inheritdoc/>
    public void RegisterCompilers(IEnumerable<KeyValuePair<MemberInfo, Func<MemberInfo, T, T[], T>>> compilerDefinitions, ConflictHandlingMethod conflictHandlingMethod)
    {
      ArgumentValidator.EnsureArgumentNotNull(compilerDefinitions, "compilerDefinitions");
      this.EnsureNotLocked();

      var newItems = new MemberCompilerCollection();
      foreach (var item in compilerDefinitions)
        newItems.Add(new MemberCompilerRegistration(GetCanonicalMember(item.Key), item.Value));
      UpdateRegistry(newItems, conflictHandlingMethod);
    }

    #region Private methods

    private void UpdateRegistry(MemberCompilerCollection newItems, ConflictHandlingMethod conflictHandlingMethod)
    {
      if (newItems.Count==0)
        return;
      switch (conflictHandlingMethod) {
        case ConflictHandlingMethod.KeepOld:
          newItems.MergeWith(compilers, false);
          compilers = newItems;
          break;
        case ConflictHandlingMethod.Overwrite:
          compilers.MergeWith(newItems, false);
          break;
        case ConflictHandlingMethod.ReportError:
          compilers.MergeWith(newItems, true);
          break;
      }
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

    private static Type[] ValidateCompilerParametersAndExtractTargetSignature(MethodInfo compiler, bool requireMemberInfo)
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

      if (requireMemberInfo)
        ValidateCompilerParameter(parameters[0], typeof (MemberInfo), compiler);

      var result = new Type[requireMemberInfo ? length - 1 : length];
      var regularParameters = requireMemberInfo ? parameters.Skip(1) : parameters;

      int i = 0;
      foreach (var parameter in regularParameters) {
        ValidateCompilerParameter(parameter, typeof(T), compiler);
        var attribute = (TypeAttribute) parameter.GetCustomAttributes(typeof (TypeAttribute), false).FirstOrDefault();
        result[i++] = attribute==null ? null : attribute.Value;
      }

      return result;
    }

    private static MemberCompilerRegistration ProcessCompiler(MethodInfo compiler)
    {
      var attribute = compiler.GetAttribute<CompilerAttribute>(AttributeSearchOptions.InheritNone);

      var targetType = Type.GetType(attribute.TargetTypeAssemblyQualifiedName, false);

      ValidateTargetType(targetType, compiler);

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
            Strings.ExCompilerXHasInvalidTargetType, compiler.GetFullName(true)));

      var parameterTypes = ValidateCompilerParametersAndExtractTargetSignature(compiler, isGeneric);
      var bindingFlags = BindingFlags.Public;

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

      MemberInfo targetMember = null;
      bool specialCase = false;

      // handle stupid cast operator that may be overloaded by return type
      if (memberName == WellKnown.Operator.Explicit || memberName == WellKnown.Operator.Implicit) {
        var returnTypeAttribute = compiler.ReturnTypeCustomAttributes
          .GetCustomAttributes(typeof (TypeAttribute), false)
          .Cast<TypeAttribute>()
          .FirstOrDefault();

        if (returnTypeAttribute != null && returnTypeAttribute.Value != null) {
          targetMember = targetType.GetMethods()
          .Where(mi => mi.Name == memberName
                    && mi.IsStatic
                    && mi.ReturnType == returnTypeAttribute.Value)
          .FirstOrDefault();
          specialCase = true;
        }
      }
      
      if (!specialCase) {
        if (isCtor)
          targetMember = targetType.GetConstructor(bindingFlags, parameterTypes);
        else if (isField)
          targetMember = targetType.GetField(memberName, bindingFlags);
        else {
          // method / property getter / property setter
          var genericArgumentNames = isGenericMethod ? new string[attribute.NumberOfGenericArguments] : null;
          targetMember = targetType
            .GetMethod(memberName, bindingFlags, genericArgumentNames, parameterTypes);
        }
      }

      if (targetMember == null)
        throw new InvalidOperationException(string.Format(
          Strings.ExTargetMemberIsNotFoundForCompilerX,
          compiler.GetFullName(true)));

      var invoker = CreateInvoker(compiler, isStatic || isCtor, isGeneric);
      return new MemberCompilerRegistration(targetMember, invoker);
    }

    private static Func<MemberInfo, T, T[], T> CreateInvoker(MethodInfo compiler, bool targetIsStaticOrCtor, bool targetIsGeneric)
    {
      if (targetIsGeneric)
        if (targetIsStaticOrCtor)
          return CreateInvokerForStaticGenericCompiler(compiler);
        else
          return CreateInvokerForInstanceGenericCompiler(compiler);
      else
        if (targetIsStaticOrCtor)
          return CreateInvokerForStaticCompiler(compiler);
        else
          return CreateInvokerForInstanceCompiler(compiler);
    }

    private static void ValidateTargetType(Type targetType, MethodInfo compiler)
    {
      bool isInvalidTargetType = targetType==null
        || (targetType.IsGenericType && !targetType.IsGenericTypeDefinition);

      if (isInvalidTargetType)
        throw new InvalidOperationException(string.Format(
          Strings.ExCompilerXHasInvalidTargetType, compiler.GetFullName(true)));
    }

    private static void ValidateCompilerParameter(ParameterInfo parameter, Type requiredType, MethodInfo compiler)
    {
      if (parameter.ParameterType != requiredType)
        throw new InvalidOperationException(string.Format(
          Strings.ExCompilerXShouldHaveParameterYOfTypeZ,
          compiler.GetFullName(true), parameter.Name, requiredType.GetFullName(true)));
    }

    private static MemberInfo GetCanonicalMember(MemberInfo member)
    {
      var canonicalMember = member;

      var sourceProperty = canonicalMember as PropertyInfo;
      if (sourceProperty!=null) {
        canonicalMember = sourceProperty.GetGetMethod();
        // GetGetMethod returns null in case of non public getter.
        if (canonicalMember==null)
          return null;
      }

      var sourceMethod = canonicalMember as MethodInfo;
      if (sourceMethod!=null && sourceMethod.IsGenericMethod)
        canonicalMember = sourceMethod.GetGenericMethodDefinition();

      var targetType = canonicalMember.ReflectedType;
      if (targetType.IsGenericType) {
        targetType = targetType.GetGenericTypeDefinition();
        if (canonicalMember is FieldInfo)
          canonicalMember = targetType.GetField(canonicalMember.Name);
        else if (canonicalMember is MethodInfo)
          canonicalMember = FindBestMethod(targetType.GetMethods(), (MethodInfo) canonicalMember);
        else if (canonicalMember is ConstructorInfo)
          canonicalMember = FindBestMethod(targetType.GetConstructors(), (ConstructorInfo) canonicalMember);
        else
          canonicalMember = null;
      }

      return canonicalMember;
    }

    #endregion


    // Constructors

    internal MemberCompilerProvider()
    {
    }
  }
}
