using System.Text;
using UnityEngine;
using UnityEngine.UI;
using OutpostBalance.Core;
using OutpostBalance.Data;

namespace OutpostBalance.UI
{
    public class DebriefUI : MonoBehaviour
    {
        public Text titleText;
        public Text rankText;
        public Text statsText;
        public Text logText;
        public Text playAgainBtnText;
        public Text otherSiteBtnText;

        public void Show(bool won, SiteType site, ResourceSystem res)
        {
            gameObject.SetActive(true);
            bool isUz = LocalizationMgr.Current == GameLang.UZ;

            titleText.text = won ? LocalizationMgr.Get("debrief_win") : LocalizationMgr.Get("debrief_lose");
            titleText.color = won ? new Color(0.22f, 0.88f, 0.45f) : new Color(0.95f, 0.30f, 0.30f);

            if (!won)
            {
                rankText.text = $"{res.GetFailureReason(isUz)} " +
                    (isUz ? "Qaysi qarorlar resursni tugatganini ko'rib qayta sinang!" : "Review which trade-offs drained your reserves and try another strategy!");
            }
            else if (res.Science >= 75f && res.LifeSupport >= 55f && res.Shielding >= 55f)
            {
                rankText.text = isUz
                    ? "NASA Parvoz Direktori! Siz ham omon qolish, ham yuqori ilmni a'lo bajardingiz!"
                    : "NASA Flight Director! You balanced survival AND high science!";
            }
            else
            {
                rankText.text = isUz
                    ? "Baza Qo'mondoni! Ekipaj omon qoldi — keyingi safar ko'proq Ilm to'plashga urining!"
                    : "Outpost Commander! Crew survived safely — try pushing more Science next time!";
            }

            statsText.text = LocalizationMgr.Pick(
                $"Power: {Mathf.RoundToInt(res.Power)}%  |  Life Support: {Mathf.RoundToInt(res.LifeSupport)}%  |  Shielding: {Mathf.RoundToInt(res.Shielding)}%  |  Science: {Mathf.RoundToInt(res.Science)}/100",
                $"Quvvat: {Mathf.RoundToInt(res.Power)}%  |  Hayot: {Mathf.RoundToInt(res.LifeSupport)}%  |  Qalqon: {Mathf.RoundToInt(res.Shielding)}%  |  Ilm: {Mathf.RoundToInt(res.Science)}/100");

            var profile = MissionConfig.GetProfile(site);
            string dayLbl = isUz ? profile.dayLabelUz : profile.dayLabelEn;

            var sb = new StringBuilder();
            foreach (var h in res.History)
            {
                sb.AppendLine($"• {dayLbl} {h.day}: {h.eventTitle} → {h.decisionSummary} ({h.statsSnapshot})");
            }
            logText.text = sb.ToString();

            playAgainBtnText.text = LocalizationMgr.Get("play_again");
            otherSiteBtnText.text = LocalizationMgr.Get("other_planet");
        }
    }
}
