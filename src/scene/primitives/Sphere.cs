using System;

namespace RayTracer
{
    /// <summary>
    /// Class to represent an (infinite) plane in a scene.
    /// </summary>
    public class Sphere : SceneEntity
    {
        private Vector3 center;
        private double radius;
        private Material material;

        /// <summary>
        /// Construct a sphere given its center point and a radius.
        /// </summary>
        /// <param name="center">Center of the sphere</param>
        /// <param name="radius">Radius of the spher</param>
        /// <param name="material">Material assigned to the sphere</param>
        public Sphere(Vector3 center, double radius, Material material)
        {
            this.center = center;
            this.radius = radius;
            this.material = material;
        }

        /// <summary>
        /// Determine if a ray intersects with the sphere, and if so, return hit data.
        /// </summary>
        /// <param name="ray">Ray to check</param>
        /// <returns>Hit data (or null if no intersection)</returns>
        public RayHit Intersect(Ray ray)
        {
            Vector3 length = this.center - ray.Origin;
            double tca = length.Dot(ray.Direction);
            if (tca < 0)
            {
                return null;
            }
            double d2 = length.Dot(length) - Math.Pow(tca, 2);
            double r2 = Math.Pow(this.radius, 2);
            if (d2 > r2)
            {
                return null;
            }
            double thc = Math.Sqrt(r2 - d2);
            double time0 = tca - thc;
            double time1 = tca + thc;
            if (time0 > time1)
            {
                double temporary = time0;
                time0 = time1;
                time1 = temporary;
            }
            if (time0 < 0)
            {
                time0 = time1;
                if (time0 < 0)
                {
                    return null;
                }
            }
            Vector3 hitLocation = ray.Origin + time0 * ray.Direction;
            return new RayHit(hitLocation, (hitLocation - this.center).Normalized(), ray.Direction, this.material);
        }

        /// <summary>
        /// The material of the sphere.
        /// </summary>
        public Material Material { get { return this.material; } }
    }

}
