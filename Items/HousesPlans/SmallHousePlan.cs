using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace ClassesSellers.Items.HousesPlans
{
    public class SmallHousePlan : ModItem
    {
        private const int SchematicWidth = 41;
        private const int SchematicHeight = 11;

        public override void SetStaticDefaults() { Item.ResearchUnlockCount = 1; }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Tooltip0", "Materializa una Estación de Sanación de alta eficiencia."));
            tooltips.Add(new TooltipLine(Mod, "Tooltip1", "¡Actívala para recibir una lluvia de corazones!"));
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.useTurn = true;
            Item.maxStack = 99;
            Item.value = Item.buyPrice(platinum: 1);
            Item.rare = ItemRarityID.Cyan;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.UseSound = SoundID.Item149;
            Item.consumable = true;
        }

        public override bool? UseItem(Player player)
        {
            int tileX = (int)(Main.MouseWorld.X / 16f) - (SchematicWidth / 2);
            int tileY = (int)(Main.MouseWorld.Y / 16f) - SchematicHeight;

            if (tileX < 20 || tileY < 20 || tileX + SchematicWidth > Main.maxTilesX - 20 || tileY + SchematicHeight > Main.maxTilesY - 20) {
                Main.NewText("Demasiado cerca del borde del mundo.", new Color(255, 100, 100));
                return false;
            }

            BuildHealingStation(tileX, tileY);
            Main.NewText("¡Estación de Sanación desplegada!", new Color(255, 50, 50));
            NetMessage.SendTileSquare(-1, tileX - 2, tileY - 2, SchematicWidth + 4, SchematicHeight + 4);

            return true;
        }

        private void BuildHealingStation(int startX, int startY)
        {
            // Limpiamos el área primero
            for (int x = startX - 2; x < startX + SchematicWidth + 2; x++) {
                for (int y = startY - 2; y < startY + SchematicHeight + 2; y++) {
                    WorldGen.KillTile(x, y, false, false, true);
                    WorldGen.KillWall(x, y);
                }
            }
            
            ushort block = TileID.GrayBrick;
            ushort platform = TileID.Platforms;

            // FASE 1: CONSTRUIR LA CARCASA Y LAS SUPERFICIES
            // Marco exterior y paredes interiores
            for (int x = 0; x < SchematicWidth; x++) {
                for (int y = 0; y < SchematicHeight; y++) {
                    if (x == 0 || x == SchematicWidth - 1 || y == 0 || y == SchematicHeight - 1 || x == 13 || x == 27)
                        WorldGen.PlaceTile(startX + x, startY + y, block);
                }
            }
            // Plataformas
            for (int x = 1; x < 13; x++) { WorldGen.PlaceTile(startX + x, startY + 3, platform); WorldGen.PlaceTile(startX + x, startY + 6, platform); WorldGen.PlaceTile(startX + x, startY + 10, platform); }
            for (int x = 28; x < 40; x++) { WorldGen.PlaceTile(startX + x, startY + 3, platform); WorldGen.PlaceTile(startX + x, startY + 6, platform); WorldGen.PlaceTile(startX + x, startY + 10, platform); }
            for (int x = 14; x < 27; x++) { WorldGen.PlaceTile(startX + x, startY + 10, platform); }
            WorldGen.PlaceTile(startX + 17, startY + 3, platform); WorldGen.PlaceTile(startX + 18, startY + 3, platform);
            WorldGen.PlaceTile(startX + 23, startY + 2, platform); WorldGen.PlaceTile(startX + 24, startY + 2, platform);
            for (int x = startX + 15; x < startX + 26; x++) { WorldGen.PlaceTile(x, startY - 2, block); }

            // FASE 2: COLOCAR OBJETOS Y LÍQUIDOS
            // Buffs de regeneración
            WorldGen.PlaceObject(startX + 17, startY + 2, TileID.Campfire);
            WorldGen.PlaceTile(startX + 20, startY, block, true, true); // Anclaje para la linterna
            WorldGen.PlaceObject(startX + 20, startY + 1, TileID.HangingLanterns, false, 6); // Linterna de corazón
            // Poza de miel
            for (int x = 19; x <= 22; x++) { for (int y = 3; y <= 4; y++) { if(x == 19 || x == 22 || y == 4) WorldGen.PlaceTile(startX + x, startY + y, block); } }
            WorldGen.PlaceLiquid(startX + 20, startY + 4, (byte)LiquidID.Honey, 255);
            
            // Palanca y Temporizador
            WorldGen.PlaceTile(startX + 24, startY + 1, TileID.Lever);
            WorldGen.PlaceTile(startX + 15, startY + 9, TileID.LogicGate, true, false, -1, 1); // Temporizador de 1s sobre el suelo

            // Estatuas (Corazón y decorativas)
            int heartStatueStyle = 6;
            int birdStatueStyle = 0;
            int[] statueX = { 3, 7, 11, 29, 33, 37 };
            int[] statueY = { 2, 5, 8 };
            foreach (int sx in statueX) {
                foreach (int sy in statueY) {
                    // Colocamos la estatua en Y-1 para que su base quede sobre la plataforma
                    WorldGen.PlaceObject(startX + sx, startY + sy, TileID.Statues, false, heartStatueStyle);
                }
            }
            WorldGen.PlaceObject(startX + 1, startY + 1, TileID.Statues, false, birdStatueStyle);
            WorldGen.PlaceObject(startX + SchematicWidth - 2, startY + 1, TileID.Statues, false, birdStatueStyle);
            WorldGen.PlaceObject(startX + 1, startY + SchematicHeight - 2, TileID.Statues, false, birdStatueStyle);
            WorldGen.PlaceObject(startX + SchematicWidth - 2, startY + SchematicHeight - 2, TileID.Statues, false, birdStatueStyle);
            WorldGen.PlaceObject(startX + 17, startY - 3, TileID.Statues, false, birdStatueStyle);
            WorldGen.PlaceObject(startX + 23, startY - 3, TileID.Statues, false, birdStatueStyle);


            // FASE 3: CABLEADO FINAL
            // Usamos colores para que coincida con tu plano
            
            // Cable Amarillo: Palanca al Temporizador
            for (int y = startY + 1; y <= startY + 9; y++) WorldGen.PlaceWire(startX + 24, y);
            for (int x = 24; x >= 15; x--) WorldGen.PlaceWire(startX + x, startY + 9);
            
            // Cable Azul: Temporizador a las estatuas
            for (int y = startY + 9; y >= startY + 3; y--) WorldGen.PlaceWire(startX + 15, y);
            for (int x = 15; x >= 3; x--) WorldGen.PlaceWire(startX + x, startY + 3);
            for (int x = 15; x <= 37; x++) WorldGen.PlaceWire(startX + x, startY + 3);
            foreach(int sx in new int[]{3, 7, 11, 29, 33, 37}) WorldGen.PlaceWire(startX + sx, startY + 3);

            // Cable Verde: Conexiones verticales
            foreach(int sx in new int[]{3, 7, 11}) { for(int y = 3; y <= 9; y++) WorldGen.PlaceWire3(startX + sx, startY + y); }
            foreach(int sx in new int[]{29, 33, 37}) { for(int y = 3; y <= 9; y++) WorldGen.PlaceWire3(startX + sx, startY + y); }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.GrayBrick, 250)
                .AddIngredient(ItemID.HeartStatue, 1)
                .AddIngredient(ItemID.Wire, 200) // Más cable
                .AddIngredient(ItemID.Timer1Second, 1)
                .AddIngredient(ItemID.Campfire, 1)
                .AddIngredient(ItemID.HoneyBucket, 1)
                .AddIngredient(ItemID.HeartLantern, 1)
                .AddTile(TileID.HeavyWorkBench)
                .Register();
        }
    }
}