using UnityEngine;

[CreateAssetMenu]
public class FellowData : ScriptableObject
{
    public StackType positionStack;
    public Sprite fellowSprite;
    public int currentHp = 100;
    public int currentStress = 0;
    public int currentStack = 0; 
    public bool isDead = false;
}
