#!/usr/bin/env dotnet run
#:property Nullable=enable
#:property ImplicitUsings=enable

using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace UpSparkle.Tools;

/// <summary>
/// Checks GitHub for the latest Sparkle, libMacSparkle, and WinSparkle releases and
/// rewrites their download links in <c>.gitbinmodules</c>. Other lines in the file
/// are preserved untouched.
/// </summary>
public static class NativeVersionUpdater
{
    private const string GitBinModulesFileName = ".gitbinmodules";
    private const string UserAgent =
        "UpSparkle-VersionCheck/1.0 (+https://github.com/junian/upsparkle)";

    private static readonly NativeLibrary[] Libraries =
    [
        new(
            Name: "Sparkle",
            Repository: "sparkle-project/Sparkle",
            ReleasePage: "https://github.com/sparkle-project/Sparkle/releases/latest",
            UrlMarker: "github.com/sparkle-project/Sparkle/releases/download/",
            AssetPattern: new Regex(
                @"^Sparkle-(?<version>\d+(?:\.\d+)*)\.tar\.xz$",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)),
        new(
            Name: "libMacSparkle",
            Repository: "junian/libmacsparkle",
            ReleasePage: "https://github.com/junian/libmacsparkle/releases/latest",
            UrlMarker: "github.com/junian/libmacsparkle/releases/download/",
            AssetPattern: new Regex(
                @"^libMacSparkle\.v?(?<version>\d+(?:\.\d+)*)\.zip$",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)),
        new(
            Name: "WinSparkle",
            Repository: "vslavik/winsparkle",
            ReleasePage: "https://github.com/vslavik/winsparkle/releases/latest",
            UrlMarker: "github.com/vslavik/winsparkle/releases/download/",
            AssetPattern: new Regex(
                @"^WinSparkle-(?<version>\d+(?:\.\d+)*)\.zip$",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)),
    ];

