// Copyright (C) 2011-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2011.05.29

using System;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.ProjectCustomCompilerException.Model;
using System.Linq;

namespace Xtensive.Orm.Tests.Issues
{
    using System.Linq.Expressions;
    namespace ProjectCustomCompilerException.Model
    {
        [Serializable]
        [HierarchyRoot]
        public abstract class FundBase : Entity
        {
            [Field, Key]
            public int Id { get; private set; }

            private static readonly Expression<Func<FundBase, JuridicalPerson>> VirtualJuridicalPersonExpression =
                obj =>
                (obj is MutualFund)
                    ? (obj as MutualFund).ManagementCompany
                    : (obj is MilitaryMortgage)
                          ? (obj as MilitaryMortgage).JuridicalPerson
                          : (obj is SelfRegulatoryOrganization)
                                ? (obj as SelfRegulatoryOrganization).Organization
                                : (obj is PrivatePensionFund)
                                      ? (obj as PrivatePensionFund).JuridicalPerson
                                      : (obj is ShareholderInvestmentFund)
                                            ? (obj as ShareholderInvestmentFund).JuridicalPerson
                                            : null;

            private static readonly Func<FundBase, JuridicalPerson> VirtualJuridicalPersonCompiled =
                VirtualJuridicalPersonExpression.Compile();

            public JuridicalPerson VirtualJuridicalPerson
            {
                get { return VirtualJuridicalPersonCompiled(this); }
            }

            [CompilerContainer(typeof(Expression))]
            public static class CustomLinqCompilerContainer
            {
                /// <summary>Required to use a virtual field</summary>
                /// <param name="assignmentExpression"> The assignment expression. </param>
                /// <returns>Expression with bound parameters</returns>
                [Compiler(typeof(FundBase), "VirtualJuridicalPerson", TargetKind.PropertyGet)]
                public static Expression Depositary(Expression assignmentExpression)
                {
                    return VirtualJuridicalPersonExpression.BindParameters(assignmentExpression);
                }
            }
        }

        [Serializable]
        public class MutualFund : FundBase
        {
            /// <summary>
            /// Fund's management company
            /// </summary>
            [Field(Nullable = false)]
            public JuridicalPerson ManagementCompany { get; set; }
        }

        [Serializable]
        public class MilitaryMortgage : FundBase
        {
            [Field(Nullable = false)]
            public JuridicalPerson JuridicalPerson { get; set; }
        }

        [Serializable]
        public class SelfRegulatoryOrganization : FundBase
        {
            [Field(Nullable = false)]
            public JuridicalPerson Organization { get; set; }
        }
        [Serializable]
        public class PrivatePensionFund : FundBase
        {
            [Field(Nullable = false)]
            public JuridicalPerson JuridicalPerson { get; set; }
        }

        [Serializable]
        public class ShareholderInvestmentFund : FundBase
        {
            [Field(Nullable = false)]
            public JuridicalPerson JuridicalPerson { get; set; }
        }

        [Serializable]
        [HierarchyRoot]
        public partial class JuridicalPerson : Entity
        {
            [Field, Key]
            public int Id { get; private set; }

            [Field(Nullable = false, Length = 400)]
            public string FullName { get; set; }
        }
    }


    public class CustomCompilerException : AutoBuildTest
    {
        protected override DomainConfiguration BuildConfiguration()
        {
            var config = base.BuildConfiguration();
            config.Types.Register(typeof (FundBase).Assembly, typeof (FundBase).Namespace);
            return config;
        }

        [Test]
        public void MainTest()
        {
            Require.ProviderIsNot(StorageProvider.Sqlite, "parser stack overflow");

            using (var session = Domain.OpenSession())
            {
                using (var transactionScope = session.OpenTransaction())
                {
                    var result = session.Query.All<FundBase>()
                        .Where(q => q.VirtualJuridicalPerson != null && q.VirtualJuridicalPerson.FullName != null && q.VirtualJuridicalPerson.FullName.StartsWith("qweqwe"))
                        .ToArray();
                }
            }
        }
    }
}