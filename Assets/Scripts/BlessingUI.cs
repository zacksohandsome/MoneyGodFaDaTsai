using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlessingUI : MonoBehaviour
{
    [SerializeField] private Text blessingText;

    public void ShowBlessing(string blessing)
    {
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
