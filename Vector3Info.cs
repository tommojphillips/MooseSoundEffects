using UnityEngine;

namespace TommoJProductions.MooseSounds
{
    /// <summary>
    /// Represents a only x, y, z from a Vector3
    /// </summary>
    public class Vector3Info
    {
        // Written, 15.07.2022

        /// <summary>
        /// represents the x cord.
        /// </summary>
        public float x
        {
            get => vector3.x;
            set => vector3.x = value;
        }
        /// <summary>
        /// represents the y cord.
        /// </summary>
        public float y
        {
            get => vector3.y;
            set => vector3.y = value;
        }
        /// <summary>
        /// represents the z cord.
        /// </summary>
        public float z
        {
            get => vector3.z;
            set => vector3.z = value;
        }

        internal Vector3 vector3 = new Vector3();

        /// <summary>
        /// inits a new info with zeros.
        /// </summary>
        public Vector3Info() { }
        /// <summary>
        /// inits a new info.
        /// </summary>
        public Vector3Info(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        /// <summary>
        /// inits a new vector3info with vector3 values.
        /// </summary>
        /// <param name="v"></param>
        public Vector3Info(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        /// <summary>
        /// converts a vector3.
        /// </summary>
        /// <param name="v">the vector3 to convert.</param>
        /// <returns>new instance Vector3Info</returns>
        public static Vector3Info getInfo(Vector3 v)
        {
            return new Vector3Info(v);
        }

        /// <summary>
        /// casts an info to a vector3. (creates a new instance.)
        /// </summary>
        /// <param name="v">the vector3 to convert.</param>
        public static implicit operator Vector3(Vector3Info v) 
        {
            v.vector3.Set(v.x, v.y, v.z);
            return v.vector3;
        }
        /// <summary>
        /// casts a vector3 to an info. (creates a new instance.)
        /// </summary>
        /// <param name="v">the vector3 to convert.</param>
        public static implicit operator Vector3Info(Vector3 v) => new Vector3Info(v);
    }
}
