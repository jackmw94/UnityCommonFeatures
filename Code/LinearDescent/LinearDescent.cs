using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Very primitive gradient descent helper for finding inputs that minimise a loss function.
/// I've called it linear descent since it doesn't account for the slope of the gradient.
///
/// You can use the minimise function to find optimised inputs for functions that have lots of moving parts.
/// E.g. I've used this function to determine a direction and force for a squash ball to hit a target in the court
/// while accounting for bouncing off walls and the associated reduction in speed after a bounce.
/// Note, I wouldn't recommend using this in realtime for most cases
///
/// For a given input type, extend IDescendible - see the internal classes for default
/// implementations of the input functions for float, Vector2, Vector3 and Vector4.
/// Calling Minimise will return an input value that is at a local minimum for your loss function.
///
/// "Local minima" can be thought of by imagining a mountain with two peaks (the loss function being -height).
/// If the starting value is near the lower peak then the minimise function will reach that lower peak and consider
/// that as minimised since every direction leads downwards and thus to a greater value from the loss function.
/// This is an inherent limitation of this implementation.
/// </summary>
public static class LinearDescent
{
    public static T Minimise<T>(IDescendible<T> descendible, T startingValue, T minRange, T maxRange, T epsilon, int iterations)
    {
        T testInput = startingValue;
        List<T> directions = descendible.GetDescentDirections(epsilon);
        float currentMinima = descendible.LossFunction(testInput);
        T currentMinimaInput = testInput;

        for (int i = 0; i < iterations; i++)
        {
            // painful part: run the loss function for each direction to guess at direction of steepest descent
            T descentDirection = directions.OrderBy(p => descendible.LossFunction(descendible.AddInputs(testInput, p))).First();

            // update input and plug into loss function
            testInput = descendible.ClampInputs(descendible.AddInputs(testInput, descentDirection), minRange, maxRange);
            float nextValue = descendible.LossFunction(testInput);

            // if the updated value is higher than previous then we've already hit the local minima
            if (nextValue > currentMinima)
            {
                break;
            }

            // if it is lower then update minima and reiterate
            currentMinimaInput = testInput;
        }

        return currentMinimaInput;
    }

    public static class Vector4InputFunctions
    {
        public static List<Vector4> GetDescentDirections(Vector4 epsilon)
        {
            List<Vector4> vectors = new List<Vector4>();
            for (float x = -1f; x <= 1f; x += 1f)
            {
                for (float y = -1f; y <= 1f; y += 1f)
                {
                    for (float z = -1f; z <= 1f; z += 1f)
                    {
                        for (float w = -1f; w <= 1f; w += 1f)
                        {
                            Vector4 direction = new Vector4(
                                x * epsilon.x,
                                y * epsilon.y,
                                z * epsilon.z,
                                w * epsilon.w);

                            // no need to have (0,0,0,0) as a direction
                            if (direction.magnitude.Equals(0f))
                            {
                                continue;
                            }

                            vectors.Add(direction);
                        }
                    }
                }
            }

            return vectors;
        }

        public static Vector4 AddInputs(Vector4 v1, Vector4 v2) => v1 + v2;

        public static Vector4 ClampInputs(Vector4 value, Vector4 min, Vector4 max) =>
            new Vector4(
                Mathf.Clamp(value.x, min.x, max.x),
                Mathf.Clamp(value.y, min.y, max.y),
                Mathf.Clamp(value.z, min.z, max.z),
                Mathf.Clamp(value.w, min.w, max.w));

        public static Vector4 ClampSomeInputs(Vector4 value, Vector4 min, Vector4 max, Vector4 clampedInputsMask) =>
            new Vector4(
                clampedInputsMask.x.Equals(0f) ? value.x : Mathf.Clamp(value.x, min.x, max.x),
                clampedInputsMask.y.Equals(0f) ? value.y : Mathf.Clamp(value.y, min.y, max.y),
                clampedInputsMask.z.Equals(0f) ? value.z : Mathf.Clamp(value.z, min.z, max.z),
                clampedInputsMask.w.Equals(0f) ? value.w : Mathf.Clamp(value.w, min.w, max.w));
    }

    public static class Vector3InputFunctions
    {
        public static List<Vector3> GetDescentDirections(Vector3 epsilon)
        {
            List<Vector3> vectors = new List<Vector3>();
            for (float x = -1f; x <= 1f; x += 1f)
            {
                for (float y = -1f; y <= 1f; y += 1f)
                {
                    for (float z = -1f; z <= 1f; z += 1f)
                    {
                        Vector3 direction = new Vector3(
                            x * epsilon.x,
                            y * epsilon.y,
                            z * epsilon.z);

                        // no need to have (0,0,0) as a direction
                        if (direction.magnitude.Equals(0f))
                        {
                            continue;
                        }

                        vectors.Add(direction);
                    }
                }
            }

            return vectors;
        }

        public static Vector3 AddInputs(Vector3 v1, Vector3 v2) => v1 + v2;

        public static Vector3 ClampInputs(Vector3 value, Vector3 min, Vector3 max) =>
            new Vector3(
                Mathf.Clamp(value.x, min.x, max.x),
                Mathf.Clamp(value.y, min.y, max.y),
                Mathf.Clamp(value.z, min.z, max.z));

        public static Vector3 ClampSomeInputs(Vector3 value, Vector3 min, Vector3 max, Vector3 clampedInputsMask) =>
            new Vector3(
                clampedInputsMask.x.Equals(0f) ? value.x : Mathf.Clamp(value.x, min.x, max.x),
                clampedInputsMask.y.Equals(0f) ? value.y : Mathf.Clamp(value.y, min.y, max.y),
                clampedInputsMask.z.Equals(0f) ? value.z : Mathf.Clamp(value.z, min.z, max.z));
    }

    public static class Vector2InputFunctions
    {
        public static List<Vector2> GetDescentDirections(Vector2 epsilon)
        {
            List<Vector2> vectors = new List<Vector2>();
            for (float x = -1f; x <= 1f; x += 1f)
            {
                for (float y = -1f; y <= 1f; y += 1f)
                {
                    Vector2 direction = new Vector2(
                        x * epsilon.x,
                        y * epsilon.y);

                    // no need to have (0,0) as a direction
                    if (direction.magnitude.Equals(0f))
                    {
                        continue;
                    }

                    vectors.Add(direction);
                }
            }

            return vectors;
        }

        public static Vector2 AddInputs(Vector2 v1, Vector2 v2) => v1 + v2;

        public static Vector2 ClampInputs(Vector2 value, Vector2 min, Vector2 max) =>
            new Vector2(
                Mathf.Clamp(value.x, min.x, max.x),
                Mathf.Clamp(value.y, min.y, max.y));

        public static Vector3 ClampSomeInputs(Vector2 value, Vector2 min, Vector2 max, Vector2 clampedInputsMask) =>
            new Vector3(
                clampedInputsMask.x.Equals(0f) ? value.x : Mathf.Clamp(value.x, min.x, max.x),
                clampedInputsMask.y.Equals(0f) ? value.y : Mathf.Clamp(value.y, min.y, max.y));
    }

    public static class FloatInputFunctions
    {
        // these functions are trivial to the point of being useless but hopefully
        // they elucidate the purpose of these functions for different types

        public static List<float> GetDescentDirections(float epsilon)
        {
            return new List<float>
            {
                -epsilon, epsilon
            };
        }

        public static float AddInputs(float v1, float v2) => v1 + v2;

        public static float ClampInputs(float value, float min, float max) => Mathf.Clamp(value, min, max);
    }
}
