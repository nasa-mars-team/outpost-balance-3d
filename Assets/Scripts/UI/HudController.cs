using UnityEngine;
using UnityEngine.UI;
using OutpostBalance.Core;
using OutpostBalance.Data;

namespace OutpostBalance.UI
{
    public class HudController : MonoBehaviour
    {
        public Text siteBadgeText;
        public Text dayBadgeText;
        public Text commBadgeText;

        public Image powerFill;
        public Text powerValText;
        public Text powerLabelText;

        public Image lifeFill;
        public Text lifeValText;
        public Text lifeLabelText;

        public Image shieldFill;
        public Text shieldValText;
        public Text shieldLabelText;

        public Image scienceFill;
        public Text scienceValText;
        public Text scienceLabelText;

        public GameObject rippleBox;
        public Text rippleText;

        public void Refresh(SiteType site, int day, ResourceSystem res)
        {
            var profile = MissionConfig.GetProfile(site);
            bool isUz = LocalizationMgr.Current == GameLang.UZ;

            siteBadgeText.text = isUz ? profile.nameUz : profile.nameEn;
            string dayLbl = isUz ? profile.dayLabelUz : profile.dayLabelEn;
            dayBadgeText.text = $"{dayLbl} {day} / {MissionConfig.MaxDays}";
            string commLbl = isUz ? profile.commLabelUz : profile.commLabelEn;
            commBadgeText.text = $"{LocalizationMgr.Get("comm_delay")}: {commLbl}";

            powerLabelText.text = LocalizationMgr.Get("power");
            lifeLabelText.text = LocalizationMgr.Get("life");
            shieldLabelText.text = LocalizationMgr.Get("shield");
            scienceLabelText.text = LocalizationMgr.Get("science");

            SetBar(powerFill, powerValText, res.Power, new Color(0.95f, 0.78f, 0.15f), true);
            SetBar(lifeFill, lifeValText, res.LifeSupport, new Color(0.16f, 0.82f, 0.52f), true);
            SetBar(shieldFill, shieldValText, res.Shielding, new Color(0.22f, 0.72f, 0.98f), true);
            SetBar(scienceFill, scienceValText, res.Science, new Color(0.75f, 0.42f, 0.96f), false);

            if (res.RippleNotes.Count > 0)
            {
                rippleBox.SetActive(true);
                rippleText.text = string.Join(" • ", res.RippleNotes);
            }
            else
            {
                rippleBox.SetActive(false);
            }
        }

        private void SetBar(Image fill, Text valText, float value, Color healthyColor, bool isPercent)
        {
            float clamped = Mathf.Clamp(value, 0f, 100f);
            fill.fillAmount = clamped / 100f;
            valText.text = isPercent ? $"{Mathf.RoundToInt(clamped)}%" : $"{Mathf.RoundToInt(clamped)} / 100";

            if (isPercent && clamped <= 25f)
                fill.color = new Color(0.94f, 0.27f, 0.27f);
            else if (isPercent && clamped <= 45f)
                fill.color = new Color(0.96f, 0.62f, 0.08f);
            else
                fill.color = healthyColor;
        }
    }
}
