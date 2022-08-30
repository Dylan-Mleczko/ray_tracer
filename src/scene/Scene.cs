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

        private Random random = new Random();

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
                    HashSet<Color> pixelColors = new HashSet<Color>();
                    for (int i = 1; i <= options.AAMultiplier; i++)
                    {
                        for (int j = 1; j <= options.AAMultiplier; j++)
                        {
                            double normX = (x + (i / (options.AAMultiplier + 1.0))) / outputImage.Width;
                            double normY = (y + (j / (options.AAMultiplier + 1.0))) / outputImage.Height;
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
                            pixelColors.Add(hitEntity != null && nearestHit != null ? PixelColor(hitEntity, nearestHit, 10) : new Color(0, 0, 0));
                        }
                    }
                    Color colorSum = new Color(0, 0, 0);
                    foreach(Color color in pixelColors)
                    {
                        colorSum += color;
                    }
                    outputImage.SetPixel(x, y, colorSum / pixelColors.Count);
                }
            }
        }

        private bool IsShadow(SceneEntity entity, RayHit hit)
        {
            foreach (PointLight light in this.lights)
            {
                Ray shadowRay = new Ray(light.Position, (hit.Position - light.Position).Normalized());
                RayHit? nearestHit = null;
                SceneEntity? hitEntity = null;
                foreach (SceneEntity blockingEntity in this.entities)
                {
                    RayHit currentHit = blockingEntity.Intersect(shadowRay);
                    if (currentHit != null && (currentHit.Position.LengthSq() < nearestHit?.Position.LengthSq() || nearestHit == null))
                    {
                        nearestHit = currentHit;
                        hitEntity = blockingEntity;
                    }
                }
                if (hitEntity == null || hitEntity == entity)
                {
                    return false;
                }
            }
            return true;
        }

        private HashSet<PointLight> GetVisibleLights(SceneEntity entity, RayHit hit)
        {
            HashSet<PointLight> visibleLights = new HashSet<PointLight>();
            foreach (PointLight light in this.lights)
            {
                Ray shadowRay = new Ray(light.Position, (hit.Position - light.Position).Normalized());
                RayHit? nearestHit = null;
                SceneEntity? hitEntity = null;
                foreach (SceneEntity blockingEntity in this.entities)
                {
                    RayHit currentHit = blockingEntity.Intersect(shadowRay);
                    if (currentHit != null && (currentHit.Position.LengthSq() < nearestHit?.Position.LengthSq() || nearestHit == null))
                    {
                        nearestHit = currentHit;
                        hitEntity = blockingEntity;
                    }
                }
                if (hitEntity == null || hitEntity == entity)
                {
                    visibleLights.Add(light);
                }
            }
            return visibleLights;
        }

        private Color PixelColor(SceneEntity entity, RayHit hit, int depth)
        {
            if (depth == 0)
            {
                return entity.Material.Color;
            }
            if (IsShadow(entity, hit))
            {
                return new Color(0, 0, 0);
            }
            switch (entity.Material.Type)
            {
                case Material.MaterialType.Diffuse:
                    Color color = new Color(0, 0, 0);
                    Color subcolor;
                    foreach (PointLight light in GetVisibleLights(entity, hit))
                    {
                        subcolor = hit.Material.Color * light.Color * hit.Normal.Dot((light.Position - hit.Position).Normalized());
                        if (subcolor.R >= 0 && subcolor.G >= 0 && subcolor.B >= 0)
                        {
                            color += subcolor;
                        }
                    }
                    return color;
                case Material.MaterialType.Reflective:
                    return ReflectedColor(entity, hit, depth - 1);
                case Material.MaterialType.Refractive:
                    //double fresnelProportion = FresnelProportion(entity.Material.RefractiveIndex, hit.Incident, hit.Normal.Normalized());
                    //return RefractedColor(entity, hit, visibleLights, depth - 1) * fresnelProportion + ReflectedColor(entity, hit, visibleLights, depth - 1) * (1 - fresnelProportion);
                    return RefractedColor(entity, hit, depth - 1);
                case Material.MaterialType.Glossy:
                    double REFLECTIVITY = 0.5;
                    Color colorSum = new Color(0, 0, 0);
                    Vector3 normalisedNormal = hit.Normal.Normalized();
                    Ray reflectedRay = new Ray(hit.Position, hit.Incident - 2 * hit.Incident.Dot(normalisedNormal) * normalisedNormal);
                    for (int i = 0; i < 80; i++)
                    {
                        double theta = Math.Acos(Math.Pow(random.NextDouble(), REFLECTIVITY));
                        double phi = 2 * Math.PI * random.NextDouble();
                        double x = Math.Sin(phi) * Math.Cos(theta) / 16;
                        double y = Math.Sin(phi) * Math.Sin(theta) / 16;
                        Vector3 u = reflectedRay.Direction.Cross(normalisedNormal);
                        Vector3 v = reflectedRay.Direction.Cross(u);
                        Ray ray = new Ray(reflectedRay.Origin, (x * u + y * v + reflectedRay.Direction).Normalized());
                        RayHit? nearestHit = null;
                        SceneEntity? hitEntity = null;
                        foreach (SceneEntity targetEntity in this.entities)
                        {
                            if (targetEntity != entity)
                            {
                                RayHit currentHit = targetEntity.Intersect(ray);
                                if (currentHit != null && (currentHit.Position.LengthSq() < nearestHit?.Position.LengthSq() || nearestHit == null))
                                {
                                    nearestHit = currentHit;
                                    hitEntity = targetEntity;
                                }
                            }
                        }

                        colorSum += (hitEntity != null && nearestHit != null ? PixelColor(hitEntity, nearestHit, depth - 1) : new Color(0, 0, 0)) * REFLECTIVITY;
                    }
                    return entity.Material.Color + colorSum / 80;
                default:
                    return entity.Material.Color;
            }
        }

        private Color ReflectedColor(SceneEntity entity, RayHit currentHit, int depth)
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
            return hitEntity != null && nearestHit != null ? PixelColor(hitEntity, nearestHit, depth - 1) : new Color(0, 0, 0);
        }

        private Color RefractedColor(SceneEntity entity, RayHit currentHit, int depth)
        {
            double refractiveIndexRatio = 1 / entity.Material.RefractiveIndex;
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
            return hitEntity != null && nearestHit != null ? PixelColor(hitEntity, nearestHit, depth - 1) : new Color(0, 0, 0);
        }

        private double FresnelProportion(double refractiveIndex, Vector3 incidentVector, Vector3 normalVector)
        {
            double refraction = Math.Pow((1 - refractiveIndex) / (1 + refractiveIndex), 2);
            double cosX = -normalVector.Dot(incidentVector);
            if (1 > refractiveIndex)
            {
                double sinT2 = 1 - Math.Pow(cosX, 2);
                if (sinT2 > 1)
                {
                    return 1;
                }
                cosX = Math.Sqrt(1 - sinT2);
            }
            return refraction + (1 - refraction) * Math.Pow(1 - cosX, 5);
        }
    }
}
