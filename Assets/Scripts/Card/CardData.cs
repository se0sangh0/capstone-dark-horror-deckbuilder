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
    [Tooltip("동료의 성향. 스택 기여량의 생성 범위를 결정한다.")]
    public CardAffinity affinity;
}