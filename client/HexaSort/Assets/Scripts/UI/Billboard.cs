using UnityEngine;

namespace HexaSort.UI
{
    public class Billboard : MonoBehaviour
    {
        private Camera _camera;

        private void Start()
        {
            _camera = Camera.main;
        }

        private void LateUpdate()
        {
            if (_camera != null)
            {
                transform.rotation = _camera.transform.rotation;
            }
        }
    }
}