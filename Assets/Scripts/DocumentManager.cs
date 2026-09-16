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
        if (profile == null) return;

        // 1. Assign Name, Planet/Species
        if (nameText != null) nameText.text = $"NAME: {profile.alienName}";
        if (speciesText != null) speciesText.text = $"SPECIES: {profile.race}";

        // 2. Generate procedural ID & Entry Code
        string generatedID = $"#{Random.Range(100, 999)}-{Random.Range(1000, 9999)}";
        string generatedCode = $"EC-{Random.Range(10, 99)}{(char)Random.Range('A', 'Z')}";

        if (idNumText != null) idNumText.text = $"ID: {generatedID}";
        if (entryCodeText != null) entryCodeText.text = $"ENTRY CODE: {generatedCode}";

        // 3. Set Clearance & Document Declaration
        if (purposeText != null)
        {
            purposeText.text = GetClearanceDeclaration(profile);
        }

        if (expirationText != null) expirationText.text = "EXPIRES: SHIFT END";

        // 4. Assign Alien Portrait using safe bitwise hash masking
        if (alienPhoto != null && defaultAlienPortraits != null && defaultAlienPortraits.Length > 0)
        {
            int hash = !string.IsNullOrEmpty(profile.alienName) ? profile.alienName.GetHashCode() : 0;
            int spriteIndex = (hash & 0x7FFFFFFF) % defaultAlienPortraits.Length;
            alienPhoto.sprite = defaultAlienPortraits[spriteIndex];
        }
    }

    private string GetClearanceDeclaration(AlienProfile profile)
    {
        switch (profile.cargoThreat)
        {
            case ThreatCategory.Safe:
                return "CLEARANCE: CLASS-A (DECLARED SAFE)";

            case ThreatCategory.Hazard:
                // Smuggler chance: 50% forge Class-A safe docs, 50% declare Class-C hazard
                bool isSmuggler = ((profile.alienName.GetHashCode() & 1) == 0);
                return isSmuggler ? "CLEARANCE: CLASS-A (DECLARED SAFE)" : "CLEARANCE: CLASS-C (HAZARD)";

            case ThreatCategory.Unknown:
                return "CLEARANCE: CLASS-B (SCAN REQUIRED)";

            default:
                return "CLEARANCE: UNKNOWN";
        }
    }
}