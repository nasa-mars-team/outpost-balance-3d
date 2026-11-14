using UnityEngine;
using UnityEngine.UI;
using OutpostBalance.Data;
using OutpostBalance.Gameplay;
using OutpostBalance.UI;
using OutpostBalance.World3D;

namespace OutpostBalance.Core
{
    public enum GamePhase { SiteSelect, PowerPhase, DilemmaPhase, Debrief }

    /// <summary>
    /// Central coordinator for Outpost Balance 3D: manages mission flow, 3D visuals, NASA data, and UI.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Core Services")]
        public NasaDonkiClient donkiClient;
        public CommDelayMgr commDelayMgr;
        public OutpostVisuals outpostVisuals;
        public SfxPlayer sfxPlayer;

        [Header("Screens & Panels")]
        public GameObject siteSelectScreen;
        public GameObject gameplayHudRoot;
        public PowerPanelUI powerPanel;
        public DilemmaPanelUI dilemmaPanel;
        public DebriefUI debriefUI;
        public DataGlossaryUI dataModal;

        [Header("Top Bar & Select UI")]
        public HudController hud;
        public Text donkiBannerText;
        public Text appTitleText;
        public Text appSubText;
        public Text chooseTitleText;
        public Text chooseDescText;
        public Text moonBtnText;
        public Text marsBtnText;
        public Text moonCardSpecText;
        public Text marsCardSpecText;
        public Text dataBtnText;
        public Text soundBtnText;
        public Text switchBtnText;
        public Button switchSiteButton;

        private readonly ResourceSystem _res = new ResourceSystem();
        private readonly PowerAllocator _alloc = new PowerAllocator();
        private SiteType _site = SiteType.Moon;
        private int _day = 1;
        private GamePhase _phase = GamePhase.SiteSelect;
        private DilemmaData _currentDilemma;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            LocalizationMgr.OnLanguageChanged += RefreshAllLocalizedUI;
            if (donkiClient != null)
            {
                donkiClient.OnDataReady += UpdateDonkiBanner;
                donkiClient.FetchLiveSpaceWeather();
            }
            if (commDelayMgr != null)
            {
                commDelayMgr.OnProgressChanged += p => dilemmaPanel?.UpdateCommProgress(p);
            }

