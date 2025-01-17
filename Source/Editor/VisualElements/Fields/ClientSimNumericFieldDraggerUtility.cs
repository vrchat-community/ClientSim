using System;
using UnityEngine;

namespace VRC.SDK3.ClientSim.Editor.VisualElements.Fields
{
    public class ClientSimNumericFieldDraggerUtility
    {
        private static bool s_UseYSign;
        private const double kDragSensitivity = 0.029999999329447746;
        private const double kYSignThreshold = 0.10000000149011612;

        internal static float Acceleration(bool shiftPressed, bool altPressed) => (float) ((shiftPressed ? 4.0 : 1.0) * (altPressed ? 0.25 : 1.0));

        internal static float NiceDelta(Vector2 deviceDelta, float acceleration)
        {
            deviceDelta.y = -deviceDelta.y;
            if ((double) Mathf.Abs(Mathf.Abs(deviceDelta.x) - Mathf.Abs(deviceDelta.y)) / (double) Mathf.Max(Mathf.Abs(deviceDelta.x), Mathf.Abs(deviceDelta.y)) > kYSignThreshold)
                ClientSimNumericFieldDraggerUtility.s_UseYSign = (double) Mathf.Abs(deviceDelta.x) <= (double) Mathf.Abs(deviceDelta.y);
            return ClientSimNumericFieldDraggerUtility.s_UseYSign ? Mathf.Sign(deviceDelta.y) * deviceDelta.magnitude * acceleration : Mathf.Sign(deviceDelta.x) * deviceDelta.magnitude * acceleration;
        }

        internal static double CalculateFloatDragSensitivity(double value) => double.IsInfinity(value) || double.IsNaN(value) ? 0.0 : Math.Max(1.0, Math.Pow(Math.Abs(value), 0.5)) * kDragSensitivity;

        internal static double CalculateFloatDragSensitivity(
            double value,
            double minValue,
            double maxValue)
        {
            return double.IsInfinity(value) || double.IsNaN(value) ? 0.0 : Math.Abs(maxValue - minValue) / 100.0 * kDragSensitivity;
        }

        internal static long CalculateIntDragSensitivity(long value) => (long) ClientSimNumericFieldDraggerUtility.CalculateIntDragSensitivity((double) value);

        internal static ulong CalculateIntDragSensitivity(ulong value) => (ulong) ClientSimNumericFieldDraggerUtility.CalculateIntDragSensitivity((double) value);

        private static double CalculateIntDragSensitivity(double value) => Math.Max(1.0, Math.Pow(Math.Abs(value), 0.5) * kDragSensitivity);

        internal static long CalculateIntDragSensitivity(long value, long minValue, long maxValue) => Math.Max(1L, (long) (kDragSensitivity * (double) Math.Abs(maxValue - minValue) / 100.0));
    }
}