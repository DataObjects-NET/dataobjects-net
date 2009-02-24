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
    /// Returns "Item"
    /// </summary>
    public static readonly string IndexerPropertyName = "Item";

    /// <summary>
    /// Returns "add_".
    /// </summary>
    public static readonly string AddEventHandlerPrefix = "add_";

    /// <summary>
    /// Returns "remove_".
    /// </summary>
    public static readonly string RemoveEventHandlerPrefix = "remove_";

    /// <summary>
    /// Returns "op_Addition".
    /// </summary>
    public static readonly string OperatorAddition = "op_Addition";

    /// <summary>
    /// Returns "op_Subtraction".
    /// </summary>
    public static readonly string OperatorSubtraction = "op_Subtraction";

    /// <summary>
    /// Returns "op_Multiply".
    /// </summary>
    public static readonly string OperatorMultiply = "op_Multiply";

    /// <summary>
    /// Returns "op_Divide".
    /// </summary>
    public static readonly string OperatorDivide = "op_Divide";

    /// <summary>
    /// Returns "op_Modulus".
    /// </summary>
    public static readonly string OperatorModulus = "op_Modulus";

    /// <summary>
    /// Returns "op_Increment".
    /// </summary>
    public static readonly string OperatorIncrement = "op_Increment";

    /// <summary>
    /// Returns "op_Decrement".
    /// </summary>
    public static readonly string OperatorDecrement = "op_Decrement";

    /// <summary>
    /// Returns "op_UnaryNegation".
    /// </summary>
    public static readonly string OperatorUnaryNegation = "op_UnaryNegation";

    /// <summary>
    /// Returns "op_UnaryPlus".
    /// </summary>
    public static readonly string OperatorUnaryPlus = "op_UnaryPlus";

    /// <summary>
    /// Returns "op_BitwiseOr".
    /// </summary>
    public static readonly string OperatorBitwiseOr = "op_BitwiseOr";

    /// <summary>
    /// Returns "op_BitwiseAnd".
    /// </summary>
    public static readonly string OperatorBitwiseAnd = "op_BitwiseAnd";

    /// <summary>
    /// Returns "op_ExclusiveOr".
    /// </summary>
    public static readonly string OperatorExclusiveOr = "op_ExclusiveOr";

    /// <summary>
    /// Returns "op_OnesComplement".
    /// </summary>
    public static readonly string OperatorOnesComplement = "op_OnesComplement";

    /// <summary>
    /// Returns "op_LeftShift"
    /// </summary>
    public static readonly string OperatorLeftShift = "op_LeftShift";

    /// <summary>
    /// Returns "op_RightShift
    /// </summary>
    public static readonly string OperatorRightShift = "op_RightShift";

    /// <summary>
    /// Returns "op_LogicalNot".
    /// </summary>
    public static readonly string OperatorLogicalNot = "op_LogicalNot";

    /// <summary>
    /// Returns "op_True".
    /// </summary>
    public static readonly string OperatorTrue = "op_True";

    /// <summary>
    /// Returns "op_False".
    /// </summary>
    public static readonly string OperatorFalse = "op_False";

    /// <summary>
    /// Returns "op_Explicit"
    /// </summary>
    public static readonly string OperatorExplicit = "op_Explicit";

    /// <summary>
    /// Returns "op_Implicit".
    /// </summary>
    public static readonly string OperatorImplicit = "op_Implicit";

    /// <summary>
    /// Returns "op_Equality".
    /// </summary>
    public static readonly string OperatorEquality = "op_Equality";

    /// <summary>
    /// Returns "op_Inequality".
    /// </summary>
    public static readonly string OperatorInequality = "op_Inequality";

    /// <summary>
    /// Returns "op_GreatherThan".
    /// </summary>
    public static readonly string OperatorGreatherThan = "op_GreatherThan";

    /// <summary>
    /// Returns "op_GreatherThanOrEqual".
    /// </summary>
    public static readonly string OperatorGreatherThanOrEqual = "op_GreatherThanOrEqual";

    /// <summary>
    /// Returns "op_LessThan".
    /// </summary>
    public static readonly string OperatorLessThan = "op_LessThan";

    /// <summary>
    /// Returns "op_LessThanOrEqual".
    /// </summary>
    public static readonly string OperatorLessThanOrEqual = "op_LessThanOrEqual";

    /// <summary>
    /// Returns "System.Reflection.RuntimeMethodInfo".
    /// </summary>
    public static readonly string RuntimeMethodInfoName = "System.Reflection.RuntimeMethodInfo";

    ///<summary>
    /// Various well-known constants related to <see cref="Object"/>.
    ///</summary>
    public static class Object
    {
      /// <summary>
      /// Returns "Clone".
      /// </summary>
      public const string Clone = "Clone";

      /// <summary>
      /// Returns "Equals".
      /// </summary>
      public const string Equals = "Equals";

      /// <summary>
      /// Returns "GetHashCode".
      /// </summary>
      public const string GetHashCode = "GetHashCode";
    }

    ///<summary>
    /// Various well-known constants related to <see cref="Tuple"/>.
    ///</summary>
    public static class Tuple
    {

      /// <summary>
      /// Returns "GetValueOrDefault".
      /// </summary>
      public const string GetValueOrDefault = "GetValueOrDefault";

      /// <summary>
      /// Returns "GetValue".
      /// </summary>
      public const string GetValue = "GetValue";

      /// <summary>
      /// Returns "SetValue".
      /// </summary>
      public const string SetValue = "SetValue";

      /// <summary>
      /// Returns "HasValue".
      /// </summary>
      public const string HasValue = "HasValue";

      /// <summary>
      /// Returns "GetFieldState".
      /// </summary>
      public const string GetFieldState = "GetFieldState";

      /// <summary>
      /// Returns "SetFieldState".
      /// </summary>
      public const string SetFieldState = "SetFieldState";

      /// <summary>
      /// Returns "descriptor".
      /// </summary>
      public const string DescriptorFieldName = "descriptor";

      /// <summary>
      /// Returns "Descriptor".
      /// </summary>
      public const string Descriptor = "Descriptor";

      /// <summary>
      /// Returns "Count".
      /// </summary>
      public const string Count = "Count";

      /// <summary>
      /// Returns "CreateNew".
      /// </summary>
      public const string CreateNew = "CreateNew";
    }

    ///<summary>
    /// Various well-known constants related to <see cref="IQueryable"/>.
    ///</summary>
    public static class Queryable
    {
      /// <summary>
      /// Returns "Aggregate".
      /// </summary>
      public const string Aggregate = "Aggregate";

      /// <summary>
      /// Returns "All".
      /// </summary>
      public const string All = "All";

      /// <summary>
      /// Returns "Any".
      /// </summary>
      public const string Any = "Any";

      /// <summary>
      /// Returns "AsEnumerable".
      /// </summary>
      public const string AsEnumerable = "AsEnumerable";

      /// <summary>
      /// Returns "AsQueryable".
      /// </summary>
      public const string AsQueryable = "AsQueryable";

      /// <summary>
      /// Returns "Average".
      /// </summary>
      public const string Average = "Average";

      /// <summary>
      /// Returns "Cast".
      /// </summary>
      public const string Cast = "Cast";

      /// <summary>
      /// Returns "Concat".
      /// </summary>
      public const string Concat = "Concat";

      /// <summary>
      /// Returns "All".
      /// </summary>
      public const string Contains = "Contains";

      /// <summary>
      /// Returns "Count".
      /// </summary>
      public const string Count = "Count";

      /// <summary>
      /// Returns "DefaultIfEmpty".
      /// </summary>
      public const string DefaultIfEmpty = "DefaultIfEmpty";

      /// <summary>
      /// Returns "Distinct".
      /// </summary>
      public const string Distinct = "Distinct";

      /// <summary>
      /// Returns "ElementAt".
      /// </summary>
      public const string ElementAt = "ElementAt";

      /// <summary>
      /// Returns "ElementAtOrDefault".
      /// </summary>
      public const string ElementAtOrDefault = "ElementAtOrDefault";

      /// <summary>
      /// Returns "Except".
      /// </summary>
      public const string Except = "Except";

      /// <summary>
      /// Returns "First".
      /// </summary>
      public const string First = "First";

      /// <summary>
      /// Returns "FirstOrDefault".
      /// </summary>
      public const string FirstOrDefault = "FirstOrDefault";

      /// <summary>
      /// Returns "GroupBy".
      /// </summary>
      public const string GroupBy = "GroupBy";

      /// <summary>
      /// Returns "GroupJoin".
      /// </summary>
      public const string GroupJoin = "GroupJoin";

      /// <summary>
      /// Returns "Intersect".
      /// </summary>
      public const string Intersect = "Intersect";

      /// <summary>
      /// Returns "Join".
      /// </summary>
      public const string Join = "Join";

      /// <summary>
      /// Returns "Last".
      /// </summary>
      public const string Last = "Last";

      /// <summary>
      /// Returns "LastOrDefault".
      /// </summary>
      public const string LastOrDefault = "LastOrDefault";

      /// <summary>
      /// Returns "LongCount".
      /// </summary>
      public const string LongCount = "LongCount";

      /// <summary>
      /// Returns "Max".
      /// </summary>
      public const string Max = "Max";

      /// <summary>
      /// Returns "Min".
      /// </summary>
      public const string Min = "Min";

      /// <summary>
      /// Returns "OfType".
      /// </summary>
      public const string OfType = "OfType";

      /// <summary>
      /// Returns "OrderBy".
      /// </summary>
      public const string OrderBy = "OrderBy";

      /// <summary>
      /// Returns "OrderByDescending".
      /// </summary>
      public const string OrderByDescending = "OrderByDescending";

      /// <summary>
      /// Returns "Reverse".
      /// </summary>
      public const string Reverse = "Reverse";

      /// <summary>
      /// Returns "Select".
      /// </summary>
      public const string Select = "Select";

      /// <summary>
      /// Returns "SelectMany".
      /// </summary>
      public const string SelectMany = "SelectMany";

      /// <summary>
      /// Returns "SequenceEqual".
      /// </summary>
      public const string SequenceEqual = "SequenceEqual";

      /// <summary>
      /// Returns "Single".
      /// </summary>
      public const string Single = "Single";

      /// <summary>
      /// Returns "SingleOrDefault".
      /// </summary>
      public const string SingleOrDefault = "SingleOrDefault";

      /// <summary>
      /// Returns "Skip".
      /// </summary>
      public const string Skip = "Skip";

      /// <summary>
      /// Returns "SkipWhile".
      /// </summary>
      public const string SkipWhile = "SkipWhile";

      /// <summary>
      /// Returns "Sum".
      /// </summary>
      public const string Sum = "Sum";

      /// <summary>
      /// Returns "Take".
      /// </summary>
      public const string Take = "Take";

      /// <summary>
      /// Returns "TakeWhile".
      /// </summary>
      public const string TakeWhile = "TakeWhile";

      /// <summary>
      /// Returns "ThenBy".
      /// </summary>
      public const string ThenBy = "ThenBy";

      /// <summary>
      /// Returns "ThenByDescending".
      /// </summary>
      public const string ThenByDescending = "ThenByDescending";

      /// <summary>
      /// Returns "ToArray".
      /// </summary>
      public const string ToArray = "ToArray";

      /// <summary>
      /// Returns "ToList".
      /// </summary>
      public const string ToList = "ToList";

      /// <summary>
      /// Returns "Union".
      /// </summary>
      public const string Union = "Union";

      /// <summary>
      /// Returns "Where".
      /// </summary>
      public const string Where = "Where";
    }
  }
}
