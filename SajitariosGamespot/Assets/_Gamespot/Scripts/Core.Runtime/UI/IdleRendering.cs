using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace Gamespot.Core.Runtime.UI
{
    /// <summary>
    /// Reposo (V063): tras <see cref="IdleSeconds"/> sin tocar la pantalla se dibuja un fotograma de cada
    /// <see cref="IdleInterval"/> (30 de 60) con <see cref="OnDemandRendering"/>; al tocar, o al cambiar de pantalla
    /// (<see cref="Wake"/>), vuelve a todos. Solo se salta el dibujo: la lógica (temporizadores, la mecha, los
    /// sonidos) sigue corriendo a 60. En una mesa con el móvil quieto en Yo Nunca o en el menú gasta la mitad de GPU.
    /// </summary>
    public sealed class IdleRendering : MonoBehaviour
    {
        public const float IdleSeconds = 3f;
        public const int IdleInterval = 2;

        private float _idle;

        /// <summary>Cada cuántos fotogramas se dibuja tras <paramref name="idleSeconds"/> sin tocar.</summary>
        public static int IntervalFor(float idleSeconds) => idleSeconds >= IdleSeconds ? IdleInterval : 1;

        public static IdleRendering Create(GameObject host) => host.AddComponent<IdleRendering>();

        /// <summary>Vuelve a dibujar todos los fotogramas y reinicia la espera.</summary>
        public void Wake()
        {
            _idle = 0f;
            Apply();
        }

        private void Update()
        {
            _idle = HasInput() ? 0f : _idle + Time.unscaledDeltaTime;
            Apply();
        }

        private void OnDisable() => OnDemandRendering.renderFrameInterval = 1;

        private void Apply()
        {
            var interval = IntervalFor(_idle);
            if (OnDemandRendering.renderFrameInterval != interval) OnDemandRendering.renderFrameInterval = interval;
        }

        private static bool HasInput()
        {
            var pointer = Pointer.current;
            if (pointer != null && pointer.press.isPressed) return true;
            var mouse = Mouse.current;
            if (mouse != null && mouse.scroll.ReadValue() != Vector2.zero) return true;
            var keyboard = Keyboard.current;
            return keyboard != null && keyboard.anyKey.isPressed;
        }
    }
}
