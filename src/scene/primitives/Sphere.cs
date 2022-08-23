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
            Vector3 originCenterDistance = ray.Origin - this.center;
            double a = ray.Direction.Dot(ray.Direction);
            double b = 2 * ray.Direction.Dot(originCenterDistance);
            double c = originCenterDistance.Dot(originCenterDistance) - Math.Pow(this.radius, 2);
            double[] times = quadraticSolution(a, b, c);
            if (times == null)
            {
                return null;
            }
            if (times[0] > times[1])
            {
                double temporary = times[0];
                times[0] = times[1];
                times[1] = temporary;
            }
            if (times[0] < 0)
            {
                times[0] = times[1];
                if (times[0] < 0)
                {
                    return null;
                }
            }
            Vector3 hitLocation = ray.Origin + times[0] * ray.Direction;
            return new RayHit(hitLocation, (hitLocation - this.center).Normalized(), ray.Direction, null);
        }

        private double[] quadraticSolution(double a, double b, double c)
        {
            double discriminant = Math.Pow(b, 2) - 4 * a * c;
            if (discriminant < 0)
            {
                return null;
            }
            double x0, x1;
            if (discriminant == 0)
            {
                x0 = x1 = -0.5 * b / a;
            }
            else {
                double quadratic = b > 0 ? -(Math.Sqrt(discriminant) + b) / 2 : (Math.Sqrt(discriminant) - b) / 2;
                x0 = quadratic / a;
                x1 = c / quadratic;
            }
            return new double[] {x0, x1};
        }

        /// <summary>
        /// The material of the sphere.
        /// </summary>
        public Material Material { get { return this.material; } }
    }

}
