using UnityEngine;

public class HitData
{
    public int minIndex;
    public Vector2 minPoint;
    public int maxIndex;
    public Vector2 maxPoint;

    public HitData(int index, Vector2 point)
    {
        minIndex = index;
        maxIndex = index;
        minPoint = point;
        maxPoint = point;
    }
    
    public void Update(int index, Vector2 point)
    {
        if (index < minIndex)
        {
            minIndex = index;
            minPoint = point;
        }
        if (index > maxIndex)
        {
            maxIndex = index;
            maxPoint = point;
        }
    }
}