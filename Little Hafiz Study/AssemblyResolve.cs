using System;
using System.Reflection;

class AssemblyResolve
{
    public static void AssemblyResolveEventHandler() => AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
    public static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    {
        string assemblyPath = System.IO.Path.Combine("libraries", new AssemblyName(args.Name).Name + ".dll");

        if (System.IO.File.Exists(assemblyPath))
            return Assembly.LoadFrom(assemblyPath);
        
        return null;
    }
}
