using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpinCore.UI
{
    public static class ModalMessageDialogExtensions
    {
        private static CustomGroup _modalGroup;

        internal static CustomGroup ModalMessageDialogCustomGroup
        {
            get
            {
                if (_modalGroup == null)
                {
                    _modalGroup = UIHelper.CreateGroup(ModalMessageDialog.Instance.transform.Find("Container/Body"), "SpinCoreModalExtension");
                    _modalGroup.Transform.SetSiblingIndex(5);
                }

                return _modalGroup;
            }
        }

        public static ModalMessageDialog.ModalMessage CreateYesNo()
        {
            return new ModalMessageDialog.ModalMessage
            {
                affirmativeText = new TranslationReference("UI_Yes", false),
                cancelText = new TranslationReference("UI_No", false),
            };
        }

        public static void Open(this ModalMessageDialog.ModalMessage msg)
        {
            ModalMessageDialog.Instance.AddMessage(msg);
        }

        public static void OpenWithCustomUI(this ModalMessageDialog.ModalMessage msg, Action<Transform> onOpen)
        {
            msg.Open();
            onOpen?.Invoke(ModalMessageDialogCustomGroup.Transform);
        }

        internal static void ModalMessageDialogClosed()
        {
            _modalGroup?.Transform.RemoveAllChildren();
        }
    }
}
