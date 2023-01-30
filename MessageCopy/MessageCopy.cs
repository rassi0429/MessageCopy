using HarmonyLib;
using NeosModLoader;
using System;
using System.Reflection;
using FrooxEngine;
using BaseX;
using FrooxEngine.UIX;
using System.Collections.Generic;
using CloudX.Shared;

namespace MessageCopy
{
    public class MessageCopy : NeosMod
    {
        public override string Name => "MessageCopy";
        public override string Author => "kka429";
        public override string Version => "2.0.0";
        public override string Link => "https://github.com/rassi0429/MessageCopy"; // this line is optional and can be omitted

        [AutoRegisterConfigKey]
        public static ModConfigurationKey<bool> KEY_TIMESTAMPS = new("always_timestamp", "If true timestamps will be displayed on every message.", () => true);
        internal static ModConfiguration config;

        public override void OnEngineInit()
        {
            config = GetConfiguration();
            var harmony = new Harmony("dev.kokoa.messagecopy");
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(FriendsDialog))]
        [HarmonyPatch("AddMessage")]
        class MessageCopyPatch
        {
            static void Postfix(UIBuilder ___messagesUi, Image __result, Message message)
            {
                string toCopy;
                switch (message.MessageType)
                {
                    case CloudX.Shared.MessageType.Text:
                        toCopy = message.Content; break;
                    case CloudX.Shared.MessageType.Sound:
                    case CloudX.Shared.MessageType.Object:
                        toCopy = message.ExtractContent<CloudX.Shared.Record>().AssetURI; break;
                    case CloudX.Shared.MessageType.SessionInvite:
                        // should this copy session id or first uri?
                        toCopy = message.ExtractContent<SessionInfo>().SessionId; break;
                    default:
                        // CreditTransfer and SugarCubes cannot be copied (what would you even copy)
                        return;
                        
                }
                Slot cpyRoot;
                if (___messagesUi.Current[0][0].Name == "Panel") cpyRoot = ___messagesUi.Current[0][0][0][0].AddSlot("CopyBtn");
                else cpyRoot = ___messagesUi.Current[0].AddSlot("CopyBtn");
                var rect = cpyRoot.AttachComponent<RectTransform>();
                rect.AnchorMin.Value = new float2(1, 1);
                rect.OffsetMin.Value = new float2(-12, -12);
                cpyRoot.AttachComponent<Image>().Tint.Value = color.White.SetA(0.75f);
                cpyRoot.AttachComponent<Button>().LocalPressed += (btn, data) => { cpyRoot.Engine.InputInterface.Clipboard.SetText(toCopy); };
            }
        }

        // works a lot better with thisa
        [HarmonyPatch(typeof(FriendsDialog))]
        [HarmonyPatch("ShouldAddTimestamp")]
        class TimestampPatch
        {
            public static bool Prefix(ref bool __result)
            {
                if (!config.GetValue(KEY_TIMESTAMPS)) return true;
                __result = true;
                return false;
            }
        }
    }
}