using HarmonyLib;
using NeosModLoader;
using System;
using System.Reflection;
using FrooxEngine;
using BaseX;
using FrooxEngine.UIX;
using FrooxEngine.Undo;
using CloudX.Shared;
using CodeX;
using System.Collections.Generic;

namespace MessageCopy
{
    public class MessageCopy : NeosMod
    {
        public override string Name => "MessageCopy";
        public override string Author => "kka429";
        public override string Version => "1.1.0";
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
                            ui.Button(msg, new color(1, 1, 1, 0)).LocalPressed += (IButton btn, ButtonEventData _) => { btn.World.InputInterface.Clipboard.SetText(btn.LabelText); };
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
                                Slot d = c.Parent.Duplicate();
                                var t = d.GetComponent<RectTransform>();
                                t.AnchorMin.Value = new float2(0.4f, 0f);
                                t.AnchorMax.Value = new float2(0.69f, 1f);
                                var btn = d.GetComponent<Button>();
                                var image = d.GetComponent<Image>();
                                btn.Destroy();
                                image.Destroy();
                                var newImage = d.AttachComponent<Image>();
                                var newBtn = d.AttachComponent<Button>();
                                newBtn.LocalPressed += (IButton b, ButtonEventData _) =>
                                {
                                    SessionInfo sessionInfo = message.ExtractContent<SessionInfo>();
                                    World world = __instance.LocalUser.World.WorldManager.FocusedWorld;
                                    world.RunSynchronously((Action)(() =>
                                    {
                                        Slot slot = world.RootSlot.LocalUserSpace.AddSlot("World Orb");
                                        WorldOrb worldOrb = slot.AttachComponent<WorldOrb>();
                                        worldOrb.ActiveSessionURLs = sessionInfo.GetSessionURLs();
                                        worldOrb.ActiveUsers.Value = sessionInfo.JoinedUsers;
                                        worldOrb.WorldName = sessionInfo.Name;
                                        slot.PositionInFrontOfUser();
                                    }));
                                };
                                d.GetComponentInChildren<Text>().Content.Value = "Orb";
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