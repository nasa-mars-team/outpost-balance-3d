using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OutpostBalance.Core;
using OutpostBalance.Data;
using OutpostBalance.Gameplay;

namespace OutpostBalance.UI
{
    public class DilemmaPanelUI : MonoBehaviour
    {
        public Text tagText;
        public Text sourceText;
        public Text titleText;
        public Text descText;
        public Text scienceCalloutText;

        public GameObject commDelayBox;
        public Text commDelayText;
        public Image commProgressFill;

        public Button[] choiceButtons;
        public Text[] choiceButtonTexts;

        private DilemmaData _current;
        private Action<int> _onSelect;

        public void Show(DilemmaData data, SiteType site, Action<int> onSelect)
        {
            _current = data;
            _onSelect = onSelect;
            gameObject.SetActive(true);
            RefreshTexts(site);
        }

        public void RefreshTexts(SiteType site)
        {
            if (_current == null) return;
            bool isUz = LocalizationMgr.Current == GameLang.UZ;

            tagText.text = _current.tag;
            sourceText.text = $"Source: {_current.source}";
            titleText.text = isUz ? _current.titleUz : _current.titleEn;
            descText.text = isUz ? _current.descUz : _current.descEn;

            string note = isUz ? _current.scienceNoteUz : _current.scienceNoteEn;
            scienceCalloutText.text = $"{LocalizationMgr.Get("why_nasa")} {note}";

            commDelayBox.SetActive(_current.hasCommDelay);
            if (_current.hasCommDelay)
            {
                commDelayText.text = site == SiteType.Mars
                    ? LocalizationMgr.Get("comm_mars_warn")
                    : LocalizationMgr.Get("comm_moon_ok");
            }

            for (int i = 0; i < choiceButtons.Length; i++)
            {
                if (i < _current.choices.Count)
                {
                    choiceButtons[i].gameObject.SetActive(true);
                    choiceButtonTexts[i].text = isUz ? _current.choices[i].labelUz : _current.choices[i].labelEn;
                    int idx = i;
                    choiceButtons[i].onClick.RemoveAllListeners();
                    choiceButtons[i].onClick.AddListener(() => _onSelect?.Invoke(idx));
                }
                else
                {
                    choiceButtons[i].gameObject.SetActive(false);
                }
            }
        }

        public void UpdateCommProgress(float progress01)
        {
            if (commProgressFill != null)
                commProgressFill.fillAmount = Mathf.Clamp01(progress01);
        }
    }
}
