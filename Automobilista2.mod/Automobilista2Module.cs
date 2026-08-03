using System.Text;
using WindowsGSH.Core.Modules;

namespace WindowsGSH.Modules.Automobilista2;

public sealed class Automobilista2Module : ManifestBackedGameServerModule, IModuleExistingServerImportCapability
{
    private const string ConfigFileName = "server.cfg";
    private const string SampleConfigRelativePath = @"config_sample\server_with_lists.cfg";

    public bool CanImport(string path) => ExistingInstallImport.CanImport(this, path);

    public Task<ModuleExistingServerImportProbe> PreviewImportAsync(string path, CancellationToken cancellationToken) =>
        ExistingInstallImport.PreviewAsync(this, path, cancellationToken);
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    public override Task<IReadOnlyDictionary<string, object?>> ReadConfigFileSettingsAsync(
        ServerInstance instance,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var configPath = Path.Combine(instance.InstallPath, ConfigFileName);
        if (!File.Exists(configPath))
        {
            return Task.FromResult<IReadOnlyDictionary<string, object?>>(new Dictionary<string, object?>());
        }

        var settings = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in File.ReadLines(configPath))
        {
            CopySetting(line, "name", "server.name", settings);
            CopySetting(line, "hostPort", "network.port", settings);
            CopySetting(line, "queryPort", "network.queryPort", settings);
            CopySetting(line, "maxPlayerCount", "server.maxPlayers", settings);
            CopySetting(line, "password", "server.password", settings);
            CopySetting(line, "spectatorPassword", "server.spectatorPassword", settings);
            CopySetting(line, "allowEmptyJoin", "server.allowEmptyJoin", settings);
            CopySetting(line, "controlGameSetup", "server.controlGameSetup", settings);
        }

        return Task.FromResult<IReadOnlyDictionary<string, object?>>(settings);
    }

    public override Task WriteConfigFileSettingsAsync(ServerInstance instance, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var configPath = Path.Combine(instance.InstallPath, ConfigFileName);
        var samplePath = Path.Combine(instance.InstallPath, SampleConfigRelativePath);
        var sourcePath = File.Exists(configPath) ? configPath : samplePath;
        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException(
                "Automobilista 2 did not install its sample server configuration. Run Verify Files, then try again.",
                samplePath);
        }

        var replacements = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["name"] = GetSetting(instance, "server.name", "Automobilista 2 Dedicated Server"),
            ["hostPort"] = GetSetting(instance, "network.port", "27015"),
            ["queryPort"] = GetSetting(instance, "network.queryPort", "27016"),
            ["maxPlayerCount"] = GetSetting(instance, "server.maxPlayers", "32"),
            ["password"] = GetSetting(instance, "server.password", ""),
            ["spectatorPassword"] = GetSetting(instance, "server.spectatorPassword", ""),
            ["allowEmptyJoin"] = GetSetting(instance, "server.allowEmptyJoin", "true").ToLowerInvariant(),
            ["controlGameSetup"] = GetSetting(instance, "server.controlGameSetup", "false").ToLowerInvariant()
        };

        var lines = File.ReadAllLines(sourcePath);
        for (var index = 0; index < lines.Length; index++)
        {
            foreach (var replacement in replacements)
            {
                if (TryGetConfigValue(lines[index], replacement.Key, out _))
                {
                    var indentation = lines[index][..^lines[index].TrimStart().Length];
                    lines[index] = $"{indentation}{replacement.Key} : {replacement.Value}";
                    break;
                }
            }
        }

        Directory.CreateDirectory(instance.InstallPath);
        var temporaryPath = configPath + ".windowsgsh.tmp";
        try
        {
            File.WriteAllLines(temporaryPath, lines, Utf8NoBom);
            File.Move(temporaryPath, configPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }

        return Task.CompletedTask;
    }

    private static void CopySetting(
        string line,
        string configKey,
        string settingKey,
        IDictionary<string, object?> settings)
    {
        if (TryGetConfigValue(line, configKey, out var value))
        {
            settings[settingKey] = value;
        }
    }

    private static bool TryGetConfigValue(string line, string key, out string value)
    {
        value = string.Empty;
        var trimmed = line.Trim();
        if (trimmed.StartsWith("//", StringComparison.Ordinal) || trimmed.StartsWith('#'))
        {
            return false;
        }

        var separator = trimmed.IndexOf(':');
        if (separator < 0 || !trimmed[..separator].Trim().Equals(key, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        value = trimmed[(separator + 1)..].Trim();
        return true;
    }
}
