namespace vMenu.LegacyChecks;

internal static class Program
{
    private static int Main(string[] args)
    {
        var root = FindRoot();

        if (args.Contains("--verify-il"))
        {
            var problems = IlCheck.Run(Targets(args, "--verify-il"));
            Console.WriteLine(problems == 0 ? "No IL the Legacy client refuses." : $"{problems} place(s) the Legacy client would refuse.");
            return problems == 0 ? 0 : 1;
        }

        if (args.Contains("--check-refs"))
        {
            var fivem = args.SkipWhile(arg => arg != "--fivem-lib").Skip(1).FirstOrDefault();

            if (fivem is not null && !File.Exists(Path.Combine(fivem, "mscorlib.dll")))
            {
                Console.Error.WriteLine($"No FiveM libraries in {fivem}.");
                return 1;
            }

            var libraries = fivem is not null
                ? Directory.GetFiles(fivem, "*.dll")
                : Directory.GetFiles(Path.Combine(root, "dependencies", "legacy", "client-ref"), "*.dll")
                    .Concat(Directory.GetFiles(CitizenFxClientLibrary(root), "*.dll")).ToArray();

            var problems = RefCheck.Run(libraries, fivem is null ? RefCheck.OtherClientLibraries : [], Targets(args, "--check-refs"));
            Console.WriteLine(problems == 0 ? "Everything these DLLs use is there for them in FiveM." : $"{problems} reference(s) FiveM cannot resolve.");
            return problems == 0 ? 0 : 1;
        }

        Console.Error.WriteLine("Usage: LegacyChecks --verify-il <dll or folder>... | --check-refs <dll or folder>... [--fivem-lib <folder>]");
        return 1;
    }

    private static List<string> Targets(string[] args, string mode) =>
        args.SkipWhile(arg => arg != mode).Skip(1).TakeWhile(arg => !arg.StartsWith("--", StringComparison.Ordinal))
            .SelectMany(arg => Directory.Exists(arg) ? Directory.GetFiles(arg, "*.dll") : [arg]).ToList();

    private static string CitizenFxClientLibrary(string root)
    {
        var packages = File.ReadAllText(Path.Combine(root, "src", "Directory.Packages.props"));
        var version = System.Text.RegularExpressions.Regex.Match(packages, "\"CitizenFX.Core.Client\" Version=\"([^\"]+)\"").Groups[1].Value;

        var cache = Environment.GetEnvironmentVariable("NUGET_PACKAGES")
            ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");

        return Path.Combine(cache, "citizenfx.core.client", version, "lib", "net45");
    }

    private static string FindRoot()
    {
        foreach (var start in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
        {
            for (var directory = new DirectoryInfo(start); directory is not null; directory = directory.Parent)
            {
                if (File.Exists(Path.Combine(directory.FullName, "vMenu.Enhanced.slnx")))
                {
                    return directory.FullName;
                }
            }
        }

        throw new InvalidOperationException("Run this from inside the vMenu repository.");
    }
}
