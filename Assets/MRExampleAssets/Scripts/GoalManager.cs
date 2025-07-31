using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LazyFollow = UnityEngine.XR.Interaction.Toolkit.UI.LazyFollow;

public struct Goal
{
    public GoalManager.OnboardingGoals CurrentGoal;
    public bool Completed;

    public Goal(GoalManager.OnboardingGoals goal)
    {
        CurrentGoal = goal;
        Completed = false;
    }
}

public class GoalManager : MonoBehaviour
{
    public enum OnboardingGoals
    {
        Info1,
        Info2,
        Info3,
        EnterCode // Last page with code input
    }

    Queue<Goal> m_OnboardingGoals;
    Goal m_CurrentGoal;
    bool m_AllGoalsFinished;
    int m_CurrentGoalIndex = 0;

    [Serializable]
    class Step
    {
        [SerializeField] public GameObject stepObject;
        [SerializeField] public string buttonText;
        public bool includeSkipButton;
    }

    [Header("Step List")]
    [SerializeField] List<Step> m_StepList = new List<Step>();
    [SerializeField] public TextMeshProUGUI m_StepButtonTextField;

    [Header("Buttons")]
    [SerializeField] public GameObject m_ContinueButton;
    [SerializeField] public GameObject m_SkipButton;
    [SerializeField] public GameObject m_DoneButton;

    [Header("User code input (last step)")]
    [SerializeField] TMP_InputField userCodeInput;
    [SerializeField] TMP_Text errorText;
    public static string UserCode = "";

    [SerializeField] GameObject m_CoachingUIParent;
    [SerializeField] LazyFollow m_GoalPanelLazyFollow;

    // ---- INIT ----
    void Start()
    {
        InitGoals();
        ShowCurrentStep();
    }

    // ---- MAIN ----
    void Update() { /* Optionally add logic here */ }

    // ---- GOAL/STEP MANAGEMENT ----

    void InitGoals()
    {
        m_OnboardingGoals = new Queue<Goal>();
        m_OnboardingGoals.Enqueue(new Goal(OnboardingGoals.Info1));
        m_OnboardingGoals.Enqueue(new Goal(OnboardingGoals.Info2));
        m_OnboardingGoals.Enqueue(new Goal(OnboardingGoals.Info3));
        m_OnboardingGoals.Enqueue(new Goal(OnboardingGoals.EnterCode));

        m_CurrentGoalIndex = 0;
        m_CurrentGoal = m_OnboardingGoals.Dequeue();

        if (errorText != null) errorText.text = "";
        if (userCodeInput != null) userCodeInput.text = "";
    }

    void ShowCurrentStep()
    {
        // Disattiva tutte le card
        for (int i = 0; i < m_StepList.Count; i++)
            m_StepList[i].stepObject.SetActive(i == m_CurrentGoalIndex);

        // Aggiorna testo bottone e visibilità skip
        if (m_StepButtonTextField != null)
            m_StepButtonTextField.text = m_StepList[m_CurrentGoalIndex].buttonText;

        // Bottone logica: SOLO l'ultimo step (EnterCode) mostra Done, gli altri Continue/Skip
        if (m_CurrentGoal.CurrentGoal == OnboardingGoals.EnterCode)
        {
            m_ContinueButton.SetActive(false);
            m_SkipButton.SetActive(false);
            m_DoneButton.SetActive(true);
        }
        else
        {
            m_ContinueButton.SetActive(true);
            m_SkipButton.SetActive(m_StepList[m_CurrentGoalIndex].includeSkipButton);
            m_DoneButton.SetActive(false);
        }
    }

    public void OnContinueClicked()
    {
        CompleteGoal();
    }

    public void OnSkipClicked()
    {
        // Salta direttamente all'ultimo step (EnterCode)
        m_CurrentGoalIndex = m_StepList.Count - 1;
        m_CurrentGoal = new Goal(OnboardingGoals.EnterCode);
        ShowCurrentStep();
    }

    void CompleteGoal()
    {
        m_CurrentGoal.Completed = true;
        m_CurrentGoalIndex++;

        if (m_CurrentGoalIndex >= m_StepList.Count)
            return;

        if (m_OnboardingGoals.Count > 0)
        {
            m_CurrentGoal = m_OnboardingGoals.Dequeue();
            ShowCurrentStep();
        }
        else
        {
            m_AllGoalsFinished = true;
            if (m_CoachingUIParent != null)
                m_CoachingUIParent.transform.localScale = Vector3.zero;
        }
    }

    // ---- CODE INPUT LOGIC ----

    public void OnUserCodeEndEdit(string value)
    {
        // Optional: Validate as user types
        if (errorText == null) return;
        if (!string.IsNullOrEmpty(value))
            errorText.text = "";
        else
            errorText.text = "Please enter your code.";
    }

    public void OnDoneClicked()
    {
        string code = userCodeInput != null ? userCodeInput.text.Trim() : "";

        if (!string.IsNullOrEmpty(code))
        {
            UserCode = code;
            PlayerPrefs.SetString("UserCode", code);

            if (userCodeInput != null)
                userCodeInput.DeactivateInputField();
            if (userCodeInput != null)
                userCodeInput.gameObject.SetActive(false);

            // AGGIORNA SUBITO IL CAMPO USER (TMP) SULLA MANO SINISTRA
            var qoe = FindObjectOfType<QoESliderManager>();
            if (qoe != null)
                qoe.UpdateUserCodeInfo(code);

            if (m_CoachingUIParent != null)
                m_CoachingUIParent.transform.localScale = Vector3.zero;

            m_AllGoalsFinished = true;
            Debug.Log($"[GoalManager] Codice salvato: {code}");
        }
        else
        {
            if (errorText != null)
                errorText.text = "Please enter your code.";
        }
    }












    // ---- UTILITY ----

    public void ResetCoaching()
    {
        InitGoals();
        ShowCurrentStep();
        m_AllGoalsFinished = false;
        if (m_CoachingUIParent != null)
            m_CoachingUIParent.transform.localScale = Vector3.one;
    }
}
