using System;
using System.Collections.Generic;
using UnityEngine;

namespace OutpostBalance.Core
{
    [Serializable]
    public class DecisionLogEntry
    {
        public int day;
        public string eventTitle;
        public string decisionSummary;
        public string statsSnapshot;
    }

    /// <summary>
    /// Tracks the 3 core survival resources (Power, LifeSupport, Shielding) and Science progress.
    /// </summary>
    public class ResourceSystem
    {
        public float Power { get; private set; }
        public float LifeSupport { get; private set; }
        public float Shielding { get; private set; }
        public float Science { get; private set; }

        public bool CropsWilted { get; set; }
        public int DustStormDays { get; set; }
        public int ShadowDays { get; set; }
        public bool SolarFlareActive { get; set; }

        public List<DecisionLogEntry> History { get; } = new List<DecisionLogEntry>();
        public List<string> RippleNotes { get; } = new List<string>();

        public void Reset()
        {
            Power = 75f;
            LifeSupport = 80f;
            Shielding = 80f;
            Science = 0f;
            CropsWilted = false;
            DustStormDays = 0;
            ShadowDays = 0;
            SolarFlareActive = false;
            History.Clear();
            RippleNotes.Clear();
        }

        public void ApplyDelta(float powerDelta, float lifeDelta, float shieldDelta, float scienceDelta)
        {
            Power = Mathf.Clamp(Power + powerDelta, 0f, 100f);
            LifeSupport = Mathf.Clamp(LifeSupport + lifeDelta, 0f, 100f);
            Shielding = Mathf.Clamp(Shielding + shieldDelta, 0f, 100f);
            Science = Mathf.Clamp(Science + scienceDelta, 0f, 100f);
        }

        public bool IsFailed() => Power <= 0.01f || LifeSupport <= 0.01f || Shielding <= 0.01f;

        public string GetFailureReason(bool isUz)
        {
            if (Power <= 0.01f)
                return isUz ? "Batareya quvvati 0% ga tushdi!" : "Power reserves hit 0%!";
            if (LifeSupport <= 0.01f)
                return isUz ? "Kislorod va Hayot Tizimi 0% ga tushdi!" : "Life Support (O2 & Food) hit 0%!";
            if (Shielding <= 0.01f)
                return isUz ? "Radiatsiya Qalqoni 0% ga tushdi!" : "Radiation Shielding hit 0%!";
            return string.Empty;
        }
    }
}
