using HarmonyLib;
using NeosModLoader;
using System;
using System.Reflection;
using FrooxEngine;
using BaseX;
using CloudX.Shared;
using CodeX;
using FrooxEngine.UIX;
using System.Collections.Generic;

namespace MessageCopy
{
    public class MessageCopy : NeosMod
    {
        public override string Name => "MessageCopy";
        public override string Author => "kka429";
        public override string Version => "1.0.0";
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
                if (message.MessageType != CloudX.Shared.MessageType.Text) return;
                List<Slot> child = ___messagesUi.Current.GetAllChildren();
                foreach (Slot c in child)
                {
                    if (c.GetComponent<Text>() != null)
                    {
                        var text = c.GetComponent<Text>();
                        string msg = text.Content;
                        text.Destroy();
                        var ui = new UIBuilder(c);
                        ui.Button(msg, new color(1, 1, 1, 0.8f)).LocalPressed += (IButton btn, ButtonEventData _) => { btn.World.InputInterface.Clipboard.SetText(btn.LabelText); };
                        var btnText = c.GetComponentInChildren<Text>();
                        btnText.Align = message.IsSent ? Alignment.MiddleRight : Alignment.MiddleLeft;
                    }
                }
            }
        }
    }
}