    /// <summary>
    /// Runs the version check and updates <c>.gitbinmodules</c> with the latest releases.
    /// </summary>
    /// <param name="args">
    /// Optional path to the repository root. When omitted, the nearest parent directory
    /// containing <c>.gitbinmodules</c> is used.
    /// </param>
    /// <returns>0 when the check completes, 1 when an error occurs.</returns>
    public static async Task<int> Main(string[] args)
    {
        try
        {
            var repositoryRoot = ResolveRepositoryRoot(args);
            var gitBinModulesPath = Path.Combine(repositoryRoot, GitBinModulesFileName);
            var originalContent = File.ReadAllText(gitBinModulesPath);
            var newLine = originalContent.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
            var hadTrailingNewLine = originalContent.EndsWith('\n');

            using var httpClient = CreateHttpClient();
            var releases = new List<ReleaseInfo>(Libraries.Length);

            foreach (var library in Libraries)
            {
                var release = await GetLatestReleaseAsync(httpClient, library);
                releases.Add(release);
                Console.WriteLine($"{library.Name} latest version: {release.Version}");
                Console.WriteLine($"  {release.DownloadUrl}");
            }

            var updatedContent = UpdateGitBinModules(
                originalContent,
                releases,
                newLine,
                hadTrailingNewLine);

            if (string.Equals(originalContent, updatedContent, StringComparison.Ordinal))
            {
                Console.WriteLine($"No changes to {GitBinModulesFileName}.");
                WriteGitHubOutput("changed", "false");
            }
            else
            {
                File.WriteAllText(gitBinModulesPath, updatedContent);
                Console.WriteLine($"Updated {GitBinModulesFileName}.");
                WriteGitHubOutput("changed", "true");
            }

            foreach (var release in releases)
            {
                WriteGitHubOutput($"{release.Library.Name.ToLowerInvariant()}_version", release.Version);
            }

            WriteGitHubSummary(releases);

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Creates an HTTP client with a descriptive user agent and optional GitHub token.
    /// </summary>
    /// <remarks>
    /// When <c>GITHUB_TOKEN</c> or <c>GH_TOKEN</c> is set the GitHub API allows more
    /// requests per hour, which matters when the workflow runs frequently.
    /// </remarks>
    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
        client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

        var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN")
                    ?? Environment.GetEnvironmentVariable("GH_TOKEN");

        if (!string.IsNullOrWhiteSpace(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        return client;
    }

    /// <summary>
    /// Locates the repository root that contains <c>.gitbinmodules</c>.
    /// </summary>
    /// <param name="args">Optional repository root supplied on the command line.</param>
    /// <returns>The absolute path to the repository root.</returns>
    /// <exception cref="DirectoryNotFoundException">
    /// Thrown when no directory containing <c>.gitbinmodules</c> can be found.
    /// </exception>
    private static string ResolveRepositoryRoot(string[] args)
    {
        if (args.Length > 0)
        {
            var candidate = Path.GetFullPath(args[0]);
            if (!File.Exists(Path.Combine(candidate, GitBinModulesFileName)))
            {
                throw new DirectoryNotFoundException(
                    $"'{candidate}' does not contain {GitBinModulesFileName}.");
            }

            return candidate;
        }

        for (var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, GitBinModulesFileName)))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException(
            $"Could not find {GitBinModulesFileName} in the current directory or any parent directory.");
    }

    /// <summary>
    /// Queries the GitHub API for the latest published (non-prerelease) release.
    /// </summary>
    /// <param name="httpClient">The client used to talk to the GitHub API.</param>
    /// <param name="library">The library whose release should be resolved.</param>
    /// <returns>The release version and its download link.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the release cannot be fetched or contains no matching asset.
    /// </exception>
    private static async Task<ReleaseInfo> GetLatestReleaseAsync(
        HttpClient httpClient,
        NativeLibrary library)
    {
        var apiUrl = $"https://api.github.com/repos/{library.Repository}/releases/latest";
        var json = await DownloadStringAsync(httpClient, apiUrl);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        var tagName = root.GetProperty("tag_name").GetString();
        var releaseUrl = root.GetProperty("html_url").GetString();

        if (string.IsNullOrWhiteSpace(tagName))
        {
            throw new InvalidOperationException(
                $"The latest {library.Name} release has no tag name.");
        }

        var releaseVersion = NormalizeVersion(tagName);
        var assets = root.GetProperty("assets");

        var asset = SelectAsset(library, assets, releaseVersion);

        if (asset is null)
        {
            throw new InvalidOperationException(
                $"The latest {library.Name} release ({tagName}) does not contain an asset " +
                $"matching {library.AssetPattern}.");
        }

        return new ReleaseInfo(
            library,
            releaseVersion,
            asset.Value.Name,
            asset.Value.DownloadUrl,
            releaseUrl ?? library.ReleasePage);
    }

    /// <summary>
    /// Picks the archive that matches the library's asset pattern and release version.
    /// </summary>
    /// <param name="library">The library whose assets are being inspected.</param>
    /// <param name="assets">The <c>assets</c> array from the GitHub release response.</param>
    /// <param name="releaseVersion">The version taken from the release tag.</param>
    /// <returns>The matching asset, or <see langword="null"/> when none matches.</returns>
    private static (string Name, string DownloadUrl)? SelectAsset(
        NativeLibrary library,
        JsonElement assets,
        string releaseVersion)
    {
        (string Name, string DownloadUrl, string Version)? fallback = null;

        foreach (var asset in assets.EnumerateArray())
        {
            var name = asset.GetProperty("name").GetString();
            var downloadUrl = asset.GetProperty("browser_download_url").GetString();

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(downloadUrl))
            {
                continue;
            }

            var match = library.AssetPattern.Match(name);
            if (!match.Success)
            {
                continue;
            }

            var assetVersion = NormalizeVersion(match.Groups["version"].Value);

            if (assetVersion == releaseVersion)
            {
                return (name, downloadUrl);
            }

            // Keep the highest versioned asset as a fallback in case the tag
            // does not carry a plain semver (for example "release-2024-01").
            if (fallback is null ||
                Version.TryParse(assetVersion, out var candidate) &&
                Version.TryParse(fallback.Value.Version, out var current) &&
                candidate > current)
            {
                fallback = (name, downloadUrl, assetVersion);
            }
        }

        return fallback is null ? null : (fallback.Value.Name, fallback.Value.DownloadUrl);
    }

    /// <summary>
    /// Downloads a URL, retrying transient failures.
    /// </summary>
    /// <param name="httpClient">The client used for the request.</param>
    /// <param name="url">The absolute URL to download.</param>
    /// <returns>The response body as a string.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the resource cannot be downloaded after three attempts.
    /// </exception>
    private static async Task<string> DownloadStringAsync(HttpClient httpClient, string url)
    {
        Exception? lastError = null;

        for (var attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                using var response = await httpClient.GetAsync(url);

                if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Forbidden)
                {
                    throw new InvalidOperationException(
                        $"GitHub returned {(int)response.StatusCode} {response.ReasonPhrase} for {url}.");
                }

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex) when (
                ex is HttpRequestException or TaskCanceledException ||
                ex is InvalidOperationException &&
                ex.Message.Contains("GitHub returned", StringComparison.Ordinal))
            {
                lastError = ex;

                if (attempt < 3)
                {
                    Console.WriteLine($"Request failed (attempt {attempt}/3): {ex.Message}");
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }
        }

