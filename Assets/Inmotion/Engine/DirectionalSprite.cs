using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DirectionalSprite
{
    public List<Sprite> sprites = new List<Sprite>(8);

    public DirectionalSprite(List<Sprite> target)
    {
        sprites = target;
    }
}
