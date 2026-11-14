# TEXNIK TOPSHIRIQ (TZ) — "OUTPOST BALANCE 3D"
**Loyiha:** NASA Moon & Mars Outpost Trade-Off Simulator (Unity 3D)  
**Format:** 2 kunlik (48 soatlik) jamoaviy hakaton  
**Maqsadli auditoriya:** 7–12 yoshli o'quvchilar (+ ota-onalar, o'qituvchilar va hakamlar)  
**Platforma:** Unity 3D → **WebGL** (brauzerda yuklab olishsiz o'ynash) + Windows Desktop Build  

---

## 1. Muammo va Yechim G'oyasi
Kosmik STEM materiallari ko'pincha bolalar uchun yo juda yuzaki (shunchaki faktlar), yo juda murakkab bo'ladi. **Outpost Balance 3D** bolalarga **haqiqiy muhandislik murosalarini (trade-offs)** jonli 3D kosmik bazani boshqarish orqali his qilish imkonini beradi:
- **Hech bir qaror tekin emas:** Quvvatni tejash ertangi kislorodni kamaytiradi; radiatsiya qalqonini kuchaytirish issiqxona yoki laboratoriya energiyasini yeydi.
- **Vaqt o'tishi bilan kuchayuvchi oqibat (Compounding Cost):** Bugun gidroponik issiqxonani o'chirsangiz, ertaga o'simliklar so'lib kislorod tiklanishi 50% ga tushadi.
- **Haqiqiy NASA ma'lumotlari:** O'yin hodisalari to'qima emas — **NASA DONKI**, **NASA OSDR**, **Artemis** va **Mars Perseverance/REMS** ma'lumotlariga tayanadi.

---

## 2. Asosiy O'yin Mexanikasi (Core Gameplay)

### 2.1. Faqat 3 ta Asosiy Resurs + 1 Maqsad
*(Shartlarga ko'ra yosh bolalarni chalg'itmaslik uchun 5–6 tizim emas, faqat 3 ta asosiy bar ko'rsatiladi)*
1. **⚡ Power (`0–100%`):** Quyosh panellari ishlab chiqaradigan kunlik quvvat va batareya zaxirasi.
2. **🫧 Life Support (`0–100%`):** Ekipaj nafas olishi (O₂) va gidroponik issiqxonadan keladigan oziq-ovqat/suv.
3. **🛡️ Shielding (`0–100%`):** Radiatsiya gumbazi, suv devorlari va regolit himoyasi.
4. **🔬 Science Goal (`0–100 ball`):** 7 kun/Sol davomida ekipajni tirik saqlab, ilmiy maqsadni bajarish.

> **Mag'lubiyat sharti:** `Power`, `LifeSupport` yoki `Shielding` dan birortasi `0%` ga tushsa — missiya favqulodda evakuatsiya bilan tugaydi va bolaga *aynan qaysi murosa bunga olib kelgani* tushuntiriladi.

### 2.2. Ikkita O'ynaladigan Sayyora (Replayability)
| Parametr | 🌕 Oy: Shackleton Ridge (Artemis) | 🔴 Mars: Jezero Crater (Perseverance) |
| :--- | :--- | :--- |
| **Vaqt birligi** | Day (1–7) | Sol — 24s 39d (1–7) |
| **Radio kechikishi** | **1.3 soniya** (Yer tez javob beradi) | **~12 daqiqa** (Mustaqil qaror shart!) |
| **Gravitatsiya / Harorat** | `0.16 g` • `-130°C .. +120°C` | `0.38 g` • `-95°C .. 0°C` |
| **Maxsus xavf** | Krater soyalari va mikrometeoritlar | Chang bo'ronlari (panellarni 40% to'sadi) |

### 2.3. Har Kunlik Sikl (2 Bosqich)
1. **1-bosqich — 3D Baza Quvvat Taqsimoti (`Power Allocation`):**
   O'yinchi 3D modullarni bosib (yoki yon paneldan) bugungi cheklangan quyosh quvvatini (`9–10 kW`) 4 ta tizimga bo'ladi (`0–4` daraja):
   - **O₂ & Water Recycler:** `0` daraja → `-20% Life`, `3–4` daraja → `+7..+12% Life`.
   - **Hydroponic Greenhouse:** `0` daraja → o'simliklar so'ladi (`cropsWilted = true`, ertaga hosil yarim bo'ladi!).
   - **Radiation Shield Tower:** Yuqori rejim radiatsiyani to'sadi, lekin ko'p quvvat sarflaydi.
   - **Science & Radio Lab:** Ilm ballari beradi, lekin `3+` darajada qo'shimcha sovutish suvi (`-3% Life`) yeydi.
   - *Ortiqcha quvvat sarflansa → Batareya (`Power`) keskin kamayadi; tejalgan quvvat → Batareyani zaryadlaydi.*
2. **2-bosqich — Real-Data 3D Hodisa & Dilemma (`Dilemma Event`):**
   - **NASA DONKI Quyosh Chaqnashi (`FLR` / `CME`):** 3D osmondan zarrachalar yog'iladi, qalqon gumbazi miltillaydi. Tanlovlar: *Qalqonni kuchaytirish (−Quvvat)* / *Boshpanaga yashirinish (0 Ilm)* / *Tashqarida o'lchov qilish (+Ilm, −Qalqon)*.
   - **Aloqa Kechikishi Favqulodda Holati:** Marsda `12 daqiqa` kechikish indikatori yonadi — o'yinchi Yerni kutolmasdan o'zi qaror qabul qiladi!
   - **NASA OSDR Biologiya / ISRU Dilemmasi:** Suvni issiqxonaga berishmi yoki radiatsiya devori baklarigami? Roverni himoya tuprog'i (regolit) yig'ishga yuborishmi yoki muz qazishgami?

---

## 3. Real NASA Ochiq Ma'lumotlar Integratsiyasi
1. **NASA DONKI (`api.nasa.gov/DONKI/FLR`):**
   - O'yin boshida `UnityWebRequest` orqali so'nggi 30 kunlik quyosh chaqnashlari (`DEMO_KEY`) yuklanadi.
   - Internet o'chiq bo'lsa yoki rate-limit bo'lsa, `StreamingAssets/Data/donki_archive.json` (haqiqiy 2024–2025 `X8.7`, `X9.0`, `CME` hodisalari) ishlatiladi.
   - UI da har doim ochiq ko'rsatiladi: `🟢 LIVE NASA DONKI` yoki `📦 ARCHIVED NASA DATA` (arxiv hech qachon jonli deb ko'rsatilmaydi!).
2. **NASA OSDR (`osdr_findings.json`):**
   - `OSD-120` (ISS Veggie o'simlik stressi), `OSD-436` (radiatsiyadan DNK shikastlanishi va suv/regolit himoyasi), `OSD-253` (ekipaj salomatligi) 7–12 yoshga mos tilda dilemma kartalarida chiqadi.
3. **Bolalarbop Lug'at (`glossary.json`) & Data Attribution Modal:**
   - *Sol, CME, Radiation Shielding, Light-Speed Delay, Hydroponics* atamalari 1 bosishda tushuntiriladi.
   - Tillar: **EN / UZ** bir tugma bilan almashadi (xalqaro hakamlar va mahalliy foydalanuvchilar uchun).

---

## 4. Unity 3D Sahna va Arxitektura

### 4.1. 3D Sahna Tuzilishi (`OutpostScene.unity`)
Tez yig'ish va barqaror WebGL ishlashi uchun **3D Isometric/Orbital Diorama** uslubi tanlanadi:
- **Kamera:** `CameraOrbitController` (sichqoncha bilan aylantirish, scroll bilan zoom, bino tanlanganda yumshoq fokus).
- **3D Baza Prefablari (Low-Poly / Unity Primitives + Toza Materiallar):**
  - `TerrainSurface`: Oy (kulrang kraterlar) va Mars (zangori-qizil qum/qoyalar) orasida almashadi.
  - `SolarArrayModule`: Quyosh panellari (chang bo'ronida chang qoplab rangi xiralashadi).
  - `LifeSupportDome` + `O2Tower`: Kislorod minorasi va asosiy gumbaz (quvvat holatiga qarab deraza chiroqlari yashil/sariq/qizil yonadi).
  - `GreenhouseModule`: Shaffof gumbaz ichida 3D o'simliklar (`cropsWilted` bo'lsa kichrayib sarg'ayadi!).
  - `ShieldTower` + `ShieldBubble`: Baza ustidagi yarim-shaffof energiya gumbazi (`Material` alpha/pulse effekti).
  - `ScienceLab` + `RoverPatrol`: Baza atrofida harakatlanadigan 3D Rover va aylanuvchi antenna.
- **VFX (Particle Systems):**
  - `SolarFlareVFX`: Osmon qizg'ish-to'q sariq tusga kirib radiatsiya to'lqinlari tushadi.
  - `DustStormVFX`: Marsda chang zarralari va tuman effekti.

### 4.2. C# Papka va Skript Tuzilishi (`Assets/Scripts/`)
```text
Assets/
├── Resources/Data/
│   ├── donki_archive.json       # Haqiqiy arxivlangan NASA DONKI hodisalari
│   ├── osdr_findings.json       # NASA OSDR biologiya xulosalari (EN/UZ)
│   └── glossary.json            # Bolalarbop kosmik atamalar lug'ati (EN/UZ)
├── Scripts/
│   ├── Core/
│   │   ├── GameManager.cs       # Asosiy o'yin holati (Select -> Play -> Debrief)
│   │   ├── MissionConfig.cs     # Moon vs Mars parametrlari (ScriptableObject)
│   │   └── ResourceSystem.cs    # Power, Life, Shield, Science hisob-kitobi
│   ├── Data/
│   │   ├── NasaDataModels.cs    # JSON seriyalashtirish modellari
│   │   ├── NasaDonkiClient.cs   # UnityWebRequest + halol fallback tizimi
│   │   └── LocalizationMgr.cs   # EN / UZ dinamik tarjima
│   ├── Gameplay/
│   │   ├── PowerAllocator.cs    # 4 ta tizimga quvvat bo'lish & zanjirli oqibatlar
│   │   ├── DilemmaEngine.cs     # Kunlik NASA hodisalari generatori & tanlovlar
│   │   └── CommDelayMgr.cs      # Moon (1.3s) vs Mars (12 min) kechikish simulyatori
│   ├── World3D/
│   │   ├── OutpostVisuals.cs    # 3D o'simlik, gumbaz, chiroq, ob-havo boshqaruvi
│   │   ├── CameraOrbit.cs       # Aylantirish va modulga yaqinlashish
│   │   ├── ModuleClickable.cs   # 3D binoni bosib quvvat/ma'lumot ko'rish
│   │   └── RoverPatrol.cs       # Rover harakati animatsiyasi
│   └── UI/
│       ├── HudController.cs     # 3 ta asosiy bar + Science + Kun/Sol
│       ├── PowerPanelUI.cs      # +/- quvvat boshqaruvi va ogohlantirishlar
│       ├── DilemmaPanelUI.cs    # Hodisa kartasi, aloqa taymeri, NASA manbasi
│       ├── DataGlossaryUI.cs    # NASA DONKI/OSDR & Lug'at oynasi
│       └── DebriefUI.cs         # Yakuniy reyting, qarorlar tarixi, Replay
└── Scenes/
    └── MainScene.unity
```

---

## 5. GitHub Jamoaviy Ishlash Qoidasi (2 kishi uchun)
Unity'da konflikt (`merge conflict`) bo'lmasligi uchun:
1. Majburiy Unity `.gitignore` ishlatiladi (`Library/`, `Temp/`, `Obj/`, `Build/`, `Logs/` GitHub'ga yuklanmaydi).
2. `ProjectSettings/EditorSettings`: **Asset Serialization = Force Text**, **Version Control = Visible Meta Files**.
3. **Rollar taqsimoti:**
   - **Dasturchi (Logic & Data):** `Assets/Scripts/` va `Resources/Data/` papkalarini yuritadi.
   - **3D & UI Dizayner (Scene & VFX):** `MainScene.unity`, 3D prefabs, materiallar, Particle VFX va UI Canvas'ni yig'adi.
4. Har kuni 2–3 marta kichik `commit/pull` qilinadi (sahna faylida bir vaqtda ikki kishi tahrir qilmaydi).

---

## 6. 48 Soatlik Harakat Rejasi
- **1-kun (Bugun):**
  - GitHub repo ochish + Unity `.gitignore` + loyiha strukturasi.
  - Barcha C# Core/Data/Gameplay skriptlar va NASA JSON fayllarini repo'ga yuklash.
  - Unity'da 3D Oy/Mars sahnasini, 4 ta modulni va HUD/Quvvat panelini ulash.
- **2-kun (Ertaga):**
  - 3D Particle VFX (Quyosh chaqnashi, Chang bo'roni, Qalqon shaderi) va Rover animatsiyasi.
  - NASA Data & Glossary modali, EN/UZ tarjima, Ovoz effektlari, Debrief oynasi.
  - **WebGL Build** + test + Hakamlar uchun 3 daqiqalik pitch & dataset hujjati.
