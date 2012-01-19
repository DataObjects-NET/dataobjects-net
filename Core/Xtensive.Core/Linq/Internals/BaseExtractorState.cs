// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.27

using System;
using System.Linq.Expressions;

namespace Xtensive.Linq
{
  internal abstract class BaseExtractorState : ExpressionVisitor<ExtractionInfo>
  {
    protected Func<Expression, bool> KeySelector;

    #region ExtractorStates
    private OperandVisitState operandVisitState;
    private VisitComparisonMethodState visitComparisonMethodState;

    protected OperandVisitState OperandVisitState {
      get {
        if (operandVisitState == null)
          operandVisitState = new OperandVisitState();
        return operandVisitState;
      }
    }

    protected VisitComparisonMethodState VisitComparisonMethodState {
      get {
        if (visitComparisonMethodState == null)
          visitComparisonMethodState = new VisitComparisonMethodState();
        return visitComparisonMethodState;
      }
    }
    #endregion

    #region Overrides of ExpressionVisitor<Expression>

    protected override ExtractionInfo VisitUnary(UnaryExpression exp)
    {
      return SelectKey(exp);
    }

    protected override ExtractionInfo VisitBinary(BinaryExpression exp)
    {
      return SelectKey(exp);
    }

    protected override ExtractionInfo VisitTypeIs(TypeBinaryExpression exp)
    {
      return SelectKey(exp);
    }

    protected override ExtractionInfo VisitConstant(ConstantExpression exp)
    {
      return SelectKey(exp);
    }

    protected override ExtractionInfo VisitConditional(ConditionalExpression exp)
    {
      return SelectKey(exp);
    }

    protected override ExtractionInfo VisitParameter(ParameterExpression exp)
    {
      return SelectKey(exp);
    }

    protected override ExtractionInfo VisitMemberAccess(MemberExpression exp)
    {
      return SelectKey(exp);
    }

    protected override ExtractionInfo VisitMethodCall(MethodCallExpression exp)
    {
      return SelectKey(exp);
    }

    protected override ExtractionInfo VisitLambda(LambdaExpression exp)
    {
      var keyInfo = SelectKey(exp);
      if (keyInfo != null)
        return keyInfo;
      return Visit(exp.Body);
    }

    protected override ExtractionInfo VisitNew(NewExpression exp)
    {
      return SelectKey(exp);
    }

    protected override ExtractionInfo VisitMemberInit(MemberInitExpression exp)
    {
      return SelectKey(exp);
    }

    protected override ExtractionInfo VisitListInit(ListInitExpression exp)
    {
      return SelectKey(exp);
    }

    protected override ExtractionInfo VisitNewArray(NewArrayExpression exp)
    {
      return SelectKey(exp);
    }

    protected override ExtractionInfo VisitInvocation(InvocationExpression exp)
    {
      return SelectKey(exp);
    }

    #endregion

    protected ExtractionInfo SelectKey(Expression keyCandidate)
    {
      if(KeySelector(keyCandidate))
        return new ExtractionInfo {Key = keyCandidate};
      return null;
    }
  }
}