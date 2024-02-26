using GameData;
using System.Collections.Generic;

namespace ExtraRecoilData.CustomRecoil
{
    public class CustomRecoilData
    {
        public uint ArchetypeID { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public float RecoilScaleCap { get; set; } = 1f;
        public float RecoilScaleThreshold { get; set; } = 0f;
        public float RecoilScaleDecay { get; set; } = 1f;
        public float RecoilScaleDecayDelay { get; set; } = 0.016f;
        public float RecoilScaleMin { get; set; } = 1f;
        public float RecoilScaleMax { get; set; } = 1f;
        public float RecoilPatternScaleMin { get; set; } = 1f;
        public float RecoilPatternScaleMax { get; set; } = 1f;

        public List<float> RecoilPattern { get; set; } = new();
        public List<float> RecoilPatternFirst { get; set; } = new();
        public RecoilPatternAlign RecoilPatternAlign { get; set; } = RecoilPatternAlign.ALIGN;
        public MinMaxValue RecoilPatternPower { get; set; } = new() { Min = 0, Max = 0 };
        public float RecoilPatternResetDelay { get; set; } = 0.016f;
    }
}
