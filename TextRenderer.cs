using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BlackHoles;

public enum Menu {
	Main,
	Details,
	Settings,
	Credits,
	Controls
}

public class TextRenderer {
	public static TextRenderer Instance { get; private set; }
	
	private const float Margin = 20.0f;
	private const float Spacing = 10.0f;
	
	private Vector2 _textPosition;
	private SpriteBatch spriteBatch;
	private SpriteFont spriteFont;

	public void Load(ContentManager content, SpriteBatch spriteBatch) {
		this.spriteBatch = spriteBatch;
		spriteFont = content.Load<SpriteFont>("jetbrains_mono");
	}
	
	public TextRenderer() {
		Instance = this;
	}
	
	public string[] GetMenuText(Menu menu, bool isMenuOpen) {
		List<string> lines = [ isMenuOpen ? "[H] to close" : "[H] to open" ];
		if (!isMenuOpen) return lines.ToArray();
		
		switch (menu) {
            case Menu.Main:
                lines[0] += $" - Black hole simulation [{Core.VERSION} - {Core.BUILD}]";
                lines.Add("\nControls, can be used in any menu");
                lines.Add(" - [C] -> Camera Controls");
                lines.Add(" - [D] -> View details");
                lines.Add(" - [S] -> Settings");
                lines.Add(" - [G] -> Credits");
                lines.Add(" - [B] -> Back (this menu)");
                lines.Add(" - [Backspace] / [Delete] -> Quit");
                break;
                
            case Menu.Details:
                lines[0] += " - Simulation Details";
                lines.Add($"{Settings.ResolutionX}x{Settings.ResolutionY} - {FrameStats.Instance.FPS:0.00}fps - {FrameStats.Instance.DeltaTime:0.000}ms");
                lines.Add("\nSimulation scale: " +
                          "\n  1r = 1 schwarzschild radius");
                
                lines.Add("\nDisk radius:" +
                          $"\n  Inner {Settings.CurrentSettings.diskInnerRadius:0.0}r " +
                          $"\n  Outer {Settings.CurrentSettings.diskOuterRadius:0.0}r");
                
                float simRange = Settings.CurrentSettings.diskOuterRadius * 1.25f;
                lines.Add("\nRay simulation range: " +
                          $"\n  {simRange:0.00}r");
                
                lines.Add("\nRay step size: " +
                          $"\n  {Settings.CurrentSettings.simulationStepSize:0.0}r");
                
                lines.Add("\n\n[B] Go back");
                break;
            
            case Menu.Credits:
                lines[0] += " - Credits - Links inside \"readme.md\"";
                
                lines.Add("\n  \"the magical -3/2 * h2 * r^(-5)\" -> Riccardo Antonelli");
                lines.Add("\n  Bloom effect for Monogame/XNA -> Kosmonaut3d");
                lines.Add("\n  Monogame fork for Compute Shaders -> Markus Hotzinger (cpt-max)");
                lines.Add("\n  Made by Lepsima");
                
                lines.Add("\n\n[B] Go back");
                break;
            
            case Menu.Settings:
                lines[0] += " - Settings";
                
                lines.Add($"\nTo edit the simulation settings, open the generated file \"{Settings.SettingsFileName}\"" +
                          "\ninside the extracted folder");
                
                lines.Add("\nYou can also press [O] to automatically open the settings file in your default text editor");
                lines.Add("Or if you want to access the default values, press [P]. The default values are" +
                          "\nREAD ONLY, changing them will have no effect");
                
                lines.Add("\nOnce all changes are saved, press [Enter] in this window to apply all the changes");
                lines.Add("\nIf you want to revert to the default settings, copy all the contents found" +
                          $"\nin side the \"{Settings.DefaultsFileName}\" file.");
               
                lines.Add("\n\n[B] Go back");
                break;
            
            case Menu.Controls:
                lines[0] += " - Camera Controls";
                
                lines.Add("\nHold a mouse button down and drag");
                lines.Add("  [RMB] Right click -> Aim the camera");
                lines.Add("  [LMB] Left click -> Move the camera");
                lines.Add("  [MMB] Middle click -> Alternate movement, mixed controls");
                
                lines.Add("\n Use the [SCROLL] scroll wheel to zoom in and out");
                
                lines.Add("\n\n[B] Go back");
                break;
        }
		
        return lines.ToArray();
	}
	
	public void Draw(string[] text) {
		_textPosition = new Vector2(Margin, Margin);
		foreach (string s in text) DrawText(s);
	}

	private void DrawText(string text) {
		Vector2 textSize = spriteFont.MeasureString(text);
		
		spriteBatch.DrawString(spriteFont, text, _textPosition, Color.LightGreen,
			0, Vector2.Zero, 1.0f, SpriteEffects.None, 0.5f);
		
		_textPosition.Y += textSize.Y + Spacing;
	}
}