using System;
using UnityEngine;
using OutpostBalance.Core;

namespace OutpostBalance.Gameplay
{
    /// <summary>
    /// Simulates radio light-speed delay differences between Moon (1.3s) and Mars (~12 min).
    /// </summary>
    public class CommDelayMgr : MonoBehaviour
    {
        public bool IsRunning { get; private set; }
        public float Progress01 { get; private set; }
        public SiteType ActiveSite { get; private set; }

        private float _elapsed;
        public event Action<float> OnProgressChanged;

        public void Begin(SiteType site)
        {
            ActiveSite = site;
            IsRunning = true;
            _elapsed = 0f;
            Progress01 = site == SiteType.Mars ? 1f : 0.1f;
            OnProgressChanged?.Invoke(Progress01);
        }

        public void Stop()
        {
            IsRunning = false;
        }

        private void Update()
        {
            if (!IsRunning) return;

            _elapsed += Time.deltaTime;
            if (ActiveSite == SiteType.Moon)
            {
                Progress01 = Mathf.Clamp01(_elapsed / 1.3f);
            }
            else
            {
                // Mars ~12 min: pulse countdown visually to communicate inability to wait for Earth
                Progress01 = 1f - ((_elapsed * 0.18f) % 0.9f);
            }
            OnProgressChanged?.Invoke(Progress01);
        }
    }
}
