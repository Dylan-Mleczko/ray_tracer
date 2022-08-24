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
                    double normX = (x + 0.5) / outputImage.Width;
                    double normY = (y + 0.5) / outputImage.Height;
                    Vector3 coordinates = new Vector3(outputImage.Width * (normX - 0.5), outputImage.Height * (0.5 - normY), z);
                    Ray ray = new Ray(new Vector3(0, 0, 0), coordinates.Normalized());
                    RayHit? nearestHit = null;
                    SceneEntity? hitEntity = null;
                    foreach (SceneEntity entity in this.entities)
                    {
                        RayHit hit = entity.Intersect(ray);
                        if (hit != null && (hit.Position.LengthSq() < nearestHit?.Position.LengthSq() || nearestHit == null))
                        {
                            nearestHit = hit;
                            hitEntity = entity;
                        }
                    }
                    outputImage.SetPixel(x, y, hitEntity != null && nearestHit != null ? PixelColor(hitEntity, nearestHit, GetVisibleLights(hitEntity, nearestHit), false) : new Color(0, 0, 0));
                }
            }
        }

        private HashSet<PointLight> GetVisibleLights(SceneEntity entity, RayHit hit)
        {
            HashSet<PointLight> visibleLights = new HashSet<PointLight>();
            foreach (PointLight light in this.lights)
            {
                Ray shadowRay = new Ray(hit.Position, (light.Position - hit.Position).Normalized());
                bool isVisible = true;
                foreach (SceneEntity blockingEntity in this.entities)
                {
                    if (blockingEntity != entity && blockingEntity.Intersect(shadowRay) != null)
                    {
                        isVisible = false;
                        break;
                    }
                }
                if (isVisible)
                {
                    visibleLights.Add(light);
                }
            }
            return visibleLights;
        }

        private Color PixelColor(SceneEntity entity, RayHit hit, HashSet<PointLight> visibleLights, bool inSphere)
        {
            if (visibleLights.Count == 0)
            {
                return new Color(0, 0, 0);
            }
            switch (entity.Material.Type)
            {
                case Material.MaterialType.Diffuse:
                    Color color = new Color(0, 0, 0);
                    Color subcolor;
                    foreach (PointLight light in visibleLights)
                    {
                        subcolor = hit.Material.Color * light.Color * hit.Normal.Dot((light.Position - hit.Position).Normalized());
                        if (subcolor.R >= 0 && subcolor.G >= 0 && subcolor.B >= 0)
                        {
                            color += subcolor;
                        }
                    }
                    return color;
                case Material.MaterialType.Reflective:
                    return ReflectedColor(entity, hit, visibleLights);
                case Material.MaterialType.Refractive:
                    return RefractedColor(entity, hit, visibleLights, inSphere);
                default:
                    return entity.Material.Color;
            }
        }

        private Color ReflectedColor(SceneEntity entity, RayHit currentHit, HashSet<PointLight> visibleLights)
        {
            Vector3 normalisedNormal = currentHit.Normal.Normalized();
            Ray reflectedRay = new Ray(currentHit.Position, currentHit.Incident - 2 * currentHit.Incident.Dot(normalisedNormal) * normalisedNormal);
            RayHit? nearestHit = null;
            SceneEntity? hitEntity = null;
            foreach (SceneEntity targetEntity in this.entities)
            {
                if (targetEntity != entity)
                {
                    RayHit hit = targetEntity.Intersect(reflectedRay);
                    if (hit != null && (hit.Position.LengthSq() < nearestHit?.Position.LengthSq() || nearestHit == null))
                    {
                        nearestHit = hit;
                        hitEntity = targetEntity;
                    }
                }
            }
            return hitEntity != null && nearestHit != null ? PixelColor(hitEntity, nearestHit, GetVisibleLights(hitEntity, nearestHit), false) : new Color(0, 0, 0);
        }

        private Color RefractedColor(SceneEntity entity, RayHit currentHit, HashSet<PointLight> visibleLights, bool inSphere)
        {
            double refractiveIndexRatio = inSphere ? entity.Material.RefractiveIndex : 1 / entity.Material.RefractiveIndex;
            double cosI = Math.Abs(currentHit.Normal.Dot(currentHit.Incident));
            double sinT2 = Math.Pow(refractiveIndexRatio, 2) * (1 - Math.Pow(cosI, 2));
            if (sinT2 > 1)
            {
                return new Color(0, 0, 0);
            }
            Ray refractedRay = new Ray(currentHit.Position, refractiveIndexRatio * currentHit.Incident + (refractiveIndexRatio * cosI - Math.Sqrt(1 - sinT2)) * currentHit.Normal);
            RayHit? nearestHit = null;
            SceneEntity? hitEntity = null;
            foreach (SceneEntity targetEntity in this.entities)
            {
                RayHit hit = targetEntity.Intersect(refractedRay);
                if (hit != null && hit != currentHit && (hit.Position.LengthSq() < nearestHit?.Position.LengthSq() || nearestHit == null))
                {
                    nearestHit = hit;
                    hitEntity = targetEntity;
                }
            }
            return hitEntity != null && nearestHit != null ? PixelColor(hitEntity, nearestHit, GetVisibleLights(hitEntity, nearestHit), !inSphere) : new Color(0, 0, 0);
        }
    }
}
