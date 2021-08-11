using System.Collections.Generic;
using UnityEngine;

namespace Brigadier.Plugin
{
    partial class ChatManager
    {
        public readonly List<string> history = new();
        int selectedMessage = -1;
        public void Move(int delta)
        {
            if (suggestions != null)
            {
                var currentSelection = selectedSuggestion;
                selectedSuggestion = Mathf.Clamp(selectedSuggestion - delta, 0, suggestions.List.Count - 1);
                if (selectedSuggestion != currentSelection)
                {
                    var diff = selectedSuggestion - topSuggestion;
                    if (diff >= 10) topSuggestion += diff - 9;
                    else if (diff < 0) topSuggestion += diff;
                    SelectSuggestion();
                }
            }
            else
            {
                var currentSelection = selectedMessage;

                if (selectedMessage >= 0 && inputField.text != history[selectedMessage]) selectedMessage = -1;

                selectedMessage = Mathf.Clamp(selectedMessage + delta, -1, history.Count - 1);
                if (currentSelection != selectedMessage)
                {
                    charColors = null;
                    inputField.text = selectedMessage >= 0 ? history[selectedMessage] : "";
                    inputField.stringPosition = inputField.text.Length;
                }
            }
        }

        public void UserInput()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (ChatBox.Instance.typing)
                {
                    charColors = null;
                    suggestions = null;
                    selectedSuggestion = 0;
                    selectedMessage = -1;
                    topSuggestion = 0;
                    suggestionText = "";
                    suggestionsTextObj.text = "";
                    suggestionsBackground.sizeDelta = new(0, 0);
                    lastParse = null;
                    ChatBox.Instance.SendMessage(inputField.text);
                }
                else
                {
                    ChatBox.Instance.ShowChat();
                    ChatBox.Instance.typing = true;
                    inputField.interactable = true;
                    inputField.Select();
                }
            }

            if (ChatBox.Instance.typing && !inputField.isFocused)
            {
                inputField.Select();
            }

            if (ChatBox.Instance.typing)
            {
                if (cursor != inputField.stringPosition && lastParse != null)
                {
                    cursor = inputField.stringPosition;
                    Suggestions(lastParse, cursor);
                    return;
                }
                if (suggestions != null)
                {

                    if (Input.GetKeyDown(KeyCode.Tab))
                    {
                        var suggestion = suggestions.List[selectedSuggestion];
                        selectedSuggestion = 0;
                        topSuggestion = 0;
                        suggestionText = "";
                        charColors = null;
                        inputField.text = suggestion.Apply(inputField.text);
                        inputField.stringPosition = suggestion.Range.Start.Value + suggestion.Text.Length;
                    }

                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        suggestions = null;
                        suggestionText = "";
                        selectedSuggestion = 0;
                        topSuggestion = 0;
                    }
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        ChatBox.Instance.ClearMessage();
                    }
                }
            }
        }
    }
}