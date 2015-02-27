using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace MasterSharp
{
    internal class SummonerItems
    {
        public enum ItemIds
        {
            //MuramanaDe = 3043,
            Muramana = 3042,
            Tiamat = 3077,
            Hydra = 3074,
            MercScim = 3139,
            Hextech = 3146,
            SwordOd = 3131,
            Ghostblade = 3142,
            BotRk = 3153,
            Cutlass = 3144,

            Omen = 3143
        }

        private readonly SpellSlot _ignite;
        private readonly Obj_AI_Hero _player;
        private readonly SpellSlot _smite;
        private readonly Spellbook _sumBook;

        public SummonerItems(Obj_AI_Hero myHero)
        {
            _player = myHero;
            _sumBook = _player.Spellbook;
            _ignite = _player.GetSpellSlot("summonerdot");
            _smite = _player.GetSpellSlot("SummonerSmite");
        }

        public void CastIgnite(Obj_AI_Hero target)
        {
            if (_ignite != SpellSlot.Unknown && _sumBook.CanUseSpell(_ignite) == SpellState.Ready)
                _sumBook.CastSpell(_ignite, target);
        }

        public void CastSmite(Obj_AI_Hero target)
        {
            if (_smite != SpellSlot.Unknown && _sumBook.CanUseSpell(_smite) == SpellState.Ready)
                _sumBook.CastSpell(_smite, target);
        }

        public void cast(ItemIds item)
        {
            var itemId = (int) item;
            if (Items.CanUseItem(itemId))
                Items.UseItem(itemId);
        }

        public void cast(ItemIds item, Vector3 target)
        {
            var itemId = (int) item;
            if (Items.CanUseItem(itemId))
                _player.Spellbook.CastSpell(GetInvSlot(itemId).SpellSlot, target);
        }

        public void cast(ItemIds item, Obj_AI_Base target)
        {
            var itemId = (int) item;
            if (Items.CanUseItem(itemId))
                Items.UseItem(itemId, target);
        }

        private InventorySlot GetInvSlot(int id)
        {
            return _player.InventoryItems.FirstOrDefault(iSlot => (int) iSlot.Id == id);
        }
    }
}