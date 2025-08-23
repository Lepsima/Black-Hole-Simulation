# A performant Black Hole renderer in C# and HLSL
This renderer does it's best to ray-trace in real time an interactive black hole with an accretion disk, you can move rotate and zoom in anywhere you want

<img width="1570" height="892" alt="bh_skybox" src="https://github.com/user-attachments/assets/aca08b01-17f1-4d96-ba39-3193715cae76" />

## Main Features:
- Light bending
- Accretion disk with volume (NOT volumetric, just 3D)
- Accretion disk's light emmision based on temperature and measured in nm wavelength
- wavelength redshift based on the disk's light direction
- the modified wavelength back to rgb
- Skybox distorsion, the stars in the background will also distort

<img width="1740" height="634" alt="bh_blue" src="https://github.com/user-attachments/assets/8e21cc71-cd43-4706-ba52-e94bcbcf2969" />

## Performance 
I tested on 2 devices with the GPUs: rtx4060 (desktop) and Intel Raptor Lake-P (laptop), giving the following results:
- Desktop: 1920x1080p at ~300fps average (depends on the distance) 
- Laptop: 1280x720p at 60-30fps average (depends on the distance) 

<img width="1566" height="616" alt="bh_rainbow" src="https://github.com/user-attachments/assets/031526c8-ec79-426c-82cd-2db85f574bc5" />

## Settings
Almost all the fun parameters are exposed in a .json file for you to edit at your pleasure

Some of the best parameters to change and play with are:
- FOV
- Step size (increase this one for a performace boost)
- Disk radius
- Disk temperature (this one will change the percieved color in the final image)
- Disk velocity
 
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
