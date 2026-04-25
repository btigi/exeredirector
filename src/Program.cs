using System.Diagnostics;
using System.Text.Json;
using ExeRedirector.Model;

namespace ExeRedirector;

public static class Program
{
    private const string ConfigFileName = "ExeRedirector.json";

    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
        {
            return ExitCodes.NoInputFile;
        }

        string inputFilePath;
        try
        {
            inputFilePath = Path.GetFullPath(args[0]);
        }
        catch
        {
            return ExitCodes.InvalidInputFile;
        }

        var configPath = Path.Combine(AppContext.BaseDirectory, ConfigFileName);
        var mappings = LoadMappings(configPath);
        if (mappings is null || mappings.Count == 0)
        {
            return ExitCodes.InvalidConfiguration;
        }

        var mapping = FindBestMapping(mappings, inputFilePath);
        if (mapping is null)
        {
            return ExitCodes.NoMatchingMapping;
        }

        return StartMappedApp(mapping, inputFilePath);
    }

    private static IReadOnlyList<RedirectMapping>? LoadMappings(string configPath)
    {
        try
        {
            if (!File.Exists(configPath))
            {
                return null;
            }

            using var stream = File.OpenRead(configPath);
            return JsonSerializer.Deserialize<List<RedirectMapping>>(stream, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private static RedirectMapping? FindBestMapping(IEnumerable<RedirectMapping> mappings, string inputFilePath)
    {
        return mappings
            .Select(mapping => new
            {
                Mapping = mapping,
                NormalizedPath = NormalizeDirectoryPath(mapping.Path),
                NormalizedFileType = NormalizeFileType(mapping.FileType)
            })
            .Where(candidate => candidate.NormalizedPath is not null)
            .Where(candidate => candidate.NormalizedFileType is not null)
            .Where(candidate => IsInDirectory(inputFilePath, candidate.NormalizedPath!))
            .Where(candidate => IsFileType(inputFilePath, candidate.NormalizedFileType!))
            .OrderByDescending(candidate => candidate.NormalizedPath!.Length)
            .Select(candidate => candidate.Mapping)
            .FirstOrDefault();
    }

    private static string? NormalizeDirectoryPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        try
        {
            return TrimEndingDirectorySeparator(Path.GetFullPath(path));
        }
        catch
        {
            return null;
        }
    }

    private static string? NormalizeFileType(string? fileType)
    {
        if (string.IsNullOrWhiteSpace(fileType))
        {
            return null;
        }

        fileType = fileType.Trim();
        if (fileType.StartsWith("*."))
        {
            fileType = fileType[1..];
        }
        else if (!fileType.StartsWith('.'))
        {
            fileType = "." + fileType;
        }

        return fileType;
    }

    private static bool IsFileType(string filePath, string fileType)
    {
        return Path.GetExtension(filePath).Equals(fileType, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsInDirectory(string filePath, string directoryPath)
    {
        var normalizedFilePath = TrimEndingDirectorySeparator(filePath);
        if (IsRootDirectory(directoryPath))
        {
            return normalizedFilePath.StartsWith(directoryPath, StringComparison.OrdinalIgnoreCase);
        }

        return normalizedFilePath.Equals(directoryPath, StringComparison.OrdinalIgnoreCase) ||
               normalizedFilePath.StartsWith(directoryPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
               normalizedFilePath.StartsWith(directoryPath + Path.AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsRootDirectory(string path)
    {
        return Path.GetPathRoot(path)?.Equals(path, StringComparison.OrdinalIgnoreCase) == true;
    }

    private static string TrimEndingDirectorySeparator(string path)
    {
        var rootLength = Path.GetPathRoot(path)?.Length ?? 0;
        while (Path.EndsInDirectorySeparator(path) && path.Length > rootLength)
        {
            path = path[..^1];
        }

        return path;
    }

    private static int StartMappedApp(RedirectMapping mapping, string inputFilePath)
    {
        if (string.IsNullOrWhiteSpace(mapping.App))
        {
            return ExitCodes.InvalidConfiguration;
        }

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = mapping.App,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            foreach (var argument in mapping.Arguments ?? [])
            {
                startInfo.ArgumentList.Add(argument);
            }

            startInfo.ArgumentList.Add(inputFilePath);

            Process.Start(startInfo);
            return ExitCodes.Success;
        }
        catch
        {
            return ExitCodes.StartFailed;
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };
}
