namespace GmhWorkshop.CommonTools;

public class SettingsFileProperty<T>
{
    /// <summary>
    ///     The 'name' for the property that will be used in the console
    ///     prompts if user data entry is needed.
    /// </summary>
    public string PropertyDisplayName { get; set; } = string.Empty;

    /// <summary>
    ///     Turns the entered values to '*' characters - useful for passwords. THIS HAS NOTHING
    ///     TO DO WITH 'SECURE STRINGS' - this is just a visual obfuscation, examining the memory
    ///     of the running program will reveal this value in plain text!!
    /// </summary>
    public bool HideEnteredValue { get; set; } = false;

    /// <summary>
    ///     Non-blank values are displayed to the user if they are prompted
    ///     to enter a value for this property.
    /// </summary>
    public string PropertyEntryHelp { get; set; } = string.Empty;

    /// <summary>
    ///     Used to check if a value is valid - if the initial settings file
    ///     value is not valid the user will be prompted for entry - if the
    ///     value the user enters is not valid the user will be prompter
    /// </summary>
    public Func<T, (bool isValid, string message)> PropertyIsValid { get; set; } = _ => (true, string.Empty);

    public Func<string, (bool isValid, string message)> UserEntryIsValid { get; set; } = _ => (true, string.Empty);

    public Action<T, string> SetValue { get; set; } = (arg1, s) => { };
}