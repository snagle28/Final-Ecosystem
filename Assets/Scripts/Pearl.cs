using UnityEngine;

[CreateAssetMenu]
public class Pearl : ScriptableObject
{
    //Variables for dropping pearls
    public Sprite pearlSprite;
    public string pearlName;
    public int dropChance;

    public Pearl(string pearlName, int dropChance)
    {
        this.pearlName = pearlName;
        this.dropChance = dropChance;
    }
    
    
}
