using Tomlet.Attributes;

namespace KeepInventory.SaveSlot
{
    public class Save_MagazineState
    {
        [TomlPrecedingComment("Amount of rounds in the magazine")]
        public int Count;

        public Save_MagazineState()
        { }

        public Save_MagazineState(int count)
        {
            Count = count;
        }
    }
}