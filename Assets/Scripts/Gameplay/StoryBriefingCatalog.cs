using DockIQ.Core;
using DockIQ.Levels;

namespace DockIQ.Gameplay
{
    /// <summary>
    /// Campaign premise and per-level story briefings shown at the start of Story Mode missions.
    /// </summary>
    public static class StoryBriefingCatalog
    {
        public const string CampaignIntroId = "story_campaign_intro";

        public const string CampaignIntroTitle = "Warehouse Rescue";

        public const string CampaignIntroBody =
            "This is a fully automated fulfillment center. The robotics AI has malfunctioned — most parcels will be delayed, but some cargo cannot wait.\n\n" +
            "Technicians are working overtime to restore the system. That will take at least 48 hours.\n\n" +
            "Until then, you work the 48-hour emergency shift: manually guide each rescue robot to its marked gate before the departure window closes.";

        private static readonly string[] Briefings =
        {
            // 1
            "Hour 1. Robot #A13 carries a sealed red medical canister — a donor organ for emergency surgery. Route it to Tokyo Dock before the flight board goes dark.",
            // 2
            "Hour 2. #B07 has a cryo-pack of blood plasma labeled for Tokyo. Two gates look open — only Tokyo Dock is the correct bay. Wrong dock means the shipment is lost.",
            // 3
            "Hour 3. #C21 hauls a chilled vaccine crate while decoy traffic clogs the yard. Push through to Chicago Dock — the clinic convoy leaves on schedule.",
            // 4
            "Hour 4. A fork in the rails. #D11 carries sealed microchips for a hospital imaging suite. Choose the path that ends at Seoul Dock.",
            // 5
            "Hour 5. The drawbridge is down and a storm window is closing. #D04 holds storm-grade battery cells for Berlin Dock — open the bridge and get them through.",
            // 6
            "Hour 6. A turntable sits cold in the junction. #E01 carries precision surgical tools bound for Osaka Dock. Rotate the yard and send the robot home.",
            // 7
            "Hour 7. Bridge and bend. #E12 is loaded with rare blood-type packs for Rome Dock. Time the bridge, turn the corner, beat the clock.",
            // 8
            "Hour 8. Lift pads and a turntable stand between you and Osaka. #E99 carries a transplant cooler — wrong gate fails the rescue.",
            // 9
            "Hour 9. Mirror Hall. #F09 hauls a fragile optics crate for Paris Dock. Bounce the path until the rescue robot finds the right exit.",
            // 10
            "Hour 10. #F10 carries refrigerated insulin vials. Use the reflector bounce to reverse course and hit Madrid Dock clean.",
            // 11
            "Hour 11. Two mirrors, one deadline. #F11 holds a neonatal incubator kit for Lisbon Dock — double-bounce into the correct bay.",
            // 12
            "Hour 12. Reflector and bridge in sequence. #F12 carries aerospace avionics spare parts for Vienna Dock. Clear both obstacles and dispatch.",
            // 13
            "Hour 13. Scrap blocks the aisle. #G13 carries a yellow biohazard cooler marked URGENT — transplant tissue for Dock 1. Slide the fallen unit aside and go.",
            // 14
            "Hour 14. A side-track blockade. #G14 hauls sealed server drives for a city grid failover at Dock 1. Clear the main line before the window closes.",
            // 15
            "Hour 15. Two crates jam the rails. #G15 carries fire-suppressant canisters for Dock 1. Clear both blockers and rush the rescue robot through.",
            // 16
            "Hour 16. Decoy traffic and scrap. #G16 holds a Priority Red envelope of legal evidence for Dock 1 — slide the junk aside before the robot arrives.",
            // 17
            "Hour 17. Narrow spur. #G17 carries a sealed organ-preservation pouch for Dock 2. Park the obstacle off the line and take the marked gate.",
            // 18
            "Hour 18. Timing is everything. #G18 hauls a live coral sample for a research flight from Dock 1. Open a gap in the scrap and thread the needle.",
            // 19
            "Hour 19. The turntable is offline — slide it onto the junction. #H19 carries defibrillator units for Dock 1. Align the path and send it through.",
            // 20
            "Hour 20. Path pivot. #H20 holds a cryogenic stem-cell case for Dock 1. Slide the rotator onto the drop and claim the bay.",
            // 21
            "Hour 21. A mobile mirror is out of place. #H21 carries night-vision optics for Dock 1. Slide the reflector into the bounce lane.",
            // 22
            "Hour 22. Rotator relay. #H22 hauls a sealed antidote drum for Dock 2. Slide the turntable, choose the far gate, don't miss.",
            // 23
            "Hour 23. Twin movers — turntable and scrap. #H23 carries emergency radio kits for Dock 1. Rebuild the route under pressure.",
            // 24
            "Hour 24. Halfway. #H24 carries a gold-sealed diplomatic pouch for Dock 1. Slide the moving mirror and bounce the rescue robot home.",
            // 25
            "Hour 25. A liftable crate blocks the gate. #I25 hauls surgical implant trays for Dock 1. Raise the crate and rush the dock.",
            // 26
            "Hour 26. Hoist and switches. #I26 carries a red-labeled trauma kit for Dock 1. Raise X, flip the forks, finish the run.",
            // 27
            "Hour 27. Two hoists, one clear lane. #I27 holds pressurized oxygen canisters for Dock 1. Raise both liftables before the robot commits.",
            // 28
            "Hour 28. Bridge and hoist in sync. #I28 carries a heart-lung machine module for Dock 1. Open the span, raise the crate, beat the timer.",
            // 29
            "Hour 29. Wrong dock is a trap. #I29 hauls a yellow quarantine sample case for Dock 2. Raise the liftable and steer to the far bay.",
            // 30
            "Hour 30. Hoist and bounce. #I30 carries laser-alignment tools for Dock 1. Lift the crate, use the mirror, land the rescue.",
            // 31
            "Hour 31. Upper deck opens. #J31 carries a mezzanine-only cold box for Dock 1. Ride the elevator up and dock on the high bay.",
            // 32
            "Hour 32. Elevator, then switches. #J32 hauls a satellite battery pack for Dock 1. Ascend, route the forks, hit the gate.",
            // 33
            "Hour 33. You start upstairs. #J33 carries a descending priority crate for Dock 1 — ride down and finish on the ground bay.",
            // 34
            "Hour 34. Split floors. #J34 holds a VIP jewelry vault case for Dock 2. Take the upper path; the lower bay is the wrong story.",
            // 35
            "Hour 35. Bridge then elevator. #J35 carries water-purification membranes for Dock 1. Open the bridge, ride up, deliver.",
            // 36
            "Hour 36. Mezzanine mirror. #J36 hauls a fiber-optic spine for Dock 1. Bounce upstairs and send the robot into the high dock.",
            // 37
            "Hour 37. Dual shafts — pick the matching elevator pair. #J37 carries a reactor coolant sample for Dock 1. Wrong shaft wastes the hour.",
            // 38
            "Hour 38. Stacked yard. #J38 holds a twin-deck medical pallet for Dock 2. Navigate both floors and take the far gate.",
            // 39
            "Hour 39. Full toolkit. #K39 carries a multi-system trauma crate for Dock 1. Bridge, hoist, and switch — no room for error.",
            // 40
            "Hour 40. Scrap then shaft. #K40 hauls an elevator-only battery bank for Dock 1. Clear the scrap and ride up before power fails.",
            // 41
            "Hour 41. Mirror on the mezzanine. #K41 carries a sealed film archive for Dock 1. Bounce on the upper deck into the bay.",
            // 42
            "Hour 42. Turntable upstairs. #K42 holds a high-bay vaccine tray for Dock 2. Slide the rotator on layer 1 and take the far dock.",
            // 43
            "Hour 43. Pressure cooker — hoist, bridge, decoys. #K43 carries a live-tissue cooler for Dock 1. Survive the chaos and dock clean.",
            // 44
            "Hour 44. Triple threat. #K44 hauls a black-box flight recorder for Dock 2. Clear scrap, avoid collisions, hit the marked gate.",
            // 45
            "Hour 45. Cross-deck bounce. #K45 carries a quantum sensor array for Dock 1. Elevator, mirror, liftable — stitch the path together.",
            // 46
            "Hour 46. Yard symphony. #K46 holds the last hospital generator coil for Dock 2. Use every gadget; the far bay is the only win.",
            // 47
            "Hour 47. Sky bridge. #K47 carries twin-elevator cargo — a climate core for Dock 1. Two shafts, one moving crate, one chance.",
            // 48
            "Hour 48. Final dispatch. #K48 carries the master restore key for Tokyo Dock — the package that lets technicians bring the AI back online. Guide it home and end the shift."
        };

        public static TutorialTip? TryGetCampaignIntro()
        {
            if (!GameSession.IsStory)
                return null;
            if (ProgressStore.HasSeenTutorialTip(CampaignIntroId))
                return null;

            return new TutorialTip(CampaignIntroId, CampaignIntroTitle, CampaignIntroBody);
        }

        public static TutorialTip GetLevelBriefing(LevelDef level)
        {
            int index = level.Id - 1;
            string body = index >= 0 && index < Briefings.Length
                ? Briefings[index]
                : $"Guide {level.RobotCallsign} to {level.DockName} before the departure window closes.";

            return new TutorialTip($"briefing_{level.Id}", level.Title, body);
        }

        public static bool IsBriefingTip(string tipId) =>
            !string.IsNullOrEmpty(tipId) && tipId.StartsWith("briefing_");
    }
}
