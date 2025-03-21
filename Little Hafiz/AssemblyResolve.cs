using System;
using System.IO;
using System.Reflection;

class AssemblyResolver // GPT-4o
{
    private static readonly string LibrariesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lib");

    public static void Initialize() => AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;

    private static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
    {
        string assemblyName = new AssemblyName(args.Name).Name + ".dll";
        string assemblyPath = Path.Combine(LibrariesPath, assemblyName);

        if (File.Exists(assemblyPath))
            return Assembly.LoadFrom(assemblyPath);

        return null;
    }
}
