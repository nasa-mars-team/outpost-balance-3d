using UnityEngine;
using UnityEngine.UI;
using OutpostBalance.Core;
using OutpostBalance.Data;
using OutpostBalance.Gameplay;

namespace OutpostBalance.UI
{
    public class PowerPanelUI : MonoBehaviour
    {
        public Text titleText;
        public Text descText;
        public Text summaryText;
        public GameObject warningBanner;
        public Text warningText;

        public Text o2NameText;
        public Text o2ValText;
        public Text ghNameText;
        public Text ghValText;
        public Text shieldNameText;
        public Text shieldValText;
        public Text labNameText;
        public Text labValText;
        public Text runButtonText;

        public void Refresh(SiteType site, ResourceSystem res, PowerAllocator alloc)
        {
            titleText.text = LocalizationMgr.Get("budget_title");
            descText.text = LocalizationMgr.Get("budget_desc");

            int solar = alloc.GetTodaySolarOutput(site, res);
            int used = alloc.TotalAllocated;
            int batDelta = alloc.PreviewBatteryDeltaPercent(site, res);
            string sign = batDelta >= 0 ? "+" : "";

            summaryText.text = LocalizationMgr.Pick(
                $"Solar Output: {solar} kW   |   Allocated: {used} kW   |   Battery: {sign}{batDelta}%",
                $"Quyosh Quvvati: {solar} kW   |   Ajratildi: {used} kW   |   Batareya: {sign}{batDelta}%");

            bool over = used > solar;
            warningBanner.SetActive(over);
            warningText.text = LocalizationMgr.Get("over_budget");

            o2NameText.text = LocalizationMgr.Get("sys_o2");
            o2ValText.text = $"{alloc.O2Level} / 4";

            ghNameText.text = LocalizationMgr.Get("sys_gh");
            ghValText.text = $"{alloc.GreenhouseLevel} / 4";

            shieldNameText.text = LocalizationMgr.Get("sys_shield");
            shieldValText.text = $"{alloc.ShieldLevel} / 4";

            labNameText.text = LocalizationMgr.Get("sys_lab");
            labValText.text = $"{alloc.LabLevel} / 4";

            runButtonText.text = LocalizationMgr.Get("run_day");
        }
    }
}
