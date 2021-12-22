using HarmonyLib;
using NeosModLoader;
using FrooxEngine;
using BaseX;
using FrooxEngine.UIX;
using FrooxEngine.Undo;
using CloudX.Shared;
using FrooxEngine.UIX;
using System.Collections.Generic;

namespace MessageCopy
{
    public class MessageCopy : NeosMod
    {
        public override string Name => "MessageCopy";
        public override string Author => "kka429";
        public override string Version => "1.2.0";
        public override string Link => "https://github.com/rassi0429/MessageCopy"; // this line is optional and can be omitted

        public override void OnEngineInit()
        {
            Harmony harmony = new Harmony("dev.kokoa.messagecopy");
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(FriendsDialog))]
        [HarmonyPatch("AddMessage")]
        class MessageCopyPatch
        {
            static void Postfix(UIBuilder ___messagesUi, FriendsDialog __instance, ref Image __result, Message message)
            {
                if (message.MessageType == CloudX.Shared.MessageType.Text)
                {
                    List<Slot> child = ___messagesUi.Current.GetAllChildren();
                    foreach (Slot c in child)
                    {
                        if (c.GetComponent<Text>() != null)
                        {
                            var text = c.GetComponent<Text>();
                            string msg = text.Content;
                            Msg(msg);
                            text.Destroy();
                            var ui = new UIBuilder(c);
                            ui.Button(msg, new color(1, 1, 1, 0)).LocalPressed += (IButton btn, ButtonEventData _) =>
                            {
                                if (!__instance.InputInterface.IsClipboardSupported)
                                    return;
                                btn.World.InputInterface.Clipboard.SetText(btn.LabelText);
                            };
                            var btnText = c.GetComponentInChildren<Text>();
                            btnText.Align = message.IsSent ? Alignment.MiddleRight : Alignment.MiddleLeft;
                        }
                    }
                }
                else if (message.MessageType == CloudX.Shared.MessageType.SessionInvite)
                {
                    Msg("Invite");
                    List<Slot> child = ___messagesUi.Current.GetAllChildren();
                    foreach (Slot c in child)
                    {
                        if (c.GetComponent<Text>() != null)
                        {
                            var text = c.GetComponent<Text>().Content;
                            if (text == "Join")
                            {
                                //Source(Join) Button Resize
                                Slot DuplicatedSlot_Src = c.Parent;
                                DuplicatedSlot_Src.GetComponent<RectTransform>().AnchorMin.Value = new float2(0.8f, 0f);

                                //Create Orb Button
                                Slot DuplicatedSlot_Orb = c.Parent.Duplicate();
                                DuplicatedSlot_Orb.GetComponent<RectTransform>().AnchorMin.Value = new float2(0.6f, 0f);
                                DuplicatedSlot_Orb.GetComponent<RectTransform>().AnchorMax.Value = new float2(0.79f, 1f);

                                DuplicatedSlot_Orb.GetComponent<Button>().Destroy();
                                DuplicatedSlot_Orb.GetComponent<Image>().Destroy();

                                DuplicatedSlot_Orb.AttachComponent<Image>();
                                DuplicatedSlot_Orb.GetComponentInChildren<Text>().Content.Value = "Orb";

                                DuplicatedSlot_Orb.AttachComponent<Button>().LocalPressed += (IButton b, ButtonEventData _) =>
                                {
                                    SessionInfo sessionInfo = message.ExtractContent<SessionInfo>();
                                    World world = __instance.LocalUser.World.WorldManager.FocusedWorld;
                                    world.RunSynchronously(() =>
                                    {
                                        Slot slot = world.RootSlot.LocalUserSpace.AddSlot("World Orb");
                                        WorldOrb worldOrb = slot.AttachComponent<WorldOrb>();
                                        worldOrb.ActiveSessionURLs = sessionInfo.GetSessionURLs();
                                        worldOrb.ActiveUsers.Value = sessionInfo.JoinedUsers;
                                        worldOrb.WorldName = sessionInfo.Name;
                                        slot.PositionInFrontOfUser();
                                    });
                                };

                                //Create Open Button
                                Slot DuplicatedSlot_Open = c.Parent.Duplicate();
                                DuplicatedSlot_Open.GetComponent<RectTransform>().AnchorMin.Value = new float2(0.6f, 0f);
                                DuplicatedSlot_Open.GetComponent<RectTransform>().AnchorMax.Value = new float2(0.79f, 1f);

                                DuplicatedSlot_Open.GetComponent<Button>().Destroy();
                                DuplicatedSlot_Open.GetComponent<Image>().Destroy();

                                DuplicatedSlot_Open.AttachComponent<Image>();
                                DuplicatedSlot_Open.GetComponentInChildren<Text>().Content.Value = "Open";

                                DuplicatedSlot_Open.AttachComponent<Button>().LocalPressed += (IButton b, ButtonEventData _) =>
                                {
                                    SessionInfo sessionInfo = message.ExtractContent<SessionInfo>();
                                    World world = __instance.LocalUser.World.WorldManager.FocusedWorld;
                                    world.RunSynchronously(() =>
                                    {
                                        if (sessionInfo.HasEnded)
                                            return;
                                        Userspace.OpenWorld(new WorldStartSettings()
                                        {
                                            URIs = sessionInfo.GetSessionURLs(),
                                            GetExisting = true,
                                            AutoFocus = false
                                        });
                                    });
                                };
                                ////Create Copy Button
                                //Slot DuplicatedSlot_Copy = c.Parent.Duplicate();
                                //DuplicatedSlot_Copy.GetComponent<RectTransform>().AnchorMin.Value = new float2(0.4f, 0f);
                                //DuplicatedSlot_Copy.GetComponent<RectTransform>().AnchorMax.Value = new float2(0.59f, 1f);

                                //DuplicatedSlot_Copy.GetComponent<Button>().Destroy();
                                //DuplicatedSlot_Copy.GetComponent<Image>().Destroy();

                                //DuplicatedSlot_Copy.AttachComponent<Image>();
                                //DuplicatedSlot_Copy.GetComponentInChildren<Text>().Content.Value = "Copy";

                                //DuplicatedSlot_Copy.AttachComponent<Button>().LocalPressed += (IButton b, ButtonEventData _) =>
                                //{
                                //    SessionInfo sessionInfo = message.ExtractContent<SessionInfo>();
                                //    World world = __instance.LocalUser.World.WorldManager.FocusedWorld;
                                //    world.RunSynchronously(() =>
                                //    {
                                //        if (!__instance.InputInterface.IsClipboardSupported)
                                //            return;
                                //        b.World.InputInterface.Clipboard.SetText("neos-session:///" + sessionInfo.SessionId);
                                //    });
                                //};
                            }
                        }
                    }
                }
                else if (message.MessageType == CloudX.Shared.MessageType.Sound)
                {
                    FrooxEngine.Record record = message.ExtractContent<FrooxEngine.Record>();
                    Msg(record.AssetURI);
                    List<Slot> child = ___messagesUi.Current.GetAllChildren();
                    foreach (Slot c in child)
                    {
                        if (c.Name == "Header")
                        {
                            Msg(c.ChildrenCount);
                            if (c.ChildrenCount == 3)
                            {
                                Slot d = c.Duplicate();
                                RectTransform r = d.GetComponent<RectTransform>();
                                r.AnchorMin.Value = new float2(0.92f, 0f);
                                r.AnchorMax.Value = new float2(1f, 1f);
                                r.OffsetMin.Value = new float2(0f, 0f);
                            }
                        }
                    }
                }
            }
        }
    }
}