using UnityEngine;
[CreateAssetMenu(fileName = "NewHeroData", menuName = "Hero Data/Hero")]
public class HeroData : ScriptableObject
{
    public string heroName;
    public float bCost;
    public float rpCost;

    [Header("Stats cơ bản")]
    public float HP; // Health Points
    public float HPR; // Health Points Regeneration
    public float MP; // Mana Points
    public float MPR; // Mana Points Regeneration
    public float MS; // Movement Speed
    public float AD; // Attack Damage
    public float AS; // Attack Speed
    public float AR; // Armor
    public float MR; // Magic Resist
    public float CR; // Critical Rate
    public float CritDMG; // Critical Damage
    public float MissleSpeed; // Missile Speed

    [Header("Chỉ số tăng thêm mỗi Level")]
    public float HPPL; // HP per level
    public float HPRPL; // HP Regeneration per level
    public float MPPL; // MP per level
    public float MPRPL; // MP Regeneration per level
    public float ADPL; // Attack Damage per level
    public float ASPL; // Attack Speed per level
    public float ARPL; // Armor per level
    public float MRPL; // Magic Resist per level
}