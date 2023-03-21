using UnityEngine;

namespace FlatKit {
public class Buoyancy : MonoBehaviour {
    [Tooltip("The object that contains a Water material.")]
    public Transform water;

    [Space]
    [Tooltip("Range of probing wave height for buoyancy rotation.")]
    public float size = 1f;

    [Tooltip("Max height of buoyancy going up and down.")]
    public float amplitude = 1f;

    [Space, Tooltip("Optionally provide a separate material to get the wave parameters.")]
    public Material overrideWaterMaterial;

    private Material _material;
    private float _speed;
    private float _amplitude;
    private float _frequency;
    private float _direction;

    private Vector3 _originalPosition;
    private Quaternion _originalRotation;

    private void Start() {
        var r = water.GetComponent<Renderer>();
        Debug.Assert(r);
        _material = overrideWaterMaterial != null ? overrideWaterMaterial : r.sharedMaterial;
        Debug.Assert(_material);
        Debug.Assert(_material.HasProperty("_WaveSpeed"));
        _speed = _material.GetFloat("_WaveSpeed");
        _amplitude = _material.GetFloat("_WaveAmplitude");
        _frequency = _material.GetFloat("_WaveFrequency");
        _direction = _material.GetFloat("_WaveDirection");

        var t = transform;
        _originalPosition = t.position;
        _originalRotation = t.rotation;
    }

    private void Update() {
        var positionWS = transform.position;
        var positionOS = water.InverseTransformPoint(positionWS);

        positionWS.y = GetHeightOS(positionOS) + _originalPosition.y;

        transform.position = positionWS;
        var normal = GetNormalWS(positionOS);
        transform.rotation = Quaternion.FromToRotation(Vector3.up, normal) * _originalRotation;
    }

    Vector2 GradientNoiseDir(Vector2 p) {
        p = new Vector2(p.x % 289, p.y % 289);
        float x = (34 * p.x + 1) * p.x % 289 + p.y;
        x = (34 * x + 1) * x % 289;
        x = ((x / 41) % 1) * 2 - 1;
        return (new Vector2(x - Mathf.Floor(x + 0.5f), Mathf.Abs(x) - 0.5f)).normalized;
    }

    float GradientNoise(Vector2 p) {
        Vector2 ip = new Vector2(Mathf.Floor(p.x), Mathf.Floor(p.y));
        Vector2 fp = new Vector2(p.x % 1, p.y % 1);
        float d00 = Vector3.Dot(GradientNoiseDir(ip), fp);
        float d01 = Vector3.Dot(GradientNoiseDir(ip + Vector2.up), fp - Vector2.up);
        float d10 = Vector3.Dot(GradientNoiseDir(ip + Vector2.right), fp - Vector2.right);
        float d11 = Vector3.Dot(GradientNoiseDir(ip + Vector2.one), fp - Vector2.one);
        fp = fp * fp * fp * (fp * (fp * 6f - Vector2.one * 15f) + Vector2.one * 10f);
        return Mathf.Lerp(Mathf.Lerp(d00, d01, fp.y), Mathf.Lerp(d10, d11, fp.y), fp.x);
    }

    private Vector3 GetNormalWS(Vector3 positionOS) {
        Vector3 b = positionOS + Vector3.forward * size;
        b.y = GetHeightOS(b);

        Vector3 c = positionOS + Vector3.right * size;
        c.y = GetHeightOS(b);

        Vector3 n = Vector3.Cross(b - positionOS, c - positionOS).normalized;
        return water.TransformDirection(n);
    }

    private float SineWave(Vector3 positionOS, float offset) {
        // Shader:
        // sin(offset + _Time.z * _WaveSpeed + (pos.x * sin(offset + _WaveDirection) + pos.z *
        //         cos(offset + _WaveDirection)) * _WaveFrequency);
        float timez = Time.timeSinceLevelLoad * 2f;
        float s = Mathf.Sin(offset + timez * _speed +
                            (positionOS.x * Mathf.Sin(offset + _direction) + positionOS.z *
                                Mathf.Cos(offset + _direction)) * _frequency);

        if (_material.IsKeywordEnabled("_WAVEMODE_POINTY")) {
            s = 1.0f - Mathf.Abs(s);
        }

        return s * _amplitude;
    }

    private float GetHeightOS(Vector3 positionOS) {
        float y = SineWave(positionOS, 0.0f);

        if (_material.IsKeywordEnabled("_WAVEMODE_GRID")) {
            y *= SineWave(positionOS, 1.57f);
        }

        y *= amplitude;

        return y;
    }
}
}