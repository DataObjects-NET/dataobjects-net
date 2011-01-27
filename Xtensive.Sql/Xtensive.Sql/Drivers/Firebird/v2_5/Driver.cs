// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Csaba Beer
// Created:    2011.01.10

using System;
using Xtensive.Sql.Firebird;
using Xtensive.Sql.Info;
using Xtensive.Sql.Compiler;

namespace Xtensive.Sql.Firebird.v2_5
{
    internal class Driver : Xtensive.Sql.Firebird.Driver
    {
        protected override Sql.TypeMapper CreateTypeMapper()
        {
            return new TypeMapper(this);
        }

        protected override SqlCompiler CreateCompiler()
        {
//            throw new NotImplementedException("Firebird.v2_5.Compiler");
            return new Compiler(this);
        }

        protected override SqlTranslator CreateTranslator()
        {
            return new Translator(this);
        }

        protected override Model.Extractor CreateExtractor()
        {
            return new Extractor(this);
        }

        protected override Info.ServerInfoProvider CreateServerInfoProvider()
        {
            return new ServerInfoProvider(this);
        }

        // Constructors

        public Driver(CoreServerInfo coreServerInfo)
            : base(coreServerInfo)
        {
        }
    }
}
