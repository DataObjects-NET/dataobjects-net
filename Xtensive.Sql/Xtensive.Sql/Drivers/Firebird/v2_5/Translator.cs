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
        public override string TimeSpanFormatString { get { return Constants.TimeSpanFormatString; } }
        public override string QuoteIdentifier(params string[] names)
        {
            return SqlHelper.QuoteIdentifierWithQuotes(names);
        }

        public override string Translate(SqlValueType type)
        {
            if (type.TypeName != null)
                return type.TypeName;
            if (type.Type == SqlType.Interval)
                return "VARCHAR(30)";
            if (type.Type == SqlType.Boolean)
                return "smallint";
            if (type.Type == SqlType.Int8)
                return "smallint";
            if (type.Type == SqlType.UInt8)
                return "smallint";
            if (type.Type == SqlType.UInt16)
                return "integer";
            if (type.Type == SqlType.UInt32)
                return "bigint";
            if (type.Type == SqlType.UInt64)
                return "varchar(30)";
            if (type.Type == SqlType.Guid)
                return "varchar(36)";
            return base.Translate(type);
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
                return QuoteString(SqlHelper.TimeSpanToString((TimeSpan)literalValue, Constants.TimeSpanFormatString));
            return base.Translate(context, literalValue);
        }

        // Constructors

        public Translator(SqlDriver driver)
            : base(driver)
        {
        }
    }
}
