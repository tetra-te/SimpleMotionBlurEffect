using System.Reflection;

namespace SimpleMotionBlurEffect
{
    internal class ShaderResourceLoader
    {
        public static byte[] GetShaderResource(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"SimpleMotionBlurEffect.{name}";
            using var stream = assembly.GetManifestResourceStream(resourceName) ?? throw new Exception($"Resource {resourceName} not found.");
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            return bytes;
        }
    }
}
