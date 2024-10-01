using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Vampirism.Skill
{
    [Serializable]
    public class SkillBloodForGold : VampireSkill
    {
        public string potionItemId = "PotionVampireBlood";
        private ItemData potionItemData;
        
        public string storePotionsLootId = "ShipShopPotionsRestock";
        private LootTable storePotionsLootTable;

        private bool potionAdded = false;
        private LootTable.DropLevel potionDropLevel;
        private LootTable.Drop potionDrop;

        public override VampireModule CreateModule() => null;

        public override void OnCatalogRefresh()
        {
            base.OnCatalogRefresh();

            potionItemData = Catalog.GetData<ItemData>(potionItemId);
            storePotionsLootTable = Catalog.GetData<LootTable>(storePotionsLootId);

            potionDrop = new LootTable.Drop()
            {
                referenceID = potionItemId,
                reference = LootTable.Drop.Reference.Item,
                itemData = potionItemData,
                randMode = LootTable.Drop.RandMode.ItemCount,
                minMaxRand = new Vector2(1.0f, 1.0f),
                probabilityWeight = 0.2f
            };
            potionDropLevel = new LootTable.DropLevel()
            {
                dropLevel = 0,
                drops = { potionDrop }
            };
        }

        public override void OnSkillLoaded(SkillData skillData, Creature creature)
        {
            base.OnSkillLoaded(skillData, creature);

            if (creature == null || !creature.isPlayer || potionAdded)
                return;

            storePotionsLootTable.levelledDrops.Add(potionDropLevel);
            potionAdded = true;
        }

        public override void OnSkillUnloaded(SkillData skillData, Creature creature)
        {
            base.OnSkillUnloaded(skillData, creature);

            if (creature == null || !creature.isPlayer || !potionAdded)
                return;

            storePotionsLootTable.levelledDrops.Remove(potionDropLevel);
            potionAdded = false;
        }

    }
}
