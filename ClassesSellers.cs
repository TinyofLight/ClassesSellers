using Terraria.ModLoader;

namespace ClassesSellers
{
    public class ClassesSellers : Mod
    {
        public static ClassesSellers Instance { get; private set; }

        public override void Load()
        {
            Instance = this;
        }

        public override void Unload()
        {
            Instance = null;
        }
    }
}