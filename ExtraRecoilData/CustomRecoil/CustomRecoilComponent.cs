using System;
using System.Collections.Generic;
using UnityEngine;
using ExtraRecoilData.Utils;
using System.Linq;

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
                data.RecoilScaleCap = data.RecoilScaleCap >= 0 ? data.RecoilScaleCap : 0;
                data.RecoilScaleThreshold = Mathf.Clamp(data.RecoilScaleThreshold, 0f, data.RecoilScaleCap);
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
        protected float nextShotTime = 0f;

        protected static void SetRecoilPattern(ref List<Vector2> localPattern, List<float> pattern)
        {
            localPattern.Clear();
            if (pattern.Any(val => Math.Abs(val) > 1))
                localPattern = CreatePatternFromPolar(pattern);
            else
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
                // Direction is modified here since the input is setup as an intuitive (x, y) coordinate with +x -> right and +y -> up
                // When applied, however, it is (y, x) with +x -> left and +y -> up.
                Vector2 dir = pattern[i] != 0 || pattern[i + 1] != 0 ? new Vector2(pattern[i + 1], -pattern[i]) : Vector2.right;
                dir.Normalize();
                newPattern.Add(dir);
            }
            return newPattern;
        }

        protected void UpdateToPresent()
        {
            if (lastUpdateTime == Clock.Time) return;

            float shotDelta = Clock.Time - nextShotTime;
            float delta = Clock.Time - lastUpdateTime;
            if (shotDelta > data.RecoilScaleDecayDelay)
            {
                // If the last update occured before the delay finished, reduce the delta by the missing amount.
                float decayDelta = delta - Math.Max(0, data.RecoilScaleDecayDelay + nextShotTime - lastUpdateTime);
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

            float scale = recoilScaleProgress.Map(data.RecoilScaleThreshold, data.RecoilScaleCap, data.RecoilScaleMin, data.RecoilScaleMax);
            float patternScale = recoilScaleProgress.Map(data.RecoilScaleThreshold, data.RecoilScaleCap, data.RecoilPatternScaleMin, data.RecoilPatternScaleMax);
            Vector2 patternDir = Vector2.right; // right is up in recoil land
            if (recoilPatternFirstIndex < recoilPatternFirst.Count)
                patternDir = recoilPatternFirst[recoilPatternFirstIndex];
            else if (recoilPattern.Count > 0)
                patternDir = recoilPattern[recoilPatternIndex];

            // patternDir is effectively the cos and sin 90 degrees left of the angle we want. This rotates using the right of patternDir.
            if (data.RecoilPatternAlign == RecoilPatternAlign.ALIGN)
                recoilDir.Set(recoilDir.x * patternDir.x - recoilDir.y * patternDir.y, recoilDir.y * patternDir.x + recoilDir.x * patternDir.y);

            return (recoilDir * scale + patternDir * data.RecoilPatternPower.GetRandom() * patternScale);
        }

        public void FireTriggered(float newShotTime)
        {
            // JFS - Should be called by GetModifiedRecoil running earlier.
            UpdateToPresent();

            nextShotTime = data.RecoilDelayStartOnFire ? Clock.Time : newShotTime;

            recoilScaleProgress = Math.Min(recoilScaleProgress + 1, data.RecoilScaleCap);
            if (recoilPatternFirstIndex < recoilPatternFirst.Count)
                recoilPatternFirstIndex++;
            else if (recoilPattern.Count > 0)
                recoilPatternIndex = (recoilPatternIndex + 1) % recoilPattern.Count;
        }
    }
}
