using HarmonyLib;
using System;
using static TheOtherRoles.TheOtherRoles;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace TheOtherRoles.Patches {
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
    class IntroCutsceneOnDestroyPatch
    {
        public static void Prefix(IntroCutscene __instance) {
            // Generate and initialize player icons
            int playerCounter = 0;
            if (PlayerControl.LocalPlayer != null && HudManager.Instance != null) {
                Vector3 bottomLeft = new Vector3(-HudManager.Instance.UseButton.transform.localPosition.x, HudManager.Instance.UseButton.transform.localPosition.y, HudManager.Instance.UseButton.transform.localPosition.z);
                foreach (PlayerControl p in PlayerControl.AllPlayerControls) {
                    GameData.PlayerInfo data = p.Data;
                    PoolablePlayer player = UnityEngine.Object.Instantiate<PoolablePlayer>(__instance.PlayerPrefab, HudManager.Instance.transform);
                    PlayerControl.SetPlayerMaterialColors(data.DefaultOutfit.ColorId, player.CurrentBodySprite.BodySprite);
                    player.SetSkin(data.DefaultOutfit.SkinId, data.DefaultOutfit.ColorId);
                    player.HatSlot.SetHat(data.DefaultOutfit.HatId, data.DefaultOutfit.ColorId);
                    PlayerControl.SetPetImage(data.DefaultOutfit.PetId, data.DefaultOutfit.ColorId, player.PetSlot);
                    player.NameText.text = data.PlayerName;
                    player.SetFlipX(true);
                    MapOptions.playerIcons[p.PlayerId] = player;

                    if (PlayerControl.LocalPlayer == Arsonist.arsonist && p != Arsonist.arsonist) {
                        player.transform.localPosition = bottomLeft + new Vector3(-0.25f, -0.25f, 0) + Vector3.right * playerCounter++ * 0.35f;
                        player.transform.localScale = Vector3.one * 0.2f;
                        player.setSemiTransparent(true);
                        player.gameObject.SetActive(true);
                    } else if (PlayerControl.LocalPlayer == BountyHunter.bountyHunter) {
                        player.transform.localPosition = bottomLeft + new Vector3(-0.25f, 0f, 0);
                        player.transform.localScale = Vector3.one * 0.4f;
                        player.gameObject.SetActive(false);
                    } else {
                        player.gameObject.SetActive(false);
                    }
                }
            }

            // Force Bounty Hunter to load a new Bounty when the Intro is over
            if (BountyHunter.bounty != null && PlayerControl.LocalPlayer == BountyHunter.bountyHunter) {
                BountyHunter.bountyUpdateTimer = 0f;
                if (HudManager.Instance != null) {
                    Vector3 bottomLeft = new Vector3(-HudManager.Instance.UseButton.transform.localPosition.x, HudManager.Instance.UseButton.transform.localPosition.y, HudManager.Instance.UseButton.transform.localPosition.z) + new Vector3(-0.25f, 1f, 0);
                    BountyHunter.cooldownText = UnityEngine.Object.Instantiate<TMPro.TextMeshPro>(HudManager.Instance.KillButton.cooldownTimerText, HudManager.Instance.transform);
                    BountyHunter.cooldownText.alignment = TMPro.TextAlignmentOptions.Center;
                    BountyHunter.cooldownText.transform.localPosition = bottomLeft + new Vector3(0f, -1f, -1f);
                    BountyHunter.cooldownText.gameObject.SetActive(true);
                }
            } 

            }
    }

    [HarmonyPatch]
    class IntroPatch {
        public static void setupIntroTeamIcons(IntroCutscene __instance, ref  Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam) {
            // Intro solo teams
            if (PlayerControl.LocalPlayer == Jester.jester || PlayerControl.LocalPlayer == Jackal.jackal || PlayerControl.LocalPlayer == Arsonist.arsonist || PlayerControl.LocalPlayer == Vulture.vulture || PlayerControl.LocalPlayer == Lawyer.lawyer) {
                var soloTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                soloTeam.Add(PlayerControl.LocalPlayer);
                yourTeam = soloTeam;
            }

            // Add the Spy to the Impostor team (for the Impostors)
            if (Spy.spy != null && PlayerControl.LocalPlayer.Data.Role.IsImpostor) {
                List<PlayerControl> players = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
                var fakeImpostorTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>(); // The local player always has to be the first one in the list (to be displayed in the center)
                fakeImpostorTeam.Add(PlayerControl.LocalPlayer);
                foreach (PlayerControl p in players) {
                    if (PlayerControl.LocalPlayer != p && (p == Spy.spy || p.Data.Role.IsImpostor))
                        fakeImpostorTeam.Add(p);
                }
                yourTeam = fakeImpostorTeam;
            }
        }

        public static void setupIntroTeam(IntroCutscene __instance, ref  Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam) {
            List<RoleInfo> infos = RoleInfo.getRoleInfoForPlayer(PlayerControl.LocalPlayer);
            RoleInfo roleInfo = infos.Where(info => info.roleId != RoleId.Lover).FirstOrDefault();
            if (roleInfo == null) return;
            if (roleInfo.isNeutral) {
                var neutralColor = new Color32(76, 84, 78, 255);
                __instance.BackgroundBar.material.color = neutralColor;
                __instance.TeamTitle.text = "Neutral";
                __instance.TeamTitle.color = neutralColor;
            }
        }

        public static IEnumerator<WaitForSeconds> EndShowRole(IntroCutscene __instance) {
            yield return new WaitForSeconds(5f);

            __instance.YouAreText.gameObject.SetActive(false);
            __instance.RoleText.gameObject.SetActive(false);
            __instance.RoleBlurbText.gameObject.SetActive(false);
            __instance.ourCrewmate.gameObject.SetActive(false);
           
        }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.ShowRole))]
        class SetUpRoleTextPatch {
            public static bool Prefix(IntroCutscene __instance) {
                if (!CustomOptionHolder.activateRoles.getBool()) return true; // Don't override the intro of the vanilla roles

                RoleBehaviour role = PlayerControl.LocalPlayer.Data.Role;
                __instance.RoleText.text = DestroyableSingleton<TranslationController>.Instance.GetString(role.StringName, Array.Empty<Il2CppSystem.Object>());
                __instance.RoleBlurbText.text = role.Blurb;
                __instance.RoleText.color = role.TeamColor;
                __instance.YouAreText.color = role.TeamColor;
                __instance.RoleBlurbText.color = role.TeamColor;

                List<RoleInfo> infos = RoleInfo.getRoleInfoForPlayer(PlayerControl.LocalPlayer);
                RoleInfo roleInfo = infos.Where(info => info.roleId != RoleId.Lover).FirstOrDefault();

                if (roleInfo != null) {
                    __instance.RoleText.text = roleInfo.name;
                    __instance.RoleText.color = roleInfo.color;
                    __instance.RoleBlurbText.text = roleInfo.introDescription;
                    __instance.RoleBlurbText.color = roleInfo.color;
                }

                if (infos.Any(info => info.roleId == RoleId.Lover)) {
                    PlayerControl otherLover = PlayerControl.LocalPlayer == Lovers.lover1 ? Lovers.lover2 : Lovers.lover1;
                    __instance.RoleBlurbText.text += Helpers.cs(Lovers.color, $"\n♥ You are in love with {otherLover?.Data?.PlayerName ?? ""} ♥");
                }
                if (Deputy.knowsSheriff && Deputy.deputy != null && Sheriff.sheriff != null) {
                    if (infos.Any(info => info.roleId == RoleId.Sheriff)) 
                        __instance.RoleBlurbText.text += Helpers.cs(Sheriff.color, $"\nYour Deputy is {Deputy.deputy?.Data?.PlayerName ?? ""}");
                    else if (infos.Any(info => info.roleId == RoleId.Deputy))
                        __instance.RoleBlurbText.text += Helpers.cs(Sheriff.color, $"\nYour Sheriff is {Sheriff.sheriff?.Data?.PlayerName ?? ""}");
                }
                SoundManager.Instance.PlaySound(PlayerControl.LocalPlayer.Data.Role.IntroSound, false);
                __instance.YouAreText.gameObject.SetActive(true);
                __instance.RoleText.gameObject.SetActive(true);
                __instance.RoleBlurbText.gameObject.SetActive(true);
                if ((UnityEngine.Object)__instance.ourCrewmate == (UnityEngine.Object)null) {
                    __instance.ourCrewmate = __instance.CreatePlayer(0, 1, PlayerControl.LocalPlayer.Data, false);
                    __instance.ourCrewmate.gameObject.SetActive(false);
                }
                __instance.ourCrewmate.gameObject.SetActive(true);
                __instance.ourCrewmate.transform.localPosition = new Vector3(0.0f, -1.05f, -18f);
                __instance.ourCrewmate.transform.localScale = new Vector3(1f, 1f, 1f);
                __instance.StartCoroutine((Il2CppSystem.Collections.IEnumerator)EndShowRole(__instance));

                return false;
            }
        }

            [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
        class BeginCrewmatePatch {
            public static void Prefix(IntroCutscene __instance, ref  Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay) {
                setupIntroTeamIcons(__instance, ref teamToDisplay);
            }

            public static void Postfix(IntroCutscene __instance, ref  Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay) {
                setupIntroTeam(__instance, ref teamToDisplay);
            }
        }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
        class BeginImpostorPatch {
            public static void Prefix(IntroCutscene __instance, ref  Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam) {
                setupIntroTeamIcons(__instance, ref yourTeam);
            }

            public static void Postfix(IntroCutscene __instance, ref  Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam) {
                setupIntroTeam(__instance, ref yourTeam);
            }
        }
    }

    [HarmonyPatch(typeof(Constants), nameof(Constants.ShouldHorseAround))]
    public static class ShouldAlwaysHorseAround {
        public static bool Prefix(ref bool __result) {
            __result = MapOptions.enableHorseMode;

            return false;
        }
    }
}

