RWTexture2D<float4> output;  // The texture to write the ray color to
Texture2D<float1> noise_tex; // The noise texture has only 1 channel (red), since it is greyscale
Texture2D<float3> background_tex; // Skybox texture, 3 channels, no alpha
const int width;
const int height;
const float disk_time;
const float skybox_brightness = 0.5f;

// Configurable properties, these get passed before the dispatch call
const float r_s = 1.0f;
const float disk_r1 = 3.5f;
const float disk_r2 = 15.0f;
const float disk_y = 0.2f;
const float base_step_size = 0.1;    // Length of each step

// Minimum and Maximum temperature of the accretion disk
const float MaxTemp = 4900.0f; // Def -> 4900.0f
const float MinTemp = 4300.0f; // Def -> 4300.0f
const float MaxVel = 0.1f;
const float MinVel = 0.01f;

// Camera properties, also get passed before the dispatch call
const float cam_aspect;
const float cam_tan_half_fov;
const float3 cam_pos;
const float3 cam_right;
const float3 cam_up;
const float3 cam_forward;

static float escape_r = disk_r2 * 1.25f;
static const float PI = 3.141592653589793f;
static const float TAU = PI * 2.0f;
static const float b = 2897.772f;                   // Will return a wavelength when divided by a kelvin temperature

// Converts a wavelength in nm to an RGB color
// There has to be a better way to do this...
float3 wavelength_to_rgb(float nm)
{
    float3 color = float3(0.0f, 0.0f, 0.0f);
    if (nm < 380 || nm > 780) return color;
    
    if (nm < 440) {
        color.r = -(nm - 440.0) / (440.0 - 380.0);
        color.b = 1.0;
        
    } else if (nm < 490) {
        color.g = (nm - 440.0) / (490.0 - 440.0);
        color.b = 1.0;
        
    } else if (nm < 510) {
        color.g = 1.0;
        color.b = -(nm - 510.0) / (510.0 - 490.0);
        
    } else if (nm < 580) {
        color.r = (nm - 510.0) / (580.0 - 510.0);
        color.g = 1.0;
        
    } else if (nm < 645) {
        color.r = 1.0;
        color.g = -(nm - 645.0) / (645.0 - 580.0);
        
    } else {
        color.r = 1.0;
    }

    float factor;
    
    if (nm < 420) {
        factor = 0.3 + 0.7 * (nm - 380.0) / (420.0 - 380.0);
        
    } else if (nm < 701) {
        factor = 1.0f;
        
    } else {
        factor = 0.3 + 0.7 * (780.0 - nm) / (780.0 - 700.0);
    }
    
    return saturate(pow(color * factor, 0.80f));
}

// Sets the pixel of the output texture to the desired color
void set_pixel(uint2 pixel, float4 color)
{
    output[pixel] = color;
}

// Gets the uv value for a pixel
float2 get_uv(float2 pixel)
{
    float u = pixel.x / width;
    float v = pixel.y / height;
    
    return float2(u, v);
}

float get_noise_at(float r01, float3 pos)
{
    float phi01 = atan2(pos.z, pos.x) / TAU + 0.5f;

    uint x = floor(phi01 * 256.0f + disk_time) % 256;
    uint y = floor(r01 * 512.0f + disk_time * 0.5f) % 512;
    
    uint2 tex_coord = uint2(x, y);
    return noise_tex[tex_coord];
}

float sign_mod(float x, float y)
{
    return fmod(fmod(x, y) + y, y);
}

float3 get_background_at(float3 dir)
{
    float phi = atan2(dir.x, dir.z);
    float theta = atan2(dir.y, sqrt(dir.x*dir.x + dir.z*dir.z));
    
    float2 uv = float2(sign_mod(phi + 4.5f, TAU) / TAU, (theta + PI / 2.0f) / PI);
    return background_tex[uint2(uv.x * 4096, uv.y * 2048)];
}

// Converts a world point to a pixel coordinate
uint2 world_to_pixel(float3 pos)
{
    float3 rel = pos - cam_pos;
    float cam_x = dot(rel, -cam_right);
    float cam_y = dot(rel, cam_up);
    float cam_z = dot(rel, cam_forward);
    
    float ndc_x = cam_x / (cam_z * cam_tan_half_fov * cam_aspect);
    float ndc_y = cam_y / (cam_z * cam_tan_half_fov);
    
    float pixel_x = (ndc_x * 0.5f + 0.5f) * width;
    float pixel_y = (1 - (ndc_y * 0.5f + 0.5f)) * height;
    return uint2(pixel_x, pixel_y);
}

// Converts a pixel coordinate to the direction it's ray should go
float3 pixel_to_dir(uint2 pixel)
{
    float2 uv = get_uv(pixel);
    
    float ndc_x = uv.x * 2.0f - 1.0f;
    float ndc_y = uv.y * 2.0f - 1.0f;
    
    float px = ndc_x * cam_tan_half_fov * cam_aspect;
    float py = ndc_y * cam_tan_half_fov;

    return normalize(px * cam_right + py * cam_up + cam_forward);
}

