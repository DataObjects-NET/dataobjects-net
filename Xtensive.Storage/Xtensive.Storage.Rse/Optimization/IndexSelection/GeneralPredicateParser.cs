// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.03

using System;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Linq;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Rse.Optimization.IndexSelection
{
  internal sealed class GeneralPredicateParser : ExpressionVisitor<RangeSetInfo>
  {
    private readonly ComparisonExtractor extractor = new ComparisonExtractor();
    private readonly ParserHelper parserHelper;
    private IndexInfo indexInfo;
    private RecordSetHeader recordSetHeader;
    private bool invertionIsActive;

    public RangeSetInfo Parse(Expression predicate, IndexInfo info, RecordSetHeader primaryIdxRecordSetHeader)
    {
      ArgumentValidator.EnsureArgumentNotNull(predicate, "predicate");
      ArgumentValidator.EnsureArgumentNotNull(info, "info");
      ArgumentValidator.EnsureArgumentNotNull(primaryIdxRecordSetHeader, "primaryIdxRecordSetHeader");
      var unwrappedPredicate = UnwrapPredicate(predicate);
      if (!CanBeParsed(unwrappedPredicate))
        throw new ArgumentException(
          String.Format(Resources.Strings.ExExpressionMustReturnValueOfTypeX, typeof(bool)), "predicate");
      indexInfo = info;
      recordSetHeader = primaryIdxRecordSetHeader;
      invertionIsActive = false;
      var result = Visit(unwrappedPredicate);
      return AdjustResult(unwrappedPredicate, result);
    }

    private static Expression UnwrapPredicate(Expression predicate)
    {
      var result = predicate;
      while (result.NodeType == ExpressionType.Lambda)
        result = ((LambdaExpression) result).Body;
      return result;
    }

    private RangeSetInfo AdjustResult(Expression predicate, RangeSetInfo result)
    {
      if(result != null)
        return result;
      return parserHelper.ConvertToRangeSetInfo(predicate, null, indexInfo, recordSetHeader);
    }

    #region Overrides of ExpressionVisitor<RangeSetInfo>

    protected override RangeSetInfo VisitUnary(UnaryExpression u)
    {
      if (!CanBeParsed(u))
        return null;
      var comparison = extractor.Extract(u, ParserHelper.DeafultKeySelector);
      if(comparison != null)
        return parserHelper.ConvertToRangeSetInfo(u, comparison, indexInfo, recordSetHeader);
      var prevInversionState = SwitchInversion(u);
      var result = Visit(u.Operand);
      invertionIsActive = prevInversionState;
      if (result != null && u.NodeType == ExpressionType.Not && !invertionIsActive)
        return RangeSetExpressionBuilder.BuildInvert(result);
      return result;
    }

    private bool SwitchInversion(UnaryExpression u)
    {
      var prevValue = invertionIsActive;
      if (u.NodeType == ExpressionType.Not)
        invertionIsActive = !invertionIsActive;
      return prevValue;
    }

    protected override RangeSetInfo VisitBinary(BinaryExpression b)
    {
      if (!CanBeParsed(b))
        return null;
      var comparison = extractor.Extract(b, ParserHelper.DeafultKeySelector);
      if(comparison != null)
        return parserHelper.ConvertToRangeSetInfo(b, comparison, indexInfo, recordSetHeader);
      if(b.NodeType != ExpressionType.AndAlso && b.NodeType != ExpressionType.OrElse)
        return RangeSetExpressionBuilder.BuildFullRangeSetConstructor(null);
      return VisitOperands(b);
    }

    private RangeSetInfo VisitOperands(BinaryExpression b)
    {
      var leftRs = Visit(b.Left);
      var rightRs = Visit(b.Right);
      if(leftRs == null)
        leftRs = parserHelper.ConvertToRangeSetInfo(b.Left, null, indexInfo, recordSetHeader);
      if(rightRs == null)
        rightRs = parserHelper.ConvertToRangeSetInfo(b.Right, null, indexInfo, recordSetHeader);
      if (b.NodeType == ExpressionType.AndAlso && !invertionIsActive
        || b.NodeType == ExpressionType.OrElse && invertionIsActive)
        return RangeSetExpressionBuilder.BuildIntersect(leftRs, rightRs);
      return RangeSetExpressionBuilder.BuildUnite(leftRs, rightRs);
    }

    protected override RangeSetInfo VisitMethodCall(MethodCallExpression mc)
    {
      if (!CanBeParsed(mc))
        return null;
      var comparison = extractor.Extract(mc, ParserHelper.DeafultKeySelector);
      return parserHelper.ConvertToRangeSetInfo(mc, comparison, indexInfo, recordSetHeader);
    }

    private static bool CanBeParsed(Expression exp)
    {
      return exp.Type==typeof (bool);
    }

    protected override RangeSetInfo VisitTypeIs(TypeBinaryExpression tb)
    {
      return null;
    }

    protected override RangeSetInfo VisitConstant(ConstantExpression c)
    {
      return null;
    }

    protected override RangeSetInfo VisitConditional(ConditionalExpression c)
    {
      return null;
    }

    protected override RangeSetInfo VisitParameter(ParameterExpression p)
    {
      return null;
    }

    protected override RangeSetInfo VisitMemberAccess(MemberExpression m)
    {
      return null;
    }

    protected override RangeSetInfo VisitLambda(LambdaExpression l)
    {
      return null;
    }

    protected override RangeSetInfo VisitNew(NewExpression n)
    {
      return null;
    }

    protected override RangeSetInfo VisitMemberInit(MemberInitExpression mi)
    {
      return null;
    }

    protected override RangeSetInfo VisitListInit(ListInitExpression li)
    {
      return null;
    }

    protected override RangeSetInfo VisitNewArray(NewArrayExpression na)
    {
      return null;
    }

    protected override RangeSetInfo VisitInvocation(InvocationExpression i)
    {
      return null;
    }

    #endregion

    // Constructors

    public GeneralPredicateParser(DomainModel domainModel)
    {
      parserHelper = new ParserHelper(domainModel);
    }
  }
}