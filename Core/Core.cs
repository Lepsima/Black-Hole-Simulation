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
    public const string VERSION = "v1.0";
    public const string BUILD = "b1";
    
    private readonly GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;
    private Rectangle screenRect;

    private Camera _camera;
    private BlackHole _blackHole;
    private FrameStats _frameStats;
    private BloomFilter _bloomFilter;
    private TextRenderer _textRenderer;
    private InputHandler _inputHandler;

    private bool _isMenuOpen;
    private Menu _currentMenu = Menu.Main;

    private Process settingsProcess;
    private Process defaultsProcess;
    
    public Core()
    {
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
        IsMouseVisible = settings1.showMouse;
        TargetElapsedTime = TimeSpan.FromSeconds(1.0d / settings1.targetFramerate);

        // Set the graphics manager's settings
        graphics = new GraphicsDeviceManager(this);
        graphics.GraphicsProfile = GraphicsProfile.HiDef;
        graphics.SynchronizeWithVerticalRetrace = settings1.vsync;
        graphics.IsFullScreen = settings1.fullscreen;
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
    }

    private void ApplySettings() 
    {
        // Read updated settings
        Settings.AutoLoadCurrentSettings(Settings.SettingsFileName);
        
        // Update window size
        screenRect = new Rectangle(0, 0, Settings.ResolutionX, Settings.ResolutionY); 
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
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime) {
        // Render black hole and bloom filter
        Texture2D blackHoleTex = _blackHole.Draw(gameTime);
        Texture2D bloomTex = _bloomFilter.Draw(blackHoleTex);
        
        // Apply the rendered textures to the screen
        GraphicsDevice.SetRenderTarget(null);
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
        spriteBatch.Draw(blackHoleTex, screenRect, Color.White);
        spriteBatch.Draw(bloomTex, screenRect, Color.White);
        spriteBatch.End();
        
        // Draw the text menu
        spriteBatch.Begin();
        _textRenderer.Draw(_textRenderer.GetMenuText(_currentMenu, _isMenuOpen));
        spriteBatch.End();
        
        // End draw
        base.Draw(gameTime);
        _frameStats.Update(gameTime);
    }

    private static void OpenTextEditor(ref Process process, string fileName) {
        if (process is { HasExited: false }) return;
        string fileToOpen =  Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? string.Empty, fileName);

        process = new Process();
        process.StartInfo = new ProcessStartInfo(fileToOpen) { UseShellExecute = true };
        process.Start();
    }

    private void CreateInputEvents() {
        _inputHandler.GetAction(Keys.H) += state => {
            if (state) _isMenuOpen ^= true;
        };
        
        _inputHandler.GetAction(Keys.D) += state => {
            if (state) _currentMenu = Menu.Details;
        };
        
        _inputHandler.GetAction(Keys.B) += state => {
            if (state) _currentMenu = Menu.Main;
        };
        
        _inputHandler.GetAction(Keys.G) += state => {
            if (state) _currentMenu = Menu.Credits;
        };
        
        _inputHandler.GetAction(Keys.S) += state => {
            if (state) _currentMenu = Menu.Settings;
        };
        
        _inputHandler.GetAction(Keys.C) += state => {
            if (state) _currentMenu = Menu.Controls;
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
    }
}
