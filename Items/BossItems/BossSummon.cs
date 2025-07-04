using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.Localization;
using Terraria.Chat;

namespace ClassesSellers.Items.BossItems
{
    public class CristalDraconico : ModItem
    {
        // Método para definir el nombre del item
        public override LocalizedText DisplayName => Language.GetOrRegister("Mods.ClassesSellers.Items.CristalDraconico.DisplayName", () => "Cristal Draconico");

        // Método para definir el tooltip
        public override LocalizedText Tooltip => Language.GetOrRegister("Mods.ClassesSellers.Items.CristalDraconico.Tooltip", () => "Invoca al Dragón Elemental\nSolo puede usarse durante la noche\n'Las energías de fuego y hielo se arremolinan...'");

        public override void SetStaticDefaults()
        {
            // Para Creative Mode
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 3;

            // Hacer que aparezca en la categoría de boss items (sin forzar brillo)
            ItemID.Sets.SortingPriorityBossSpawns[Type] = 12;

            // Desactivar efectos automáticos no deseados
            ItemID.Sets.ItemIconPulse[Type] = false; // Sin pulso automático
            ItemID.Sets.ItemNoGravity[Type] = false; // Sin flotación automática
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 45;
            Item.useAnimation = 45;
            Item.shoot = ProjectileID.None; // Sin proyectil automático
            Item.shootSpeed = 0f;
            Item.useStyle = ItemUseStyleID.HoldUp; // Como el ojo de cthulhu
            Item.consumable = true;
            Item.rare = ItemRarityID.Orange; // Rareza naranja (como otros boss items)
            Item.UseSound = null; // Sin sonido al usar el item - solo cuando aparezca el boss
            Item.value = Item.sellPrice(0, 2, 0, 0); // 2 gold
            Item.maxStack = 20;

            // Mini cooldown - no se puede usar inmediatamente después
            Item.reuseDelay = 60; // 1 segundo de cooldown (60 = 1 segundo)

            // Quitar efectos automáticos no deseados
            Item.noUseGraphic = false; // Mantener la animación del jugador
        }

        public override bool CanUseItem(Player player)
        {
            // Solo se puede usar durante la noche (corregido según tu anotación)
            if (!Main.dayTime)
            {
                // Verificar que el boss no esté ya activo
                if (!NPC.AnyNPCs(ModContent.NPCType<Content.NPCs.Cryoboros.Cryoboros>()))
                {
                    return true;
                }
                else
                {
                    Main.NewText("El Dragón Elemental ya está presente...", Color.Red);
                    return false;
                }
            }
            else
            {
                Main.NewText("El Dragón Elemental solo aparece durante la noche", Color.Yellow);
                return false;
            }
        }

        public override bool? UseItem(Player player)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                // Mensaje dramático inicial
                Main.NewText("Las energías se agitan en el aire...", Color.Purple);

                // Spawnar el boss con delay - más épico
                Vector2 spawnPos = player.Center + new Vector2(0, -400f); // Más alto para entrada dramática

                // Efectos pre-spawn (antes de que aparezca)
                for (int i = 0; i < 30; i++)
                {
                    Vector2 dustPos = spawnPos + Main.rand.NextVector2Circular(150f, 150f);
                    Dust preFire = Dust.NewDustDirect(dustPos, 0, 0, DustID.Torch, Scale: 1.5f);
                    preFire.noGravity = true;
                    preFire.velocity = (spawnPos - dustPos) * 0.05f;

                    Dust preIce = Dust.NewDustDirect(dustPos, 0, 0, DustID.Ice, Scale: 1.5f);
                    preIce.noGravity = true;
                    preIce.velocity = (spawnPos - dustPos) * 0.05f;
                }

                // Sonido de preparación
                SoundEngine.PlaySound(SoundID.Item73, spawnPos);

                // Delay para crear expectación (2 segundos)
                System.Threading.Tasks.Task.Delay(3000).ContinueWith(t =>
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        // Mensaje de aparición
                        Main.NewText("¡El Dragón Elemental ha despertado!", Color.OrangeRed);

                        // Ahora sí spawnar el boss
                        int bossType = ModContent.NPCType<Content.NPCs.Cryoboros.Cryoboros>();
                        int boss = NPC.NewNPC(player.GetSource_ItemUse(Item), (int)spawnPos.X, (int)spawnPos.Y, bossType);

                        // Efectos épicos al aparecer
                        if (boss < Main.maxNPCs)
                        {
                            // Efectos de aparición más intensos
                            for (int i = 0; i < 80; i++)
                            {
                                Vector2 dustPos = spawnPos + Main.rand.NextVector2Circular(200f, 200f);
                                Dust fire = Dust.NewDustDirect(dustPos, 0, 0, DustID.Torch, Scale: 2.5f);
                                fire.noGravity = true;
                                fire.velocity = (spawnPos - dustPos) * 0.15f;

                                Dust ice = Dust.NewDustDirect(dustPos, 0, 0, DustID.Ice, Scale: 2.5f);
                                ice.noGravity = true;
                                ice.velocity = (spawnPos - dustPos) * 0.15f;
                            }

                            // Sonido épico de aparición
                            SoundEngine.PlaySound(SoundID.Item74, spawnPos);
                            SoundEngine.PlaySound(SoundID.Roar, spawnPos);

                            // Sincronizar en multiplayer
                            if (Main.netMode == NetmodeID.Server)
                            {
                                ChatHelper.BroadcastChatMessage(
                                    NetworkText.FromLiteral("¡El Dragón Elemental ha despertado!"),
                                    Color.OrangeRed
                                );
                            }
                        }
                    }
                });
            }

            return true;
        }

        // Receta para craftear el ítem
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Obsidian, 10)      // 10 obsidiana
                .AddIngredient(ItemID.IceBlock, 10)      // 10 hielo
                .AddIngredient(ItemID.Ruby, 1)           // 1 rubí
                .AddIngredient(ItemID.Sapphire, 1)       // 1 zafiro
                .AddIngredient(ItemID.FallenStar, 3)     // 3 estrellas caídas
                .AddTile(TileID.Anvils)                  // En un yunque
                .Register();
        }
    }
}



