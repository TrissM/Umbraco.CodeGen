﻿using System.CodeDom;
using System.Linq;
using NUnit.Framework;
using Umbraco.CodeGen.Generators.Bcl;

namespace Umbraco.CodeGen.Tests.Generators.Bcl
{
    [TestFixture]
    public class ImportsGeneratorTests
    {
        [Test]
        public void Generate_AddsImports()
        {
            var ns = new CodeNamespace("ANamespace");
            var generator = new ImportsGenerator(null);
            generator.Generate(ns, null);
            Assert.That(
                new[]
                {
                    "global::System",
                    "global::System.ComponentModel",
                    "global::System.ComponentModel.DataAnnotations",
                    "global::Umbraco.Core.Models",
                    "global::Umbraco.Web"
                }.SequenceEqual(
                    ns.Imports.Cast<CodeNamespaceImport>()
                        .Select(import => import.Namespace)
                ));
        }
    }
}
