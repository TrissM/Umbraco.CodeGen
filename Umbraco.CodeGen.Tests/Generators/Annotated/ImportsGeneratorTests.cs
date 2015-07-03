﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Umbraco.CodeGen.Generators.Annotated;

namespace Umbraco.CodeGen.Tests.Generators.Annotated
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
                    "global::Umbraco.CodeGen.Annotations",
                    "global::Umbraco.Core.Models",
                    "global::Umbraco.Web"
                }.SequenceEqual(
                    ns.Imports.Cast<CodeNamespaceImport>()
                        .Select(import => import.Namespace)
                ));
        }
    }
}