        throw new InvalidOperationException($"Could not download {url}.", lastError);
    }

    /// <summary>
    /// Rewrites the managed download links in <c>.gitbinmodules</c> and preserves
    /// every other line, including blank lines.
    /// </summary>
    /// <param name="originalContent">The current file content.</param>
    /// <param name="releases">The latest releases discovered on GitHub.</param>
    /// <param name="newLine">The line ending used by the file.</param>
    /// <param name="hadTrailingNewLine">Whether the original file ended with a newline.</param>
    /// <returns>The updated file content.</returns>
    private static string UpdateGitBinModules(
        string originalContent,
        IReadOnlyList<ReleaseInfo> releases,
        string newLine,
        bool hadTrailingNewLine)
    {
        var lines = originalContent
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Split('\n')
            .ToList();

        // Drop the artificial empty entry produced by a trailing newline.
        if (lines.Count > 0 && lines[^1].Length == 0)
        {
            lines.RemoveAt(lines.Count - 1);
        }

        foreach (var release in releases)
        {
            var index = lines.FindIndex(line =>
                line.Contains(release.Library.UrlMarker, StringComparison.OrdinalIgnoreCase));

            if (index >= 0)
            {
                lines[index] = release.DownloadUrl;
            }
            else
            {
                Console.WriteLine(
                    $"No existing entry for {release.Library.Name}; appending it to {GitBinModulesFileName}.");
                lines.Add(release.DownloadUrl);
            }
        }

        var updated = string.Join(newLine, lines);
        return hadTrailingNewLine ? updated + newLine : updated;
    }

    /// <summary>
    /// Normalizes a tag or asset version by trimming a leading <c>v</c>.
    /// </summary>
    private static string NormalizeVersion(string value)
    {
        var trimmed = value.Trim();
        return trimmed.StartsWith('v') || trimmed.StartsWith('V') ? trimmed[1..] : trimmed;
    }

    /// <summary>
    /// Writes a value to the GitHub Actions step output file when running in Actions.
    /// </summary>
    private static void WriteGitHubOutput(string name, string value)
    {
        var outputPath = Environment.GetEnvironmentVariable("GITHUB_OUTPUT");
        if (string.IsNullOrWhiteSpace(outputPath))
        {
            return;
        }

        File.AppendAllText(outputPath, $"{name}={value}{Environment.NewLine}");
    }

    /// <summary>
    /// Writes a human-readable summary to the GitHub Actions job summary when running in Actions.
    /// </summary>
    private static void WriteGitHubSummary(IReadOnlyList<ReleaseInfo> releases)
    {
        var summaryPath = Environment.GetEnvironmentVariable("GITHUB_STEP_SUMMARY");
        if (string.IsNullOrWhiteSpace(summaryPath))
        {
            return;
        }

        var rows = string.Join(
            Environment.NewLine,
            releases.Select(release =>
                $"| {release.Library.Name} | [{release.Version}]({release.ReleaseUrl}) | `{release.AssetName}` |"));

        var summary = $"""
            ## Native library version check

            | Library | Latest version | Asset |
            | --- | --- | --- |
            {rows}

            `.gitbinmodules` has been refreshed with the links above.

            """;

        File.AppendAllText(summaryPath, summary);
    }

    /// <summary>
    /// Describes a native library tracked in <c>.gitbinmodules</c>.
    /// </summary>
    /// <param name="Name">The display name of the library.</param>
    /// <param name="Repository">The GitHub repository in <c>owner/name</c> form.</param>
    /// <param name="ReleasePage">Fallback link to the releases page.</param>
    /// <param name="UrlMarker">
    /// A substring that identifies the library's line in <c>.gitbinmodules</c>.
    /// </param>
    /// <param name="AssetPattern">
    /// A pattern matching the release archive, with a <c>version</c> capture group.
    /// </param>
    private sealed record NativeLibrary(
        string Name,
        string Repository,
        string ReleasePage,
        string UrlMarker,
        Regex AssetPattern);

    /// <summary>
    /// A resolved release with the asset that should be recorded in <c>.gitbinmodules</c>.
    /// </summary>
    private sealed record ReleaseInfo(
        NativeLibrary Library,
        string Version,
        string AssetName,
        string DownloadUrl,
        string ReleaseUrl);
}
