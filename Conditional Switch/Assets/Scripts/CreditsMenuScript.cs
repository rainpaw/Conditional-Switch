using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CreditsMenuScript : MonoBehaviour
{
    public TextMeshProUGUI creditsText;
    public TextAsset creditsAsset;

    private void Start()
    {
        creditsText.text = creditsAsset.text;
    }
}
