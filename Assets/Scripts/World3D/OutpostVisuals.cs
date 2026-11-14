using UnityEngine;
using OutpostBalance.Core;
using OutpostBalance.Gameplay;

namespace OutpostBalance.World3D
{
    /// <summary>
    /// Synchronizes 3D world geometry, materials, lights, shield dome, plants, and particle VFX with game state.
    /// </summary>
    public class OutpostVisuals : MonoBehaviour
    {
        [Header("Environment")]
        public Camera mainCam;
        public Light sunLight;
        public Renderer groundRenderer;
        public Renderer[] craterRenderers;
        public GameObject earthInSky;

        [Header("Outpost Modules")]
        public Renderer[] solarPanelRenderers;
        public Renderer[] habWindowRenderers;
        public Renderer[] plantRenderers;
        public Renderer shieldBubbleRenderer;
        public Transform commsDishTransform;
        public RoverPatrol roverPatrol;

        [Header("VFX")]
        public ParticleSystem solarFlareParticles;
        public ParticleSystem dustStormParticles;

        private int _currentShieldLevel = 2;
        private int _currentLabLevel = 2;
        private bool _flareActive;

        public void SyncWorld(SiteType site, ResourceSystem res, PowerAllocator alloc)
        {
            var profile = MissionConfig.GetProfile(site);
            _currentShieldLevel = alloc.ShieldLevel;
            _currentLabLevel = alloc.LabLevel;
            _flareActive = res.SolarFlareActive;

            if (mainCam != null)
            {
                mainCam.backgroundColor = profile.skyColor;
            }
            if (RenderSettings.fog)
            {
                RenderSettings.fogColor = profile.skyColor;
                RenderSettings.fogDensity = (site == SiteType.Mars && res.DustStormDays > 0) ? 0.045f : 0.012f;
            }

            if (groundRenderer != null)
                groundRenderer.material.color = profile.groundColor;

            if (craterRenderers != null)
            {
                foreach (var cr in craterRenderers)
                    if (cr != null) cr.material.color = profile.groundAccentColor;
            }

            if (earthInSky != null)
                earthInSky.SetActive(site == SiteType.Moon);

            if (sunLight != null)
                sunLight.color = _flareActive ? new Color(1f, 0.55f, 0.25f) : (site == SiteType.Mars ? new Color(1f, 0.85f, 0.72f) : Color.white);

            // Solar panels dim when Mars dust storm covers them
            bool dusty = site == SiteType.Mars && res.DustStormDays > 0;
            if (solarPanelRenderers != null)
            {
                Color panelCol = dusty ? new Color(0.45f, 0.32f, 0.26f) : new Color(0.12f, 0.34f, 0.72f);
                foreach (var sp in solarPanelRenderers)
                    if (sp != null) sp.material.color = panelCol;
            }

            // Habitat windows warn red if Life Support low
            if (habWindowRenderers != null)
            {
                Color winCol = res.LifeSupport > 35f ? new Color(1f, 0.88f, 0.35f) : new Color(0.95f, 0.25f, 0.22f);
                foreach (var w in habWindowRenderers)
                {
                    if (w != null)
                    {
                        w.material.color = winCol;
                        if (w.material.HasProperty("_EmissionColor"))
                            w.material.SetColor("_EmissionColor", winCol * 1.4f);
                    }
                }
            }

            // Hydroponic plants grow lush green or shrink yellow if neglected/wilted
            if (plantRenderers != null)
            {
                bool healthy = alloc.GreenhouseLevel >= 2 && !res.CropsWilted;
                Color plantCol = healthy ? new Color(0.16f, 0.78f, 0.32f)
                    : (alloc.GreenhouseLevel == 1 ? new Color(0.55f, 0.75f, 0.18f) : new Color(0.75f, 0.52f, 0.12f));
                float scaleY = healthy ? 1.25f : (alloc.GreenhouseLevel == 1 ? 0.9f : 0.55f);

                foreach (var pl in plantRenderers)
                {
                    if (pl == null) continue;
                    pl.material.color = plantCol;
                    var s = pl.transform.localScale;
                    pl.transform.localScale = new Vector3(s.x, scaleY, s.z);
                }
            }

            if (roverPatrol != null)
                roverPatrol.SetPowered(alloc.LabLevel);

            // Particle VFX
            UpdateParticle(solarFlareParticles, _flareActive);
            UpdateParticle(dustStormParticles, dusty);
        }

        private void UpdateParticle(ParticleSystem ps, bool active)
        {
            if (ps == null) return;
            if (active && !ps.isPlaying) ps.Play();
            else if (!active && ps.isPlaying) ps.Stop();
        }

        private void Update()
        {
            // Rotate radar dish when Science Lab is powered
            if (commsDishTransform != null && _currentLabLevel > 0)
            {
                commsDishTransform.Rotate(Vector3.up, (12f + _currentLabLevel * 12f) * Time.deltaTime, Space.World);
            }

            // Pulse translucent radiation shield dome
            if (shieldBubbleRenderer != null)
            {
                bool showDome = _currentShieldLevel > 0 || _flareActive;
                shieldBubbleRenderer.enabled = showDome;
                if (showDome)
                {
                    float pulse = 0.06f * Mathf.Sin(Time.time * 3f);
                    float alpha = Mathf.Clamp(0.08f + _currentShieldLevel * 0.065f + pulse, 0.05f, 0.42f);
                    Color baseCol = _currentShieldLevel >= 2
                        ? new Color(0.22f, 0.82f, 1f, alpha)
                        : new Color(1f, 0.36f, 0.32f, alpha);
                    shieldBubbleRenderer.material.color = baseCol;
                }
            }
        }
    }
}
