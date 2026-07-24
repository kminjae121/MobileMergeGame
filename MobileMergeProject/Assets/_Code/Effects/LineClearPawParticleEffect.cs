using System.Collections.Generic;
using UnityEngine;

namespace _Code.Effects
{
    public sealed class LineClearPawParticleEffect : MonoBehaviour
    {
        [SerializeField] private Sprite _pawSprite;
        [SerializeField] private Sprite _sparkleSprite;
        [SerializeField] private bool _playOnStart;
        [SerializeField, Min(1)] private int _pawBurstCount = 18;
        [SerializeField, Min(0)] private int _sparkleBurstCount = 10;
        [SerializeField, Min(1)] private int _perBlockPawBurstCount = 4;
        [SerializeField, Min(0)] private int _perBlockSparkleBurstCount = 2;
        [SerializeField, Min(0.05f)] private float _burstRadius = 0.72f;
        [SerializeField] private int _sortingOrder = 60;

        private const string PawSpritePath = "BlockBlast/LineClearPawParticleSprite";
        private const string SparkleSpritePath = "BlockBlast/LineClearSparkleParticleSprite";

        private ParticleSystem _pawParticles;
        private ParticleSystem _sparkleParticles;
        private Material _particleMaterial;
        private bool _systemsConfigured;

        private void Awake()
        {
            LoadSprites();
            EnsureParticleSystems();
        }

        private void Start()
        {
            if (_playOnStart)
                Play(transform.position, 1);
        }

        private void OnDestroy()
        {
            if (_particleMaterial != null)
                Destroy(_particleMaterial);
        }

        public void Play(Vector3 worldPosition, int clearedLineCount)
        {
            LoadSprites();
            EnsureParticleSystems();

            transform.position = worldPosition;

            int burstMultiplier = Mathf.Max(1, clearedLineCount);
            Emit(_pawParticles, _pawBurstCount * burstMultiplier);
            Emit(_sparkleParticles, _sparkleBurstCount * burstMultiplier);
        }

        public void PlayAtPositions(IReadOnlyList<Vector3> worldPositions, int clearedLineCount)
        {
            if (worldPositions == null || worldPositions.Count == 0)
            {
                Play(transform.position, clearedLineCount);
                return;
            }

            LoadSprites();
            EnsureParticleSystems();
            Clear(_pawParticles);
            Clear(_sparkleParticles);

            foreach (Vector3 worldPosition in worldPositions)
            {
                EmitAt(_pawParticles, worldPosition, _perBlockPawBurstCount);
                EmitAt(_sparkleParticles, worldPosition, _perBlockSparkleBurstCount);
            }
        }

        private void LoadSprites()
        {
            if (_pawSprite == null)
                _pawSprite = Resources.Load<Sprite>(PawSpritePath);

            if (_sparkleSprite == null)
                _sparkleSprite = Resources.Load<Sprite>(SparkleSpritePath);
        }

        private void EnsureParticleSystems()
        {
            if (_pawParticles == null)
                _pawParticles = CreateParticleSystem("PawBurstParticles");

            if (_sparkleParticles == null)
                _sparkleParticles = CreateParticleSystem("SparkleParticles");

            if (_systemsConfigured)
                return;

            ConfigurePawParticles(_pawParticles);
            ConfigureSparkleParticles(_sparkleParticles);
            _systemsConfigured = true;
        }

        private ParticleSystem CreateParticleSystem(string objectName)
        {
            GameObject particleObject = new GameObject(objectName);
            particleObject.transform.SetParent(transform, false);

            ParticleSystem particles = particleObject.AddComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            return particles;
        }

        private void ConfigurePawParticles(ParticleSystem particles)
        {
            ParticleSystem.MainModule main = particles.main;
            main.duration = 0.75f;
            main.loop = false;
            main.prewarm = false;
            main.startDelay = 0f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.48f, 0.86f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.15f, 2.25f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.16f, 0.28f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.gravityModifier = new ParticleSystem.MinMaxCurve(0.05f, 0.18f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 180;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.enabled = false;

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = _burstRadius;
            shape.radiusThickness = 0.45f;
            shape.randomDirectionAmount = 0.35f;

            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(CreateFadeGradient(1f, 0.78f, 0f));

            ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, CreatePopCurve());

            ParticleSystem.RotationOverLifetimeModule rotationOverLifetime = particles.rotationOverLifetime;
            rotationOverLifetime.enabled = true;
            rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(-1.5f, 1.5f);

            ConfigureRenderer(particles, _pawSprite, _sortingOrder);
        }

        private void ConfigureSparkleParticles(ParticleSystem particles)
        {
            ParticleSystem.MainModule main = particles.main;
            main.duration = 0.55f;
            main.loop = false;
            main.prewarm = false;
            main.startDelay = 0f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.32f, 0.58f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.75f, 1.55f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.16f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.gravityModifier = 0f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 120;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.enabled = false;

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = _burstRadius * 0.78f;
            shape.radiusThickness = 0.15f;
            shape.randomDirectionAmount = 0.45f;

            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(CreateFadeGradient(1f, 1f, 0f));

            ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, CreatePopCurve());

            ConfigureRenderer(particles, _sparkleSprite, _sortingOrder + 1);
        }

        private void ConfigureRenderer(ParticleSystem particles, Sprite sprite, int sortingOrder)
        {
            ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingOrder = sortingOrder;

            Material material = GetParticleMaterial();
            if (material != null)
                renderer.material = material;

            ParticleSystem.TextureSheetAnimationModule textureSheetAnimation = particles.textureSheetAnimation;
            textureSheetAnimation.enabled = sprite != null;

            if (sprite == null)
                return;

            textureSheetAnimation.mode = ParticleSystemAnimationMode.Sprites;
            textureSheetAnimation.AddSprite(sprite);
        }

        private Material GetParticleMaterial()
        {
            if (_particleMaterial != null)
                return _particleMaterial;

            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
                shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");

            if (shader == null)
                return null;

            _particleMaterial = new Material(shader);
            return _particleMaterial;
        }

        private static void Emit(ParticleSystem particles, int count)
        {
            if (particles == null || count <= 0)
                return;

            Clear(particles);
            particles.Emit(count);
        }

        private static void EmitAt(ParticleSystem particles, Vector3 worldPosition, int count)
        {
            if (particles == null || count <= 0)
                return;

            ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams
            {
                position = worldPosition,
                applyShapeToPosition = true
            };
            particles.Emit(emitParams, count);
        }

        private static void Clear(ParticleSystem particles)
        {
            if (particles != null)
                particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private static Gradient CreateFadeGradient(float startAlpha, float middleAlpha, float endAlpha)
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(startAlpha, 0f),
                    new GradientAlphaKey(middleAlpha, 0.42f),
                    new GradientAlphaKey(endAlpha, 1f)
                });
            return gradient;
        }

        private static AnimationCurve CreatePopCurve()
        {
            return new AnimationCurve(
                new Keyframe(0f, 0.2f),
                new Keyframe(0.16f, 1.1f),
                new Keyframe(1f, 0.35f));
        }
    }
}
