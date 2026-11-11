using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace FlappyComet.Core
{
    public class GameInput : IInputService
    {
        public bool StartPressed
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) return true;
                if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) return true;
                if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame) return true;
                return false;
#else
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) return true;
                for (int i = 0; i < Input.touchCount; i++)
                    if (Input.GetTouch(i).phase == TouchPhase.Began) return true;
                return false;
#endif
            }
        }

        public bool ThrustHeld
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                if (Keyboard.current != null && Keyboard.current.spaceKey.isPressed) return true;
                if (Mouse.current != null && Mouse.current.leftButton.isPressed) return true;
                if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed) return true;
                return false;
#else
                return Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0) || Input.touchCount > 0;
#endif
            }
        }
    }
}
