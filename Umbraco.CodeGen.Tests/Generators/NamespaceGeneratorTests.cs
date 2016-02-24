﻿using System;
using System.CodeDom;
using System.Linq;
using NUnit.Framework;
using Umbraco.CodeGen.Configuration;
using Umbraco.CodeGen.Definitions;
using Umbraco.CodeGen.Generators;
using Umbraco.ModelsBuilder.Building;

namespace Umbraco.CodeGen.Tests.Generators
{
    [TestFixture]
    public class NamespaceGeneratorTests : TypeCodeGeneratorTestBase
    {
        private CodeCompileUnit compileUnit;

        [SetUp]
        public void SetUp()
        {
            Configuration = new GeneratorConfig();
            Configuration.Namespace = "MyWeb.Models";
            ContentType = new TypeModel {ItemType = TypeModel.ItemTypes.Media};
            Generator = new NamespaceGenerator(Configuration);
            compileUnit = new CodeCompileUnit();
        }

        [Test]
        public void Generate_AddsNamespace()
        {
            Generator.Generate(compileUnit, ContentType);
            Assert.AreEqual("MyWeb.Models", GetNamespace().Name);
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Namespace not configured.")]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void Generate_NoConfiguredNamespace_Throws(string value)
        {
            Configuration.Namespace = value;
            Generator.Generate(compileUnit, ContentType);
        }

        [Test]
        public void CallsMemberGenerators()
        {
            var spies = new[]{new SpyGenerator(), new SpyGenerator()};
            var memberGenerators = spies.Cast<CodeGeneratorBase>().ToArray();
            Generator = new NamespaceGenerator(Configuration, memberGenerators);
            Generator.Generate(compileUnit, ContentType);
            Assert.That(spies.All(s => s.Called));
        }

        private CodeNamespace GetNamespace()
        {
            return compileUnit.Namespaces.Cast<CodeNamespace>().Single();
        }
    }
}
