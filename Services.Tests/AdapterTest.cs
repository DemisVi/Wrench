using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Tests
{
    public class AdapterTest
    {
        public Adapter adapter = new("USBCOM17A");
        ConcurrentQueue<bool> result = new ConcurrentQueue<bool>();

        public AdapterTest()
        {
            adapter.SensorStateChanged += StoreResult;

        }

        private void StoreResult(object sender, AdapterSensorEventArgs e) => result.Enqueue(e.SensorState);

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

            Assert.Same(result, new ConcurrentQueue<bool>(new[] { true, false, true, false, true, false }));
        }
    }
}
