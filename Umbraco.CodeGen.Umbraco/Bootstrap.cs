﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using Umbraco.CodeGen.Configuration;
using Umbraco.CodeGen.Generators;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;

namespace Umbraco.CodeGen.Umbraco
{
    public class Bootstrap : ApplicationEventHandler
    {
        private ModelGenerator generator;

        private CodeGeneratorConfiguration configuration;
        private IEnumerable<Type> types;

        protected override void ApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            Initialize();
            if (configuration != null)
            {
                ListenForContentTypeSaves();
                SetModelFactory();
            }
            else
            {
                LogHelper.Warn<Bootstrap>("Couldn't initialize codegen due to missing configuration.");
            }
        }

        private void Initialize()
        {
            LoadConfiguration();
            if (configuration != null)
            {
                InitializeGenerator();
                FindModelTypes();
            }
        }

        private void ListenForContentTypeSaves()
        {
            ContentTypeService.SavedContentType += ContentTypeSaved;
        }

        public void ContentTypeSaved(IContentTypeService service, SaveEventArgs<IContentType> args)
        {
            foreach (var contentType in args.SavedEntities)
                generator.GenerateModelAndDependants(service, contentType);
        }

        private void SetModelFactory()
        {
            PublishedContentModelFactoryResolver.Current.SetFactory(new PublishedContentModelFactory(types));
        }

        private void FindModelTypes()
        {
            var namespaces = new[] { configuration.DocumentTypes.Namespace, configuration.MediaTypes.Namespace }.Distinct();
            types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => TypesFromNamespaces(a, namespaces));
        }

        private static IEnumerable<Type> TypesFromNamespaces(Assembly assembly, IEnumerable<string> namespaces)
        {

            try
            {
                return assembly.GetTypes().Where(t => TypeIsInNamespace(namespaces, t));
            }
            catch (ReflectionTypeLoadException)
            {
                return new Type[0];
            }
        }

        private static bool TypeIsInNamespace(IEnumerable<string> namespaces, Type t)
        {
            return namespaces.Contains(t.Namespace) && !t.IsInterface;
        }

        private void InitializeGenerator()
        {
            var generatorFactory = CreateFactory<CodeGeneratorFactory>(configuration.GeneratorFactory);
            var interfaceGeneratorFactory = CreateFactory<CodeGeneratorFactory>(configuration.InterfaceFactory);
            var dataTypeProvider = new UmbracoDataTypesProvider();
            var paths = new Dictionary<string, string>
            {
                {"DocumentType", IOHelper.MapPath(configuration.DocumentTypes.ModelPath)},
                {"MediaType", IOHelper.MapPath(configuration.MediaTypes.ModelPath)}
            };

            generator = new ModelGenerator(
                configuration,
                generatorFactory,
                interfaceGeneratorFactory,
                dataTypeProvider,
                paths
                );
        }

        private void LoadConfiguration()
        {
            var configurationProvider = new CodeGeneratorConfigurationFileProvider(
                IOHelper.MapPath("~/config/CodeGen.config")
                );
            configuration = configurationProvider.GetConfiguration();
        }

        internal static T CreateFactory<T>(string typeName)
        {
            try
            {
                var factoryType = Type.GetType(typeName);
                if (factoryType == null)
                    factoryType = Type.GetType(String.Format("{0}, Umbraco.CodeGen", typeName));
                if (factoryType == null)
                    throw new Exception(String.Format("Type {0} not found", typeName));
                return (T)Activator.CreateInstance(factoryType);
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Invalid factory '{0}'", typeName), ex);
            }
        }

    }
}
