using GameData;
using Gear;
using HarmonyLib;
using UnityEngine;

namespace ExtraRecoilData.Patches
{
    internal static class RecoilPatches
    {
        static Weapon? cachedWeapon;
        static CustomRecoilManager? cachedManager;

        private static CustomRecoilManager? GetCustomRecoilManager(Weapon? newWeapon = null)
        {
            if (newWeapon == null)
                return cachedManager;

            if (newWeapon != cachedWeapon)
            {
                cachedWeapon = newWeapon;
                cachedManager = newWeapon.GetComponent<CustomRecoilManager>();
            }

            return cachedManager;
        }

        [HarmonyPatch(typeof(BulletWeaponArchetype), nameof(BulletWeaponArchetype.PostFireCheck))]
        [HarmonyWrapSafe]
        [HarmonyPostfix]
        private static void UpdateCustomRecoil(BulletWeaponArchetype __instance)
        {
            CustomRecoilManager? crm = GetCustomRecoilManager(__instance.m_weapon);
            if (crm == null) return;

            crm.FireTriggered(System.Math.Max(0, System.Math.Max(__instance.m_nextShotTimer, __instance.m_nextBurstTimer) - Clock.Time));
        }

        [HarmonyPatch(typeof(Weapon), nameof(Weapon.ApplyRecoil))]
        [HarmonyWrapSafe]
        [HarmonyPrefix]
        private static bool CustomApplyRecoil(Weapon __instance, bool resetSimilarity = true)
        {
            if (!__instance.Owner.IsLocallyOwned)
            {
                return false;
            }

            // Direct copy-paste from datamine, except this line. Necessary to patch into the weapon and modify the function called
            // since we need to pass in the weapon-unique custom recoil data.
            // We don't want to attach it to the recoil data in case multiple weapons use the same recoil data.
            CustomRecoilManager? crm = GetCustomRecoilManager(__instance);
            Vector2 vector = CustomCameraRecoil(__instance.Owner.FPSCamera, resetSimilarity, __instance.RecoilData, crm);

            if (__instance.RecoilAnimation != null)
            {
                int num = 0;
                float num2 = 0f;
                int i;
                for (i = 0; i < __instance.m_recoilTimes.Length; i++)
                {
                    float num3 = Clock.Time - __instance.m_recoilTimes[i];
                    if (__instance.m_recoilTimes[i] < Mathf.Epsilon || num3 > __instance.RecoilAnimation.GetDuration())
                    {
                        __instance.m_recoilTimes[i] = Clock.Time;
                        __instance.m_recoilVariance[i] = Random.value;
                        break;
                    }
                    if (num3 > num2)
                    {
                        num = i;
                        num2 = num3;
                    }
                }
                if (i == __instance.m_recoilTimes.Length)
                {
                    __instance.m_recoilTimes[num] = Clock.Time;
                    __instance.m_recoilVariance[num] = Random.value;
                }
            }
            else
            {
                __instance.m_recoilPosOffset.Damping = __instance.RecoilData.recoilPosDamping;
                __instance.m_recoilPosOffset.Stiffness = __instance.RecoilData.recoilPosStiffness;
                __instance.m_recoilRotOffset.Damping = __instance.RecoilData.recoilRotDamping;
                __instance.m_recoilRotOffset.Stiffness = __instance.RecoilData.recoilRotStiffness;
                Vector3 force = Vector3.right * vector.x * __instance.RecoilData.recoilPosImpulse.x + Vector3.up * vector.y * __instance.RecoilData.recoilPosImpulse.y + Vector3.forward * __instance.RecoilData.recoilPosImpulse.z;
                Vector3 shift = Vector3.right * vector.x * __instance.RecoilData.recoilPosShift.x + Vector3.up * vector.y * __instance.RecoilData.recoilPosShift.y + Vector3.forward * __instance.RecoilData.recoilPosShift.z;
                Vector3 vector2 = Vector3.right * vector.y * __instance.RecoilData.recoilRotImpulse.x + Vector3.up * vector.x * __instance.RecoilData.recoilRotImpulse.y + Vector3.forward * vector.x * __instance.RecoilData.recoilRotImpulse.z;
                force *= __instance.RecoilData.recoilPosImpulseWeight;
                shift *= __instance.RecoilData.recoilPosShiftWeight;
                __instance.m_recoilPosOffset.Impulse(force);
                __instance.m_recoilPosOffset.Shift(shift);
                __instance.m_recoilRotOffset.Impulse(vector2 * __instance.RecoilData.recoilRotImpulseWeight);
            }
            return false;
        }

        private static Vector2 CustomCameraRecoil(FPSCamera camera, bool resetSimilarity, RecoilDataBlock recoilData, CustomRecoilManager? recoilManager)
        {
            FPS_RecoilSystem? rs = camera.m_recoilSystem;
            if (rs == null)
                return Vector2.zero;

            rs.m_spring = recoilData.spring;
            rs.m_damp = recoilData.dampening;
            rs.recoilDir = new Vector2(recoilData.verticalScale.GetRandom(), recoilData.horizontalScale.GetRandom());
            if (rs.recoilDir.sqrMagnitude < 0.01f)
            {
                Vector2 recoilVector = rs.recoilDir;
                recoilVector.x = recoilData.verticalScale.Max;
                rs.recoilDir = recoilVector;
            }
            rs.recoilDir.Normalize();
            if (resetSimilarity)
            {
                rs.m_lastRecoilDir = rs.recoilDir;
            }
            else
            {
                rs.m_lastRecoilDir = Vector2.Lerp(rs.recoilDir, rs.m_lastRecoilDir, recoilData.directionalSimilarity);
            }
            rs.recoilDir *= recoilData.power.GetRandom();

            // Only custom part here
            if (recoilManager != null)
                rs.recoilDir = recoilManager.GetModifiedRecoil(rs.recoilDir);

            Vector2 assignVector = rs.currentRecoilForce;
            assignVector.x -= rs.recoilDir.x * (1f - recoilData.worldToViewSpaceBlendVertical);
            assignVector.y -= rs.recoilDir.y * (1f - recoilData.worldToViewSpaceBlendHorizontal);
            rs.currentRecoilForce = assignVector;

            assignVector = rs.currentRecoilForceVP;
            assignVector.x -= rs.recoilDir.x * recoilData.worldToViewSpaceBlendVertical;
            assignVector.y -= rs.recoilDir.y * recoilData.worldToViewSpaceBlendHorizontal;
            rs.currentRecoilForceVP = assignVector;

            rs.m_recoilRotateOffset = rs.currentRecoilOffset;
            rs.currentRecoilOffset = Vector2.zero;
            rs.m_hasOverrideParentRotation = true;
            GuiManager.CrosshairLayer.PopCircleCrosshair(recoilData.hipFireCrosshairRecoilPop, recoilData.hipFireCrosshairSizeMax);
            rs.m_returnVector.Set(rs.currentRecoilForceVP.y * 0.1f, rs.currentRecoilForceVP.x * 0.1f);
            rs.m_concussionShakeIntensity = recoilData.concussionIntensity;
            rs.m_concussionShakeFrequency = recoilData.concussionFrequency * 2f * (float)System.Math.PI;
            rs.m_concussionShakeOffset = UnityEngine.Random.value * 2f * (float)System.Math.PI;
            rs.m_concussionShakeDuration = recoilData.concussionDuration;
            rs.m_concussionShakeTimer = 1f;

            return rs.m_returnVector;
        }
    }
}
