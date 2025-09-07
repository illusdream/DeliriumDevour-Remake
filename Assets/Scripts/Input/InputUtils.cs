using UnityEngine.InputSystem;

public static class InputUtils
{
        public static void SaveBinding(InputAction inputAction)
        {
                InputManager.Instance.SaveBinding(inputAction);
        }
        public static PlayerInput GetCurrentInputAction()
        {
                return InputManager.Instance.GetCurrentInputAction();
        }
}