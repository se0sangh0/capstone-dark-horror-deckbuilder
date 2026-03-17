using UnityEngine;

public enum CardType
{
    Attacker,
    Defender,
    Supporter
}
[CreateAssetMenu]
public class CardData : ScriptableObject {
    public CardType type; // Attacker, Defender, Supporter
    public Sprite cardArt;
    public int cardPower;
}