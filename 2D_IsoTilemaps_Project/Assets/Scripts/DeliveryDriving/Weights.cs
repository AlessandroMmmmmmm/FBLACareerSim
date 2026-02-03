using UnityEngine;

// This attribute ONLY works on classes that inherit from ScriptableObject
[CreateAssetMenu(fileName = "CityWeights", menuName = "WFC/Weights")]
public class Weights : ScriptableObject
{
    [Header("Road Settings")]
    [Range(0, 10)] public int intersectionWeight = 5;
    [Range(0, 10)] public int roadStraightWeight = 10;

    [Header("Environment Settings")]
    [Range(0, 10)] public int natureWeight = 8;
    [Range(0, 10)] public int buildingWeight = 5;
    [Range(0, 10)] public int propsWeight = 3;

    [Header("Water Settings")]
    [Range(0, 10)] public int waterWeight = 2;
    [Range(0, 10)] public int waterBendWeight = 1;

    public int GetWeight(Attribute a)
    {
        switch (a)
        {
            case Attribute.Road: return roadStraightWeight;
            case Attribute.Intersection: return intersectionWeight;
            case Attribute.Nature: return natureWeight;
            case Attribute.Building: return buildingWeight;
            case Attribute.Props: return propsWeight;
            case Attribute.Water: return waterWeight;
            default: return 1;
        }
    }
}

public enum Attribute { Building, Nature, Props, Road, Intersection, Water };
