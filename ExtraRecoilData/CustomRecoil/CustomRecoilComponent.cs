using ExtraRecoilData.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExtraRecoilData.CustomRecoil
{
    public class CustomRecoilComponent : MonoBehaviour
    {
        public CustomRecoilComponent(IntPtr value) : base(value) { }

        protected CustomRecoilData data = new();
        public CustomRecoilData Data
        {
            get { return data; }
            set
            {
                data = value;
                SetRecoilPattern(ref recoilPattern, data.RecoilPattern);
                SetRecoilPattern(ref recoilPatternFirst, data.RecoilPatternFirst);
            }
        }

        protected List<Vector2> recoilPattern = new();
        protected int recoilPatternIndex = 0;
        protected List<Vector2> recoilPatternFirst = new();
        protected int recoilPatternFirstIndex = 0;

        protected float recoilScaleProgress = 0f;
        protected float lastUpdateTime = 0f;
        protected float lastShotTime = 0f;
        protected float shotDelay = 0f;

        protected static void SetRecoilPattern(ref List<Vector2> localPattern, List<float> pattern)
        {
            localPattern.Clear();
            foreach (float val in pattern)
            {
                // Only values in the range [-1, 1] are considered euclidean; if any are not, then it is a polar pattern.
                // Patterns can only be of one type.
                if (Math.Abs(val) > 1)
                {
                    localPattern = CreatePatternFromPolar(pattern);
                    return;
                }
            }

            localPattern = CreatePatternFromEuclidean(pattern);
        }

        protected static List<Vector2> CreatePatternFromPolar(List<float> pattern)
        {
            List<Vector2> newPattern = new(pattern.Count);
            for (int i = 0; i < pattern.Count; i++)
            {
                // Angles are expected to be in degrees with clock angles (0 = up, positive = right).
                float angle = (-pattern[i] + 90f) * Mathf.Deg2Rad;
                newPattern.Add(new Vector2((float)Math.Sin(angle), (float)-Math.Cos(angle)));
            }
            return newPattern;
        }

        protected static List<Vector2> CreatePatternFromEuclidean(List<float> pattern)
        {
            List<Vector2> newPattern = new(pattern.Count / 2);
            for (int i = 0; i < pattern.Count - 1; i += 2)
            {
                Vector2 dir = pattern[i] != 0 || pattern[i + 1] != 0 ? new Vector2(pattern[i + 1], -pattern[i]) : Vector2.right;
                dir.Normalize();
                newPattern.Add(dir);
            }
            return newPattern;
        }

        protected void UpdateToPresent()
        {
            if (lastUpdateTime == Clock.Time) return;

            float shotDelta = Clock.Time - lastShotTime - shotDelay;
            float delta = Clock.Time - lastUpdateTime;
            if (shotDelta > data.RecoilScaleDecayDelay)
            {
                float decayDelta = delta - Math.Max(0, data.RecoilScaleDecayDelay + lastShotTime + shotDelay - lastUpdateTime);
                recoilScaleProgress = Math.Max(0, Math.Min(data.RecoilScaleCap, recoilScaleProgress - data.RecoilScaleDecay * decayDelta));
            }

            if (shotDelta > data.RecoilPatternResetDelay)
            {
                recoilPatternIndex = 0;
                recoilPatternFirstIndex = 0;
            }

            lastUpdateTime = Clock.Time;
        }

        public Vector2 GetModifiedRecoil(Vector2 recoilDir)
        {
            // Recoil is triggered before FireTriggered runs, so we need to make sure the custom recoil is up to date.
            UpdateToPresent();

            float scale = Mathf.Lerp(data.RecoilScaleMin, data.RecoilScaleMax, recoilScaleProgress / data.RecoilScaleCap);
            Vector2 patternDir = Vector2.right;
            if (recoilPatternFirstIndex < recoilPatternFirst.Count)
                patternDir = recoilPatternFirst[recoilPatternFirstIndex];
            else if (recoilPattern.Count > 0)
                patternDir = recoilPattern[recoilPatternIndex];

            // patternDir is effectively the cos and sin 90 degrees left of the angle we want. This rotates using the right of patternDir.
            if (data.RecoilPatternAlign == RecoilPatternAlign.ALIGN)
                recoilDir.Set(recoilDir.x * patternDir.x - recoilDir.y * patternDir.y, recoilDir.y * patternDir.x + recoilDir.x * patternDir.y);

            return (recoilDir + patternDir * data.RecoilPatternPower.GetRandom()) * scale;
        }

        public void FireTriggered(float newDelay)
        {
            // JFS - Should be called by GetModifiedRecoil running earlier.
            UpdateToPresent();

            lastShotTime = Clock.Time;
            shotDelay = newDelay;

            recoilScaleProgress = Math.Min(recoilScaleProgress + data.RecoilScaleGrowth, data.RecoilScaleCap);
            if (recoilPatternFirstIndex < recoilPatternFirst.Count)
                recoilPatternFirstIndex++;
            else if (recoilPattern.Count > 0)
                recoilPatternIndex = (recoilPatternIndex + 1) % recoilPattern.Count;
        }
    }
}
