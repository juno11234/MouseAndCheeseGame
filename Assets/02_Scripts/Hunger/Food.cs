using UnityEngine;

public class Food : MonoBehaviour, IHungerObj
{
    public HungerObjType HungerObjType => _type;
    public float Amount => _amount;
    [SerializeField] private float _amount;
    [SerializeField] private HungerObjType _type;
}