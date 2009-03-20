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
  /// The type of a normal form.
  ///</summary>
  public enum NormalFormType
  {
    Conjunctive = 0,
    Disjunctive
  }

  ///<summary>
  /// Represents the normal form of a boolean expression.
  ///</summary>
  public class NormalizedBooleanExpression : Expression, IEnumerable<Expression>
  {
    private readonly SetSlim<Expression> terms = new SetSlim<Expression>();

    public readonly NormalFormType NormalForm;

    ///<summary>
    /// Adds the term <paramref name="other"/> to the inner collection.
    /// If <paramref name="other"/> is NormalizedBooleanExpression having <c>NormalForm</c> 
    /// equal to <c>NormalForm</c> of this instance, then the method adds all terms containing 
    /// in <paramref name="other"/> to the inner collection.
    ///</summary>
    ///<param name="other"></param>
    public void AddTerm(Expression other)
    {
      ArgumentValidator.EnsureArgumentNotNull(other, "other");
      var otherNormalized = other as NormalizedBooleanExpression;
      if (otherNormalized != null)
        AddNormalizedExpression(otherNormalized);
      else
        AddBooleanExpression(other);
    }

    private void AddBooleanExpression(Expression otherBoolean)
    {
      if (otherBoolean.Type != typeof(bool))
        throw new ArgumentException(String.
          Format(Resources.Strings.ExExpressionMustReturnValueOfTypeX, typeof(bool)));
      terms.Add(otherBoolean);
    }

    private void AddNormalizedExpression(NormalizedBooleanExpression otherNormalized)
    {
      if (otherNormalized.NormalForm == NormalForm)
        foreach (var term in otherNormalized) {
          terms.Add(term);
        }
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
    public NormalizedBooleanExpression(NormalFormType normalForm)
      //The real value of nodeType doesn't matter.
      : base(normalForm == NormalFormType.Conjunctive ? ExpressionType.And : ExpressionType.Or,
             typeof(bool))
    {
      NormalForm = normalForm;
    }
  }
}
