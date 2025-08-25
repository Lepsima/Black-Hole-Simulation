using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.AccessControl;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BlackHoles;
public class Core : Game {
    public static Core Instance;
    
    public const string VERSION = "v1.1";
    public const string BUILD = "b1";
    
    private readonly GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;

    private Camera _camera;
    private BlackHole _blackHole;
    private FrameStats _frameStats;
    private BloomFilter _bloomFilter;
    private TextRenderer _textRenderer;
    private InputHandler _inputHandler;
    private FrameRenderer _frameRenderer;

    public static bool IsMenuOpen { get; private set; }
    public static Menu CurrentMenu { get; private set; } = Menu.Main;

    private Process settingsProcess;
    private Process defaultsProcess;
    
    public Core() {
        Instance = this;
        
        // Load settings from file
        Settings.CreateIfMissing(Settings.DefaultsFileName);
        Settings.AutoLoadCurrentSettings(Settings.SettingsFileName);
        Settings settings1 = Settings.CurrentSettings;

        if (settings1.version != VERSION || settings1.build != BUILD) {
            Console.WriteLine("The VERSION or BUILD from the loaded settings doesn't match, unexpected behaviour may occur.");
        }
        
        // Static parameters
        Content.RootDirectory = "Content";
        Window.Title = "Black Hole Simulation";
        
        // Set the graphics manager's settings
        graphics = new GraphicsDeviceManager(this);
        graphics.GraphicsProfile = GraphicsProfile.HiDef;
    }

    protected override void Initialize() 
    {
        // Instantiate all scripts
        _camera = new Camera();
        _blackHole = new BlackHole();
        _frameStats = new FrameStats();
        _bloomFilter = new BloomFilter();
        _textRenderer = new TextRenderer();
        _inputHandler = new InputHandler();
        _frameRenderer = new FrameRenderer();

        // Create events for keyboard presses
        CreateInputEvents();
        base.Initialize();
        
        // Apply settings once everything has been instantiated
        ApplySettings();
    }

    protected override void LoadContent() 
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        
        _textRenderer.Load(Content, spriteBatch);
        _blackHole.Load(GraphicsDevice, Content);
        _bloomFilter.Load(GraphicsDevice, Content);
        _frameRenderer.Load(GraphicsDevice, spriteBatch);
    }

    private void ApplySettings() 
    {
        // Read updated settings
        Settings.AutoLoadCurrentSettings(Settings.SettingsFileName);
        
        graphics.SynchronizeWithVerticalRetrace = Settings.CurrentSettings.vsync;
        graphics.IsFullScreen = Settings.CurrentSettings.fullscreen;
        IsMouseVisible = Settings.CurrentSettings.showMouse;
        TargetElapsedTime = TimeSpan.FromSeconds(1.0d / Settings.CurrentSettings.targetFramerate);
        
        // Update window size
        graphics.PreferredBackBufferWidth = Settings.ResolutionX;
        graphics.PreferredBackBufferHeight = Settings.ResolutionY;
        graphics.ApplyChanges();
        
        // Update the configuration of all the scripts
        Configurable.ApplySettingsToAll();
    }
    
    protected override void Update(GameTime gameTime)
    {
        _inputHandler.Update();
        _camera.Update();
    }

    protected override void Draw(GameTime gameTime) 
    {
        _frameRenderer.DrawFrame(gameTime);
        _frameStats.Update(gameTime);
    }
    
    private static void OpenTextEditor(ref Process process, string fileName) {
        if (process is { HasExited: false }) return;
        string fileToOpen =  Path.Combine(Path.GetDirectoryName(AppContext.BaseDirectory) ?? string.Empty, fileName);

        process = new Process();
        process.StartInfo = new ProcessStartInfo(fileToOpen) { UseShellExecute = true };
        process.Start();
    }

    private void CreateInputEvents() {
        _inputHandler.GetAction(Keys.H) += state => {
            if (state) IsMenuOpen ^= true;
        };
        
        _inputHandler.GetAction(Keys.D) += state => {
            if (state) CurrentMenu = Menu.Details;
        };
        
        _inputHandler.GetAction(Keys.B) += state => {
            if (state) CurrentMenu = Menu.Main;
        };
        
        _inputHandler.GetAction(Keys.G) += state => {
            if (state) CurrentMenu = Menu.Credits;
        };
        
        _inputHandler.GetAction(Keys.S) += state => {
            if (state) CurrentMenu = Menu.Settings;
        };
        
        _inputHandler.GetAction(Keys.C) += state => {
            if (state) CurrentMenu = Menu.Controls;
        };
        
        _inputHandler.GetAction(Keys.Back) += state => {
            if (state) Exit();
        };
        
        _inputHandler.GetAction(Keys.Enter) += state => {
            if (state) ApplySettings();
        };
        
        _inputHandler.GetAction(Keys.O) += state => {
            if (state) OpenTextEditor(ref settingsProcess, Settings.SettingsFileName);
        };
        
        _inputHandler.GetAction(Keys.P) += state => {
            if (state) OpenTextEditor(ref defaultsProcess, Settings.DefaultsFileName);
        };
        
        _inputHandler.GetAction(Keys.D1) += state => {
            if (state) FrameRenderer.Instance.SetVideoPoint(true);
        };
        
        _inputHandler.GetAction(Keys.D2) += state => {
            if (state) FrameRenderer.Instance.SetVideoPoint(false);
        };
    }
}
