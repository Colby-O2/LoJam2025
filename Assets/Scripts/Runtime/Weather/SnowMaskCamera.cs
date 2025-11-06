using UnityEngine;

namespace LJ2025
{
    public class SnowMaskCamera : MonoBehaviour
    {
        private static readonly int SnowMaskPositionId = Shader.PropertyToID("_SnowMaskPosition");
        private static readonly int SnowMaskScaleId = Shader.PropertyToID("_SnowMaskScale");

        [SerializeField] private Material _groundSnowMaterial;

        private void Start()
        {
            _groundSnowMaterial.SetFloat(SnowMaskScaleId, GetComponent<Camera>().orthographicSize * 2);
        }

        private void Update()
        {
            _groundSnowMaterial.SetVector(SnowMaskPositionId, transform.position);
        }
    }
}
