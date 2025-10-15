using UnityEngine;
[CreateAssetMenu]
public class littleEgg : ScriptableObject
{
    public Sprite eggSprite;
    public string eggName;
    public int dropChance;
   
    

    public littleEgg(string eggName, int dropChance)
    {
        this.eggName = eggName;
        this.dropChance = dropChance;
    }
}
