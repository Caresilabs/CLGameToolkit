using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ColorCycler : MonoBehaviour
{
    [Header("IColor target")]
    [SerializeField] private Renderer Renderer;
    [SerializeField] private Graphic Graphic;
    [SerializeField] private Light Light;

    [Header("Config")]
    [SerializeField] private Vector3 Speed;
    [SerializeField] private Vector3Int Min;
    [SerializeField, FormerlySerializedAs("Max2")] private Vector3Int Max;
    [SerializeField, Range(0, 1)] private float RandomDirectionChance;

    private IColor colorable;
    private Color currentColor;
    private float hue;
    private float saturation;
    private float brightness;

    private float hueMax;
    private float saturationMax;
    private float brightnessMax;

    private float hueMin;
    private float saturationMin;
    private float brightnessMin;

    void Awake()
    {
        if (Graphic != null)
            colorable = new GraphicsProxy(Graphic);
        else if (Renderer != null)
            colorable = new RendererProxy(Renderer);
        else if (Light != null)
            colorable = new LightProxy(Light);

        currentColor = colorable.color;
        Color.RGBToHSV(currentColor, out hue, out saturation, out brightness);
        // Color.RGBToHSV(Max, out hueMax, out saturationMax, out brightnessMax);

        hueMax = Max.x / 360f;
        saturationMax = Max.y / 100f;
        brightnessMax = Max.z / 100f;

        hueMin = Min.x / 360f;
        saturationMin = Min.y / 100f;
        brightnessMin = Min.z / 100f;
    }

    private void OnDisable()
    {
        colorable.color = currentColor;
        Color.RGBToHSV(currentColor, out hue, out saturation, out brightness);
    }

    void Update()
    {
        if (!enabled)
            return;

        hue += Speed.x * Time.deltaTime;
        saturation += Speed.y * Time.deltaTime;
        brightness += Speed.z * Time.deltaTime;

        if (hue >= hueMax)
            Speed.x = -Mathf.Abs(Speed.x);
        else if (hue <= hueMin)
            Speed.x = Mathf.Abs(Speed.x);

        if (saturation >= saturationMax)
            Speed.y = -Mathf.Abs(Speed.y);
        else if (saturation <= saturationMin)
            Speed.y = Mathf.Abs(Speed.y);

        if (brightness >= brightnessMax)
            Speed.z = -Mathf.Abs(Speed.z);
        else if (brightness <= brightnessMin)
            Speed.z = Mathf.Abs(Speed.z);

        if (RandomDirectionChance > 0 && Random.value < RandomDirectionChance)
            Speed *= -1;

        var newColor = Color.HSVToRGB(hue, saturation, brightness);
        newColor.a = currentColor.a;

        colorable.color = newColor;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Graphic == null)
            Graphic = GetComponent<Graphic>();

        if (Light == null)
            Light = GetComponent<Light>();
    }
#endif

    private readonly struct RendererProxy : IColor
    {
        private readonly Material material;
        internal RendererProxy(Renderer renderer) => this.material = renderer.sharedMaterial; // Should not use sharedMaterial.

        public readonly Color color
        {
            get => material.color;
            set => material.color = value; // * this.intensity;
        }
    }

    private readonly struct GraphicsProxy : IColor
    {
        private readonly Graphic graphic;
        internal GraphicsProxy(Graphic graphic) => this.graphic = graphic;

        public readonly Color color { get => graphic.color; set => graphic.color = value; }
    }

    private readonly struct LightProxy : IColor
    {
        private readonly Light light;
        internal LightProxy(Light light) => this.light = light;

        public readonly Color color { get => light.color; set => light.color = value; }
    }
}
