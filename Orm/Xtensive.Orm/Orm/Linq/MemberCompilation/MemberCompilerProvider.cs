// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.02.09

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Reflection;
using AttributeSearchOptions = Xtensive.Reflection.AttributeSearchOptions;
using DelegateHelper = Xtensive.Reflection.DelegateHelper;

namespace Xtensive.Orm.Linq.MemberCompilation
{
  internal partial class MemberCompilerProvider<T> : LockableBase, IMemberCompilerProvider<T>
  {
    private readonly struct CompilerKey : IEquatable<CompilerKey>
    {
      private readonly Module module;
      private readonly int metadataToken;

      public bool Equals(CompilerKey other) => metadataToken == other.metadataToken
        && (ReferenceEquals(module, other.module) || module == other.module);

      public override bool Equals(object obj) => obj is CompilerKey other && Equals(other);

      public override int GetHashCode()
      {
        unchecked {
          return module == null ? metadataToken : (module.GetHashCode() * 397) ^ metadataToken;
        }
      }

      public CompilerKey(MemberInfo memberInfo)
      {
        module = memberInfo.Module;
        metadataToken = memberInfo.MetadataToken;
      }
    }

    private readonly Dictionary<CompilerKey, Delegate> compilers
      = new Dictionary<CompilerKey, Delegate>();

    public Type ExpressionType => typeof(T);

    public Delegate GetUntypedCompiler(MemberInfo target)
    {
      ArgumentValidator.EnsureArgumentNotNull(target, nameof(target));

      return compilers.GetValueOrDefault(GetCompilerKey(target));
    }

    public Func<T, T[], T> GetCompiler(MemberInfo target)
    {
      var compiler = (Func<MemberInfo, T, T[], T>) GetUntypedCompiler(target);
      return compiler.Bind(target);
    }

    public void RegisterCompilers(Type compilerContainer)
    {
      RegisterCompilers(compilerContainer, ConflictHandlingMethod.Default);
    }

    public void RegisterCompilers(Type compilerContainer, ConflictHandlingMethod conflictHandlingMethod)
    {
      ArgumentValidator.EnsureArgumentNotNull(compilerContainer, "compilerContainer");
      EnsureNotLocked();

      if (compilerContainer.IsGenericType)
        throw new InvalidOperationException(string.Format(
          Strings.ExTypeXShouldNotBeGeneric, compilerContainer.GetFullName(true)));

      var compilerMethods = compilerContainer
        .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
        .Where(method => method.IsDefined(typeof (CompilerAttribute), false) && !method.IsGenericMethod);

      UpdateRegistry(compilerMethods.Select(ProcessCompiler), conflictHandlingMethod);
    }

    public void RegisterCompilers(IEnumerable<KeyValuePair<MemberInfo, Func<MemberInfo, T, T[], T>>> compilerDefinitions)
    {
      RegisterCompilers(compilerDefinitions, ConflictHandlingMethod.Default);
    }

    public void RegisterCompilers(IEnumerable<KeyValuePair<MemberInfo, Func<MemberInfo, T, T[], T>>> compilerDefinitions, ConflictHandlingMethod conflictHandlingMethod)
    {
      ArgumentValidator.EnsureArgumentNotNull(compilerDefinitions, "compilerDefinitions");
      EnsureNotLocked();

      var newItems = compilerDefinitions.Select(item => (item.Key, (Delegate) item.Value));
      UpdateRegistry(newItems, conflictHandlingMethod);
    }

    #region Private methods

    private void UpdateRegistry(
      IEnumerable<(MemberInfo targetMember, Delegate compiler)> newRegistrations, ConflictHandlingMethod conflictHandlingMethod)
    {
      foreach (var (targetMember, compiler) in newRegistrations) {
        var key = GetCompilerKey(targetMember);
        if (!compilers.TryAdd(key, compiler)) {
          switch (conflictHandlingMethod) {
            case ConflictHandlingMethod.Overwrite:
              compilers[key] = compiler;
              break;
            case ConflictHandlingMethod.ReportError:
              throw new InvalidOperationException(string.Format(Strings.ExCompilerForXIsAlreadyRegistered, targetMember.GetFullName(true)));
          }
        }
      }
    }

