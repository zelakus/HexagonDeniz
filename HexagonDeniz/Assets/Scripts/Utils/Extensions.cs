using UnityEngine;

namespace HexDeniz
{
    public static class Extensions
    {
        public static float Angle(this Vector2 vector)
        {
            var angle = Vector2.SignedAngle(Vector2.right, vector);
            if (angle < 0)
                angle += 360;
            return angle;
        }

        public static Vector2 NegateY(this Vector2 vector)
        {
            return new Vector2(vector.x, -vector.y);
        }
    }
}