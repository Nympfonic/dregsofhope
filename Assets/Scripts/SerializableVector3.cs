using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializableVector3
{
    private float x, y, z;

    public SerializableVector3(Vector3 vec3)
    {
        x = vec3.x;
        y = vec3.y;
        z = vec3.z;
    }

    public static implicit operator SerializableVector3(Vector3 vec3)
    {
        return new SerializableVector3(vec3);
    }

    public static explicit operator Vector3(SerializableVector3 serializableVec3)
    {
        return new Vector3(serializableVec3.x, serializableVec3.y, serializableVec3.z);
    }
}
