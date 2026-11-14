using System;
using System.Collections.Generic;

namespace OutpostBalance.Data
{
    [Serializable]
    public class DonkiEvent
    {
        public string id;
        public string type;
        public string classType;
        public string severity;
        public string peakTime;
        public string activeRegion;
        public string noteEn;
        public string noteUz;
        public string source;
        public bool isLive;
    }

    [Serializable]
    public class DonkiArchiveWrapper
    {
        public List<DonkiEvent> events = new List<DonkiEvent>();
    }

    // Matches NASA DONKI FLR API JSON array items when wrapped for JsonUtility
    [Serializable]
    public class LiveDonkiFlrItem
    {
        public string flrID;
        public string classType;
        public string peakTime;
        public string beginTime;
        public int activeRegionNum;
    }

    [Serializable]
    public class LiveDonkiFlrWrapper
    {
        public List<LiveDonkiFlrItem> items = new List<LiveDonkiFlrItem>();
    }

    [Serializable]
    public class OsdrFinding
    {
        public string id;
        public string titleEn;
        public string titleUz;
        public string summaryEn;
        public string summaryUz;
        public string source;
    }

    [Serializable]
    public class OsdrWrapper
    {
        public List<OsdrFinding> findings = new List<OsdrFinding>();
    }

    [Serializable]
    public class GlossaryTerm
    {
        public string term;
        public string descEn;
        public string descUz;
    }

    [Serializable]
    public class GlossaryWrapper
    {
        public List<GlossaryTerm> terms = new List<GlossaryTerm>();
    }
}
