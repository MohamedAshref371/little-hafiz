using Little_Hafiz;
using System;
using System.IO;
using System.Reflection;

class AssemblyResolver
{
    public static void Initialize() => AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;

    private static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
    {
        try
        {
            string assemblyPath = Path.Combine("lib", new AssemblyName(args.Name).Name + ".dll");

            if (File.Exists(assemblyPath))
                return Assembly.LoadFrom(assemblyPath);
        }
        catch (Exception ex)
        {
            Program.LogError(ex.Message, ex.StackTrace);
        }

        return null;
    }
}