            _res.Reset();
            _alloc.Reset();
            SetPhase(GamePhase.SiteSelect);
            outpostVisuals?.SyncWorld(_site, _res, _alloc);
            RefreshAllLocalizedUI();
        }

        private void OnDestroy()
        {
            LocalizationMgr.OnLanguageChanged -= RefreshAllLocalizedUI;
        }

        public void StartMoonMission() => StartMission(SiteType.Moon);
        public void StartMarsMission() => StartMission(SiteType.Mars);

        public void StartMission(SiteType site)
        {
            sfxPlayer?.PlaySuccess();
            commDelayMgr?.Stop();

            _site = site;
            _day = 1;
            _res.Reset();
            _alloc.Reset();
            _currentDilemma = null;

            SetPhase(GamePhase.PowerPhase);
            RefreshAllGameplayUI();
        }

        public void ReturnToSiteSelect()
        {
            sfxPlayer?.PlayClick();
            commDelayMgr?.Stop();
            SetPhase(GamePhase.SiteSelect);
            RefreshAllLocalizedUI();
        }

        public void AdjustSubsystem(int sysIndex, int delta)
        {
            if (_phase != GamePhase.PowerPhase) return;
            var sys = (OutpostSubsystem)sysIndex;
            if (_alloc.Adjust(sys, delta))
            {
                sfxPlayer?.PlayClick();
                RefreshAllGameplayUI();
            }
        }

        public void AdjustO2Plus() => AdjustSubsystem((int)OutpostSubsystem.O2, 1);
        public void AdjustO2Minus() => AdjustSubsystem((int)OutpostSubsystem.O2, -1);
        public void AdjustGhPlus() => AdjustSubsystem((int)OutpostSubsystem.Greenhouse, 1);
        public void AdjustGhMinus() => AdjustSubsystem((int)OutpostSubsystem.Greenhouse, -1);
        public void AdjustShieldPlus() => AdjustSubsystem((int)OutpostSubsystem.Shield, 1);
        public void AdjustShieldMinus() => AdjustSubsystem((int)OutpostSubsystem.Shield, -1);
        public void AdjustLabPlus() => AdjustSubsystem((int)OutpostSubsystem.Lab, 1);
        public void AdjustLabMinus() => AdjustSubsystem((int)OutpostSubsystem.Lab, -1);

        public void CycleSubsystemFrom3D(OutpostSubsystem sys)
        {
            if (_phase != GamePhase.PowerPhase) return;
            int nextDelta = _alloc.GetLevel(sys) >= 4 ? -4 : 1;
            AdjustSubsystem((int)sys, nextDelta);
        }

        public void RunMissionDay()
        {
            if (_phase != GamePhase.PowerPhase) return;
            sfxPlayer?.PlayClick();

            _alloc.ExecuteAllocation(_site, _res);
            if (_res.IsFailed())
            {
                FinishMission(false);
                return;
            }

            _currentDilemma = DilemmaEngine.BuildForDay(_day, _site, donkiClient);
            _res.SolarFlareActive = _currentDilemma.triggersSolarFlareVfx;

            SetPhase(GamePhase.DilemmaPhase);
            sfxPlayer?.PlayAlert();

            if (_currentDilemma.hasCommDelay)
                commDelayMgr?.Begin(_site);
            else
                commDelayMgr?.Stop();

            dilemmaPanel.Show(_currentDilemma, _site, ResolveDilemmaChoice);
            RefreshAllGameplayUI();
        }

        private void ResolveDilemmaChoice(int index)
        {
            if (_phase != GamePhase.DilemmaPhase || _currentDilemma == null) return;
            if (index < 0 || index >= _currentDilemma.choices.Count) return;

            commDelayMgr?.Stop();
            var choice = _currentDilemma.choices[index];
            _res.ApplyDelta(choice.powerDelta, choice.lifeDelta, choice.shieldDelta, choice.scienceDelta);

            if (choice.triggersEnvPenalty)
            {
                if (_site == SiteType.Mars) _res.DustStormDays = 2;
                else _res.ShadowDays = 2;
            }

            bool isUz = LocalizationMgr.Current == GameLang.UZ;
            _res.History.Add(new DecisionLogEntry
            {
                day = _day,
                eventTitle = isUz ? _currentDilemma.titleUz : _currentDilemma.titleEn,
                decisionSummary = isUz ? choice.summaryUz : choice.summaryEn,
                statsSnapshot = $"P:{Mathf.RoundToInt(_res.Power)}% L:{Mathf.RoundToInt(_res.LifeSupport)}% S:{Mathf.RoundToInt(_res.Shielding)}%"
            });

            _res.SolarFlareActive = false;

            if (_res.IsFailed())
            {
                FinishMission(false);
                return;
            }

            if (_day >= MissionConfig.MaxDays)
            {
                FinishMission(true);
                return;
            }

            sfxPlayer?.PlaySuccess();
            _day++;
            SetPhase(GamePhase.PowerPhase);
            RefreshAllGameplayUI();
        }

        private void FinishMission(bool won)
        {
            commDelayMgr?.Stop();
            if (won) sfxPlayer?.PlaySuccess();
            else sfxPlayer?.PlayDanger();

            SetPhase(GamePhase.Debrief);
            outpostVisuals?.SyncWorld(_site, _res, _alloc);
            debriefUI?.Show(won, _site, _res);
        }

        public void ReplaySameSite() => StartMission(_site);
        public void PlayOtherPlanet() => StartMission(_site == SiteType.Moon ? SiteType.Mars : SiteType.Moon);

        public void ToggleLanguage()
        {
            sfxPlayer?.PlayClick();
            LocalizationMgr.Toggle();
        }

        public void ToggleSound()
        {
            sfxPlayer?.ToggleSound();
            RefreshAllLocalizedUI();
        }

        public void OpenDataModal()
        {
            sfxPlayer?.PlayClick();
            dataModal?.Show(donkiClient);
        }

        public void CloseDataModal()
        {
            sfxPlayer?.PlayClick();
            if (dataModal != null) dataModal.gameObject.SetActive(false);
        }

        private void SetPhase(GamePhase next)
        {
            _phase = next;
            if (siteSelectScreen != null) siteSelectScreen.SetActive(next == GamePhase.SiteSelect);
            if (gameplayHudRoot != null) gameplayHudRoot.SetActive(next == GamePhase.PowerPhase || next == GamePhase.DilemmaPhase);
            if (powerPanel != null) powerPanel.gameObject.SetActive(next == GamePhase.PowerPhase);
            if (dilemmaPanel != null) dilemmaPanel.gameObject.SetActive(next == GamePhase.DilemmaPhase);
            if (debriefUI != null) debriefUI.gameObject.SetActive(next == GamePhase.Debrief);
            if (switchSiteButton != null) switchSiteButton.gameObject.SetActive(next != GamePhase.SiteSelect);
        }

        private void RefreshAllGameplayUI()
        {
            hud?.Refresh(_site, _day, _res);
            powerPanel?.Refresh(_site, _res, _alloc);
            outpostVisuals?.SyncWorld(_site, _res, _alloc);
        }

        private void RefreshAllLocalizedUI()
        {
            bool isUz = LocalizationMgr.Current == GameLang.UZ;
            if (appTitleText != null) appTitleText.text = LocalizationMgr.Get("app_title");
            if (appSubText != null) appSubText.text = LocalizationMgr.Get("app_sub");
            if (chooseTitleText != null) chooseTitleText.text = LocalizationMgr.Get("choose_title");
            if (chooseDescText != null) chooseDescText.text = LocalizationMgr.Get("choose_desc");
            if (moonBtnText != null) moonBtnText.text = LocalizationMgr.Get("launch_moon");
            if (marsBtnText != null) marsBtnText.text = LocalizationMgr.Get("launch_mars");
            if (dataBtnText != null) dataBtnText.text = LocalizationMgr.Get("data_btn");
            if (switchBtnText != null) switchBtnText.text = LocalizationMgr.Get("switch_btn");
            if (soundBtnText != null)
            {
                bool on = sfxPlayer == null || sfxPlayer.SoundEnabled;
                soundBtnText.text = LocalizationMgr.Get(on ? "sound_on" : "sound_off");
            }

            if (moonCardSpecText != null)
            {
                var m = MissionConfig.Moon;
                moonCardSpecText.text = $"{(isUz ? m.nameUz : m.nameEn)}\n{(isUz ? m.factUz : m.factEn)}\n" +
                    $"{LocalizationMgr.Get("comm_delay")}: {(isUz ? m.commLabelUz : m.commLabelEn)} • {m.gravity} • {m.tempRange}";
            }
            if (marsCardSpecText != null)
            {
                var ma = MissionConfig.Mars;
                marsCardSpecText.text = $"{(isUz ? ma.nameUz : ma.nameEn)}\n{(isUz ? ma.factUz : ma.factEn)}\n" +
                    $"{LocalizationMgr.Get("comm_delay")}: {(isUz ? ma.commLabelUz : ma.commLabelEn)} • {ma.gravity} • {ma.tempRange}";
            }

            UpdateDonkiBanner();
            if (_phase == GamePhase.PowerPhase || _phase == GamePhase.DilemmaPhase)
            {
                RefreshAllGameplayUI();
                if (_phase == GamePhase.DilemmaPhase) dilemmaPanel?.RefreshTexts(_site);
            }
            if (dataModal != null && dataModal.gameObject.activeSelf)
                dataModal.Refresh(donkiClient);
        }

        private void UpdateDonkiBanner()
        {
            if (donkiBannerText == null || donkiClient == null) return;
            bool isUz = LocalizationMgr.Current == GameLang.UZ;
            donkiBannerText.text = isUz ? donkiClient.StatusNoteUz : donkiClient.StatusNoteEn;
        }
    }
}
