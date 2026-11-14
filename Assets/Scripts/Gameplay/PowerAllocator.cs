using OutpostBalance.Core;
using OutpostBalance.Data;
using UnityEngine;

namespace OutpostBalance.Gameplay
{
    public enum OutpostSubsystem { O2, Greenhouse, Shield, Lab }

    /// <summary>
    /// Handles daily solar power allocation across the 4 outpost modules and computes compounding trade-offs.
    /// </summary>
    public class PowerAllocator
    {
        public int O2Level { get; private set; } = 3;
        public int GreenhouseLevel { get; private set; } = 2;
        public int ShieldLevel { get; private set; } = 2;
        public int LabLevel { get; private set; } = 2;

        public void Reset()
        {
            O2Level = 3;
            GreenhouseLevel = 2;
            ShieldLevel = 2;
            LabLevel = 2;
        }

        public int GetLevel(OutpostSubsystem sys) => sys switch
        {
            OutpostSubsystem.O2 => O2Level,
            OutpostSubsystem.Greenhouse => GreenhouseLevel,
            OutpostSubsystem.Shield => ShieldLevel,
            OutpostSubsystem.Lab => LabLevel,
            _ => 0
        };

        public bool Adjust(OutpostSubsystem sys, int delta)
        {
            int cur = GetLevel(sys);
            int next = Mathf.Clamp(cur + delta, 0, 4);
            if (next == cur) return false;

            switch (sys)
            {
                case OutpostSubsystem.O2: O2Level = next; break;
                case OutpostSubsystem.Greenhouse: GreenhouseLevel = next; break;
                case OutpostSubsystem.Shield: ShieldLevel = next; break;
                case OutpostSubsystem.Lab: LabLevel = next; break;
            }
            return true;
        }

        public int TotalAllocated => O2Level + GreenhouseLevel + ShieldLevel + LabLevel;

        public int GetTodaySolarOutput(SiteType site, ResourceSystem res)
        {
            int baseOut = MissionConfig.GetProfile(site).baseSolarOutput;
            int penalty = 0;
            if (site == SiteType.Mars && res.DustStormDays > 0) penalty += 4;
            if (site == SiteType.Moon && res.ShadowDays > 0) penalty += 3;
            return Mathf.Max(4, baseOut - penalty);
        }

        public int PreviewBatteryDeltaPercent(SiteType site, ResourceSystem res)
        {
            int diff = GetTodaySolarOutput(site, res) - TotalAllocated;
            return diff >= 0 ? diff * 6 : diff * 9;
        }

        public void ExecuteAllocation(SiteType site, ResourceSystem res)
        {
            res.RippleNotes.Clear();
            int solar = GetTodaySolarOutput(site, res);
            int diff = solar - TotalAllocated;

            float powerDelta = diff >= 0 ? diff * 6f : diff * 9f;
            if (diff < 0)
            {
                res.RippleNotes.Add(LocalizationMgr.Pick(
                    $"Over-budget power drained Battery by {Mathf.Abs(diff * 9)}%.",
                    $"Ortiqcha quvvat sarfi Batareyani {Mathf.Abs(diff * 9)}% kamaytirdi."));
            }

            // O2 & Water Recycler
            float[] o2Table = { -20f, -9f, 2f, 7f, 12f };
            float lifeDelta = o2Table[O2Level];

            // Hydroponic Greenhouse + Compounding Ripple Effect
            if (GreenhouseLevel == 0)
            {
                res.CropsWilted = true;
                lifeDelta -= 5f;
                res.RippleNotes.Add(LocalizationMgr.Pick(
                    "Greenhouse unpowered: crops wilted (OSDR OSD-120) — tomorrow gives half food!",
                    "Issiqxona o'chdi: o'simliklar so'ldi (OSDR OSD-120) — ertaga hosil kamayadi!"));
            }
            else
            {
                float ghBonus = GreenhouseLevel * 3f;
                if (res.CropsWilted)
                {
                    ghBonus = Mathf.Floor(ghBonus * 0.5f);
                    res.CropsWilted = false;
                    res.RippleNotes.Add(LocalizationMgr.Pick(
                        "Recovering wilted crops gave half O2/food bonus today.",
                        "So'lgan o'simliklar tiklanmoqda — bugun yarmi foyda berdi."));
                }
                lifeDelta += ghBonus;
            }

            // Radiation Shielding
            float[] shieldTable = { -9f, -3f, 3f, 8f, 13f };
            float shieldDelta = shieldTable[ShieldLevel];

            // Science Lab (high output uses cooling water!)
            float sciDelta = LabLevel * 6f;
            if (LabLevel >= 3)
            {
                lifeDelta -= 3f;
                res.RippleNotes.Add(LocalizationMgr.Pick(
                    "High Science Lab load used extra cooling water (-3% Life Support).",
                    "Yuqori Laboratoriya yuki qo'shimcha sovutish suvi sarfladi (-3% Hayot)."));
            }

            if (res.DustStormDays > 0) res.DustStormDays--;
            if (res.ShadowDays > 0) res.ShadowDays--;

            res.ApplyDelta(powerDelta, lifeDelta, shieldDelta, sciDelta);
        }
    }
}
