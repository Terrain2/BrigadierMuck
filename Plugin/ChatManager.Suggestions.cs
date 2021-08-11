using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Brigadier.StaticUtils;

namespace Brigadier.Plugin
{
    partial class ChatManager
    {
        Suggestions suggestions = null;
        string suggestionText = "";
        int topSuggestion = 0;
        int selectedSuggestion = 0;

        TextMeshProUGUI suggestionsTextObj;
        TextMeshProUGUI placeholderTextObj;
        RectTransform suggestionsBackground;

        void SuggestionsAwake()
        {
            // add suggestionsTextObj as clone of __instance.inputField.textComponent
            // but reorder hierarchy to ensure it is not clipped by any RectMask2D
            var chatbox = ChatBox.Instance.transform.GetChild(0);
            var suggestionsTextParent = Instantiate(inputField.textViewport, transform, true);

            suggestionsTextObj = suggestionsTextParent.GetChild(1).GetComponent<TextMeshProUGUI>();
            Destroy(suggestionsTextParent.GetChild(2).gameObject);
            Destroy(suggestionsTextParent.GetChild(0).gameObject);
            placeholderTextObj = Instantiate(suggestionsTextObj, suggestionsTextParent, true);

            suggestionsTextObj.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            suggestionsTextObj.verticalAlignment = VerticalAlignmentOptions.Bottom;

            placeholderTextObj.color = Colors.Invisible;
            placeholderTextObj.enabled = true;
            placeholderTextObj.text = "";

            var background = new GameObject("Suggestions Background Fill");
            suggestionsBackground = background.AddComponent<RectTransform>() ?? (RectTransform)background.transform;
            var img = background.AddComponent<RawImage>();
            img.color = new Color(0, 0, 0, 0.8f);
            suggestionsBackground.SetParent(suggestionsTextParent, false);
            suggestionsBackground.SetSiblingIndex(0);
            suggestionsBackground.anchorMin = new(0, 0);
            suggestionsBackground.anchorMax = new(0, 0);
            suggestionsBackground.pivot = new(0, 0);
            suggestionsBackground.localPosition = new(0, 0, 0);
            suggestionsBackground.sizeDelta = new(0, 0);

            Destroy(suggestionsTextParent.GetComponent<RectMask2D>());
            Destroy(chatbox.GetComponent<RectMask2D>());

            var o = new GameObject("Messages Rect Mask");
            o.AddComponent<RectMask2D>();

            var ot = o.AddComponent<RectTransform>() ?? (RectTransform)o.transform;
            ot.SetParent(chatbox, false);
            ot.SetSiblingIndex(0);

            var messages = ChatBox.Instance.messages.rectTransform;
            ot.sizeDelta = messages.sizeDelta;
            ot.anchorMin = messages.anchorMin;
            ot.anchorMax = messages.anchorMax;
            ot.pivot = messages.pivot;
            ot.anchoredPosition = messages.anchoredPosition;
            messages.SetParent(ot, false);

            var ol = o.AddComponent<VerticalLayoutGroup>();
            ol.childControlWidth = true;
            ol.childControlHeight = false;
            ol.childScaleWidth = true;
            ol.childScaleHeight = true;
            ol.childAlignment = TextAnchor.UpperLeft;


            inputField.textComponent.textPreprocessor = new SuggestionPreprocessor();
        }

        class SuggestionPreprocessor : ITextPreprocessor
        {
            public string PreprocessText(string text) => text[..^1] + Instance.suggestionText + "\u200B";
        }

        void SelectSuggestion()
        {
            var suggestion = suggestions.List[selectedSuggestion];
            var endOfInput = inputField.text[suggestion.Range.Start..];
            if (suggestions?.List.Any() == true)
            {
                placeholderTextObj.text = inputField.text[..suggestions.Range.Start];
                asyncQueue.Add(ShowSuggestions);
            }
            if (suggestion.Text.StartsWith(endOfInput))
            {
                suggestionText = suggestion.Text[endOfInput.Length..];
            }
            else
            {
                suggestionText = "";
            }

            inputField.textComponent.ForceMeshUpdate(forceTextReparsing: true);
        }

        void ShowSuggestions()
        {
            if (suggestions == null)
            {
                suggestionsTextObj.text = "";
                return;
            }
            var visibleSuggestions = new StringBuilder();
            var width = placeholderTextObj.renderedWidth;
            var textInfo = placeholderTextObj.textInfo;
            for (var i = placeholderTextObj.textInfo.characterCount; i > 0; i--)
            {
                var charInfo = textInfo.characterInfo[i - 1];
                if (!char.IsWhiteSpace(charInfo.character)) break;
                width += Math.Abs(charInfo.bottomLeft.x - charInfo.bottomRight.x);
            }
            visibleSuggestions.Append($"<margin-left={width}px>");
            for (var i = 0; i < 10 && (topSuggestion + i) < suggestions.List.Count; i++)
            {
                var idx = topSuggestion + i;
                visibleSuggestions.AppendLine((idx == selectedSuggestion ? ChatColors.Suggestion : ChatColors.Gray) + suggestions.List[idx].Text + ChatColors.End);
            }

            visibleSuggestions.Append($"<line-height=80%>\n{ChatColors.Invisible}.{ChatColors.End}");

            suggestionsTextObj.text = visibleSuggestions.ToString();
            asyncQueue.Add(ShowSuggestionsBackground);
        }

        void ShowSuggestionsBackground()
        {
            var width = placeholderTextObj.renderedWidth;
            var textInfo = placeholderTextObj.textInfo;
            for (var i = placeholderTextObj.textInfo.characterCount; i > 0; i--)
            {
                var charInfo = textInfo.characterInfo[i - 1];
                if (!char.IsWhiteSpace(charInfo.character)) break;
                width += Math.Abs(charInfo.bottomLeft.x - charInfo.bottomRight.x);
            }
            var size = suggestionsTextObj.textBounds.size;
            size.y -= placeholderTextObj.fontSize * 1.8f;
            suggestionsBackground.sizeDelta = size;
            suggestionsBackground.anchoredPosition = new(width, placeholderTextObj.fontSize * 1.8f);
        }

        async void Suggestions(ParseResults parse, int cursor)
        {
            if (cursor == 0) return; // if before slash, cannot get suggestions
            try
            {
                suggestions = await Dispatcher.GetCompletionSuggestions(parse, cursor);
                if (suggestions.List.Any())
                {
                    asyncQueue.Add(SelectSuggestion);
                }
                else
                {
                    suggestions = null;
                    suggestionsTextObj.text = "";
                    suggestionsBackground.sizeDelta = new(0, 0);
                }
            }
            catch (Exception ex) // async without await exception is not caught and logged automatically
            {
                Debug.LogException(ex);
            }
        }
    }
}