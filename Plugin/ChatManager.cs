using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Brigadier.Plugin
{
    partial class ChatManager : MonoBehaviour
    {
        public static ChatManager Instance;
        public TMP_InputField inputField;
        protected void Awake()
        {
            inputField = GetComponent<TMP_InputField>();
            Instance = this;

            SuggestionsAwake();
            DispatchAwake();
        }

        readonly List<Action> asyncQueue = new();

        void Update()
        {
            var queue = asyncQueue.ToArray();
            asyncQueue.Clear();
            foreach (var action in queue)
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }
    }
}