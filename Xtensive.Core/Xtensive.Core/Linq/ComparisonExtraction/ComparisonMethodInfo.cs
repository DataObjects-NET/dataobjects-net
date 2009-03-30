// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.27

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xtensive.Core.Collections;

namespace Xtensive.Core.Linq.ComparisonExtraction
{
  internal class ComparisonMethodInfo
  {
    public readonly static SetSlim<MethodInfo> methodsCorrespondingToLike = new SetSlim<MethodInfo>();

    public readonly MethodInfo Method;

    public bool IsComplex { get; private set; }

    public readonly bool CorrespondsToLikeOperation;

    public static ReadOnlySet<MethodInfo> GetMethodsCorrespondingToLike()
    {
      return new ReadOnlySet<MethodInfo>(methodsCorrespondingToLike);
    }

    //It can recognize an implementation of IComparable<T> only, if the one matches the following pattern:
    //class ClassName : IComparable<ClassName>
    public bool Matches(MethodInfo sample)
    {
      if (sample == Method)
        return true;
      return Method.DeclaringType.IsInterface && InterfaceMethodMatches(sample);
    }

    private bool InterfaceMethodMatches(MethodInfo sample)
    {
      Type sampleInterface;
      if (Method.DeclaringType.IsGenericTypeDefinition)
        sampleInterface = GetGenericSampleInterface(sample.DeclaringType);
      else
        sampleInterface = GetSampleInterface(sample.DeclaringType);
      return ContainsTargetMethod(sampleInterface, sample);
    }

    private Type GetGenericSampleInterface(Type sampleType)
    {
      Type genericType;
      try {
        genericType = Method.DeclaringType.MakeGenericType(sampleType);
      }
      catch(ArgumentException) {
        return null;
      }
      return sampleType.GetInterfaces().SingleOrDefault(i => i == genericType);
    }

    private Type GetSampleInterface(Type sampleType)
    {
      var declaringType = Method.DeclaringType;
      return sampleType.GetInterfaces().SingleOrDefault(i => i == declaringType);
    }

    private bool ContainsTargetMethod(Type sampleInterface, MethodInfo sample)
    {
      if (sampleInterface == null)
        return false;
      var mapping = sample.DeclaringType.GetInterfaceMap(sampleInterface);
      for (int i = 0; i < mapping.TargetMethods.Length; i++) {
        if (mapping.TargetMethods[i] == sample)
          if (mapping.InterfaceType.IsGenericType)
            return mapping.InterfaceType.GetGenericTypeDefinition().GetMethods().Contains(Method);
          else
            return mapping.InterfaceMethods[i] == Method;
      }
      return false;
    }

    private void EvaluateComplexity(MethodInfo method)
    {
      var parameterInfo = method.GetParameters();
      if (parameterInfo.Length == 0) {
        IsComplex = false;
        return;
      }
      IsComplex = !AllParametersHasEqualType(parameterInfo);
    }

    private bool AllParametersHasEqualType(ParameterInfo[] parameterInfo)
    {
      var referenceType = parameterInfo[0].ParameterType;
      for (int i = 1; i < parameterInfo.Length; i++) {
        if (parameterInfo[i].ParameterType != referenceType) {
          return false;
        }
      }
      return true;
    }

    private static void AddMethods(IEnumerable<MethodInfo> newMethods, string name)
    {
      foreach (var method in newMethods) {
        if(method.Name == name)
          methodsCorrespondingToLike.Add(method);
      }
    }

    //Constructors

    static ComparisonMethodInfo()
    {
      AddMethods(typeof(string).GetMethods(), "EndsWith");
      AddMethods(typeof(string).GetMethods(), "StartsWith");
    }

    public ComparisonMethodInfo(MethodInfo method)
    {
      ArgumentValidator.EnsureArgumentNotNull(method, "method");
      Method = method;
      if (method.DeclaringType == typeof(string))
        CorrespondsToLikeOperation = methodsCorrespondingToLike.Contains(method);
      else
        CorrespondsToLikeOperation = false;
      EvaluateComplexity(method);
    }
  }
}