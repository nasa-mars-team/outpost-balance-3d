using System.Text;
using UnityEngine;
using UnityEngine.UI;
using OutpostBalance.Data;

namespace OutpostBalance.UI
{
    public class DataGlossaryUI : MonoBehaviour
    {
        public Text titleText;
        public Text bodyText;
        public Text closeBtnText;

        public void Show(NasaDonkiClient donki)
        {
            gameObject.SetActive(true);
            Refresh(donki);
        }

        public void Refresh(NasaDonkiClient donki)
        {
            bool isUz = LocalizationMgr.Current == GameLang.UZ;
            titleText.text = LocalizationMgr.Get("data_btn");
            closeBtnText.text = LocalizationMgr.Get("close");

            var sb = new StringBuilder();
            sb.AppendLine(isUz
                ? "=== NASA DONKI KOSMIK OB-HAVO (Quyosh Chaqnashlari) ==="
                : "=== NASA DONKI SPACE WEATHER (Solar Flares & CMEs) ===");

            foreach (var ev in donki.Events)
            {
                string tag = ev.isLive ? "[LIVE]" : "[ARCHIVED]";
                string note = isUz ? ev.noteUz : ev.noteEn;
                sb.AppendLine($"{tag} {ev.type} {ev.classType} ({ev.activeRegion}) — {note} [{ev.source}]");
            }

            sb.AppendLine();
            sb.AppendLine(isUz
                ? "=== NASA OSDR KOSMIK BIOLOGIYA ==="
                : "=== NASA OSDR SPACE BIOLOGY FINDINGS ===");
            foreach (var o in donki.OsdrFindings)
            {
                string t = isUz ? o.titleUz : o.titleEn;
                string s = isUz ? o.summaryUz : o.summaryEn;
                sb.AppendLine($"• {t}: {s} ({o.source})");
            }

            sb.AppendLine();
            sb.AppendLine(isUz
                ? "=== BOLALAR UCHUN KOSMIK LUG'AT ==="
                : "=== KID-FRIENDLY SPACE GLOSSARY ===");
            foreach (var g in donki.GlossaryTerms)
            {
                string d = isUz ? g.descUz : g.descEn;
                sb.AppendLine($"• {g.term}: {d}");
            }

            bodyText.text = sb.ToString();
        }
    }
}
