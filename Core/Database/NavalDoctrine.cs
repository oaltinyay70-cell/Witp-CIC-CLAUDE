using System;
using System.Collections.Generic;
using System.Linq;

namespace MUTHUR6000.Core.Database
{
    public enum NavalFunctionalClass
    {
        MajorSurfaceCombatant,
        SurfaceEscort,
        MineWarfare,
        AmphibiousTransport,
        CargoLogistics,
        MobileTenderAuxiliary,
        Submarine,
        Unknown
    }

    public class NavalUnitCapability
    {
        public string HullCode { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public NavalFunctionalClass FunctionalClass { get; set; } = NavalFunctionalClass.Unknown;
        public string FunctionalClassName { get; set; } = string.Empty;
        public string OperationalRole { get; set; } = string.Empty;
        public string TacticalReasoning { get; set; } = string.Empty;
        public string InterceptSignificance { get; set; } = string.Empty;
        public bool IsCapital { get; set; }
        public bool IsCarrier { get; set; }
        public bool IsAmphibiousAssault { get; set; }
        public bool IsFastInsertion { get; set; }
        public bool IsAircraftTransport { get; set; }
        public bool IsUnderwayReplenishment { get; set; }
        public bool IsMobileTender { get; set; }
        public bool IsMobileRepair { get; set; }
    }

    public class NavalDoctrineRegistry
    {
        private static readonly Lazy<NavalDoctrineRegistry> _instance = new(() => new NavalDoctrineRegistry());
        public static NavalDoctrineRegistry Instance => _instance.Value;

        private readonly List<NavalUnitCapability> _capabilities = new();

        public IReadOnlyList<NavalUnitCapability> Capabilities => _capabilities;

        private NavalDoctrineRegistry()
        {
            InitCapabilities();
        }

