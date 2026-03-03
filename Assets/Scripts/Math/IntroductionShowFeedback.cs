using UnityEngine;
using UnityEngine.UI;

public class IntroductionShowFeedback : MonoBehaviour
{
    public void Feedback() {
        if (Globals.MathManager.displayExerciseUI) return;
        Globals.MathManager.feedback = true;
        Globals.MathManager.displayExerciseUI = true;

        foreach (var answer in Globals.MathManager.answers)
        {
            GameObject btn = answer.transform.parent.gameObject;
            btn.SetActive(true);
            if (answer.text != LocalizationManager.Localize(Globals.MathManager.activeQuestion.GetCorrectLocalizationKey(), LocalizationTable.QUESTIONS))
            {
                var button = btn.GetComponent<Button>();
                button.enabled = true;
                button.onClick.AddListener(ShowFeedbackEvent);
                btn.GetComponent<Image>().color = Color.white;
            } else {
                //btn.GetComponent<Image>().color = Color.green;
                btn.GetComponent<Button>().onClick.AddListener(ShowFeedbackEvent);
            }

            if (answer.text.Equals(Globals.MathManager.wrongAnsw)) {
                btn.GetComponent<Image>().color = Color.red;
                btn.GetComponent<Button>().enabled = false;
            }
        }
        Globals.DialogueManager.nextSentence.RemoveListener(Feedback);
    }

    private void ShowFeedbackEvent()
    {
        Globals.MathManager.activeQuestion.dialogue.content[0].localizationOverride = LocalizationManager.Localize(Globals.MathManager.activeQuestion.GetFeedbackLocalizationKey(), LocalizationTable.QUESTIONS);
        Globals.DialogueManager.AddDialogue(Globals.MathManager.activeQuestion.dialogue);
        Globals.DialogueManager.DisplayNextSentence();
        StartCoroutine(FindObjectOfType<FailRoomIntroduction>().WaitForFeedback());
    }
}
