﻿using System;
using System.CodeDom;
using System.Linq;
using Umbraco.CodeGen.Configuration;
using Umbraco.CodeGen.Definitions;

namespace Umbraco.CodeGen.Generators.Bcl
{
    public class StructureGenerator : CodeGeneratorBase
    {
        public StructureGenerator(ContentTypeConfiguration config) : base(config)
        {
        }

        public override void Generate(object codeObject, Entity entity)
        {
            var contentType = (ContentType) entity;
            var type = (CodeTypeDeclaration) codeObject;

            AddStructure(type, contentType);
        }

        private static void AddStructure(CodeTypeDeclaration type, ContentType contentType)
        {
            if (contentType.Structure.All(String.IsNullOrWhiteSpace))
                return;
            var field = new CodeMemberField(
                typeof (Type[]),
                "structure"
                );
            var typeofExpressions = 
                contentType.Structure
                    .Where(allowedType => !String.IsNullOrWhiteSpace(allowedType))
                    .Select(allowedType => new CodeTypeOfExpression(allowedType.PascalCase()))
                    .Cast<CodeExpression>()
                    .ToArray();
            field.InitExpression = new CodeArrayCreateExpression(
                typeof(Type[]),
                typeofExpressions
                );
            type.Members.Add(field);
        }
    }
}
