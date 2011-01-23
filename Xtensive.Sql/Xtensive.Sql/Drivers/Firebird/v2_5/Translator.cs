// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Csaba Beer
// Created:    2011.01.17

using Xtensive.Sql.Compiler;
using System;
using System.Text;
using Xtensive.Core;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Ddl;


namespace Xtensive.Sql.Drivers.Firebird.v2_5
{

    internal class Translator : SqlTranslator
    {
        public override string DateTimeFormatString { get { return Constants.DateTimeFormatString; } }
        public override string TimeSpanFormatString { get { return string.Empty; } }
//        public override string TimeSpanFormatString { get { return Constants.TimeSpanFormatString; } }
  
        public override string QuoteIdentifier(params string[] names)
        {
            return SqlHelper.QuoteIdentifierWithQuotes(names);
        }

        public override string Translate(SqlCompilerContext context, SqlDropTable node)
        {
            return "DROP TABLE " + Translate(node.Table);
        }


        public override string Translate(SqlCompilerContext context, object literalValue)
        {
            var literalType = literalValue.GetType();
            switch (Type.GetTypeCode(literalType))
            {
                case TypeCode.Boolean:
                    return (bool)literalValue ? "1" : "0";
                case TypeCode.UInt64:
                    return QuoteString(((UInt64)literalValue).ToString());
            }
            if (literalType == typeof(byte[]))
            {
                var values = (byte[])literalValue;
                var builder = new StringBuilder(2 * (values.Length + 1));
                builder.Append("'");
                builder.AppendHexArray(values);
                builder.Append("'");
                return builder.ToString();
            }
            if (literalType == typeof(Guid))
                return QuoteString(SqlHelper.GuidToString((Guid)literalValue));
            if (literalType == typeof(TimeSpan))
                return Convert.ToString((long)((TimeSpan)literalValue).Ticks * 100);
            return base.Translate(context, literalValue);
        }

        public override string Translate(SqlNodeType type)
        {
            switch (type)
            {
                case SqlNodeType.Equals:
                    return "IS NOT DISTINCT FROM";
                case SqlNodeType.NotEquals:
                    return "IS DISTINCT FROM";
                case SqlNodeType.Modulo:
                    return "MOD";
                case SqlNodeType.DateTimeMinusDateTime:
                    return "-";
                case SqlNodeType.Except:
                    throw SqlHelper.NotSupported(type.ToString());
                case SqlNodeType.DateTimePlusInterval:
                case SqlNodeType.DateTimeMinusInterval:
                    throw SqlHelper.NotSupported(type.ToString());  // handled in compiler
                case SqlNodeType.BitAnd:
                    return "BIT_AND";
                case SqlNodeType.BitOr:
                    return "BIT_OR";
                case SqlNodeType.BitXor:
                    return "BIT_XOR";
                case SqlNodeType.BitNot:
                    throw SqlHelper.NotSupported(type.ToString());  // handled in compiler
                case SqlNodeType.Overlaps:
                    throw SqlHelper.NotSupported(type.ToString());
                default:
                    return base.Translate(type);
            }
        }

        public override string Translate(SqlFunctionType type)
        {
            switch (type)
            {
                case SqlFunctionType.CharLength:
                    return "CHAR_LENGTH";
                case SqlFunctionType.BinaryLength:
                    return "OCTET_LENGTH";
                case SqlFunctionType.Truncate:
                    return "TRUNC";
                case SqlFunctionType.IntervalNegate:
                    return "-";
                case SqlFunctionType.Log:
                    return "LN";
                case SqlFunctionType.Log10:
                    return "LOG10";
                case SqlFunctionType.Ceiling:
                    return "CEIL";
                case SqlFunctionType.PadLeft:
                    return "LPAD";
                case SqlFunctionType.PadRight:
                    return "RPAD";
                case SqlFunctionType.Concat:
                    return "||";
                case SqlFunctionType.SystemUser:
                case SqlFunctionType.SessionUser:
                    return base.Translate(SqlFunctionType.CurrentUser);
                case SqlFunctionType.Degrees:
                case SqlFunctionType.Radians:
                case SqlFunctionType.Square:
                    throw SqlHelper.NotSupported(type.ToString());
                default:
                    return base.Translate(type);
            }
        }

        public override string Translate(SqlLockType lockType)
        {
            if (lockType.Supports(SqlLockType.Shared) || lockType.Supports(SqlLockType.SkipLocked))
                return base.Translate(lockType);
            return "WITH LOCK";
        }


        // Constructors

        public Translator(SqlDriver driver)
            : base(driver)
        {
        }
    }
}
