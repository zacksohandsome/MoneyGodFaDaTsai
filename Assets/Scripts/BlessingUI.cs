using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlessingUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI blessingText;

    public void ShowBlessing(string blessing)
    {
        Debug.Log($"Εγ₯ά―¬ΊΦ: {blessing}");
        blessingText.text = ConvertToVertical(blessing);
        gameObject.SetActive(true);
    }

    public void HideBlessing()
    {
        gameObject.SetActive(false);
    }

    private string ConvertToVertical(string input)
    {
        return string.Join("\n", input.ToCharArray());
    }
}
