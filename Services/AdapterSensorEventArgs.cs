using System;

namespace Wrench.Services
{
    public class AdapterSensorEventArgs : EventArgs
    {
        public AdapterSensorEventArgs(bool sensorState) => SensorState = sensorState;

        public bool SensorState { get; private set; }
    }
}