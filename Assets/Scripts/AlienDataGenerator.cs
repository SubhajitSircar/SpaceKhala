using UnityEngine;
using System.Collections.Generic;

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

    [Header("Voice & Audio Parameters")]
    public float voicePitch = 1.0f;
    public float typingSpeed = 30f;
}

public class AlienDataGenerator : MonoBehaviour
{
    private static AlienDataGenerator _instance;
    public static AlienDataGenerator Instance
    {
        get { return _instance; }
        private set { _instance = value; }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    [Header("Anti-Duplication Tracking")]
    [SerializeField] private int nameHistorySize = 25;
    [SerializeField] private int planetHistorySize = 20;
    [SerializeField] private int cargoHistorySize = 20;

    private readonly Queue<string> recentNames = new Queue<string>();
    private readonly Queue<string> recentPlanets = new Queue<string>();
    private readonly Queue<string> recentCargos = new Queue<string>();

    [Header("Identity Pools")]
    private string[] alienNames = {
        // Sci-Fi & Sci-Fi Trope Identities
        "XORGLON", "ZETA-9", "BLARK", "MALAKOR", "VEX-T", "GORATH", "KLYNNE", "NIMBUS", "OZA-OZA", "RROTH",
        "T'VELL", "KHAAN", "ZAP-B0T", "C'THALLO", "BLAX-7", "VROK", "KRANG-X", "ZOLTAN", "YZX-99", "QUARK",
        "GORT", "ARTHAS-VOID", "BLAZE-K", "NEBULA-BOY", "ZAX-PRIME", "KARR-THUN", "SKRAAG", "MORDUK", "XANTHUS", "THRAX",
        "VAZ-KUL", "OMNI-7", "GLITCH-90", "RAZOR-EYE", "BROXX", "T'KAR", "DREK", "VAZIR", "SARK", "PHANTOM-X",
        "CYBX-11", "KRONOS", "NEBULA-MAX", "T-800-ALPHA", "KIL-ROY", "NORAD-9", "XERXES", "BRAIN-8", "OMEGA-PRIME", "ZARKON",
        // Space Pop-Culture & Sci-Fi References
        "DECKARD", "SPIKE-SPIEGEL", "GENDO", "KAMINA-X", "CHAR-A", "BRIGHT-NOA", "LOKI-VOID", "GROGU-9", "HAL-9001", "RIPLEY-V",
        "ZAPHOD", "BOBA-F", "MANDO-88", "GARRUS-V", "LIARA-T", "WREX-K", "SHEPARD-N7", "CORTANA-PRIME", "MASTER-CHIEF", "SAMUS-ARAN",
        "ISAAC-CLARKE", "JOSHUA-2000", "BENDER-B", "LEELA-7", "FARNSWORTH", "PAUL-ATREIDES", "RIDDICK", "STAR-LORD", "ROCKET-RACCOON",
        "MERCER-X", "SULLY-4", "KAYLEE-FRYE", "MALCOLM-REYNOLDS", "THRAWN", "AHSOKA-T", "CAD-BANE", "JABBA-EXPRESS", "MANDO-KREED", "DARROW-AU"
    };

    private string[] planets = {
        // Astronomy & Deep Space
        "GLIESE 667", "KEPLER-186F", "ZETA RETICULI", "AQUILA-9", "TITAN OUTPOST", "NEBULA PRIME", "VALKYRIE-4", "CYGNUS-X1",
        "ANDROMEDA-CORE", "EUROPA-UNDER-ICE", "MARS-RED-CITY", "VENUS-CLOUD-CITY", "GANYMEDE-COLONY", "CALLISTO-BASE", "DEIMOS-MINES",
        "OORT-CLOUD-STATION", "POLARIS-OUTPOST", "RIGEL-KAPPA", "BETELGEUSE-V", "SIRIUS-MINOR", "PROXIMA-CENTAURI-B", "TRAPPIST-1E",
        "GLIESE-832C", "HD-189733B", "WASP-12B", "COROT-7B", "PROMETHEUS-SITE", "TARTARUS-6", "KRONOS-MINES", "XYLOS-4",
        // Sci-Fi Iconic Planets & Stations
        "TATOOINE-OUTSKIRTS", "CORUSCANT-LOWER", "VORMIR-7", "ARRAKIS-PRIME", "KRYPTON-REMNANT", "HALO-RING-04", "REACH-SURVIVOR",
        "LV-426", "NOSTROMO-STATION", "CYBERTRON-SECTOR-7", "RAXXLA", "EDEN-PRIME", "OMEGA-STATION", "BABYLON-5", "GALLIFREY-FALL",
        "VULCAN-PRIME", "ROMULUS-SECTOR", "TARSONIS", "CHAR-SURFACE", "AIUR-OUTSKIRTS", "SERRANO-SECTOR", "PANDORA-MOON", "KASHYYYK",
        "NIGHT-CITY-ORBITAL", "TALOS-I", "AEGIS-VII", "SUPER-EARTH-CENTRAL", "MORAQ", "KNOWHERE-OUTSKIRTS", "XANDAR-PRIME", "TRENZALORE"
    };

    private string[] races = {
        // Species Types (Mapped with specific voice profiles below)
        "SYLPHID", "CHITINITE", "CYBORG HYBRID", "PLASMA FLUID", "VOID WALKER", "INSECTOID", "MOLLUSKAN", "SILICON-BASED CRYSTALLINE",
        "PROTO-GEL", "NEURAL-PARASITE", "MECHANIZED-HUMANOID", "FELINE-HUMANOID", "AVIAN-AETHERIAL", "SHADOW-STALKER", "CEPHALOPOD-CENTAUR",
        "GEOMETRIC-ENTITY", "ENERGY-CONSTRUCT", "REPTILIAN-OVERLORD", "BOTANICAL-FLORA", "AMORPHOUS-SLIME", "QUANTUM-GHOST", "DWARVEN-MINER-CYBORG",
        "MYCELIUM-COLLECTIVE", "COGNITIVE-AI-CHASSIS", "GAS-GIANT-FLOATERS", "SYMBIOTE-HOST", "NANITE-SWARM", "SYNTHETIC-ANDROID", "GRAVITY-BENDER",
        "TURIAN", "ASARI", "KROGAN", "VULCAN", "KLINGON", "WOOKIEE", "DALEK-REMNANT", "CYBERMAN", "METROID-HYBRID", "ELDRAZI-CORRUPTED",
        "ZERG-EVOLVED", "PROTOSS-MIND", "XENOMORPH-HYBRID", "TIME-LORD-DRIFTER", "GETH-UNIT", "COVENANT-REMNANT", "BARRAGAN-DEVOURER"
    };

    [Header("Cargo Pools (Logically Threat-Categorized)")]
    private string[] safeCargos = {
        // Everyday Civilian Goods & Food
        "PET STORE ANIMALS", "HYDRO-FARM WHEAT", "MEDICAL SUPPLIES", "HOLIDAY GIFTS", "TEXTBOOKS", "DRINKING WATER", "PIXEL ART PRINTS",
        "SPACE INSTANT NOODLES", "SYNTHETIC COFFEE BEANS", "VINTAGE VINYL RECORDS", "MOISTURE VAPORIZERS", "TRIBBLE PELLETS", "HYPER-DRIVE LUBRICANT",
        "RETRO ARCADE CABINETS", "BOTTLED OXYGEN TANKS", "CONCENTRATED PROTEIN PASTE", "CERAMIC HEAT TILES", "MEDICAL GEL PACKS", "BLUE MILK CRATES",
        "RECYCLED PLASTIC BRICKS", "UNFINISHED NOVEL MANUSCRIPTS", "BOARD GAMES FOR DEEP SPACE", "FRESH HYDRO-TOMATOES", "SOLAR BATTERY CELLS",
        "CLONED ORANGE JUICE", "SYNTHETIC CAT FOOD", "FLOPPY DISKS WITH RETRO GAMES", "SPACE-SUIT SEALS", "ZERO-GRAVITY TENNIS BALLS",
        "DEHYDRATED PIZZA SLICES", "THERMAL BLANKETS", "MECH LUBRICANT OIL", "VIRTUAL REALITY HEADSETS", "MEMORY CARDS WITH ANIME SOUNDTRACKS",
        "DENTAL REPAIR KITS", "CLEAN WATER FILTERS", "ATMOSPHERIC SCRUBBERS", "COMMUNICATOR BATTERIES", "COMPACT DISCS OF 80S MUSIC",
        "PLUMMIE BERRIES", "CATNIP FROM EARTH", "SYNTHETIC RAMEN PACKETS", "POCKET HOLOGRAPHIC PROJECTORS", "USED MANGA VOLUMES",
        
        // Non-Threat Black Market & Poached Goods
        "POACHED EXOTIC ANIMAL PELTS", "POACHED SPACE-WHALE BONE CARVINGS", "BOOTLEG VR BRAINDANCE HOLOS", "COUNTERFEIT SECTOR CREDITS",
        "UNTAXED ALIEN VAPE PODS", "ILLEGAL VINTAGE EARTH WINE", "BLACK-MARKET LUXURY PERFUME", "UNLICENSED MOVIE REPLICAS",
        "KNOCKOFF CYBERNETIC FINGERS", "SMUGGLED CIGARS FROM TITAN", "TAX-EVADED SYNTHETIC SILK", "BOOTLEG HOLO-CARDS"
    };

    private string[] hazardCargos = {
        "XENOMORPH EGGS", "UNSTABLE ANTIMATTER", "ILLEGAL WEAPONS", "DARK MATTER BOMBS", "TOXIC WASTE", "CONTRABAND DRUGS",
        "ILLEGAL SPICE FROM ARRAKIS", "PROTO-MOLECULE CONTAINERS", "CORRUPTED CYBERWARE", "WEAPONIZED VIRUS VIALS", "RED MERCURY WARHEADS",
        "HIGH-EXPOSURE RADIATION RODS", "BANNED PLASMA RIFLES", "MUTAGENIC OOZE", "BLACK-MARKET NEURAL CHIPS", "TACHYON VOLTAGE MINES",
        "UNREGISTERED CLONE WARHEADS", "EXOTIC MATTER EXPLOSIVES", "BIOLOGICAL THREAT SAMPLE-9", "SEAVER-CLASS CONCUSSION MINES",
        "BANNED GENETIC REAGENTS", "EMP SHOCK CHARGES", "UNSTABLE SINGULARITY CORE", "DEMONIC PROTOCOL DISKS", "NERVE GAS CYLINDERS",
        "VOLATILE HYPER-FUEL", "ILLEGAL MECH WARHEADS", "CORROSIVE ACID VATS", "SUB-SPACE WARHEADS", "NANO-BOT DEMOLITION SWARMS",
        "UNSTABLE KYBER CRYSTALS", "ILLEGAL EXOPLASM CHARGES", "HELLFIRE CORE UNITS", "NAPALM GEL TANKS", "UNLICENSED DISINTEGRATORS",
        "OVERCLOCKED QUANTUM BOMBS", "BANNED SECTOR BIOWEAPONS", "VOLATILE STELLAR PLASMA", "DARK ENTITY CONTAINMENT POD", "NECROMORPH MARKER FRAGMENT",
        "ACTIVE FACEHUGGER STASIS POD", "LIVE COMBAT DROIDS WITH ROUGE AI", "CORRUPTED PROTO-VIRUS SAMPLES", "SECTOR-DESTROYING ANTIMATTER CANISTERS"
    };

    private string[] unknownCargos = {
        "LIVE EXOTIC BEASTS", "ANCIENT RELICS", "EXPERIMENTAL AI CORES", "CYBERNETIC IMPLANTS", "UNREGISTERED BIOMASS", "FOSSILIZED EGGS",
        "PROTHEAN BEACON REPLICAS", "SHOGGOTH EMBRYO IN STASIS", "GLOWING SPACE MONOLITH", "BORG NANITE CANISTERS", "MYSTERIOUS GLOWING ORB",
        "HALO INDEX KEY REPLICA", "ANCIENT PRECURSOR ARTIFACT", "UNOPENED STASIS POD", "DEAD GOD'S CRYSTALLIZED BRAIN", "FLOATING GEOMETRIC CUBE",
        "UNKNOWN ALIEN METEORITE", "WHISPERING CONTAINMENT CASKET", "QUANTUM ENTANGLED BOX", "VOID NOISE EMITTER", "TIME-DILATED HOURGLASS",
        "EGG OF AN UNKNOWN APEX PREDATOR", "HYPER-ADVANCED NEURAL INTERFACE", "ANCIENT ALIEN RUNE STONE", "MEMORY RECOVERY CHIP",
        "SEALED CONTRABAND CYLINDER", "CHRONO-DISPLACED DIARY", "UNIDENTIFIED ORGANIC SLIME", "UNKNOWN ENERGY SOURCE", "RESONATING CRYSTAL SHARD",
        "ALIEN MEMORY CONDUIT", "PHANTOM SIGNAL TRANSMITTER", "PULSATING METEOR SHARD", "BIOMECHANICAL PARASITE CORE", "COGNITIVE SPHERE FROM THE VOID",
        "DRIFTING ESCAPE POD RECEPTACLE", "MYSTERIOUS BLACK PYRAMID", "ANCIENT CYBER-TOTEM", "ENCRYPTED DATAPAD FROM A DEAD CIVILIZATION", "EXTRA-DIMENSIONAL SPHERE"
    };

    [Header("Dialogue Archetype Pools")]
    private string[] friendlyIntros = {
        "GREETINGS OFFICER! WE ARE CARRYING {CARGO}. REQUESTING CLEARANCE TO DOCK.",
        "HELLO FRIEND. MY SHIP FROM {ORIGIN} IS LOADED WITH {CARGO}. DECODING TRANSMISSION NOW.",
        "GOOD DAY! JUST A SIMPLE TRANSPORT OF {CARGO}. PLEASE OPEN THE GATE FOR A {RACE}.",
        "STATION CONTROL, THIS IS {NAME}. WE'RE HAULING A FRESH SHIPMENT OF {CARGO}. BLESSINGS OF THE STARS UPON YOU!",
        "ALL SYSTEMS GREEN! WE'VE BROUGHT {CARGO} ALL THE WAY FROM {ORIGIN}. CAN'T WAIT TO UNLOAD!",
        "HEY THERE GUARD! SHIFT ALMOST OVER? WE JUST NEED TO DOCK THIS {CARGO} REAL QUICK.",
        "MAY THE COSMIC WINDS BE IN YOUR FAVOR. WE HAVE AUTHORIZED DELIVERY OF {CARGO}.",
        "TRANSMITTING PERMIT CODES NOW. CARGO HOLD CONTAINS {CARGO}. THANK YOU FOR YOUR PATIENCE, OFFICER!",
        "GREETINGS! PILOT {NAME} HERE. JUST RETURNING FROM {ORIGIN} WITH {CARGO}. READY WHEN YOU ARE!"
    };

    private string[] neutralIntros = {
        "IDENTIFICATION VERIFIED. SHIP CARGO IS {CARGO}. AWAITING GATE DECISION.",
        "TRANSMITTING MANIFEST. WE CARRY {CARGO}. DO NOT KEEP US WAITING AT THIS CHECKPOINT.",
        "THIS IS SHIP {NAME} FROM {ORIGIN}. OUR HOLD CONTAINS {CARGO}. OPEN GATE AS PER PROTOCOL.",
        "REGISTRATION CODE ACKNOWLEDGED. HAULING {CARGO}. PLEASE PROCEED WITH SCANNING.",
        "PILOT {NAME} REGISTERED AS {RACE}. SHIPLOAD: {CARGO}. STANDING BY FOR DOCKING SIGNAL.",
        "CHECKPOINT CONTROL, WE ARE IN VECTOR RANGE. MANIFEST CONFIRMS {CARGO}. INITIATE CLEARANCE.",
        "DECLARING CARGO CONTENTS: {CARGO}. VEHICLE ORIGINATED FROM {ORIGIN}. INSTRUCT NEXT STEPS.",
        "COMMENCING FREQUENCY LOCK. TRANSMITTING MANIFEST FOR {CARGO}. OVER."
    };

    private string[] rudeBanterIntros = {
        "MOVE IT SLOW-BRAIN. I'M HAULING {CARGO} AND TIME IS CREDITS IN THIS SECTOR!",
        "WHY DO WE STILL DEAL WITH FLESH-MIND GUARDS? CARGO IS {CARGO}. LET ME IN OR I FLIP THE ENGINE!",
        "DONT LOOK AT ME LIKE THAT, TWO-LEGS. IT IS JUST {CARGO}. OPEN UP BEFORE MY FUEL EXPIRES!",
        "YOU AGED THREE CYCLES JUST CHECKING MY ID. I HAVE {CARGO} ON BOARD. MOVE THE GATE!",
        "KEEP YOUR SENSORS OFF MY ENGINE BAY. I'M DELIVERING {CARGO} FROM {ORIGIN} AND I'M LATE!",
        "A {RACE} DOES NOT HAVE TIME FOR YOUR BUREAUCRATIC DUST. MANIFEST IS {CARGO}. LET US THROUGH!",
        "WHAT NOW? MORE FORMS? LOOK AT THE HOLD: IT'S JUST {CARGO}! OPEN THE DAMN BARRIER!",
        "STOP STARING AT MY COCKPIT AND HIT THE GREEN BUTTON. I HAVE {CARGO} TO DROP OFF!"
    };

    private string[] nervousPanickedIntros = {
        "U-UH, HELLO CONTROL! NOTHING SUSPICIOUS HERE, JUST HAULING {CARGO}! PLEASE LET US PASS QUICKLY!",
        "OFFICER! THEY'RE RIGHT BEHIND US! I MEAN... WE HAVE A TOTALLY NORMAL CARGO OF {CARGO}! OPEN THE GATE!",
        "PLEASE DON'T SCAN TOO CLOSELY... I MEAN, SCAN ALL YOU WANT! IT'S JUST LEGITIMATE {CARGO} FROM {ORIGIN}!",
        "SYSTEMS CRITICAL! WE NEED DOCKING IMMEDIATE! WE ARE CARRYING {CARGO} AND CANNOT RISK SENSOR DECAY!",
        "SWEATING? ME? NO, MY SPECIES JUST SECRETES MOISTURE WHEN... CARRYING {CARGO}! LET US THROUGH!",
        "THIS TRANSMISSION IS ENCRYPTED FOR A REASON! QUICK, SCAN THE {CARGO} BEFORE WE GET INTERCEPTED!"
    };

    private string[] popCultureSciFiIntros = {
        "DONT PANIC! WE'RE JUST SIMPLE TRADERS FROM {ORIGIN} CARRYING {CARGO}. DO YOU KNOW WHERE YOUR TOWEL IS?",
        "THESE ARE NOT THE DROIDS YOU ARE LOOKING FOR... IT IS MERELY A SHIPMENT OF {CARGO}. MOVE ALONG!",
        "IN SPACE, NO ONE CAN HEAR YOU DELIVER {CARGO}. BUT ON THIS RADIO, WE REALLY NEED THE GATE OPEN.",
        "THE SPICE MUST FLOW! OR IN OUR CASE... THIS SHIPMENT OF {CARGO} FROM {ORIGIN}. DECODE US QUICKLY!",
        "I'VE SEEN THINGS YOU PEOPLE WOULDN'T BELIEVE. ATTACK SHIPS ON FIRE... AND NOW ME DELIVERING {CARGO}.",
        "I AM {NAME}, AND THIS IS MY FAVORITE CARGO HOLD ON THE CITADEL. WE CARRY {CARGO}!",
        "LIVE LONG AND PROSPER, OFFICER. WE BRING A PEACEFUL CARGO OF {CARGO} FROM {ORIGIN}.",
        "IT'S DANGEROUS TO GO ALONE! TAKE THIS... WAIT, NO, THIS IS {CARGO}. DONT TOUCH IT, JUST OPEN THE GATE!",
        "I NEED TO GET THIS PAYLOAD TO CITADEL STATION. MANIFEST: {CARGO}. MAKE IT FAST!",
        "THE EMPEROR PROTECTS, OFFICER! OUR HOLD CONTAINS PURIFIED {CARGO}. CLEAR OUR DOCKING VECTOR!",
        "DEMOCRACY REQUIRES THIS SHIPMENT OF {CARGO}! CLEAR THE WAY FOR SUPER EARTH'S FINEST!",
        "MAKE IT SO, CONTROL. WE ARE HAULING {CARGO} FROM {ORIGIN} AND CAPTAIN {NAME} DOES NOT LIKE DELAYS!"
    };

    public AlienProfile GenerateNewProfile()
    {
        AlienProfile profile = new AlienProfile();

        // 1. Pick Identity with Anti-Duplication Filtering
        profile.alienName = GetUniqueRandom(alienNames, recentNames, nameHistorySize);
        profile.originPlanet = GetUniqueRandom(planets, recentPlanets, planetHistorySize);
        profile.race = races[Random.Range(0, races.Length)];

        // 2. Configure Voice Pitch and Typing Speed according to Race Archetype
        ApplyRaceVoiceProfile(profile);

        // 3. Pick Threat & Anti-Duplicated Cargo
        int categoryIndex = Random.Range(0, 3);
        profile.cargoThreat = (ThreatCategory)categoryIndex;

        switch (profile.cargoThreat)
        {
            case ThreatCategory.Safe:
                profile.cargoItem = GetUniqueRandom(safeCargos, recentCargos, cargoHistorySize);
                break;
            case ThreatCategory.Hazard:
                profile.cargoItem = GetUniqueRandom(hazardCargos, recentCargos, cargoHistorySize);
                break;
            case ThreatCategory.Unknown:
                profile.cargoItem = GetUniqueRandom(unknownCargos, recentCargos, cargoHistorySize);
                break;
        }

        // 4. Pick Dialogue Archetype (5 Styles)
        int attitude = Random.Range(0, 5);
        string selectedTemplate = "";
        switch (attitude)
        {
            case 0: selectedTemplate = friendlyIntros[Random.Range(0, friendlyIntros.Length)]; break;
            case 1: selectedTemplate = neutralIntros[Random.Range(0, neutralIntros.Length)]; break;
            case 2: selectedTemplate = rudeBanterIntros[Random.Range(0, rudeBanterIntros.Length)]; break;
            case 3: selectedTemplate = nervousPanickedIntros[Random.Range(0, nervousPanickedIntros.Length)]; break;
            case 4: selectedTemplate = popCultureSciFiIntros[Random.Range(0, popCultureSciFiIntros.Length)]; break;
        }

        // 5. Format Dialogue replacing all dynamic placeholders
        profile.dialogue = selectedTemplate
            .Replace("{CARGO}", $"[{profile.cargoItem}]")
            .Replace("{NAME}", profile.alienName)
            .Replace("{ORIGIN}", profile.originPlanet)
            .Replace("{RACE}", profile.race);

        // 6. Generate Target Signal Frequencies
        profile.targetFrequency = Random.Range(0.1f, 0.9f);
        profile.targetPhase = Random.Range(0.1f, 0.9f);

        return profile;
    }

    private string GetUniqueRandom(string[] pool, Queue<string> historyQueue, int maxHistorySize)
    {
        if (pool == null || pool.Length == 0) return string.Empty;

        // Prevent infinite loops if history size >= pool size
        int safeHistoryLimit = Mathf.Min(maxHistorySize, pool.Length - 1);
        if (safeHistoryLimit < 1) safeHistoryLimit = 0;

        string selected = pool[Random.Range(0, pool.Length)];
        int attempts = 0;

        // Reroll up to 50 times to find an item not present in recent history queue
        while (historyQueue.Contains(selected) && attempts < 50)
        {
            selected = pool[Random.Range(0, pool.Length)];
            attempts++;
        }

        // Enqueue new pick and trim queue to match configured max size
        historyQueue.Enqueue(selected);
        while (historyQueue.Count > safeHistoryLimit && historyQueue.Count > 0)
        {
            historyQueue.Dequeue();
        }

        return selected;
    }

    private void ApplyRaceVoiceProfile(AlienProfile profile)
    {
        string raceUpper = profile.race.ToUpper();

        if (raceUpper.Contains("INSECTOID") || raceUpper.Contains("CHITINITE") || raceUpper.Contains("NANITE") || raceUpper.Contains("DALEK"))
        {
            profile.voicePitch = Random.Range(1.4f, 1.8f);
            profile.typingSpeed = Random.Range(40f, 55f);
        }
        else if (raceUpper.Contains("KROGAN") || raceUpper.Contains("WOOKIEE") || raceUpper.Contains("REPTILIAN") || raceUpper.Contains("DWARVEN"))
        {
            profile.voicePitch = Random.Range(0.5f, 0.75f);
            profile.typingSpeed = Random.Range(18f, 25f);
        }
        else if (raceUpper.Contains("CYBORG") || raceUpper.Contains("GETH") || raceUpper.Contains("MECHANIZED") || raceUpper.Contains("SYNTHETIC"))
        {
            profile.voicePitch = Random.Range(0.9f, 1.1f);
            profile.typingSpeed = Random.Range(35f, 45f);
        }
        else if (raceUpper.Contains("PLASMA") || raceUpper.Contains("SLIME") || raceUpper.Contains("MOLLUSKAN") || raceUpper.Contains("PROTO-GEL"))
        {
            profile.voicePitch = Random.Range(0.7f, 0.9f);
            profile.typingSpeed = Random.Range(20f, 30f);
        }
        else
        {
            profile.voicePitch = Random.Range(0.85f, 1.25f);
            profile.typingSpeed = Random.Range(25f, 38f);
        }
    }
}