        private void InitCapabilities()
        {
            Add("CV", "Fleet Aircraft Carrier", NavalFunctionalClass.MajorSurfaceCombatant, "Major Surface Combatant",
                "High-capacity flight deck; offensive core of naval air combat task forces.",
                "Fleet aircraft carrier constitutes offensive core of naval air strike task force; high-speed mobile air power projection.",
                "Primary strategic target. Sighting establishes carrier flight deck presence; indicates active or imminent naval air strike within 3-5 hexes.",
                isCapital: true, isCarrier: true);

            Add("CVL", "Light Aircraft Carrier", NavalFunctionalClass.MajorSurfaceCombatant, "Major Surface Combatant",
                "Converted cruiser or purpose-built light carrier for fleet air defense and strike augmentation.",
                "Light aircraft carrier providing organic air umbrella and secondary strike capability for combat formations.",
                "High-value naval aviation asset; enhances task force air defense and anti-shipping strike capacity.",
                isCapital: true, isCarrier: true);

            Add("CVE", "Escort Aircraft Carrier", NavalFunctionalClass.MajorSurfaceCombatant, "Major Surface Combatant",
                "Converted merchant hull or auxiliary carrier providing ASW coverage, convoy escort, and aircraft ferrying.",
                "Auxiliary flight deck asset supporting ASW patrols and air combat replenishment.",
                "Significant naval aviation unit; key component in amphibious support groups and ASW hunter-killer forces.",
                isCapital: true, isCarrier: true);

            Add("BB", "Battleship", NavalFunctionalClass.MajorSurfaceCombatant, "Major Surface Combatant",
                "Heavily armored capital warship with main battery exceeding 12-inch caliber; designed for fleet battle line action and heavy shore bombardment.",
                "Ultimate surface gun platform; high survival probability and lethal surface gunnery combat effectiveness.",
                "Top-tier capital target. Deployment indicates heavy surface strike action or preparation for major amphibious shore bombardment.",
                isCapital: true);

            Add("BC", "Battlecruiser", NavalFunctionalClass.MajorSurfaceCombatant, "Major Surface Combatant",
                "Capital ship trading armor for high sustained tactical speed; heavy main battery for surface interdiction.",
                "Fast surface capital interdictor capable of destroying cruiser forces and outrunning slower battleships.",
                "Major capital target; formidable threat against merchant convoys, cruisers, and forward anchorages.",
                isCapital: true);

            Add("CA", "Heavy Cruiser", NavalFunctionalClass.MajorSurfaceCombatant, "Major Surface Combatant",
                "Armored cruiser armed with 8-inch main battery; backbone of surface strike groups and carrier escort screens.",
                "Formidable gun platform capable of independent surface raids and heavy carrier escort screening.",
                "Priority surface combatant. Presence signals offensive surface combat group operations.",
                isCapital: true);

            Add("CL", "Light Cruiser", NavalFunctionalClass.MajorSurfaceCombatant, "Major Surface Combatant",
                "Cruiser armed with 5-inch to 6-inch rapid-fire main battery; destroyer squadron leader and fleet air defense umbrella.",
                "Fast surface combatant delivering high volume of gunfire against escorts, aircraft, and light shipping.",
                "Tactical threat to light surface forces and torpedo craft; key escort unit for capital formations.");

            Add("DD", "Fleet Destroyer", NavalFunctionalClass.SurfaceEscort, "Surface Escort",
                "Fast multi-mission surface combatant with heavy torpedo armament and ASW depth charge battery.",
                "Versatile screen protecting capital units from submarine attack and delivering lethal torpedo spreads.",
                "Standard surface combatant; high tactical agility.");

            Add("DE", "Destroyer Escort", NavalFunctionalClass.SurfaceEscort, "Surface Escort",
                "Purpose-built convoy escort specialized in ASW and anti-aircraft protection.",
                "Dedicated escort protecting merchant convoys and supply lanes from undersea interdiction.",
                "Convoy escort unit.");

            Add("SS", "Submarine", NavalFunctionalClass.Submarine, "Submarine",
                "Undersea combatant specialized in merchant interdiction, intelligence reconnaissance, and naval ambush.",
                "Stealth torpedo and mine delivery platform capable of penetrating guarded sea corridors.",
                "Critical maritime threat to logistical and capital formations.");

            Add("AP", "Troop Transport", NavalFunctionalClass.AmphibiousTransport, "Amphibious Transport",
                "Dedicated troop carrying vessel for amphibious invasion operations.",
                "Primary amphibious assault asset; presence indicates imminent landing operation.",
                "High-priority interdiction target to prevent enemy beachhead expansion.",
                isAmphibiousAssault: true);

            Add("APA", "Attack Transport", NavalFunctionalClass.AmphibiousTransport, "Amphibious Transport",
                "Combat-loaded troop transport equipped with organic landing craft for contested beachhead assault.",
                "Elite amphibious assault vessel delivering frontline assault infantry and heavy equipment under fire.",
                "Top-priority amphibious target; neutralization halts assault momentum.",
                isAmphibiousAssault: true);

            Add("LST", "Landing Ship Tank", NavalFunctionalClass.AmphibiousTransport, "Amphibious Transport",
                "Beachable amphibious vessel delivering armor, motorized transport, and heavy combat artillery directly ashore.",
                "Direct beachhead armor and logistical delivery asset.",
                "High-value amphibious target.",
                isAmphibiousAssault: true);

            Add("AK", "Cargo Freighter", NavalFunctionalClass.CargoLogistics, "Cargo Logistics",
                "Standard naval cargo ship transporting general military cargo, equipment, and ammunition.",
                "Sustains forward operational bases and army supply lines.",
                "Logistical lifeline target.");

            Add("AO", "Fleet Oiler", NavalFunctionalClass.CargoLogistics, "Cargo Logistics",
                "Naval oiler equipped for underway replenishment of combat warships at sea.",
                "Critical force multiplier enabling extended forward carrier and strike force combat operations.",
                "High-priority strategic target; loss limits enemy fleet operational endurance.",
                isUnderwayReplenishment: true);

            Add("TK", "Commercial Tanker", NavalFunctionalClass.CargoLogistics, "Cargo Logistics",
                "Merchant tanker hauling bulk petroleum and fuel products between resource nodes and refineries.",
                "Transports oil supplies crucial for naval operations and industrial production.",
                "Prime target for economic warfare and submarine trade interdiction.");

            Add("AS", "Submarine Tender", NavalFunctionalClass.MobileTenderAuxiliary, "Mobile Tender / Auxiliary",
                "Provides mobile base support, torpedo rearming, and maintenance for submarine flotillas.",
                "Enables forward submarine staging without requiring permanent dockyard facilities.",
                "High-value auxiliary target; disruption degrades forward submarine patrols.",
                isMobileTender: true);

            Add("AD", "Destroyer Tender", NavalFunctionalClass.MobileTenderAuxiliary, "Mobile Tender / Auxiliary",
                "Provides mobile repair, rearming, and maintenance facilities for destroyer and escort squadrons.",
                "Maintains operational readiness of forward surface screening forces.",
                "Strategic auxiliary asset.",
                isMobileTender: true);

            Add("AV", "Seaplane Tender", NavalFunctionalClass.MobileTenderAuxiliary, "Mobile Tender / Auxiliary",
                "Mobile seaplane base providing aviation fuel, maintenance, and flight operations support in forward anchorages.",
                "Establishes forward maritime air search umbrellas in undeveloped lagoons and islands.",
                "Key reconnaissance enabler; destroys enemy eyes across ocean sectors.",
                isMobileTender: true);

            Add("AR", "Repair Ship", NavalFunctionalClass.MobileTenderAuxiliary, "Mobile Tender / Auxiliary",
                "Floating workshop equipped to repair battle damage on warships in forward fleet anchorages.",
                "Reduces fleet transit downtime by repairing combat damage forward.",
                "Priority auxiliary asset.",
                isMobileRepair: true);
        }

