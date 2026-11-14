using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace OutpostBalance.Data
{
    /// <summary>
    /// Fetches live NASA DONKI solar flare data via UnityWebRequest and falls back
    /// transparently to the embedded real NASA archive JSON when offline or rate-limited.
    /// </summary>
    public class NasaDonkiClient : MonoBehaviour
    {
        public bool IsLive { get; private set; }
        public string StatusNoteEn { get; private set; } = "Loading NASA DONKI Space Weather...";
        public string StatusNoteUz { get; private set; } = "NASA DONKI ma'lumotlari yuklanmoqda...";

        public List<DonkiEvent> Events { get; private set; } = new List<DonkiEvent>();
        public List<OsdrFinding> OsdrFindings { get; private set; } = new List<OsdrFinding>();
        public List<GlossaryTerm> GlossaryTerms { get; private set; } = new List<GlossaryTerm>();

        public event Action OnDataReady;

        private void Awake()
        {
            LoadLocalDatasets();
        }

        public void LoadLocalDatasets()
        {
            var donkiAsset = Resources.Load<TextAsset>("Data/donki_archive");
            if (donkiAsset != null)
            {
                var parsed = JsonUtility.FromJson<DonkiArchiveWrapper>(donkiAsset.text);
                if (parsed != null && parsed.events != null)
                    Events = parsed.events;
            }

            var osdrAsset = Resources.Load<TextAsset>("Data/osdr_findings");
            if (osdrAsset != null)
            {
                var parsed = JsonUtility.FromJson<OsdrWrapper>(osdrAsset.text);
                if (parsed != null && parsed.findings != null)
                    OsdrFindings = parsed.findings;
            }

            var glossaryAsset = Resources.Load<TextAsset>("Data/glossary");
            if (glossaryAsset != null)
            {
                var parsed = JsonUtility.FromJson<GlossaryWrapper>(glossaryAsset.text);
                if (parsed != null && parsed.terms != null)
                    GlossaryTerms = parsed.terms;
            }
        }

        public void FetchLiveSpaceWeather()
        {
            StartCoroutine(FetchDonkiRoutine());
        }

        private IEnumerator FetchDonkiRoutine()
        {
            string endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
            string startDate = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");
            string url = $"https://api.nasa.gov/DONKI/FLR?startDate={startDate}&endDate={endDate}&api_key=DEMO_KEY";

            using (UnityWebRequest req = UnityWebRequest.Get(url))
            {
                req.timeout = 5;
                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success && !string.IsNullOrEmpty(req.downloadHandler.text))
                {
                    string raw = req.downloadHandler.text.Trim();
                    if (raw.StartsWith("[") && raw.Length > 4)
                    {
                        string wrapped = "{\"items\":" + raw + "}";
                        var parsed = JsonUtility.FromJson<LiveDonkiFlrWrapper>(wrapped);
                        if (parsed != null && parsed.items != null && parsed.items.Count > 0)
                        {
                            var liveList = new List<DonkiEvent>();
                            int startIdx = Mathf.Max(0, parsed.items.Count - 5);
                            for (int i = parsed.items.Count - 1; i >= startIdx; i--)
                            {
                                var item = parsed.items[i];
                                string cls = string.IsNullOrEmpty(item.classType) ? "C1.0" : item.classType;
                                string sev = cls.StartsWith("X") ? "extreme" : (cls.StartsWith("M") ? "high" : "moderate");
                                string peak = string.IsNullOrEmpty(item.peakTime) ? item.beginTime : item.peakTime;
                                string region = item.activeRegionNum > 0 ? $"AR{item.activeRegionNum}" : "Solar Disk";

                                liveList.Add(new DonkiEvent
                                {
                                    id = item.flrID,
                                    type = "FLR",
                                    classType = cls,
                                    severity = sev,
                                    peakTime = peak,
                                    activeRegion = region,
                                    noteEn = $"Live NASA DONKI solar flare ({cls}) observed at {peak}.",
                                    noteUz = $"Jonli NASA DONKI quyosh chaqnashi ({cls}) — {peak}.",
                                    source = "NASA DONKI Live API (api.nasa.gov/DONKI/FLR)",
                                    isLive = true
                                });
                            }

                            Events = liveList;
                            IsLive = true;
                            StatusNoteEn = $"LIVE NASA DONKI API ({liveList.Count} recent flare events loaded)";
                            StatusNoteUz = $"JONLI NASA DONKI API ({liveList.Count} ta chaqnash yuklandi)";
                            OnDataReady?.Invoke();
                            yield break;
                        }
                    }
                }
            }

            // Honest fallback to curated archived NASA DONKI dataset
            IsLive = false;
            StatusNoteEn = "ARCHIVED NASA DONKI DATA (offline or DEMO_KEY limit — real historical flares)";
            StatusNoteUz = "ARXIV NASA DONKI MA'LUMOTI (oflayn yoki limit — haqiqiy tarixiy chaqnashlar)";
            OnDataReady?.Invoke();
        }
    }
}
