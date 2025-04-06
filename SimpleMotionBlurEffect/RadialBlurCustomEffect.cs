using System.Numerics;
using System.Runtime.InteropServices;
using Vortice;
using Vortice.Direct2D1;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;

namespace SimpleMotionBlurEffect
{
    internal class RadialBlurCustomEffect : D2D1CustomShaderEffectBase
    {
        public float Blur
        {
            set => SetValue((int)EffectImpl.Properties.Blur, value);
            get => GetFloatValue((int)EffectImpl.Properties.Blur);
        }

        public RadialBlurCustomEffect(IGraphicsDevicesAndContext devices) : base(Create<EffectImpl>(devices))
        {
        }

        [CustomEffect(1)]
        class EffectImpl : D2D1CustomShaderEffectImplBase<EffectImpl>
        {
            ConstantBuffer constantBuffer;

            [CustomEffectProperty(PropertyType.Float, (int)Properties.Blur)]
            public float Blur
            {
                get => constantBuffer.Blur;
                set
                {
                    constantBuffer.Blur = value;
                    UpdateConstants();
                }
            }

            public EffectImpl() : base(ShaderResourceLoader.GetShaderResource("PixelShader.cso"))
            {
            }

            protected override void UpdateConstants()
            {
                drawInformation?.SetPixelShaderConstantBuffer(constantBuffer);
            }

            public override void MapInputRectsToOutputRect(RawRect[] inputRects, RawRect[] inputOpaqueSubRects, out RawRect outputRect, out RawRect outputOpaqueSubRect)
            {
                RawRect rect = inputRects[0];
                Vector2[] points =
                {
                    GetOutputPoint(new Vector2(rect.Left, rect.Top)),
                    GetOutputPoint(new Vector2(rect.Right, rect.Top)),
                    GetOutputPoint(new Vector2(rect.Left, rect.Bottom)),
                    GetOutputPoint(new Vector2(rect.Right, rect.Bottom)),
                    new Vector2(rect.Left, rect.Top),
                    new Vector2(rect.Right, rect.Top),
                    new Vector2(rect.Left, rect.Bottom),
                    new Vector2(rect.Right, rect.Bottom)
                };
                outputRect = new RawRect((int)points.Select((Vector2 v) => v.X).Min(), (int)points.Select((Vector2 v) => v.Y).Min(), (int)points.Select((Vector2 v) => v.X).Max(), (int)points.Select((Vector2 v) => v.Y).Max());
                outputOpaqueSubRect = default;
            }

            Vector2 GetOutputPoint(Vector2 input)
            {
                return input * 100f / (100f - Blur);
            }

            RawRect GetInputRect(Vector2 output)
            {
                Vector2 dv = ClampVector(output * Blur / 100f, 4000);
                Vector2 from = output - dv;
                Vector2 to = output;
                return new RawRect((int)(Math.Min(from.X, to.X) - 1f), (int)(Math.Min(from.Y, to.Y) - 1f), (int)(Math.Max(from.X, to.X) + 1f), (int)(Math.Max(from.Y, to.Y) + 1f));
            }

            Vector2 ClampVector(Vector2 v, int length)
            {
                float l = v.Length();
                if (!(l > length))
                {
                    return v;
                }
                return v * length / l;
            }

            public override void MapOutputRectToInputRects(RawRect outputRect, RawRect[] inputRects)
            {
                RawRect[] rects =
                {
                    GetInputRect(new Vector2(outputRect.Left, outputRect.Top)),
                    GetInputRect(new Vector2(outputRect.Right, outputRect.Top)),
                    GetInputRect(new Vector2(outputRect.Left, outputRect.Bottom)),
                    GetInputRect(new Vector2(outputRect.Right, outputRect.Bottom))
                };
                inputRects[0] = new RawRect(rects.Select((RawRect r) => r.Left).Min(), rects.Select((RawRect r) => r.Top).Min(), rects.Select((RawRect r) => r.Right).Max(), rects.Select((RawRect r) => r.Bottom).Max());
            }

            [StructLayout(LayoutKind.Sequential)]
            struct ConstantBuffer
            {
                public float Blur;
            }
            public enum Properties
            {
                Blur = 0
            }
        }
    }
}
