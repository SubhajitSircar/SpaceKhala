using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DocumentManager : MonoBehaviour
{
    [Header("UI Text Components")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI speciesText;
    [SerializeField] private TextMeshProUGUI idNumText;
    [SerializeField] private TextMeshProUGUI entryCodeText;
    [SerializeField] private TextMeshProUGUI purposeText;
    [SerializeField] private TextMeshProUGUI expirationText;

    [Header("UI Image Component")]
    [SerializeField] private Image alienPhoto;

    [Header("Default Sprites Pool (Optional)")]
    [SerializeField] private Sprite[] defaultAlienPortraits;

    public void PopulateDocument(AlienProfile profile)
    {
        // 1. Assign Name, Planet/Species
        if (nameText != null) nameText.text = $"NAME: {profile.alienName}";
        if (speciesText != null) speciesText.text = $"SPECIES: {profile.race}";

        // 2. Generate procedural ID & Entry Code
        string generatedID = $"#{Random.Range(100, 999)}-{Random.Range(1000, 9999)}";
        string generatedCode = $"EC-{Random.Range(10, 99)}{(char)Random.Range('A', 'Z')}";

        if (idNumText != null) idNumText.text = $"ID: {generatedID}";
        if (entryCodeText != null) entryCodeText.text = $"ENTRY CODE: {generatedCode}";
        if (purposeText != null) purposeText.text = "PURPOSE: TRANSIT";
        if (expirationText != null) expirationText.text = "EXPIRES: SHIFT END";

        // 3. Assign Alien Portrait
        if (alienPhoto != null && defaultAlienPortraits != null && defaultAlienPortraits.Length > 0)
        {
            int spriteIndex = Mathf.Abs(profile.alienName.GetHashCode()) % defaultAlienPortraits.Length;
            alienPhoto.sprite = defaultAlienPortraits[spriteIndex];
        }
    }
}