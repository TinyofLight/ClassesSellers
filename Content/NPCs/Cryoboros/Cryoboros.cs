using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.Bestiary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassesSellers.Content.NPCs.Cryoboros
{
    [AutoloadBossHead]
    public class Cryoboros : ModNPC

    {


      
        private int currentPhase = 1;
        private int teleportLocationX = 0;
       // public override string Texture => "ClassesSellers/NPCs/Cryoboros/CryoborosPhase1";
        public static readonly SoundStyle HitSound = new("ClassesSellers/NPCs/Cryoboros/hit", 2);
        public static readonly SoundStyle TransitionSound = new("ClassesSellers/NPCs/Cryoboros/Cryoboros_Phase");
        public static readonly SoundStyle DeathSound = new("ClassesSellers/NPCs/Cryoboros/Cryoboros_Death.ogg");
        
        #region Variables de IA y Sincronización
        private int attackPhase = 0; // Estado actual del ataque
        private int attackTimer = 0; // Timer para ataques
        private int phaseTransitionTimer = 0; // Timer para transiciones
        private int currentStance = 0; // 0 para Hielo, 1 para Fuego
        private int attackCycleCounter = 0; // Contador de ciclos de ataque
        private bool isEnraged = false; // Estado de enfurecimiento
        private bool hasEnteredSecondPhase = false; // Fase 2 activada
        private bool inMajorTransition = false; // Transición épica de fase
        private Vector2 targetPosition; // Posición objetivo para movimiento
        private int chargeDirection = 1; // Dirección de carga
        private int projectileCount = 0; // Contador de proyectiles disparados
        private int maxAttacksInStance = 3; // Máximo de ataques antes de cambiar stance
        private float idleCircleAngle = 0f; // Ángulo para movimiento circular en idle
        private Vector2 idleOffset; // Offset para movimiento orgánico

        // Referencias AI de Terraria
        private ref float AI_Timer => ref NPC.ai[0];
        private ref float AI_Phase => ref NPC.ai[1];
        private ref float AI_AttackType => ref NPC.ai[2];
        private ref float AI_Stance => ref NPC.ai[3];
        #endregion

        #region Configuración y Estándares
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 6; // 6 frames de animación
            NPCID.Sets.TrailCacheLength[NPC.type] = 10;
            NPCID.Sets.TrailingMode[NPC.type] = 1;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.MPAllowedEnemies[Type] = true;
        }

        public override void SetDefaults()
        {
            NPC.width = 120;
            NPC.height = 80;
            NPC.damage = 85;
            NPC.defense = 25;
            NPC.lifeMax = 28000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath14;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = Item.buyPrice(gold: 15);
            NPC.SpawnWithHigherTime(30);
            NPC.boss = true;
            NPC.npcSlots = 10f;
            NPC.aiStyle = -1; // IA personalizada
            NPC.coldDamage = true;
            NPC.HitSound = HitSound;
            NPC.DeathSound = DeathSound;
            if (!Main.dedServ)
            {
                Music = MusicLoader.GetMusicSlot(Mod, "ClassesSellers/NPCs/Cryoboros/BossMusic"); //cancion personalizada para el boss
            }
        }

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * balance);
            NPC.damage = (int)(NPC.damage * bossAdjustment);
        }

        // =================================================================================
        // MÉTODO PARA CONDICIÓN DE APARICIÓN NATURAL (DESPUÉS DEL MURO DE CARNE)
        // =================================================================================
        public override float SpawnChance(NPCSpawnInfo spawnInfo)

        {
            // Ejemplo: Aparece durante una Ventisca en el bioma de Nieve, en la superficie,
            // después de derrotar al Wall of Flesh, y solo si no hay otra instancia de este jefe activa.
            if (Main.hardMode && // NUEVA CONDICIÓN: Solo en Hardmode (después del Wall of Flesh)
                spawnInfo.Player.ZoneSnow &&
                Main.raining && // Ventisca (lluvia en bioma de nieve)
                spawnInfo.Player.ZoneOverworldHeight)
            {
                // Comprueba si ya existe una instancia de este jefe en el mundo.
                // 'Type' es el identificador numérico del tipo de este NPC.
                bool bossAlreadySpawned = NPC.AnyNPCs(Type);

                if (!bossAlreadySpawned)
                {
                    // Devuelve la probabilidad de aparición. 
                    // 0.05f es un 5% de probabilidad en cada "tick" de spawn que cumpla las condiciones.
                    // Para un jefe, incluso si es un mini-jefe de evento, esta probabilidad debe ser
                    // cuidadosamente ajustada para que no sea ni muy raro ni muy común.
                    // Si este es un JEFE PRINCIPAL, es MUY RECOMENDABLE usar un ÍTEM INVOCADOR en lugar de SpawnChance.
                    return 0.05f;
                }
                //else
                //{
                //    this.LogMessage($"Intento de spawn para {DisplayName} abortado, jefe ya existe."); // Para depuración
                //}
            }
            return 0f; // No aparece en otras condiciones.
        }


        #endregion

        #region Sincronización Multijugador/ Spawn condition
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(currentStance);
            writer.Write(attackCycleCounter);
            writer.Write(isEnraged);
            writer.Write(hasEnteredSecondPhase);
            writer.Write(inMajorTransition);
            writer.Write(idleCircleAngle);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            currentStance = reader.ReadInt32();
            attackCycleCounter = reader.ReadInt32();
            isEnraged = reader.ReadBoolean();
            hasEnteredSecondPhase = reader.ReadBoolean();
            inMajorTransition = reader.ReadBoolean();
            idleCircleAngle = reader.ReadSingle();
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
        // Ruta corregida para la etiqueta del evento:
        BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Snow, 
        // Si quieres indicar que es un jefe de Hardmode:
        //SpawnConditionBestiaryInfoElement("Mods.ClassesSellers.Bestiary.SpawnConditions.Hardmode"),
        // Si quieres indicar que aparece de noche:
        BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
        new FlavorTextBestiaryInfoElement("Un antiguo dragón que domina tanto el hielo como el fuego. Sus emociones cambian su elemento, volviéndolo impredecible en batalla. Solo aparece en Hardmode durante las ventiscas más feroces.")
    });

        }
        #endregion

        #region IA Principal
        public override void AI()
        {
            // Encontrar jugador objetivo
            Player target = Main.player[NPC.target];
            if (!target.active || target.dead)
            {
                NPC.TargetClosest();
                target = Main.player[NPC.target];
                if (!target.active || target.dead)
                {
                    AI_Despawn();
                    return;
                }
            }

            // Verificar transición de fase
            CheckPhaseTransition();

            // Actualizar estado de enfurecimiento
            UpdateEnrageStatus(target);

            // Sincronizar variables
            AI_Stance = currentStance;
            AI_Timer = attackTimer;

            // Máquina de estados principal
            switch (attackPhase)
            {
                case 0: // Estado de pausa y selección
                    AI_State_Idle(target);
                    break;
                case 1: // Embestidas
                    AI_State_Charge(target);
                    break;
                case 2: // Ataques a distancia
                    AI_State_RangedAttack(target);
                    break;
                case 3: // Ataque especial circular
                    AI_State_CircularAttack(target);
                    break;
                case 4: // Transición de stance
                    AI_State_StanceTransition(target);
                    break;
                case 5: // Ataque de vórtices (Fase 2)
                    AI_State_VortexAttack(target);
                    break;
                case 99: // Transición épica de fase
                    AI_State_MajorPhaseTransition(target);
                    break;
            }

            attackTimer++;
        }

        private void AI_Despawn()
        {
            NPC.velocity.Y -= 0.04f;
            if (NPC.timeLeft > 10)
                NPC.timeLeft = 10;
        }
        #endregion

        #region Estados de IA
        private void AI_State_Idle(Player target)
        {
            // Hacer que el NPC siempre mire al jugador
            if (target.Center.X < NPC.Center.X)
                NPC.direction = -1;
            else
                NPC.direction = 1;

            // Movimiento orgánico - crear un patrón de figura-8 o círculos pequeños
            idleCircleAngle += 0.03f * (hasEnteredSecondPhase ? 1.5f : 1f);

            // Crear movimiento en forma de 8 para sentirse más vivo
            float horizontalOffset = (float)Math.Sin(idleCircleAngle) * 80f;
            float verticalOffset = (float)Math.Sin(idleCircleAngle * 2f) * 40f;

            // Posición base relativa al jugador
            Vector2 basePosition = target.Center + new Vector2(250f * NPC.direction, -200f);
            Vector2 targetPos = basePosition + new Vector2(horizontalOffset, verticalOffset);

            // Movimiento suave hacia la posición objetivo
            Vector2 direction = targetPos - NPC.Center;
            float distance = direction.Length();

            if (distance > 30f)
            {
                direction.Normalize();
                NPC.velocity = Vector2.Lerp(NPC.velocity, direction * 6f, 0.1f);
            }
            else
            {
                NPC.velocity *= 0.95f;
            }

            // Pequeños movimientos adicionales para parecer más vivo
            if (attackTimer % 120 == 0)
            {
                // Cambio ocasional de lado
                if (Main.rand.NextBool(3))
                {
                    NPC.direction *= -1;
                }
            }

            // Efectos de partículas ambientales
            if (attackTimer % 30 == 0)
            {
                CreateAmbientParticles();
            }

            if (attackTimer >= 60) // 1 segundo de pausa
            {
                // Elegir próximo ataque basado en la distancia al jugador y fase
                float distanceToPlayer = Vector2.Distance(NPC.Center, target.Center);

                if (hasEnteredSecondPhase && Main.rand.NextBool(4)) // 25% chance en fase 2
                {
                    attackPhase = 5; // Ataque de vórtices
                }
                else if (distanceToPlayer > 400f)
                {
                    attackPhase = 2; // Ataque a distancia
                }
                else if (Main.rand.NextBool(2))
                {
                    attackPhase = 1; // Embestida
                }
                else
                {
                    attackPhase = 3; // Ataque circular
                }

                attackTimer = 0;
                projectileCount = 0;
                NPC.netUpdate = true;
            }
        }

        private void AI_State_Charge(Player target)
        {
            if (attackTimer == 1)
            {
                // Calcular dirección de carga
                Vector2 chargeDirection = (target.Center - NPC.Center);
                chargeDirection.Normalize();
                NPC.velocity = chargeDirection * 16f;

                // Sonido de carga
                SoundEngine.PlaySound(SoundID.Item74, NPC.Center);
            }

            // Desacelerar gradualmente
            if (attackTimer > 30)
            {
                NPC.velocity *= 0.95f;
            }

            // Crear efectos de partículas según el stance
            if (attackTimer % 5 == 0)
            {
                CreateStanceParticles(NPC.Center, currentStance);
            }

            // Finalizar ataque
            if (attackTimer >= 80)
            {
                FinishAttack();
            }
        }

        private void AI_State_RangedAttack(Player target)
        {
            // Mantener distancia del jugador
            Vector2 targetPos = target.Center + new Vector2(300f * (NPC.Center.X < target.Center.X ? -1 : 1), -150);
            Vector2 direction = targetPos - NPC.Center;
            direction.Normalize();
            NPC.velocity = (NPC.velocity * 8f + direction * 6f) / 9f;

            // Disparar proyectiles
            if (attackTimer % 20 == 0 && projectileCount < 5)
            {
                if (currentStance == 0) // Hielo
                {
                    SpawnProjectiles_IceShards(target);
                }
                else // Fuego
                {
                    SpawnProjectiles_Fireballs(target);
                }
                projectileCount++;
            }

            if (attackTimer >= 120)
            {
                FinishAttack();
            }
        }

        private void AI_State_CircularAttack(Player target)
        {
            // Moverse en círculo alrededor del jugador
            float radius = 250f;
            float speed = 0.08f * (hasEnteredSecondPhase ? 1.5f : 1f);

            Vector2 center = target.Center;
            float angle = (attackTimer * speed) + (float)(Main.rand.NextDouble() * MathHelper.TwoPi);

            targetPosition = center + new Vector2(
                (float)Math.Cos(angle) * radius,
                (float)Math.Sin(angle) * radius - 100f
            );

            Vector2 moveDirection = targetPosition - NPC.Center;
            NPC.velocity = moveDirection * 0.08f;

            // Disparar proyectiles mientras gira
            if (attackTimer % 15 == 0)
            {
                Vector2 shootDirection = (target.Center - NPC.Center);
                shootDirection.Normalize();

                int projectileType = currentStance == 0 ? ProjectileID.FrostBeam : ProjectileID.Fireball;
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootDirection * 8f,
                    projectileType, NPC.damage / 4, 0f);
            }

            if (attackTimer >= 180)
            {
                FinishAttack();
            }
        }

        private void AI_State_StanceTransition(Player target)
        {
            // Ralentizar movimiento durante transición
            NPC.velocity *= 0.9f;

            // Efectos visuales de transición
            if (attackTimer % 10 == 0)
            {
                CreateTransitionEffect();
            }

            // Cambiar stance a la mitad de la transición
            if (attackTimer == 30)
            {
                currentStance = 1 - currentStance; // Cambiar entre 0 y 1
                attackCycleCounter = 0;

                // Sonido de transformación
                SoundEngine.PlaySound(currentStance == 0 ? SoundID.Item28 : SoundID.Item74, NPC.Center);
            }

            if (attackTimer >= 60)
            {
                attackPhase = 0; // Volver a idle
                attackTimer = 0;
                NPC.netUpdate = true;
            }
        }

        private void AI_State_VortexAttack(Player target)
        {
            // Moverse al centro de la arena
            Vector2 centerPosition = target.Center + new Vector2(0, -300f);
            Vector2 direction = centerPosition - NPC.Center;

            if (direction.Length() > 50f)
            {
                direction.Normalize();
                NPC.velocity = direction * 4f;
            }
            else
            {
                NPC.velocity *= 0.9f;
            }

            // Crear vórtices duales
            if (attackTimer == 60)
            {
                // Crear vórtice de hielo a la izquierda
                Vector2 iceVortexPos = target.Center + new Vector2(-200f, 0);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), iceVortexPos, Vector2.Zero,
                    ProjectileID.FrostBeam, NPC.damage / 3, 0f); // Placeholder - usar proyectil personalizado

                // Crear vórtice de fuego a la derecha  
                Vector2 fireVortexPos = target.Center + new Vector2(200f, 0);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), fireVortexPos, Vector2.Zero,
                    ProjectileID.Fireball, NPC.damage / 3, 0f); // Placeholder - usar proyectil personalizado

                SoundEngine.PlaySound(SoundID.Item62, NPC.Center); // Sonido épico
            }

            // Efectos visuales durante el ataque
            if (attackTimer > 60 && attackTimer % 10 == 0)
            {
                CreateDualElementExplosion(target.Center + new Vector2(-200f, 0), 0); // Hielo
                CreateDualElementExplosion(target.Center + new Vector2(200f, 0), 1);  // Fuego
            }

            if (attackTimer >= 240) // 4 segundos de duración
            {
                FinishAttack();
            }
        }

        private void AI_State_MajorPhaseTransition(Player target)
        {
            // Fase 1: Moverse al centro (primeros 60 ticks)
            if (attackTimer < 60)
            {
                Vector2 centerPos = target.Center + new Vector2(0, -250f);
                Vector2 direction = centerPos - NPC.Center;

                if (direction.Length() > 30f)
                {
                    direction.Normalize();
                    NPC.velocity = direction * 8f;
                }
                else
                {
                    NPC.velocity *= 0.8f;
                }

                // Volverse invulnerable
                NPC.dontTakeDamage = true;
            }
            // Fase 2: Rugido épico y efectos (ticks 60-120)
            else if (attackTimer == 60)
            {
                // Rugido épico
                SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
                SoundEngine.PlaySound(SoundID.Item14, NPC.Center); // Sonido adicional dramático

                // Parar completamente
                NPC.velocity = Vector2.Zero;
            }
            // Fase 3: Explosión de partículas (ticks 60-120)
            else if (attackTimer > 60 && attackTimer < 120)
            {
                // Efectos visuales masivos cada 5 ticks
                if (attackTimer % 5 == 0)
                {
                    CreateMajorTransitionExplosion();
                }

                // Temblor de pantalla (si está disponible)
                if (attackTimer % 10 == 0)
                {
                    // Main.NewText("¡El dragón está entrando en su segunda fase!", Color.OrangeRed);
                }
            }
            // Fase 4: Finalizar transición
            else if (attackTimer >= 120)
            {
                // Activar segunda fase
                if (!hasEnteredSecondPhase)
                {
                    hasEnteredSecondPhase = true;
                    maxAttacksInStance = 2; // Cambios de stance más frecuentes
                    currentStance = 1 - currentStance; // Cambiar stance también
                }

                // Restaurar vulnerabilidad
                NPC.dontTakeDamage = false;
                inMajorTransition = false;

                // Volver al estado idle
                attackPhase = 0;
                attackTimer = 0;
                NPC.netUpdate = true;
            }
        }
        #endregion

        #region Métodos de Ayuda
        private void SpawnProjectiles_IceShards(Player target)
        {
            Vector2 direction = (target.Center - NPC.Center);
            direction.Normalize();

            // Disparar 3 fragmentos de hielo en abanico
            for (int i = -1; i <= 1; i++)
            {
                Vector2 shootVel = direction.RotatedBy(MathHelper.ToRadians(15 * i)) * 10f;
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootVel,
                    ProjectileID.FrostBeam, NPC.damage / 3, 0f);
            }

            SoundEngine.PlaySound(SoundID.Item28, NPC.Center);
        }

        private void SpawnProjectiles_Fireballs(Player target)
        {
            Vector2 direction = (target.Center - NPC.Center);
            direction.Normalize();
            direction *= 12f;

            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, direction,
                ProjectileID.Fireball, NPC.damage / 3, 0f);

            SoundEngine.PlaySound(SoundID.Item34, NPC.Center);
        }

        private void CreateStanceParticles(Vector2 position, int stance)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(3f, 3f);
                int dustType = stance == 0 ? DustID.Ice : DustID.Torch;
                Dust.NewDust(position, 0, 0, dustType, dustVel.X, dustVel.Y, Scale: 1.2f);
            }
        }

        private void CreateTransitionEffect()
        {
            // Crear efectos de ambos elementos durante la transición
            for (int i = 0; i < 5; i++)
            {
                Vector2 dustPos = NPC.Center + Main.rand.NextVector2Circular(60f, 60f);
                Dust.NewDust(dustPos, 0, 0, DustID.Ice, Scale: 1.5f);
                Dust.NewDust(dustPos, 0, 0, DustID.Torch, Scale: 1.5f);
            }
        }

        private void CreateAmbientParticles()
        {
            // Partículas ambientales sutiles según el stance actual
            int dustType = currentStance == 0 ? DustID.Ice : DustID.Torch;
            Vector2 dustPos = NPC.Center + Main.rand.NextVector2Circular(40f, 40f);
            Dust ambientDust = Dust.NewDustDirect(dustPos, 0, 0, dustType, Scale: 0.8f);
            ambientDust.velocity = Main.rand.NextVector2Circular(1f, 1f);
            ambientDust.alpha = 100;
        }

        private void CreateDualElementExplosion(Vector2 position, int element)
        {
            // Crear explosión de partículas específica del elemento
            int dustType = element == 0 ? DustID.Ice : DustID.Torch;
            Color dustColor = element == 0 ? Color.Cyan : Color.OrangeRed;

            for (int i = 0; i < 8; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(8f, 8f);
                Dust explosionDust = Dust.NewDustDirect(position, 0, 0, dustType, velocity.X, velocity.Y, Scale: 1.8f);
                explosionDust.color = dustColor;
            }
        }

        private void CreateMajorTransitionExplosion()
        {
            // Efectos masivos para la transición de fase
            for (int i = 0; i < 15; i++)
            {
                Vector2 dustPos = NPC.Center + Main.rand.NextVector2Circular(120f, 120f);
                Vector2 dustVel = Main.rand.NextVector2Circular(12f, 12f);

                // Alternar entre hielo y fuego
                int dustType = i % 2 == 0 ? DustID.Ice : DustID.Torch;
                Dust explosionDust = Dust.NewDustDirect(dustPos, 0, 0, dustType, dustVel.X, dustVel.Y, Scale: 2.5f);
                explosionDust.noGravity = true;
            }

            // Crear algunos efectos de humo adicionales
            for (int i = 0; i < 5; i++)
            {
                Vector2 smokePos = NPC.Center + Main.rand.NextVector2Circular(80f, 80f);
                Dust.NewDust(smokePos, 0, 0, DustID.Smoke, Scale: 3f);
            }
        }

        private void CheckPhaseTransition()
        {
            // Entrar en segunda fase al 50% de vida
            if (NPC.life < NPC.lifeMax * 0.5f && !hasEnteredSecondPhase && !inMajorTransition)
            {
                // Iniciar transición épica
                attackPhase = 99; // Estado de transición mayor
                attackTimer = 0;
                inMajorTransition = true;
                NPC.netUpdate = true;
            }
        }

        private void UpdateEnrageStatus(Player target)
        {
            // Enfurecerse si el jugador está muy lejos
            float distance = Vector2.Distance(NPC.Center, target.Center);
            isEnraged = distance > 1000f;

            if (isEnraged)
            {
                NPC.damage = (int)(NPC.defDamage * 1.5f);
                // Crear polvo de enfado
                if (Main.rand.NextBool(10))
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Smoke, Scale: 2f);
                }
            }
            else
            {
                NPC.damage = NPC.defDamage;
            }
        }

        private void FinishAttack()
        {
            attackCycleCounter++;

            // Cambiar stance después de varios ataques
            if (attackCycleCounter >= maxAttacksInStance)
            {
                attackPhase = 4; // Transición de stance
            }
            else
            {
                attackPhase = 0; // Volver a idle
            }

            attackTimer = 0;
            NPC.netUpdate = true;
        }
        #endregion

        #region Efectos Visuales y Sonido
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[Type].Value;
            Vector2 drawPos = NPC.Center - screenPos;

            // Cambiar color según el stance
            if (currentStance == 0) // Hielo
            {
                drawColor = Color.Lerp(drawColor, Color.LightBlue , 0.4f);
            }
            else // Fuego
            {
                drawColor = Color.Lerp(drawColor, Color.OrangeRed, 0.4f);
            }

            // Efecto de brillo durante transición mayor
            if (inMajorTransition && attackTimer > 60 && attackTimer < 120)
            {
                // Crear efecto de pulso de brillo
                float glowIntensity = 0.5f + (float)Math.Sin(attackTimer * 0.3f) * 0.3f;
                Color glowColor = Color.Lerp(Color.Cyan, Color.OrangeRed, (float)Math.Sin(attackTimer * 0.1f) * 0.5f + 0.5f);

                // Dibujar sprite con brillo
                for (int i = 0; i < 4; i++)
                {
                    Vector2 glowOffset = new Vector2(2f, 0).RotatedBy(MathHelper.PiOver2 * i);
                    spriteBatch.Draw(texture, drawPos + glowOffset, NPC.frame, glowColor * glowIntensity * 0.5f,
                        NPC.rotation, NPC.frame.Size() / 2f, NPC.scale, SpriteEffects.None, 0f);
                }
            }

            // Dibujar estela si está cargando
            if (attackPhase == 1 && attackTimer < 40)
            {
                for (int i = 0; i < NPC.oldPos.Length; i++)
                {
                    Vector2 trailDrawPos = NPC.oldPos[i] - screenPos + NPC.Size / 2f;
                    Color trailColor = drawColor * ((NPC.oldPos.Length - i) / (float)NPC.oldPos.Length) * 0.5f;
                    spriteBatch.Draw(texture, trailDrawPos, NPC.frame, trailColor, NPC.oldRot[i],
                        NPC.frame.Size() / 2f, NPC.scale, SpriteEffects.None, 0f);
                }
            }

            // Dibujar sprite principal
            SpriteEffects effects = NPC.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            spriteBatch.Draw(texture, drawPos, NPC.frame, drawColor, NPC.rotation,
                NPC.frame.Size() / 2f, NPC.scale, effects, 0f);

            // TODO: Agregar Glowmask aquí cuando tengas la textura
            /*
            Texture2D glowTexture = ModContent.Request<Texture2D>("TuMod/Content/NPCs/Dragon/Cryoboros_Glow").Value;
            Color glowColor = currentStance == 0 ? Color.Cyan : Color.OrangeRed;
            spriteBatch.Draw(glowTexture, drawPos, NPC.frame, glowColor, NPC.rotation, 
                NPC.frame.Size() / 2f, NPC.scale, effects, 0f);
            */

            return false; // No dibujar el sprite por defecto
        }

        public override void FindFrame(int frameHeight)
        {
            // Animación básica
            NPC.frameCounter++;
            if (NPC.frameCounter >= 8)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;
                if (NPC.frame.Y >= frameHeight * Main.npcFrameCount[Type])
                {
                    NPC.frame.Y = 0;
                }
            }
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            // Efectos al recibir daño
            int dustType = currentStance == 0 ? DustID.Ice : DustID.Torch;
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, dustType);
            }
        }
        #endregion

        #region Botín y Muerte
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // Agregar drops personalizados aquí
            // npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<CryoborosScale>(), 1, 15, 25));
            npcLoot.Add(ItemDropRule.Common(ItemID.SoulofFright, 1, 5, 10));
        }

        public override void OnKill()
        {
            // Efectos especiales al morir
            for (int i = 0; i < 50; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(10f, 10f);
                Dust.NewDust(NPC.Center, 0, 0, DustID.Ice, dustVel.X, dustVel.Y, Scale: 2f);
                Dust.NewDust(NPC.Center, 0, 0, DustID.Torch, dustVel.X, dustVel.Y, Scale: 2f);
            }

            SoundEngine.PlaySound(SoundID.NPCDeath14, NPC.Center);
        }

        public override bool CheckDead()
        {
            return true;
        }
        #endregion
    }
}