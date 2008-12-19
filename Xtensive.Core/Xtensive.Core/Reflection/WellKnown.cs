// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.30

using System.Linq;

namespace Xtensive.Core.Reflection
{
  /// <summary>
  /// Various well-known constants related to this namespace.
  /// </summary>
  public static class WellKnown
  {
    /// <summary>
    /// Returns ".ctor".
    /// </summary>
    public static readonly string CtorName = ".ctor";

    /// <summary>
    /// Returns "get_".
    /// </summary>
    public static readonly string GetterPrefix = "get_";
    /// <summary>
    /// Returns "set_".
    /// </summary>
    public static readonly string SetterPrefix = "set_";

    /// <summary>
    /// Returns "add_".
    /// </summary>
    public static readonly string AddEventHandlerPrefix = "add_";
    /// <summary>
    /// Returns "remove_".
    /// </summary>
    public static readonly string RemoveEventHandlerPrefix = "remove_";

    /// <summary>
    /// Returns "System.Reflection.RuntimeMethodInfo".
    /// </summary>
    public static readonly string RuntimeMethodInfoName = "System.Reflection.RuntimeMethodInfo";

    /// <summary>
    /// Returns "GetValueOrDefault".
    /// </summary>
    public static readonly string GetValueOrDefault = "GetValueOrDefault";

    /// <summary>
    /// Returns "GetValue".
    /// </summary>
    public static readonly string GetValue = "GetValue";

    ///<summary>
    /// Various well-known constants related to <see cref="IQueryable"/>.
    ///</summary>
    public static class Queryable
    {
      /// <summary>
      /// Returns "Where".
      /// </summary>
      public const string Where = "Where";

      /// <summary>
      /// Returns "Select".
      /// </summary>
      public const string Select = "Select";

      /// <summary>
      /// Returns "SelectMany".
      /// </summary>
      public const string SelectMany = "SelectMany";

      /// <summary>
      /// Returns "Join".
      /// </summary>
      public const string Join = "Join";

      /// <summary>
      /// Returns "OrderBy".
      /// </summary>
      public const string OrderBy = "OrderBy";

      /// <summary>
      /// Returns "OrderByDescending".
      /// </summary>
      public const string OrderByDescending = "OrderByDescending";

      /// <summary>
      /// Returns "ThenBy".
      /// </summary>
      public const string ThenBy = "ThenBy";

      /// <summary>
      /// Returns "ThenByDescending".
      /// </summary>
      public const string ThenByDescending = "ThenByDescending";

      /// <summary>
      /// Returns "GroupBy".
      /// </summary>
      public const string GroupBy = "GroupBy";

      /// <summary>
      /// Returns "Count".
      /// </summary>
      public const string Count = "Count";

      /// <summary>
      /// Returns "Min".
      /// </summary>
      public const string Min = "Min";

      /// <summary>
      /// Returns "Max".
      /// </summary>
      public const string Max = "Max";

      /// <summary>
      /// Returns "Sum".
      /// </summary>
      public const string Sum = "Sum";

      /// <summary>
      /// Returns "Average".
      /// </summary>
      public const string Average = "Average";

      /// <summary>
      /// Returns "Distinct".
      /// </summary>
      public const string Distinct = "Distinct";

      /// <summary>
      /// Returns "Skip".
      /// </summary>
      public const string Skip = "Skip";

      /// <summary>
      /// Returns "Take".
      /// </summary>
      public const string Take = "Take";

      /// <summary>
      /// Returns "First".
      /// </summary>
      public const string First = "First";

      /// <summary>
      /// Returns "FirstOrDefault".
      /// </summary>
      public const string FirstOrDefault = "FirstOrDefault";

      /// <summary>
      /// Returns "Single".
      /// </summary>
      public const string Single = "Single";

      /// <summary>
      /// Returns "SingleOrDefault".
      /// </summary>
      public const string SingleOrDefault = "SingleOrDefault";

      /// <summary>
      /// Returns "Any".
      /// </summary>
      public const string Any = "Any";

      /// <summary>
      /// Returns "All".
      /// </summary>
      public const string All = "All";
      
      /// <summary>
      /// Returns "All".
      /// </summary>
      public const string Contains = "Contains";
    }
  }
}
