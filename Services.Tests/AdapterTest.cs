using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wrench.Services.Tests
{
    public class AdapterTest
    {
        public Adapter adapter = new("USBCOM17A");
        List<bool> result = new List<bool>();

        public AdapterTest()
        {
            adapter.SensorStateChanged += StoreResult;

        }

        private void StoreResult(object sender, AdapterSensorEventArgs e) => result.Add(e.SensorState);

        [Fact]
        public void ShouldThrow()
        {
            Assert.Throws(new InvalidOperationException().GetType(), adapter.BeginWatchSensorState);
        }

        [Fact]
        public async void ShouldReportSensorState()
        {
            adapter.OpenAdapter();
            adapter.BeginWatchSensorState();

            while (result.Count < 6) { await Task.Delay(10); }

            Assert.Equal(result, new List<bool> { true, false, true, false, true, false });
        }
    }
}
