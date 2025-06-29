using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.Localization;
using Microsoft.Xna.Framework;

// Usamos el mismo namespace para todas las clases en este archivo
namespace ClassesSellers.Items.Consumables.FrutaDelCataclismo // Puedes llamarlo como quieras
{
    
    #region el item
    public class FrutaDelCataclismo : ModItem
    {
        public override void SetStaticDefaults()
        {
            // En tModLoader 1.4.4+ NO usamos SetDefault()
            // El nombre y descripción se definen en archivos .hjson o se auto-generan
            
            // Para que brille en el inventario
            ItemID.Sets.ItemIconPulse[Item.type] = true;
            ItemID.Sets.ItemNoGravity[Item.type] = true;
        }

        // Método para definir el nombre manualmente (opcional)
        public override LocalizedText DisplayName => Language.GetOrRegister("Content/Items/FrutaDelCataclismo", () => "Fruta del Cataclismo");
        
        // Método para definir el tooltip manualmente (opcional)  
        public override LocalizedText Tooltip => Language.GetOrRegister("Content/Items/FrutaDelCataclismo", () => "¡El poder de un dragón corre por tus venas!");

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.EatFood;
            Item.maxStack = 30;
            Item.consumable = true;
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Lime;

            // Le decimos al ítem qué buff debe aplicar y por cuánto tiempo.
            // Apunta a la clase "FuriaCataclismicaBuff" que está más abajo en este mismo archivo.
            Item.buffType = ModContent.BuffType<FuriaCataclismicaBuff>();
            Item.buffTime = 60 * 60 * 5; // 5 minutos
        }

        public override bool? UseItem(Player player)
        {
            // Sonido al consumir
            SoundEngine.PlaySound(SoundID.Item4, player.position);
            return true;
        }
    } 
    #endregion

    
    #region Buff efecto de la fruta

    public class FuriaCataclismicaBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            // En tModLoader 1.4.4+ NO usamos SetDefault() para buffs tampoco
            Main.debuff[Type] = false; // Es un buff, no un debuff
            Main.buffNoSave[Type] = true; // Se guarda al salir del mundo
        }

        // Método para definir el nombre del buff manualmente (opcional)
        public override LocalizedText DisplayName => Language.GetOrRegister("Mods.ClassesSellers.Buffs.FuriaCataclismicaBuff.DisplayName", () => "Furia Cataclísmica");
        
        // Método para definir la descripción del buff manualmente (opcional)
        public override LocalizedText Description => Language.GetOrRegister("Mods.ClassesSellers.Buffs.FuriaCataclismicaBuff.Description", () => "El poder del dragón fluye por tu ser");

        public override void Update(Player player, ref int buffIndex)
        {
            // --- APLICAR LOS EFECTOS ---
            player.GetDamage(DamageClass.Generic) += 0.20f; // +20% daño
            player.GetAttackSpeed(DamageClass.Generic) += 0.15f; // +15% velocidad de ataque
            player.GetCritChance(DamageClass.Generic) += 10; // +10% de crítico
            player.statDefense += 12; // +12 defensa
            player.moveSpeed += 0.15f; // +15% velocidad de movimiento
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Frostburn] = true;

            // Le decimos al ModPlayer que el buff está activo.
            player.GetModPlayer<FuriaCataclismicaPlayer>().hasFuriaBuff = true;
        }
    }
    #endregion
    
    #region Efecto Especial explosion elemental
    public class FuriaCataclismicaPlayer : ModPlayer
    {
        // Esta variable solo sirve para saber si el buff está activo.
        public bool hasFuriaBuff;

        // Se llama cada tick para resetear el estado
        public override void ResetEffects()
        {
            hasFuriaBuff = false;
        }

        // Se llama cuando el jugador golpea a un NPC
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Si el jugador tiene el buff activo...
            if (hasFuriaBuff)
            {
                // ...hay un 10% de probabilidad de crear una explosión.
                if (Main.rand.NextBool(10))
                {
                    // Elige un elemento al azar (fuego o hielo)
                    int explosionType = Main.rand.NextBool() ? ProjectileID.InfernoFriendlyBlast : ProjectileID.Blizzard;

                    // Crea el proyectil de explosión en el centro del enemigo golpeado
                    Projectile.NewProjectile(Player.GetSource_OnHit(target), target.Center, Vector2.Zero,
                        explosionType, hit.Damage / 4, 0, Player.whoAmI);
                }
            }
        }
    }
    #endregion
}