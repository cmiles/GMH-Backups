using System.Text.Json;
using GitCredentialManager;
using Serilog;

namespace GmhWorkshop.CommonTools;

public class ObfuscatedSettingsConsoleSetup<T> where T : class, ISettingsFileType, new()
{
    /// <summary>
    ///     Full Path to the Settings File - if the file does not exist it will be created.
    /// </summary>
    public string SettingsFile { get; set; } = string.Empty;

    /// <summary>
    ///     The Vault Service Identifier is used to store the obfuscation key in the Windows Credential Manager.
    /// </summary>
    public string VaultServiceIdentifier { get; set; } = string.Empty;

    /// <summary>
    ///     A string identifier for the 'type' of settings file - this helps to verify that the
    ///     de-obfuscated settings file is the correct type and valid.
    /// </summary>
    public string SettingsFileIdentifier { get; set; } = string.Empty;

    public List<SettingsFileProperty<T>> SettingsFileProperties { get; set; } = new();

    public async Task<(bool isValid, T settings)> Setup()
    {
        if (string.IsNullOrWhiteSpace(SettingsFile))
        {
            Log.Error($"A non-Blank/Empty Settings File must be provided");
            return (false, new T());
        }

        var store = CredentialManager.Create();

        var settingsFileKey = store.Get(VaultServiceIdentifier, "AutomatedUserSettings");
        var obfuscationKey = settingsFileKey?.Password ?? string.Empty;

        if (settingsFileKey == null || string.IsNullOrWhiteSpace(settingsFileKey.Password))
        {
            Console.WriteLine();
            var userSettingsFileKey =
                ConsoleTools.GetPasswordFromConsole("Please enter the settings file Obfuscation Key: ");
            Console.WriteLine();

            if (string.IsNullOrWhiteSpace(userSettingsFileKey))
            {
                Log.Error("Sorry - a non-blank Obfuscation Key must provided... exiting.");
                return (false, new T());
            }

            store.AddOrUpdate("http://sensorpushbackup.com", "AutomatedUserSettings", userSettingsFileKey);
            obfuscationKey = userSettingsFileKey;
        }

        var settingsFile = new FileInfo(SettingsFile);

        if (!settingsFile.Exists)
        {
            Log.Information($"Settings File {settingsFile.FullName} does not exist - creating it now.");
            var newSettings = JsonSerializer.Serialize(new T());
            var newSettingsJsonObfuscated = newSettings.Encrypt(obfuscationKey);
            await File.WriteAllTextAsync(settingsFile.FullName, newSettingsJsonObfuscated);
        }

        var settingsFileContentsObfuscated = await File.ReadAllTextAsync(settingsFile.FullName);
        var settingsFileContents = settingsFileContentsObfuscated.Decrypt(obfuscationKey);
        var settings = JsonSerializer.Deserialize<T>(settingsFileContents);

        if (settings == null)
        {
            Log.Error("Could not read the Settings File {fileName}", settingsFile.FullName);
            return (false, new T());
        }

        if (string.IsNullOrWhiteSpace(settings.SettingsType) ||
            !settings.SettingsType.Equals(SettingsFileIdentifier))
        {
            Log.Error("{fileName} could not be read as a settings file - wrong key? wrong file?",
                settingsFile.FullName);
            return (false, new T());
        }

        foreach (var loopSettings in SettingsFileProperties)
        {
            var shouldHaveUserEnterValue = !loopSettings.PropertyIsValid(settings).isValid;

            while (shouldHaveUserEnterValue)
            {
                Console.WriteLine();

                if (!string.IsNullOrWhiteSpace(loopSettings.PropertyEntryHelp))
                {
                    Console.WriteLine(loopSettings.PropertyEntryHelp);
                }

                Console.Write($"{loopSettings.PropertyDisplayName}: ");

                var userEnteredValue = loopSettings.HideEnteredValue
                    ? ConsoleTools.GetPasswordFromConsole(string.Empty)
                    : Console.ReadLine();

                var userEnteredValueIsValid = loopSettings.UserEntryIsValid(userEnteredValue ?? string.Empty);

                if (!userEnteredValueIsValid.isValid)
                {
                    if (!string.IsNullOrWhiteSpace(userEnteredValueIsValid.message))
                    {
                        Console.WriteLine(userEnteredValueIsValid.message);
                    }

                    continue;
                }

                loopSettings.SetValue(settings, userEnteredValue);

                var propertyIsValid = loopSettings.PropertyIsValid(settings);

                if (!propertyIsValid.isValid)
                {
                    if (!string.IsNullOrWhiteSpace(propertyIsValid.message))
                    {
                        Console.WriteLine(propertyIsValid.message);
                    }

                    continue;
                }

                shouldHaveUserEnterValue = false;
            }
        }

        var currentSettingsJson = JsonSerializer.Serialize(settings);
        var currentSettingsJsonObfuscated = currentSettingsJson.Encrypt(obfuscationKey);
        await File.WriteAllTextAsync(settingsFile.FullName, currentSettingsJsonObfuscated);

        return (true, settings);
    }
}