using UnityEngine;

public enum CardType
{
    Attacker,
    Defender,
    Supporter
};
[CreateAssetMenu]
public class CardData : ScriptableObject {
    public CardType type; // Sword, Shield, Staff
    public Sprite cardArt;
    public int cardPower;
}