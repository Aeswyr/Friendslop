using UnityEngine;

[CreateAssetMenu(fileName = "ItemActions", menuName = "Scriptable Objects/ItemActions")]
public class ItemActions : ScriptableObject
{
    public void Eat(PlayerController player, ItemID item)
    {
        switch(item)
        {
            case ItemID.FOOD_CHICKEN:
                player.RemoveDamage(HealthType.HUNGER, 35);
                break;
            case ItemID.QUEST_POSTING:
                player.RemoveDamage(HealthType.HUNGER, 15);
                player.AddDamage(HealthType.DOOM, 5);
                break;
        }
        player.RemoveHeldItem();
    }
}
