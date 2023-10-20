/*
 * GameSettings.cs is part of Cosmetris.
 *
 * Copyright (c) 2023 CKProductions, https://ckproductions.dev/
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Serialization;
using Cosmetris.Game.GameModes;
using Cosmetris.Game.Objects.Cosmonoes.Util;
using Cosmetris.Input;
using Microsoft.Xna.Framework;

namespace Cosmetris.Settings;

/// <summary>
///     This class allows for custom settings to be saved to the settings xml file for the game.
/// </summary>
public class GameSettings
{
    public static GameSettings Instance => _instance ??= new GameSettings();
    
    private const string SettingsFile = "settings.xml";

    private const string SettingsPath = "Cosmetris";

    private const string SettingsVersion = "1.0.0";

    // Private instance of the class.
    private static GameSettings _instance;

    // This is going to point to the OS specific location of the AppData folder.
    private readonly string _appData;

    // This is a list of all the categories that we have.
    private List<GameOptionCategory> _optionCategories;
    private Dictionary<string, GameOptionBase> _options;

    private GameSettings()
    {
        _optionCategories = new List<GameOptionCategory>();
        _options = new Dictionary<string, GameOptionBase>();

        // Locate the AppData folder.
        _appData = GetLocalAppDataFolder();
        
        //If there is nothing to load, we need to create the settings file.
        if (!DoesFileExist())
            Save();
        
        // Register all the settings.
        RegisterSettings();
        
        // Load the settings file.
        Deserialize();

        // Ensure that the settings file is up to date.
        if (!IsSettingsLatest())
        {
            ConvertToUpdatedSettings();
        }

        // Load the settings from the file.
        Load();
    }
    
    private void RegisterSettings()
    {
        _optionCategories = new List<GameOptionCategory>();
        
        //  Create categories for each type of setting.
        AddCategory(new GameOptionCategory("Graphics"));
        AddCategory(new GameOptionCategory("Input"));
        AddCategory(new GameOptionCategory("Input Delay"));
        AddCategory(new GameOptionCategory("Audio"));
        AddCategory(new GameOptionCategory("Background Options"));
        AddCategory(new GameOptionCategory("High Scores"));
        AddCategory(new GameOptionCategory("Strings"));

        foreach (var button in Controller.Instance.GetButtons())
        {
            AddOptionToCategory("Input", button.Name, button);
            AddOptionToCategory("Input Delay", button.Name, button.Delay);
        }

        AddOptionToCategory("Input", "Left Stick Sensitivity", 0.5f);
        AddOptionToCategory("Input", "Right Stick Sensitivity", 0.5f);

        // SETTINGS
        AddOptionToCategory("Audio", "Music", true);
        AddOptionToCategory("Audio", "Sound Effects", true);
        AddOptionToCategory("Audio", "Master Volume", 0.1f);
        AddOptionToCategory("Audio", "Music Volume", 0.1f);
        AddOptionToCategory("Audio", "Sound Effect Volume", 0.1f);
        AddOptionToCategory("Graphics", "Fullscreen", false);
        AddOptionToCategory("Graphics", "VSync", false);
        
        AddOptionToCategory("Background Options", "Cosmono Rain", true);
        AddOptionToCategory("Background Options", "Cosmono Rain Intensity", 1.0f);
        AddOptionToCategory("Background Options", "Cosmono Rain Speed", 1.0f);

        AddOptionToCategory("Background Options", "Galaxy Color 1", new Color(190, 9, 102));
        AddOptionToCategory("Background Options", "Galaxy Color 2",  new Color(9, 169, 118));
        AddOptionToCategory("Background Options", "Galaxy Color 3",  new Color(212, 160, 45));
        AddOptionToCategory("Background Options", "Nebula Color",  new Color(168, 10, 245));
        AddOptionToCategory("Background Options", "Cloud Color", new Color(12, 226, 255));

        AddOptionToCategory("Strings", "Current Texture Pack", "");
        AddOptionToCategory("Strings", "Current Version", SettingsVersion);

        foreach (var gameMode in GameModeManager.Instance.GameModes)
        {
            AddOptionToCategory("High Scores", $"{gameMode} High Score", new Score());
        }
        
        //AddOptionToCategory("High Scores", "

        //foreach (var control in ColorManager.Instance.GuiColor)
        //AddOptionToCategory(control.Key, control.Value);
    }

    private void AddCategory(GameOptionCategory category)
    {
        _optionCategories.Add(category);
    }

    private void AddOptionToCategory<T>(string category, string name, T value)
    {
        foreach (var c in _optionCategories.Where(c =>
                     c.CategoryName.Equals(category, StringComparison.OrdinalIgnoreCase)))
        {
            var gOption = new GameOption<T>(category, name, value);
            c.AddOption(gOption);
            if (!_options.ContainsKey($"{name}{c.CategoryName}")) _options.Add($"{name}{c.CategoryName}", gOption);
        }

    }

    /// <summary>
    ///     Sets the value of the option with the given name.
    /// </summary>
    /// <param name="category"> The category of the option to set. </param>
    /// <param name="name"> The name of the option to set. </param>
    /// <param name="value"> The value to set the option to. </param>
    /// <typeparam name="T"> The type of the option. </typeparam>
    public void SetValue<T>(string category, string name, T value)
    {
        GameOptionBase foundOption = null;
        _optionCategories.ForEach(cat =>
        {
            if (cat.CategoryName.Equals(category, StringComparison.OrdinalIgnoreCase))
                foreach (var option in cat.CategoryOptions)
                    if (option.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                        foundOption = option;
        });

        if (foundOption.GetValue().GetType() == value.GetType())
        {
            foundOption.SetValue(value);
            if (_options.ContainsKey($"{foundOption.Name}{foundOption.CategoryName}"))
            {
                _options[$"{foundOption.Name}{foundOption.CategoryName}"] = foundOption;
            }
        }
    }

    /// <summary>
    ///     Gets the option with the given name.
    /// </summary>
    /// <param name="name"> The name of the option to get. </param>
    /// <typeparam name="T"> The type of the option. </typeparam>
    /// <returns> The option with the given name. </returns>
    public GameOption<T> GetOption<T>(string name)
    {
        GameOption<T> foundOption = null;
        _optionCategories.ForEach(category =>
        {
            dynamic cat = category;
            foreach (var option in cat.CategoryOptions)
                if (option.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    foundOption = option;
        });

        return foundOption;
    }

    /// <summary>
    ///     Gets the value of the option with the given name.
    /// </summary>
    /// <param name="name"> The name of the option to get the value of. </param>
    /// <typeparam name="T"> The type of the option. </typeparam>
    /// <returns> The value of the option with the given name. </returns>
    public T GetValue<T>(string category, string name)
    {
        T foundValue = default;
        _optionCategories.ForEach(cat =>
        {
            dynamic c = cat;
            if (!c.CategoryName.Equals(category, StringComparison.OrdinalIgnoreCase)) return;
            foreach (var option in c.CategoryOptions)
                if (option.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    foundValue = option.Value;
        });

        return foundValue;
    }

    private string GetSettingsFile()
    {
        return Path.Combine(_appData, SettingsPath, SettingsFile);
    }

    /// <summary>
    ///     Attempts to locate the OS specific AppData folder.
    /// </summary>
    private string GetLocalAppDataFolder()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return Environment.GetEnvironmentVariable("LOCALAPPDATA");
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return Environment.GetEnvironmentVariable("XDG_DATA_HOME") ??
                   Path.Combine(Environment.GetEnvironmentVariable("HOME"), ".local", "share");
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return Path.Combine(Environment.GetEnvironmentVariable("HOME"), "Library", "Application Support");

        throw new Exception(
            $"Unable to detect current OS. Please report this error to the developer. RenderInformation.Description -> ${RuntimeInformation.OSDescription}");
    }

    private bool IsSettingsLatest()
    {
        string version = GetValue<string>("Strings", "Current Version");
        version ??= "unk";
        return version.Equals(SettingsVersion, StringComparison.OrdinalIgnoreCase);
    }

    private void ConvertToUpdatedSettings()
    {
        // We need to get all settings names + values, we will then regenerate the file & look for matches with the old settings.
        var oldSettings = new Dictionary<string, GameOptionCategory>();
        
        foreach (var category in _optionCategories)
        {
            oldSettings.Add(category.CategoryName, category);
        }
            
        // Reset the Settings file.
        Reset();
            
        // Now we need to set the values of the new settings file to the old settings.
        foreach (var category in _optionCategories)
        {
            if (oldSettings.TryGetValue(category.CategoryName, out GameOptionCategory prevCategory))
            {
                foreach (var option in category.CategoryOptions)
                {
                    GameOptionBase opt = option;
                    foreach (var prevOption in prevCategory.CategoryOptions)
                    {
                        GameOptionBase prevOpt = prevOption;
                        if (opt.Name.Equals(prevOpt.Name, StringComparison.OrdinalIgnoreCase) &&
                            opt.GetType() == prevOpt.GetType())
                        {
                            opt.SetValue(prevOpt.GetValue());
                        }
                    }
                }
            }
        }
        
        // BUT, make sure we set the version to the latest version.
        SetValue("Strings", "Current Version", SettingsVersion);
            
        // Save the new settings file.
        Save();
    }

    private bool DoesFileExist()
    {
        if (File.Exists(GetSettingsFile()))
            return true;

        return false;
    }

    /// <summary>
    ///     Loads the settings file by deserializing the XML settings file.
    /// </summary>
    public void Load()
    {
        Deserialize();
    }

    internal void Save()
    {
        if (!Directory.Exists(Path.Combine(_appData, SettingsPath)))
        {
            Directory.CreateDirectory(Path.Combine(_appData, SettingsPath));
            Directory.CreateDirectory(Path.Combine(_appData, SettingsPath, "Texture Packs"));

            //Extracts & copies the default theme to the Texture Packs folder
            CopyDefaultTheme(Path.Combine(_appData, SettingsPath, "Texture Packs"));
        }

        Serialize();
        //Gui.Instance.AddDebugMessage($"Successfully saved settings file: {GetSettingsFile()}");
    }

    /// <summary>
    ///     Reset the settings file to default values.
    /// </summary>
    private void Reset()
    {
        // Re-register all settings.
        RegisterSettings();
        // Delete the current settings file.
        File.Delete(GetSettingsFile());
        // Save a new settings file.
        Save();
        // Load the new file.
        Load();
    }

    /// <summary>
    ///     Serialize and save the current state of all game settings.
    /// </summary>
    private void Serialize()
    {
        using var sw = new StreamWriter(new FileStream(GetSettingsFile(), FileMode.Create));
        using var tw = new XmlTextWriter(sw);
        tw.Formatting = Formatting.Indented;
        tw.Indentation = 4;
        var extraTypes = GenerateTypeList();
        var serializer = new XmlSerializer(_optionCategories.GetType(), null, extraTypes.ToArray(),
            new XmlRootAttribute("Options"),
            string.Empty);
        serializer.Serialize(tw, _optionCategories);
    }

    /// <summary>
    ///     Deserialize and load the content of our settings file.
    /// </summary>
    private void Deserialize()
    {
        using var sr = new XmlTextReader(new FileStream(GetSettingsFile(), FileMode.Open));
        var extraTypes = GenerateTypeList();
        var serializer = new XmlSerializer(_optionCategories.GetType(), null, extraTypes.ToArray(),
            new XmlRootAttribute("Options"),
            string.Empty);
        var checkList = (List<GameOptionCategory>)serializer.Deserialize(sr);
        //Load the deserialized content into our Option Categories list.
        _optionCategories = checkList;
    }

    /// <summary>
    ///  Copies the default theme, stored in the MonoGame content folder, into the user's AppData folder.
    /// </summary>
    /// <param name="targetDirectory">The directory to copy the default theme to.</param>
    private void CopyDefaultTheme(string targetDirectory)
    {
        // Create the target directory
        Directory.CreateDirectory(targetDirectory);

        // Get the current assembly
        var assembly = Assembly.GetExecutingAssembly();

        // Define the embedded resource name
        var resourceName = "Cosmetris.Content.Theme.default.zip";

        // Read the resource file from the assembly
        using (var resourceStream = assembly.GetManifestResourceStream(resourceName))
        {
            // Create a temporary file path to store the zip file
            var tempFilePath = Path.Combine(Path.GetTempPath(), "DefaultTheme.zip");

            // Save the resource stream to a temporary file
            using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
            {
                resourceStream?.CopyTo(fileStream);
            }

            // Extract the zip file to the target directory
            ZipFile.ExtractToDirectory(tempFilePath, targetDirectory);

            // Delete the temporary zip file
            File.Delete(tempFilePath);
        }
    }

    /// <summary>
    ///     Generates a list containing all GameOption types, so that XML can serialize & deserialize the data.
    /// </summary>
    private List<Type> GenerateTypeList()
    {
        List<Type> extraTypes = new() { _optionCategories.GetType() };
        for (var i = 0; i < _optionCategories.Count; i++)
        {
            //Need to use dynamic as we are unable to cast any object to GameOptionCategory at this point.
            GameOptionCategory gen = _optionCategories[i];
            extraTypes.Add(_optionCategories[i].GetType());
            extraTypes.Add(gen.CategoryOptions.GetType());
            foreach (var option in gen.CategoryOptions) extraTypes.Add(option.GetType());
        }

        return extraTypes;
    }

    public string GetCosmetrisFolderPath()
    {
        // Combine the AppData path with your game's folder name
        return Path.Combine(_appData, SettingsPath);
    }

    public string GetTexturePacksFolderPath()
    {
        // Add "Texture Packs" folder inside the "Cosmetris" folder
        return Path.Combine(GetCosmetrisFolderPath(), "Texture Packs");
    }

    public List<GameOptionCategory> GetCategories()
    {
        return _optionCategories;
    }

    public List<GameOptionBase> GetOptions()
    {
        return _options.Values.ToList();
    }
}