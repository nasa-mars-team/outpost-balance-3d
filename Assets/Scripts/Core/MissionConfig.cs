using UnityEngine;

namespace OutpostBalance.Core
{
    public enum SiteType { Moon, Mars }

    [System.Serializable]
    public class SiteProfile
    {
        public SiteType siteType;
        public string nameEn;
        public string nameUz;
        public string dayLabelEn;
        public string dayLabelUz;
        public float commDelaySeconds;
        public string commLabelEn;
        public string commLabelUz;
        public string gravity;
        public string tempRange;
        public string atmosphereEn;
        public string atmosphereUz;
        public int baseSolarOutput;
        public string factEn;
        public string factUz;
        public string source;
        public Color skyColor;
        public Color groundColor;
        public Color groundAccentColor;
    }

    public static class MissionConfig
    {
        public const int MaxDays = 7;

        public static readonly SiteProfile Moon = new SiteProfile
        {
            siteType = SiteType.Moon,
            nameEn = "Moon: Shackleton Ridge (Artemis)",
            nameUz = "Oy: Sheklton tizmasi (Artemis)",
            dayLabelEn = "Day",
            dayLabelUz = "Kun",
            commDelaySeconds = 1.3f,
            commLabelEn = "1.3 seconds",
            commLabelUz = "1.3 soniya",
            gravity = "0.16 g (1/6 Earth)",
            tempRange = "-130°C to +120°C",
            atmosphereEn = "None (Vacuum)",
            atmosphereUz = "Yo'q (Vakuum)",
            baseSolarOutput = 10,
            factEn = "At the Lunar South Pole, sunlight hits crater rims at a low angle—giving power on ridges while nearby craters stay freezing dark!",
            factUz = "Oyning Janubiy qutbida quyosh nuri krater qirralariga qiya tushadi: tepaliklarda yorug'lik bo'lsa, chuqurlar muzlagan qorong'ilikda qoladi!",
            source = "NASA Artemis III Science Definition & LRO",
            skyColor = new Color(0.03f, 0.05f, 0.12f),
            groundColor = new Color(0.45f, 0.48f, 0.53f),
            groundAccentColor = new Color(0.28f, 0.30f, 0.35f)
        };

        public static readonly SiteProfile Mars = new SiteProfile
        {
            siteType = SiteType.Mars,
            nameEn = "Mars: Jezero Crater (Perseverance)",
            nameUz = "Mars: Jezero krateri (Perseverance)",
            dayLabelEn = "Sol",
            dayLabelUz = "Sol",
            commDelaySeconds = 720f,
            commLabelEn = "12 minutes (avg)",
            commLabelUz = "12 daqiqa (o'rtacha)",
            gravity = "0.38 g (~1/3 Earth)",
            tempRange = "-95°C to 0°C",
            atmosphereEn = "Thin CO2 (1% Earth)",
            atmosphereUz = "Yupqa CO2 (Yerning 1%)",
            baseSolarOutput = 9,
            factEn = "A Martian day is called a 'Sol' (24h 39m). Mars gets ~43% of Earth's sunlight, and dust storms can block solar panels for days!",
            factUz = "Marsdagi bir kun 'Sol' deyiladi (24s 39d). Mars ~43% quyosh nuri oladi, chang bo'ronlari esa panellarni kunlab to'sadi!",
            source = "NASA Mars 2020 MEDA & Curiosity REMS",
            skyColor = new Color(0.32f, 0.14f, 0.08f),
            groundColor = new Color(0.68f, 0.33f, 0.21f),
            groundAccentColor = new Color(0.46f, 0.20f, 0.12f)
        };

        public static SiteProfile GetProfile(SiteType site) => site == SiteType.Moon ? Moon : Mars;
    }
}
