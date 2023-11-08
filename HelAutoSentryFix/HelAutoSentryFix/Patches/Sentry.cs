using FX_EffectSystem;
using Gear;
using HarmonyLib;
using UnityEngine;

namespace HelSentryFix.Patches {
    [HarmonyPatch]
    internal class PlayerSentryPatches {
        private static void MakeShot(SentryGunInstance_Firing_Bullets __instance, bool doDamage, bool targetIsTagged) {
            SentryGunInstance_Firing_Bullets.s_weaponRayData.owner = __instance.m_core.Owner;
            SentryGunInstance_Firing_Bullets.s_weaponRayData.damage = __instance.m_archetypeData.GetSentryDamage(SentryGunInstance_Firing_Bullets.s_weaponRayData.owner, SentryGunInstance_Firing_Bullets.s_weaponRayData.rayHit.distance, targetIsTagged);
            SentryGunInstance_Firing_Bullets.s_weaponRayData.staggerMulti = __instance.m_archetypeData.GetSentryStaggerDamage(targetIsTagged);
            SentryGunInstance_Firing_Bullets.s_weaponRayData.precisionMulti = __instance.m_archetypeData.PrecisionDamageMulti;
            SentryGunInstance_Firing_Bullets.s_weaponRayData.damageFalloff = __instance.m_archetypeData.DamageFalloff;
            SentryGunInstance_Firing_Bullets.s_weaponRayData.vfxBulletHit = __instance.m_vfxBulletHit; // NOTE(randomuserhi): In base game, shotgun sentry does not have this line, but for easier code ive included it across all sentries
            Weapon.WeaponHitData temp = SentryGunInstance_Firing_Bullets.s_weaponRayData;
            if (__instance.m_archetypeData.PiercingBullets) {
                Debug.Log("Sentry has pierce!");
                int num2 = 5;
                int num3 = 0;
                bool flag = false;
                float num4 = 0f;
                int num5 = 0;
                Vector3 origin = __instance.MuzzleAlign.position;
                while (!flag && num3 < num2 && SentryGunInstance_Firing_Bullets.s_weaponRayData.maxRayDist > 0f && num5 < __instance.m_archetypeData.PiercingDamageCountLimit) {
                    Debug.Log($"Pierced ${num3}");
                    if (Weapon.CastWeaponRay(__instance.MuzzleAlign, ref temp, origin, LayerManager.MASK_SENTRYGUN_RAY)) {
                        SentryGunInstance_Firing_Bullets.s_weaponRayData = temp;
                        if (BulletWeapon.BulletHit(SentryGunInstance_Firing_Bullets.s_weaponRayData, doDamage)) {
                            num5++;
                        }
                        FX_Manager.EffectTargetPosition = SentryGunInstance_Firing_Bullets.s_weaponRayData.rayHit.point;
                        flag = !SentryGunInstance_Firing_Bullets.s_weaponRayData.rayHit.collider.gameObject.IsInLayerMask(LayerManager.MASK_BULLETWEAPON_PIERCING_PASS); // NOTE(randomuserhi): Using regular bulletweapon layer cause no idea if sentries even have one
                        origin = SentryGunInstance_Firing_Bullets.s_weaponRayData.rayHit.point + SentryGunInstance_Firing_Bullets.s_weaponRayData.fireDir * 0.1f;
                        num4 += SentryGunInstance_Firing_Bullets.s_weaponRayData.rayHit.distance;
                        SentryGunInstance_Firing_Bullets.s_weaponRayData.maxRayDist -= SentryGunInstance_Firing_Bullets.s_weaponRayData.rayHit.distance;
                    } else {
                        flag = true;
                        FX_Manager.EffectTargetPosition = __instance.MuzzleAlign.position + __instance.MuzzleAlign.forward * 50f;
                    }
                    num3++;
                }
            } else if (Weapon.CastWeaponRay(__instance.MuzzleAlign, ref temp, LayerManager.MASK_SENTRYGUN_RAY)) {
                SentryGunInstance_Firing_Bullets.s_weaponRayData = temp;
                BulletWeapon.BulletHit(SentryGunInstance_Firing_Bullets.s_weaponRayData, doDamage);
                FX_Manager.EffectTargetPosition = SentryGunInstance_Firing_Bullets.s_weaponRayData.rayHit.point;
            } else {
                FX_Manager.EffectTargetPosition = __instance.MuzzleAlign.position + __instance.MuzzleAlign.forward * 50f;
            }
        }