        private void Add(string hullCode, string className, NavalFunctionalClass funcClass, string funcClassName,
            string role, string reasoning, string intercept,
            bool isCapital = false, bool isCarrier = false, bool isAmphibiousAssault = false,
            bool isFastInsertion = false, bool isAircraftTransport = false, bool isUnderwayReplenishment = false,
            bool isMobileTender = false, bool isMobileRepair = false)
        {
            _capabilities.Add(new NavalUnitCapability
            {
                HullCode = hullCode,
                ClassName = className,
                FunctionalClass = funcClass,
                FunctionalClassName = funcClassName,
                OperationalRole = role,
                TacticalReasoning = reasoning,
                InterceptSignificance = intercept,
                IsCapital = isCapital,
                IsCarrier = isCarrier,
                IsAmphibiousAssault = isAmphibiousAssault,
                IsFastInsertion = isFastInsertion,
                IsAircraftTransport = isAircraftTransport,
                IsUnderwayReplenishment = isUnderwayReplenishment,
                IsMobileTender = isMobileTender,
                IsMobileRepair = isMobileRepair
            });
        }

        public NavalUnitCapability? FindCapability(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return null;

            string qTrim = query.Trim();
            foreach (var cap in _capabilities)
            {
                if (string.Equals(qTrim, cap.HullCode, StringComparison.OrdinalIgnoreCase))
                    return cap;
            }

            string qUpper = qTrim.ToUpperInvariant();
            foreach (var cap in _capabilities)
            {
                if (qUpper.Contains($"[{cap.HullCode}]") ||
                    qUpper.Contains($" {cap.HullCode} ") ||
                    qUpper.StartsWith($"{cap.HullCode} ") ||
                    qUpper.EndsWith($" {cap.HullCode}"))
                {
                    return cap;
                }
            }

            foreach (var cap in _capabilities)
            {
                if (qUpper.Contains(cap.ClassName.ToUpperInvariant()))
                    return cap;
            }

            if (qUpper.Contains("CARRIER") || qUpper.Contains("FLATTOP")) return FindCapability("CV");
            if (qUpper.Contains("BATTLESHIP")) return FindCapability("BB");
            if (qUpper.Contains("HEAVY CRUISER")) return FindCapability("CA");
            if (qUpper.Contains("LIGHT CRUISER")) return FindCapability("CL");
            if (qUpper.Contains("DESTROYER")) return FindCapability("DD");
            if (qUpper.Contains("SUBMARINE")) return FindCapability("SS");
            if (qUpper.Contains("TRANSPORT")) return FindCapability("AP");
            if (qUpper.Contains("OILER")) return FindCapability("AO");
            if (qUpper.Contains("TANKER")) return FindCapability("TK");
            if (qUpper.Contains("CARGO") || qUpper.Contains("FREIGHTER")) return FindCapability("AK");

            return null;
        }

        public static bool IsJapaneseVessel(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            string upper = name.ToUpperInvariant();
            if (upper.Contains("MARU") || upper.Contains("IJN") ||
                upper.Contains("I-") || upper.Contains("RO-") || upper.Contains("HA-") ||
                upper.Contains("CH ") || upper.Contains("CH-") ||
                upper.Contains("W.") || upper.Contains("PB ")) return true;

            string[] jNames = {
                "YAMATO", "MUSASHI", "NAGATO", "MUTSU", "KONGO", "HIEI", "HARUNA", "KIRISHIMA", "FUSO", "YAMASHIRO", "ISE", "HYUGA",
                "AKAGI", "KAGA", "SORYU", "HIRYU", "SHOKAKU", "ZUIKAKU", "RYUJO", "ZUIHO", "SHOHO", "HOSHO", "TAIHO", "JUNYO", "HIYO",
                "AOBA", "KINUGASA", "FURUTAKA", "KAKO", "MYOKO", "NACHI", "HAGURO", "ASHIGARA", "TAKAO", "ATAGO", "MAYA", "CHOKAI",
                "MOGAMI", "MIKUMA", "SUZUYA", "KUMANO", "TONE", "CHIKUMA", "SENDAI", "JINTSU", "NAKA", "YURA", "KINU", "ABUKUMA",
                "NAGARA", "ISUZU", "NATORI", "KUMA", "TAMA", "KITAKAMI", "OI", "KISO", "TENRYU", "TATSUTA", "YUBARI", "AGANO", "NOSHIRO", "YAHAGI", "SAKAWA", "OYODO",
                "FUBUKI", "AYANAMI", "URANAMI", "SHIKINAMI", "AKATSUKI", "HIBIKI", "IKAZUCHI", "INAZUMA", "HATSUHARU", "SHIRATSUYU",
                "ASASHIO", "KAGERO", "KUROSHIO", "OYASHIO", "YUKIKAZE", "AMATSUKAZE", "TOKITSUKAZE", "URAKAZE", "ISOKAZE", "HAMAKAZE",
                "TANIKAZE", "NOWAKI", "ARASHI", "HAGIKAZE", "MAIKAZE", "YUGUMO", "AKIZUKI", "SHIMAKAZE", "MUTSUKI", "KAMIKAZE", "MINEKAZE"
            };
            return jNames.Any(j => upper.Contains(j));
        }
    }
}
