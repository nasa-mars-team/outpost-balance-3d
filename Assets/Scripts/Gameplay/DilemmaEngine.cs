using System.Collections.Generic;
using OutpostBalance.Core;
using OutpostBalance.Data;

namespace OutpostBalance.Gameplay
{
    public class DilemmaChoice
    {
        public string labelEn;
        public string labelUz;
        public float powerDelta;
        public float lifeDelta;
        public float shieldDelta;
        public float scienceDelta;
        public bool triggersEnvPenalty;
        public string summaryEn;
        public string summaryUz;
    }

    public class DilemmaData
    {
        public string tag;
        public string source;
        public string titleEn;
        public string titleUz;
        public string descEn;
        public string descUz;
        public string scienceNoteEn;
        public string scienceNoteUz;
        public bool hasCommDelay;
        public bool triggersSolarFlareVfx;
        public List<DilemmaChoice> choices = new List<DilemmaChoice>();
    }

    /// <summary>
    /// Generates daily engineering trade-off dilemmas grounded in NASA DONKI, OSDR, and Moon/Mars surface data.
    /// </summary>
    public static class DilemmaEngine
    {
        public static DilemmaData BuildForDay(int day, SiteType site, NasaDonkiClient donki)
        {
            var donkiPool = donki.Events;
            DonkiEvent flare = donkiPool.Count > 0
                ? donkiPool[(day - 1) % donkiPool.Count]
                : new DonkiEvent { type = "FLR", classType = "X8.7", activeRegion = "AR3664", peakTime = "2024-05-14T16:51Z", source = "NASA DONKI Archive" };

            var osdrPool = donki.OsdrFindings;
            OsdrFinding osdr = osdrPool.Count > 0
                ? osdrPool[(day - 1) % osdrPool.Count]
                : new OsdrFinding
                {
                    id = "OSD-120",
                    summaryEn = "Space plants need steady water and light to support crew nutrition.",
                    summaryUz = "Kosmik o'simliklar ekipaj ovqati uchun doimiy suv va yorug'lik talab qiladi.",
                    source = "NASA OSDR"
                };

            bool isMars = site == SiteType.Mars;
            int idx = (day - 1) % 4;

            if (idx == 0)
            {
                return new DilemmaData
                {
                    tag = $"SOLAR EVENT: {flare.type} {flare.classType} ({flare.activeRegion})",
                    source = $"{flare.source} • Peak: {flare.peakTime}",
                    titleEn = $"Incoming Solar Radiation Storm ({flare.classType})!",
                    titleUz = $"Quyosh Radiatsiya Bo'roni Kelmoqda ({flare.classType})!",
                    descEn = $"NASA DONKI detected a real {flare.classType} solar event! Charged particles are racing toward your {(isMars ? "Mars" : "Moon")} outpost.",
                    descUz = $"NASA DONKI haqiqiy {flare.classType} quyosh chaqnashini aniqladi! Zarrachalar bazangizga yaqinlashmoqda.",
                    scienceNoteEn = osdr.summaryEn,
                    scienceNoteUz = osdr.summaryUz,
                    triggersSolarFlareVfx = true,
                    choices = new List<DilemmaChoice>
                    {
                        new DilemmaChoice
                        {
                            labelEn = "Boost Water-Wall & Magnetic Shield (-22% Power, -8% Life Support, +10% Shield)",
                            labelUz = "Suv-Devor va Magnit Qalqonni Kuchaytirish (-22% Quvvat, -8% Hayot, +10% Qalqon)",
                            powerDelta = -22f, lifeDelta = -8f, shieldDelta = 10f, scienceDelta = 4f,
                            summaryEn = "Blocked flare with heavy power & cooling water",
                            summaryUz = "Bo'ron kuchli quvvat evaziga to'sildi"
                        },
                        new DilemmaChoice
                        {
                            labelEn = "Hunker Down in Storm Shelter (+5% Power, -12% Shield, 0 Science today)",
                            labelUz = "Boshpanada Kutish (+5% Quvvat, -12% Qalqon, bugun ilm yo'q)",
                            powerDelta = 5f, lifeDelta = -4f, shieldDelta = -12f, scienceDelta = 0f,
                            summaryEn = "Saved power by hiding; shield took wear",
                            summaryUz = "Quvvat saqlandi, qalqon zarar ko'rdi"
                        },
                        new DilemmaChoice
                        {
                            labelEn = "Measure Solar Particles Outside! (+16 Science, -24% Shield, -6% Life)",
                            labelUz = "Tashqarida Bo'ronni O'lchash! (+16 Ilm, -24% Qalqon, -6% Hayot)",
                            powerDelta = -6f, lifeDelta = -6f, shieldDelta = -24f, scienceDelta = 16f,
                            summaryEn = "High-risk science during solar storm",
                            summaryUz = "Bo'ron paytida xavfli ilmiy tadqiqot"
                        }
                    }
                };
            }

            if (idx == 1)
            {
                var profile = MissionConfig.GetProfile(site);
                return new DilemmaData
                {
                    tag = isMars ? "MARS MEDA WEATHER + COMM DELAY" : "ARTEMIS SHADOW + COMM LINK",
                    source = profile.source,
                    titleEn = isMars ? "Dust Storm Approaching & Valve Leak!" : "Crater Shadow Eclipse & Air Valve Glitch!",
                    titleUz = isMars ? "Chang Bo'roni va Havo Klapani Nosozligi!" : "Krater Soyasi va Havo Klapani Nosozligi!",
                    descEn = isMars
                        ? "A Martian dust cloud is dimming sunlight AND an O2 valve is leaking! Earth is 12 light-minutes away—you cannot wait for their reply."
                        : "Your Moon base is entering a cold crater shadow AND an O2 valve is leaking! Radio to Earth takes 1.3 seconds.",
                    descUz = isMars
                        ? "Mars chang buluti quyoshni to'smoqda VA kislorod klapani sizmoqda! Yerga signal 12 daqiqa ketadi — javobni kutib bo'lmaydi."
                        : "Oy bazasi sovuq krater soyasiga kirmoqda VA kislorod klapani sizmoqda! Yerga signal 1.3 soniya ketadi.",
                    scienceNoteEn = profile.factEn,
                    scienceNoteUz = profile.factUz,
                    hasCommDelay = true,
                    choices = new List<DilemmaChoice>
                    {
                        new DilemmaChoice
                        {
                            labelEn = "Seal Valve with Emergency Foam (+12% Life, -16% Power, -6% Shield)",
                            labelUz = "Klapanni Darhol Yopish (+12% Hayot, -16% Quvvat, -6% Qalqon)",
                            powerDelta = -16f, lifeDelta = 12f, shieldDelta = -6f, scienceDelta = 3f,
                            triggersEnvPenalty = true,
                            summaryEn = "Fixed air leak quickly using reserve power",
                            summaryUz = "Zaxira quvvat bilan havo sizishi to'xtatildi"
                        },
                        new DilemmaChoice
                        {
                            labelEn = "Conserve Battery for Tomorrow's Storm (+14% Power, -16% Life)",
                            labelUz = "Ertangi Bo'ron Uchun Batareyani Saqlash (+14% Quvvat, -16% Hayot)",
                            powerDelta = 14f, lifeDelta = -16f, shieldDelta = 0f, scienceDelta = 2f,
                            triggersEnvPenalty = true,
                            summaryEn = "Protected battery ahead of low-sunlight day",
                            summaryUz = "Kam quyoshli kun uchun batareya saqlandi"
                        }
                    }
                };
            }

            if (idx == 2)
            {
                return new DilemmaData
                {
                    tag = $"{osdr.id} • NASA OSDR BIOLOGY",
                    source = osdr.source,
                    titleEn = osdr.titleEn,
                    titleUz = osdr.titleUz,
                    descEn = "Your hydroponic greenhouse needs extra water and grow-lights to produce vitamins, OR you can pump that water into your radiation wall tanks!",
                    descUz = "Gidroponik issiqxonaga vitaminli ko'katlar o'stirish uchun ko'proq suv kerak, YOKI bu suvni radiatsiya devori baklariga quyishingiz mumkin!",
                    scienceNoteEn = osdr.summaryEn,
                    scienceNoteUz = osdr.summaryUz,
                    choices = new List<DilemmaChoice>
                    {
                        new DilemmaChoice
                        {
                            labelEn = "Feed Greenhouse Water & LEDs (+15% Life, +8 Science, -15% Power, -8% Shield)",
                            labelUz = "Issiqxonaga Suv va Chiroq Berish (+15% Hayot, +8 Ilm, -15% Quvvat, -8% Qalqon)",
                            powerDelta = -15f, lifeDelta = 15f, shieldDelta = -8f, scienceDelta = 8f,
                            summaryEn = "Boosted fresh food & O2; thinner water shield",
                            summaryUz = "Oziq-ovqat va O2 oshdi, suv qalqoni kamaydi"
                        },
                        new DilemmaChoice
                        {
                            labelEn = "Pump Water into Radiation Walls (+14% Shield, -12% Life, -6% Power)",
                            labelUz = "Suvni Radiatsiya Devoriga Quyish (+14% Qalqon, -12% Hayot, -6% Quvvat)",
                            powerDelta = -6f, lifeDelta = -12f, shieldDelta = 14f, scienceDelta = 3f,
                            summaryEn = "Strengthened water shield using greenhouse reserves",
                            summaryUz = "Issiqxona suvi evaziga qalqon kuchaytirildi"
                        }
                    }
                };
            }

            return new DilemmaData
            {
                tag = "ISRU REGOLITH ENGINEERING TRADE-OFF",
                source = "NASA Space Technology & ISRU",
                titleEn = "Use 3D Rover to Dig Shield Dirt or Hunt Ice?",
                titleUz = "3D Rover Bilan Qalqon Tuprog'i Qazishmi Yoki Muz Qidirish?",
                descEn = $"Your rover has battery for ONE trip! It can pile {(isMars ? "Martian" : "Lunar")} regolith dirt on the habitat roof for radiation shielding, OR drill ice/rock samples.",
                descUz = "Roverda faqat BITTA safarga quvvat bor! U baza ustiga radiatsiyadan himoya uchun tuproq uyishi YOKI ilmiy muz namunasi olishi mumkin.",
                scienceNoteEn = "NASA calls using local Moon/Mars dirt and ice 'ISRU' (living off the land) so rockets carry less heavy cargo from Earth!",
                scienceNoteUz = "NASA mahalliy Oy/Mars tuprog'i va muzidan foydalanishni 'ISRU' deydi — shunda Yerdan og'ir yuk tashish shart bo'lmaydi!",
                choices = new List<DilemmaChoice>
                {
                    new DilemmaChoice
                    {
                        labelEn = "Pile Regolith Dirt on Roof (+16% Shield, -12% Power, +2 Science)",
                        labelUz = "Tomga Himoya Tuprog'i Uyish (+16% Qalqon, -12% Quvvat, +2 Ilm)",
                        powerDelta = -12f, lifeDelta = -3f, shieldDelta = 16f, scienceDelta = 2f,
                        summaryEn = "Used local regolith for long-term radiation safety",
                        summaryUz = "Mahalliy tuproq bilan qalqon mustahkamlandi"
                    },
                    new DilemmaChoice
                    {
                        labelEn = "Drill Ice & Science Core (+18 Science, +6% Life, -16% Power, -10% Shield)",
                        labelUz = "Muz va Ilmiy Namuna Qazish (+18 Ilm, +6% Hayot, -16% Quvvat, -10% Qalqon)",
                        powerDelta = -16f, lifeDelta = 6f, shieldDelta = -10f, scienceDelta = 18f,
                        summaryEn = "Big science & ice discovery; skipped roof shielding",
                        summaryUz = "Katta ilmiy topilma, lekin qalqon zaiflashdi"
                    }
                }
            };
        }
    }
}
