using System;
using System.Collections.Generic;

# nullable enable

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
                    double normX = (x + 0.5) / outputImage.Width;
                    double normY = (y + 0.5) / outputImage.Height;
                    Vector3 coordinates = new Vector3(outputImage.Width * (normX - 0.5), outputImage.Height * (0.5 - normY), z);
                    Ray ray = new Ray(new Vector3(0, 0, 0), coordinates.Normalized());
                    RayHit? nearestHit = null;
                    foreach (SceneEntity entity in this.entities)
                    {
                        RayHit hit = entity.Intersect(ray);
                        if (hit != null && (hit?.Position.LengthSq() < nearestHit?.Position.LengthSq() || nearestHit == null))
                        {
                            outputImage.SetPixel(x, y, hit.Material.Type == Material.MaterialType.Diffuse ? diffuseColor(hit) : hit.Material.Color);
                            nearestHit = hit;
                        }
                    }
                }
            }
        }
        private Color diffuseColor(RayHit hit)
        {
            Color color = new Color(0, 0, 0);
            Color subcolor;
            foreach (PointLight light in this.lights)
            {
                subcolor = hit.Material.Color * light.Color * hit.Normal.Dot(light.Position);
                if (subcolor.R >= 0 && subcolor.G >= 0 && subcolor.B >= 0)
                {
                    color += subcolor;
                }
            }
            return color;
        }
    }
}
