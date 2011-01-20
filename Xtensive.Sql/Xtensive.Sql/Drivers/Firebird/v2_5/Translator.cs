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


namespace Xtensive.Sql.Drivers.Firebird.v2_5
{

    internal class Translator : SqlTranslator
    {
        public override string DateTimeFormatString { get { return @"'cast ('\'yyyy\.MM\.dd HH\:mm\:ss\'' as timestamp)'"; } }
        public override string TimeSpanFormatString { get { return @"'{0}{1},{2},{3},{4},{5:000}'"; } }
        public override string QuoteIdentifier(params string[] names)
        {
            return SqlHelper.QuoteIdentifierWithQuotes(names);
        }

        public override string Translate(SqlValueType type)
        {
            if (type.Type == SqlType.Interval)
                return "VARCHAR(30)";
            return base.Translate(type);
        }

        public override string Translate(SqlCompilerContext context, SqlQueryRef node, TableSection section)
        {
            switch (section)
            {
                case TableSection.Entry:
                    return string.Empty; // node.Query is SqlFreeTextTable ? String.Empty : "(";
                case TableSection.Exit:
                    return string.Empty; // node.Query is SqlFreeTextTable ? String.Empty : ")";
                case TableSection.AliasDeclaration:
                    string alias = context.TableNameProvider.GetName(node);
                    return (string.IsNullOrEmpty(alias)) ? string.Empty : QuoteIdentifier(alias);
            }
            return string.Empty;
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
            return base.Translate(context, literalValue);
        }

        // Constructors

        public Translator(SqlDriver driver)
            : base(driver)
        {
        }
    }
}
