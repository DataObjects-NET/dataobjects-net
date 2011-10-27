// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.10.27

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Resources;

namespace Xtensive.Orm.Building.Builders
{
  internal sealed class MemberCompilerProviderBuilder
  {
    private readonly DomainConfiguration configuration;
    private readonly IEnumerable<Type> systemCompilerContainers;
    private readonly Dictionary<Type, IMemberCompilerProvider> memberCompilerProviders = new Dictionary<Type, IMemberCompilerProvider>();

    public static Dictionary<Type, IMemberCompilerProvider> Build(DomainConfiguration configuration, IEnumerable<Type> systemCompilerContainers)
    {
      ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");
      ArgumentValidator.EnsureArgumentNotNull(systemCompilerContainers, "systemCompilerContainers");
      var builder = new MemberCompilerProviderBuilder(configuration, systemCompilerContainers);
      builder.Build();
      return builder.memberCompilerProviders;
    }

    private void Build()
    {
      var userCompilerContainers = configuration.Types.CompilerContainers.ToList();
      RegisterContainers(systemCompilerContainers);
      RegisterContainers(userCompilerContainers);
      RegisterLinqExtensions();

      foreach (var provider in memberCompilerProviders.Values)
        provider.Lock();
    }

    private void RegisterLinqExtensions()
    {
      var linqExtensionProvider = (IMemberCompilerProvider<Expression>) GetProvider(typeof (Expression));
      var substitutions = configuration.LinqExtensions.Substitutions
        .Select(item => new KeyValuePair<MemberInfo, Func<MemberInfo, Expression, Expression[], Expression>>(item.Member, CreateCompilerFromSubstitution(item)));
      linqExtensionProvider.RegisterCompilers(substitutions);
      var compilers = configuration.LinqExtensions.Compilers
        .Select(item => new KeyValuePair<MemberInfo, Func<MemberInfo, Expression, Expression[], Expression>>(item.Member, item.Compiler));
      linqExtensionProvider.RegisterCompilers(compilers);
    }

    private void RegisterContainers(IEnumerable<Type> containers)
    {
      var groupings = containers
        .Select(container => new {
          Container = container,
          Attribute = GetCompilerContainerAttribute(container)
        })
        .GroupBy(item => item.Attribute.TargetType);

      foreach (var grouping in groupings) {
        var targetType = grouping.Key;
        var memberCompilerProvider = GetProvider(targetType);
        foreach (var item in grouping)
          memberCompilerProvider.RegisterCompilers(item.Container, item.Attribute.ConflictHandlingMethod);
      }
    }

    private IMemberCompilerProvider GetProvider(Type targetType)
    {
      IMemberCompilerProvider memberCompilerProvider;
      memberCompilerProviders.TryGetValue(targetType, out memberCompilerProvider);
      if (memberCompilerProvider==null) {
        memberCompilerProvider = MemberCompilerProviderFactory.Create(targetType);
        memberCompilerProviders.Add(targetType, memberCompilerProvider);
      }
      return memberCompilerProvider;
    }

    private static CompilerContainerAttribute GetCompilerContainerAttribute(Type container)
    {
      var attributes = container.GetCustomAttributes(typeof (CompilerContainerAttribute), false);
      if (attributes.Length==0)
        throw new InvalidOperationException(string.Format(
          Strings.ExCompilerContainerAttributeIsNotAppliedToTypeX, container.Name));
      return (CompilerContainerAttribute) attributes[0];
    }

    private static Func<MemberInfo, Expression, Expression[], Expression> CreateCompilerFromSubstitution(LinqExtensionRegistration registration)
    {
      bool isStatic;
      int expectedParametersCount;

      var member = registration.Member;
      var substitution = registration.Substitution;

      switch (member.MemberType) {
        case MemberTypes.Constructor:
          var ctor = (ConstructorInfo) member;
          isStatic = true;
          expectedParametersCount = ctor.GetParameters().Length;
          break;
        case MemberTypes.Field:
          var field = (FieldInfo) member;
          isStatic = field.IsStatic;
          expectedParametersCount = 0;
          break;
        case MemberTypes.Method:
          var method = (MethodInfo) member;
          isStatic = method.IsStatic;
          expectedParametersCount = method.GetParameters().Length;
          break;
        case MemberTypes.Property:
          var property = (PropertyInfo) member;
          var getMethod = property.GetGetMethod();
          var setMethod = property.GetSetMethod();
          isStatic = getMethod!=null && getMethod.IsStatic
            || setMethod!=null && setMethod.IsStatic;
          expectedParametersCount = 0;
          break;
        default:
          throw new InvalidOperationException(string.Format(Strings.ExMemberXIsNotSupported, member));
      }

      if (!isStatic)
        expectedParametersCount++;

      if (substitution.Parameters.Count!=expectedParametersCount)
        throw new InvalidOperationException(string.Format(
          Strings.ExpressionXShouldTakeYParameters, substitution, expectedParametersCount));

      if (isStatic)
        return (_, _this, parameters) => substitution.BindParameters(parameters);
      return (_, _this, parameters) => substitution.BindParameters(parameters.Prepend(_this));
    }


    // Constructors

    private MemberCompilerProviderBuilder(DomainConfiguration configuration, IEnumerable<Type> systemCompilerContainers)
    {
      this.configuration = configuration;
      this.systemCompilerContainers = systemCompilerContainers;
    }
  }
}