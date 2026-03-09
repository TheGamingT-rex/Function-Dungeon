using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;

public class FailRoomIntroduction : MonoBehaviour {
    public Transform spawnPos;
    [SerializeField] private Dialogue answerDialogue;
    [SerializeField] private IntroductionShowFeedback showFeedback;

    public void Event()
    {
        Globals.MathManager.inFailRoom = true;
        Globals.MathManager.displayExerciseUI = false;
        
        var list = Globals.MathManager.answers;
        List<string> answers = list.Select(question => question.text).ToList();
        var playerAnswer = Globals.MathManager.wrongAnsw;        // the player's selected (wrong) answer string
        var correct = LocalizationManager.Localize(
            Globals.MathManager.activeQuestion.GetCorrectLocalizationKey(),
            LocalizationTable.QUESTIONS
        );

        string npcGivenAnswer = null;

        if (answers.Count > 0)
        {
            // pick candidates that are neither the player's chosen answer nor the correct answer
            var candidates = answers.Where(a => a != playerAnswer && a != correct).ToList();

            if (candidates.Count > 0)
            {
                npcGivenAnswer = "'" + candidates[Random.Range(0, candidates.Count)] + "'";
            }
            else
            {
                // fallback: pick any answer that isn't the correct one
                npcGivenAnswer = answers.FirstOrDefault(a => a != correct) ?? correct;
            }
        }

        if (answerDialogue != null)
        {
            var localizedString = answerDialogue.content[0].localizationKey.GetLocalizedString(npcGivenAnswer).Replace("{0}", npcGivenAnswer);
            answerDialogue.content[0].localizationOverride = localizedString;
        }

        Globals.DialogueManager.AddDialogue(answerDialogue);
        Globals.DialogueManager.nextSentence.AddListener(showFeedback.Feedback);
        Debug.Log($"NPC will say: {npcGivenAnswer}");
    }
    
    public IEnumerator WaitForFeedback()
    {
        bool pressed = false;
        while (!pressed)
        {
            if (Input.GetKeyDown(KeyCode.Space)) pressed = true;
            yield return new WaitForFixedUpdate();
        }
        
        Globals.MathManager.displayExerciseUI = false;
        Globals.MathManager.inFailRoom = false;
        Globals.DialogueManager.EndDialogue();
    }
}