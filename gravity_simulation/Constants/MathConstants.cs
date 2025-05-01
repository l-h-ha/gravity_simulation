using Microsoft.Xna.Framework.Content;

namespace gravity_simulation.Constants
{
    internal class MathConstants
    {
        public const double G = 6.67430e-1; // (6.67430e-11) Gravitational constant in m^3 kg^-1 s^-2
        public const double EPSILON = 0.1; // Softening constant
        public const double EPSILON_SQUARED = EPSILON * EPSILON;
    }
}
