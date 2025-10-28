using Xunit;
using Armonia.App.Services;

namespace Armonia.Tests
{
    public class AudioCaptureTests
    {
        [Fact]
        public void Service_Dispose_DoesNotThrow()
        {
            using var service = new AudioCaptureService();
            service.Dispose();
        }
    }
}
