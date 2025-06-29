using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.Personalities;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace ClassesSellers.Content.NPCs.ClassesSellers
{
    [AutoloadHead]
    public class ClassesSellers : ModNPC
    {
        private int shopChoice = 0;
        private static Profiles.StackedNPCProfile NPCProfile;

        // --- Perfil Personalizado para Diálogos y Texturas Normales ---
        // Esta es la clase que necesitábamos desde el principio. Implementa la interfaz
        // ITownNPCProfile para manejar los diálogos y las texturas de forma manual.
        private class MabelProfile : ITownNPCProfile
        {
            private readonly string _rootTexturePath;
            private readonly int _headIndex;

            public MabelProfile(string texture, int head) {
                _rootTexturePath = texture;
                _headIndex = head;
            }

            public int RollVariation() => 0;
            public string GetNameForVariant(NPC npc) => null;

            public Asset<Texture2D> GetTextureNPCShouldUse(NPC npc) {
                if (npc.IsABestiaryIconDummy && npc.altTexture == 0) {
                     return ModContent.Request<Texture2D>(_rootTexturePath);
                }
                if (npc.altTexture == 1) {
                    return ModContent.Request<Texture2D>(_rootTexturePath + "_Party");
                }
                return ModContent.Request<Texture2D>(_rootTexturePath);
            }

            public int GetHeadTextureIndex(NPC npc) => _headIndex;

            // TU CÓDIGO DE DIÁLOGOS, INTACTO Y EN EL LUGAR CORRECTO
            public List<string> GetHappinessLogs()
            {
                var logs = new List<string>();
                var dislikeBiome = new WeightedRandom<string>(Main.rand);
                dislikeBiome.Add("¡Arena! ¡Arena por todas partes! ¡Y este sol abrasador! Mi piel es demasiado delicada para esta tortura.");
                dislikeBiome.Add("Siento que me voy a derretir... ¡Esto es intolerable!");

                var hateBiome = new WeightedRandom<string>(Main.rand);
                hateBiome.Add("¡Bichos! ¡Humedad! ¡Plantas! ¡Este lugar es una pesadilla verde y pegajosa! ¡SÁCAME DE AQUÍ!");
                hateBiome.Add("¡Mi ropa se está arruinando! ¡Y mi cabello! ¡Odio, odio, ODIO este lugar!");
                
                var hateNPC = new WeightedRandom<string>(Main.rand);
                hateNPC.Add($"¡Esa {NPC.GetFirstNPCNameOrNull(NPCID.PartyGirl)} y su confeti! ¡Son ruidosos, molestos y dejan todo sucio! ¡Un completo desastre para la elegancia!");
                hateNPC.Add($"Si escucho '¡Fiesta!' una vez más, voy a congelar su máquina de burbujas. ¡Te lo advierto!");

                logs.Add("Ah, por fin... espacio para respirar. Es... adecuado para alguien de mi estatus.");
                logs.Add("¡Demasiada gente! ¡Esto es una pocilga! ¡Necesito mi espacio personal, plebeyos!");
                logs.Add("El aire helado, el silencio de la nieve... sí, este es mi verdadero hogar. Siento cómo mi poder crece aquí.");
                logs.Add("Los cristales y las luces son... pasables. Le falta la elegancia fría del hielo, pero supongo que es mejor que el lodo.");
                logs.Add(dislikeBiome.Get());
                logs.Add(hateBiome.Get());
                logs.Add($"Esa... {NPC.GetFirstNPCNameOrNull(NPCID.Princess)}... no está mal. Supongo. Tiene algo de dignidad. ¡N-no es que la admire ni nada por el estilo!");
                logs.Add($"Al menos {NPC.GetFirstNPCNameOrNull(NPCID.Wizard)} entiende el arte de la magia. A diferencia de otros bárbaros que solo saben blandir espadas.");
                logs.Add($"¿Por qué {NPC.GetFirstNPCNameOrNull(NPCID.ArmsDealer)} tiene que ser tan... ruidoso? Sus explosiones son vulgares.");
                logs.Add(hateNPC.Get());
                logs.Add("Este lugar es... funcional. Supongo. Carece de la grandeza de mis dominios helados, pero servirá.");
                
                return logs;
            }
        }
        
        // ===== REEMPLAZA TU MÉTODO SetStaticDefaults CON ESTA VERSIÓN FINAL =====

public override void SetStaticDefaults()
{
    Main.npcFrameCount[NPC.type] = 23;
    NPCID.Sets.ExtraFramesCount[NPC.type] = 28;
    NPCID.Sets.AttackFrameCount[NPC.type] = 4;
    NPCID.Sets.DangerDetectRange[NPC.type] = 250;
    NPCID.Sets.AttackType[NPC.type] = 1;
    NPCID.Sets.AttackTime[NPC.type] = 17;
    NPCID.Sets.AttackAverageChance[NPC.type] = 55;
    NPCID.Sets.ShimmerTownTransform[NPC.type] = true;

    NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new() { Velocity = 0.5f };
    NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifiers);

    // 1. Se definen los niveles de afecto (la parte mecánica).
    NPC.Happiness
        .SetBiomeAffection<SnowBiome>(AffectionLevel.Love)
        .SetBiomeAffection<DesertBiome>(AffectionLevel.Hate)
        .SetBiomeAffection<OceanBiome>(AffectionLevel.Dislike)
        .SetBiomeAffection<HallowBiome>(AffectionLevel.Like)
        .SetBiomeAffection<JungleBiome>(AffectionLevel.Hate)
        .SetNPCAffection(NPCID.Wizard, AffectionLevel.Like)
        .SetNPCAffection(NPCID.PartyGirl, AffectionLevel.Hate)
        .SetNPCAffection(NPCID.Princess, AffectionLevel.Love) 
        .SetNPCAffection(NPCID.ArmsDealer, AffectionLevel.Dislike);

    // 2. Se crea el perfil apilado para que Shimmer funcione.
    // Esta es la línea corregida.
    NPCProfile = new Profiles.StackedNPCProfile(
        new MabelProfile(Texture, NPCHeadLoader.GetHeadSlot(HeadTexture)),
        // LA CORRECCIÓN: Construimos la ruta a la cabeza del Shimmer manualmente.
        new Profiles.DefaultNPCProfile(Texture + "_Shimmer", NPCHeadLoader.GetHeadSlot(Texture + "_Shimmer_Head"), Texture + "_Shimmer_Party")
    );
}

        public override ITownNPCProfile TownNPCProfile() => NPCProfile;

        public override void SetDefaults()
        {
            NPC.townNPC = true;
            NPC.friendly = true;
            NPC.width = 18;
            NPC.height = 40;
            NPC.aiStyle = 7;
            NPC.damage = 80;
            NPC.defense = 20;
            NPC.lifeMax = 500;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.3f;
            AnimationType = NPCID.Stylist;
        }

        public override bool CanTownNPCSpawn(int numTownNPCs)
        {
            for (int i = 0; i < Main.maxPlayers; i++) {
                Player player = Main.player[i];
                if (player.active) {
                    foreach (Item item in player.inventory) {
                        if (item.type == ItemID.IceBlock || item.type == ItemID.SnowBlock || NPC.downedBoss1)
                            return true;
                    }
                }
            }
            return false;
        }

        public override List<string> SetNPCNameList() => new() {
            "Mabel", "Princesa del Hielo", "Emperatriz Glacial", "Mabel la Altiva", "Dama de Escarcha", "Su Alteza Helada",
            "Mabel Corazón de Hielo", "La Tsundere de Hielo", "Reina de Invierno", "Mabel la Orgullosa", "Elsa", "Yukino", "Weiss", "Mizore", "Fubuki"
        };
        
        public override string GetChat()
        {
            // CORRECCIÓN: Se añade Main.rand al constructor de WeightedRandom
            WeightedRandom<string> chat = new(Main.rand);
            Player player = Main.LocalPlayer;

            chat.Add("Hmph... no es que me importe tu presencia o algo así.");
            chat.Add("¿Qué quieres? Espero que no sea algo trivial...");
            chat.Add("No malinterpretes mi amabilidad. Solo cumplo con mi deber como comerciante.");
            chat.Add("El hielo refleja la pureza del alma... algo que la mayoría no posee.");
            chat.Add("Mi linaje es noble, no como estos... plebeyos.");
            chat.Add("¿Te has fijado en mi elegancia? Por supuesto que sí.");
            chat.Add("No todos pueden apreciar la belleza del invierno eterno.");
            chat.Add("Suspiro... incluso aquí debo mantener mi compostura real.");
            chat.Add("La magia de hielo requiere un corazón puro... o muy frío.");
            chat.Add("Si me traes algo bonito, tal vez... tal vez sea más amable contigo.");
            chat.Add("¿Crees que soy linda? ¡P-por supuesto que lo soy! ¿Qué clase de pregunta es esa?", 0.8);
            if (player.ZoneJungle) chat.Add("Esta humedad es... desagradable. ¿Cómo pueden vivir así?");
            if (player.ZoneSnow) chat.Add("Finalmente... un lugar digno de mi presencia. ¿Ves? Aquí es donde pertenezco.");
            if (player.statLife < player.statLifeMax2 * 0.25) chat.Add("¡Estás herido! ¡N-no es que me preocupe! Simplemente... sería una molestia tener que buscar un nuevo... cliente. ¡Así que cúrate, idiota!");
            if (Main.bloodMoon) chat.Add("La luna sangrienta... qué dramática. Aunque no tanto como mi entrada real.");
            if (NPC.FindFirstNPC(NPCID.PartyGirl) != -1) chat.Add("Esa chica fiestera es tan... ruidosa. ¿Cómo puede alguien ser tan energética?");

            return chat.Get();
        }

        public override void SetChatButtons(ref string button, ref string button2)
        {
            if (shopChoice == 0) button = "Pociones Selectas";
            else if (shopChoice == 1) button = "Arsenal de Invocación";
            else if (shopChoice == 2) button = "Armas de Precisión";
            else if (shopChoice == 3) button = "Armamento Noble";
            else if (shopChoice == 4) button = "Artes Místicas";
            else { shopChoice = 0; button = "Pociones Selectas"; }
            button2 = "Siguiente categoría";
        }

        public override void OnChatButtonClicked(bool firstButton, ref string shop)
        {
            if (firstButton) {
                if (shopChoice == 0) shop = "Pociones";
                else if (shopChoice == 1) shop = "Invocador";
                else if (shopChoice == 2) shop = "Distancia";
                else if (shopChoice == 3) shop = "Cuerpo a cuerpo";
                else if (shopChoice == 4) shop = "Mago";
            } else {
                shopChoice = (shopChoice + 1) % 5;
            }
        }

                public override void AddShops()
        {
            var dropsShop = new NPCShop(Type, "Pociones")
                .Add(ItemID.GreaterManaPotion)
                .Add(ItemID.SuperManaPotion)
                .Add(ItemID.ObsidianSkinPotion)
                .Add(ItemID.RegenerationPotion)
                .Add(ItemID.SwiftnessPotion)
                .Add(ItemID.IronskinPotion)
                .Add(ItemID.WaterWalkingPotion)
                .Add(ItemID.ArcheryPotion)
                .Add(ItemID.HunterPotion)
                .Add(ItemID.GravitationPotion)
                .Add(ItemID.MiningPotion)
                .Add(ItemID.HeartreachPotion)
                .Add(ItemID.CalmingPotion)
                .Add(ItemID.BuilderPotion)
                .Add(ItemID.TrapsightPotion)
                .Add(ItemID.EndurancePotion)
                .Add(ItemID.LifeforcePotion)
                .Add(ItemID.InfernoPotion)
                .Add(ItemID.WrathPotion)
                .Add(ItemID.RagePotion)
                .Add(ItemID.SummoningPotion)
                .Add(ItemID.TitanPotion)
                .Add(ItemID.FlipperPotion)
                .Add(ItemID.ThornsPotion)
                .Add(ItemID.SpelunkerPotion)
                .Add(ItemID.SonarPotion)
                .Add(ItemID.ShinePotion)
                .Add(ItemID.NightOwlPotion)
                .Add(ItemID.BattlePotion)
                .Add(ItemID.GillsPotion)
                .Add(ItemID.FeatherfallPotion)
                .Add(ItemID.TeleportationPotion)
                .Add(ItemID.WormholePotion)
                .Add(ItemID.AmmoReservationPotion)
                .Add(ItemID.MagicPowerPotion)
                .Add(ItemID.CratePotion)
                .Add(ItemID.FishingPotion)
                .Add(ItemID.GreaterHealingPotion, Condition.Hardmode)
                .Add(ItemID.SuperHealingPotion, Condition.DownedPlantera);
            dropsShop.Register();

            var soulsShop = new NPCShop(Type, "Invocador")
                // Básicos de invocación
                .Add(ItemID.SummoningPotion)
                .Add(ItemID.SlimeStaff) // ¡El famoso Slime Staff!
                .Add(ItemID.BabyBirdStaff)
                .Add(ItemID.FlinxStaff)
                .Add(ItemID.VampireFrogStaff)
                .Add(ItemID.BoneWhip)
               

                // Pre-Hardmode avanzado
                .Add(ItemID.AbigailsFlower)
                .Add(ItemID.HornetStaff)
                .Add(ItemID.ImpStaff)
                
                
                
                .Add(ItemID.ThornWhip)
                .Add(ItemID.BlandWhip)

                // Hardmode temprano
                .Add(ItemID.BewitchingTable, Condition.Hardmode)
                .Add(ItemID.SpiderStaff, Condition.Hardmode)
                .Add(ItemID.QueenSpiderStaff, Condition.Hardmode)
                .Add(ItemID.OpticStaff, Condition.Hardmode)
                .Add(ItemID.PirateStaff, Condition.Hardmode)
                .Add(ItemID.SlimeStaff, Condition.Hardmode)
                
                
                
              
                .Add(ItemID.SanguineStaff, Condition.Hardmode)
                
                .Add(ItemID.CoolWhip, Condition.Hardmode)
                .Add(ItemID.FireWhip, Condition.Hardmode)
                .Add(ItemID.SwordWhip, Condition.Hardmode)
                .Add(ItemID.ScytheWhip, Condition.Hardmode)

                // Post-Plantera
                .Add(ItemID.PygmyStaff, Condition.DownedPlantera)
                .Add(ItemID.RavenStaff, Condition.DownedPlantera)
                .Add(ItemID.DeadlySphereStaff, Condition.DownedPlantera)
                .Add(ItemID.TempestStaff, Condition.DownedPlantera)
                .Add(ItemID.XenoStaff, Condition.DownedPlantera)
                

                // Post-Golem
                .Add(ItemID.StardustCellStaff, Condition.DownedGolem)
                .Add(ItemID.StardustDragonStaff, Condition.DownedGolem)
                .Add(ItemID.RainbowCrystalStaff, Condition.DownedGolem)
                
                .Add(ItemID.EmpressBlade, Condition.DownedGolem)
                
                

                // Moon Lord
                .Add(ItemID.StardustCellStaff, Condition.DownedMoonLord)
                .Add(ItemID.StardustDragonStaff, Condition.DownedMoonLord)
                .Add(ItemID.Terrarian, Condition.DownedMoonLord)

                // Accesorios de invocador
                
                .Add(ItemID.BeeCloak)
                .Add(ItemID.PygmyNecklace, Condition.Hardmode)
                .Add(ItemID.SummonerEmblem, Condition.Hardmode)
                .Add(ItemID.HerculesBeetle, Condition.DownedPlantera)
                .Add(ItemID.PapyrusScarab, Condition.DownedPlantera)
                .Add(ItemID.NecromanticScroll, Condition.DownedPlantera)
                .Add(ItemID.StingerNecklace, Condition.DownedPlantera)
                .Add(ItemID.BerserkerGlove, Condition.DownedGolem)
                .Add(ItemID.AvengerEmblem, Condition.DownedMechBossAll)

                // Items de apoyo
                .Add(ItemID.HermesBoots)
                .Add(ItemID.CloudinaBottle)
                .Add(ItemID.ShinyRedBalloon)
                .Add(ItemID.BundleofBalloons)
                .Add(ItemID.ObsidianShield)
                .Add(ItemID.AnkhCharm, Condition.DownedPlantera)
                .Add(ItemID.FrostsparkBoots, Condition.Hardmode)
                .Add(ItemID.TerrasparkBoots, Condition.DownedMoonLord)
                .Add(ItemID.MagicCuffs)
                .Add(ItemID.MagmaStone)
                .Add(ItemID.SharkToothNecklace)
                .Add(ItemID.BottledHoney, new Condition("Mabel's Favorite", () => true)) // Referencia cute
                .Add(ItemID.BeeGun, new Condition("Royal Bee", () => NPC.downedQueenBee))

                .Add(ItemID.RoyalGel, new Condition("Royal Slime", () => NPC.downedSlimeKing));
            soulsShop.Register();

            var ammoShop = new NPCShop(Type, "Distancia")
                .Add(ItemID.Musket)
                .Add(ItemID.TheUndertaker)
                .Add(ItemID.Minishark)
                .Add(ItemID.Boomstick)
                .Add(ItemID.PhoenixBlaster)
                .Add(ItemID.HellwingBow)
                .Add(ItemID.StarCannon)
                .Add(ItemID.MoltenFury)
                .Add(ItemID.Harpoon)
                .Add(ItemID.Blowpipe)
                .Add(ItemID.Revolver)
                .Add(ItemID.Sandgun)
                .Add(ItemID.Snowball)
                .Add(ItemID.Shotgun, Condition.Hardmode)
                .Add(ItemID.Flamethrower, Condition.Hardmode)
                .Add(ItemID.Uzi, Condition.Hardmode)
                .Add(ItemID.Marrow, Condition.Hardmode)
                .Add(ItemID.OnyxBlaster, Condition.Hardmode)
                .Add(ItemID.Megashark, Condition.Hardmode)
                .Add(ItemID.ClockworkAssaultRifle, Condition.Hardmode)
                .Add(ItemID.DaedalusStormbow, Condition.Hardmode)
                .Add(ItemID.Tsunami, Condition.DownedDukeFishron)
                .Add(ItemID.VortexBeater, Condition.DownedMoonLord)
                .Add(ItemID.Phantasm, Condition.DownedMoonLord)
                .Add(ItemID.MagicQuiver, Condition.DownedPlantera)
                .Add(ItemID.ArcheryPotion)
                .Add(ItemID.IchorArrow, Condition.Hardmode)
                .Add(ItemID.CursedArrow, Condition.Hardmode)
                .Add(ItemID.HolyArrow, Condition.Hardmode)
                .Add(ItemID.EndlessQuiver, Condition.Hardmode)
                .Add(ItemID.EndlessMusketPouch, Condition.Hardmode)
                
                .Add(ItemID.RangerEmblem, Condition.Hardmode)
                
                .Add(ItemID.SilverBullet)
               
                
                .Add(ItemID.NanoBullet, Condition.DownedPlantera)
                .Add(ItemID.ExplodingBullet, Condition.DownedPlantera)
                
                .Add(ItemID.MoonlordBullet, Condition.DownedMoonLord);
            ammoShop.Register();

            var meleeShop = new NPCShop(Type, "Cuerpo a cuerpo")
                .Add(ItemID.EnchantedSword)
                .Add(ItemID.Muramasa)
                .Add(ItemID.FieryGreatsword)
                .Add(ItemID.NightsEdge)
                .Add(ItemID.BeeKeeper)
                .Add(ItemID.Starfury)
                .Add(ItemID.DarkLance)
                .Add(ItemID.Cascade)
                .Add(ItemID.Arkhalis)
                .Add(ItemID.BladeofGrass)

                .Add(ItemID.Excalibur)
                .Add(ItemID.Bladetongue, Condition.Hardmode)
                .Add(ItemID.Cutlass, Condition.Hardmode)
                .Add(ItemID.FetidBaghnakhs, Condition.Hardmode)
                .Add(ItemID.DeathSickle, Condition.Hardmode)
                .Add(ItemID.TrueNightsEdge, Condition.Hardmode)
                .Add(ItemID.TrueExcalibur, Condition.Hardmode)
                .Add(ItemID.TerraBlade, Condition.DownedPlantera)
                .Add(ItemID.InfluxWaver, Condition.DownedMoonLord)
                .Add(ItemID.Meowmere, Condition.DownedMoonLord)
                .Add(ItemID.StarWrath, Condition.DownedMoonLord)
                .Add(ItemID.Spear)

                .Add(ItemID.ThornChakram)
                .Add(ItemID.Rally)
                .Add(ItemID.Amarok, Condition.Hardmode)
                .Add(ItemID.HelFire, Condition.Hardmode)
                .Add(ItemID.Yelets, Condition.Hardmode)
                .Add(ItemID.Terrarian, Condition.DownedMoonLord)
                .Add(ItemID.FeralClaws)
                .Add(ItemID.TitanGlove, Condition.Hardmode)
                .Add(ItemID.FireGauntlet, Condition.DownedGolem)
                .Add(ItemID.WarriorEmblem, Condition.Hardmode)
                .Add(ItemID.MechanicalGlove, Condition.Hardmode)
                .Add(ItemID.PowerGlove)
                .Add(ItemID.BerserkerGlove, Condition.DownedGolem)

                .Add(ItemID.CobaltShield, Condition.Hardmode)
                .Add(ItemID.PaladinsShield, Condition.DownedPlantera)
                .Add(ItemID.AnkhShield, Condition.DownedPlantera)
                .Add(ItemID.FrozenShield, Condition.Hardmode);
                
            meleeShop.Register();

            var magicShop = new NPCShop(Type, "Mago")
                .Add(ItemID.WandofSparking)
                .Add(ItemID.MagicMissile)
                .Add(ItemID.Flamelash)
                .Add(ItemID.WaterBolt)
                .Add(ItemID.Vilethorn)
                .Add(ItemID.CrimsonRod)
                .Add(ItemID.DemonScythe)
                .Add(ItemID.SpaceGun)
                .Add(ItemID.FlowerofFire)
                .Add(ItemID.AquaScepter)
                .Add(ItemID.WeatherPain)
                .Add(ItemID.FrostStaff)
                .Add(ItemID.FlowerofFrost, Condition.Hardmode)
                .Add(ItemID.BookofSkulls, Condition.DownedSkeletron)
                .Add(ItemID.RainbowRod, Condition.Hardmode)
                .Add(ItemID.MagicDagger, Condition.Hardmode)
                .Add(ItemID.CrystalStorm, Condition.Hardmode)
                .Add(ItemID.CursedFlames, Condition.Hardmode)
                .Add(ItemID.GoldenShower, Condition.Hardmode)
                .Add(ItemID.SpellTome, Condition.Hardmode)
                .Add(ItemID.MeteorStaff, Condition.Hardmode)
                .Add(ItemID.SkyFracture, Condition.Hardmode)
                .Add(ItemID.UnholyTrident, Condition.Hardmode)
                .Add(ItemID.NimbusRod, Condition.Hardmode)
                .Add(ItemID.SpectreStaff, Condition.DownedPlantera)
                .Add(ItemID.RazorbladeTyphoon, Condition.DownedPlantera)
                .Add(ItemID.InfernoFork, Condition.DownedPlantera)
                .Add(ItemID.StaffofEarth, Condition.DownedGolem)
                .Add(ItemID.BubbleGun, Condition.DownedDukeFishron)
                .Add(ItemID.LaserMachinegun, Condition.DownedMoonLord)
                .Add(ItemID.LunarFlareBook, Condition.DownedMoonLord)
                .Add(ItemID.MagicCuffs)
                .Add(ItemID.ManaFlower)
                .Add(ItemID.MagnetFlower, Condition.Hardmode)
                .Add(ItemID.CelestialMagnet, Condition.Hardmode)
                .Add(ItemID.CelestialEmblem, Condition.DownedMoonLord)
                .Add(ItemID.SorcererEmblem, Condition.Hardmode)
                .Add(ItemID.CrystalBall, Condition.Hardmode)
                
                .Add(ItemID.MagicPowerPotion)
                
                .Add(ItemID.ManaCrystal);
            magicShop.Register();
        }



        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0) {
                for (int i = 0; i < 15; i++) Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.IceTorch, 2f * hit.HitDirection, -2f);
                for (int i = 0; i < 5; i++) Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Snow, hit.HitDirection, -1f);
                for (int i = 0; i < 3; i++) Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.IceTorch, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f));
            }
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
                new FlavorTextBestiaryInfoElement("Una orgullosa princesa de hielo con actitud tsundere. Vende armas y pociones de la más alta calidad... no es que le importe si las compras o no, ¡baka!")
            });
        }

        public override void TownNPCAttackStrength(ref int damage, ref float knockback)
        {
            if (!Main.hardMode) { damage = 30; knockback = 4f; } else { damage = 75; knockback = 15f; }
        }

        public override void DrawTownAttackGun(ref Texture2D item, ref Rectangle itemFrame, ref float scale, ref int horizontalHoldoutOffset)
        {
            if (Main.hardMode) {
                Main.GetItemDrawFrame(ItemID.FrostStaff, out item, out itemFrame);
                horizontalHoldoutOffset = (int)Main.DrawPlayerItemPos(1f, ItemID.FrostStaff).X - 10;
            } else {
                Main.GetItemDrawFrame(ItemID.MagicMissile, out item, out itemFrame);
                horizontalHoldoutOffset = (int)Main.DrawPlayerItemPos(1f, ItemID.MagicMissile).X - 10;
            }
        }

        public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
        {
            if (!Main.hardMode) { cooldown = 6; randExtraCooldown = 12; } else { cooldown = 12; randExtraCooldown = 20; }
        }

        public override void TownNPCAttackProj(ref int projType, ref int attackDelay)
        {
            if (!Main.hardMode) { projType = ProjectileID.MagicMissile; attackDelay = 2; } else { projType = ProjectileID.FrostBlastFriendly; attackDelay = 3; }
        }

        public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset)
        {
            if (!Main.hardMode) { multiplier = 14f; randomOffset = 0.6f; } else { multiplier = 20f; randomOffset = 0.2f; }
        }

        public override bool CanGoToStatue(bool toKingStatue) => !toKingStatue;
    }
}