float disk_wavelength(float r01, float3 new_pos, float3 prev_pos)
{
    // Get disk temperature in Kelvin
    float x = r01 * 0.5f - 0.9f;
    float disk_temp01 = sin(x*x * x*x * PI);
    float disk_temp = disk_temp01 * (MaxTemp - MinTemp) + MinTemp;

    // Disk REAL color wavelength in nm
    float wavelength = b / disk_temp;

    // How aligned are the disk and ray directions
    float alignment = dot(normalize(float3(-new_pos.z, 0.0f, new_pos.x)), normalize(new_pos - prev_pos));
            
    // Get disk local and relative velocity to the ray 
    float disk_vel = pow(1.0f - r01, 3) * (MaxVel - MinVel) + MinVel;
    float disk_rel_velocity = disk_vel * alignment;

    // Redshift/Blueshift the light wavelength using the relativistic doppler effect
    float v_ratio = disk_rel_velocity;
    return sqrt((1.0f - v_ratio) / (1.0f + v_ratio)) * wavelength;
}

float3 ray_start_jump(float3 pos, float3 dir)
{
    float jump_r = disk_r2 * 1.1f;
    float r = length(pos);

    if (r < jump_r) return float3(0.0f, 0.0f, 0.0f);
    return (r - jump_r) * dir;
}

bool is_ray_relevant(float3 pos, float3 dir)
{
    float3 a = dir*dir;
    float3 b = 2.0f * dir * pos;
    float3 c = pos*pos - escape_r*escape_r;
    float3 d = b*b - 4.0f * a * c;

    return d >= 0;
}

float sqr_length(float3 v)
{
    return dot(v, v);
}

float disk_energy_curve(float r01)
{
    // Energy density curve
    float inner = sin(min(r01 * 1.25f + 0.025f, 0.25f) * TAU);
    float outer = sin((r01 * 0.75f - 1.75f) * PI);
    return min(inner, outer);
}

// Simulates a ray with origin: [pos] and direction: [dir]
float4 raycast(float3 pos, float3 dir, int steps)
{
    float sqr_disk_r1 = disk_r1 * disk_r1;
    float sqr_disk_r2 = disk_r2 * disk_r2;
    float sqr_escape_r = escape_r * escape_r;
    
    float h2 = sqr_length(cross(pos, dir));
    float step = base_step_size;
    
    for (int i = 0; i < steps; i++)
    {
        float3 prev_pos = pos;
        pos += dir * step;
        
        float sqr_pr = sqr_length(pos);

        // Entered black hole, the color should be black :/
        if (sqr_pr < r_s)
        {
            return float4(0.0f, 0.0f, 0.0f, 1.0f);
        }

        float pr = sqrt(sqr_pr);
        bool going_out = sqr_length(pos + dir) > sqr_pr;

        // Ran outside the simulation range
        if (sqr_pr > sqr_escape_r && going_out)
        {
            break;
        }

        float abs_y = abs(pos.y);
        if (abs_y < disk_y && sqr_pr < sqr_disk_r2 && sqr_pr > sqr_disk_r1)
        {
            // From 0 to 1, 0 is the innermost radius and 1 is the outermost radius
            float r01 = (pr - disk_r1) / (disk_r2 - disk_r1);
            float curve = disk_energy_curve(r01);
            
            if (abs_y < disk_y * curve)
            {
                // Get the redshift of the temperature wavelength of the disk and convert to RGB
                float wavelength = disk_wavelength(r01, pos, prev_pos) * (780.0f - 380.0f) + 380.0f;
                float3 wl_rgb = wavelength_to_rgb(wavelength);
                
                float noise = get_noise_at(r01, pos);
                return float4(wl_rgb * curve * noise, 1.0f);
            }
        }

        // Dynamic step size
        float outside_range_mod = max(0, pr - escape_r) * 3.0f * base_step_size;
        float going_outside_mod = going_out ? base_step_size * 0.5f : 0.0f;
        
        step = base_step_size + outside_range_mod + going_outside_mod;
        step *= min(max((abs_y + disk_y) * 0.25f, 0.05f), 1.0f);

        // The magic "-3/2 * h2 * r^(-5)"
        dir += -1.5f * h2 * pos / (sqr_pr * sqr_pr * pr) * step;
    }

    // Render skybox for any other cases
    return float4(get_background_at(normalize(dir)) * skybox_brightness, 1.0f);
}

#define THREADS 8
[numthreads(THREADS, THREADS, 1)]
void compute(uint3 global_id : SV_DispatchThreadID)
{
    uint2 pixel = global_id.xy;
    float3 dir = pixel_to_dir(pixel);

    if (pixel.x >= width || pixel.y >= height || !is_ray_relevant(cam_pos, dir)) return;
    set_pixel(pixel, raycast(cam_pos, dir, 10000));
}

//================================================================================================
// Monogame stuff
//================================================================================================
technique Tech0
{
    pass Pass0
    {
        ComputeShader = compile cs_5_0 compute();
    }
}