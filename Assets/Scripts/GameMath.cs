using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameMath
{
    public static float Approach(float current, float target, float inc, float dec)
    {
        if(current < target)
        {
            current += inc;
            if(current > target)
            {
                current = target;
            }
        }
        else
        {
            current -= dec;
            if(current < target)
            {
                current = target;
            }
        }
        return current;
    }

    public static float ApproachDelta(float current, float target, float inc, float dec)
    {
        if (current < target)
        {
            current += inc * Time.deltaTime;
            if (current > target)
            {
                current = target;
            }
        }
        else
        {
            current -= dec * Time.deltaTime;
            if (current < target)
            {
                current = target;
            }
        }
        return current;
    }
}
