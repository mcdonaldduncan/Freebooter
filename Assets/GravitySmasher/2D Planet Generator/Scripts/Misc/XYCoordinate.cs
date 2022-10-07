using System;
using System.Collections;
using System.Collections.Generic;

// contains XY Coordinate and dictionaries can find duplicates
public class XYCoordinate
{
    int x;
    int y;
    public XYCoordinate(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public void changeXY(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return this.x == ((XYCoordinate)obj).x && this.y == ((XYCoordinate)obj).y;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            // Choose large primes to avoid hashing collisions
            const int HashingBase = (int)2166136261;
            const int HashingMultiplier = 16777619;

            int hash = HashingBase;
            hash = (hash * HashingMultiplier) ^ (!Object.ReferenceEquals(null, x) ? x.GetHashCode() : 0);
            hash = (hash * HashingMultiplier) ^ (!Object.ReferenceEquals(null, y) ? y.GetHashCode() : 0);
            return hash;
        }
    }
}
