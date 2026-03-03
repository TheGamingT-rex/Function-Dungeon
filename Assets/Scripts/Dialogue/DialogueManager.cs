using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Components;
using UnityEngine.Localization;
using static PlayerController;
using System.Linq;
using UnityEngine.Events; // added for LINQ usage

public class DialogueManager : MonoBehaviour {
    internal bool displayDialogueUI;

    [Header("UI")]
    [SerializeField] private Image characterSprite;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private float typingSpeed;

    public Queue<string> sentences;
    private Queue<Sprite> sprites;
    [NonSerialized] public UnityEvent nextSentence;
    [NonSerialized] public UnityEvent finishedTyping;

    private void Awake() {
        sentences = new Queue<string>();
        sprites = new Queue<Sprite>();
        nextSentence = new UnityEvent();
        finishedTyping = new UnityEvent();
    }

    public void AddDialogue(Dialogue dialogue)
    {
        foreach (var answer in dialogue.content)
        {
            sprites.Enqueue(answer.sprite);

            //Debug.Log("Using localization override for " + dialogue.content[i].localizationKey.GetLocalizedString() + " + " + dialogue.content[i].localizationOverride);
            sentences.Enqueue(answer.localizationOverride.Length != 0
                ? answer.localizationOverride
                : answer.localizationKey.GetLocalizedString());
        }
    }
    
    //Insert a late dialogue at the front of the queue, without interrupting the current dialogue if one is active. Used for feedback dialogues that should appear immediately after a question is answered, but without cutting off the current dialogue if it's still being displayed.
    public void PriorityDialogue(Dialogue dialogue) {
        if (displayDialogueUI) return;
        var tempSentences = sentences.ToArray();
        var tempSprites = sprites.ToArray();
        sentences.Clear();
        sprites.Clear();
        AddDialogue(dialogue);
        foreach (var sentence in tempSentences) sentences.Enqueue(sentence);
        foreach (var sprite in tempSprites) sprites.Enqueue(sprite);
    }

    // Helper: replaces placeholders in feedback/localization strings with wrong answers,
    // ensuring the inserted wrong answer is not the player's previously chosen answer (Globals.MathManager.wrongAnsw)
    private string ReplaceWrongPlaceholders(string text, Question question) {
        if (string.IsNullOrEmpty(text) || question == null) return text;

        // Gather localized wrong answers
        var wrongKeys = new List<string>() { question.GetWrong1LocalizationKey(), question.GetWrong2LocalizationKey(), question.GetWrong3LocalizationKey() };
        var wrongs = wrongKeys.Select(k => LocalizationManager.Localize(k, LocalizationTable.QUESTIONS)).ToList();
        string playerWrong = Globals.MathManager != null ? Globals.MathManager.wrongAnsw : "";

        // Helper to pick a replacement for an index; if the default equals player's chosen answer, pick another different one
        System.Func<int, string> pickReplacement = (idx) => {
            string candidate = wrongs[idx];
            if (!string.IsNullOrEmpty(playerWrong) && candidate == playerWrong) {
                for (int j = 0; j < wrongs.Count; j++) {
                    if (j == idx) continue;
                    if (wrongs[j] != playerWrong) return wrongs[j];
                }
            }
            return candidate;
        };

        text = text.Replace("[WRONG1]", pickReplacement(0));
        text = text.Replace("[WRONG2]", pickReplacement(1));
        text = text.Replace("[WRONG3]", pickReplacement(2));

        // [WRONG] -> first wrong that isn't the player's, fallback to first
        string firstNonPlayer = wrongs.FirstOrDefault(w => w != playerWrong) ?? wrongs[0];
        text = text.Replace("[WRONG]", firstNonPlayer);

        return text;
    }

    public void StartCustomDialogue(Sprite[] customSprites, string[] customSentences) {
        //if (displayDialogueUI) return;
        sentences.Clear();
        sprites.Clear();
        displayDialogueUI = true;

        foreach (Sprite sprite in customSprites) sprites.Enqueue(sprite);
        foreach (string sentence in customSentences) sentences.Enqueue(sentence);

        if (sentences.Count == customSentences.Length) {
            DisplayNextSentence();
        }
    }

    public void StartDialogue(Dialogue dialogue) {
        if (displayDialogueUI) return;
        sentences.Clear();
        sprites.Clear();
        displayDialogueUI = true;
        AddDialogue(dialogue);

        if (sentences.Count == dialogue.content.Length) {
            DisplayNextSentence();
        }
    }

    public void DisplayNextSentence() {
        if (sentences.Count == 0) {
            EndDialogue();
            return;
        }
        nextSentence.Invoke();
        string sentence = sentences.Dequeue();
        Sprite sprite = sprites.Dequeue();
        if (sprite != null) {
            characterSprite.sprite = sprite;
        }

        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }

    IEnumerator TypeSentence(string sentence) {
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray()) {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        finishedTyping.Invoke();
    }

    public void EndDialogue() {
        displayDialogueUI = false;
        if (Globals.MathManager.feedback) return;
        Globals.PlayerController.state = PlayerState.Idle;
    }
}
