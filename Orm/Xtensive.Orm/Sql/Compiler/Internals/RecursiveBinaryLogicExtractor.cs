// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.08.29

using System;
using System.Collections.Generic;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Compiler
{
  internal static class RecursiveBinaryLogicExtractor
  {
    public static List<SqlExpression> Extract(SqlBinary input)
    {
      var operatorType = input.NodeType;
      if (operatorType!=SqlNodeType.Or && operatorType!=SqlNodeType.And)
        throw new NotSupportedException();
      var result = new List<SqlExpression>();
      Traverse(input, operatorType, result);
      return result;
    }

    private static void Traverse(SqlExpression expression, SqlNodeType operatorType, List<SqlExpression> output)
    {
      if (expression.NodeType==operatorType) {
        var binary = (SqlBinary) expression;
        Traverse(binary.Left, operatorType, output);
        Traverse(binary.Right, operatorType, output);
      }
      else
        output.Add(expression);
    }
  }
}