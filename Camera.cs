using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BlackHoles;

public class Camera {
	public static Camera Instance { get; private set; }
	private const float maxRadius = 50f;
	private const float minRadius = 0.5f;
	
	private const float zoomSpeed = 0.0025f;
	private const float orbitSpeed = 0.0025f;
	private const float aimSpeed = 0.0025f;
	
	private float radius = 15.0f;
	private float orbit_azimuth;
	private float orbit_elevation = float.Pi / 1.9f;

	private float aim_azimuth = float.Pi;
	private float aim_elevation = float.Pi / 1.9f;
	
	private Point lastMousePosition;
	private int lastScroll;

	public Camera() {
		Instance = this;
	}

	public Vector3[] GetShaderData() {
		Vector3[] data = new Vector3[4];
		data[0] = GetVector(orbit_elevation, orbit_azimuth);
		data[1] = Vector3.Normalize(GetVector(aim_elevation, aim_azimuth + orbit_azimuth));
		data[2] = Vector3.Normalize(Vector3.Cross(data[1], Vector3.Up));
		data[3] = Vector3.Normalize(Vector3.Cross(data[2], data[1]));
		return data;
	}

	private Vector3 GetVector(float elevation, float azimuth) {
		return new Vector3(
			radius * MathF.Sin(elevation) * MathF.Cos(azimuth),
			radius * MathF.Cos(elevation),
			radius * MathF.Sin(elevation) * MathF.Sin(azimuth)
		);
	}
	
	public void Update() {
		MouseState mouseState = Mouse.GetState();
		
		// Scroll delta
		int scroll = mouseState.ScrollWheelValue;
		int deltaScroll = scroll - lastScroll;
		lastScroll = scroll;
		
		// Position delta
		Point mousePosition = mouseState.Position; 
		Point delta = mousePosition - lastMousePosition;
		lastMousePosition = mousePosition;
		
		// Zoom
		radius = Math.Clamp(radius - deltaScroll * zoomSpeed, minRadius, maxRadius);
		
		// Camera movement
		if (delta is { X: 0, Y: 0 }) return;

		// Movement type
		bool isOrbiting = mouseState.LeftButton == ButtonState.Pressed;
		bool isMiddleClicking = mouseState.MiddleButton == ButtonState.Pressed;
		bool isAiming = mouseState.RightButton == ButtonState.Pressed;
		
		// Orbit camera around center
		if (isOrbiting && !isAiming) {
			// Elevate up or down
			orbit_elevation -= delta.Y * orbitSpeed * 0.25f;
			orbit_elevation = Math.Clamp(orbit_elevation, 0.01f, float.Pi - 0.01f);
			
			// Horizontal orbit
			orbit_azimuth -= delta.X * orbitSpeed;

		} else if (isAiming) {
			// Aim camera without position movement
			aim_azimuth += delta.X * aimSpeed;
			aim_elevation -= delta.Y * aimSpeed;
			aim_elevation = Math.Clamp(aim_elevation, 0.01f, float.Pi - 0.01f);
			
		} else if (isMiddleClicking) {
			// Elevate up or down
			orbit_elevation -= delta.Y * orbitSpeed * 0.25f;
			orbit_elevation = Math.Clamp(orbit_elevation, 0.01f, float.Pi - 0.01f);
			
			// Aim camera horizontally
			aim_azimuth += delta.X * aimSpeed;
		}
	}
}