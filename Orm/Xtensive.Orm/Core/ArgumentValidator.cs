// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.01

using System;
using JetBrains.Annotations;
using Xtensive.Core;
using Xtensive.Comparison;


namespace Xtensive.Core
{
  /// <summary>
  /// Helper class validation most common error conditions.
  /// </summary>
  public static class ArgumentValidator
  {
    /// <summary>
    /// Ensures argument (<paramref name="value"/>) is not
    /// <see langoword="null"/>.
    /// </summary>
    /// <param name="value">Value to compare with <see langword="null"/>.</param>
    /// <param name="parameterName">Name of the method parameter.</param>
    public static void EnsureArgumentNotNull(object value, [InvokerParameterName] string parameterName)
    {
      if (value==null) {
        EnsureArgumentNotNullOrEmpty(parameterName, "parameterName");
        throw new ArgumentNullException(parameterName);
      }
    }
    
    /// <summary>
    /// Ensures argument (<paramref name="value"/>) is not
    /// <see langoword="null"/>.
    /// </summary>
    /// <param name="value">Value to compare with <see langword="null"/>.</param>
    /// <param name="parameterName">Name of the method parameter.</param>
    /// <typeparam name="T">The type of default value.</typeparam>
    public static void EnsureArgumentIsNotDefault<T>(T value, [InvokerParameterName] string parameterName)
    {
      if (default(T)==null) {
        if (value==null) {
          EnsureArgumentNotNullOrEmpty(parameterName, "parameterName");
          throw Exceptions.InvalidArgument(value, "parameterName");
        }
      }
      else if (AdvancedComparerStruct<T>.System.Equals(value, default(T))) {
        EnsureArgumentNotNullOrEmpty(parameterName, "parameterName");
        throw Exceptions.InvalidArgument(value, "parameterName");
      }
    }

    /// <summary>
    /// Ensures argument (<paramref name="value"/>) is not
    /// <see langoword="null"/> or <see cref="string.Empty"/> string.
    /// </summary>
    /// <param name="value">Value to check.</param>
    /// <param name="parameterName">Name of the method parameter.</param>
    public static void EnsureArgumentNotNullOrEmpty(string value, [InvokerParameterName] string parameterName)
    {
      if (value == null) {
        EnsureArgumentNotNullOrEmpty(parameterName, "parameterName");
        throw new ArgumentNullException(parameterName);
      }
      if (value.Length == 0) {
        EnsureArgumentNotNullOrEmpty(parameterName, "parameterName");
        throw new ArgumentException(Strings.ExArgumentCannotBeEmptyString, parameterName);
      }
    }

    /// <summary>
    /// Ensures argument (<paramref name="value"/>) is not <see langword="null"/> 
    /// and of <typeparamref name="T"/> type.
    /// </summary>
    /// <param name="value">Value to compare check.</param>
    /// <param name="parameterName">Name of the method parameter.</param>
    /// <typeparam name="T">The expected type of value.</typeparam>
    public static void EnsureArgumentIs<T>(object value, [InvokerParameterName] string parameterName)
    {
      EnsureArgumentNotNull(value, parameterName);
      if (!(value is T)) {
        EnsureArgumentNotNullOrEmpty(parameterName, "parameterName");
        throw new ArgumentException(string.Format(Strings.ExInvalidArgumentType, typeof(T)), parameterName);
      }
    }

    /// <summary>
    /// Ensures argument (<paramref name="value"/>) is not <see langword="null"/> 
    /// and of <paramref name="type"/> type.
    /// </summary>
    /// <param name="value">Value to compare check.</param>
    /// <param name="type">The expected type of value.</param>
    /// <param name="parameterName">Name of the method parameter.</param>
    public static void EnsureArgumentIs(object value, Type type, [InvokerParameterName] string parameterName)
    {
      EnsureArgumentNotNull(value, parameterName);
      if (!type.IsAssignableFrom(value.GetType())) {
        EnsureArgumentNotNullOrEmpty(parameterName, "parameterName");
        throw new ArgumentException(string.Format(Strings.ExInvalidArgumentType, type), parameterName);
      }
    }
    
    /// <summary>
    /// Ensures argument (<paramref name="value"/>) is either <see langword="null"/>,
    /// or of <typeparamref name="T"/> type.
    /// </summary>
    /// <param name="value">Value to compare check.</param>
    /// <param name="parameterName">Name of the method parameter.</param>
    /// <typeparam name="T">The expected type of value.</typeparam>
    public static void EnsureArgumentIsNullOr<T>(object value, [InvokerParameterName] string parameterName)
    {
      if (value==null)
        return;
      if (!(value is T)) {
        EnsureArgumentNotNullOrEmpty(parameterName, "parameterName");
        throw new ArgumentException(string.Format(Strings.ExInvalidArgumentType, typeof(T)), parameterName);
      }
    }

