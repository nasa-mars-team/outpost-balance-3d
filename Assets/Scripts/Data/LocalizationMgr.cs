using System;
using System.Collections.Generic;

namespace OutpostBalance.Data
{
    public enum GameLang { EN, UZ }

    public static class LocalizationMgr
    {
        public static GameLang Current { get; private set; } = GameLang.EN;
        public static event Action OnLanguageChanged;

        private static readonly Dictionary<string, (string en, string uz)> Dict =
            new Dictionary<string, (string, string)>
            {
                { "app_title", ("OUTPOST BALANCE 3D", "OUTPOST BALANCE 3D") },
                { "app_sub", ("NASA Mission Trade-Off Simulator • Ages 7–12+", "NASA Missiya Balansi Simulyatori • 7–12+ yosh") },
                { "choose_title", ("Choose Your 3D Outpost Mission", "3D Kosmik Baza Joyini Tanlang") },
                { "choose_desc", ("Every decision in space has a real cost! Balance Power, Life Support, and Shielding for 7 days using real NASA space weather & biology data.", "Kosmosda hech bir qaror tekin emas! Haqiqiy NASA ma'lumotlari asosida 7 kun davomida Quvvat, Hayot Tizimi va Qalqonni muvozanatlang.") },
                { "launch_moon", ("Launch Moon Base", "Oy Bazasini Boshlash") },
                { "launch_mars", ("Launch Mars Base", "Mars Bazasini Boshlash") },
                { "power", ("Power", "Quvvat") },
                { "life", ("Life Support", "Hayot Tizimi") },
                { "shield", ("Shielding", "Qalqon") },
                { "science", ("Science Goal", "Ilmiy Maqsad") },
                { "budget_title", ("Step 1: Split Today's Power Budget", "1-qadam: Bugungi Quvvatni Taqsimlang") },
                { "budget_desc", ("Click 3D buildings or use + / - buttons. No choice is free!", "3D binolarni bosing yoki + / - tugmalardan foydalaning!") },
                { "sys_o2", ("O2 & Water Recycler", "O2 va Suv Qayta Ishlash") },
                { "sys_gh", ("Hydroponic Greenhouse", "Gidroponik Issiqxona") },
                { "sys_shield", ("Radiation Shield Tower", "Radiatsiya Qalqon Minorasi") },
                { "sys_lab", ("Science & Radio Lab", "Ilmiy va Aloqa Lab") },
                { "run_day", ("Run Mission Day →", "Kunni Boshlash →") },
                { "over_budget", ("Over solar output! Extra energy drains Battery.", "Quyosh quvvatidan oshdi! Ortiqchasi Batareyadan olinadi.") },
                { "data_btn", ("NASA Data & Glossary", "NASA Ma'lumot & Lug'at") },
                { "switch_btn", ("Switch Site", "Sayyorani Almashtirish") },
                { "sound_on", ("Sound: ON", "Ovoz: YOQ") },
                { "sound_off", ("Sound: OFF", "Ovoz: O'CH") },
                { "comm_delay", ("Radio Delay", "Radio Kechikishi") },
                { "comm_mars_warn", ("Mars is ~12 light-minutes away! You CANNOT wait for Earth — decide locally NOW!", "Marsga signal ~12 daqiqa ketadi! Yerni kutib bo'lmaydi — o'zingiz qaror qiling!") },
                { "comm_moon_ok", ("Moon radio link takes 1.3s — Earth Mission Control replies almost right away!", "Oyga signal 1.3 soniyada yetadi — Yer darhol javob beradi!") },
                { "why_nasa", ("Why NASA Planners Care:", "NASA Muhandislari Nega Buni Hisoblaydi:") },
                { "debrief_win", ("Mission Accomplished! Outpost Survived 7 Days!", "Missiya Bajarildi! Baza 7 Kun Omon Qoldi!") },
                { "debrief_lose", ("Mission Emergency — Outpost Evacuated!", "Favqulodda Holat — Baza Evakuatsiya Qilindi!") },
                { "play_again", ("Play Again", "Qayta O'ynash") },
                { "other_planet", ("Try Other Planet", "Boshqa Sayyorada Sinash") },
                { "close", ("Back to Mission Control", "Boshqaruvga Qaytish") }
            };

        public static void Toggle()
        {
            Current = Current == GameLang.EN ? GameLang.UZ : GameLang.EN;
            OnLanguageChanged?.Invoke();
        }

        public static string Get(string key)
        {
            if (Dict.TryGetValue(key, out var pair))
                return Current == GameLang.EN ? pair.en : pair.uz;
            return key;
        }

        public static string Pick(string en, string uz) => Current == GameLang.EN ? en : uz;
    }
}
