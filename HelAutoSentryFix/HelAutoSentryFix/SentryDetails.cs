namespace HelSentryFix {
    public static class SentryFirePatches {
        public delegate void AnySentryFire_Prefix(SentryGunInstance_Firing_Bullets __instance, bool doDamage, bool targetIsTagged);
        public delegate void AnySentryFire_Postfix(SentryGunInstance_Firing_Bullets __instance, bool doDamage, bool targetIsTagged);

        public delegate void SentryFire_Prefix(SentryGunInstance_Firing_Bullets __instance, bool doDamage, bool targetIsTagged);
        public delegate void SentryFire_Postfix(SentryGunInstance_Firing_Bullets __instance, bool doDamage, bool targetIsTagged);
        public delegate void ShotgunSentryFire_Prefix(SentryGunInstance_Firing_Bullets __instance, bool doDamage, bool targetIsTagged);
        public delegate void ShotgunSentryFire_Postfix(SentryGunInstance_Firing_Bullets __instance, bool doDamage, bool targetIsTagged);

        public static AnySentryFire_Prefix? anySentryFire_Prefix;
        public static AnySentryFire_Postfix? anySentryFire_Postfix;

        public static SentryFire_Prefix? sentryFire_Prefix;
        public static SentryFire_Prefix? sentryFire_Postfix;

        public static ShotgunSentryFire_Prefix? shotgunSentryFire_Prefix;
        public static ShotgunSentryFire_Prefix? shotgunSentryFire_Postfix;
    }
}