        [HarmonyPatch(typeof(SentryGunInstance_Firing_Bullets), nameof(SentryGunInstance_Firing_Bullets.FireBullet))]
        [HarmonyPrefix]
        private static bool SentryGunFiringBullet(SentryGunInstance_Firing_Bullets __instance, bool doDamage, bool targetIsTagged) {
            SentryFirePatches.sentryFire_Prefix?.Invoke(__instance, doDamage, targetIsTagged);
            SentryFirePatches.anySentryFire_Prefix?.Invoke(__instance, doDamage, targetIsTagged);

            SentryGunInstance_Firing_Bullets.s_weaponRayData = new Weapon.WeaponHitData {
                randomSpread = __instance.m_archetypeData.HipFireSpread,
                fireDir = __instance.MuzzleAlign.forward
            };
            if (__instance.m_archetypeData.Sentry_FireTowardsTargetInsteadOfForward && __instance.m_core.TryGetTargetAimPos(out var pos)) {
                SentryGunInstance_Firing_Bullets.s_weaponRayData.fireDir = (pos - __instance.MuzzleAlign.position).normalized;
            }
            MakeShot(__instance, doDamage, targetIsTagged);
            __instance.OnBulletFired?.Invoke();
            SentryGunInstance_Firing_Bullets.s_tracerPool.AquireEffect().Play(null, __instance.MuzzleAlign.position, Quaternion.LookRotation(SentryGunInstance_Firing_Bullets.s_weaponRayData.fireDir));
            __instance.m_muzzleFlash?.Play();
            WeaponShellManager.EjectShell(ShellTypes.Shell_338, 1, 1, __instance.ShellEjectAlign);
            __instance.m_fireBulletTimer = Clock.Time + __instance.m_archetypeData.GetSentryShotDelay(__instance.m_core.Owner, targetIsTagged);

            SentryFirePatches.sentryFire_Postfix?.Invoke(__instance, doDamage, targetIsTagged);
            SentryFirePatches.anySentryFire_Postfix?.Invoke(__instance, doDamage, targetIsTagged);
            return false;
        }

        // Special case for shotgun sentry
        [HarmonyPatch(typeof(SentryGunInstance_Firing_Bullets), nameof(SentryGunInstance_Firing_Bullets.UpdateFireShotgunSemi))]
        [HarmonyPrefix]
        private static bool ShotgunSentryFiring(SentryGunInstance_Firing_Bullets __instance, bool isMaster, bool targetIsTagged) {
            if (!(Clock.Time > __instance.m_fireBulletTimer)) {
                return false;
            }

            SentryFirePatches.shotgunSentryFire_Prefix?.Invoke(__instance, isMaster, targetIsTagged);
            SentryFirePatches.anySentryFire_Prefix?.Invoke(__instance, isMaster, targetIsTagged);

            Vector3 position = __instance.MuzzleAlign.position;
            __instance.TriggerSingleFireAudio();
            for (int i = 0; i < __instance.m_archetypeData.ShotgunBulletCount; i++) {
                float f = __instance.m_segmentSize * (float)i;
                float num = 0f;
                float num2 = 0f;
                if (i > 0) {
                    num += (float)__instance.m_archetypeData.ShotgunConeSize * Mathf.Cos(f);
                    num2 += (float)__instance.m_archetypeData.ShotgunConeSize * Mathf.Sin(f);
                }
                SentryGunInstance_Firing_Bullets.s_weaponRayData = new Weapon.WeaponHitData {
                    maxRayDist = __instance.MaxRayDist,
                    angOffsetX = num,
                    angOffsetY = num2,
                    randomSpread = __instance.m_archetypeData.ShotgunBulletSpread,
                    fireDir = __instance.MuzzleAlign.forward
                };
                MakeShot(__instance, isMaster, targetIsTagged);
                FX_Manager.PlayLocalVersion = false;
                SentryGunInstance_Firing_Bullets.s_tracerPool.AquireEffect().Play(null, __instance.MuzzleAlign.position, Quaternion.LookRotation(SentryGunInstance_Firing_Bullets.s_weaponRayData.fireDir));
            }
            __instance.UpdateAmmo(-1);
            __instance.OnBulletFired?.Invoke();
            __instance.m_fireBulletTimer = Clock.Time + __instance.m_archetypeData.GetSentryShotDelay(__instance.m_core.Owner, targetIsTagged);

            SentryFirePatches.shotgunSentryFire_Postfix?.Invoke(__instance, isMaster, targetIsTagged);
            SentryFirePatches.anySentryFire_Postfix?.Invoke(__instance, isMaster, targetIsTagged);
            return false;
        }
    }
}
