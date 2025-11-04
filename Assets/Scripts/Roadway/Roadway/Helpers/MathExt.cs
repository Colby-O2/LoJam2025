using UnityEngine;

namespace Roadway.Helper
{
    public static class MathExt
    {
        public static float Square(float value)
        {
            return value * value;
        }
        public static float Remap(float value, float inputStart, float inputEnd, float outputStart, float outputEnd)
        {
            return (value - inputStart) / (inputEnd - inputStart) * (outputEnd - outputStart) + outputStart;
        }
        public static float Sign(float value)
        {
            return value < 0 ? -1 : 1;
        }
        
        public static Quaternion ExtractRotation(this Matrix4x4 matrix)
        {
            Vector3 forward;
            forward.x = matrix.m02;
            forward.y = matrix.m12;
            forward.z = matrix.m22;

            Vector3 upwards;
            upwards.x = matrix.m01;
            upwards.y = matrix.m11;
            upwards.z = matrix.m21;

            return Quaternion.LookRotation(forward, upwards);
        }

        public static Vector3 ExtractPosition(this Matrix4x4 matrix)
        {
            Vector3 position;
            position.x = matrix.m03;
            position.y = matrix.m13;
            position.z = matrix.m23;
            return position;
        }

        public static Vector3 ExtractScale(this Matrix4x4 matrix)
        {
            Vector3 scale;
            scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
            scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
            scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
            return scale;
        }

        public static int SignInt(float v)
        {
            return Mathf.RoundToInt(Mathf.Sign(v));
        }
        
        public static Vector2 SetX(this Vector2 v, float x)
        {
            v.x = x;
            return v;
        }
        public static Vector2 SetY(this Vector2 v, float x)
        {
            v.y = x;
            return v;
        }

        public static Vector3 SetX(this Vector3 v, float x)
        {
            v.x = x;
            return v;
        }
        public static Vector3 SetY(this Vector3 v, float x)
        {
            v.y = x;
            return v;
        }
        public static Vector3 SetZ(this Vector3 v, float x)
        {
            v.z = x;
            return v;
        }
        public static Vector2 AddX(this Vector2 v, float x)
        {
            v.x += x;
            return v;
        }
        public static Vector2 AddY(this Vector2 v, float x)
        {
            v.y += x;
            return v;
        }

        public static Vector3 AddX(this Vector3 v, float x)
        {
            v.x += x;
            return v;
        }
        public static Vector3 AddY(this Vector3 v, float x)
        {
            v.y += x;
            return v;
        }
        public static Vector3 AddZ(this Vector3 v, float x)
        {
            v.z += x;
            return v;
        }

        public static Color SetR(this Color c, float v)
        {
            c.r = v;
            return c;
        }

        public static Color SetG(this Color c, float v)
        {
            c.g = v;
            return c;
        }

        public static Color SetB(this Color c, float v)
        {
            c.b = v;
            return c;
        }

        public static Color SetA(this Color c, float v)
        {
            c.a = v;
            return c;
        }

        public static float AngleBetweenVectorsOnPlane(Vector3 v1, Vector3 v2, Vector3 planeNormal)
        {
            v1 = v1.normalized;
            v2 = v2.normalized;
            planeNormal = planeNormal.normalized;
            Vector3 proj1 = Vector3.Normalize(v1 - planeNormal * Vector3.Dot(v1, planeNormal));
            Vector3 proj2 = Vector3.Normalize(v2 - planeNormal * Vector3.Dot(v2, planeNormal));

            if (proj1.magnitude < 0.0001 || proj2.magnitude < 0.0001) return 0;

            float angle = Mathf.Atan2(Vector3.Dot(planeNormal, Vector3.Cross(proj1, proj2)), Vector3.Dot(proj1, proj2));
            return angle * 180.0f / Mathf.PI;
        }

        public static float Angle360To180(float angle) => Mathf.Repeat(angle + 180, 360) - 180;

        public static float ClampMagnitude(float v, float low, float high)
        {
            float av = Mathf.Clamp(Mathf.Abs(v), low, high);
            return av * Mathf.Sign(v);
        }
    }
}