    /// <summary>
    /// Ensures argument (<paramref name="value"/>) is either <see langword="null"/>,
    /// or of <typeparamref name="T"/> type.
    /// </summary>
    /// <param name="value">Value to compare check.</param>
    /// <param name="lowerBoundary">Lower range boundary (inclusively).</param>
    /// <param name="upperBoundary">Upper range boundary (inclusively).</param>
    /// <param name="parameterName">Name of the method parameter.</param>
    /// <typeparam name="T">The type of value.</typeparam>
    public static void EnsureArgumentIsInRange<T>(T value, T lowerBoundary, T upperBoundary, [InvokerParameterName] string parameterName)
      where T: struct, IComparable<T>
    {
      if (value.CompareTo(lowerBoundary)<0 || value.CompareTo(upperBoundary)>0) {
        EnsureArgumentNotNullOrEmpty(parameterName, "parameterName");
        throw new ArgumentOutOfRangeException(parameterName, value,
          string.Format(Strings.ExArgumentShouldBeInRange, lowerBoundary, upperBoundary));
      }
    }

    /// <summary>
    /// Ensures argument (<paramref name="value"/>) is greater then the specified <paramref name="boundary"/> value.
    /// </summary>
    /// <param name="value">Value to compare check.</param>
    /// <param name="boundary">Value boundary.</param>
    /// <param name="parameterName">Name of the method parameter.</param>
    /// <typeparam name="T">The type of value.</typeparam>
    public static void EnsureArgumentIsGreaterThan<T>(T value, T boundary, [InvokerParameterName] string parameterName)
      where T: struct, IComparable<T>
    {
      if (value.CompareTo(boundary) > 0)
        return;
      EnsureArgumentNotNullOrEmpty(parameterName, "parameterName");
      throw new ArgumentOutOfRangeException(parameterName, value,
        string.Format(Strings.ExArgumentMustBeGreaterThanX, boundary));
    }

    /// <summary>
    /// Ensures argument (<paramref name="value"/>) is greater then or equal
    /// the specified <paramref name="boundary"/> value.
    /// </summary>
    /// <param name="value">Value to compare check.</param>
    /// <param name="boundary">Value boundary.</param>
    /// <param name="parameterName">Name of the method parameter.</param>
    /// <typeparam name="T">The type of value.</typeparam>
    public static void EnsureArgumentIsGreaterThanOrEqual<T>(T value, T boundary, [InvokerParameterName] string parameterName)
      where T: struct, IComparable<T>
    {
      if (value.CompareTo(boundary) >= 0)
        return;
      EnsureArgumentNotNullOrEmpty(parameterName, "parameterName");
      throw new ArgumentOutOfRangeException(parameterName, value,
        string.Format(Strings.ExArgumentMustBeGreaterThatOrEqualX, boundary));
    }

    /// <summary>
    /// Ensures argument (<paramref name="value"/>) is less then
    /// the specified <paramref name="boundary"/> value.
    /// </summary>
    /// <param name="value">Value to compare check.</param>
    /// <param name="boundary">Value boundary.</param>
    /// <param name="parameterName">Name of the method parameter.</param>
    /// <typeparam name="T">The type of value.</typeparam>
    public static void EnsureArgumentIsLessThan<T>(T value, T boundary, [InvokerParameterName] string parameterName)
      where T: struct, IComparable<T>
    {
      if (value.CompareTo(boundary) < 0)
        return;
      EnsureArgumentNotNullOrEmpty(parameterName, "parameterName");
      throw new ArgumentOutOfRangeException(parameterName, value,
        string.Format(Strings.ExArgumentMustBeLessThanX, boundary));
    }

    /// <summary>
    /// Ensures argument (<paramref name="value"/>) is less then or equal
    /// the specified <paramref name="boundary"/> value.
    /// </summary>
    /// <param name="value">Value to compare check.</param>
    /// <param name="boundary">Value boundary.</param>
    /// <param name="parameterName">Name of the method parameter.</param>
    /// <typeparam name="T">The type of value.</typeparam>
    public static void EnsureArgumentIsLessThanOrEqual<T>(T value, T boundary, [InvokerParameterName] string parameterName)
      where T: struct, IComparable<T>
    {
      if (value.CompareTo(boundary) <= 0)
        return;
      EnsureArgumentNotNullOrEmpty(parameterName, "parameterName");
      throw new ArgumentOutOfRangeException(parameterName, value,
        string.Format(Strings.ExArgumentMustBeLessThanOrEqualX, boundary));
    }
  }
}