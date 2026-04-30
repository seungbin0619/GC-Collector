using UnityEngine;
using UnityEngine.InputSystem;

namespace Gcc.Feature.Game {
    public class Player : MonoBehaviour {
        private Vector2 _direction;

        [SerializeField]
        private float speed = 5f;

        public void OnMove(InputAction.CallbackContext context) {
            _direction = context.ReadValue<Vector2>();
        }
        
        private void Update() {
            transform.Translate(speed * Time.deltaTime * _direction);
        }
    }
}