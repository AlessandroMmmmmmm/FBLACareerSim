using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Prototype
{
    public string name;
    public GameObject prefab;
    public int meshRotation; // 0, 1, 2, 3

    public List<Attribute> attributes = new List<Attribute>();

    // Rename these to match exactly what your WFC script is looking for
    public WFC_Socket posZ;
    public WFC_Socket negZ;
    public WFC_Socket posX;
    public WFC_Socket negX;
}
