using prefabs;
using UnityEngine;
using UnityEngine.Serialization;

namespace Controller
{
    public class CamController : MonoBehaviour
    {
        private const float _rotationSpeed = 0.15f;
        [FormerlySerializedAs("TargetCamera")] public Camera targetCamera;
        [FormerlySerializedAs("PlaneObject")] public GameObject planeObject;
        public Vector3 cameraPivot = new(0, 0, 0);
        private float _cameraDistance;
        private float _currentPitch;

        private float _currentYaw;

        private float _delta;
        private GridView _gridView;

        private bool _isDragging;
        private Vector3 _lastMousePos;

        private void Update() => HandleCameraRotation();

        public void Build(GridView gridView)
        {
            _gridView = gridView;

            Vector3 dir = new Vector3(0, 0, 0) - targetCamera.transform.position;

            _cameraDistance = dir.magnitude;
            _currentYaw = targetCamera.transform.eulerAngles.y;
            _currentPitch = targetCamera.transform.eulerAngles.x;
        }

        private void HandleCameraRotation()
        {
            if (_gridView is null) return;

            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = targetCamera.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (hit.collider.gameObject == planeObject)
                    {
                        _isDragging = true;
                        _lastMousePos = Input.mousePosition;
                    }
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                _isDragging = false;
            }

            if (!_isDragging)
                return;

            _delta = (Input.mousePosition - _lastMousePos).x;
            _delta *= _rotationSpeed;
            _lastMousePos = Input.mousePosition;

            _currentYaw += _delta;

            RotateCamera();
        }

        private void RotateCamera()
        {
            var rotation = Quaternion.Euler(_currentPitch, _currentYaw, 0);

            Vector3 offset = rotation * new Vector3(0, 0, -_cameraDistance);
            targetCamera.transform.position = cameraPivot + offset;
            targetCamera.transform.LookAt(cameraPivot);

            _gridView.RotateMovePlatforms(_delta);
            _delta = 0;
        }
    }
}
