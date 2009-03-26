// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.20

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core.Collections;

namespace Xtensive.Core.Linq
{
  ///<summary>
  /// Type of a normal form.
  ///</summary>
  public enum NormalFormType
  {
    Conjunctive = 0,
    Disjunctive
  }

  ///<summary>
  /// Normal form of a boolean expression.
  ///</summary>
  public class NormalizedBooleanExpression : Expression, IEnumerable<Expression>
  {
    private readonly SetSlim<Expression> terms = new SetSlim<Expression>();

    public readonly NormalFormType NormalForm;

    public readonly bool IsRoot;

    /// <summary>
    /// Adds a child expression.
    /// </summary>
    /// <param name="otherBoolean">The expression to be added.</param>
    public void AddBooleanExpression(Expression otherBoolean)
    {
      if (IsRoot)
        throw new InvalidOperationException(
          Resources.Strings.ExOnlyNormalizedExpressionCanBeAddedAsChildToRoot);
      ArgumentValidator.EnsureArgumentNotNull(otherBoolean, "otherBoolean");
      if (otherBoolean.Type != typeof(bool))
        throw new ArgumentException(String.
          Format(Resources.Strings.ExExpressionMustReturnValueOfTypeX, typeof(bool)), "otherBoolean");
      terms.Add(otherBoolean);
    }

    ///<summary>
    /// Adds a child expression.
    ///</summary>
    ///<param name="otherNormalized">The expression to be added.</param>
    public void AddNormalizedExpression(NormalizedBooleanExpression otherNormalized)
    {
      ArgumentValidator.EnsureArgumentNotNull(otherNormalized, "otherNormalized");
      if (otherNormalized.NormalForm == NormalForm)
        AddExpWithEqualNormalForm(otherNormalized);
      else
        AddExpWithDifferentNormalForm(otherNormalized);
    }

    private void AddExpWithEqualNormalForm(NormalizedBooleanExpression otherNormalized)
    {
      if (otherNormalized.IsRoot) {
        foreach (var term in otherNormalized) {
          terms.Add(term);
        }
      }
      else {
        throw new ArgumentException(Resources.Strings.ExExpressionHavingEqualNormalFormMustBeRoot,
          "otherNormalized");
      }
    }

    private void AddExpWithDifferentNormalForm(NormalizedBooleanExpression otherNormalized)
    {
      if (otherNormalized.IsRoot) {
        throw new ArgumentException(Resources.Strings.ExExpressionHavingDifferentNormalFormMustNotBeRoot,
          "otherNormalized");
      }
      terms.Add(otherNormalized);
    }

    #region Implementation of IEnumerable

    public IEnumerator<Expression> GetEnumerator()
    {
      return terms.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion

    ///<summary>
    /// Constructor.
    ///</summary>
    ///<param name="normalForm">The type of normal form.</param>
    ///<param name="isRoot">Specifies whether object being created will represent root expression or child expression.</param>
    public NormalizedBooleanExpression(NormalFormType normalForm, bool isRoot)
      //The real value of nodeType doesn't matter.
      : base(normalForm == NormalFormType.Conjunctive ? ExpressionType.And : ExpressionType.Or,
             typeof(bool))
    {
      IsRoot = isRoot;
      NormalForm = normalForm;
    }

    /// <summary>
    /// Creates a object representing a child normalized expression (i.e. NCF is a child for NDF).
    /// </summary>
    /// <param name="normalForm">The type of normal form.</param>
    /// <param name="firstChild">The first child (it must be a boolean expression).</param>
    public NormalizedBooleanExpression(NormalFormType normalForm, Expression firstChild)
      :this(normalForm, false)
    {
      if (firstChild is NormalizedBooleanExpression)
        throw new ArgumentException(String.Format(Resources.Strings.ExArgumentMustnotBeOfTypeX, "firstChild",
          typeof(NormalizedBooleanExpression)));
      AddBooleanExpression(firstChild);
    }
  }
}