    private static MethodBase GetCanonicalMethod(MethodBase inputMethod, MethodBase[] possibleCanonicalMethods)
    {
      foreach (var candidate in possibleCanonicalMethods) {
        if (inputMethod.MetadataToken == candidate.MetadataToken
          && (ReferenceEquals(inputMethod.Module, candidate.Module) || inputMethod.Module == candidate.Module)) {
          return candidate;
        }
      }

      return null;
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

    private static (MemberInfo targetMember, Delegate compilerInvoker) ProcessCompiler(MethodInfo compiler)
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
          memberName = Reflection.WellKnown.IndexerPropertyName;
        else if (isCtor)
          memberName = Reflection.WellKnown.CtorName;
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
        memberName = Reflection.WellKnown.GetterPrefix + memberName;
      }

      if (isPropertySetter) {
        bindingFlags |= BindingFlags.SetProperty;
        memberName = Reflection.WellKnown.SetterPrefix + memberName;
      }

      MemberInfo targetMember = null;
      bool specialCase = false;

      // handle stupid cast operator that may be overloaded by return type
      if (memberName == Reflection.WellKnown.Operator.Explicit || memberName == Reflection.WellKnown.Operator.Implicit) {
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
          targetMember = targetType.GetConstructorEx(bindingFlags, parameterTypes);
        else if (isField)
          targetMember = targetType.GetField(memberName, bindingFlags);
        else {
          // method / property getter / property setter
          var genericArgumentNames = isGenericMethod ? new string[attribute.NumberOfGenericArguments] : null;
          targetMember = targetType
            .GetMethodEx(memberName, bindingFlags, genericArgumentNames, parameterTypes);
        }
      }

      if (targetMember == null)
        throw new InvalidOperationException(string.Format(
          Strings.ExTargetMemberIsNotFoundForCompilerX,
          compiler.GetFullName(true)));

      var invoker = WrapInvoker(CreateInvoker(compiler, isStatic || isCtor, isGeneric));
      return (targetMember, invoker);
    }

    private static Func<MemberInfo, T, T[], T> WrapInvoker(Func<MemberInfo, T, T[], T> invoker)
    {
      return (member, instance, args) => {
        try {
          return invoker.Invoke(member, instance, args);
        }
        catch (Exception exception) {
          throw new TargetInvocationException(Strings.ExExceptionHasBeenThrownByTheUserMemberCompiler, exception);
        }
      };
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

    private static CompilerKey GetCompilerKey(MemberInfo member)
    {
      var canonicalMember = member;
      if (canonicalMember is PropertyInfo sourceProperty) {
        canonicalMember = sourceProperty.GetGetMethod();
        // GetGetMethod returns null in case of non public getter.
        if (canonicalMember is null) {
          return default;
        }
      }

      var targetType = canonicalMember.ReflectedType;
      if (targetType.IsGenericType) {
        targetType = targetType.GetGenericTypeDefinition();
        if (canonicalMember is FieldInfo)
          canonicalMember = targetType.GetField(canonicalMember.Name);
        else if (canonicalMember is MethodInfo methodInfo) {
          canonicalMember = GetCanonicalMethod(methodInfo, targetType.GetMethods());
        }
        else if (canonicalMember is ConstructorInfo constructorInfo)
          canonicalMember = GetCanonicalMethod(constructorInfo, targetType.GetConstructors());
        else
          canonicalMember = null;
      }

      if (canonicalMember == null) {
        return default;
      }

      if (targetType.IsEnum) {
        var declaringType = canonicalMember.DeclaringType;
        if (targetType != declaringType)
          canonicalMember = GetCanonicalMethod((MethodInfo) canonicalMember, declaringType.GetMethods());
        else
          canonicalMember = GetCanonicalMethod((MethodInfo) canonicalMember, targetType.GetMethods());
      }

      return new CompilerKey(canonicalMember);
    }

    #endregion
  }
}
