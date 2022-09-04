using System;

namespace RayTracer
{
    /// <summary>
    /// Class to represent a triangle in a scene represented by three vertices.
    /// </summary>
    public class Triangle : SceneEntity
    {
        private Vector3 v0, v1, v2;
        private Material material;

        /// <summary>
        /// Construct a triangle object given three vertices.
        /// </summary>
        /// <param name="v0">First vertex position</param>
        /// <param name="v1">Second vertex position</param>
        /// <param name="v2">Third vertex position</param>
        /// <param name="material">Material assigned to the triangle</param>
        public Triangle(Vector3 v0, Vector3 v1, Vector3 v2, Material material)
        {
            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;
            this.material = material;
        }

        /// <summary>
        /// Determine if a ray intersects with the triangle, and if so, return hit data.
        /// </summary>
        /// <param name="ray">Ray to check</param>
        /// <returns>Hit data (or null if no intersection)</returns>
        public RayHit Intersect(Ray ray)
        {
            Vector3 v0v1 = this.v1 - this.v0;
            Vector3 v0v2 = this.v2 - this.v0;
            Vector3 normal = v0v1.Cross(v0v2);
            double dotDirection = normal.Dot(ray.Direction);
            if (Math.Abs(dotDirection) < double.Epsilon)
            {
                return null;
            }
            double dotProduct = -normal.Dot(this.v0);
            double time = -(normal.Dot(ray.Origin) + dotProduct) / dotDirection;
            if (time < 0)
            {
                return null;
            }
            Vector3 hitLocation = ray.Origin + time * ray.Direction;
            if (normal.Dot(v0v1.Cross(hitLocation - this.v0)) < 0)
            {
                return null;
            }
            if (normal.Dot((v2 - v1).Cross(hitLocation - this.v1)) < 0)
            {
                return null;
            }
            if (normal.Dot((this.v0 - this.v2).Cross(hitLocation - this.v2)) < 0)
            {
                return null;
            }
            return new RayHit(hitLocation, normal.Normalized(), ray.Direction, this.material);
        }

        /// <summary>
        /// The material of the triangle.
        /// </summary>
        public Material Material { get { return this.material; } }
    }

}
