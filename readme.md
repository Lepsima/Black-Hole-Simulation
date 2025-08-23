# A performant Black Hole renderer in C# and HLSL
This renderer does it's best to ray-trace in real time an interactive black hole with an accretion disk (or without, whatever you want). You can move rotate and zoom in anywhere you want around it (or inside, kinda)

<img width="1856" height="660" alt="bh_red_skybox_dim" src="https://github.com/user-attachments/assets/722e9d5f-c888-422a-b65d-df8ef9bcb685" />

_The standard accretion disk look._
<br />

## Main Features:
- Accurate enough light path distortion
- Accretion disk with volume and a bit of noise detail(NOT volumetric, just 3D)
- Accretion disk's color is based on temperature and emmited as a wavelength
- The emmited wavelength is subject to red and blueshift and then converted back to an accurate RGB representation
- Skybox distorsion, the stars in the background will also distort as the rays bend
- No rendering or wait time, all the rays are simulated in real time at very decent framerates (for black hole standards)

<img width="1740" height="634" alt="bh_blue" src="https://github.com/user-attachments/assets/8e21cc71-cd43-4706-ba52-e94bcbcf2969" />

_A blue accretion disk by using hotter temperatures._
<br />

## Performance 
I tested on 2 devices with the GPUs: rtx4060 (desktop) and Intel Raptor Lake-P (laptop)<br />
At the same quality settings as the images shown around here

Results:
- Desktop: 1920x1080p at ~300fps average (depends on the distance) 
- Laptop: 1280x720p at 60-30fps average (depends on the distance) 

<img width="1566" height="616" alt="bh_rainbow" src="https://github.com/user-attachments/assets/031526c8-ec79-426c-82cd-2db85f574bc5" />

_Rainbow disk by further alterating the settings to whatever this is: 3000K at the tip, +20000k at the center._
<br />

## Customization & How to change things
Almost all the fun parameters are exposed in a .json file for you to edit at your pleasure

The settings file is auto-generated after the first run, and can be opened in-app by clicking "O". (Also, press "P" to see the default settings).<br />
*If you run linux you may have to manually open the files (sorry), they should generate in the same directory as the executable.

After saving the changes, press "Enter" in-app and the changes should apply and persist after closing the window.

Some of the best parameters to change and play with are:
- FOV
- Step size (increase this one for a performace boost)
- Disk radius (you can disable the disk by setting the MIN and MAX to the same number)
- Disk temperature (this one will change the percieved color in the final image)
- Disk velocity

<img width="1106" height="654" alt="bh_no_disk" src="https://github.com/user-attachments/assets/535933a8-b821-46a7-a4ab-225d30a4bb78" />

_No disk this time, can be done by setting the Max & Min radius to the same number. *NOTE: lower a bit the step size for this one, the skybox can show some artifacts otherwise._
<br />

## How it works
The main program handles the camera movement, input, menus and other configurations.

Each frame, a compute shader is dispatched to handle all the pixels, (i tried using a pixel shader but ran into timeout problems).<br />
Every pixel sends a ray that can end up in 3 possible places:
- Too close to the black hole -> The pixel is black
- Exceeds the simulation range -> The skybox texture is applied
- It crosses the disk plane at a valid radius -> All the accretion disk calculations are performed and the proper color is displayed

After that, bloom is applied (see credits below) and the menu is rendered.

<img width="1570" height="892" alt="bh_skybox" src="https://github.com/user-attachments/assets/aca08b01-17f1-4d96-ba39-3193715cae76" />

_The same standard look, different angle and better skybox visibility._
<br />
 
# Credits & Links

## _"The magical  `-1.5 * h2 * r^(-5)`"_
A very FAST equation to get the direction of a light ray near a black hole

### Riccardo Antonelli
Links:
- https://github.com/rantonels
- https://github.com/rantonels/starless

---

## Monogame fork for Compute Shaders
A fork of the monogame framework that enables modern shader features not supported by default, such as compute shaders needed for simulating the rays

### Markus Hötzinger (cpt-max)
Links:
- https://github.com/cpt-max
- https://github.com/cpt-max/MonoGame

---

## Bloom post-processing filter for Monogame/XNA
Highly customizable bloom filter for making bright pixels stand out

### Kosmonaut3d
Links:
- https://github.com/Kosmonaut3d
- https://github.com/Kosmonaut3d/BloomFilter-for-Monogame-and-XNA

---

### Me, Lepsima, for everything else in the project
Links:
- https://github.com/Lepsima
- https://lepsima.itch.io/
