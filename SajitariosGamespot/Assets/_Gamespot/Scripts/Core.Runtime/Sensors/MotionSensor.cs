using UnityEngine;
using UnityEngine.InputSystem;

namespace Gamespot.Core.Runtime.Sensors
{
    /// <summary>
    /// Acelerómetro del móvil con el Input System (V075, ¿Quién soy?): la gravedad en g, en los ejes del aparato (x a la
    /// derecha y y hacia arriba en vertical, z saliendo de la pantalla). En el Editor o sin sensor no hay lectura; los
    /// tests fijan una con <see cref="Override"/>.
    /// </summary>
    public static class MotionSensor
    {
        private static bool _enabled;

        /// <summary>Lectura fija para los tests (nula = la del sensor).</summary>
        public static Vector3? Override { get; set; }

        /// <summary>Si hay acelerómetro (o una lectura fija).</summary>
        public static bool Available => Override.HasValue || Accelerometer.current != null;

        public static bool TryRead(out Vector3 gravity)
        {
            if (Override.HasValue)
            {
                gravity = Override.Value;
                return true;
            }
            var sensor = Accelerometer.current;
            if (sensor == null)
            {
                gravity = Vector3.zero;
                return false;
            }
            if (!_enabled || !sensor.enabled)
            {
                InputSystem.EnableDevice(sensor);
                _enabled = true;
            }
            gravity = sensor.acceleration.ReadValue();
            return true;
        }

        /// <summary>Apaga el sensor al salir del juego (gasta batería).</summary>
        public static void Release()
        {
            var sensor = Accelerometer.current;
            if (sensor != null && sensor.enabled) InputSystem.DisableDevice(sensor);
            _enabled = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Override = null;
            _enabled = false;
        }
    }
}
