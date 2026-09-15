using UnityEngine;

public enum ThreatCategory { Safe, Hazard, Unknown }

[System.Serializable]
public class AlienProfile
{
    public string alienName;
    public string originPlanet;
    public string race;
    public string dialogue;
    public string cargoItem;
    public ThreatCategory cargoThreat;
    public float targetFrequency;
    public float targetPhase;
}

public class AlienDataGenerator : MonoBehaviour
{
    private static AlienDataGenerator _instance;
    public static AlienDataGenerator Instance
    {
        get;
        private set;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    [Header("Alien Identity Pools")]
    private string[] alienNames = { "XORGLON", "ZETA-9", "BLARK", "MALAKOR", "VEX-T", "GORATH", "KLYNNE", "NIMBUS", "OZA-OZA", "RROTH" };
    private string[] planets = { "GLIESE 667", "KEPLER-186F", "ZETA RETICULI", "AQUILA-9", "TITAN OUTPOST", "NEBULA PRIME", "VALKYRIE-4" };
    private string[] races = { "SYLPHID", "CHITINITE", "CYBORG HYBRID", "PLASMA FLUID", "VOID WALKER", "INSECTOID", "MOLLUSKAN" };

    [Header("Cargo Pools by Category")]
    private string[] safeCargos = { "PET STORE ANIMALS", "HYDRO-FARM WHEAT", "MEDICAL SUPPLIES", "HOLIDAY GIFTS", "TEXTBOOKS", "DRINKING WATER", "PIXEL ART PRINTS" };
    private string[] hazardCargos = { "XENOMORPH EGGS", "UNSTABLE ANTIMATTER", "ILLEGAL WEAPONS", "DARK MATTER BOMBS", "TOXIC WASTE", "CONTRABAND DRUGS" };
    private string[] unknownCargos = { "POACHED EXOTIC BEASTS", "ANCIENT RELICS", "EXPERIMENTAL AI CORES", "CYBERNETIC IMPLANTS", "UNREGISTERED BIOMASS", "FOSSILIZED EGGS" };

    [Header("Dialogue Templates")]
    private string[] friendlyIntros = {
        "GREETINGS OFFICER! WE ARE CARRYING {CARGO}. REQUESTING CLEARANCE.",
        "HELLO FRIEND. MY SHIP IS LOADED WITH {CARGO}. DECODING TRANSMISSION NOW.",
        "GOOD DAY! JUST A SIMPLE TRANSPORT OF {CARGO}. PLEASE OPEN THE GATE."
    };

    private string[] neutralIntros = {
        "IDENTIFICATION VERIFIED. SHIP CARGO IS {CARGO}. AWAITING GATE DECISION.",
        "TRANSMITTING MANIFEST. WE CARRY {CARGO}. DO NOT KEEP US WAITING.",
        "THIS IS SHIP {NAME}. OUR HOLD CONTAINS {CARGO}. OPEN GATE."
    };

    private string[] rudeBanterIntros = {
        "MOVE IT SLOW-BRAIN. I'M HAULING {CARGO} AND TIME IS CREDITS.",
        "WHY DO WE STILL DEAL WITH FLESH-MIND GUARDS? CARGO IS {CARGO}. LET ME IN.",
        "DONT LOOK AT ME LIKE THAT, TWO-LEGS. IT IS JUST {CARGO}. OPEN UP."
    };

    public AlienProfile GenerateNewProfile()
    {
        AlienProfile profile = new AlienProfile();

        profile.alienName = alienNames[Random.Range(0, alienNames.Length)];
        profile.originPlanet = planets[Random.Range(0, planets.Length)];
        profile.race = races[Random.Range(0, races.Length)];

        int categoryIndex = Random.Range(0, 3);
        profile.cargoThreat = (ThreatCategory)categoryIndex;

        switch (profile.cargoThreat)
        {
            case ThreatCategory.Safe:
                profile.cargoItem = safeCargos[Random.Range(0, safeCargos.Length)];
                break;
            case ThreatCategory.Hazard:
                profile.cargoItem = hazardCargos[Random.Range(0, hazardCargos.Length)];
                break;
            case ThreatCategory.Unknown:
                profile.cargoItem = unknownCargos[Random.Range(0, unknownCargos.Length)];
                break;
        }

        int attitude = Random.Range(0, 3);
        string selectedTemplate = "";
        switch (attitude)
        {
            case 0: selectedTemplate = friendlyIntros[Random.Range(0, friendlyIntros.Length)]; break;
            case 1: selectedTemplate = neutralIntros[Random.Range(0, neutralIntros.Length)]; break;
            case 2: selectedTemplate = rudeBanterIntros[Random.Range(0, rudeBanterIntros.Length)]; break;
        }

        profile.dialogue = selectedTemplate.Replace("{CARGO}", $"[{profile.cargoItem}]").Replace("{NAME}", profile.alienName);
        profile.targetFrequency = Random.Range(0.1f, 0.9f);
        profile.targetPhase = Random.Range(0.1f, 0.9f);

        return profile;
    }
}