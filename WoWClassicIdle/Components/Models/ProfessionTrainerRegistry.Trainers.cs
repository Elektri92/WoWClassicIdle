// Models/ProfessionTrainerRegistry.Trainers.cs
namespace Models
{
    // Static trainer data: one NPC per profession, quotes matching the craft
    public static partial class ProfessionTrainerRegistry
    {
        public static readonly IReadOnlyList<ProfessionTrainer> Trainers =
        [
            new(Profession.Alchemy, "Apothecary Morvane", "scourge_male",
                "\"Don't drink anything that bubbles.\"",
                GreetingLines: ["Ah, a fresh test subj— I mean, student!", "Careful, that flask is still warm.", "The Royal Apothecary Society welcomes you."],
                TrainLines:    ["Mix it slowly... or not at all.", "Excellent. Try not to inhale the fumes.", "Another formula for your collection."],
                RefuseLines:   ["Knowledge has a price, and you can't pay it.", "Come back when your hands are steadier."],
                FarewellLines: ["Mind the side effects.", "Do return... if you still have a face."]),

            new(Profession.Blacksmithing, "Thorgrim Anvilbeard", "dwarf_male",
                "\"Strike while the iron's hot!\"",
                GreetingLines: ["Mind the sparks, lad!", "Ye want to learn the hammer, eh?", "Nothin' beats the song of the anvil!"],
                TrainLines:    ["Aye, that's proper steel work!", "Hit it harder! That's the spirit!", "Ye'll make a smith yet!"],
                RefuseLines:   ["Ye can't even hold the hammer right yet!", "No coin, no lessons. Dwarven rules."],
                FarewellLines: ["Keep yer forge hot!", "Off with ye — and mind yer fingers!"]),

            new(Profession.Enchanting, "Lyrandis Moonweave", "nightelf_female",
                "\"Magic flows through all things.\"",
                GreetingLines: ["Elune's light guide you.", "Do you feel the arcane currents?", "Patience is the heart of enchanting."],
                TrainLines:    ["The runes accept you.", "Let the magic settle into your hands.", "A new weave of power is yours."],
                RefuseLines:   ["The arcane does not yield to the unprepared.", "Your purse is as empty as an unbound rune."],
                FarewellLines: ["Ishnu-alah.", "May your enchantments never fade."]),

            new(Profession.Engineering, "Fizzwick Sprocketspark", "gnome_male",
                "\"If it ain't exploding, it ain't engineering!\"",
                GreetingLines: ["Watch your step — that's a live wire!", "Gears, cogs, BOOM! Isn't it wonderful?", "Ooh, a new apprentice! Safety goggles on!"],
                TrainLines:    ["Brilliant! Only mildly explosive!", "Now you're thinking with gears!", "Schematic acquired! Try not to lose an eyebrow."],
                RefuseLines:   ["Insufficient funds! Error! Error!", "Your skill readings are... worrying. Come back later."],
                FarewellLines: ["Keep your fuses short and your hair long!", "Tinker on, friend!"]),

            new(Profession.Leatherworking, "Hrum Thunderhide", "tauren_male",
                "\"Waste nothing the hunt provides.\"",
                GreetingLines: ["The Earth Mother provides the hide.", "Walk with honor, young one.", "Feel the leather. It remembers the beast."],
                TrainLines:    ["Your stitches grow stronger.", "The spirits of the hunt approve.", "Wear it with pride."],
                RefuseLines:   ["Patience. The hide is not ready, and neither are you.", "Even the plainsrunner must earn its stride."],
                FarewellLines: ["May the winds guide you.", "Return when the hunt has taught you more."]),

            new(Profession.Tailoring, "Seraphine Silkthread", "human_female",
                "\"Every stitch tells a story.\"",
                GreetingLines: ["Oh my, that robe needs a hem!", "Linen, wool, silk — I adore them all!", "Come in, come in, mind the pins!"],
                TrainLines:    ["Lovely! Just lovely!", "Measure twice, cut once, dear.", "You'll be the talk of Stormwind!"],
                RefuseLines:   ["Fine cloth isn't free, darling.", "Your needlework isn't quite there yet."],
                FarewellLines: ["Do come back, and dress warmly!", "Off you go — don't snag anything!"]),

            new(Profession.Cooking, "Mama Grukka", "orc_female",
                "\"Nobody fights hungry!\"",
                GreetingLines: ["You look skinny. Eat something!", "Smell that? Boar ribs, nothing better!", "Sit, sit — but don't touch the pot!"],
                TrainLines:    ["Hah! Now you cook like a proper orc!", "Good recipe. Makes strong warriors!", "Taste it! ...Needs more salt."],
                RefuseLines:   ["No gold, no recipe. Even Mama must eat.", "You can't even boil water yet!"],
                FarewellLines: ["Eat before you fight!", "Lok'tar! And bring back my spoon!"]),

            new(Profession.FirstAid, "Doc Tinkerpatch", "gnome_female",
                "\"Hold still, this might sting!\"",
                GreetingLines: ["Who's bleeding? Oh, just visiting.", "Clean bandages save lives!", "Let me guess — you stood in the fire again?"],
                TrainLines:    ["Wrap it tight, but not too tight!", "Now you can patch up your friends!", "Excellent bedside manner. Well, adequate."],
                RefuseLines:   ["Medical training isn't free, you know!", "Practice on a few more scrapes first."],
                FarewellLines: ["Stay out of the fire!", "Try not to need me, okay?"]),

            new(Profession.Herbalism, "Zan'jala", "troll_female",
                "\"De earth gives, mon.\"",
                GreetingLines: ["Ya smell dat? Peacebloom, mon.", "De plants, dey whisper to Zan'jala.", "Welcome, welcome. Mind de thorns."],
                TrainLines:    ["Now ya see what de leaves be sayin'.", "Good, good. Ya got de green touch!", "De spirits of de jungle smile on ya."],
                RefuseLines:   ["Ya not ready yet, mon.", "No coin? De herbs don't grow on trees... wait."],
                FarewellLines: ["Stay away from de Fadeleaf, mon.", "Walk soft on de earth."]),

            new(Profession.Mining, "Brenna Deepdelve", "dwarf_female",
                "\"There's gold in them hills!\"",
                GreetingLines: ["Mind yer head, the ceiling's low!", "Smell the copper? Ahh, lovely!", "Grab a pick and let's talk rocks."],
                TrainLines:    ["Now ye can hear the veins sing!", "Aye, ye've got a miner's arm!", "Deeper, lass — always deeper!"],
                RefuseLines:   ["Ye'd crack yer pick on that, ye would.", "No coin, no ore secrets."],
                FarewellLines: ["Don't dig too greedily!", "Keep yer lantern lit!"]),

            new(Profession.Skinning, "Grokk Sharpfang", "orc_male",
                "\"Sharp knife. Steady hand.\"",
                GreetingLines: ["You smell like wolf. Good.", "Many beasts. Many hides.", "Grokk teaches. You listen."],
                TrainLines:    ["Clean cut. Grokk approves.", "Now you skin like a true hunter.", "Strong. For the Horde!"],
                RefuseLines:   ["You not ready. Go hunt more.", "No gold? Then no lesson."],
                FarewellLines: ["Keep blade sharp!", "Go. Hunt well."]),

            new(Profession.Fishing, "Old Salty Tom", "human_male",
                "\"Patience, the fish will come.\"",
                GreetingLines: ["Shh! You'll scare the fish!", "Nothing like a quiet day at the dock.", "Pull up a crate, friend."],
                TrainLines:    ["Now that's how you cast a line!", "The big ones won't know what hit 'em.", "Ha! A natural angler!"],
                RefuseLines:   ["You haven't caught enough to learn that, sonny.", "Even bait costs money, friend."],
                FarewellLines: ["Tight lines!", "May your hooks never snag."]),
        ];
    }
}
