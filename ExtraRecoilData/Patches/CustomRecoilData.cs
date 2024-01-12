﻿using GameData;
using Il2CppSystem.Collections.Generic;

namespace ExtraRecoilData.Patches
{
    public class CustomRecoilData
    {
        public float RecoilScaleCap { get; set; } = 1f;
        public float RecoilScaleDecay { get; set; } = 1f;
        public float RecoilScaleDecayDelay { get; set; } = 0.016f;
        public float RecoilScaleGrowth { get; set; } = 0f;
        public float RecoilScaleMin { get; set; } = 1f;
        public float RecoilScaleMax { get; set; } = 1f;

        public List<float> RecoilPatternStored { get; set; } = new();
        public List<float> RecoilPatternFirstStored { get; set; } = new();
        public RecoilPatternAlign RecoilPatternAlign { get; set; } = RecoilPatternAlign.ALIGN;
        public MinMaxValue RecoilPatternPower { get; set; } = new() { Min = 0, Max = 0 };
        public float RecoilPatternResetDelay { get; set; } = 0.016f;
    }
}