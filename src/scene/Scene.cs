using System;
using System.Collections.Generic;

namespace RayTracer
{
    /// <summary>
    /// Class to represent a ray traced scene, including the objects,
    /// light sources, and associated rendering logic.
    /// </summary>
    public class Scene
    {
        private SceneOptions options;
        private ISet<SceneEntity> entities;
        private ISet<PointLight> lights;

        /// <summary>
        /// Construct a new scene with provided options.
        /// </summary>
        /// <param name="options">Options data</param>
        public Scene(SceneOptions options = new SceneOptions())
        {
            this.options = options;
            this.entities = new HashSet<SceneEntity>();
            this.lights = new HashSet<PointLight>();
        }

        /// <summary>
        /// Add an entity to the scene that should be rendered.
        /// </summary>
        /// <param name="entity">Entity object</param>
        public void AddEntity(SceneEntity entity)
        {
            this.entities.Add(entity);
        }

        /// <summary>
        /// Add a point light to the scene that should be computed.
        /// </summary>
        /// <param name="light">Light structure</param>
        public void AddPointLight(PointLight light)
        {
            this.lights.Add(light);
        }

        /// <summary>
        /// Render the scene to an output image. This is where the bulk
        /// of your ray tracing logic should go... though you may wish to
        /// break it down into multiple functions as it gets more complex!
        /// </summary>
        /// <param name="outputImage">Image to store render output</param>
        public void Render(Image outputImage)
        {
            double z = outputImage.Width / (2 * Math.Tan(Math.PI / 6));
            for (int x = 0; x < outputImage.Width; x++)
            {
                for (int y = 0; y < outputImage.Height; y++)
                {
                    outputImage.SetPixel(x, y, new Color(0, 0, 0));
                    Vector3 coordinates = new Vector3((x - outputImage.Width) / 2, ((outputImage.Height - 1) / 2) - y, z);
                    Ray ray = new Ray(new Vector3(0, 0, 0), coordinates.Normalized());
                    RayHit? nearestHit = null;
                    foreach (SceneEntity entity in this.entities)
                    {
                        RayHit hit = entity.Intersect(ray);
                        if (hit != null && (hit?.Position.Length() < nearestHit?.Position.Length() || nearestHit == null))
                        {
                            outputImage.SetPixel(x, y, entity.Material.Color);
                            nearestHit = hit;
                        }
                    }
                }
            }
        }
    }
}
