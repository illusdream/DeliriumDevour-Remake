using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Item的基础属性
/// </summary>
[CreateAssetMenu(fileName = "ItemInfo", menuName = "Item/Info", order = 1)]
public class ItemInfo : ScriptableObject
{
        /// <summary>
        /// 物品的ID
        /// </summary>
        public EItemType ID;
        /// <summary>
        /// 物品使用的动作模型ID
        /// </summary>
        public EItemActionModel ActionModelID;

        /// <summary>
        /// 物品是否可以使用
        /// </summary>
        public bool CanUse;

        /// <summary>
        /// 物品最大堆叠数量
        /// </summary>
        public int MaxStackCount;

        public bool IsConsumable;

        public Sprite ItemTexture;
        
        public GameObject ItemPrefab;
}