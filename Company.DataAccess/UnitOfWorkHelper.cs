using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyModel;

namespace Company.DataAccess
{
    public static class UnitOfWorkHelper
    {
        private static bool IsFrameworkAssembly(string assemblyName)
        {
            return assemblyName.StartsWith("Microsoft.")
                || assemblyName.StartsWith("System.")
                || assemblyName.StartsWith("Newtonsoft.")
                || assemblyName.StartsWith("HealthChecks.")
                || assemblyName.StartsWith("Polly")
                || assemblyName.StartsWith("NLog")
                || assemblyName.StartsWith("Prometheus.")
                || assemblyName.StartsWith("netstandard");
        }

        private static void LoadAllAssemblies(bool includeFrameworkAssemblies = false)
        {
            //https://dotnetstories.com/blog/Dynamically-pre-load-assemblies-in-a-ASPNET-Core-or-any-C-project-en-7155735300

            // Storage to ensure not loading the same assembly twice and optimize calls to GetAssemblies()
            ConcurrentDictionary<string, bool> Loaded = new ConcurrentDictionary<string, bool>();

            // Filter to avoid loading all the .NET framework
            bool ShouldLoad(string assemblyName)
            {
                if (Loaded.ContainsKey(assemblyName))
                {
                    return false;
                }

                if (IsFrameworkAssembly(assemblyName) && !includeFrameworkAssemblies)
                {
                    return false;
                }

                return true;
            }
            void LoadReferencedAssembly(Assembly assembly)
            {
                try
                {
                    // Check all referenced assemblies of the specified assembly
                    foreach (AssemblyName an in assembly.GetReferencedAssemblies().Where(a => ShouldLoad(a.FullName)))
                    {
                        // Load the assembly and load its dependencies
                        LoadReferencedAssembly(Assembly.Load(an));
                        Loaded.TryAdd(an.FullName, true);
                    }
                }
                catch
                {
                    //Sometime loading fails.
                }
            }

            // Add to list already loaded assemblies
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies().Where(a => ShouldLoad(a.FullName)))
            {
                Loaded.TryAdd(a.FullName, true);
            }

            // Loop on loaded assemblies to load dependencies (it includes Startup assembly so should load all the dependency tree) 
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies().Where(a => !IsFrameworkAssembly(a.FullName)))
            {
                LoadReferencedAssembly(assembly);
            }
        }

        public static Dictionary<Type, Type> GetRepositoryInterfaceTypeToRepositoryTypeMapping(string prefix, string suffix)
        {
            // Using naming convention to find all matching repositories and corresponding interface. We want a mapping that looks like:
            // IEmployeeRepository = MemoryEmployeeRepository

            Dictionary<Type, Type> repositoryInterfaceTypeToRepositoryTypeMapping = new Dictionary<Type, Type>();
            List<Type> baseRepositoryTypes = new List<Type>();

            //Get the base type for all repositories
            Type baseRepositoryType = typeof(IRepository);

            LoadAllAssemblies(includeFrameworkAssemblies: false);

            IEnumerable<Assembly> assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(o => !IsFrameworkAssembly(o.FullName));

            foreach (System.Reflection.Assembly a in assemblies)
            {
                try
                {
                    Type[] types = a.GetTypes();
                    baseRepositoryTypes.AddRange(types.Where(p => baseRepositoryType.IsAssignableFrom(p) && p.IsClass && !p.IsAbstract && p.Name.StartsWith(prefix) && p.Name.EndsWith(suffix)));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            //Map entity type to a repository type
            foreach (Type repositoryType in baseRepositoryTypes)
            {
                //Remove prefix and suffix
                string temp = repositoryType.Name.Remove(0, prefix.Length);
                string entityName = temp.Substring(0, temp.Length - suffix.Length);

                string repositoryInterfaceName = $"I{entityName}{suffix}";
                Type? repositoryInterfaceType = GetRepositoryInterfaceType(repositoryType, repositoryInterfaceName);

                if (repositoryInterfaceType == null)
                {
                    throw new Exception("Cannot find interface '" + repositoryInterfaceName + "' for repository '" + repositoryType.Name + "'. Make sure naming convention are used.");
                }

                repositoryInterfaceTypeToRepositoryTypeMapping.Add(repositoryInterfaceType, repositoryType);
            }

            return repositoryInterfaceTypeToRepositoryTypeMapping;
        }

        private static Type? GetRepositoryInterfaceType(Type repositoryType, string repositoryInterfaceName)
        {
            Type repositoryInterfaceType;
            //Load repository assembly
            System.Reflection.Assembly repositoryAssembly = System.Reflection.Assembly.Load(repositoryType.Assembly.FullName);

            //Check if interface type is in the current assembly
            repositoryInterfaceType = repositoryAssembly.GetTypes().Where(t => t.IsInterface && t.Name == repositoryInterfaceName).FirstOrDefault();

            if (repositoryInterfaceType == null)
            {
                //Load assemblies referenced repository assembly
                foreach (System.Reflection.AssemblyName assemblyName in repositoryAssembly.GetReferencedAssemblies())
                {
                    //Load method resolve refrenced loaded assembly
                    System.Reflection.Assembly assembly = System.Reflection.Assembly.Load(assemblyName.FullName);

                    //Get the entity's type
                    repositoryInterfaceType = assembly.GetTypes().Where(t => t.IsInterface && t.Name == repositoryInterfaceName).FirstOrDefault();

                    if (repositoryInterfaceType != null)
                        break;
                }
            }

            return repositoryInterfaceType;
        }
    }
}
