using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class TutorialHint : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TextMeshProUGUI tutorialText;

    private bool hasLookedAround = false;
    private bool hasMoved = false;

    private bool headThrowTutorialActive = false;
    private bool hasThrownHead = false;

    private bool jumpTutorialActive = false;
    private bool hasJumped = false;

    private bool headReturnTutorialActive = false;
    private bool hasReturnedHead = false;

    private void Start()
    {
        ShowText(
            "Maus bewegen: Umsehen\n" +
            "WASD: Bewegen"
        );
    }

    private void Update()
    {
        CheckStartTutorial();
        CheckHeadThrowTutorial();
        CheckJumpTutorial();
        CheckHeadReturnTutorial();
    }

    private void CheckStartTutorial()
    {
        if (hasLookedAround && hasMoved)
            return;

        if (Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            if (mouseDelta.magnitude > 0.1f)
            {
                hasLookedAround = true;
            }
        }

        if (Keyboard.current != null)
        {
            if (
                Keyboard.current.wKey.wasPressedThisFrame ||
                Keyboard.current.aKey.wasPressedThisFrame ||
                Keyboard.current.sKey.wasPressedThisFrame ||
                Keyboard.current.dKey.wasPressedThisFrame
            )
            {
                hasMoved = true;
            }
        }

        if (hasLookedAround && hasMoved)
        {
            HideText();
        }
    }

    private void CheckHeadThrowTutorial()
    {
        if (!headThrowTutorialActive || hasThrownHead)
            return;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            hasThrownHead = true;
            headThrowTutorialActive = false;

            ShowJumpTutorial();
        }
    }

    private void CheckJumpTutorial()
    {
        if (!jumpTutorialActive || hasJumped)
            return;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            hasJumped = true;
            jumpTutorialActive = false;

            ShowHeadReturnTutorial();
        }
    }

    private void CheckHeadReturnTutorial()
    {
        if (!headReturnTutorialActive || hasReturnedHead)
            return;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            hasReturnedHead = true;
            headReturnTutorialActive = false;

            HideText();
        }
    }

    public void ShowHeadThrowTutorial()
    {
        if (hasThrownHead)
            return;

        headThrowTutorialActive = true;
        jumpTutorialActive = false;
        headReturnTutorialActive = false;

        ShowText("F: Kopf werfen");
    }

    private void ShowJumpTutorial()
    {
        jumpTutorialActive = true;
        headReturnTutorialActive = false;

        ShowText("Leertaste: Springen");
    }

    private void ShowHeadReturnTutorial()
    {
        headReturnTutorialActive = true;

        ShowText("F: Kopf zurückholen");
    }

    private void ShowText(string text)
    {
        tutorialPanel.SetActive(true);
        tutorialText.text = text;
    }

    private void HideText()
    {
        tutorialPanel.SetActive(false);
    }
}