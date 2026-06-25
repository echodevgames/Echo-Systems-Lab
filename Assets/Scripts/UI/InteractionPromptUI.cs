//----- InteractionPromptUI.cs -----


using TMPro;
using UnityEngine;

public class InteractionPromptUI : MonoBehaviour
{
    [SerializeField] private GameObject promptRoot;
    [SerializeField] private TMP_Text promptText;

    private void Awake()
    {
        Hide();
    }

    public void Show(string text)
    {
        if (promptRoot != null)
            promptRoot.SetActive(true);

        if (promptText != null)
            promptText.text = text;
    }

    public void Hide()
    {
        if (promptRoot != null)
            promptRoot.SetActive(false);
    }
}
//----- InteractionPromptUI.cs END -----

