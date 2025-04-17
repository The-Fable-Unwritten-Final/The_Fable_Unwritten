using UnityEngine;

namespace HisaGames.TransformSetting
{
    [System.Serializable]
    public class EcTransformSetting
    {
        [Tooltip("The unique name of this transform setting, used for identification.")]
        public string name;

        [Tooltip("The position of the object in the scene (X, Y, Z coordinates).")]
        public Vector3 position;

        [Tooltip("The rotation of the object in the scene (X, Y, Z angles in degrees).")]
        public Vector3 rotation;

        [Tooltip("The scale of the object in the scene (X, Y, Z dimensions).")]
        public Vector3 scale;
    }
}